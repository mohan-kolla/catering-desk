import { useState } from "react";
import { api, money } from "../api.js";
import { useLoad } from "./useLoad.js";

export default function PlanOrder() {
  const { data: menu, error: loadError } = useLoad(api.getMenu);
  const [customer, setCustomer] = useState("");
  const [eventDate, setEventDate] = useState(new Date().toISOString().slice(0, 10));
  const [headcount, setHeadcount] = useState(50);
  const [selected, setSelected] = useState({}); // menuItemId -> servings
  const [check, setCheck] = useState(null);
  const [message, setMessage] = useState("");

  const lines = Object.entries(selected).map(([id, servings]) => ({
    menuItemId: Number(id),
    servings: Number(servings),
  }));

  function toggle(id) {
    setCheck(null);
    setSelected((prev) => {
      const next = { ...prev };
      if (id in next) delete next[id];
      else next[id] = headcount;
      return next;
    });
  }

  function setServings(id, value) {
    setCheck(null);
    setSelected((prev) => ({ ...prev, [id]: value }));
  }

  async function runCheck() {
    setMessage("");
    try {
      setCheck(await api.checkOrder(lines));
    } catch (e) {
      setMessage(e.message);
    }
  }

  async function save() {
    setMessage("");
    try {
      await api.createOrder({ customerName: customer, eventDate, headcount: Number(headcount), lines });
      setMessage(`Order saved for ${customer}.`);
      setSelected({});
      setCheck(null);
      setCustomer("");
    } catch (e) {
      setMessage(e.message);
    }
  }

  if (loadError) return <p className="error">{loadError}</p>;
  if (!menu) return <p className="muted">Loading menu...</p>;

  return (
    <div className="plan">
      <form className="plan-form" onSubmit={(e) => { e.preventDefault(); runCheck(); }}>
        <div className="field-row">
          <label>
            Customer
            <input value={customer} onChange={(e) => setCustomer(e.target.value)} placeholder="Riverside Book Club" />
          </label>
          <label>
            Event date
            <input type="date" value={eventDate} onChange={(e) => setEventDate(e.target.value)} />
          </label>
          <label>
            Guests
            <input type="number" min="1" value={headcount} onChange={(e) => setHeadcount(e.target.value)} />
          </label>
        </div>

        <fieldset>
          <legend>Menu</legend>
          {menu.map((m) => {
            const on = m.id in selected;
            return (
              <div key={m.id} className={on ? "menu-row on" : "menu-row"}>
                <label className="check">
                  <input type="checkbox" checked={on} onChange={() => toggle(m.id)} />
                  <span>{m.name}</span>
                  <span className="muted">{m.category}, {money(m.pricePerServing)}/serving</span>
                </label>
                {on && (
                  <label className="servings">
                    Servings
                    <input type="number" min="1" value={selected[m.id]} onChange={(e) => setServings(m.id, e.target.value)} />
                  </label>
                )}
              </div>
            );
          })}
        </fieldset>

        <div className="actions">
          <button type="submit" disabled={lines.length === 0}>Check stock</button>
          <button type="button" className="ghost" disabled={!check || !customer.trim()} onClick={save}>
            Save order
          </button>
        </div>
        {message && <p className="message" role="status">{message}</p>}
      </form>

      <aside className="ticket" aria-live="polite">
        {!check ? (
          <p className="muted">Pick dishes and check stock. The prep ticket shows what you have and what you need to buy.</p>
        ) : (
          <>
            <h2 className={check.canFulfill ? "ok" : "short"}>
              {check.canFulfill ? "Stock covers this order" : "Short on ingredients"}
            </h2>
            <table>
              <thead>
                <tr>
                  <th scope="col">Ingredient</th>
                  <th scope="col" className="num">Need</th>
                  <th scope="col" className="num">Have</th>
                  <th scope="col" className="num">Buy</th>
                </tr>
              </thead>
              <tbody>
                {check.lines.map((l) => (
                  <tr key={l.ingredientId} className={l.short > 0 ? "short" : undefined}>
                    <td>{l.ingredient}</td>
                    <td className="num">{l.needed} {l.unit}</td>
                    <td className="num">{l.onHand}</td>
                    <td className="num">{l.short > 0 ? l.short : ""}</td>
                  </tr>
                ))}
              </tbody>
            </table>
            <dl className="totals">
              <div><dt>Food cost</dt><dd>{money(check.foodCost)}</dd></div>
              <div><dt>Order revenue</dt><dd>{money(check.revenue)}</dd></div>
              <div><dt>Margin</dt><dd>{money(check.revenue - check.foodCost)}</dd></div>
            </dl>
          </>
        )}
      </aside>
    </div>
  );
}
