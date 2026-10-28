import { useState } from "react";
import { api, REGIONS } from "./api.js";
import Inventory from "./components/Inventory.jsx";
import PlanOrder from "./components/PlanOrder.jsx";
import Orders from "./components/Orders.jsx";

const VIEWS = [
  { id: "plan", label: "Plan an order" },
  { id: "inventory", label: "Inventory" },
  { id: "orders", label: "Orders" },
];

const REGION_KEY = "cateringDesk.region";

function savedRegion() {
  try {
    const stored = localStorage.getItem(REGION_KEY);
    return REGIONS.some((r) => r.code === stored) ? stored : "0000";
  } catch {
    return "0000";
  }
}

export default function App() {
  const [view, setView] = useState("plan");
  const [region, setRegion] = useState(savedRegion);
  const [refreshKey, setRefreshKey] = useState(0); // remounts views after a reset
  const [resetting, setResetting] = useState(false);

  function changeRegion(code) {
    setRegion(code);
    try {
      localStorage.setItem(REGION_KEY, code);
    } catch {
      // Storage can be unavailable (private browsing); the choice still applies this session.
    }
  }

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

      <div className="toolbar">
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
        <label className="region-picker">
          Ingredient prices
          <select value={region} onChange={(e) => changeRegion(e.target.value)}>
            {REGIONS.map((r) => (
              <option key={r.code} value={r.code}>{r.name}</option>
            ))}
          </select>
        </label>
      </div>

      <main>
        {/* The planner stays mounted while hidden, so a half-built order survives
            a trip to Inventory or Orders. Those two reload fresh each time they open. */}
        <div hidden={view !== "plan"}>
          <PlanOrder key={`plan-${refreshKey}`} region={region} active={view === "plan"} />
        </div>
        {view === "inventory" && <Inventory key={`inventory-${refreshKey}`} region={region} />}
        {view === "orders" && <Orders key={`orders-${refreshKey}`} />}
      </main>

      <footer>
        Portfolio demo. Restaurant data is randomly generated; no real business data is used.
        Ingredient prices come from U.S. Bureau of Labor Statistics retail averages where available.
      </footer>
    </div>
  );
}
