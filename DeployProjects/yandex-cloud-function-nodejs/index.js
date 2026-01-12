import { Driver } from '@ydbjs/core';
import { MetadataCredentialsProvider } from '@ydbjs/auth/metadata';
import { query, identifier } from '@ydbjs/query';
import { Int64 } from '@ydbjs/value/primitive';

// Initialize YDB driver with metadata auth (automatic in Yandex Cloud)
let driver = null;
let sql = null;

const TABLE_NAME = 'buildNumbers';

/**
 * Initialize YDB connection
 */
async function initYDB() {
    if (driver && sql) {
        return sql;
    }

    const connectionString = process.env.YDB_CONNECTION_STRING;

    if (!connectionString) {
        throw new Error('YDB_CONNECTION_STRING environment variable is required');
    }

    console.log(`Initializing YDB connection: ${connectionString}`);

    driver = new Driver(connectionString, {
        credentialsProvider: new MetadataCredentialsProvider(),
    });

    // Wait for driver to be ready (with 10 second timeout)
    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), 10000);
    try {
        await driver.ready(controller.signal);
        clearTimeout(timeoutId);
    } catch (error) {
        clearTimeout(timeoutId);
        throw error;
    }

    sql = query(driver);

    // Ensure table exists
    await ensureTableExists();

    return sql;
}

/**
 * Create table if it doesn't exist
 */
async function ensureTableExists() {
    try {
        await sql`
            CREATE TABLE IF NOT EXISTS ${identifier(TABLE_NAME)} (
                id Utf8,
                bundleId Utf8,
                platform Utf8,
                buildNumber Int64,
                lastUpdated Timestamp,
                PRIMARY KEY (id)
            )
        `;
        console.log(`Table ${TABLE_NAME} ready`);
    } catch (error) {
        // Table might already exist, continue
        if (!error.message?.includes('already exists')) {
            console.error('Error creating table:', error);
        }
    }
}

/**
 * Get next build number (atomic increment)
 */
async function getNextBuildNumber(bundleId, platform = 'default') {
    const id = `${bundleId}_${platform}`;

    const [[result]] = await sql.begin(async (tx) => {
        // Get current build number or 0 if doesn't exist
        let existing = null;
        try {
            const [[row]] = await tx`
                SELECT buildNumber FROM ${identifier(TABLE_NAME)} WHERE id = ${id}
            `;
            existing = row;
        } catch (error) {
            // Row doesn't exist, will start at 1
            existing = null;
        }

        const newBuildNumber = existing?.buildNumber ? Number(existing.buildNumber) + 1 : 1;

        // Upsert the new build number
        await tx`
            UPSERT INTO ${identifier(TABLE_NAME)} (id, bundleId, platform, buildNumber, lastUpdated)
            VALUES (${id}, ${bundleId}, ${platform}, ${new Int64(newBuildNumber)}, CurrentUtcTimestamp())
        `;

        return [[{ buildNumber: newBuildNumber }]];
    });

    return result.buildNumber;
}

/**
 * Set build number to specific value
 */
async function setBuildNumber(bundleId, buildNumber, platform = 'default') {
    const id = `${bundleId}_${platform}`;

    await sql`
        UPSERT INTO ${identifier(TABLE_NAME)} (id, bundleId, platform, buildNumber, lastUpdated)
        VALUES (${id}, ${bundleId}, ${platform}, ${new Int64(buildNumber)}, CurrentUtcTimestamp())
    `;

    return buildNumber;
}

/**
 * Delete bundle ID entry
 */
async function deleteBundleId(bundleId, platform = 'default') {
    const id = `${bundleId}_${platform}`;

    await sql`
        DELETE FROM ${identifier(TABLE_NAME)} WHERE id = ${id}
    `;
}

/**
 * Check if bundle ID exists
 */
async function bundleExists(bundleId, platform = 'default') {
    const id = `${bundleId}_${platform}`;

    const [[result]] = await sql`
        SELECT COUNT(*) AS count FROM ${identifier(TABLE_NAME)} WHERE id = ${id}
    `;

    return Number(result.count) > 0;
}

/**
 * Main handler for Yandex Cloud Function
 */
