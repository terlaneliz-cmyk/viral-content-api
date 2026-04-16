function Landing({ onStart, openUpgrade }) {
  return (
    <div
      style={{
        border: "1px solid #334155",
        borderRadius: 16,
        padding: 28,
        marginBottom: 24,
        background: "linear-gradient(135deg, #0f172a 0%, #111827 100%)"
      }}
    >
      <div style={{ display: "grid", gap: 24 }}>
        <div style={{ textAlign: "center" }}>
          <div
            style={{
              display: "inline-block",
              padding: "6px 12px",
              borderRadius: 999,
              border: "1px solid #2563eb",
              background: "#172554",
              marginBottom: 14,
              fontSize: 13,
              fontWeight: 700
            }}
          >
            Generate faster. Post smarter. Grow quicker.
          </div>
          <h1 style={{ fontSize: 42, margin: "0 0 10px 0" }}>🚀 Go Viral Faster</h1>
          <p style={{ margin: 0, opacity: 0.9, fontSize: 18 }}>
            Create high-performing social content in seconds.
          </p>
        </div>

        <div
          style={{
            display: "grid",
            gridTemplateColumns: "repeat(auto-fit, minmax(220px, 1fr))",
            gap: 12
          }}
        >
          <div
            style={{
              padding: 16,
              borderRadius: 12,
              border: "1px solid #334155",
              background: "#0b1220"
            }}
          >
            <h3 style={{ marginTop: 0 }}>Free</h3>
            <div style={{ fontSize: 28, fontWeight: 700 }}>$0</div>
            <p>3 daily generations</p>
            <p style={{ opacity: 0.8 }}>Try the product and build your workflow.</p>
          </div>

          <div
            style={{
              padding: 16,
              borderRadius: 12,
              border: "1px solid #2563eb",
              background: "linear-gradient(135deg, #172554 0%, #111827 100%)"
            }}
          >
            <div
              style={{
                display: "inline-block",
                marginBottom: 8,
                padding: "4px 8px",
                borderRadius: 999,
                background: "#1d4ed8",
                fontSize: 12,
                fontWeight: 700
              }}
            >
              MOST POPULAR
            </div>
            <h3 style={{ marginTop: 0 }}>Pro</h3>
            <div style={{ fontSize: 28, fontWeight: 700 }}>$19/mo</div>
            <p>20 daily generations</p>
            <p style={{ opacity: 0.8 }}>Best for consistent creators and posting cadence.</p>
          </div>

          <div
            style={{
              padding: 16,
              borderRadius: 12,
              border: "1px solid #a855f7",
              background: "linear-gradient(135deg, #2a1639 0%, #111827 100%)"
            }}
          >
            <h3 style={{ marginTop: 0 }}>Creator</h3>
            <div style={{ fontSize: 28, fontWeight: 700 }}>$49/mo</div>
            <p>100 daily generations</p>
            <p style={{ opacity: 0.8 }}>For heavy usage, scale, and serious output volume.</p>
          </div>
        </div>

        <div
          style={{
            display: "grid",
            gridTemplateColumns: "repeat(auto-fit, minmax(220px, 1fr))",
            gap: 12
          }}
        >
          {[
            "Save time on ideation and captions",
            "Generate faster when you post daily",
            "Reuse winning templates from history",
            "Upgrade only when you actually need more volume"
          ].map((item) => (
            <div
              key={item}
              style={{
                padding: 14,
                borderRadius: 12,
                border: "1px solid #334155",
                background: "#0b1220"
              }}
            >
              ✅ {item}
            </div>
          ))}
        </div>

        <div
          style={{
            border: "1px solid #334155",
            borderRadius: 14,
            padding: 18,
            background: "#0b1220"
          }}
        >
          <h3 style={{ marginTop: 0 }}>What users like</h3>
          <div
            style={{
              display: "grid",
              gridTemplateColumns: "repeat(auto-fit, minmax(240px, 1fr))",
              gap: 12
            }}
          >
            <div style={{ border: "1px solid #334155", borderRadius: 12, padding: 14 }}>
              <div style={{ fontWeight: 700, marginBottom: 8 }}>“Way faster content workflow.”</div>
              <div style={{ opacity: 0.85 }}>
                I stopped staring at blank screens. I can batch ideas much faster now.
              </div>
            </div>
            <div style={{ border: "1px solid #334155", borderRadius: 12, padding: 14 }}>
              <div style={{ fontWeight: 700, marginBottom: 8 }}>“The history + regenerate combo is strong.”</div>
              <div style={{ opacity: 0.85 }}>
                I can quickly revisit old ideas and get fresh variants without starting over.
              </div>
            </div>
            <div style={{ border: "1px solid #334155", borderRadius: 12, padding: 14 }}>
              <div style={{ fontWeight: 700, marginBottom: 8 }}>“Worth it once you post consistently.”</div>
              <div style={{ opacity: 0.85 }}>
                The upgrade made sense as soon as content became part of my routine.
              </div>
            </div>
          </div>
        </div>

        <div
          style={{
            border: "1px solid #7c3aed",
            borderRadius: 14,
            padding: 18,
            background: "linear-gradient(135deg, #1e1b4b 0%, #111827 100%)",
            textAlign: "center"
          }}
        >
          <h3 style={{ marginTop: 0 }}>Start free. Upgrade when volume matters.</h3>
          <p style={{ marginTop: 0, opacity: 0.9 }}>
            Free gets you started. Pro and Creator unlock faster growth when you need more output.
          </p>
          <div style={{ display: "flex", justifyContent: "center", gap: 12, flexWrap: "wrap" }}>
            <button
              onClick={onStart}
              style={{
                padding: "12px 20px",
                fontSize: 18,
                background: "#1d4ed8",
                color: "white",
                border: "none",
                borderRadius: 10,
                cursor: "pointer"
              }}
            >
              Get Started
            </button>

            <button
              onClick={openUpgrade}
              style={{
                padding: "12px 20px",
                fontSize: 18,
                background: "#7c3aed",
                color: "white",
                border: "none",
                borderRadius: 10,
                cursor: "pointer"
              }}
            >
              See Paid Plans
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}

export default Landing;