import type { Request, Response, NextFunction } from "express";
export declare class ItemController {
    list: (req: Request, res: Response) => void;
    create: (req: Request, res: Response, next: NextFunction) => void;
    update: (req: Request, res: Response, next: NextFunction) => void;
}
export declare const itemController: ItemController;
//# sourceMappingURL=item.controller.d.ts.map