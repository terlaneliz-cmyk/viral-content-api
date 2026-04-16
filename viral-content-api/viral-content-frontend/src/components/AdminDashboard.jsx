function AdminDashboard({
  adminAnalytics,
  adminReferralStats,
  adminCharts,
  adminSearch,
  setAdminSearch,
  adminMessage,
  adminLoading,
  adminUsers,
  adminActionLoading,
  handleAdminAction,
  adminBillingLogs,
  adminWebhookLogs,
  webhookTypeFilter,
  setWebhookTypeFilter,
  webhookProcessedFilter,
  setWebhookProcessedFilter,
  handleRetryWebhook,
  cardStyle,
  inputStyle,
  buttonBaseStyle,
  sectionStyle,
  formatUtcDate,
  API_BASE
}) {
  const topReferrers = Array.isArray(adminReferralStats?.top) ? adminReferralStats.top : [];
  const totalShares = Number(adminReferralStats?.totalInvites ?? 0);
  const totalSignups = Number(adminReferralStats?.totalSignups ?? 0);
  const rewardUsers = Number(adminReferralStats?.rewardUsers ?? 0);
  const totalUsers = Number(adminReferralStats?.totalUsers ?? 0);

  const conversionRate =
    totalShares > 0 ? ((totalSignups / totalShares) * 100).toFixed(1) : "0.0";

  const rewardClaimRate =
    totalUsers > 0 ? ((rewardUsers / totalUsers) * 100).toFixed(1) : "0.0";

  return (
    <div style={sectionStyle}>
      <h2>Admin Dashboard</h2>

      {adminAnalytics && (
        <div style={{ display: "flex", gap: 12, flexWrap: "wrap", marginBottom: 16 }}>
          <div style={cardStyle}>Total Users: {adminAnalytics.totalUsers ?? 0}</div>
          <div style={cardStyle}>Free: {adminAnalytics.free ?? 0}</div>
          <div style={cardStyle}>Pro: {adminAnalytics.pro ?? 0}</div>
          <div style={cardStyle}>Creator: {adminAnalytics.creator ?? 0}</div>
          <div style={cardStyle}>Today Gen: {adminAnalytics.todayGenerations ?? 0}</div>
          <div style={cardStyle}>MRR: ${adminAnalytics.mrr ?? 0}</div>
          <div style={cardStyle}>Churn Signals: {adminAnalytics.churnSignals ?? 0}</div>
        </div>
      )}

      {adminReferralStats && (
        <div
          style={{
            border: "1px solid #334155",
            borderRadius: 12,
            padding: 14,
            background: "#0b1220",
            marginBottom: 24
          }}
        >
          <h3 style={{ marginTop: 0 }}>Referral Funnel</h3>

          <div
            style={{
              display: "grid",
              gridTemplateColumns: "repeat(auto-fit, minmax(180px, 1fr))",
              gap: 12,
              marginBottom: 16
            }}
          >
            <div style={cardStyle}>Users: {totalUsers}</div>
            <div style={cardStyle}>Shares: {totalShares}</div>
            <div style={cardStyle}>Signups: {totalSignups}</div>
            <div style={cardStyle}>Reward Users: {rewardUsers}</div>
            <div style={cardStyle}>Conversion: {conversionRate}%</div>
            <div style={cardStyle}>Reward Claim Rate: {rewardClaimRate}%</div>
          </div>

          <div>
            <h4 style={{ marginTop: 0 }}>Top Referrers</h4>
            <div style={{ display: "grid", gap: 8 }}>
              {topReferrers.length === 0 && <div>No referral data yet.</div>}
              {topReferrers.map((item, index) => {
                const shares = Number(item.referralInviteCount ?? 0);
                const signups = Number(item.referralSignupCount ?? 0);
                const ratio = shares > 0 ? ((signups / shares) * 100).toFixed(1) : "0.0";

                return (
                  <div
                    key={`${item.email}-${index}`}
                    style={{
                      border: "1px solid #334155",
                      borderRadius: 8,
                      padding: 12,
                      background: "#0f172a"
                    }}
                  >
                    <div style={{ fontWeight: 700, marginBottom: 6 }}>
                      #{index + 1} {item.email}
                    </div>
                    <div style={{ display: "flex", gap: 16, flexWrap: "wrap", opacity: 0.9 }}>
                      <div>Shares: {shares}</div>
                      <div>Signups: {signups}</div>
                      <div>Share → Signup: {ratio}%</div>
                    </div>
                  </div>
                );
              })}
            </div>
          </div>
        </div>
      )}

      {adminCharts && (
        <div style={{ marginBottom: 24 }}>
          <h3>Charts</h3>

          <div style={{ display: "grid", gap: 16 }}>
            <div style={{ border: "1px solid #334155", borderRadius: 12, padding: 14 }}>
              <h4>Users by Plan</h4>
              {adminCharts.usersByPlan?.map((x) => (
                <div key={x.plan} style={{ marginBottom: 8 }}>
                  <div style={{ display: "flex", justifyContent: "space-between" }}>
                    <span>{x.plan}</span>
                    <span>{x.count}</span>
                  </div>
                  <div
                    style={{
                      height: 8,
                      background: "#0f172a",
                      borderRadius: 6,
                      overflow: "hidden"
                    }}
                  >
                    <div
                      style={{
                        width: `${Math.min((x.count || 0) * 5, 100)}%`,
                        height: "100%",
                        background: "#22c55e"
                      }}
                    />
                  </div>
                </div>
              ))}
            </div>

            <div style={{ border: "1px solid #334155", borderRadius: 12, padding: 14 }}>
              <h4>Top Users by Usage</h4>
              {adminCharts.topUsersByUsage?.length === 0 && <div>No usage yet.</div>}
              {adminCharts.topUsersByUsage?.map((x) => (
                <div key={x.userId} style={{ marginBottom: 8 }}>
                  User {x.userId} — {x.count} generations
                </div>
              ))}
            </div>
          </div>
        </div>
      )}

      <div style={{ display: "grid", gap: 10, marginBottom: 16 }}>
        <input
          placeholder="Search users..."
          value={adminSearch}
          onChange={(e) => setAdminSearch(e.target.value)}
          style={inputStyle}
        />
        {adminMessage && <div>{adminMessage}</div>}
      </div>

      <div style={{ display: "grid", gap: 12, marginBottom: 24 }}>
        {adminLoading && <div>Loading admin users...</div>}

        {!adminLoading && adminUsers.map((u) => (
          <div key={u.id} style={{ border: "1px solid #334155", borderRadius: 12, padding: 14, background: "#0b1220" }}>
            <div style={{ display: "grid", gap: 6 }}>
              <div><strong>{u.email}</strong> ({u.username})</div>
              <div>Plan: {u.plan}</div>
              <div>Role: {u.role}</div>
              <div>Status: {u.isActive ? "Active" : "Disabled"}</div>
              <div>Created: {formatUtcDate(u.createdAt)}</div>
              <div>Usage total: {u.dailyUsage ?? 0}</div>
              <div>
                Subscription: {u.subscription?.planName || "—"} / {u.subscription?.status || "—"}
                {u.subscription?.cancelAtPeriodEnd ? " / Canceling" : ""}
              </div>
            </div>

            <div style={{ display: "flex", gap: 8, flexWrap: "wrap", marginTop: 12 }}>
              <button
                onClick={() =>
                  handleAdminAction(
                    `set-pro-${u.id}`,
                    `${API_BASE}/api/admin/set-plan?userId=${u.id}&plan=Pro`,
                    { method: "POST" }
                  )
                }
                disabled={!!adminActionLoading}
                style={{ ...buttonBaseStyle, opacity: adminActionLoading ? 0.7 : 1 }}
              >
                Set Pro
              </button>

              <button
                onClick={() =>
                  handleAdminAction(
                    `set-creator-${u.id}`,
                    `${API_BASE}/api/admin/set-plan?userId=${u.id}&plan=Creator`,
                    { method: "POST" }
                  )
                }
                disabled={!!adminActionLoading}
                style={{ ...buttonBaseStyle, opacity: adminActionLoading ? 0.7 : 1 }}
              >
                Set Creator
              </button>

              <button
                onClick={() =>
                  handleAdminAction(
                    `set-free-${u.id}`,
                    `${API_BASE}/api/admin/set-plan?userId=${u.id}&plan=Free`,
                    { method: "POST" }
                  )
                }
                disabled={!!adminActionLoading}
                style={{ ...buttonBaseStyle, opacity: adminActionLoading ? 0.7 : 1 }}
              >
                Set Free
              </button>

              <button
                onClick={() =>
                  handleAdminAction(
                    `reset-usage-${u.id}`,
                    `${API_BASE}/api/admin/reset-usage?userId=${u.id}`,
                    { method: "POST" }
                  )
                }
                disabled={!!adminActionLoading}
                style={{ ...buttonBaseStyle, opacity: adminActionLoading ? 0.7 : 1 }}
              >
                Reset Usage
              </button>

              <button
                onClick={() =>
                  handleAdminAction(
                    `deactivate-${u.id}`,
                    `${API_BASE}/api/admin/deactivate?userId=${u.id}`,
                    { method: "POST" }
                  )
                }
                disabled={!!adminActionLoading}
                style={{
                  ...buttonBaseStyle,
                  background: "#7f1d1d",
                  borderColor: "#991b1b",
                  opacity: adminActionLoading ? 0.7 : 1
                }}
              >
                Deactivate
              </button>
            </div>
          </div>
        ))}
      </div>

      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 16 }}>
        <div style={{ border: "1px solid #334155", borderRadius: 12, padding: 14, background: "#0b1220" }}>
          <h3 style={{ marginTop: 0 }}>Billing Audit Trail</h3>
          <div style={{ display: "grid", gap: 8, maxHeight: 320, overflowY: "auto" }}>
            {adminBillingLogs.length === 0 && <div>No billing logs yet.</div>}
            {adminBillingLogs.map((log) => (
              <div key={log.id} style={{ border: "1px solid #334155", borderRadius: 8, padding: 10 }}>
                <div><strong>{log.eventType || "unknown"}</strong></div>
                <div>User: {log.userId}</div>
                <div>{log.metadata || log.message || "—"}</div>
                <div style={{ opacity: 0.8 }}>{formatUtcDate(log.createdAtUtc)}</div>
              </div>
            ))}
          </div>
        </div>

        <div style={{ border: "1px solid #334155", borderRadius: 12, padding: 14, background: "#0b1220" }}>
          <h3 style={{ marginTop: 0 }}>Webhook Logs</h3>

          <div style={{ display: "grid", gap: 8, marginBottom: 12 }}>
            <input
              placeholder="Filter by event type..."
              value={webhookTypeFilter}
              onChange={(e) => setWebhookTypeFilter(e.target.value)}
              style={inputStyle}
            />
            <select
              value={webhookProcessedFilter}
              onChange={(e) => setWebhookProcessedFilter(e.target.value)}
              style={inputStyle}
            >
              <option value="all">All</option>
              <option value="true">Processed only</option>
              <option value="false">Unprocessed only</option>
            </select>
          </div>

          <div style={{ display: "grid", gap: 8, maxHeight: 320, overflowY: "auto" }}>
            {adminWebhookLogs.length === 0 && <div>No webhook logs yet.</div>}
            {adminWebhookLogs.map((log) => (
              <div key={log.id} style={{ border: "1px solid #334155", borderRadius: 8, padding: 10 }}>
                <div><strong>{log.eventType || "unknown"}</strong></div>
                <div>EventId: {log.eventId || "—"}</div>
                <div>Processed: {String(log.processed)}</div>
                <div>Success: {String(log.success)}</div>
                <div>{log.message || "—"}</div>
                <div style={{ opacity: 0.8, marginBottom: 8 }}>{formatUtcDate(log.createdAtUtc)}</div>
                <button
                  onClick={() => handleRetryWebhook(log.id)}
                  disabled={!!adminActionLoading}
                  style={{
                    ...buttonBaseStyle,
                    opacity: adminActionLoading ? 0.7 : 1
                  }}
                >
                  Mark retry
                </button>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}

export default AdminDashboard;