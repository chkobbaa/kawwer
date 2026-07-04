// Web Push (PWA) subscription helpers. On iOS, push works ONLY when the app has been added to the
// Home Screen and launched standalone (Safari requirement since iOS 16.4), so we gate on that.

import { api } from "./api.js";

export function pushSupported() {
  return "serviceWorker" in navigator && "PushManager" in window && "Notification" in window;
}

export function isIOS() {
  const ua = navigator.userAgent || "";
  const iOSDevice = /iPad|iPhone|iPod/.test(ua);
  // iPadOS 13+ reports as "Macintosh" but exposes touch — treat it as iOS for install guidance.
  const iPadOS = navigator.platform === "MacIntel" && navigator.maxTouchPoints > 1;
  return iOSDevice || iPadOS;
}

export function isStandalone() {
  return (
    window.matchMedia?.("(display-mode: standalone)").matches ||
    window.navigator.standalone === true
  );
}

export function permission() {
  return typeof Notification !== "undefined" ? Notification.permission : "denied";
}

// Converts a base64url VAPID key into the Uint8Array the Push API expects as applicationServerKey.
function urlBase64ToUint8Array(base64String) {
  const padding = "=".repeat((4 - (base64String.length % 4)) % 4);
  const base64 = (base64String + padding).replace(/-/g, "+").replace(/_/g, "/");
  const raw = atob(base64);
  const output = new Uint8Array(raw.length);
  for (let i = 0; i < raw.length; i++) output[i] = raw.charCodeAt(i);
  return output;
}

async function registration() {
  return navigator.serviceWorker.ready;
}

export async function currentSubscription() {
  if (!pushSupported()) return null;
  const reg = await registration();
  return reg.pushManager.getSubscription();
}

export async function isEnabled() {
  return (await currentSubscription()) != null && permission() === "granted";
}

/**
 * Returns why push can't be enabled right now, or null if it can be attempted.
 *  - "unsupported": the browser lacks the Push API.
 *  - "ios-needs-install": iOS Safari — must Add to Home Screen and open the installed app first.
 *  - "denied": the user previously blocked notifications (must re-enable in settings).
 */
export function blockedReason() {
  if (!pushSupported()) return "unsupported";
  if (isIOS() && !isStandalone()) return "ios-needs-install";
  if (permission() === "denied") return "denied";
  return null;
}

/** Runs the full enable flow: permission → subscribe → register with the server. */
export async function enable() {
  const blocked = blockedReason();
  if (blocked) return { ok: false, reason: blocked };

  const { publicKey } = await api.getVapidPublicKey();
  if (!publicKey) return { ok: false, reason: "server-not-configured" };

  const perm = await Notification.requestPermission();
  if (perm !== "granted") return { ok: false, reason: perm === "denied" ? "denied" : "dismissed" };

  const reg = await registration();
  let sub = await reg.pushManager.getSubscription();
  if (!sub) {
    sub = await reg.pushManager.subscribe({
      userVisibleOnly: true,
      applicationServerKey: urlBase64ToUint8Array(publicKey),
    });
  }

  await api.subscribeWebPush(sub.toJSON());
  return { ok: true };
}

/** Unsubscribes locally and tells the server to forget this subscription. */
export async function disable() {
  const sub = await currentSubscription();
  if (!sub) return { ok: true };
  const endpoint = sub.endpoint;
  try {
    await sub.unsubscribe();
  } catch {
    /* ignore — still tell the server */
  }
  try {
    await api.unsubscribeWebPush(endpoint);
  } catch {
    /* best effort */
  }
  return { ok: true };
}
