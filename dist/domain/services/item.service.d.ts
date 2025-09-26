import type { Item } from "../models/item";
import type { CreateItemInput, UpdateItemInput } from "../validation/item.schema";
declare class ItemService {
    private readonly idToItem;
    listItems(): Item[];
    createItem(input: CreateItemInput): Item;
    updateItem(id: string, input: UpdateItemInput): Item;
}
export declare const itemService: ItemService;
export {};
//# sourceMappingURL=item.service.d.ts.map