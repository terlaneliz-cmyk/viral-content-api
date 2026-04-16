function GeneratorSection({
  sectionStyle,
  generatorData,
  setGeneratorData,
  inputStyle,
  handleGenerate,
  loading,
  isLimitReached,
  buttonBaseStyle,
  generatorMessage
}) {
  return (
    <div style={sectionStyle}>
      <h2>Generate Content</h2>

      <div style={{ display: "grid", gap: 10 }}>
        <input
          placeholder="Topic"
          value={generatorData.topic}
          onChange={(e) => setGeneratorData({ ...generatorData, topic: e.target.value })}
          style={inputStyle}
        />

        <input
          placeholder="Target Audience"
          value={generatorData.targetAudience}
          onChange={(e) => setGeneratorData({ ...generatorData, targetAudience: e.target.value })}
          style={inputStyle}
        />

        <select
          value={generatorData.platform}
          onChange={(e) => setGeneratorData({ ...generatorData, platform: e.target.value })}
          style={inputStyle}
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
          style={inputStyle}
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
          style={inputStyle}
        >
          <option value="engagement">engagement</option>
          <option value="followers">followers</option>
          <option value="leads">leads</option>
          <option value="authority">authority</option>
        </select>

        <select
          value={generatorData.contentType}
          onChange={(e) => setGeneratorData({ ...generatorData, contentType: e.target.value })}
          style={inputStyle}
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
          style={inputStyle}
        />

        <button
          onClick={handleGenerate}
          disabled={loading || isLimitReached}
          style={{
            ...buttonBaseStyle,
            opacity: loading || isLimitReached ? 0.7 : 1,
            cursor: loading || isLimitReached ? "not-allowed" : "pointer",
            background: loading || isLimitReached ? "#334155" : "#1d4ed8"
          }}
        >
          {loading ? "Generating..." : isLimitReached ? "Limit reached" : "Generate"}
        </button>

        {generatorMessage && (
          <div
            style={{
              padding: 12,
              border: "1px solid #475569",
              borderRadius: 8,
              background: "#0f172a"
            }}
          >
            <strong>Status:</strong> {generatorMessage}
          </div>
        )}
      </div>
    </div>
  );
}

export default GeneratorSection;