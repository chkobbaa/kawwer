// Small rendering helpers. The app renders with template strings, so escapeHtml is used on every
// piece of user-supplied text to avoid HTML injection.

export function escapeHtml(value) {
  if (value == null) return "";
  return String(value)
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;")
    .replace(/'/g, "&#39;");
}

export function capitalize(value) {
  if (!value) return "";
  const v = String(value).trim();
  return v.charAt(0).toUpperCase() + v.slice(1);
}

export function fullName(person) {
  if (!person) return "";
  return `${capitalize(person.firstName)} ${capitalize(person.lastName)}`.trim();
}

export function initials(person) {
  if (!person) return "?";
  const a = person.firstName?.[0] ?? "";
  const b = person.lastName?.[0] ?? "";
  return (a + b).toUpperCase() || "?";
}

const MONTHS = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
const DAYS = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];

// The API serialises DateOnly as "YYYY-MM-DD" and TimeOnly as "HH:mm:ss".
function parseDate(dateOnly) {
  if (!dateOnly) return null;
  const [y, m, d] = String(dateOnly).split("-").map(Number);
  return new Date(y, m - 1, d);
}

export function shortTime(timeOnly) {
  if (!timeOnly) return "";
  return String(timeOnly).slice(0, 5); // HH:mm
}

export function dayNumber(dateOnly) {
  const d = parseDate(dateOnly);
  return d ? String(d.getDate()).padStart(2, "0") : "--";
}

export function monthShort(dateOnly) {
  const d = parseDate(dateOnly);
  return d ? MONTHS[d.getMonth()].toUpperCase() : "";
}

export function whenLabel(dateOnly, timeOnly) {
  const d = parseDate(dateOnly);
  if (!d) return "";
  return `${DAYS[d.getDay()]} ${String(d.getDate()).padStart(2, "0")} ${MONTHS[d.getMonth()]} · ${shortTime(timeOnly)}`;
}

export function relativeTime(iso) {
  const then = new Date(iso).getTime();
  if (Number.isNaN(then)) return "";
  const secs = Math.round((Date.now() - then) / 1000);
  if (secs < 60) return "just now";
  const mins = Math.round(secs / 60);
  if (mins < 60) return `${mins}m ago`;
  const hrs = Math.round(mins / 60);
  if (hrs < 24) return `${hrs}h ago`;
  const days = Math.round(hrs / 24);
  if (days < 7) return `${days}d ago`;
  return new Date(iso).toLocaleDateString();
}

export function money(amount) {
  const n = Number(amount) || 0;
  return `${n % 1 === 0 ? n : n.toFixed(2)} TND`;
}

export function notificationIcon(category) {
  switch (category) {
    case "Match": return "⚽";
    case "Invitation": return "✉️";
    case "Payment": return "💰";
    case "LiveMatch": return "📍";
    case "Friend": return "🤝";
    case "Team": return "👥";
    case "WaitingList": return "⏳";
    default: return "🔔";
  }
}

let toastTimer = null;
export function toast(message, type = "info") {
  let host = document.getElementById("toast");
  if (!host) {
    host = document.createElement("div");
    host.id = "toast";
    document.body.appendChild(host);
  }
  host.textContent = message;
  host.className = `toast toast--${type} toast--show`;
  clearTimeout(toastTimer);
  toastTimer = setTimeout(() => {
    host.className = "toast";
  }, 3200);
}
