import { Request, Response, NextFunction } from "express";
import { createItemSchema, updateItemSchema } from "../schemas/itemSchemas";
import * as service from "../services/itemService";

export async function getItems(req: Request, res: Response, next: NextFunction) {
  try {
    const data = service.listItems();
    res.status(200).json(data);
  } catch (err) {
    next(err);
  }
}

export async function postItem(req: Request, res: Response, next: NextFunction) {
  try {
    const parsed = createItemSchema.parse(req.body);
    const created = service.createItem(parsed);
    res.status(201).json(created);
  } catch (err) {
    next(err);
  }
}

export async function putItem(req: Request, res: Response, next: NextFunction) {
  try {
    const itemId = req.params.id;
    const parsed = updateItemSchema.parse(req.body);
    const updated = service.updateItem(itemId, parsed);
    res.status(200).json(updated);
  } catch (err) {
    next(err);
  }
}
