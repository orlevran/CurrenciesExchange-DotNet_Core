import type { Request, Response, NextFunction } from "express";

export function notFoundHandler(req: Request, res: Response, next: NextFunction) {
  res.status(404).json({ message: "Not Found" });
}

