import { useCallback, useEffect, useState } from "react";

// Small helper: runs an async loader on mount and exposes loading/error state.
export function useLoad(loader) {
  const [data, setData] = useState(null);
  const [error, setError] = useState("");

  const reload = useCallback(async () => {
    try {
      setError("");
      setData(await loader());
    } catch (e) {
      setError(`Couldn't reach the API. Is the backend running? (${e.message})`);
    }
  }, [loader]);

  useEffect(() => {
    reload();
  }, [reload]);

  return { data, setData, error, reload };
}
