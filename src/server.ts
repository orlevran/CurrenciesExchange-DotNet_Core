import { createServer } from "node:http";
import { createApp } from "./core/config/app";
import { env } from "./core/config/env";

const app = createApp();
const server = createServer(app);

server.listen(env.PORT, () => {
  // eslint-disable-next-line no-console
  console.log(`Server listening on http://localhost:${env.PORT}`);
});

