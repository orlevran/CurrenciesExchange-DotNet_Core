import express from "express";
import helmet from "helmet";
import cors from "cors";
import morgan from "morgan";
import itemRoutes from "../../api/routes/item.routes";
import { notFoundHandler } from "../middleware/notFound.middleware";
import { errorHandler } from "../middleware/error.middleware";

export function createApp() {
  const app = express();

  app.use(helmet());
  app.use(cors());
  app.use(morgan("dev"));
  app.use(express.json());

  app.get("/health", (req, res) => {
    res.json({ status: "ok" });
  });

  app.use("/api", itemRoutes);

  app.use(notFoundHandler);
  app.use(errorHandler);

  return app;
}

