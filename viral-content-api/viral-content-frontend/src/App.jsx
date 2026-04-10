import { useEffect, useState } from "react";

const API_BASE = "https://viral-content-api-mcnr.onrender.com";

function App() {
  const [health, setHealth] = useState("Checking...");
  const [plans, setPlans] = useState([]);

  useEffect(() => {
    fetch(`${API_BASE}/health`)
      .then(res => res.text())
      .then(data => setHealth(data))
      .catch(() => setHealth("Error"));

    fetch(`${API_BASE}/api/Plans`)
      .then(res => res.json())
      .then(data => setPlans(data))
      .catch(() => setPlans([]));
  }, []);

  return (
    <div style={{ padding: "20px", fontFamily: "Arial" }}>
      <h1>🚀 Viral Content App</h1>

      <h2>Backend Status:</h2>
      <p>{health}</p>

      <h2>Plans:</h2>
      {plans.length === 0 ? (
        <p>Loading plans...</p>
      ) : (
        <ul>
          {plans.map((plan) => (
            <li key={plan.name}>
              <strong>{plan.displayName}</strong> - ${plan.monthlyPrice}/month
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}

export default App;