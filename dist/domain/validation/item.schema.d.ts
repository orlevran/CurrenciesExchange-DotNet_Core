import { z } from "zod";
export declare const createItemSchema: z.ZodObject<{
    name: z.ZodString;
    price: z.ZodNumber;
    description: z.ZodOptional<z.ZodString>;
}, z.core.$strip>;
export type CreateItemInput = z.infer<typeof createItemSchema>;
export declare const updateItemSchema: z.ZodObject<{
    name: z.ZodOptional<z.ZodString>;
    price: z.ZodOptional<z.ZodNumber>;
    description: z.ZodOptional<z.ZodString>;
}, z.core.$strip>;
export type UpdateItemInput = z.infer<typeof updateItemSchema>;
export declare const idParamSchema: z.ZodObject<{
    id: z.ZodString;
}, z.core.$strip>;
//# sourceMappingURL=item.schema.d.ts.map