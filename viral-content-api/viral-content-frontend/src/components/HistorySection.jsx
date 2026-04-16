function HistorySection({
  sectionStyle,
  historySearch,
  setHistorySearch,
  historySort,
  setHistorySort,
  inputStyle,
  historyMessage,
  historyItems,
  filteredAndSortedHistoryItems,
  selectedHistoryItem,
  setSelectedHistoryItem,
  regeneratingId,
  deletingId,
  isLimitReached,
  handleRegenerate,
  handleUseAsTemplate,
  handleDeleteHistoryItem,
  buttonBaseStyle,
  renderHistoryContent
}) {
  return (
    <div style={sectionStyle}>
      <h2>History</h2>

      <div style={{ display: "grid", gap: 10, marginBottom: 16 }}>
        <input
          placeholder="Search by topic / platform / tone"
          value={historySearch}
          onChange={(e) => setHistorySearch(e.target.value)}
          style={inputStyle}
        />

        <select
          value={historySort}
          onChange={(e) => setHistorySort(e.target.value)}
          style={{ ...inputStyle, maxWidth: 220 }}
        >
          <option value="newest">Sort: newest first</option>
          <option value="oldest">Sort: oldest first</option>
        </select>
      </div>

      {historyMessage && <p>{historyMessage}</p>}

      {historyItems.length === 0 && (
        <div
          style={{
            border: "1px dashed #475569",
            borderRadius: 8,
            padding: 20,
            textAlign: "center",
            background: "#0f172a"
          }}
        >
          No history yet.
        </div>
      )}

      {filteredAndSortedHistoryItems.length > 0 && (
        <div style={{ display: "grid", gap: 10 }}>
          {filteredAndSortedHistoryItems.map((item) => {
            const isRegenerating = regeneratingId === item.id;
            const isDeleting = deletingId === item.id;
            const isBusy = isRegenerating || isDeleting || isLimitReached;

            return (
              <div
                key={item.id}
                style={{
                  padding: 12,
                  borderRadius: 8,
                  border: "1px solid #475569",
                  background: selectedHistoryItem?.id === item.id ? "#1e293b" : "#0f172a",
                  color: "white"
                }}
              >
                <button
                  onClick={() => setSelectedHistoryItem(item)}
                  style={{
                    width: "100%",
                    textAlign: "left",
                    background: "transparent",
                    border: "none",
                    color: "white",
                    cursor: "pointer",
                    padding: 0,
                    marginBottom: 12
                  }}
                >
                  <div><strong>{item.topic}</strong></div>
                  <div>{item.platform} / {item.tone}</div>
                  <div>{new Date(item.createdAt).toLocaleString()}</div>
                </button>

                <div style={{ display: "flex", gap: 8, flexWrap: "wrap" }}>
                  <button
                    onClick={() => handleRegenerate(item.id)}
                    disabled={isBusy}
                    style={{
                      ...buttonBaseStyle,
                      opacity: isBusy ? 0.7 : 1,
                      cursor: isBusy ? "not-allowed" : "pointer"
                    }}
                  >
                    {isRegenerating ? "Regenerating..." : isLimitReached ? "Limit reached" : "Regenerate"}
                  </button>

                  <button
                    onClick={() => handleUseAsTemplate(item)}
                    disabled={isDeleting || isRegenerating}
                    style={{
                      ...buttonBaseStyle,
                      opacity: isDeleting || isRegenerating ? 0.7 : 1,
                      cursor: isDeleting || isRegenerating ? "not-allowed" : "pointer"
                    }}
                  >
                    Use as template
                  </button>

                  <button
                    onClick={() => handleDeleteHistoryItem(item.id)}
                    disabled={isDeleting || isRegenerating}
                    style={{
                      ...buttonBaseStyle,
                      opacity: isDeleting || isRegenerating ? 0.7 : 1,
                      cursor: isDeleting || isRegenerating ? "not-allowed" : "pointer"
                    }}
                  >
                    {isDeleting ? "Deleting..." : "Delete"}
                  </button>
                </div>
              </div>
            );
          })}
        </div>
      )}

      {filteredAndSortedHistoryItems.length === 0 && historyItems.length > 0 && (
        <div
          style={{
            border: "1px dashed #475569",
            borderRadius: 8,
            padding: 20,
            textAlign: "center",
            background: "#0f172a"
          }}
        >
          No history items match your search.
        </div>
      )}

      {selectedHistoryItem && (
        <div style={{ marginTop: 20 }}>
          <h3 style={{ wordBreak: "break-word" }}>{selectedHistoryItem.topic}</h3>

          <div style={{ display: "flex", gap: 10, flexWrap: "wrap", marginBottom: 16 }}>
            <button
              onClick={() => handleRegenerate(selectedHistoryItem.id)}
              disabled={regeneratingId === selectedHistoryItem.id || deletingId === selectedHistoryItem.id || isLimitReached}
              style={{
                ...buttonBaseStyle,
                background: "#1d4ed8",
                opacity:
                  regeneratingId === selectedHistoryItem.id ||
                  deletingId === selectedHistoryItem.id ||
                  isLimitReached
                    ? 0.7
                    : 1,
                cursor:
                  regeneratingId === selectedHistoryItem.id ||
                  deletingId === selectedHistoryItem.id ||
                  isLimitReached
                    ? "not-allowed"
                    : "pointer"
              }}
            >
              {regeneratingId === selectedHistoryItem.id ? "Regenerating..." : isLimitReached ? "Limit reached" : "Regenerate"}
            </button>

            <button
              onClick={() => handleUseAsTemplate(selectedHistoryItem)}
              disabled={regeneratingId === selectedHistoryItem.id || deletingId === selectedHistoryItem.id}
              style={{
                ...buttonBaseStyle,
                opacity:
                  regeneratingId === selectedHistoryItem.id || deletingId === selectedHistoryItem.id ? 0.7 : 1,
                cursor:
                  regeneratingId === selectedHistoryItem.id || deletingId === selectedHistoryItem.id
                    ? "not-allowed"
                    : "pointer"
              }}
            >
              Use as template
            </button>

            <button
              onClick={() => handleDeleteHistoryItem(selectedHistoryItem.id)}
              disabled={deletingId === selectedHistoryItem.id || regeneratingId === selectedHistoryItem.id}
              style={{
                ...buttonBaseStyle,
                opacity:
                  regeneratingId === selectedHistoryItem.id || deletingId === selectedHistoryItem.id ? 0.7 : 1,
                cursor:
                  regeneratingId === selectedHistoryItem.id || deletingId === selectedHistoryItem.id
                    ? "not-allowed"
                    : "pointer"
              }}
            >
              {deletingId === selectedHistoryItem.id ? "Deleting..." : "Delete"}
            </button>
          </div>

          {renderHistoryContent(selectedHistoryItem)}
        </div>
      )}
    </div>
  );
}

export default HistorySection;