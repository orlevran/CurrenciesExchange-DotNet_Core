"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.env = void 0;
const dotenv_1 = __importDefault(require("dotenv"));
dotenv_1.default.config();
function getEnv(name, fallback) {
    const value = process.env[name] ?? fallback;
    if (value === undefined) {
        throw new Error(`Missing required env var ${name}`);
    }
    return value;
}
exports.env = {
    NODE_ENV: getEnv("NODE_ENV", "development"),
    PORT: parseInt(getEnv("PORT", "3000"), 10),
};
//# sourceMappingURL=env.js.map