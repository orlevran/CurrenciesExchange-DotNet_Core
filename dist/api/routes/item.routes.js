"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const express_1 = require("express");
const item_controller_1 = require("../controllers/item.controller");
const router = (0, express_1.Router)();
router.get("/items", item_controller_1.itemController.list);
router.post("/items", item_controller_1.itemController.create);
router.put("/items/:id", item_controller_1.itemController.update);
exports.default = router;
//# sourceMappingURL=item.routes.js.map