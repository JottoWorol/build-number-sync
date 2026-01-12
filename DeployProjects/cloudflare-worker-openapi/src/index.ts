import { fromHono } from "chanfana";
import { Hono } from "hono";
import { GetNextBuildNumber } from "./endpoints/getNextBuildNumber";
import { DeleteBundleId } from "./endpoints/deleteBundleId";
import { SetBuildNumber } from "./endpoints/setBuildNumber";

// Start a Hono app
const app = new Hono<{ Bindings: Env }>();

// Setup OpenAPI registry
const openapi = fromHono(app, {
	docs_url: "/",
});

// Register OpenAPI endpoints
openapi.get("/getNextBuildNumber", GetNextBuildNumber);
openapi.delete("/deleteBundleId", DeleteBundleId);
openapi.post("/setBuildNumber", SetBuildNumber);

// Export the Hono app
export default app;
