function MyPlanSection({
  myPlanMessage,
  myPlan,
  remainingToday,
  planLimit,
  usageUsed,
  usagePercent,
  referralSignupCount,
  referralTrialThreshold,
  referralsRemainingForTrial,
  referralProgressPercent,
  hasEarnedReferralTrial,
  referralTrialDaysLeft,
  currentPlanName,
  openUpgradeModal,
  subscriptionDetails,
  getStatusBadgeStyle,
  getRenewalLabel,
  formatUtcDate,
  billingErrorMessage,
  billingSuccessMessage,
  isFreePlan,
  canCancelAtPeriodEnd,
  canReactivate,
  subscriptionActionLoading,
  handleCancelAtPeriodEnd,
  handleReactivateSubscription,
  openBillingPortal,
  sectionStyle,
  buttonBaseStyle,
  planCardStyle
}) {
  const currentStreak = Number(myPlan?.currentStreak ?? 0);
  const bestStreak = Number(myPlan?.bestStreak ?? 0);

  const streakRewardText =
    currentStreak >= 14
      ? "+2 daily generations active"
      : currentStreak >= 7
        ? "+1 daily generation active"
        : currentStreak >= 3
          ? "3-day streak achieved"
          : "Keep generating daily to unlock streak rewards";

  const streakRewardColor =
    currentStreak >= 14
      ? "#a855f7"
      : currentStreak >= 7
        ? "#60a5fa"
        : currentStreak >= 3
          ? "#22c55e"
          : "#94a3b8";

  return (
    <div style={sectionStyle}>
      <h2>My Plan</h2>
      {myPlanMessage && <p>{myPlanMessage}</p>}

      {myPlan && (
        <div style={planCardStyle}>
          <div
            style={{
              display: "flex",
              justifyContent: "space-between",
              gap: 16,
              alignItems: "center",
              flexWrap: "wrap",
              marginBottom: 12
            }}
          >
            <div>
              <p style={{ margin: 0, fontSize: 12, opacity: 0.8 }}>CURRENT PLAN</p>
              <h3 style={{ margin: "4px 0 0 0" }}>{myPlan.planName}</h3>
            </div>
            <div>
              <p style={{ margin: 0 }}><strong>Remaining today:</strong> {remainingToday}</p>
              <p style={{ margin: "4px 0 0 0" }}><strong>Daily limit:</strong> {planLimit}</p>
            </div>
          </div>

          <div style={{ marginBottom: 8 }}>
            <div
              style={{
                width: "100%",
                height: 12,
                borderRadius: 999,
                background: "#0f172a",
                overflow: "hidden",
                border: "1px solid #334155"
              }}
            >
              <div
                style={{
                  width: `${usagePercent}%`,
                  height: "100%",
                  background:
                    usagePercent >= 100
                      ? "#dc2626"
                      : usagePercent >= 70
                        ? "#f59e0b"
                        : "#22c55e",
                  transition: "width 0.3s ease"
                }}
              />
            </div>
          </div>

          <p style={{ marginTop: 0, opacity: 0.85 }}>
            Used {usageUsed} of {planLimit} daily generations
          </p>

          {myPlan?.isReferralTrialActive && (
            <div
              style={{
                marginTop: 10,
                padding: "12px",
                borderRadius: 10,
                background: "#1e293b",
                border: "1px solid #3b82f6",
                color: "#60a5fa",
                fontWeight: 600
              }}
            >
              🎉 Free Pro trial active
              <div style={{ fontSize: 13, opacity: 0.8, marginTop: 4 }}>
                Ends in {referralTrialDaysLeft} day{referralTrialDaysLeft === 1 ? "" : "s"}
              </div>
            </div>
          )}

          {myPlan?.referralBonus > 0 && (
            <div
              style={{
                marginTop: 10,
                padding: "10px 12px",
                borderRadius: 10,
                border: "1px solid #15803d",
                background: "linear-gradient(135deg, #052e16 0%, #14532d 100%)",
                color: "#dcfce7",
                fontWeight: 600,
                boxShadow: "0 6px 20px rgba(0,0,0,0.2)"
              }}
            >
              🚀 Referral bonus active: +{myPlan.referralBonus} daily generations
            </div>
          )}

          <div
            style={{
              display: "grid",
              gridTemplateColumns: "repeat(auto-fit, minmax(220px, 1fr))",
              gap: 12,
              marginTop: 14,
              marginBottom: 14
            }}
          >
            <div
              style={{
                border: "1px solid #334155",
                borderRadius: 10,
                padding: 14,
                background: "#0b1220"
              }}
            >
              <div style={{ fontSize: 12, opacity: 0.75, marginBottom: 6 }}>CURRENT STREAK</div>
              <div style={{ fontSize: 26, fontWeight: 700 }}>🔥 {currentStreak}</div>
              <div style={{ opacity: 0.9, marginTop: 6, color: streakRewardColor }}>
                {currentStreak >= 14 && "🚀 +2 daily generations active"}
                {currentStreak >= 7 && currentStreak < 14 && "⚡ +1 daily generation active"}
                {currentStreak >= 3 && currentStreak < 7 && "🔥 3-day streak achieved"}
                {currentStreak < 3 && "Generate daily to unlock streak rewards"}
              </div>
            </div>

            <div
              style={{
                border: "1px solid #334155",
                borderRadius: 10,
                padding: 14,
                background: "#0b1220"
              }}
            >
              <div style={{ fontSize: 12, opacity: 0.75, marginBottom: 6 }}>BEST STREAK</div>
              <div style={{ fontSize: 26, fontWeight: 700 }}>🏆 {bestStreak}</div>
              <div style={{ opacity: 0.8, marginTop: 6 }}>
                Longest run of active generation days
              </div>
            </div>

            <div
              style={{
                border: "1px solid #7c3aed",
                borderRadius: 10,
                padding: 14,
                background: "linear-gradient(135deg, #2a1639 0%, #111827 100%)"
              }}
            >
              <div style={{ fontSize: 12, opacity: 0.75, marginBottom: 6 }}>REFERRAL TRIAL PROGRESS</div>
              <div style={{ fontSize: 26, fontWeight: 700 }}>
                🎁 {Math.min(referralSignupCount, referralTrialThreshold)}/{referralTrialThreshold}
              </div>
              <div style={{ opacity: 0.9, marginTop: 6 }}>
                {hasEarnedReferralTrial
                  ? "Free Pro trial unlocked"
                  : referralsRemainingForTrial === 0
                    ? "Reward should unlock now"
                    : `${referralsRemainingForTrial} more referral${referralsRemainingForTrial === 1 ? "" : "s"} to unlock free Pro`}
              </div>
              <div
                style={{
                  width: "100%",
                  height: 10,
                  borderRadius: 999,
                  background: "#111827",
                  overflow: "hidden",
                  border: "1px solid #334155",
                  marginTop: 10
                }}
              >
                <div
                  style={{
                    width: `${referralProgressPercent}%`,
                    height: "100%",
                    background: hasEarnedReferralTrial ? "#22c55e" : "#a855f7",
                    transition: "width 0.3s ease"
                  }}
                />
              </div>
            </div>
          </div>

          <div
            style={{
              marginBottom: 14,
              padding: 14,
              borderRadius: 10,
              border: "1px solid #334155",
              background: "#0b1220"
            }}
          >
            <div style={{ fontSize: 12, opacity: 0.75, marginBottom: 8 }}>STREAK REWARD STATUS</div>
            <div style={{ fontWeight: 700, color: streakRewardColor }}>{streakRewardText}</div>
            <div style={{ marginTop: 8, opacity: 0.85 }}>
              3 days: badge · 7 days: +1 daily generation · 14 days: +2 daily generations
            </div>
          </div>

          <div
            style={{
              display: "grid",
              gap: 10,
              marginTop: 14
            }}
          >
            <div
              style={{
                display: "flex",
                gap: 10,
                flexWrap: "wrap"
              }}
            >
              <div
                style={{
                  padding: "10px 12px",
                  borderRadius: 8,
                  border: "1px solid #475569",
                  background: "#0f172a"
                }}
              >
                {myPlan?.planName?.toLowerCase().includes("free") && "Free plan active"}
                {myPlan?.planName?.toLowerCase().includes("pro") && "Pro features unlocked"}
                {myPlan?.planName?.toLowerCase().includes("creator") && "Creator features unlocked"}
                {!myPlan?.planName && "Plan active"}
              </div>

              <div
                style={{
                  padding: "10px 12px",
                  borderRadius: 8,
                  border: "1px solid #475569",
                  background: "#0f172a"
                }}
              >
                Available upgrades: {myPlan.availableUpgrades?.join(", ") || "None"}
              </div>

              {!currentPlanName.includes("creator") && (
                <button
                  onClick={openUpgradeModal}
                  style={{ ...buttonBaseStyle, background: "#1d4ed8", borderColor: "#1d4ed8" }}
                >
                  View upgrade options
                </button>
              )}
            </div>

            {subscriptionDetails && (
              <div
                style={{
                  border: "1px solid #334155",
                  borderRadius: 10,
                  padding: 14,
                  background: "#0b1220"
                }}
              >
                <div
                  style={{
                    display: "flex",
                    justifyContent: "space-between",
                    alignItems: "center",
                    gap: 12,
                    flexWrap: "wrap",
                    marginBottom: 12
                  }}
                >
                  <h4 style={{ margin: 0 }}>Subscription Billing</h4>
                  <span
                    style={{
                      ...getStatusBadgeStyle(subscriptionDetails.status),
                      padding: "6px 10px",
                      borderRadius: 999,
                      fontSize: 12,
                      fontWeight: 700,
                      textTransform: "uppercase",
                      letterSpacing: 0.4
                    }}
                  >
                    {subscriptionDetails.status || "unknown"}
                  </span>
                </div>

                <div style={{ display: "grid", gap: 8 }}>
                  <div><strong>Plan:</strong> {subscriptionDetails.planName || "—"}</div>
                  <div><strong>Billing cycle:</strong> {subscriptionDetails.billingCycle || "—"}</div>
                  <div><strong>Price:</strong> {subscriptionDetails.price ?? 0} {subscriptionDetails.currency || ""}</div>
                  <div><strong>Started:</strong> {formatUtcDate(subscriptionDetails.activatedAtUtc || subscriptionDetails.createdAtUtc)}</div>
                  {getRenewalLabel() && (
                    <div>
                      <strong>{getRenewalLabel()}:</strong> {formatUtcDate(subscriptionDetails.currentPeriodEndUtc)}
                    </div>
                  )}
                  <div><strong>Cancel at period end:</strong> {subscriptionDetails.cancelAtPeriodEnd ? "Yes" : "No"}</div>
                  {subscriptionDetails.cancelRequestedAtUtc && (
                    <div><strong>Cancel requested:</strong> {formatUtcDate(subscriptionDetails.cancelRequestedAtUtc)}</div>
                  )}
                </div>

                {billingErrorMessage && (
                  <div
                    style={{
                      marginTop: 14,
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
                      marginTop: 14,
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

                <div style={{ display: "flex", gap: 10, flexWrap: "wrap", marginTop: 14 }}>
                  {!isFreePlan && (
                    <button
                      onClick={openBillingPortal}
                      style={{
                        ...buttonBaseStyle,
                        background: "#1e293b",
                        borderColor: "#475569"
                      }}
                    >
                      Manage billing
                    </button>
                  )}

                  {canCancelAtPeriodEnd && (
                    <button
                      onClick={handleCancelAtPeriodEnd}
                      disabled={subscriptionActionLoading === "cancel"}
                      style={{
                        ...buttonBaseStyle,
                        background: "#7f1d1d",
                        borderColor: "#991b1b",
                        opacity: subscriptionActionLoading === "cancel" ? 0.7 : 1,
                        cursor: subscriptionActionLoading === "cancel" ? "not-allowed" : "pointer"
                      }}
                    >
                      {subscriptionActionLoading === "cancel" ? "Canceling..." : "Cancel at period end"}
                    </button>
                  )}

                  {canReactivate && (
                    <button
                      onClick={handleReactivateSubscription}
                      disabled={subscriptionActionLoading === "reactivate"}
                      style={{
                        ...buttonBaseStyle,
                        background: "#14532d",
                        borderColor: "#166534",
                        opacity: subscriptionActionLoading === "reactivate" ? 0.7 : 1,
                        cursor: subscriptionActionLoading === "reactivate" ? "not-allowed" : "pointer"
                      }}
                    >
                      {subscriptionActionLoading === "reactivate" ? "Reactivating..." : "Reactivate"}
                    </button>
                  )}

                  {isFreePlan && (
                    <div
                      style={{
                        padding: "10px 12px",
                        borderRadius: 8,
                        border: "1px solid #475569",
                        background: "#111827",
                        opacity: 0.9
                      }}
                    >
                      Billing controls are available for paid plans only.
                    </div>
                  )}
                </div>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}

export default MyPlanSection;