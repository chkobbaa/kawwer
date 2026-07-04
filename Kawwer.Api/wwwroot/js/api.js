// Typed-ish client over the Kawwer REST API. Unwraps the standard { success, data, message }
// envelope, stores JWTs in localStorage, and transparently refreshes an expired access token.

const API_BASE = "/api/v1";

const Keys = {
  access: "kawwer.accessToken",
  refresh: "kawwer.refreshToken",
  user: "kawwer.user",
};

export const auth = {
  get accessToken() {
    return localStorage.getItem(Keys.access);
  },
  get refreshToken() {
    return localStorage.getItem(Keys.refresh);
  },
  get user() {
    const raw = localStorage.getItem(Keys.user);
    return raw ? JSON.parse(raw) : null;
  },
  get isAuthenticated() {
    return !!localStorage.getItem(Keys.access);
  },
  save(authResponse) {
    localStorage.setItem(Keys.access, authResponse.accessToken);
    localStorage.setItem(Keys.refresh, authResponse.refreshToken);
    localStorage.setItem(Keys.user, JSON.stringify(authResponse.user));
  },
  saveUser(user) {
    localStorage.setItem(Keys.user, JSON.stringify(user));
  },
  clear() {
    localStorage.removeItem(Keys.access);
    localStorage.removeItem(Keys.refresh);
    localStorage.removeItem(Keys.user);
  },
};

export class ApiError extends Error {
  constructor(message, status) {
    super(message);
    this.status = status;
  }
}

let refreshInFlight = null;

async function tryRefresh() {
  const token = auth.refreshToken;
  if (!token) return false;
  // Collapse concurrent 401s into a single refresh call.
  if (!refreshInFlight) {
    refreshInFlight = (async () => {
      try {
        const res = await fetch(`${API_BASE}/auth/refresh`, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ refreshToken: token }),
        });
        const json = await res.json().catch(() => null);
        if (res.ok && json && json.success && json.data) {
          auth.save(json.data);
          return true;
        }
      } catch {
        /* fall through */
      }
      return false;
    })();
  }
  const ok = await refreshInFlight;
  refreshInFlight = null;
  return ok;
}

async function request(method, path, body, { retry = true, anonymous = false } = {}) {
  const headers = {};
  if (body !== undefined && body !== null) headers["Content-Type"] = "application/json";
  if (!anonymous && auth.accessToken) headers["Authorization"] = `Bearer ${auth.accessToken}`;

  let res;
  try {
    res = await fetch(`${API_BASE}${path}`, {
      method,
      headers,
      body: body !== undefined && body !== null ? JSON.stringify(body) : undefined,
    });
  } catch (networkError) {
    throw new ApiError("Network error — check your connection.", 0);
  }

  if (res.status === 401 && !anonymous && retry) {
    const refreshed = await tryRefresh();
    if (refreshed) return request(method, path, body, { retry: false, anonymous });
    auth.clear();
    throw new ApiError("Your session expired. Please sign in again.", 401);
  }

  const text = await res.text();
  const json = text ? safeParse(text) : null;

  if (json && json.success) return json.data;

  throw new ApiError(extractError(json, res.status), res.status);
}

function safeParse(text) {
  try {
    return JSON.parse(text);
  } catch {
    return null;
  }
}

function extractError(json, status) {
  if (json) {
    if (json.message) return json.message;
    if (Array.isArray(json.errors) && json.errors.length) return json.errors[0];
    if (json.errors && typeof json.errors === "object") {
      const first = Object.values(json.errors)[0];
      if (Array.isArray(first) && first.length) return first[0];
    }
    if (json.detail) return String(json.detail).split("\n")[0];
    if (json.title) return json.title;
  }
  return `Request failed (${status}).`;
}

export const api = {
  // ----- Auth -----
  register: (body) => request("POST", "/auth/register", body, { anonymous: true }),
  login: (usernameOrEmail, password) =>
    request("POST", "/auth/login", { usernameOrEmail, password }, { anonymous: true }),
  logout: (refreshToken) => request("POST", "/auth/logout", { refreshToken }, { anonymous: true }),

  // ----- Users -----
  getMe: () => request("GET", "/users/me"),
  getMyStatistics: () => request("GET", "/users/me/statistics"),

  // ----- Matches -----
  getUpcoming: () => request("GET", "/matches/upcoming"),
  getDashboard: () => request("GET", "/matches/dashboard"),
  getMatch: (id) => request("GET", `/matches/${id}`),
  getParticipants: (id) => request("GET", `/matches/${id}/participants`),
  respond: (id, accept) => request("POST", `/matches/${id}/respond`, { accept }),
  leave: (id) => request("POST", `/matches/${id}/leave`, {}),
  markThinking: (id) => request("POST", `/matches/${id}/thinking`, {}),

  // ----- Notifications -----
  getNotifications: (page = 1, pageSize = 30) =>
    request("GET", `/notifications?page=${page}&pageSize=${pageSize}`),
  getUnreadCount: () => request("GET", "/notifications/unread-count"),
  markNotificationRead: (id) => request("POST", `/notifications/${id}/read`, {}),
  markAllRead: () => request("POST", "/notifications/read-all", {}),
  deleteNotification: (id) => request("DELETE", `/notifications/${id}`),

  // ----- Public matches -----
  discover: (lat, lng, radiusKm) => {
    let q = "/public-matches/discover?page=1&pageSize=50";
    if (lat != null && lng != null) {
      q += `&latitude=${lat}&longitude=${lng}`;
      if (radiusKm != null) q += `&radiusKm=${radiusKm}`;
    }
    return request("GET", q);
  },
  joinPublicMatch: (id) => request("POST", `/public-matches/${id}/join`, {}),

  // ----- Push (PWA / Web Push) -----
  getVapidPublicKey: () => request("GET", "/push/vapid-public-key", null, { anonymous: true }),
  subscribeWebPush: (subscription) => request("POST", "/push/web/subscribe", subscription),
  unsubscribeWebPush: (endpoint) => request("POST", "/push/web/unsubscribe", { endpoint }),
};
