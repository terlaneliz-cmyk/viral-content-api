import { useEffect, useMemo, useState } from "react";
import Landing from "./components/Landing";
import UpgradeModal from "./components/UpgradeModal";
import AuthSection from "./components/AuthSection";
import MyPlanSection from "./components/MyPlanSection";
import ReferralSection from "./components/ReferralSection";
import GeneratorSection from "./components/GeneratorSection";
import LatestResultSection from "./components/LatestResultSection";
import HistorySection from "./components/HistorySection";
import AdminDashboard from "./components/AdminDashboard";

const API_BASE = "";

function App() {
  const [token, setToken] = useState(localStorage.getItem("token") || "");
  const [showLanding, setShowLanding] = useState(!localStorage.getItem("token"));

  const [loginData, setLoginData] = useState({ email: "", password: "" });
  const [registerData, setRegisterData] = useState({
    email: "",
    password: "",
    confirmPassword: ""
  });

  const [loginMessage, setLoginMessage] = useState("");
  const [registerMessage, setRegisterMessage] = useState("");
  const [loginLoading, setLoginLoading] = useState(false);
  const [registerLoading, setRegisterLoading] = useState(false);

  const [myPlan, setMyPlan] = useState(null);
  const [myPlanMessage, setMyPlanMessage] = useState("");
  const [subscriptionDetails, setSubscriptionDetails] = useState(null);
  const [billingErrorMessage, setBillingErrorMessage] = useState("");
  const [billingSuccessMessage, setBillingSuccessMessage] = useState("");

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

  const [historyItems, setHistoryItems] = useState([]);
  const [historyMessage, setHistoryMessage] = useState("");
  const [selectedHistoryItem, setSelectedHistoryItem] = useState(null);

  const [regeneratingId, setRegeneratingId] = useState(null);
  const [deletingId, setDeletingId] = useState(null);

  const [historySearch, setHistorySearch] = useState("");
  const [historySort, setHistorySort] = useState("newest");

  const [toast, setToast] = useState(null);
  const [showUpgradeModal, setShowUpgradeModal] = useState(false);
  const [startingUpgradePlan, setStartingUpgradePlan] = useState("");
  const [subscriptionActionLoading, setSubscriptionActionLoading] = useState("");

  const [isAdmin, setIsAdmin] = useState(false);
  const [adminUsers, setAdminUsers] = useState([]);
  const [adminAnalytics, setAdminAnalytics] = useState(null);
  const [adminCharts, setAdminCharts] = useState(null);
  const [adminSearch, setAdminSearch] = useState("");
  const [adminLoading, setAdminLoading] = useState(false);
  const [adminActionLoading, setAdminActionLoading] = useState("");
  const [adminMessage, setAdminMessage] = useState("");
  const [adminBillingLogs, setAdminBillingLogs] = useState([]);
  const [adminWebhookLogs, setAdminWebhookLogs] = useState([]);
  const [webhookTypeFilter, setWebhookTypeFilter] = useState("");
  const [webhookProcessedFilter, setWebhookProcessedFilter] = useState("all");
  const [adminReferralStats, setAdminReferralStats] = useState(null);

  const [referralInfo, setReferralInfo] = useState(null);
  const [referralCodeInput, setReferralCodeInput] = useState("");
  const [referralLoading, setReferralLoading] = useState("");
  const [referralMessage, setReferralMessage] = useState("");
  const [referralLeaderboard, setReferralLeaderboard] = useState([]);
  const [myUserId, setMyUserId] = useState(null);

  useEffect(() => {
    if (!toast) return;

    const timeout = setTimeout(() => {
      setToast(null);
    }, 3000);

    return () => clearTimeout(timeout);
  }, [toast]);

  useEffect(() => {
    const params = new URLSearchParams(window.location.search);
    const ref = params.get("ref");

    if (ref) {
      const normalized = String(ref).trim().toUpperCase();
      localStorage.setItem("referralCode", normalized);
      setReferralCodeInput(normalized);
    } else {
      setReferralCodeInput(localStorage.getItem("referralCode") || "");
    }
  }, []);

  useEffect(() => {
    if (!token) {
      setMyUserId(null);
      return;
    }

    try {
      const payload = JSON.parse(atob(token.split(".")[1]));
      const rawUserId =
        payload?.nameid ||
        payload?.sub ||
        payload?.["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"];

      const parsedUserId = Number(rawUserId);
      setMyUserId(Number.isFinite(parsedUserId) ? parsedUserId : null);
    } catch {
      setMyUserId(null);
    }
  }, [token]);

  const showToast = (message, type = "info") => {
    setToast({ message, type });
  };

  const normalizePlanName = (name) => String(name || "").trim().toLowerCase();

  const isValidEmail = (email) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(String(email || "").trim());

  const isStrongPassword = (password) => {
    const value = String(password || "");
    if (value.length < 8) return false;
    if (!/[A-Z]/.test(value)) return false;
    if (!/[a-z]/.test(value)) return false;
    if (!/[0-9]/.test(value)) return false;
    return true;
  };

  const getApiMessage = (data, fallbackMessage) => {
    if (!data) return fallbackMessage;
    if (typeof data === "string" && data.trim()) return data;
    if (typeof data?.message === "string" && data.message.trim()) return data.message;
    if (typeof data?.title === "string" && data.title.trim()) return data.title;
    if (Array.isArray(data?.errors) && data.errors.length > 0) {
      const firstError = data.errors.find((x) => typeof x === "string" && x.trim());
      if (firstError) return firstError;
    }
    return fallbackMessage;
  };

  const clearBillingMessages = () => {
    setBillingErrorMessage("");
    setBillingSuccessMessage("");
  };

  const handleAuthExpired = (message = "Session expired. Please log in again.") => {
    localStorage.removeItem("token");
    setToken("");
    setShowLanding(false);
    setResult(null);
    setGeneratorMessage("");
    setHistoryItems([]);
    setSelectedHistoryItem(null);
    setMyPlan(null);
    setSubscriptionDetails(null);
    setShowUpgradeModal(false);
    setStartingUpgradePlan("");
    setSubscriptionActionLoading("");
    setLoginLoading(false);
    setRegisterLoading(false);
    setIsAdmin(false);
    setAdminUsers([]);
    setAdminAnalytics(null);
    setAdminCharts(null);
    setAdminBillingLogs([]);
    setAdminWebhookLogs([]);
    setAdminReferralStats(null);
    setReferralInfo(null);
    setReferralCodeInput(localStorage.getItem("referralCode") || "");
    setReferralLoading("");
    setReferralMessage("");
    setReferralLeaderboard([]);
    clearBillingMessages();
    setLoginMessage(message);
    showToast(message, "error");
  };

  const fetchJson = async (
    url,
    options = {},
    { auth = false, fallbackMessage = "Request failed.", logoutOn401 = true } = {}
  ) => {
    const headers = {
      ...(options.headers || {})
    };

    if (auth && token) {
      headers.Authorization = `Bearer ${token}`;
    }

    const response = await fetch(url, {
      ...options,
      headers
    });

    let data = null;
    try {
      data = await response.json();
    } catch {
      data = null;
    }

    if (!response.ok) {
      const message = getApiMessage(data, fallbackMessage);

      if (auth && response.status === 401 && logoutOn401) {
        handleAuthExpired(message);
      }

      return {
        ok: false,
        status: response.status,
        data,
        message
      };
    }

    return {
      ok: true,
      status: response.status,
      data,
      message: ""
    };
  };

  const getPlanLimit = (plan) => {
    if (!plan) return 0;

    if (typeof plan.dailyLimit === "number") return plan.dailyLimit;
    if (typeof plan.maxDailyGenerations === "number") return plan.maxDailyGenerations;
    if (typeof plan.dailyGenerationLimit === "number") return plan.dailyGenerationLimit;
    if (typeof plan.dailyGenerateLimit === "number") return plan.dailyGenerateLimit;

    const planName = String(plan.planName || "").toLowerCase();

    if (planName.includes("creator")) return 100;
    if (planName.includes("agency")) return 100;
    if (planName.includes("pro")) return 20;
    if (planName.includes("free")) return 3;

    return Math.max(Number(plan.remainingToday || 0), 1);
  };

  const remainingToday = Number(myPlan?.remainingToday ?? 0);
  const planLimit = getPlanLimit(myPlan);
  const usageUsed = Math.max(planLimit - remainingToday, 0);
  const usagePercent = planLimit > 0 ? Math.min((usageUsed / planLimit) * 100, 100) : 0;
  const isLimitReached = token && myPlan && remainingToday <= 0;
  const currentPlanName = normalizePlanName(myPlan?.planName);
  const subscriptionStatus = normalizePlanName(subscriptionDetails?.status);
  const isFreePlan = currentPlanName === "free";

  const referralTrialThreshold = Number(referralInfo?.referralTrialThreshold ?? 3);
  const referralSignupCount = Number(referralInfo?.referralSignupCount ?? 0);
  const referralProgressPercent =
    referralTrialThreshold > 0 ? Math.min((referralSignupCount / referralTrialThreshold) * 100, 100) : 0;
  const referralsRemainingForTrial = Math.max(referralTrialThreshold - referralSignupCount, 0);
  const hasEarnedReferralTrial =
    Boolean(referralInfo?.hasEarnedReferralTrial) || Boolean(myPlan?.isReferralTrialActive);

  const referralTrialDaysLeft = myPlan?.referralTrialEndsAtUtc
    ? Math.max(
        0,
        Math.ceil(
          (new Date(myPlan.referralTrialEndsAtUtc).getTime() - new Date().getTime()) /
            (1000 * 60 * 60 * 24)
        )
      )
    : 0;

  const plans = [
    {
      key: "free",
      name: "Free",
      price: "$0",
      subtitle: "Good to try it",
      limit: "3 daily generations",
      features: ["Basic generation", "History access", "Manual workflow"],
      accent: "#475569"
    },
    {
      key: "pro",
      name: "Pro",
      price: "$19/mo",
      subtitle: "Best for most users",
      limit: "20 daily generations",
      features: ["Higher daily limits", "Faster repeat workflow", "Better for consistent posting"],
      accent: "#2563eb"
    },
    {
      key: "creator",
      name: "Creator",
      price: "$49/mo",
      subtitle: "For heavy usage",
      limit: "100 daily generations",
      features: ["Highest daily limits", "Best for power users", "Built for real volume"],
      accent: "#a855f7"
    }
  ];

  const recommendedPlanKey = currentPlanName.includes("free")
    ? "pro"
    : currentPlanName.includes("pro")
      ? "creator"
      : "creator";

  const canCancelAtPeriodEnd =
    !isFreePlan &&
    (subscriptionStatus === "active" ||
      subscriptionStatus === "trialing" ||
      subscriptionStatus === "past_due");

  const canReactivate = !isFreePlan && subscriptionStatus === "canceling";

  const openUpgradeModal = () => {
    clearBillingMessages();
    setShowUpgradeModal(true);
  };

  const closeUpgradeModal = () => {
    if (startingUpgradePlan) return;
    setShowUpgradeModal(false);
  };

  const loadMyPlan = async () => {
    if (!token) {
      setMyPlan(null);
      setMyPlanMessage("Not logged in.");
      return;
    }

    setMyPlanMessage("Loading your plan...");

    const res = await fetchJson(`${API_BASE}/api/Plans/my-plan`, {}, {
      auth: true,
      fallbackMessage: "Failed to load plan."
    });

    if (!res.ok) {
      setMyPlan(null);
      setMyPlanMessage(res.message);
      return;
    }

    setMyPlan(res.data);
    setMyPlanMessage("");
  };

  const loadSubscriptionDetails = async () => {
    if (!token) {
      setSubscriptionDetails(null);
      return;
    }

    const res = await fetchJson(`${API_BASE}/api/Plans/subscription`, {}, {
      auth: true,
      fallbackMessage: "Failed to load subscription."
    });

    if (!res.ok) {
      setSubscriptionDetails(null);
      return;
    }

    setSubscriptionDetails(res.data);
  };

  const loadHistory = async (preferredSelectedId = null) => {
    if (!token) {
      setHistoryItems([]);
      setSelectedHistoryItem(null);
      setHistoryMessage("Not logged in.");
      return [];
    }

    setHistoryMessage("Loading history...");

    const res = await fetchJson(`${API_BASE}/api/Ai/history`, {}, {
      auth: true,
      fallbackMessage: "Failed to load history."
    });

    if (!res.ok) {
      setHistoryItems([]);
      setSelectedHistoryItem(null);
      setHistoryMessage(res.message);
      return [];
    }

    const items = Array.isArray(res.data) ? res.data : [];

    setHistoryItems(items);
    setHistoryMessage(items.length === 0 ? "No history yet." : "");

    if (items.length > 0) {
      let nextSelected = null;

      if (preferredSelectedId) {
        nextSelected = items.find((x) => x.id === preferredSelectedId) || null;
      }

      if (!nextSelected && selectedHistoryItem?.id) {
        nextSelected = items.find((x) => x.id === selectedHistoryItem.id) || null;
      }

      if (!nextSelected) {
        nextSelected = items[0];
      }

      setSelectedHistoryItem(nextSelected);
    } else {
      setSelectedHistoryItem(null);
    }

    return items;
  };

  const loadAdminUsers = async () => {
    if (!token) return;

    setAdminLoading(true);
    const query = adminSearch.trim() ? `?search=${encodeURIComponent(adminSearch.trim())}` : "";

    const res = await fetchJson(`${API_BASE}/api/admin/users${query}`, {}, {
      auth: true,
      fallbackMessage: "Failed to load admin users.",
      logoutOn401: false
    });

    if (!res.ok) {
      if (res.status === 401) {
        handleAuthExpired(res.message);
      } else {
        setIsAdmin(false);
        setAdminUsers([]);
      }
      setAdminLoading(false);
      return;
    }

    setIsAdmin(true);
    setAdminUsers(Array.isArray(res.data) ? res.data : []);
    setAdminLoading(false);
  };

  const loadAdminAnalytics = async () => {
    if (!token) return;

    const res = await fetchJson(`${API_BASE}/api/admin/analytics`, {}, {
      auth: true,
      fallbackMessage: "Failed to load admin analytics.",
      logoutOn401: false
    });

    if (!res.ok) {
      if (res.status === 401) {
        handleAuthExpired(res.message);
      } else if (res.status === 403) {
        setAdminAnalytics(null);
      }
      return;
    }

    setAdminAnalytics(res.data || null);
    setIsAdmin(true);
  };

  const loadAdminCharts = async () => {
    if (!token) return;

    const res = await fetchJson(`${API_BASE}/api/admin/charts`, {}, {
      auth: true,
      fallbackMessage: "Failed to load charts.",
      logoutOn401: false
    });

    if (!res.ok) return;

    setAdminCharts(res.data || null);
  };

  const loadAdminBillingLogs = async () => {
    if (!token) return;

    const res = await fetchJson(`${API_BASE}/api/admin/billing-log`, {}, {
      auth: true,
      fallbackMessage: "Failed to load billing logs.",
      logoutOn401: false
    });

    if (!res.ok) {
      if (res.status === 401) {
        handleAuthExpired(res.message);
      } else if (res.status === 403) {
        setAdminBillingLogs([]);
      }
      return;
    }

    setAdminBillingLogs(Array.isArray(res.data) ? res.data : []);
    setIsAdmin(true);
  };

  const loadAdminWebhookLogs = async () => {
    if (!token) return;

    const params = new URLSearchParams();

    if (webhookTypeFilter.trim()) {
      params.set("type", webhookTypeFilter.trim());
    }

    if (webhookProcessedFilter === "true") {
      params.set("processed", "true");
    } else if (webhookProcessedFilter === "false") {
      params.set("processed", "false");
    }

    const query = params.toString() ? `?${params.toString()}` : "";

    const res = await fetchJson(`${API_BASE}/api/admin/webhooks${query}`, {}, {
      auth: true,
      fallbackMessage: "Failed to load webhook logs.",
      logoutOn401: false
    });

    if (!res.ok) {
      if (res.status === 401) {
        handleAuthExpired(res.message);
      } else if (res.status === 403) {
        setAdminWebhookLogs([]);
      }
      return;
    }

    setAdminWebhookLogs(Array.isArray(res.data) ? res.data : []);
    setIsAdmin(true);
  };

  const loadAdminReferralStats = async () => {
    if (!token) return;

    const res = await fetchJson(`${API_BASE}/api/referrals/admin/stats`, {}, {
      auth: true,
      fallbackMessage: "Failed to load referral stats.",
      logoutOn401: false
    });

    if (!res.ok) {
      if (res.status === 401) {
        handleAuthExpired(res.message);
      } else if (res.status === 403) {
        setAdminReferralStats(null);
      }
      return;
    }

    setAdminReferralStats(res.data || null);
    setIsAdmin(true);
  };

  const loadReferralInfo = async () => {
    if (!token) {
      setReferralInfo(null);
      return;
    }

    const res = await fetchJson(`${API_BASE}/api/referrals/me`, {}, {
      auth: true,
      fallbackMessage: "Failed to load referral info."
    });

    if (!res.ok) {
      setReferralInfo(null);
      return;
    }

    setReferralInfo(res.data || null);
  };

  const loadReferralLeaderboard = async () => {
    const res = await fetchJson(`${API_BASE}/api/referrals/leaderboard`, {}, {
      fallbackMessage: "Failed to load referral leaderboard."
    });

    if (!res.ok) {
      setReferralLeaderboard([]);
      return;
    }

    setReferralLeaderboard(Array.isArray(res.data) ? res.data : []);
  };

  const reloadAdmin = async () => {
    await loadAdminUsers();
    await loadAdminAnalytics();
    await loadAdminBillingLogs();
    await loadAdminWebhookLogs();
    await loadAdminCharts();
    await loadAdminReferralStats();
  };

  const refreshPaidState = async () => {
    await loadMyPlan();
    await loadSubscriptionDetails();
    await loadHistory();
    await loadAdminAnalytics();
    await loadAdminBillingLogs();
    await loadAdminWebhookLogs();
    await loadAdminReferralStats();
    await loadReferralInfo();
    await loadReferralLeaderboard();
  };

  useEffect(() => {
    loadReferralLeaderboard();
  }, []);

  useEffect(() => {
    if (token) {
      setShowLanding(false);
      loadMyPlan();
      loadSubscriptionDetails();
      loadHistory();
      loadReferralInfo();
      loadReferralLeaderboard();
      reloadAdmin();
    } else {
      setMyPlan(null);
      setMyPlanMessage("Not logged in.");
      setSubscriptionDetails(null);
      setHistoryItems([]);
      setSelectedHistoryItem(null);
      setHistoryMessage("Not logged in.");
      clearBillingMessages();
      setIsAdmin(false);
      setAdminUsers([]);
      setAdminAnalytics(null);
      setAdminCharts(null);
      setAdminBillingLogs([]);
      setAdminWebhookLogs([]);
      setAdminReferralStats(null);
      setAdminMessage("");
      setReferralInfo(null);
      setReferralCodeInput(localStorage.getItem("referralCode") || "");
      setReferralLoading("");
      setReferralMessage("");
      setReferralLeaderboard([]);
    }
  }, [token]);

  useEffect(() => {
    if (!token || !isAdmin) return;
    loadAdminUsers();
  }, [adminSearch]);

  useEffect(() => {
    if (!token || !isAdmin) return;
    loadAdminWebhookLogs();
  }, [webhookTypeFilter, webhookProcessedFilter]);

  useEffect(() => {
    if (!token) return;

    const params = new URLSearchParams(window.location.search);
    const checkoutStatus = params.get("checkout");
    const referralFromUrl = params.get("ref");

    if (checkoutStatus === "success") {
      clearBillingMessages();
      showToast("Checkout successful. Refreshing your plan...", "success");
      refreshPaidState();
      closeUpgradeModal();
    } else if (checkoutStatus === "cancel") {
      clearBillingMessages();
      showToast("Checkout canceled.", "info");
    }

    if (checkoutStatus || referralFromUrl) {
      params.delete("checkout");
      params.delete("ref");
      const newQuery = params.toString();
      const newUrl = `${window.location.pathname}${newQuery ? `?${newQuery}` : ""}${window.location.hash || ""}`;
      window.history.replaceState({}, "", newUrl);
    }
  }, [token]);

  const handleRegister = async (e) => {
    e?.preventDefault?.();
    if (registerLoading) return;

    const email = String(registerData.email || "").trim();
    const password = String(registerData.password || "");
    const confirmPassword = String(registerData.confirmPassword || "");
    const referralCode = String(localStorage.getItem("referralCode") || "").trim().toUpperCase();

    if (!email) {
      setRegisterMessage("Email is required.");
      showToast("Email is required.", "error");
      return;
    }

    if (!isValidEmail(email)) {
      setRegisterMessage("Invalid email format.");
      showToast("Invalid email format.", "error");
      return;
    }

    if (!password) {
      setRegisterMessage("Password is required.");
      showToast("Password is required.", "error");
      return;
    }

    if (!isStrongPassword(password)) {
      const message = "Password must be 8+ chars and include uppercase, lowercase, and number.";
      setRegisterMessage(message);
      showToast(message, "error");
      return;
    }

    if (!confirmPassword) {
      setRegisterMessage("Please confirm your password.");
      showToast("Please confirm your password.", "error");
      return;
    }

    if (password !== confirmPassword) {
      setRegisterMessage("Passwords do not match.");
      showToast("Passwords do not match.", "error");
      return;
    }

    setRegisterLoading(true);
    setRegisterMessage("Creating account...");

    const res = await fetchJson(`${API_BASE}/api/Auth/register`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify({
        email,
        password,
        referralCode: referralCode || null
      })
    }, {
      fallbackMessage: "Registration failed."
    });

    if (!res.ok) {
      setRegisterMessage(res.message);
      showToast(res.message, "error");
      setRegisterLoading(false);
      return;
    }

    if (res.data?.token) {
      localStorage.setItem("token", res.data.token);
      setToken(res.data.token);
    }

    setRegisterData({
      email: "",
      password: "",
      confirmPassword: ""
    });
    setRegisterMessage(referralCode ? "Account created. Referral code applied." : "Account created.");
    clearBillingMessages();
    showToast("Registered successfully.", "success");
    setRegisterLoading(false);
  };

  const handleLogin = async (e) => {
    e?.preventDefault?.();
    if (loginLoading) return;

    const email = String(loginData.email || "").trim();
    const password = String(loginData.password || "");

    if (!email) {
      setLoginMessage("Email is required.");
      showToast("Email is required.", "error");
      return;
    }

    if (!isValidEmail(email)) {
      setLoginMessage("Invalid email format.");
      showToast("Invalid email format.", "error");
      return;
    }

    if (!password) {
      setLoginMessage("Password is required.");
      showToast("Password is required.", "error");
      return;
    }

    setLoginLoading(true);
    setLoginMessage("Logging in...");

    const res = await fetchJson(`/api/Auth/login`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify({
        email,
        password
      })
    }, {
      fallbackMessage: "Login failed."
    });

    if (!res.ok) {
      setLoginMessage(res.message);
      showToast(res.message, "error");
      setLoginLoading(false);
      return;
    }

    localStorage.setItem("token", res.data.token);
    setToken(res.data.token);
    setShowLanding(false);
    setLoginMessage("Logged in.");
    setLoginData({ email: "", password: "" });
    clearBillingMessages();
    showToast("Logged in successfully.", "success");
    setLoginLoading(false);
  };

  const handleLogout = () => {
    localStorage.removeItem("token");
    setToken("");
    setShowLanding(true);
    setResult(null);
    setGeneratorMessage("");
    setHistoryItems([]);
    setSelectedHistoryItem(null);
    setLoginMessage("Logged out.");
    setRegisterMessage("");
    setHistorySearch("");
    setHistorySort("newest");
    setSubscriptionDetails(null);
    setShowUpgradeModal(false);
    setStartingUpgradePlan("");
    setSubscriptionActionLoading("");
    setLoginLoading(false);
    setRegisterLoading(false);
    setIsAdmin(false);
    setAdminUsers([]);
    setAdminAnalytics(null);
    setAdminCharts(null);
    setAdminBillingLogs([]);
    setAdminWebhookLogs([]);
    setAdminReferralStats(null);
    setAdminMessage("");
    setReferralInfo(null);
    setReferralCodeInput(localStorage.getItem("referralCode") || "");
    setReferralLoading("");
    setReferralMessage("");
    setReferralLeaderboard([]);
    clearBillingMessages();
    showToast("Logged out.", "success");
  };

  const handleStartUpgradeFlow = async (planName) => {
    const normalized = normalizePlanName(planName);

    if (normalized === currentPlanName) {
      showToast(`${planName} is already your current plan.`, "info");
      return;
    }

    if (startingUpgradePlan) return;

    clearBillingMessages();
    setStartingUpgradePlan(planName);

    const res = await fetchJson(`${API_BASE}/api/Plans/upgrade`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify({ planName })
    }, {
      auth: true,
      fallbackMessage: `Failed to start ${planName} upgrade.`
    });

    if (!res.ok) {
      setBillingErrorMessage(res.message);
      showToast(res.message, "error");
      setStartingUpgradePlan("");
      return;
    }

    const checkoutUrl = res.data?.checkoutUrl || res.data?.url;

    if (!checkoutUrl || typeof checkoutUrl !== "string") {
      const message = `Checkout URL missing for ${planName}.`;
      setBillingErrorMessage(message);
      showToast(message, "error");
      setStartingUpgradePlan("");
      return;
    }

    setBillingSuccessMessage(`Starting ${planName} checkout...`);
    showToast(`Starting ${planName} checkout...`, "success");
    window.location.href = checkoutUrl;
  };

  const handleCancelAtPeriodEnd = async () => {
    if (!token || isFreePlan || subscriptionActionLoading) return;

    clearBillingMessages();
    setSubscriptionActionLoading("cancel");

    const res = await fetchJson(`${API_BASE}/api/Plans/cancel`, {
      method: "POST"
    }, {
      auth: true,
      fallbackMessage: "Failed to cancel subscription."
    });

    if (!res.ok) {
      setBillingErrorMessage(res.message);
      showToast(res.message, "error");
      setSubscriptionActionLoading("");
      return;
    }

    setSubscriptionDetails(res.data);
    setBillingSuccessMessage("Subscription will cancel at period end.");
    showToast("Subscription will cancel at period end.", "success");
    await loadMyPlan();
    await loadAdminBillingLogs();
    await loadAdminAnalytics();
    setSubscriptionActionLoading("");
  };

  const handleReactivateSubscription = async () => {
    if (!token || isFreePlan || subscriptionActionLoading) return;

    clearBillingMessages();
    setSubscriptionActionLoading("reactivate");

    const res = await fetchJson(`${API_BASE}/api/Plans/reactivate`, {
      method: "POST"
    }, {
      auth: true,
      fallbackMessage: "Failed to reactivate subscription."
    });

    if (!res.ok) {
      setBillingErrorMessage(res.message);
      showToast(res.message, "error");
      setSubscriptionActionLoading("");
      return;
    }

    setSubscriptionDetails(res.data);
    setBillingSuccessMessage("Subscription reactivated.");
    showToast("Subscription reactivated.", "success");
    await loadMyPlan();
    await loadAdminBillingLogs();
    await loadAdminAnalytics();
    setSubscriptionActionLoading("");
  };

  const openBillingPortal = async () => {
    const res = await fetchJson(`${API_BASE}/api/Plans/billing-portal`, {
      method: "POST"
    }, {
      auth: true,
      fallbackMessage: "Failed to open billing portal."
    });

    if (!res.ok) {
      showToast(res.message, "error");
      return;
    }

    window.location.href = res.data.url;
  };

  const handleGenerate = async () => {
    if (isLimitReached) {
      const message = "Daily limit reached. Upgrade your plan.";
      setGeneratorMessage(message);
      showToast(message, "error");
      openUpgradeModal();
      return;
    }

    setLoading(true);
    setResult(null);
    setGeneratorMessage("Generating...");

    const res = await fetchJson(`${API_BASE}/api/Ai/generate`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(generatorData)
    }, {
      auth: true,
      fallbackMessage: "Generation failed."
    });

    if (!res.ok) {
      if (res.status === 429) {
        const message = res.message || "Daily limit reached. Upgrade your plan.";
        setGeneratorMessage(message);
        setLoading(false);
        showToast(message, "error");
        openUpgradeModal();
        await loadMyPlan();
        await loadSubscriptionDetails();
        return;
      }

      setGeneratorMessage(res.message);
      setLoading(false);
      showToast(res.message, "error");
      return;
    }

    if (!res.data?.variants || !Array.isArray(res.data.variants)) {
      const message = "Generation returned an unexpected response.";
      setGeneratorMessage(message);
      setLoading(false);
      showToast(message, "error");
      return;
    }

    setResult(res.data);
    setGeneratorMessage("Generation successful.");
    showToast("Content generated successfully.", "success");
    await loadMyPlan();
    await loadSubscriptionDetails();
    await loadHistory();
    await loadAdminAnalytics();
    await loadReferralInfo();
    setLoading(false);
  };

  const handleRegenerate = async (id) => {
    if (isLimitReached) {
      const message = "Daily limit reached. Upgrade your plan.";
      setGeneratorMessage(message);
      showToast(message, "error");
      openUpgradeModal();
      return;
    }

    setRegeneratingId(id);
    setGeneratorMessage("Regenerating...");

    const res = await fetchJson(`${API_BASE}/api/Ai/${id}/regenerate`, {
      method: "POST"
    }, {
      auth: true,
      fallbackMessage: "Regeneration failed."
    });

    if (!res.ok) {
      if (res.status === 429) {
        const message = res.message || "Daily limit reached. Upgrade your plan.";
        setGeneratorMessage(message);
        setRegeneratingId(null);
        showToast(message, "error");
        openUpgradeModal();
        await loadMyPlan();
        await loadSubscriptionDetails();
        return;
      }

      setGeneratorMessage(res.message);
      setRegeneratingId(null);
      showToast(res.message, "error");
      return;
    }

    if (!res.data?.variants || !Array.isArray(res.data.variants)) {
      const message = "Regeneration returned an unexpected response.";
      setGeneratorMessage(message);
      setRegeneratingId(null);
      showToast(message, "error");
      return;
    }

    setResult(res.data);
    setGeneratorMessage("Regeneration successful.");
    showToast("Content regenerated successfully.", "success");

    await loadMyPlan();
    await loadSubscriptionDetails();
    await loadAdminAnalytics();
    await loadReferralInfo();

    const updatedItems = await loadHistory();

    if (Array.isArray(updatedItems) && updatedItems.length > 0) {
      setSelectedHistoryItem(updatedItems[0]);
    }

    setRegeneratingId(null);
  };

  const handleDeleteHistoryItem = async (id) => {
    const confirmed = window.confirm("Delete this history item?");
    if (!confirmed) return;

    setDeletingId(id);
    setHistoryMessage("Deleting history item...");

    const res = await fetchJson(`${API_BASE}/api/Ai/${id}`, {
      method: "DELETE"
    }, {
      auth: true,
      fallbackMessage: "Failed to delete history item."
    });

    if (!res.ok) {
      setHistoryMessage(res.message);
      setDeletingId(null);
      showToast(res.message, "error");
      return;
    }

    const remainingItems = historyItems.filter((x) => x.id !== id);
    setHistoryItems(remainingItems);

    if (selectedHistoryItem?.id === id) {
      setSelectedHistoryItem(remainingItems.length > 0 ? remainingItems[0] : null);
    }

    const message = remainingItems.length === 0 ? "No history yet." : "History item deleted.";
    setHistoryMessage(message);
    showToast("History item deleted.", "success");
    setDeletingId(null);
  };

  const handleAdminAction = async (label, url, options = {}) => {
    if (!token || adminActionLoading) return;

    setAdminActionLoading(label);
    setAdminMessage("");

    const res = await fetchJson(url, options, {
      auth: true,
      fallbackMessage: "Admin action failed."
    });

    if (!res.ok) {
      setAdminMessage(res.message);
      showToast(res.message, "error");
      setAdminActionLoading("");
      return;
    }

    setAdminMessage("Admin action completed.");
    showToast("Admin action completed.", "success");
    await reloadAdmin();
    setAdminActionLoading("");
  };

  const handleRetryWebhook = async (id) => {
    await handleAdminAction(
      `retry-webhook-${id}`,
      `${API_BASE}/api/admin/webhooks/retry?id=${id}`,
      { method: "POST" }
    );
  };

  const handleUseAsTemplate = (item) => {
    if (!item) return;

    setGeneratorData({
      topic: item.topic || "",
      platform: item.platform || "TikTok",
      tone: item.tone || "bold",
      goal: "engagement",
      contentType: "post",
      targetAudience: "",
      numberOfVariants: 3
    });

    setGeneratorMessage("Template loaded from history.");
    showToast("Template loaded from history.", "success");
  };

  const handleUseLatestResultAsTemplate = () => {
    setGeneratorData((current) => ({
      topic: result?.topic || current.topic,
      platform: result?.platform || current.platform || "TikTok",
      tone: result?.tone || current.tone || "bold",
      goal: result?.goal || current.goal || "engagement",
      contentType: result?.contentType || current.contentType || "post",
      targetAudience: result?.targetAudience || current.targetAudience || "",
      numberOfVariants: result?.numberOfVariants || result?.variants?.length || current.numberOfVariants || 1
    }));

    setGeneratorMessage("Latest result loaded into generator.");
    showToast("Latest result loaded into generator.", "success");
  };

  const clearLatestResult = () => {
    setResult(null);
    setGeneratorMessage("Latest result cleared.");
    showToast("Latest result cleared.", "success");
  };

  const buildVariantText = (variant, isHistoryVariant = false) => {
    const hook = isHistoryVariant ? variant.Hook : variant.hook;
    const content = isHistoryVariant ? variant.Content : variant.content;
    const callToAction = isHistoryVariant ? variant.CallToAction : variant.callToAction;
    const hashtags = isHistoryVariant ? variant.Hashtags : variant.hashtags;

    return [hook || "", "", content || "", "", callToAction || "", "", Array.isArray(hashtags) ? hashtags.join(" ") : ""].join("\n");
  };

  const copyVariantText = (variant, isHistoryVariant = false) => {
    navigator.clipboard.writeText(buildVariantText(variant, isHistoryVariant));
    showToast("Variant copied.", "success");
  };

  const copyAllLatestVariants = () => {
    if (!result?.variants || !Array.isArray(result.variants) || result.variants.length === 0) {
      setGeneratorMessage("No latest result to copy.");
      showToast("No latest result to copy.", "error");
      return;
    }

    const text = result.variants
      .map((variant, index) => `Variant ${index + 1}\n${buildVariantText(variant, false)}`)
      .join("\n\n--------------------\n\n");

    navigator.clipboard.writeText(text);
    setGeneratorMessage("All latest variants copied.");
    showToast("All latest variants copied.", "success");
  };

  const exportLatestResultToTxt = () => {
    if (!result?.variants || !Array.isArray(result.variants) || result.variants.length === 0) {
      showToast("No latest result to export.", "error");
      return;
    }

    const header = [
      `Topic: ${result.topic || generatorData.topic || ""}`,
      `Platform: ${result.platform || generatorData.platform || ""}`,
      `Tone: ${result.tone || generatorData.tone || ""}`,
      `Goal: ${result.goal || generatorData.goal || ""}`,
      `Content Type: ${result.contentType || generatorData.contentType || ""}`,
      `Target Audience: ${result.targetAudience || generatorData.targetAudience || ""}`
    ].join("\n");

    const body = result.variants
      .map((variant, index) => `Variant ${index + 1}\n${buildVariantText(variant, false)}`)
      .join("\n\n====================\n\n");

    const content = `${header}\n\n====================\n\n${body}`;
    const blob = new Blob([content], { type: "text/plain;charset=utf-8" });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    const safeTopic = (result.topic || generatorData.topic || "viral-content")
      .toLowerCase()
      .replace(/[^a-z0-9]+/g, "-")
      .replace(/^-+|-+$/g, "");

    link.href = url;
    link.download = `${safeTopic || "viral-content"}-result.txt`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);

    showToast("Latest result exported to txt.", "success");
  };

  const getVariantCharacterCount = (variant, isHistoryVariant = false) => {
    return buildVariantText(variant, isHistoryVariant).length;
  };

  const handleGenerateReferralCode = async () => {
    if (referralLoading) return;

    setReferralLoading("generate");
    setReferralMessage("");

    const res = await fetchJson(`${API_BASE}/api/referrals/generate`, {
      method: "POST"
    }, {
      auth: true,
      fallbackMessage: "Failed to generate referral code."
    });

    if (!res.ok) {
      setReferralMessage(res.message);
      showToast(res.message, "error");
      setReferralLoading("");
      return;
    }

    setReferralMessage("Referral code ready.");
    showToast("Referral code ready.", "success");
    await loadReferralInfo();
    await loadReferralLeaderboard();
    setReferralLoading("");
  };

  const handleApplyReferralCode = async () => {
    if (referralLoading) return;

    const code = String(referralCodeInput || "").trim();
    if (!code) {
      setReferralMessage("Referral code is required.");
      showToast("Referral code is required.", "error");
      return;
    }

    setReferralLoading("apply");
    setReferralMessage("");

    const res = await fetchJson(`${API_BASE}/api/referrals/apply`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify({ code })
    }, {
      auth: true,
      fallbackMessage: "Failed to apply referral code."
    });

    if (!res.ok) {
      setReferralMessage(res.message);
      showToast(res.message, "error");
      setReferralLoading("");
      return;
    }

    setReferralMessage(res.message || "Referral code applied.");
    showToast(res.message || "Referral code applied.", "success");
    await loadReferralInfo();
    await loadMyPlan();
    await loadSubscriptionDetails();
    await loadReferralLeaderboard();
    setReferralLoading("");
  };

  const handleCopyReferralLink = async () => {
    if (!referralInfo?.referralLink) {
      showToast("No referral link available.", "error");
      return;
    }

    try {
      await navigator.clipboard.writeText(referralInfo.referralLink);

      await fetchJson(`${API_BASE}/api/referrals/track-share`, {
        method: "POST"
      }, {
        auth: true,
        fallbackMessage: "Failed to track share."
      });

      await loadReferralInfo();
      await loadReferralLeaderboard();

      showToast("Referral link copied.", "success");
    } catch {
      showToast("Failed to copy referral link.", "error");
    }
  };

  const filteredAndSortedHistoryItems = useMemo(() => {
    const search = historySearch.trim().toLowerCase();

    const filtered = historyItems.filter((item) => {
      if (!search) return true;

      const topic = item.topic?.toLowerCase() || "";
      const platform = item.platform?.toLowerCase() || "";
      const tone = item.tone?.toLowerCase() || "";

      return topic.includes(search) || platform.includes(search) || tone.includes(search);
    });

    const sorted = [...filtered].sort((a, b) => {
      const aTime = new Date(a.createdAt).getTime();
      const bTime = new Date(b.createdAt).getTime();
      return historySort === "oldest" ? aTime - bTime : bTime - aTime;
    });

    return sorted;
  }, [historyItems, historySearch, historySort]);

  const renderHistoryContent = (item) => {
    if (!item?.content) return null;

    try {
      const parsed = JSON.parse(item.content);

      if (!parsed?.Variants || !Array.isArray(parsed.Variants)) {
        return (
          <pre
            style={{
              whiteSpace: "pre-wrap",
              wordBreak: "break-word",
              background: "#0f172a",
              padding: 12,
              borderRadius: 8,
              border: "1px solid #334155"
            }}
          >
            {item.content}
          </pre>
        );
      }

      return parsed.Variants.map((v) => (
        <div
          key={v.VariantNumber}
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
            <h4 style={{ margin: 0 }}>Variant {v.VariantNumber}</h4>
            <span style={{ fontSize: 12, opacity: 0.8 }}>
              {getVariantCharacterCount(v, true)} chars
            </span>
          </div>
          <p><strong>Hook:</strong> {v.Hook}</p>
          <p style={{ whiteSpace: "pre-wrap" }}>{v.Content}</p>
          <p><strong>CTA:</strong> {v.CallToAction}</p>
          <p>{Array.isArray(v.Hashtags) ? v.Hashtags.join(" ") : ""}</p>
          <button onClick={() => copyVariantText(v, true)} style={buttonBaseStyle}>Copy</button>
        </div>
      ));
    } catch {
      return (
        <pre
          style={{
            whiteSpace: "pre-wrap",
            wordBreak: "break-word",
            background: "#0f172a",
            padding: 12,
            borderRadius: 8,
            border: "1px solid #334155"
          }}
        >
          {item.content}
        </pre>
      );
    }
  };

  const formatUtcDate = (value) => {
    if (!value) return "—";
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return "—";
    return date.toLocaleString();
  };

  const getRenewalLabel = () => {
    if (!subscriptionDetails?.currentPeriodEndUtc) return null;

    if (subscriptionStatus === "canceling") return "Ends on";
    if (subscriptionStatus === "active" || subscriptionStatus === "trialing" || subscriptionStatus === "past_due") {
      return "Renews on";
    }

    return "Period ends";
  };

  const getStatusBadgeStyle = (status) => {
    const normalized = normalizePlanName(status);

    if (normalized === "active") {
      return {
        background: "#14532d",
        border: "1px solid #166534",
        color: "white"
      };
    }

    if (normalized === "canceling") {
      return {
        background: "#78350f",
        border: "1px solid #b45309",
        color: "white"
      };
    }

    if (normalized === "past_due") {
      return {
        background: "#7f1d1d",
        border: "1px solid #991b1b",
        color: "white"
      };
    }

    if (normalized === "trialing") {
      return {
        background: "#1d4ed8",
        border: "1px solid #2563eb",
        color: "white"
      };
    }

    return {
      background: "#1f2937",
      border: "1px solid #475569",
      color: "white"
    };
  };

  const buttonBaseStyle = {
    padding: "10px 14px",
    borderRadius: 8,
    border: "1px solid #475569",
    background: "#1f2937",
    color: "white",
    cursor: "pointer"
  };

  const sectionStyle = {
    marginBottom: 24,
    padding: 16,
    border: "1px solid #334155",
    borderRadius: 8,
    background: "#111827"
  };

  const inputStyle = {
    padding: 10,
    borderRadius: 8,
    border: "1px solid #475569",
    background: "#0f172a",
    color: "white",
    width: "100%",
    boxSizing: "border-box"
  };

  const cardStyle = {
    border: "1px solid #334155",
    borderRadius: 12,
    padding: 14,
    background: "#0b1220",
    minWidth: 140
  };

  const planCardStyle = {
    border: myPlan?.planName?.toLowerCase().includes("creator")
      ? "2px solid #a855f7"
      : myPlan?.planName?.toLowerCase().includes("pro")
        ? "2px solid #2563eb"
        : "2px solid #475569",
    background: myPlan?.planName?.toLowerCase().includes("creator")
      ? "linear-gradient(135deg, #2a1639 0%, #111827 100%)"
      : myPlan?.planName?.toLowerCase().includes("pro")
        ? "linear-gradient(135deg, #172554 0%, #111827 100%)"
        : "#111827",
    borderRadius: 12,
    padding: 16
  };

  return (
    <div
      style={{
        padding: 20,
        maxWidth: 1200,
        margin: "0 auto",
        color: "white"
      }}
    >
      {toast && (
        <div
          style={{
            position: "sticky",
            top: 12,
            zIndex: 1000,
            marginBottom: 16
          }}
        >
          <div
            style={{
              padding: "12px 16px",
              borderRadius: 10,
              border: "1px solid",
              borderColor:
                toast.type === "success"
                  ? "#166534"
                  : toast.type === "error"
                    ? "#991b1b"
                    : "#475569",
              background:
                toast.type === "success"
                  ? "#14532d"
                  : toast.type === "error"
                    ? "#7f1d1d"
                    : "#1e293b",
              color: "white",
              boxShadow: "0 10px 30px rgba(0,0,0,0.25)"
            }}
          >
            {toast.message}
          </div>
        </div>
      )}

      <UpgradeModal
        showUpgradeModal={showUpgradeModal}
        closeUpgradeModal={closeUpgradeModal}
        startingUpgradePlan={startingUpgradePlan}
        billingErrorMessage={billingErrorMessage}
        billingSuccessMessage={billingSuccessMessage}
        plans={plans}
        currentPlanName={currentPlanName}
        recommendedPlanKey={recommendedPlanKey}
        normalizePlanName={normalizePlanName}
        handleStartUpgradeFlow={handleStartUpgradeFlow}
        buttonBaseStyle={buttonBaseStyle}
      />

      <h1 style={{ marginBottom: 24 }}>🚀 Viral Content Generator</h1>

      {showLanding && !token && (
        <Landing onStart={() => setShowLanding(false)} openUpgrade={openUpgradeModal} />
      )}

      {!token && !showLanding && (
        <AuthSection
          registerData={registerData}
          setRegisterData={setRegisterData}
          registerLoading={registerLoading}
          registerMessage={registerMessage}
          handleRegister={handleRegister}
          loginData={loginData}
          setLoginData={setLoginData}
          loginLoading={loginLoading}
          loginMessage={loginMessage}
          handleLogin={handleLogin}
          inputStyle={inputStyle}
          buttonBaseStyle={buttonBaseStyle}
        />
      )}

      {token && (
        <>
          <div style={sectionStyle}>
            <h2>Auth</h2>
            <p>Logged in.</p>
            <button onClick={handleLogout} style={buttonBaseStyle}>
              Logout
            </button>
          </div>

          {isAdmin && (
            <AdminDashboard
              adminAnalytics={adminAnalytics}
              adminReferralStats={adminReferralStats}
              adminCharts={adminCharts}
              adminSearch={adminSearch}
              setAdminSearch={setAdminSearch}
              adminMessage={adminMessage}
              adminLoading={adminLoading}
              adminUsers={adminUsers}
              adminActionLoading={adminActionLoading}
              handleAdminAction={handleAdminAction}
              adminBillingLogs={adminBillingLogs}
              adminWebhookLogs={adminWebhookLogs}
              webhookTypeFilter={webhookTypeFilter}
              setWebhookTypeFilter={setWebhookTypeFilter}
              webhookProcessedFilter={webhookProcessedFilter}
              setWebhookProcessedFilter={setWebhookProcessedFilter}
              handleRetryWebhook={handleRetryWebhook}
              cardStyle={cardStyle}
              inputStyle={inputStyle}
              buttonBaseStyle={buttonBaseStyle}
              sectionStyle={sectionStyle}
              formatUtcDate={formatUtcDate}
              API_BASE={API_BASE}
            />
          )}

          <MyPlanSection
            myPlanMessage={myPlanMessage}
            myPlan={myPlan}
            remainingToday={remainingToday}
            planLimit={planLimit}
            usageUsed={usageUsed}
            usagePercent={usagePercent}
            referralSignupCount={referralSignupCount}
            referralTrialThreshold={referralTrialThreshold}
            referralsRemainingForTrial={referralsRemainingForTrial}
            referralProgressPercent={referralProgressPercent}
            hasEarnedReferralTrial={hasEarnedReferralTrial}
            referralTrialDaysLeft={referralTrialDaysLeft}
            currentPlanName={currentPlanName}
            openUpgradeModal={openUpgradeModal}
            subscriptionDetails={subscriptionDetails}
            getStatusBadgeStyle={getStatusBadgeStyle}
            getRenewalLabel={getRenewalLabel}
            formatUtcDate={formatUtcDate}
            billingErrorMessage={billingErrorMessage}
            billingSuccessMessage={billingSuccessMessage}
            isFreePlan={isFreePlan}
            canCancelAtPeriodEnd={canCancelAtPeriodEnd}
            canReactivate={canReactivate}
            subscriptionActionLoading={subscriptionActionLoading}
            handleCancelAtPeriodEnd={handleCancelAtPeriodEnd}
            handleReactivateSubscription={handleReactivateSubscription}
            openBillingPortal={openBillingPortal}
            sectionStyle={sectionStyle}
            buttonBaseStyle={buttonBaseStyle}
            planCardStyle={planCardStyle}
          />

          <ReferralSection
            referralInfo={referralInfo}
            referralTrialThreshold={referralTrialThreshold}
            referralSignupCount={referralSignupCount}
            referralProgressPercent={referralProgressPercent}
            referralsRemainingForTrial={referralsRemainingForTrial}
            hasEarnedReferralTrial={hasEarnedReferralTrial}
            referralLoading={referralLoading}
            referralMessage={referralMessage}
            referralCodeInput={referralCodeInput}
            setReferralCodeInput={setReferralCodeInput}
            handleGenerateReferralCode={handleGenerateReferralCode}
            handleCopyReferralLink={handleCopyReferralLink}
            referralLeaderboard={referralLeaderboard}
            myUserId={myUserId}
            handleApplyReferralCode={handleApplyReferralCode}
            sectionStyle={sectionStyle}
            cardStyle={cardStyle}
            buttonBaseStyle={buttonBaseStyle}
            inputStyle={inputStyle}
          />

          <GeneratorSection
            sectionStyle={sectionStyle}
            generatorData={generatorData}
            setGeneratorData={setGeneratorData}
            inputStyle={inputStyle}
            handleGenerate={handleGenerate}
            loading={loading}
            isLimitReached={isLimitReached}
            buttonBaseStyle={buttonBaseStyle}
            generatorMessage={generatorMessage}
          />

          <LatestResultSection
            sectionStyle={sectionStyle}
            result={result}
            copyAllLatestVariants={copyAllLatestVariants}
            handleUseLatestResultAsTemplate={handleUseLatestResultAsTemplate}
            exportLatestResultToTxt={exportLatestResultToTxt}
            clearLatestResult={clearLatestResult}
            buttonBaseStyle={buttonBaseStyle}
            getVariantCharacterCount={getVariantCharacterCount}
            copyVariantText={copyVariantText}
          />

          <HistorySection
            sectionStyle={sectionStyle}
            historySearch={historySearch}
            setHistorySearch={setHistorySearch}
            historySort={historySort}
            setHistorySort={setHistorySort}
            inputStyle={inputStyle}
            historyMessage={historyMessage}
            historyItems={historyItems}
            filteredAndSortedHistoryItems={filteredAndSortedHistoryItems}
            selectedHistoryItem={selectedHistoryItem}
            setSelectedHistoryItem={setSelectedHistoryItem}
            regeneratingId={regeneratingId}
            deletingId={deletingId}
            isLimitReached={isLimitReached}
            handleRegenerate={handleRegenerate}
            handleUseAsTemplate={handleUseAsTemplate}
            handleDeleteHistoryItem={handleDeleteHistoryItem}
            buttonBaseStyle={buttonBaseStyle}
            renderHistoryContent={renderHistoryContent}
          />
        </>
      )}
    </div>
  );
}

export default App;