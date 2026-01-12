import { Bool, Int, Str, OpenAPIRoute } from "chanfana";
import { z } from "zod";
import { type AppContext } from "../types";

export class SetBuildNumber extends OpenAPIRoute {
	schema = {
		tags: ["BuildNumber"],
		summary: "Set current synced build number for a bundle",
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
				buildNumber: Int({
					description: "New build number to set",
					required: true,
				}),
			}),
		},
		responses: {
			"200": {
				description: "Returns the stored build number",
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
		// Validate input
		const data = await this.getValidatedData<typeof this.schema>();
		const { bundleId, platform, buildNumber } = data.query;

		// Key pattern matches GetNextBuildNumber
		const kvKey = `build_number_${bundleId}_${platform}`;

		// Store provided build number
		await c.env.BUILD_VERSION_SYNC.put(kvKey, buildNumber.toString());

		return {
			success: true,
			buildNumber: buildNumber,
		};
	}
}
