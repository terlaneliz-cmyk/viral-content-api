function LatestResultSection({
  sectionStyle,
  result,
  copyAllLatestVariants,
  handleUseLatestResultAsTemplate,
  exportLatestResultToTxt,
  clearLatestResult,
  buttonBaseStyle,
  getVariantCharacterCount,
  copyVariantText
}) {
  return (
    <div style={sectionStyle}>
      <h2>Latest Result</h2>

      {!result && (
        <div
          style={{
            border: "1px dashed #475569",
            borderRadius: 8,
            padding: 20,
            textAlign: "center",
            background: "#0f172a"
          }}
        >
          No latest result yet.
        </div>
      )}

      {result && (
        <>
          <div style={{ display: "flex", gap: 10, flexWrap: "wrap", marginBottom: 16 }}>
            <button onClick={copyAllLatestVariants} style={buttonBaseStyle}>
              Copy all variants
            </button>

            <button onClick={handleUseLatestResultAsTemplate} style={buttonBaseStyle}>
              Use latest result as template
            </button>

            <button onClick={exportLatestResultToTxt} style={buttonBaseStyle}>
              Export latest result to txt
            </button>

            <button onClick={clearLatestResult} style={buttonBaseStyle}>
              Clear latest result
            </button>
          </div>

          {result.variants.map((v) => (
            <div
              key={v.variantNumber}
              style={{
                border: "1px solid #333",
                padding: 12,
                marginBottom: 12,
                borderRadius: 8,
                background: "#111827"
              }}
            >
              <div
                style={{
                  display: "flex",
                  justifyContent: "space-between",
                  gap: 12,
                  alignItems: "center",
                  flexWrap: "wrap"
                }}
              >
                <h4 style={{ margin: 0 }}>Variant {v.variantNumber}</h4>
                <span style={{ fontSize: 12, opacity: 0.8 }}>
                  {getVariantCharacterCount(v, false)} chars
                </span>
              </div>
              <p><strong>Hook:</strong> {v.hook}</p>
              <p style={{ whiteSpace: "pre-wrap" }}>{v.content}</p>
              <p><strong>CTA:</strong> {v.callToAction}</p>
              <p>{Array.isArray(v.hashtags) ? v.hashtags.join(" ") : ""}</p>
              <button onClick={() => copyVariantText(v, false)} style={buttonBaseStyle}>
                Copy
              </button>
            </div>
          ))}
        </>
      )}
    </div>
  );
}

export default LatestResultSection;