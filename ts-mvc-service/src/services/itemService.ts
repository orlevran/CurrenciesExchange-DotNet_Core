import { Item } from "../models/Item";
import { CreateItemInput, UpdateItemInput } from "../schemas/itemSchemas";
import { HttpError } from "../utils/HttpError";
import { randomUUID } from "crypto";

const items = new Map<string, Item>();

function generateId(): string {
  try {
    return randomUUID();
  } catch {
    return `${Date.now()}-${Math.random().toString(16).slice(2)}`;
  }
}

export function listItems(): Item[] {
  return Array.from(items.values());
}

export function createItem(input: CreateItemInput): Item {
  const newItem: Item = { id: generateId(), name: input.name, description: input.description };
  items.set(newItem.id, newItem);
  return newItem;
}

export function updateItem(itemId: string, input: UpdateItemInput): Item {
  const existing = items.get(itemId);
  if (!existing) {
    throw new HttpError(404, `Item with id ${itemId} not found`);
  }
  const updated: Item = {
    ...existing,
    ...input
  };
  items.set(itemId, updated);
  return updated;
}
