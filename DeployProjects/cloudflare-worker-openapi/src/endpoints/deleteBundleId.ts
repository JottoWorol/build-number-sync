import { Bool, Str, OpenAPIRoute } from "chanfana";
import { z } from "zod";
import { type AppContext } from "../types";

export class DeleteBundleId extends OpenAPIRoute {
	schema = {
		tags: ["BuildNumber"],
		summary: "Delete a bundle ID and its build number",
		request: {
			query: z.object({
				bundleId: Str({
					description: "Bundle ID to delete",
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
				description: "Bundle ID deleted successfully",
				content: {
					"application/json": {
						schema: z.object({
							success: Bool(),
							message: Str(),
						}),
					},
				},
			},
			"404": {
				description: "Bundle ID not found",
				content: {
					"application/json": {
						schema: z.object({
							success: Bool(),
							message: Str(),
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

		// Create the key for this bundle ID
		const kvKey = `build_number_${bundleId}_${platform}`;

		// Check if the key exists
		const exists = await c.env.BUILD_VERSION_SYNC.get(kvKey);

		if (!exists) {
			return c.json({
				success: false,
				message: `Bundle ID '${bundleId}' not found`,
			}, 404);
		}

		// Delete the key from KV
		await c.env.BUILD_VERSION_SYNC.delete(kvKey);

		return {
			success: true,
			message: `Bundle ID '${bundleId}' deleted successfully`,
		};
	}
}
