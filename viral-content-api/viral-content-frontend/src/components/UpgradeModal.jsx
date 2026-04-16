function UpgradeModal({
  showUpgradeModal,
  closeUpgradeModal,
  startingUpgradePlan,
  billingErrorMessage,
  billingSuccessMessage,
  plans,
  currentPlanName,
  recommendedPlanKey,
  normalizePlanName,
  handleStartUpgradeFlow,
  buttonBaseStyle
}) {
  if (!showUpgradeModal) return null;

  return (
    <div
      onClick={closeUpgradeModal}
      style={{
        position: "fixed",
        inset: 0,
        background: "rgba(0, 0, 0, 0.7)",
        zIndex: 2000,
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        padding: 20
      }}
    >
      <div
        onClick={(e) => e.stopPropagation()}
        style={{
          width: "100%",
          maxWidth: 980,
          maxHeight: "90vh",
          overflowY: "auto",
          background: "#0f172a",
          border: "1px solid #334155",
          borderRadius: 16,
          padding: 20,
          boxShadow: "0 20px 60px rgba(0,0,0,0.4)"
        }}
      >
        <div
          style={{
            display: "flex",
            justifyContent: "space-between",
            gap: 12,
            alignItems: "center",
            marginBottom: 20,
            flexWrap: "wrap"
          }}
        >
          <div>
            <h2 style={{ margin: 0 }}>Choose your plan</h2>
            <p style={{ margin: "6px 0 0 0", opacity: 0.8 }}>
              Upgrade to unlock more daily generations and smoother workflow.
            </p>
          </div>

          <button
            onClick={closeUpgradeModal}
            style={{
              ...buttonBaseStyle,
              opacity: startingUpgradePlan ? 0.7 : 1,
              cursor: startingUpgradePlan ? "not-allowed" : "pointer"
            }}
            disabled={!!startingUpgradePlan}
          >
            Close
          </button>
        </div>

        {billingErrorMessage && (
          <div
            style={{
              marginBottom: 16,
              padding: "12px 14px",
              borderRadius: 10,
              border: "1px solid #991b1b",
              background: "#7f1d1d",
              color: "white"
            }}
          >
            {billingErrorMessage}
          </div>
        )}

        {billingSuccessMessage && (
          <div
            style={{
              marginBottom: 16,
              padding: "12px 14px",
              borderRadius: 10,
              border: "1px solid #166534",
              background: "#14532d",
              color: "white"
            }}
          >
            {billingSuccessMessage}
          </div>
        )}

        <div
          style={{
            display: "grid",
            gridTemplateColumns: "repeat(auto-fit, minmax(240px, 1fr))",
            gap: 16
          }}
        >
          {plans.map((plan) => {
            const isCurrent = currentPlanName.includes(plan.key);
            const isRecommended = recommendedPlanKey === plan.key;
            const isStartingThisPlan = normalizePlanName(startingUpgradePlan) === plan.key;
            const isAnyUpgradeStarting = !!startingUpgradePlan;
            const isSelectable = plan.key !== "free";

            return (
              <div
                key={plan.key}
                style={{
                  position: "relative",
                  border: isCurrent
                    ? `2px solid ${plan.accent}`
                    : isRecommended
                      ? `2px solid ${plan.accent}`
                      : "1px solid #334155",
                  borderRadius: 14,
                  padding: 18,
                  background: isRecommended
                    ? `linear-gradient(135deg, ${plan.accent}22 0%, #111827 100%)`
                    : "#111827"
                }}
              >
                <div
                  style={{
                    display: "flex",
                    gap: 8,
                    flexWrap: "wrap",
                    marginBottom: 12
                  }}
                >
                  {isCurrent && (
                    <span
                      style={{
                        fontSize: 12,
                        padding: "4px 8px",
                        borderRadius: 999,
                        background: "#14532d",
                        border: "1px solid #166534"
                      }}
                    >
                      Current plan
                    </span>
                  )}

                  {isRecommended && !isCurrent && (
                    <span
                      style={{
                        fontSize: 12,
                        padding: "4px 8px",
                        borderRadius: 999,
                        background: `${plan.accent}33`,
                        border: `1px solid ${plan.accent}`
                      }}
                    >
                      Recommended
                    </span>
                  )}
                </div>

                <h3 style={{ marginTop: 0, marginBottom: 4 }}>{plan.name}</h3>
                <div style={{ fontSize: 28, fontWeight: 700, marginBottom: 6 }}>{plan.price}</div>
                <div style={{ opacity: 0.8, marginBottom: 12 }}>{plan.subtitle}</div>

                <div
                  style={{
                    padding: "10px 12px",
                    borderRadius: 8,
                    background: "#0f172a",
                    border: "1px solid #334155",
                    marginBottom: 12
                  }}
                >
                  <strong>{plan.limit}</strong>
                </div>

                <div style={{ display: "grid", gap: 8, marginBottom: 16 }}>
                  {plan.features.map((feature) => (
                    <div key={feature} style={{ opacity: 0.9 }}>
                      • {feature}
                    </div>
                  ))}
                </div>

                <button
                  onClick={() => isSelectable && handleStartUpgradeFlow(plan.name)}
                  disabled={isCurrent || isAnyUpgradeStarting || !isSelectable}
                  style={{
                    ...buttonBaseStyle,
                    width: "100%",
                    background: isCurrent ? "#334155" : !isSelectable ? "#334155" : plan.accent,
                    borderColor: !isSelectable ? "#475569" : plan.accent,
                    opacity: isCurrent || isAnyUpgradeStarting || !isSelectable ? 0.7 : 1,
                    cursor: isCurrent || isAnyUpgradeStarting || !isSelectable ? "not-allowed" : "pointer"
                  }}
                >
                  {isCurrent
                    ? "Current plan"
                    : !isSelectable
                      ? "Included"
                      : isStartingThisPlan
                        ? "Starting checkout..."
                        : `Start ${plan.name}`}
                </button>
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
}

export default UpgradeModal;