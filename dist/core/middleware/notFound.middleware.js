"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.notFoundHandler = notFoundHandler;
function notFoundHandler(req, res, next) {
    res.status(404).json({ message: "Not Found" });
}
//# sourceMappingURL=notFound.middleware.js.map