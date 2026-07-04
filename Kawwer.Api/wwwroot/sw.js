// Kawwer PWA service worker: offline app shell + Web Push display and click handling.
// Bump CACHE when shipping new front-end assets so clients pick them up.
const CACHE = "kawwer-pwa-v1";
const SHELL = [
  "/",
  "/index.html",
  "/css/styles.css",
  "/js/app.js",
  "/js/api.js",
  "/js/push.js",
  "/js/ui.js",
  "/manifest.webmanifest",
  "/icons/icon-192.png",
  "/icons/icon-512.png",
];

self.addEventListener("install", (event) => {
  event.waitUntil(
    caches.open(CACHE).then((cache) => cache.addAll(SHELL)).then(() => self.skipWaiting())
  );
});

self.addEventListener("activate", (event) => {
  event.waitUntil(
    caches.keys()
      .then((keys) => Promise.all(keys.filter((k) => k !== CACHE).map((k) => caches.delete(k))))
      .then(() => self.clients.claim())
  );
});

// Never cache API/realtime traffic; serve the app shell for navigations; cache-first for assets.
self.addEventListener("fetch", (event) => {
  const req = event.request;
  if (req.method !== "GET") return;

  const url = new URL(req.url);
  if (url.origin !== self.location.origin) return;
  if (url.pathname.startsWith("/api") || url.pathname.startsWith("/hubs")) return;

  if (req.mode === "navigate") {
    event.respondWith(
      fetch(req).catch(() => caches.match("/index.html"))
    );
    return;
  }

  event.respondWith(
    caches.match(req).then((cached) =>
      cached ||
      fetch(req).then((res) => {
        // Runtime-cache successful same-origin GETs so the shell keeps working offline.
        if (res.ok && res.type === "basic") {
          const copy = res.clone();
          caches.open(CACHE).then((cache) => cache.put(req, copy));
        }
        return res;
      }).catch(() => cached)
    )
  );
});

// A Web Push arrived. The payload is the JSON our backend WebPushNotificationSender produced.
self.addEventListener("push", (event) => {
  let payload = {};
  try {
    payload = event.data ? event.data.json() : {};
  } catch {
    payload = { title: "Kawwer", body: event.data ? event.data.text() : "" };
  }

  const title = payload.title || "Kawwer";
  const data = payload.data || {};
  const options = {
    body: payload.body || "",
    icon: "/icons/icon-192.png",
    badge: "/icons/badge-72.png",
    tag: data.matchId || data.category || "kawwer",
    renotify: true,
    data,
  };

  event.waitUntil(
    Promise.all([
      self.registration.showNotification(title, options),
      notifyClients({ type: "push-received", data }),
    ])
  );
});

self.addEventListener("notificationclick", (event) => {
  event.notification.close();
  const data = event.notification.data || {};
  const target = data.matchId ? `/#/match/${data.matchId}` : "/#/notifications";

  event.waitUntil(
    self.clients.matchAll({ type: "window", includeUncontrolled: true }).then((clients) => {
      for (const client of clients) {
        if ("focus" in client) {
          client.focus();
          client.postMessage({ type: "navigate", url: target.replace(/^\//, "") });
          return;
        }
      }
      return self.clients.openWindow(target);
    })
  );
});

async function notifyClients(message) {
  const clients = await self.clients.matchAll({ type: "window", includeUncontrolled: true });
  for (const client of clients) client.postMessage(message);
}
