"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.createApp = createApp;
const express_1 = __importDefault(require("express"));
const helmet_1 = __importDefault(require("helmet"));
const cors_1 = __importDefault(require("cors"));
const morgan_1 = __importDefault(require("morgan"));
const item_routes_1 = __importDefault(require("../../api/routes/item.routes"));
const notFound_middleware_1 = require("../middleware/notFound.middleware");
const error_middleware_1 = require("../middleware/error.middleware");
function createApp() {
    const app = (0, express_1.default)();
    app.use((0, helmet_1.default)());
    app.use((0, cors_1.default)());
    app.use((0, morgan_1.default)("dev"));
    app.use(express_1.default.json());
    app.get("/health", (req, res) => {
        res.json({ status: "ok" });
    });
    app.use("/api", item_routes_1.default);
    app.use(notFound_middleware_1.notFoundHandler);
    app.use(error_middleware_1.errorHandler);
    return app;
}
//# sourceMappingURL=app.js.map