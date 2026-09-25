self.addEventListener(
    'install',
    event =>
        event.waitUntil(
            self.skipWaiting()
        )
);


self.addEventListener(
    'activate',
    event =>
        event.waitUntil(
            self.clients.claim()
        )
);


// Never cache authenticated pages, tokens or personal data.
self.addEventListener(
    'push',
    event => {
        let data = {};

        try {
            data =
                event.data
                    ? event.data.json()
                    : {};
        }
        catch {
            // Display a safe fallback.
        }

        event.waitUntil(
            self.registration.showNotification(
                data.title || 'Koffi',
                {
                    body:
                        data.body || 'Koffi',

                    icon:
                        '/icon-192.png',

                    badge:
                        '/icon-192.png',

                    tag:
                        data.tag || 'koffi',

                    data:
                        {
                            url:
                                data.url || '/'
                        }
                }
            )
        );
    }
);


self.addEventListener(
    'notificationclick',
    event => {
        event.notification.close();

        const target =
            new URL(
                event.notification.data?.url || '/',
                self.location.origin
            );

        const safe =
            target.origin === self.location.origin
                ? target.href
                : self.location.origin + '/';


        event.waitUntil(
            (async () => {
                const windows =
                    await self.clients.matchAll({
                        type: 'window',
                        includeUncontrolled: true
                    });

                for (const client of windows) {
                    if (
                        new URL(client.url).origin ===
                        self.location.origin
                    ) {
                        await client.navigate(safe);

                        return client.focus();
                    }
                }

                return self.clients.openWindow(safe);
            })()
        );
    }
);