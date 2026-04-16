import { useEffect, useState } from "react";
import { getHistory } from "../api";

export default function History() {
  const [items, setItems] = useState([]);

  useEffect(() => {
    getHistory().then(setItems);
  }, []);

  return (
    <div>
      <h2>History</h2>
      {items.map(x => (
        <div key={x.id}>
          <b>{x.topic}</b> ({x.platform}) - {x.tone}
        </div>
      ))}
    </div>
  );
}