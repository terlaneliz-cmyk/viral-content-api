function AuthSection({
  registerData,
  setRegisterData,
  registerLoading,
  registerMessage,
  handleRegister,
  loginData,
  setLoginData,
  loginLoading,
  loginMessage,
  handleLogin,
  inputStyle,
  buttonBaseStyle
}) {
  return (
    <>
      <div
        style={{
          marginBottom: 24,
          padding: 16,
          border: "1px solid #334155",
          borderRadius: 8,
          background: "#111827"
        }}
      >
        <h2>Register</h2>
        <div style={{ display: "grid", gap: 10 }}>
          <input
            placeholder="Email"
            type="email"
            value={registerData.email}
            onChange={(e) => setRegisterData({ ...registerData, email: e.target.value })}
            style={inputStyle}
          />
          <input
            placeholder="Password"
            type="password"
            value={registerData.password}
            onChange={(e) => setRegisterData({ ...registerData, password: e.target.value })}
            style={inputStyle}
          />
          <input
            placeholder="Confirm Password"
            type="password"
            value={registerData.confirmPassword}
            onChange={(e) => setRegisterData({ ...registerData, confirmPassword: e.target.value })}
            style={inputStyle}
          />

          {!!localStorage.getItem("referralCode") && (
            <div
              style={{
                padding: 12,
                borderRadius: 8,
                border: "1px solid #7c3aed",
                background: "#2a1639",
                color: "#e9d5ff",
                fontWeight: 600
              }}
            >
              Referral code detected: {localStorage.getItem("referralCode")}
            </div>
          )}

          <div style={{ fontSize: 13, opacity: 0.8 }}>
            Password must be 8+ chars and include uppercase, lowercase, and a number.
          </div>
          <button
            onClick={handleRegister}
            disabled={registerLoading}
            style={{
              ...buttonBaseStyle,
              opacity: registerLoading ? 0.7 : 1,
              cursor: registerLoading ? "not-allowed" : "pointer"
            }}
          >
            {registerLoading ? "Creating account..." : "Register"}
          </button>
          {registerMessage && <p>{registerMessage}</p>}
        </div>
      </div>

      <div
        style={{
          marginBottom: 24,
          padding: 16,
          border: "1px solid #334155",
          borderRadius: 8,
          background: "#111827"
        }}
      >
        <h2>Login</h2>
        <div style={{ display: "grid", gap: 10 }}>
          <input
            placeholder="Email"
            type="email"
            value={loginData.email}
            onChange={(e) => setLoginData({ ...loginData, email: e.target.value })}
            style={inputStyle}
          />
          <input
            placeholder="Password"
            type="password"
            value={loginData.password}
            onChange={(e) => setLoginData({ ...loginData, password: e.target.value })}
            style={inputStyle}
          />
          <button
            onClick={handleLogin}
            disabled={loginLoading}
            style={{
              ...buttonBaseStyle,
              opacity: loginLoading ? 0.7 : 1,
              cursor: loginLoading ? "not-allowed" : "pointer"
            }}
          >
            {loginLoading ? "Logging in..." : "Login"}
          </button>
          {loginMessage && <p>{loginMessage}</p>}
        </div>
      </div>
    </>
  );
}

export default AuthSection;