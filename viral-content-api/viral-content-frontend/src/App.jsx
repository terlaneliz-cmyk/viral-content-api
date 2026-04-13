import { useEffect, useState } from "react";

const API_BASE = "https://viral-content-api-mcnr.onrender.com";

function App() {
  const [token, setToken] = useState(localStorage.getItem("token") || "");

  const [loginData, setLoginData] = useState({ email: "", password: "" });
  const [loginMessage, setLoginMessage] = useState("");

  const [myPlan, setMyPlan] = useState(null);
  const [myPlanMessage, setMyPlanMessage] = useState("");

  const [generatorData, setGeneratorData] = useState({
    topic: "",
    platform: "TikTok",
    tone: "bold",
    goal: "engagement",
    contentType: "post",
    targetAudience: "",
    numberOfVariants: 1
  });

  const [result, setResult] = useState(null);
  const [generatorMessage, setGeneratorMessage] = useState("");
  const [loading, setLoading] = useState(false);

  const loadMyPlan = async () => {
    if (!token) {
      setMyPlan(null);
      setMyPlanMessage("Not logged in.");
      return;
    }

    setMyPlanMessage("Loading your plan...");

    try {
      const res = await fetch(`${API_BASE}/api/Plans/my-plan`, {
        headers: {
          Authorization: `Bearer ${token}`
        }
      });

      const text = await res.text();
      const data = text ? JSON.parse(text) : null;

      if (!res.ok) {
        setMyPlan(null);
        setMyPlanMessage(data?.message || "Failed to load plan.");
        return;
      }

      setMyPlan(data);
      setMyPlanMessage("");
    } catch {
      setMyPlan(null);
      setMyPlanMessage("Failed to load plan.");
    }
  };

  useEffect(() => {
    if (token) {
      loadMyPlan();
    } else {
      setMyPlan(null);
      setMyPlanMessage("Not logged in.");
    }
  }, [token]);

  const handleLogin = async (e) => {
    e.preventDefault();
    setLoginMessage("Logging in...");

    try {
      const res = await fetch(`${API_BASE}/api/Auth/login`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json"
        },
        body: JSON.stringify(loginData)
      });

      const text = await res.text();
      const data = text ? JSON.parse(text) : null;

      if (!res.ok) {
        setLoginMessage(data?.message || "Login failed.");
        return;
      }

      localStorage.setItem("token", data.token);
      setToken(data.token);
      setLoginMessage("Logged in.");
      setLoginData({ email: "", password: "" });
    } catch {
      setLoginMessage("Login failed.");
    }
  };

  const handleLogout = () => {
    localStorage.removeItem("token");
    setToken("");
    setResult(null);
    setGeneratorMessage("");
    setLoginMessage("Logged out.");
  };

  const handleGenerate = async () => {
    setLoading(true);
    setResult(null);
    setGeneratorMessage("Generating...");

    try {
      const res = await fetch(`${API_BASE}/api/AiContent/generate`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`
        },
        body: JSON.stringify(generatorData)
      });

      const rawText = await res.text();

let data = null;

try {
  data = JSON.parse(rawText);
} catch {
  // backend returned plain text → show it directly
  setGeneratorMessage(`Backend response: ${rawText}`);
  setLoading(false);
  return;
}

      if (res.status === 429) {
        setGeneratorMessage(
          typeof data === "string"
            ? data
            : data?.message || "Daily limit reached. Upgrade your plan."
        );
        setLoading(false);
        return;
      }

      if (!res.ok) {
        if (typeof data === "string") {
          setGeneratorMessage(`Generation failed: ${data}`);
        } else {
          setGeneratorMessage(
            data?.message ||
              data?.title ||
              JSON.stringify(data) ||
              "Generation failed."
          );
        }
        setLoading(false);
        return;
      }

      if (!data || !data.variants || !Array.isArray(data.variants)) {
        setGeneratorMessage("Generation returned an unexpected response.");
        setLoading(false);
        return;
      }

      setResult(data);
      setGeneratorMessage("Generation successful.");
      loadMyPlan();
    } catch (error) {
      setGeneratorMessage(`Error generating content: ${error.message}`);
    }

    setLoading(false);
  };

  return (
    <div style={{ padding: 20, maxWidth: 1000, margin: "0 auto", color: "white" }}>
      <h1>🚀 Viral Content Generator</h1>

      {!token && (
        <div style={{ marginBottom: 24, padding: 16, border: "1px solid #334155", borderRadius: 8 }}>
          <h2>Login</h2>
          <div style={{ display: "grid", gap: 10 }}>
            <input
              placeholder="Email"
              value={loginData.email}
              onChange={(e) => setLoginData({ ...loginData, email: e.target.value })}
              style={{ padding: 10 }}
            />
            <input
              placeholder="Password"
              type="password"
              value={loginData.password}
              onChange={(e) => setLoginData({ ...loginData, password: e.target.value })}
              style={{ padding: 10 }}
            />
            <button onClick={handleLogin}>Login</button>
            {loginMessage && <p>{loginMessage}</p>}
          </div>
        </div>
      )}

      {token && (
        <>
          <div style={{ marginBottom: 24, padding: 16, border: "1px solid #334155", borderRadius: 8 }}>
            <h2>Auth</h2>
            <p>Logged in.</p>
            <button onClick={handleLogout}>Logout</button>
          </div>

          <div style={{ marginBottom: 24, padding: 16, border: "1px solid #334155", borderRadius: 8 }}>
            <h2>My Plan</h2>
            {myPlanMessage && <p>{myPlanMessage}</p>}
            {myPlan && (
              <div>
                <p><strong>Plan:</strong> {myPlan.planName}</p>
                <p><strong>Remaining today:</strong> {myPlan.remainingToday}</p>
                <p><strong>Available upgrades:</strong> {myPlan.availableUpgrades?.join(", ") || "None"}</p>
              </div>
            )}
          </div>

          <div style={{ marginBottom: 24, padding: 16, border: "1px solid #334155", borderRadius: 8 }}>
            <h2>Generate Content</h2>

            <div style={{ display: "grid", gap: 10 }}>
              <input
                placeholder="Topic"
                value={generatorData.topic}
                onChange={(e) => setGeneratorData({ ...generatorData, topic: e.target.value })}
                style={{ padding: 10 }}
              />

              <input
                placeholder="Target Audience"
                value={generatorData.targetAudience}
                onChange={(e) => setGeneratorData({ ...generatorData, targetAudience: e.target.value })}
                style={{ padding: 10 }}
              />

              <select
                value={generatorData.platform}
                onChange={(e) => setGeneratorData({ ...generatorData, platform: e.target.value })}
                style={{ padding: 10 }}
              >
                <option value="TikTok">TikTok</option>
                <option value="Instagram">Instagram</option>
                <option value="LinkedIn">LinkedIn</option>
                <option value="YouTube">YouTube</option>
                <option value="X">X</option>
                <option value="Twitter">Twitter</option>
                <option value="Facebook">Facebook</option>
              </select>

              <select
                value={generatorData.tone}
                onChange={(e) => setGeneratorData({ ...generatorData, tone: e.target.value })}
                style={{ padding: 10 }}
              >
                <option value="bold">bold</option>
                <option value="professional">professional</option>
                <option value="friendly">friendly</option>
                <option value="confident">confident</option>
                <option value="inspirational">inspirational</option>
              </select>

              <select
                value={generatorData.goal}
                onChange={(e) => setGeneratorData({ ...generatorData, goal: e.target.value })}
                style={{ padding: 10 }}
              >
                <option value="engagement">engagement</option>
                <option value="followers">followers</option>
                <option value="leads">leads</option>
                <option value="authority">authority</option>
              </select>

              <select
                value={generatorData.contentType}
                onChange={(e) => setGeneratorData({ ...generatorData, contentType: e.target.value })}
                style={{ padding: 10 }}
              >
                <option value="post">post</option>
                <option value="caption">caption</option>
                <option value="script">script</option>
                <option value="thread">thread</option>
                <option value="video idea">video idea</option>
              </select>

              <input
                type="number"
                min="1"
                max="10"
                value={generatorData.numberOfVariants}
                onChange={(e) =>
                  setGeneratorData({
                    ...generatorData,
                    numberOfVariants: Number(e.target.value)
                  })
                }
                style={{ padding: 10 }}
              />

              <button onClick={handleGenerate} disabled={loading}>
                {loading ? "Generating..." : "Generate"}
              </button>

              {generatorMessage && (
                <div style={{ padding: 12, border: "1px solid #475569", borderRadius: 8 }}>
                  <strong>Status:</strong> {generatorMessage}
                </div>
              )}
            </div>
          </div>

          {result && (
            <div style={{ marginTop: 20 }}>
              <h3>Results</h3>

              {result.variants.map((v) => (
                <div
                  key={v.variantNumber}
                  style={{ border: "1px solid #333", padding: 12, marginBottom: 12, borderRadius: 8 }}
                >
                  <h4>Variant {v.variantNumber + 1}</h4>
                  <p><strong>Hook:</strong> {v.hook}</p>
                  <p>{v.content}</p>
                  <p><strong>CTA:</strong> {v.callToAction}</p>
                  <p>{Array.isArray(v.hashtags) ? v.hashtags.join(" ") : ""}</p>
                </div>
              ))}
            </div>
          )}
        </>
      )}
    </div>
  );
}

export default App;