export const handler = async (event, context) => {
    const startTime = Date.now();

    // Initialize YDB connection
    try {
        await initYDB();
    } catch (error) {
        console.error('Failed to initialize YDB:', error);
        return {
            statusCode: 500,
            headers: {
                'Content-Type': 'application/json',
                'Access-Control-Allow-Origin': '*',
                'Access-Control-Allow-Methods': 'GET, POST, DELETE, OPTIONS',
                'Access-Control-Allow-Headers': 'Content-Type',
            },
            body: JSON.stringify({
                success: false,
                error: 'Database initialization failed',
                details: error.message,
            }),
        };
    }

    // Parse request
    const httpMethod = event.httpMethod || event.requestContext?.http?.method || 'GET';
    const path = event.path || event.requestContext?.http?.path || '/';
    const queryParams = event.queryStringParameters || {};

    console.log(`[${httpMethod}] ${path}`, queryParams);

    // CORS preflight
    if (httpMethod === 'OPTIONS') {
        return {
            statusCode: 204,
            headers: {
                'Access-Control-Allow-Origin': '*',
                'Access-Control-Allow-Methods': 'GET, POST, DELETE, OPTIONS',
                'Access-Control-Allow-Headers': 'Content-Type',
            },
            body: '',
        };
    }

    const headers = {
        'Content-Type': 'application/json',
        'Access-Control-Allow-Origin': '*',
        'Access-Control-Allow-Methods': 'GET, POST, DELETE, OPTIONS',
        'Access-Control-Allow-Headers': 'Content-Type',
    };

    try {
        // Route: GET /getNextBuildNumber
        if (httpMethod === 'GET' && path === '/getNextBuildNumber') {
            const bundleId = queryParams.bundleId;
            const platform = queryParams.platform || 'default';

            if (!bundleId) {
                console.warn('[getNextBuildNumber] Missing bundleId parameter');
                return {
                    statusCode: 400,
                    headers,
                    body: JSON.stringify({
                        success: false,
                        error: 'Missing required parameter: bundleId',
                    }),
                };
            }

            console.log(`[getNextBuildNumber] bundleId: ${bundleId}, platform: ${platform}`);

            const buildNumber = await getNextBuildNumber(bundleId, platform);

            const duration = Date.now() - startTime;
            console.log(`[getNextBuildNumber] Success - buildNumber: ${buildNumber}, duration: ${duration}ms`);

            return {
                statusCode: 200,
                headers,
                body: JSON.stringify({
                    success: true,
                    buildNumber: Number(buildNumber),
                }),
            };
        }

        // Route: POST /setBuildNumber
        if (httpMethod === 'POST' && path === '/setBuildNumber') {
            const bundleId = queryParams.bundleId;
            const platform = queryParams.platform || 'default';
            const buildNumberParam = queryParams.buildNumber;

            if (!bundleId) {
                console.warn('[setBuildNumber] Missing bundleId parameter');
                return {
                    statusCode: 400,
                    headers,
                    body: JSON.stringify({
                        success: false,
                        error: 'Missing required parameter: bundleId',
                    }),
                };
            }

            if (!buildNumberParam) {
                console.warn('[setBuildNumber] Missing buildNumber parameter');
                return {
                    statusCode: 400,
                    headers,
                    body: JSON.stringify({
                        success: false,
                        error: 'Missing required parameter: buildNumber',
                    }),
                };
            }

            const buildNumber = parseInt(buildNumberParam, 10);
            if (isNaN(buildNumber) || !Number.isInteger(buildNumber)) {
                console.warn(`[setBuildNumber] Invalid buildNumber: ${buildNumberParam}`);
                return {
                    statusCode: 400,
                    headers,
                    body: JSON.stringify({
                        success: false,
                        error: 'buildNumber must be an integer',
                    }),
                };
            }

            if (buildNumber < 0) {
                console.warn(`[setBuildNumber] Negative buildNumber: ${buildNumber}`);
                return {
                    statusCode: 400,
                    headers,
                    body: JSON.stringify({
                        success: false,
                        error: 'buildNumber must be a non-negative integer',
                    }),
                };
            }

            console.log(`[setBuildNumber] bundleId: ${bundleId}, platform: ${platform}, buildNumber: ${buildNumber}`);

            await setBuildNumber(bundleId, buildNumber, platform);

            const duration = Date.now() - startTime;
            console.log(`[setBuildNumber] Success - duration: ${duration}ms`);

            return {
                statusCode: 200,
                headers,
                body: JSON.stringify({
                    success: true,
                    buildNumber: buildNumber,
                }),
            };
        }

        // Route: DELETE /deleteBundleId
        if (httpMethod === 'DELETE' && path === '/deleteBundleId') {
            const bundleId = queryParams.bundleId;
            const platform = queryParams.platform || 'default';

            if (!bundleId) {
                console.warn('[deleteBundleId] Missing bundleId parameter');
                return {
                    statusCode: 400,
                    headers,
                    body: JSON.stringify({
                        success: false,
                        error: 'Missing required parameter: bundleId',
                    }),
                };
            }

            console.log(`[deleteBundleId] bundleId: ${bundleId}, platform: ${platform}`);

            const exists = await bundleExists(bundleId, platform);

            if (!exists) {
                const duration = Date.now() - startTime;
                console.warn(`[deleteBundleId] Bundle ID '${bundleId}' not found, duration: ${duration}ms`);
                return {
                    statusCode: 404,
                    headers,
                    body: JSON.stringify({
                        success: false,
                        message: `Bundle ID '${bundleId}' not found`,
                    }),
                };
            }

            await deleteBundleId(bundleId, platform);

            const duration = Date.now() - startTime;
            console.log(`[deleteBundleId] Success - duration: ${duration}ms`);

            return {
                statusCode: 200,
                headers,
                body: JSON.stringify({
                    success: true,
                    message: `Bundle ID '${bundleId}' deleted successfully`,
                }),
            };
        }

        // Route: Root endpoint
        if (httpMethod === 'GET' && path === '/') {
            console.log('[root] API info requested');
            return {
                statusCode: 200,
                headers,
                body: JSON.stringify({
                    name: 'Build Number Sync API',
                    version: '1.0.0',
                    endpoints: {
                        getNextBuildNumber: 'GET /getNextBuildNumber?bundleId={bundleId}&platform={platform}',
                        deleteBundleId: 'DELETE /deleteBundleId?bundleId={bundleId}&platform={platform}',
                        setBuildNumber: 'POST /setBuildNumber?bundleId={bundleId}&buildNumber={buildNumber}&platform={platform}',
                    },
                }),
            };
        }

        // 404 - Not Found
        console.warn(`[404] Not found: ${httpMethod} ${path}`);
        return {
            statusCode: 404,
            headers,
            body: JSON.stringify({
                success: false,
                error: 'Not Found',
            }),
        };

    } catch (error) {
        const duration = Date.now() - startTime;
        console.error(`[Error] after ${duration}ms:`, error);
        return {
            statusCode: 500,
            headers,
            body: JSON.stringify({
                success: false,
                error: 'Internal Server Error',
                details: error.message,
            }),
        };
    }
};

