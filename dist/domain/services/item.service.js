"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.itemService = void 0;
const node_crypto_1 = require("node:crypto");
class ItemService {
    constructor() {
        this.idToItem = new Map();
    }
    listItems() {
        return Array.from(this.idToItem.values());
    }
    createItem(input) {
        const nowIso = new Date().toISOString();
        const item = {
            id: (0, node_crypto_1.randomUUID)(),
            name: input.name,
            price: input.price,
            description: input.description,
            createdAt: nowIso,
            updatedAt: nowIso,
        };
        this.idToItem.set(item.id, item);
        return item;
    }
    updateItem(id, input) {
        const existing = this.idToItem.get(id);
        if (!existing) {
            const notFoundError = new Error("Item not found");
            // @ts-expect-error add status for error handler
            notFoundError.status = 404;
            throw notFoundError;
        }
        const updated = {
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
exports.itemService = new ItemService();
//# sourceMappingURL=item.service.js.map