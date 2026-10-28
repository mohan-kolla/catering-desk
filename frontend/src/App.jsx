import { useState } from "react";
import { api } from "./api.js";
import Inventory from "./components/Inventory.jsx";
import PlanOrder from "./components/PlanOrder.jsx";
import Orders from "./components/Orders.jsx";

const VIEWS = [
  { id: "plan", label: "Plan an order", Component: PlanOrder },
  { id: "inventory", label: "Inventory", Component: Inventory },
  { id: "orders", label: "Orders", Component: Orders },
];

export default function App() {
  const [view, setView] = useState("plan");
  const [refreshKey, setRefreshKey] = useState(0); // remounts views after a reset
  const [resetting, setResetting] = useState(false);

  const { Component } = VIEWS.find((v) => v.id === view);

  async function handleReset() {
    setResetting(true);
    try {
      await api.resetDemo();
      setRefreshKey((k) => k + 1);
    } finally {
      setResetting(false);
    }
  }

  return (
    <div className="shell">
      <header className="masthead">
        <div>
          <h1>Catering Desk</h1>
          <p className="tagline">Check catering orders against kitchen stock before you say yes.</p>
        </div>
        <button className="ghost" onClick={handleReset} disabled={resetting}>
          {resetting ? "Resetting..." : "Reset demo data"}
        </button>
      </header>

      <nav className="tabs" aria-label="Sections">
        {VIEWS.map((v) => (
          <button
            key={v.id}
            className={v.id === view ? "tab active" : "tab"}
            aria-current={v.id === view ? "page" : undefined}
            onClick={() => setView(v.id)}
          >
            {v.label}
          </button>
        ))}
      </nav>

      <main>
        <Component key={`${view}-${refreshKey}`} />
      </main>

      <footer>
        Portfolio demo. All restaurant data is randomly generated; no real business data is used.
      </footer>
    </div>
  );
}
