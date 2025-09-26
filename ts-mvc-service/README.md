# TypeScript MVC Microservice (Express)

A minimal Express microservice in TypeScript following an MVC layering approach (routes → controllers → services → models), with basic middlewares and validation via Zod.

## Stack
- Node.js + TypeScript
- Express 5
- Zod for request body validation

## Project Structure
```
src/
  controllers/
    itemController.ts
  middleware/
    errorHandler.ts
    notFound.ts
  models/
    Item.ts
  routes/
    itemRoutes.ts
  schemas/
    itemSchemas.ts
  services/
    itemService.ts
  index.ts
  server.ts
```

## Scripts
- `npm run dev`: Start in watch mode with ts-node-dev
- `npm run build`: Compile TypeScript to `dist/`
- `npm start`: Run compiled server

## Getting Started
```bash
npm install
npm run dev
# or
npm run build && npm start
```

Server starts on `http://localhost:3000`.

## Endpoints
- GET `/items` → List all items
- POST `/items` → Create an item `{ name: string, description?: string }`
- PUT `/items/:id` → Update an item `{ name?: string, description?: string }`
- GET `/health` → Health check

## Example Requests
```bash
# List items
curl -s http://localhost:3000/items | jq

# Create item
curl -s -X POST http://localhost:3000/items \
  -H 'Content-Type: application/json' \
  -d '{"name":"Widget","description":"My widget"}' | jq

# Update item
curl -s -X PUT http://localhost:3000/items/REPLACE_ID \
  -H 'Content-Type: application/json' \
  -d '{"description":"Updated"}' | jq
```

## Notes
- Storage is in-memory (`Map`) for simplicity; replace `services/itemService.ts` with your persistence (DB/ORM) layer.
- `HttpError` encapsulates typed errors from the service layer.
- Zod schemas validate inputs close to the controller boundary.
