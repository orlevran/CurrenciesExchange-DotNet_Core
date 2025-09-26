import { Router } from "express";
import { getItems, postItem, putItem } from "../controllers/itemController";

const router = Router();

router.get("/", getItems);
router.post("/", postItem);
router.put("/:id", putItem);

export default router;
