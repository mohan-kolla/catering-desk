import { useCallback } from "react";
import { api, money, formatPeriod, regionPhrase, sourceLabel } from "../api.js";
import { useLoad } from "./useLoad.js";

const RESTOCK_AMOUNT = 25;

export default function Inventory({ region }) {
  const loader = useCallback(
    () =>
      Promise.all([api.getInventory(), api.getPrices(region)]).then(([items, priceList]) => ({
        items,
        priceList,
      })),
    [region]
  );
  const { data, setData, error } = useLoad(loader);

  async function restock(id) {
    const updated = await api.restock(id, RESTOCK_AMOUNT);
    setData((prev) => ({ ...prev, items: prev.items.map((i) => (i.id === id ? updated : i)) }));
  }

  if (error) return <p className="error">{error}</p>;
  if (!data) return <p className="muted">Loading inventory...</p>;

  const { items, priceList } = data;
  const prices = new Map(priceList.prices.map((p) => [p.ingredientId, p]));
  const lowCount = items.filter((i) => i.quantityOnHand <= i.reorderLevel).length;
  const latestPeriod = priceList.prices
    .map((p) => p.period)
    .filter(Boolean)
    .sort()
    .at(-1);

  return (
    <section>
      <p className="summary">
        {lowCount === 0 ? "Everything is above its reorder level." : `${lowCount} items at or below reorder level.`}
      </p>
      <p className="price-note">
        {latestPeriod
          ? `Prices: BLS retail ${regionPhrase(region)}, ${formatPeriod(latestPeriod)}. Items BLS doesn't track use estimates.`
          : "Live market prices are unavailable right now, so all prices are estimates."}
      </p>
      <div className="table-wrap">
        <table>
          <thead>
            <tr>
              <th scope="col">Ingredient</th>
              <th scope="col" className="num">On hand</th>
              <th scope="col" className="num">Reorder at</th>
              <th scope="col" className="num">Price / unit</th>
              <th scope="col">Price source</th>
              <th scope="col"><span className="sr-only">Actions</span></th>
            </tr>
          </thead>
          <tbody>
            {items.map((i) => {
              const low = i.quantityOnHand <= i.reorderLevel;
              const price = prices.get(i.id);
              return (
                <tr key={i.id} className={low ? "low" : undefined}>
                  <td>
                    {i.name}
                    {low && <span className="flag">Low</span>}
                  </td>
                  <td className="num">{i.quantityOnHand} {i.unit}</td>
                  <td className="num">{i.reorderLevel} {i.unit}</td>
                  <td className="num">{money(price?.pricePerUnit ?? i.costPerUnit)}</td>
                  <td>
                    <span className={`source ${price?.source ?? "estimated"}`} title={price?.blsItem ?? undefined}>
                      {sourceLabel(price?.source ?? "estimated", region)}
                    </span>
                  </td>
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
