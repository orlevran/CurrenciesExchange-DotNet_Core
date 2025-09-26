import { Router } from "express";
import { itemController } from "../controllers/item.controller";

const router = Router();

router.get("/items", itemController.list);
router.post("/items", itemController.create);
router.put("/items/:id", itemController.update);

export default router;

