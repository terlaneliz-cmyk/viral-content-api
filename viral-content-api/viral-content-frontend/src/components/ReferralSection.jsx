function ReferralSection({
  referralInfo,
  referralTrialThreshold,
  referralSignupCount,
  referralProgressPercent,
  referralsRemainingForTrial,
  hasEarnedReferralTrial,
  referralLoading,
  referralMessage,
  referralCodeInput,
  setReferralCodeInput,
  handleGenerateReferralCode,
  handleCopyReferralLink,
  referralLeaderboard,
  myUserId,
  handleApplyReferralCode,
  sectionStyle,
  cardStyle,
  buttonBaseStyle,
  inputStyle
}) {
  return (
    <div style={sectionStyle}>
      <h2>Referrals</h2>

      <div
        style={{
          display: "grid",
          gridTemplateColumns: "repeat(auto-fit, minmax(240px, 1fr))",
          gap: 12,
          marginBottom: 16
        }}
      >
        <div style={cardStyle}>
          <div style={{ opacity: 0.8, fontSize: 12 }}>YOUR CODE</div>
          <div style={{ fontSize: 22, fontWeight: 700, marginTop: 6 }}>
            {referralInfo?.referralCode || "—"}
          </div>
        </div>

        <div style={cardStyle}>
          <div style={{ opacity: 0.8, fontSize: 12 }}>SHARES</div>
          <div style={{ fontSize: 22, fontWeight: 700, marginTop: 6 }}>
            {referralInfo?.referralInviteCount ?? 0}
          </div>
        </div>

        <div style={cardStyle}>
          <div style={{ opacity: 0.8, fontSize: 12 }}>SIGNUPS</div>
          <div style={{ fontSize: 22, fontWeight: 700, marginTop: 6 }}>
            {referralInfo?.referralSignupCount ?? 0}
          </div>
        </div>
      </div>

      <div
        style={{
          border: "1px solid #7c3aed",
          borderRadius: 12,
          padding: 14,
          background: "linear-gradient(135deg, #2a1639 0%, #111827 100%)",
          marginBottom: 14
        }}
      >
        <h3 style={{ marginTop: 0 }}>🎁 Free Pro trial reward</h3>
        <div style={{ opacity: 0.92, marginBottom: 10 }}>
          Invite {referralTrialThreshold} successful signups to unlock a free Pro trial.
        </div>

        <div
          style={{
            width: "100%",
            height: 12,
            borderRadius: 999,
            background: "#0f172a",
            overflow: "hidden",
            border: "1px solid #334155",
            marginBottom: 10
          }}
        >
          <div
            style={{
              width: `${referralProgressPercent}%`,
              height: "100%",
              background: hasEarnedReferralTrial ? "#22c55e" : "#a855f7"
            }}
          />
        </div>

        <div style={{ fontWeight: 700 }}>
          {hasEarnedReferralTrial
            ? "Unlocked: free Pro trial earned"
            : `${referralSignupCount}/${referralTrialThreshold} signups complete`}
        </div>

        {!hasEarnedReferralTrial && (
          <div style={{ opacity: 0.85, marginTop: 6 }}>
            {referralsRemainingForTrial} more referral{referralsRemainingForTrial === 1 ? "" : "s"} needed
          </div>
        )}
      </div>

      <div
        style={{
          border: "1px solid #334155",
          borderRadius: 12,
          padding: 14,
          background: "#0b1220",
          marginBottom: 14
        }}
      >
        <h3 style={{ marginTop: 0 }}>Invite people</h3>
        <div style={{ display: "grid", gap: 10 }}>
          <div style={{ wordBreak: "break-all", opacity: 0.9 }}>
            {referralInfo?.referralLink || "Generate a referral code to get your invite link."}
          </div>

          <div style={{ display: "flex", gap: 10, flexWrap: "wrap" }}>
            <button
              onClick={handleGenerateReferralCode}
              disabled={!!referralLoading}
              style={{
                ...buttonBaseStyle,
                opacity: referralLoading ? 0.7 : 1
              }}
            >
              {referralLoading === "generate" ? "Preparing..." : "Generate / refresh code"}
            </button>

            <button
              onClick={handleCopyReferralLink}
              disabled={!referralInfo?.referralLink}
              style={{
                ...buttonBaseStyle,
                background: "#1d4ed8",
                borderColor: "#1d4ed8",
                opacity: referralInfo?.referralLink ? 1 : 0.7,
                cursor: referralInfo?.referralLink ? "pointer" : "not-allowed"
              }}
            >
              Copy referral link
            </button>
          </div>
        </div>
      </div>

      <div
        style={{
          border: "1px solid #334155",
          borderRadius: 12,
          padding: 14,
          background: "#0b1220",
          marginBottom: 14
        }}
      >
        <h3 style={{ marginTop: 0 }}>Leaderboard</h3>
        <div style={{ display: "grid", gap: 8 }}>
          {referralLeaderboard.length === 0 && <div>No referral leaderboard data yet.</div>}
          {referralLeaderboard.map((entry, index) => {
            const isMine = Number(entry.userId) === Number(myUserId);
            const background =
              entry.rank === 1
                ? "#78350f"
                : entry.rank === 2
                  ? "#374151"
                  : entry.rank === 3
                    ? "#3f3f46"
                    : isMine
                      ? "#1e293b"
                      : "#111";

            const border = isMine ? "1px solid #22c55e" : "1px solid #222";

            return (
              <div
                key={`${entry.userId}-${index}`}
                style={{
                  padding: 10,
                  borderRadius: 8,
                  background,
                  border
                }}
              >
                <div style={{ display: "flex", justifyContent: "space-between", gap: 12, flexWrap: "wrap" }}>
                  <div style={{ fontWeight: 700 }}>
                    #{entry.rank ?? index + 1} — {entry.email || entry.username || `User ${entry.userId}`}
                    {isMine ? " (You)" : ""}
                  </div>
                  <div style={{ fontWeight: 700 }}>
                    {entry.referrals ?? entry.referralSignupCount ?? 0}
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      </div>

      <div
        style={{
          border: "1px solid #334155",
          borderRadius: 12,
          padding: 14,
          background: "#0b1220"
        }}
      >
        <h3 style={{ marginTop: 0 }}>Apply referral code</h3>
        <div style={{ display: "grid", gap: 10 }}>
          <input
            placeholder="Enter referral code"
            value={referralCodeInput}
            onChange={(e) => setReferralCodeInput(e.target.value)}
            style={inputStyle}
          />

          <div style={{ display: "flex", gap: 10, flexWrap: "wrap" }}>
            <button
              onClick={handleApplyReferralCode}
              disabled={!!referralLoading}
              style={{
                ...buttonBaseStyle,
                background: "#7c3aed",
                borderColor: "#7c3aed",
                opacity: referralLoading ? 0.7 : 1
              }}
            >
              {referralLoading === "apply" ? "Applying..." : "Apply code"}
            </button>
          </div>

          {referralInfo?.referredByEmail && (
            <div style={{ opacity: 0.85 }}>
              Referred by: {referralInfo.referredByEmail}
            </div>
          )}

          {referralMessage && (
            <div
              style={{
                padding: 12,
                borderRadius: 8,
                border: "1px solid #475569",
                background: "#0f172a"
              }}
            >
              {referralMessage}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default ReferralSection;