const API_URL = "";

function getToken() {
  return localStorage.getItem("token");
}

function authHeader() {
  const token = getToken();
  return token ? { Authorization: `Bearer ${token}` } : {};
}

async function handleResponse(response) {
  const text = await response.text();
  const data = text ? JSON.parse(text) : null;

  if (!response.ok) {
    throw new Error(data?.message || data?.details || `HTTP ${response.status}`);
  }

  return data;
}

export async function login(email, password) {
  const response = await fetch(`${API_URL}/api/Auth/login`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify({ email, password })
  });

  const data = await handleResponse(response);

  if (data?.token) {
    localStorage.setItem("token", data.token);
  }

  return data;
}

export async function register(username, email, password) {
  const response = await fetch(`${API_URL}/api/Auth/register`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify({ username, email, password })
  });

  const data = await handleResponse(response);

  if (data?.token) {
    localStorage.setItem("token", data.token);
  }

  return data;
}

export async function getMyPlan() {
  const response = await fetch(`${API_URL}/api/Plans/my-plan`, {
    headers: {
      ...authHeader()
    }
  });

  return handleResponse(response);
}

export async function generateContent(payload) {
  const response = await fetch(`${API_URL}/api/Ai/generate`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      ...authHeader()
    },
    body: JSON.stringify(payload)
  });

  return handleResponse(response);
}

export async function getHistory() {
  const response = await fetch(`${API_URL}/api/Ai/history`, {
    headers: {
      ...authHeader()
    }
  });

  return handleResponse(response);
}

export async function getUsage() {
  const response = await fetch(`${API_URL}/api/Ai/usage`, {
    headers: {
      ...authHeader()
    }
  });

  return handleResponse(response);
}

export async function getUsageHistory(days = 30) {
  const response = await fetch(`${API_URL}/api/Ai/usage/history?days=${days}`, {
    headers: {
      ...authHeader()
    }
  });

  return handleResponse(response);
}

export async function regenerateHistoryItem(id) {
  const response = await fetch(`${API_URL}/api/Ai/${id}/regenerate`, {
    method: "POST",
    headers: {
      ...authHeader()
    }
  });

  return handleResponse(response);
}