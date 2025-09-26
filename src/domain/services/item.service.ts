import { randomUUID } from "node:crypto";
import type { Item } from "../models/item";
import type { CreateItemInput, UpdateItemInput } from "../validation/item.schema";

class ItemService {
  private readonly idToItem: Map<string, Item> = new Map();

  listItems(): Item[] {
    return Array.from(this.idToItem.values());
  }

  createItem(input: CreateItemInput): Item {
    const nowIso = new Date().toISOString();
    const item: Item = {
      id: randomUUID(),
      name: input.name,
      price: input.price,
      description: input.description,
      createdAt: nowIso,
      updatedAt: nowIso,
    };
    this.idToItem.set(item.id, item);
    return item;
  }

  updateItem(id: string, input: UpdateItemInput): Item {
    const existing = this.idToItem.get(id);
    if (!existing) {
      const notFoundError = new Error("Item not found");
      // @ts-expect-error add status for error handler
      notFoundError.status = 404;
      throw notFoundError;
    }

    const updated: Item = {
      ...existing,
      name: input.name ?? existing.name,
      price: input.price ?? existing.price,
      description: input.description ?? existing.description,
      updatedAt: new Date().toISOString(),
    };
    this.idToItem.set(id, updated);
    return updated;
  }
}

export const itemService = new ItemService();

