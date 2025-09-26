import type { Request, Response, NextFunction } from "express";
import { itemService } from "../../domain/services/item.service";
import {
  createItemSchema,
  idParamSchema,
  updateItemSchema,
} from "../../domain/validation/item.schema";

export class ItemController {
  list = (req: Request, res: Response) => {
    const items = itemService.listItems();
    res.json(items);
  };

  create = (req: Request, res: Response, next: NextFunction) => {
    try {
      const parsed = createItemSchema.parse(req.body);
      const created = itemService.createItem(parsed);
      res.status(201).json(created);
    } catch (err) {
      next(err);
    }
  };

  update = (req: Request, res: Response, next: NextFunction) => {
    try {
      const { id } = idParamSchema.parse(req.params);
      const parsed = updateItemSchema.parse(req.body);
      const updated = itemService.updateItem(id, parsed);
      res.json(updated);
    } catch (err) {
      next(err);
    }
  };
}

export const itemController = new ItemController();

