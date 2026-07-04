import { api, auth, ApiError } from "./api.js";
import * as push from "./push.js";
import {
  escapeHtml, capitalize, fullName, initials, whenLabel, shortTime,
  dayNumber, monthShort, relativeTime, money, notificationIcon, toast,
} from "./ui.js";

const app = document.getElementById("app");
let unreadCount = 0;

// ---------------------------------------------------------------------------
// Labels
// ---------------------------------------------------------------------------
const BADGE_LABEL = {
  VeryReliable: "Very reliable", Reliable: "Reliable",
  OccasionallyCancels: "Occasionally cancels", OftenLate: "Often late",
  FrequentNoShow: "Frequent no-show",
};

function participantStatusLabel(p) {
  switch (p.status) {
    case "Accepted": return "In";
    case "WaitingList": return p.waitingListPosition ? `Waiting #${p.waitingListPosition}` : "Waiting";
    case "Thinking": return "Thinking";
    case "Invited": return p.isJoinRequest ? "Wants in" : "Invited";
    case "Seen": return p.isJoinRequest ? "Wants in" : "Seen";
    case "Declined": return "Declined";
    default: return p.status;
  }
}

// ---------------------------------------------------------------------------
// Router
// ---------------------------------------------------------------------------
const routes = [
  { pattern: /^#\/login$/, handler: renderLogin, public: true },
  { pattern: /^#\/register$/, handler: renderRegister, public: true },
  { pattern: /^#\/discover$/, handler: renderDiscover, tab: "discover" },
  { pattern: /^#\/notifications$/, handler: renderNotifications, tab: "notifications" },
  { pattern: /^#\/profile$/, handler: renderProfile, tab: "profile" },
  { pattern: /^#\/match\/([0-9a-fA-F-]+)$/, handler: renderMatch },
  { pattern: /^#\/$/, handler: renderHome, tab: "home" },
];

function currentHash() {
  return location.hash && location.hash !== "#" ? location.hash : "#/";
}

export function navigate(hash) {
  if (currentHash() === hash) router();
  else location.hash = hash;
}

async function router() {
  const hash = currentHash();
  const match = routes.find((r) => r.pattern.test(hash));

  if (!auth.isAuthenticated) {
    if (!match || !match.public) return navigate("#/login");
  } else if (match && match.public) {
    return navigate("#/");
  }

  if (!match) return navigate("#/");
  const params = hash.match(match.pattern).slice(1);
  try {
    await match.handler(...params);
  } catch (err) {
    if (err instanceof ApiError && err.status === 401) return navigate("#/login");
    renderError(err);
  }
  window.scrollTo(0, 0);
}

// ---------------------------------------------------------------------------
// Shell
// ---------------------------------------------------------------------------
function shell(tab, title, content, { back = null } = {}) {
  const nav = tab
    ? `<nav class="tabbar">
        ${tabButton("home", "#/", "🏠", "Home", tab)}
        ${tabButton("discover", "#/discover", "🔎", "Discover", tab)}
        ${tabButton("notifications", "#/notifications", "🔔", "Alerts", tab, unreadCount)}
        ${tabButton("profile", "#/profile", "👤", "Profile", tab)}
      </nav>`
    : "";
  const leading = back
    ? `<button class="iconbtn" id="backBtn" aria-label="Back">‹</button>`
    : "";
  app.innerHTML = `
    <header class="appbar">
      ${leading}
      <h1 class="appbar__title">${escapeHtml(title)}</h1>
      <span class="appbar__spacer"></span>
    </header>
    <main class="content ${tab ? "content--tabbed" : ""}">${content}</main>
    ${nav}`;
  if (back) document.getElementById("backBtn").addEventListener("click", () => history.back());
}

function tabButton(id, hash, icon, label, active, badge = 0) {
  const badgeHtml = badge > 0 ? `<span class="tabbar__badge">${badge > 99 ? "99+" : badge}</span>` : "";
  return `<a class="tabbar__item ${active === id ? "is-active" : ""}" href="${hash}">
      <span class="tabbar__icon">${icon}${badgeHtml}</span>
      <span class="tabbar__label">${label}</span>
    </a>`;
}

function skeleton(rows = 3) {
  return `<div class="stack">${Array.from({ length: rows }).map(() => `<div class="skeleton-card"></div>`).join("")}</div>`;
}

function emptyState(icon, title, description) {
  return `<div class="empty">
      <div class="empty__icon">${icon}</div>
      <h2 class="empty__title">${escapeHtml(title)}</h2>
      <p class="empty__desc">${escapeHtml(description)}</p>
    </div>`;
}

function renderError(err) {
  const message = err instanceof ApiError ? err.message : "Something went wrong.";
  shell(null, "Error", `<div class="alert alert--error">${escapeHtml(message)}</div>
    <button class="btn btn--primary" onclick="location.reload()">Reload</button>`);
}

// ---------------------------------------------------------------------------
// Auth screens
// ---------------------------------------------------------------------------
function authLayout(inner) {
  app.innerHTML = `<div class="auth">
      <div class="auth__brand"><span class="auth__logo">⚽</span><h1>Kawwer</h1>
      <p class="auth__tag">Organize your football night, calmly.</p></div>
      ${inner}
    </div>`;
}

function renderLogin() {
  authLayout(`
    <form class="card auth__card" id="loginForm">
      <h2>Welcome back</h2>
      <label class="field"><span>Username or email</span>
        <input name="usernameOrEmail" autocomplete="username" required></label>
      <label class="field"><span>Password</span>
        <input name="password" type="password" autocomplete="current-password" required></label>
      <button class="btn btn--primary btn--block" type="submit">Sign in</button>
      <p class="auth__switch">New here? <a href="#/register">Create an account</a></p>
    </form>`);
  const form = document.getElementById("loginForm");
  form.addEventListener("submit", async (e) => {
    e.preventDefault();
    const btn = form.querySelector("button");
    btn.disabled = true; btn.textContent = "Signing in…";
    try {
      const data = new FormData(form);
      const res = await api.login(data.get("usernameOrEmail").trim(), data.get("password"));
      auth.save(res);
      navigate("#/");
      refreshUnread();
    } catch (err) {
      toast(err.message || "Sign in failed.", "error");
      btn.disabled = false; btn.textContent = "Sign in";
    }
  });
}

function renderRegister() {
  authLayout(`
    <form class="card auth__card" id="regForm">
      <h2>Create your account</h2>
      <div class="grid2">
        <label class="field"><span>First name</span><input name="firstName" required></label>
        <label class="field"><span>Last name</span><input name="lastName" required></label>
      </div>
      <label class="field"><span>Username</span><input name="username" autocomplete="username" required></label>
      <label class="field"><span>Email</span><input name="email" type="email" autocomplete="email" required></label>
      <label class="field"><span>Password</span>
        <input name="password" type="password" autocomplete="new-password" minlength="8" required></label>
      <button class="btn btn--primary btn--block" type="submit">Create account</button>
      <p class="auth__switch">Already have an account? <a href="#/login">Sign in</a></p>
    </form>`);
  const form = document.getElementById("regForm");
  form.addEventListener("submit", async (e) => {
    e.preventDefault();
    const btn = form.querySelector("button");
    btn.disabled = true; btn.textContent = "Creating…";
    try {
      const d = new FormData(form);
      const res = await api.register({
        username: d.get("username").trim(),
        email: d.get("email").trim(),
        password: d.get("password"),
        firstName: d.get("firstName").trim(),
        lastName: d.get("lastName").trim(),
      });
      auth.save(res);
      navigate("#/");
    } catch (err) {
      toast(err.message || "Registration failed.", "error");
      btn.disabled = false; btn.textContent = "Create account";
    }
  });
}

// ---------------------------------------------------------------------------
// Home
// ---------------------------------------------------------------------------
async function renderHome() {
  const user = auth.user;
  shell("home", "Kawwer", `<div class="stack" id="homeBody">
      <p class="greeting">Hi, ${escapeHtml(capitalize(user?.firstName) || "player")} 👋</p>
      ${skeleton(2)}</div>`);

  const [upcoming, dashboard] = await Promise.all([
    api.getUpcoming().catch(() => []),
    api.getDashboard().catch(() => []),
  ]);

  const body = document.getElementById("homeBody");
  const pushCard = await pushPromptCard();
  const next = upcoming[0];
  const rest = upcoming.slice(1);

  body.innerHTML = `
    <p class="greeting">Hi, ${escapeHtml(capitalize(user?.firstName) || "player")} 👋</p>
    ${pushCard}
    ${next ? nextMatchHero(next) : emptyState("⚽", "No upcoming matches", "When you accept an invitation, your next game shows up here.")}
    ${rest.length ? `<h2 class="section">Upcoming</h2>${rest.map(matchCard).join("")}` : ""}
    ${dashboard.length ? `<h2 class="section">You're organizing</h2>${dashboard.map(dashboardCard).join("")}` : ""}
  `;
  wireMatchLinks(body);
  wirePushCard(body);
}

function nextMatchHero(m) {
  return `<a class="card hero match-link" data-id="${m.id}">
      <div class="hero__top"><span class="pill">Next match</span>
        <span class="pill pill--ghost">${escapeHtml(m.playersLabel ?? `${(m.acceptedCount ?? 0) + 1}/${m.maxPlayers} players`)}</span></div>
      <h2 class="hero__title">${escapeHtml(m.title)}</h2>
      <p class="hero__meta">${escapeHtml(whenLabel(m.matchDate, m.startTime))}</p>
      <p class="hero__meta">📍 ${escapeHtml(m.field?.name ?? "")}</p>
    </a>`;
}

function matchCard(m) {
  return `<a class="card row match-link" data-id="${m.id}">
      <div class="datechip"><span class="datechip__day">${dayNumber(m.matchDate)}</span>
        <span class="datechip__mon">${monthShort(m.matchDate)}</span></div>
      <div class="row__body"><h3>${escapeHtml(m.title)}</h3>
        <p class="muted">${escapeHtml(shortTime(m.startTime))} · ${escapeHtml(m.field?.name ?? "")}</p></div>
      <div class="row__end"><span class="pill pill--ghost">${(m.acceptedCount ?? 0) + 1}/${m.maxPlayers}</span></div>
    </a>`;
}

function dashboardCard(d) {
  return `<a class="card row match-link" data-id="${d.matchId}">
      <div class="datechip datechip--alt"><span class="datechip__day">${dayNumber(d.matchDate)}</span>
        <span class="datechip__mon">${monthShort(d.matchDate)}</span></div>
      <div class="row__body"><h3>${escapeHtml(d.title)}</h3>
        <p class="muted">${escapeHtml(shortTime(d.startTime))} · ${escapeHtml(d.fieldName)}</p>
        <p class="muted small">✅ ${d.acceptedCount} · ⏳ ${d.waitingCount} · 💭 ${d.thinkingCount}</p></div>
      <div class="row__end"><span class="chev">›</span></div>
    </a>`;
}

// ---------------------------------------------------------------------------
// Push prompt card (shown on Home + Profile)
// ---------------------------------------------------------------------------
async function pushPromptCard() {
  if (!push.pushSupported()) return "";
  let enabled = false;
  try { enabled = await push.isEnabled(); } catch { /* ignore */ }
  if (enabled) return "";

  const reason = push.blockedReason();
  if (reason === "ios-needs-install") {
    return `<div class="card notice" id="pushCard">
        <div class="notice__icon">📲</div>
        <div><h3>Turn on match alerts</h3>
        <p class="muted">On iPhone, tap the <b>Share</b> button in Safari, then <b>“Add to Home Screen”</b>. Open Kawwer from your Home Screen to enable notifications.</p></div>
      </div>`;
  }
  if (reason === "denied") {
    return `<div class="card notice" id="pushCard">
        <div class="notice__icon">🔕</div>
        <div><h3>Notifications are blocked</h3>
        <p class="muted">Enable notifications for Kawwer in your browser/site settings to get match alerts.</p></div>
      </div>`;
  }
  if (reason) return ""; // unsupported etc.
  return `<div class="card notice" id="pushCard">
      <div class="notice__icon">🔔</div>
      <div class="notice__body"><h3>Turn on match alerts</h3>
      <p class="muted">Get notified about invitations, waiting-list spots and payments.</p></div>
      <button class="btn btn--primary" id="enablePushBtn">Enable</button>
    </div>`;
}

function wirePushCard(scope) {
  const btn = scope.querySelector("#enablePushBtn");
  if (!btn) return;
  btn.addEventListener("click", async () => {
    btn.disabled = true; btn.textContent = "Enabling…";
    try {
      const res = await push.enable();
      if (res.ok) {
        toast("Notifications enabled ✅", "success");
        document.getElementById("pushCard")?.remove();
      } else {
        toast(pushFailureMessage(res.reason), "error");
        btn.disabled = false; btn.textContent = "Enable";
      }
    } catch (err) {
      toast(err.message || "Could not enable notifications.", "error");
      btn.disabled = false; btn.textContent = "Enable";
    }
  });
}

// The iOS Safari "Share" glyph (tray with an up-arrow), inline so it renders on every platform.
function shareIconSvg() {
  return `<svg class="ios-share" viewBox="0 0 24 24" width="17" height="17" aria-hidden="true">
      <path d="M12 3l4 4-1.4 1.4L13 6.8V15h-2V6.8L9.4 8.4 8 7l4-4z" fill="currentColor"/>
      <path d="M6 10h2v9h8v-9h2v9a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2v-9z" fill="currentColor"/>
    </svg>`;
}

function pushFailureMessage(reason) {
  switch (reason) {
    case "ios-needs-install": return "On iPhone, add Kawwer to your Home Screen first, then open it from there.";
    case "denied": return "Notifications are blocked. Enable them in your site settings.";
    case "dismissed": return "Notification permission was dismissed.";
    case "server-not-configured": return "Push isn't configured on the server yet.";
    case "unsupported": return "This browser doesn't support push notifications.";
    default: return "Could not enable notifications.";
  }
}

// ---------------------------------------------------------------------------
// Match details
// ---------------------------------------------------------------------------
async function renderMatch(id) {
  shell(null, "Match", skeleton(3), { back: true });
  const [m, participants] = await Promise.all([api.getMatch(id), api.getParticipants(id)]);
  const myId = auth.user?.id;
  const me = participants.find((p) => p.user?.id === myId);
  const isOrganizer = m.organizerId === myId;

  const main = document.querySelector("main.content");
  main.innerHTML = `
    <div class="card">
      <h2 class="match__title">${escapeHtml(m.title)}</h2>
      <p class="match__meta">🗓️ ${escapeHtml(whenLabel(m.matchDate, m.startTime))} · ${m.durationMinutes} min</p>
      <p class="match__meta">📍 ${escapeHtml(m.field?.name ?? "")}${m.field?.address ? " — " + escapeHtml(m.field.address) : ""}</p>
      <div class="chips">
        <span class="pill">${(m.acceptedCount ?? 0) + 1}/${m.maxPlayers} players</span>
        <span class="pill pill--ghost">${escapeHtml(statusText(m.status))}</span>
        <span class="pill pill--ghost">${escapeHtml(money(m.sharePerPlayer))} / player</span>
      </div>
      ${m.description ? `<p class="match__desc">${escapeHtml(m.description)}</p>` : ""}
    </div>
    <div id="matchActions">${matchActions(m, me, isOrganizer)}</div>
    <h2 class="section">Players <span class="muted">(${participants.filter(p => p.status === "Accepted").length + 1})</span></h2>
    <div class="stack">
      ${organizerRow(m)}
      ${participants.map(participantRow).join("")}
    </div>`;
  wireMatchActions(main, id);
}

function statusText(s) {
  return { Published: "Open", Full: "Full", Playing: "Live", Finished: "Finished", Cancelled: "Cancelled", Draft: "Draft" }[s] || s;
}

function matchActions(m, me, isOrganizer) {
  if (m.status === "Cancelled") return `<div class="alert alert--error">This match was cancelled.</div>`;
  if (isOrganizer) return `<div class="alert">You're the organizer of this match.</div>`;
  if (!me) return "";
  if (me.status === "Invited" || me.status === "Seen" || me.status === "Thinking") {
    return `<div class="actionbar">
        <button class="btn btn--primary btn--block" data-act="accept">Accept</button>
        <button class="btn btn--danger btn--block" data-act="decline">Decline</button>
      </div>`;
  }
  if (me.status === "Accepted") {
    return `<div class="alert alert--success">You're in for this match. ⚽</div>
      <button class="btn btn--ghost btn--block" data-act="leave">Leave match</button>`;
  }
  if (me.status === "WaitingList") {
    return `<div class="alert">You're on the waiting list${me.waitingListPosition ? ` (#${me.waitingListPosition})` : ""}.</div>
      <button class="btn btn--ghost btn--block" data-act="leave">Leave waiting list</button>`;
  }
  if (me.status === "Declined") {
    return `<div class="alert">You declined this invitation.</div>
      <button class="btn btn--primary btn--block" data-act="accept">Change to Accept</button>`;
  }
  return "";
}

function organizerRow(m) {
  const o = m.organizer || {};
  return `<div class="card row">
      <div class="avatar avatar--org">${escapeHtml(initials(o))}</div>
      <div class="row__body"><h3>${escapeHtml(fullName(o))}</h3><p class="muted">Organizer</p></div>
      <span class="pill pill--org">Host</span>
    </div>`;
}

function participantRow(p) {
  const u = p.user || {};
  const cls = { Accepted: "ok", Declined: "no", WaitingList: "wait" }[p.status] || "neutral";
  return `<div class="card row">
      <div class="avatar">${escapeHtml(initials(u))}</div>
      <div class="row__body"><h3>${escapeHtml(fullName(u))}</h3><p class="muted">@${escapeHtml(u.username ?? "")}</p></div>
      <span class="pill pill--${cls}">${escapeHtml(participantStatusLabel(p))}</span>
    </div>`;
}

function wireMatchActions(scope, id) {
  scope.querySelectorAll("[data-act]").forEach((btn) => {
    btn.addEventListener("click", async () => {
      const act = btn.dataset.act;
      scope.querySelectorAll("[data-act]").forEach((b) => (b.disabled = true));
      try {
        if (act === "accept") { await api.respond(id, true); toast("You're in! ⚽", "success"); }
        else if (act === "decline") { await api.respond(id, false); toast("Invitation declined.", "info"); }
        else if (act === "leave") { await api.leave(id); toast("You left the match.", "info"); }
        await renderMatch(id);
      } catch (err) {
        toast(err.message || "Action failed.", "error");
        scope.querySelectorAll("[data-act]").forEach((b) => (b.disabled = false));
      }
    });
  });
}

// ---------------------------------------------------------------------------
// Notifications
// ---------------------------------------------------------------------------
async function renderNotifications() {
  shell("notifications", "Notifications", skeleton(4));
  const page = await api.getNotifications(1, 40);
  const items = page.items || [];
  unreadCount = items.filter((n) => !n.isRead).length;

  const main = document.querySelector("main.content");
  if (!items.length) {
    main.innerHTML = emptyState("🔔", "No notifications yet", "Invitations, payments and match updates will appear here.");
    updateTabBadge();
    return;
  }
  main.innerHTML = `
    <div class="listhead">
      <span class="muted">${unreadCount} unread</span>
      <button class="btn btn--text" id="markAll">Mark all read</button>
    </div>
    <div class="stack">${items.map(notificationRow).join("")}</div>`;

  main.querySelector("#markAll").addEventListener("click", async () => {
    try { await api.markAllRead(); toast("All caught up ✅", "success"); await renderNotifications(); }
    catch (err) { toast(err.message, "error"); }
  });
  main.querySelectorAll("[data-notif]").forEach(wireNotificationRow);
  updateTabBadge();
}

function isInvitation(n) {
  return n.category === "Invitation" && n.relatedMatchId &&
    (n.title || "").toLowerCase() === "new match invitation";
}

function notificationRow(n) {
  const invite = isInvitation(n);
  return `<div class="card notif ${n.isRead ? "" : "notif--unread"}" data-notif="${n.id}"
      data-match="${n.relatedMatchId ?? ""}" data-read="${n.isRead}">
      <div class="notif__icon">${notificationIcon(n.category)}</div>
      <div class="notif__body">
        <h3>${escapeHtml(n.title)}</h3>
        <p class="muted">${escapeHtml(n.message)}</p>
        <p class="muted small">${escapeHtml(relativeTime(n.createdAt))}</p>
        ${invite ? `<div class="actionbar actionbar--sm">
            <button class="btn btn--primary btn--sm" data-inv="accept" data-match="${n.relatedMatchId}">Accept</button>
            <button class="btn btn--danger btn--sm" data-inv="decline" data-match="${n.relatedMatchId}">Decline</button>
          </div>` : ""}
      </div>
      ${n.isRead ? "" : `<span class="dot"></span>`}
    </div>`;
}

function wireNotificationRow(row) {
  const id = row.dataset.notif;
  const matchId = row.dataset.match;

  row.querySelectorAll("[data-inv]").forEach((btn) => {
    btn.addEventListener("click", async (e) => {
      e.stopPropagation();
      const accept = btn.dataset.inv === "accept";
      row.querySelectorAll("[data-inv]").forEach((b) => (b.disabled = true));
      try {
        await api.respond(btn.dataset.match, accept);
        await api.markNotificationRead(id).catch(() => {});
        toast(accept ? "You're in! ⚽" : "Invitation declined.", accept ? "success" : "info");
        await renderNotifications();
      } catch (err) {
        toast(err.message || "Action failed.", "error");
        row.querySelectorAll("[data-inv]").forEach((b) => (b.disabled = false));
      }
    });
  });

  row.addEventListener("click", async () => {
    if (row.dataset.read !== "true") {
      try { await api.markNotificationRead(id); } catch { /* ignore */ }
    }
    if (matchId) navigate(`#/match/${matchId}`);
  });
}

// ---------------------------------------------------------------------------
// Discover
// ---------------------------------------------------------------------------
async function renderDiscover() {
  shell("discover", "Discover", skeleton(3));
  let coords = null;
  try { coords = await getPosition(); } catch { /* location optional */ }

  const page = await api.discover(coords?.latitude, coords?.longitude, coords ? 25 : null).catch(() => ({ items: [] }));
  const items = page.items || [];
  const main = document.querySelector("main.content");
  if (!items.length) {
    main.innerHTML = emptyState("🔎", "No public matches nearby", "Public matches open for join requests will appear here.");
    return;
  }
  main.innerHTML = `<div class="stack">${items.map(discoverCard).join("")}</div>`;
  main.querySelectorAll("[data-join]").forEach((btn) => {
    btn.addEventListener("click", async () => {
      btn.disabled = true; btn.textContent = "Requesting…";
      try {
        const joined = await api.joinPublicMatch(btn.dataset.join);
        toast(joined ? "You joined the match! ⚽" : "Join request sent to the organizer.", "success");
        await renderDiscover();
      } catch (err) {
        toast(err.message || "Could not join.", "error");
        btn.disabled = false; btn.textContent = "Join";
      }
    });
  });
}

function discoverCard(d) {
  const spots = d.availableSpots ?? 0;
  return `<div class="card">
      <div class="row__body">
        <h3>${escapeHtml(d.title)}</h3>
        <p class="muted">${escapeHtml(whenLabel(d.matchDate, d.startTime))}</p>
        <p class="muted small">📍 ${escapeHtml(d.fieldName)}${d.distanceKm != null ? ` · ${d.distanceKm.toFixed(1)} km` : ""}</p>
      </div>
      <div class="discover__foot">
        <span class="pill pill--ghost">${d.acceptedCount}/${d.acceptedCount + spots} · ${spots} left</span>
        ${d.isJoined ? `<span class="pill pill--ok">Joined</span>`
          : spots > 0 ? `<button class="btn btn--primary btn--sm" data-join="${d.id}">Join</button>`
          : `<span class="pill">Full</span>`}
      </div>
    </div>`;
}

function getPosition() {
  return new Promise((resolve, reject) => {
    if (!navigator.geolocation) return reject(new Error("no geo"));
    navigator.geolocation.getCurrentPosition(
      (pos) => resolve(pos.coords),
      reject,
      { timeout: 6000, maximumAge: 300000 }
    );
  });
}

// ---------------------------------------------------------------------------
// Profile / Settings
// ---------------------------------------------------------------------------
async function renderProfile() {
  const user = auth.user || {};
  shell("profile", "Profile", `<div class="stack" id="profileBody">
      <div class="card profilehead">
        <div class="avatar avatar--lg">${escapeHtml(initials(user))}</div>
        <div><h2>${escapeHtml(fullName(user))}</h2><p class="muted">@${escapeHtml(user.username ?? "")}</p></div>
      </div>
      ${skeleton(1)}
      <div id="pushSettings"></div>
      <button class="btn btn--ghost btn--block" id="logoutBtn">Sign out</button>
    </div>`);

  // Reputation / stats
  api.getMyStatistics().then((s) => {
    const stats = `<div class="card stats">
        <div class="stat"><span class="stat__num">${s.matchesPlayed}</span><span class="stat__lbl">Played</span></div>
        <div class="stat"><span class="stat__num">${s.matchesOrganized}</span><span class="stat__lbl">Organized</span></div>
        <div class="stat"><span class="stat__num">${Math.round((s.attendanceRate || 0) * 100)}%</span><span class="stat__lbl">Attendance</span></div>
        <div class="stat"><span class="stat__num">★</span><span class="stat__lbl">${escapeHtml(BADGE_LABEL[user.reliabilityBadge] || "Reliable")}</span></div>
      </div>`;
    const sk = document.querySelector("#profileBody .skeleton-card");
    if (sk) sk.outerHTML = stats;
  }).catch(() => {
    document.querySelector("#profileBody .skeleton-card")?.remove();
  });

  await renderPushSettings();

  document.getElementById("logoutBtn").addEventListener("click", async () => {
    try { await push.disable().catch(() => {}); } catch { /* ignore */ }
    try { if (auth.refreshToken) await api.logout(auth.refreshToken); } catch { /* ignore */ }
    auth.clear();
    unreadCount = 0;
    navigate("#/login");
  });
}

async function renderPushSettings() {
  const host = document.getElementById("pushSettings");
  if (!push.pushSupported()) {
    host.innerHTML = `<div class="card"><h3>Notifications</h3>
      <p class="muted">This browser doesn't support push notifications.</p></div>`;
    return;
  }
  let enabled = false;
  try { enabled = await push.isEnabled(); } catch { /* ignore */ }
  const reason = push.blockedReason();

  let controls;
  if (enabled) {
    controls = `<div class="setting">
        <div><b>Push notifications</b><p class="muted">On — you'll get match alerts.</p></div>
        <button class="btn btn--ghost btn--sm" id="pushToggle" data-on="1">Turn off</button>
      </div>`;
  } else if (reason === "ios-needs-install") {
    controls = `<div class="ios-steps">
        <p><b>Add Kawwer to your Home Screen to enable push:</b></p>
        <ol>
          <li>Tap the <b>Share</b> icon ${shareIconSvg()} in Safari.</li>
          <li>Choose <b>Add to Home Screen</b>.</li>
          <li>Open <b>Kawwer</b> from your Home Screen, then come back here.</li>
        </ol>
      </div>`;
  } else if (reason === "denied") {
    controls = `<p class="muted">Notifications are blocked. Enable them for this site in your browser settings, then reload.</p>`;
  } else {
    controls = `<div class="setting">
        <div><b>Push notifications</b><p class="muted">Off</p></div>
        <button class="btn btn--primary btn--sm" id="pushToggle" data-on="0">Turn on</button>
      </div>`;
  }
  host.innerHTML = `<div class="card"><h3>Notifications</h3>${controls}</div>`;

  const toggle = host.querySelector("#pushToggle");
  if (toggle) {
    toggle.addEventListener("click", async () => {
      toggle.disabled = true;
      const turningOn = toggle.dataset.on === "0";
      try {
        const res = turningOn ? await push.enable() : await push.disable();
        if (res.ok) toast(turningOn ? "Notifications enabled ✅" : "Notifications turned off.", turningOn ? "success" : "info");
        else toast(pushFailureMessage(res.reason), "error");
      } catch (err) {
        toast(err.message || "Failed.", "error");
      }
      await renderPushSettings();
    });
  }
}

// ---------------------------------------------------------------------------
// Cross-cutting
// ---------------------------------------------------------------------------
function wireMatchLinks(scope) {
  scope.querySelectorAll(".match-link").forEach((el) =>
    el.addEventListener("click", () => navigate(`#/match/${el.dataset.id}`)));
}

function updateTabBadge() {
  const icon = document.querySelector('a[href="#/notifications"] .tabbar__icon');
  if (icon) {
    const existing = icon.querySelector(".tabbar__badge");
    if (unreadCount > 0) {
      const text = unreadCount > 99 ? "99+" : String(unreadCount);
      if (existing) existing.textContent = text;
      else icon.insertAdjacentHTML("beforeend", `<span class="tabbar__badge">${text}</span>`);
    } else if (existing) existing.remove();
  }
}

async function refreshUnread() {
  if (!auth.isAuthenticated) return;
  try {
    unreadCount = await api.getUnreadCount();
    updateTabBadge();
  } catch { /* ignore */ }
}

// Service worker tells us a push arrived / was clicked while the app is open.
function listenToServiceWorker() {
  if (!("serviceWorker" in navigator)) return;
  navigator.serviceWorker.addEventListener("message", (event) => {
    const data = event.data || {};
    if (data.type === "push-received") {
      refreshUnread();
      const hash = currentHash();
      if (hash === "#/notifications") renderNotifications();
    } else if (data.type === "navigate" && data.url) {
      navigate(data.url);
    }
  });
}

// ---------------------------------------------------------------------------
// Boot
// ---------------------------------------------------------------------------
async function boot() {
  if ("serviceWorker" in navigator) {
    try { await navigator.serviceWorker.register("/sw.js"); } catch (e) { console.warn("SW registration failed", e); }
  }
  listenToServiceWorker();
  window.addEventListener("hashchange", router);
  await router();
  refreshUnread();
}

boot();
