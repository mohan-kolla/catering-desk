# Catering Desk

A full-stack web app that checks catering orders against kitchen inventory before a restaurant commits to them. Pick dishes and a guest count, and it calculates every ingredient the order needs, flags what's short, and estimates food cost, revenue, and margin.

**All data is randomly generated.** This is a portfolio demo modeled on real restaurant catering problems, but it contains no real business data. The generator uses a fixed seed, so every reset produces the same fictional restaurant.

## Stack

| Layer | Tech |
| --- | --- |
| API | C#, ASP.NET Core 8 Web API |
| Data | Entity Framework Core, SQLite |
| Demo data | [Bogus](https://github.com/bchavez/Bogus) |
| Frontend | React 18, Vite |
| Tests | xUnit |
| CI | GitHub Actions (API tests, ESLint, frontend build) |

## Features

- **Order planning:** totals ingredient needs across every dish in an order and compares them to stock on hand.
- **Shortfall report:** lists exactly how much of each ingredient to buy before the event.
- **Cost estimate:** food cost, order revenue, and margin for each order.
- **Inventory:** stock levels with low-stock flags and one-click restocking.
- **Orders:** saved catering orders with status.
- **Demo reset:** restores the generated data after visitors experiment.

## Running locally

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download) and [Node.js 20+](https://nodejs.org).

**API** (runs at http://localhost:5080, Swagger UI at /swagger):

```bash
cd backend/CateringDemo.Api
dotnet run
```

**Frontend** (runs at http://localhost:5173):

```bash
cd frontend
cp .env.example .env
npm install
npm run dev
```

**Tests:**

```bash
dotnet test backend/CateringDemo.Tests
```

## API

| Method | Route | Description |
| --- | --- | --- |
| GET | `/api/inventory` | All ingredients with stock levels |
| GET | `/api/inventory/low-stock` | Ingredients at or below reorder level |
| PATCH | `/api/inventory/{id}/restock` | Add stock to an ingredient |
| GET | `/api/menu` | Menu items with prices |
| GET | `/api/orders` | All catering orders |
| GET | `/api/orders/{id}` | One order |
| POST | `/api/orders/check` | Check an order against stock without saving |
| POST | `/api/orders` | Save an order and return its stock check |
| DELETE | `/api/orders/{id}` | Delete an order |
| POST | `/api/demo/reset` | Restore the generated demo data |

## How the stock check works

`ShortfallCalculator` (in `backend/CateringDemo.Api/Services`) is a pure function: it takes menu items with their recipes and servings, sums the quantity of each ingredient across all dishes, and compares the total to what's on hand. Keeping it free of database code is what makes it straightforward to unit test.

## Deployment

The API ships with a `Dockerfile` (in `backend/`) and can run on any container host, such as Render or Azure App Service. Set the `AllowedOrigins` environment variable to your frontend's URL so the browser is allowed to call the API. The frontend builds to static files (`npm run build`) and can be hosted on Vercel, Netlify, or GitHub Pages. Set `VITE_API_URL` to the deployed API's URL before building.

SQLite lives inside the container, so data resets on each redeploy. That's intentional for a demo.
