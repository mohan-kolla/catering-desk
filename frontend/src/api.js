const BASE = import.meta.env.VITE_API_URL ?? "http://localhost:5080";

async function request(path, options = {}) {
  const res = await fetch(`${BASE}${path}`, {
    headers: { "Content-Type": "application/json" },
    ...options,
  });
  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || `Request failed with status ${res.status}`);
  }
  return res.status === 204 ? null : res.json();
}

export const api = {
  getInventory: () => request("/api/inventory"),
  restock: (id, amount) =>
    request(`/api/inventory/${id}/restock`, { method: "PATCH", body: JSON.stringify({ amount }) }),
  getMenu: () => request("/api/menu"),
  getOrders: () => request("/api/orders"),
  getPrices: (region) => request(`/api/prices?region=${encodeURIComponent(region)}`),
  checkOrder: (lines, region) =>
    request("/api/orders/check", { method: "POST", body: JSON.stringify({ lines, region }) }),
  createOrder: (order) =>
    request("/api/orders", { method: "POST", body: JSON.stringify(order) }),
  deleteOrder: (id) => request(`/api/orders/${id}`, { method: "DELETE" }),
  resetDemo: () => request("/api/demo/reset", { method: "POST" }),
};

// BLS area codes, matching Services/BlsCatalog.cs on the API.
export const REGIONS = [
  { code: "0000", name: "U.S. average" },
  { code: "0100", name: "Northeast" },
  { code: "0200", name: "Midwest" },
  { code: "0300", name: "South" },
  { code: "0400", name: "West" },
];

export const regionName = (code) => REGIONS.find((r) => r.code === code)?.name ?? code;

// Reads naturally in a sentence: "U.S. averages" or "averages for the South region".
export const regionPhrase = (code) =>
  code === "0000" ? "U.S. averages" : `averages for the ${regionName(code)} region`;

// "2026-08" -> "August 2026"
export const formatPeriod = (period) => {
  if (!period) return "";
  const [year, month] = period.split("-").map(Number);
  return new Date(year, month - 1, 1).toLocaleString("en-US", { month: "long", year: "numeric" });
};

export const sourceLabel = (source, region) => {
  if (source === "regional") return `BLS, ${regionName(region)}`;
  if (source === "national") return "BLS, U.S. average";
  return "Estimated";
};

export const money = (n) =>
  n.toLocaleString("en-US", { style: "currency", currency: "USD" });
