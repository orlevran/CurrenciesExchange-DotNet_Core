import express from "express";
import itemRoutes from "./routes/itemRoutes";
import { notFound } from "./middleware/notFound";
import { errorHandler } from "./middleware/errorHandler";

export const app = express();

app.use(express.json());

app.get("/health", (_req, res) => {
  res.status(200).send("OK");
});

app.use("/items", itemRoutes);

app.use(notFound);
app.use(errorHandler);
