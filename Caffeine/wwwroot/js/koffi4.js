(() => {
    const policy = window.KoffiExperience;
    const key = `koffi4:${document.body.dataset.clientKey || 'unknown'}`;
    let local = {};
    try { local = JSON.parse(localStorage.getItem(key) || '{}') || {}; } catch {}
    const save = () => { try { localStorage.setItem(key, JSON.stringify(local)); } catch {} };
    const tokenForm = document.getElementById('experience-token');
    const acknowledge = async (kind, version = '') => {
        if (!tokenForm) return;
        const data = new FormData(tokenForm); data.set('kind', kind); data.set('version', version);
        try { await fetch(tokenForm.action, { method: 'POST', body: data, credentials: 'same-origin' }); } catch {}
    };
    const info = document.getElementById('info-dialog'), tutorial = document.getElementById('tutorial-dialog'), release = document.getElementById('release-dialog');
    const open = dialog => { if (dialog && !document.querySelector('dialog[open]')) { dialog.showModal(); dialog.querySelector('button, a')?.focus(); } };
    const close = dialog => { if (dialog?.open) dialog.close(); };
    document.querySelectorAll('[data-info]').forEach(button => button.addEventListener('click', () => {
        document.getElementById('info-content').textContent = button.dataset.info; open(info);
    }));
    document.querySelectorAll('[data-close]').forEach(button => button.addEventListener('click', () => close(button.closest('dialog'))));
    document.querySelectorAll('dialog').forEach(dialog => dialog.addEventListener('click', event => {
        if (event.target === dialog) { const b = dialog.getBoundingClientRect(); if (event.clientX < b.left || event.clientX > b.right || event.clientY < b.top || event.clientY > b.bottom) close(dialog); }
    }));
    let step = 0; const steps = Array.from(document.querySelectorAll('[data-tutorial-step]'));
    const showStep = () => { steps.forEach((s, i) => s.hidden = i !== step);
        document.getElementById('tutorial-progress').textContent = `${step + 1} / ${steps.length}`;
        const next = document.querySelector('[data-tutorial-next]'); next.textContent = step === steps.length - 1 ? next.dataset.done : next.dataset.next;
    };
    const startTutorial = () => { step = 0; showStep(); open(tutorial); };
    document.querySelectorAll('[data-tutorial-start]').forEach(button => button.addEventListener('click', startTutorial));
    document.querySelector('[data-tutorial-next]')?.addEventListener('click', () => { if (step < steps.length - 1) { step++; showStep(); } else close(tutorial); });
    document.querySelector('[data-tutorial-skip]')?.addEventListener('click', () => close(tutorial));
    tutorial?.addEventListener('close', () => { local.tutorialDone = true; save(); acknowledge('tutorial'); });
    document.querySelector('[data-release-close]')?.addEventListener('click', () => close(release));
    release?.addEventListener('close', () => { local.seenRelease = release.dataset.version; save(); acknowledge('release', release.dataset.version); });
    // One automatic dialog per visit; user-triggered Help can always replay the tutorial.
    if (tutorial?.dataset.auto === 'true' && !local.tutorialDone) startTutorial();
    else if (release?.dataset.auto === 'true' && policy.releaseDue(local.seenRelease, release.dataset.version)) open(release);
    document.querySelector('[data-dismiss-morning]')?.addEventListener('click', event => {
        event.currentTarget.disabled = true; acknowledge('morning');
        local.morningDismissed = document.documentElement.dataset.trackerDay; save();
        document.getElementById('morning-reminder')?.remove();
    });
    if (local.morningDismissed === document.documentElement.dataset.trackerDay) document.getElementById('morning-reminder')?.remove();
    document.querySelector('[data-language]')?.addEventListener('change', event => event.target.form.requestSubmit());
    document.querySelectorAll('form[data-confirm]').forEach(form => form.addEventListener('submit', event => { if (!confirm(form.dataset.confirm)) event.preventDefault(); }));
    document.querySelectorAll('form[data-single-submit]').forEach(form => form.addEventListener('submit', event => {
        if (event.defaultPrevented || !form.checkValidity()) return;
        const button = form.querySelector('button[type=submit]'); if (button) button.disabled = true;
    }));
    if ('serviceWorker' in navigator) window.addEventListener('load', () => navigator.serviceWorker.register('/sw.js').catch(() => {}));
    // Browser install prompt is shown only from a user gesture. Safari gets instructions.
    let installPrompt; const card = document.getElementById('install-card'), instructions = document.getElementById('install-instructions');
    const ios = policy.platform(navigator.userAgent, navigator.maxTouchPoints, navigator.platform) === 'ios';
    const standalone = window.matchMedia('(display-mode: standalone)').matches || navigator.standalone === true;
    const now = Date.now();
    local.install = policy.visit(local.install || {}, document.documentElement.dataset.trackerDay); save();
    const showInstall = force => {
        if (standalone || !card) return;
        if (!force && (document.querySelector('dialog[open]') || !policy.installDue(local.install, now, standalone, ios || !!installPrompt))) return;
        instructions.textContent = ios ? instructions.dataset.ios : instructions.dataset.browser;
        card.querySelector('[data-install]').hidden = !installPrompt; card.hidden = false;
    };
    window.addEventListener('beforeinstallprompt', event => { event.preventDefault(); installPrompt = event; showInstall(false); });
    document.querySelector('[data-install]')?.addEventListener('click', async () => { if (!installPrompt) return; await installPrompt.prompt(); await installPrompt.userChoice; installPrompt = null; card.hidden = true; });
    document.querySelector('[data-install-later]')?.addEventListener('click', () => { local.install = policy.later(local.install, Date.now()); save(); card.hidden = true; });
    document.querySelector('[data-install-never]')?.addEventListener('click', () => { local.install = policy.never(local.install); save(); card.hidden = true; });
    window.addEventListener('appinstalled', () => { card.hidden = true; installPrompt = null; });
    document.querySelector('[data-install-help]')?.addEventListener('click', () => showInstall(true));
    if (ios) showInstall(false);
})();
