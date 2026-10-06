// Network-only for private pages: never cache accounts, logs or reset tokens.
self.addEventListener('install', () => self.skipWaiting());
self.addEventListener('activate', event => event.waitUntil((async () => {
    // Retire any previous notification permission/subscription locally.
    const subscription = await self.registration.pushManager?.getSubscription().catch(() => null);
    if (subscription) await subscription.unsubscribe().catch(() => false);
    await self.clients.claim();
})()));
// Koffi 4.0 deliberately has no push or notificationclick handlers.
