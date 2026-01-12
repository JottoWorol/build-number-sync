import express from 'express';
import { Firestore, FieldValue } from '@google-cloud/firestore';

const app = express();
const firestore = new Firestore();
const COLLECTION_NAME = 'buildNumbers';

// Enable CORS for all routes
app.use((req, res, next) => {
    res.set('Access-Control-Allow-Origin', '*');
    res.set('Access-Control-Allow-Methods', 'GET, POST, DELETE, OPTIONS');
    res.set('Access-Control-Allow-Headers', 'Content-Type');
    
    if (req.method === 'OPTIONS') {
        res.status(204).send('');
        return;
    }
    next();
});

// JSON response helper
app.use(express.json());

/**
 * GET /getNextBuildNumber
 * Query params: bundleId (required), platform (optional, default: 'default')
 * Returns: { success: true, buildNumber: number }
 */
app.get('/getNextBuildNumber', async (req, res) => {
    const startTime = Date.now();
    const bundleId = req.query.bundleId;
    const platform = req.query.platform || 'default';

    console.log(`[getNextBuildNumber] Request - bundleId: ${bundleId}, platform: ${platform}`);

    // Validate required parameter
    if (!bundleId) {
        console.warn('[getNextBuildNumber] Missing bundleId parameter');
        return res.status(400).json({
            success: false,
            error: 'Missing required parameter: bundleId'
        });
    }

    try {
        // Create document key
        const docId = `${bundleId}_${platform}`;
        const docRef = firestore.collection(COLLECTION_NAME).doc(docId);

        // Use Firestore transaction to ensure atomic increment
        const nextBuildNumber = await firestore.runTransaction(async (transaction) => {
            const doc = await transaction.get(docRef);
            
            let newBuildNumber;
            if (!doc.exists) {
                // First time - start with 1
                newBuildNumber = 1;
                console.log(`[getNextBuildNumber] Creating new entry for ${docId} with buildNumber: 1`);
                transaction.set(docRef, {
                    buildNumber: newBuildNumber,
                    bundleId: bundleId,
                    platform: platform,
                    lastUpdated: FieldValue.serverTimestamp()
                });
            } else {
                // Increment existing build number
                const currentData = doc.data();
                newBuildNumber = (currentData.buildNumber || 0) + 1;
                console.log(`[getNextBuildNumber] Incrementing ${docId} from ${currentData.buildNumber} to ${newBuildNumber}`);
                transaction.update(docRef, {
                    buildNumber: newBuildNumber,
                    lastUpdated: FieldValue.serverTimestamp()
                });
            }
            
            return newBuildNumber;
        });

        const duration = Date.now() - startTime;
        console.log(`[getNextBuildNumber] Success - buildNumber: ${nextBuildNumber}, duration: ${duration}ms`);

        return res.status(200).json({
            success: true,
            buildNumber: nextBuildNumber
        });
    } catch (error) {
        const duration = Date.now() - startTime;
        console.error(`[getNextBuildNumber] Error after ${duration}ms:`, error);
        return res.status(500).json({
            success: false,
            error: 'Internal Server Error',
            details: error.message
        });
    }
});

/**
 * DELETE /deleteBundleId
 * Query params: bundleId (required), platform (optional, default: 'default')
 * Returns: { success: true, message: string }
 */
app.delete('/deleteBundleId', async (req, res) => {
    const startTime = Date.now();
    const bundleId = req.query.bundleId;
    const platform = req.query.platform || 'default';

    console.log(`[deleteBundleId] Request - bundleId: ${bundleId}, platform: ${platform}`);

    // Validate required parameter
    if (!bundleId) {
        console.warn('[deleteBundleId] Missing bundleId parameter');
        return res.status(400).json({
            success: false,
            error: 'Missing required parameter: bundleId'
        });
    }

    try {
        // Create document key
        const docId = `${bundleId}_${platform}`;
        const docRef = firestore.collection(COLLECTION_NAME).doc(docId);

        // Check if document exists
        const doc = await docRef.get();

        if (!doc.exists) {
            const duration = Date.now() - startTime;
            console.warn(`[deleteBundleId] Bundle ID '${bundleId}' not found, duration: ${duration}ms`);
            return res.status(404).json({
                success: false,
                message: `Bundle ID '${bundleId}' not found`
            });
        }

        // Delete the document
        await docRef.delete();

        const duration = Date.now() - startTime;
        console.log(`[deleteBundleId] Success - deleted ${docId}, duration: ${duration}ms`);

        return res.status(200).json({
            success: true,
            message: `Bundle ID '${bundleId}' deleted successfully`
        });
    } catch (error) {
        const duration = Date.now() - startTime;
        console.error(`[deleteBundleId] Error after ${duration}ms:`, error);
        return res.status(500).json({
            success: false,
            error: 'Internal Server Error',
            details: error.message
        });
    }
});

