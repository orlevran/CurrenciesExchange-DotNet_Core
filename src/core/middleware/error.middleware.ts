import type { ErrorRequestHandler } from "express";
import { ZodError } from "zod";

export const errorHandler: ErrorRequestHandler = (err, req, res, next) => {
  if (err instanceof ZodError) {
    return res.status(400).json({
      message: "Validation failed",
      errors: err.issues,
    });
  }

  const status = (err as any).status ?? 500;
  const message = err?.message ?? "Internal Server Error";
  res.status(status).json({ message });
};

