"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.itemController = exports.ItemController = void 0;
const item_service_1 = require("../../domain/services/item.service");
const item_schema_1 = require("../../domain/validation/item.schema");
class ItemController {
    constructor() {
        this.list = (req, res) => {
            const items = item_service_1.itemService.listItems();
            res.json(items);
        };
        this.create = (req, res, next) => {
            try {
                const parsed = item_schema_1.createItemSchema.parse(req.body);
                const created = item_service_1.itemService.createItem(parsed);
                res.status(201).json(created);
            }
            catch (err) {
                next(err);
            }
        };
        this.update = (req, res, next) => {
            try {
                const { id } = item_schema_1.idParamSchema.parse(req.params);
                const parsed = item_schema_1.updateItemSchema.parse(req.body);
                const updated = item_service_1.itemService.updateItem(id, parsed);
                res.json(updated);
            }
            catch (err) {
                next(err);
            }
        };
    }
}
exports.ItemController = ItemController;
exports.itemController = new ItemController();
//# sourceMappingURL=item.controller.js.map