/**
 * POST /setBuildNumber
 * Query params: bundleId (required), buildNumber (required), platform (optional, default: 'default')
 * Returns: { success: true, buildNumber: number }
 */
app.post('/setBuildNumber', async (req, res) => {
    const startTime = Date.now();
    const bundleId = req.query.bundleId;
    const platform = req.query.platform || 'default';
    const buildNumberParam = req.query.buildNumber;

    console.log(`[setBuildNumber] Request - bundleId: ${bundleId}, platform: ${platform}, buildNumber: ${buildNumberParam}`);

    // Validate required parameters
    if (!bundleId) {
        console.warn('[setBuildNumber] Missing bundleId parameter');
        return res.status(400).json({
            success: false,
            error: 'Missing required parameter: bundleId'
        });
    }

    if (!buildNumberParam) {
        console.warn('[setBuildNumber] Missing buildNumber parameter');
        return res.status(400).json({
            success: false,
            error: 'Missing required parameter: buildNumber'
        });
    }

    // Validate buildNumber is a valid integer
    const buildNumber = parseInt(buildNumberParam, 10);
    if (isNaN(buildNumber) || !Number.isInteger(buildNumber)) {
        console.warn(`[setBuildNumber] Invalid buildNumber: ${buildNumberParam}`);
        return res.status(400).json({
            success: false,
            error: 'buildNumber must be an integer'
        });
    }

    if (buildNumber < 0) {
        console.warn(`[setBuildNumber] Negative buildNumber: ${buildNumber}`);
        return res.status(400).json({
            success: false,
            error: 'buildNumber must be a non-negative integer'
        });
    }

    try {
        // Create document key
        const docId = `${bundleId}_${platform}`;
        const docRef = firestore.collection(COLLECTION_NAME).doc(docId);

        // Set the build number (overwrite if exists)
        await docRef.set({
            buildNumber: buildNumber,
            bundleId: bundleId,
            platform: platform,
            lastUpdated: FieldValue.serverTimestamp()
        });

        const duration = Date.now() - startTime;
        console.log(`[setBuildNumber] Success - set ${docId} to ${buildNumber}, duration: ${duration}ms`);

        return res.status(200).json({
            success: true,
            buildNumber: buildNumber
        });
    } catch (error) {
        const duration = Date.now() - startTime;
        console.error(`[setBuildNumber] Error after ${duration}ms:`, error);
        return res.status(500).json({
            success: false,
            error: 'Internal Server Error',
            details: error.message
        });
    }
});

/**
 * Root endpoint - API info
 */
app.get('/', (req, res) => {
    console.log('[root] API info requested');
    res.json({
        name: 'Build Number Sync API',
        version: '1.0.0',
        endpoints: {
            getNextBuildNumber: 'GET /getNextBuildNumber?bundleId={bundleId}&platform={platform}',
            deleteBundleId: 'DELETE /deleteBundleId?bundleId={bundleId}&platform={platform}',
            setBuildNumber: 'POST /setBuildNumber?bundleId={bundleId}&buildNumber={buildNumber}&platform={platform}'
        }
    });
});

/**
 * 404 handler
 */
app.use((req, res) => {
    console.warn(`[404] Not found: ${req.method} ${req.path}`);
    res.status(404).json({
        success: false,
        error: 'Not Found'
    });
});

/**
 * Error handler
 */
app.use((err, req, res, next) => {
    console.error('[Error]', err);
    res.status(500).json({
        success: false,
        error: 'Internal Server Error',
        details: err.message
    });
});

const port = parseInt(process.env.PORT) || 8080;
app.listen(port, () => {
    console.log(`Build Number Sync API listening on port ${port}`);
    console.log(`Environment: ${process.env.NODE_ENV || 'development'}`);
    console.log(`Firestore project: ${process.env.GOOGLE_CLOUD_PROJECT || 'default'}`);
});
