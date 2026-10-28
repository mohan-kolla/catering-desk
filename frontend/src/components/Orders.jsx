import { api } from "../api.js";
import { useLoad } from "./useLoad.js";

const dateFmt = new Intl.DateTimeFormat("en-US", { month: "short", day: "numeric", year: "numeric" });

export default function Orders() {
  const { data: orders, setData, error } = useLoad(api.getOrders);

  async function remove(id) {
    await api.deleteOrder(id);
    setData((prev) => prev.filter((o) => o.id !== id));
  }

  if (error) return <p className="error">{error}</p>;
  if (!orders) return <p className="muted">Loading orders...</p>;
  if (orders.length === 0) return <p className="muted">No orders yet. Plan one to see it here.</p>;

  return (
    <ul className="order-list">
      {orders.map((o) => (
        <li key={o.id} className="order">
          <div className="order-head">
            <strong>{o.customerName}</strong>
            <span className={`status ${o.status.toLowerCase()}`}>{o.status}</span>
          </div>
          <p className="muted">
            {dateFmt.format(new Date(o.eventDate))}, {o.headcount} guests
          </p>
          <p>{o.lines.map((l) => `${l.menuItem} (${l.servings})`).join(", ")}</p>
          <button className="small ghost" onClick={() => remove(o.id)}>Delete order</button>
        </li>
      ))}
    </ul>
  );
}
