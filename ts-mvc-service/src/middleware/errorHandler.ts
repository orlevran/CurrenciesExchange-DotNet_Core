import { Request, Response, NextFunction } from "express";
import { ZodError } from "zod";
import { HttpError } from "../utils/HttpError";

export function errorHandler(err: unknown, req: Request, res: Response, _next: NextFunction) {
  if (err instanceof ZodError) {
    return res.status(400).json({ error: "Validation error", details: err.flatten() });
  }
  if (err instanceof HttpError) {
    return res.status(err.statusCode).json({ error: err.message });
  }
  console.error("Unhandled error:", err);
  return res.status(500).json({ error: "Internal Server Error" });
}
