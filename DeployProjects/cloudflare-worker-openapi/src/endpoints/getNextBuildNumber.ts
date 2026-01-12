import { Bool, Int, Str, OpenAPIRoute } from "chanfana";
import { z } from "zod";
import { type AppContext } from "../types";

export class GetNextBuildNumber extends OpenAPIRoute {
	schema = {
		tags: ["BuildNumber"],
		summary: "Get next build number & increment",
		request: {
			query: z.object({
				bundleId: Str({
					description: "Bundle ID",
					required: true,
				}),
				platform: Str({
					description: "Platform name, e.g. iOS/Android/MacOS",
					default: "default",
					required: false,
				}),
			}),
		},
		responses: {
			"200": {
				description: "Returns the next available build number",
				content: {
					"application/json": {
						schema: z.object({
							success: Bool(),
							buildNumber: Int(),
						}),
					},
				},
			},
		},
	};

	async handle(c: AppContext) {
		// Get validated data
		const data = await this.getValidatedData<typeof this.schema>();

		// Retrieve the validated parameters
		const { bundleId, platform } = data.query;

		// Create a unique key for this bundle ID
		const kvKey = `build_number_${bundleId}_${platform}`;

		// Get current build number from KV (default to 0 if not exists)
		const currentBuildNumber = await c.env.BUILD_VERSION_SYNC.get(kvKey);
		const nextBuildNumber = currentBuildNumber ? parseInt(currentBuildNumber) + 1 : 1;

		// Store the incremented build number
		await c.env.BUILD_VERSION_SYNC.put(kvKey, nextBuildNumber.toString());

		return {
			success: true,
			buildNumber: nextBuildNumber,
		};
	}
}
