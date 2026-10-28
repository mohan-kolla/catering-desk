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
  checkOrder: (lines) =>
    request("/api/orders/check", { method: "POST", body: JSON.stringify({ lines }) }),
  createOrder: (order) =>
    request("/api/orders", { method: "POST", body: JSON.stringify(order) }),
  deleteOrder: (id) => request(`/api/orders/${id}`, { method: "DELETE" }),
  resetDemo: () => request("/api/demo/reset", { method: "POST" }),
};

export const money = (n) =>
  n.toLocaleString("en-US", { style: "currency", currency: "USD" });
