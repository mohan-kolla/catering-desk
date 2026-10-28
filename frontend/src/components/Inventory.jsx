import { api, money } from "../api.js";
import { useLoad } from "./useLoad.js";

const RESTOCK_AMOUNT = 25;

export default function Inventory() {
  const { data: items, setData, error } = useLoad(api.getInventory);

  async function restock(id) {
    const updated = await api.restock(id, RESTOCK_AMOUNT);
    setData((prev) => prev.map((i) => (i.id === id ? updated : i)));
  }

  if (error) return <p className="error">{error}</p>;
  if (!items) return <p className="muted">Loading inventory...</p>;

  const lowCount = items.filter((i) => i.quantityOnHand <= i.reorderLevel).length;

  return (
    <section>
      <p className="summary">
        {lowCount === 0 ? "Everything is above its reorder level." : `${lowCount} items at or below reorder level.`}
      </p>
      <div className="table-wrap">
        <table>
          <thead>
            <tr>
              <th scope="col">Ingredient</th>
              <th scope="col" className="num">On hand</th>
              <th scope="col" className="num">Reorder at</th>
              <th scope="col" className="num">Cost / unit</th>
              <th scope="col"><span className="sr-only">Actions</span></th>
            </tr>
          </thead>
          <tbody>
            {items.map((i) => {
              const low = i.quantityOnHand <= i.reorderLevel;
              return (
                <tr key={i.id} className={low ? "low" : undefined}>
                  <td>
                    {i.name}
                    {low && <span className="flag">Low</span>}
                  </td>
                  <td className="num">{i.quantityOnHand} {i.unit}</td>
                  <td className="num">{i.reorderLevel} {i.unit}</td>
                  <td className="num">{money(i.costPerUnit)}</td>
                  <td className="num">
                    <button className="small ghost" onClick={() => restock(i.id)}>
                      Restock +{RESTOCK_AMOUNT}
                    </button>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </section>
  );
}
