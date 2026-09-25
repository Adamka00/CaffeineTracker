(() => {
    const controls = document.getElementById('push-controls');
    const enable = document.getElementById('enable-push');
    const disable = document.getElementById('disable-push');
    const status = document.getElementById('push-status');

    if (!controls || !enable)
        return;

    const supported =
        window.isSecureContext &&
        'serviceWorker' in navigator &&
        'PushManager' in window &&
        'Notification' in window;

    if (!supported) {
        enable.disabled = disable.disabled = true;
        status.textContent = controls.dataset.unsupported;
        return;
    }

    const post = (path, data) =>
        fetch(path, {
            method: 'POST',
            credentials: 'same-origin',
            body: new URLSearchParams({
                ...data,
                __RequestVerificationToken:
                document.querySelector(
                    'input[name="__RequestVerificationToken"]'
                ).value
            })
        });

    const keyBytes = key =>
        Uint8Array.from(
            atob(
                key
                    .replace(/-/g, '+')
                    .replace(/_/g, '/')
                +
                '='.repeat(
                    (4 - key.length % 4) % 4
                )
            ),
            c => c.charCodeAt(0)
        );

    enable.addEventListener(
        'click',
        async () => {
            enable.disabled = true;

            try {
                // Permission is requested directly from this
                // user gesture (required by iOS).
                if (
                    await Notification.requestPermission()
                    !== 'granted'
                ) {
                    throw new Error();
                }

                const registration =
                    await navigator.serviceWorker.register(
                        '/sw.js'
                    );

                await navigator.serviceWorker.ready;

                let subscription =
                    await registration.pushManager
                        .getSubscription();

                if (!subscription) {
                    subscription =
                        await registration.pushManager
                            .subscribe({
                                userVisibleOnly: true,
                                applicationServerKey:
                                    keyBytes(
                                        controls.dataset.key
                                    )
                            });
                }

                const send = sub => {
                    const data = sub.toJSON();

                    return post(
                        '/Notifications/Subscribe',
                        {
                            Endpoint:
                            data.endpoint,

                            P256dh:
                            data.keys.p256dh,

                            Auth:
                            data.keys.auth
                        });
                };

                let response =
                    await send(subscription);

                if (response.status === 409) {
                    await subscription.unsubscribe();

                    subscription =
                        await registration.pushManager
                            .subscribe({
                                userVisibleOnly: true,
                                applicationServerKey:
                                    keyBytes(
                                        controls.dataset.key
                                    )
                            });

                    response =
                        await send(subscription);
                }

                if (!response.ok)
                    throw new Error();

                status.textContent =
                    controls.dataset.ok;
            }
            catch {
                status.textContent =
                    controls.dataset.error;
            }
            finally {
                enable.disabled = false;
            }
        });


    disable.addEventListener(
        'click',
        async () => {
            disable.disabled = true;

            try {
                const registration =
                    await navigator.serviceWorker.ready;

                const subscription =
                    await registration.pushManager
                        .getSubscription();

                if (subscription) {
                    if (
                        !(
                            await post(
                                '/Notifications/Unsubscribe',
                                {
                                    endpoint:
                                    subscription.endpoint
                                })
                        ).ok
                    ) {
                        throw new Error();
                    }

                    await subscription.unsubscribe();
                }

                status.textContent =
                    controls.dataset.off;
            }
            catch {
                status.textContent =
                    controls.dataset.error;
            }
            finally {
                disable.disabled = false;
            }
        });
})();