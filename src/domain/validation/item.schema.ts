import { z } from "zod";

export const createItemSchema = z.object({
  name: z.string().min(1, "name is required"),
  price: z.number().positive("price must be > 0"),
  description: z.string().trim().optional(),
});

export type CreateItemInput = z.infer<typeof createItemSchema>;

export const updateItemSchema = z
  .object({
    name: z.string().min(1, "name cannot be empty").optional(),
    price: z.number().positive("price must be > 0").optional(),
    description: z.string().trim().optional(),
  })
  .refine(
    (value) => Object.keys(value).length > 0,
    {
      message: "At least one field must be provided",
    }
  );

export type UpdateItemInput = z.infer<typeof updateItemSchema>;

export const idParamSchema = z.object({
  id: z.string().uuid("id must be a UUID"),
});

