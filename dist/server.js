"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const node_http_1 = require("node:http");
const app_1 = require("./core/config/app");
const env_1 = require("./core/config/env");
const app = (0, app_1.createApp)();
const server = (0, node_http_1.createServer)(app);
server.listen(env_1.env.PORT, () => {
    // eslint-disable-next-line no-console
    console.log(`Server listening on http://localhost:${env_1.env.PORT}`);
});
//# sourceMappingURL=server.js.map