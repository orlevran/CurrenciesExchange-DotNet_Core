"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.idParamSchema = exports.updateItemSchema = exports.createItemSchema = void 0;
const zod_1 = require("zod");
exports.createItemSchema = zod_1.z.object({
    name: zod_1.z.string().min(1, "name is required"),
    price: zod_1.z.number().positive("price must be > 0"),
    description: zod_1.z.string().trim().optional(),
});
exports.updateItemSchema = zod_1.z
    .object({
    name: zod_1.z.string().min(1, "name cannot be empty").optional(),
    price: zod_1.z.number().positive("price must be > 0").optional(),
    description: zod_1.z.string().trim().optional(),
})
    .refine((value) => Object.keys(value).length > 0, {
    message: "At least one field must be provided",
});
exports.idParamSchema = zod_1.z.object({
    id: zod_1.z.string().uuid("id must be a UUID"),
});
//# sourceMappingURL=item.schema.js.map