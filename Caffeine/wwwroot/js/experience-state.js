// Pure policy functions shared with Node tests. Local fallbacks are scoped by user/guest hash.
(function (root) {
    const api = {
        releaseDue: (seen, version) => seen !== version,
        installDue: (state, now, standalone, supported) => !standalone && supported && !state.never && (state.visits || 0) >= 3 && (!state.dismissedUntil || now >= state.dismissedUntil),
        visit: (state, day) => state.lastVisit === day ? state : { ...state, lastVisit: day, visits: Math.min(100, (state.visits || 0) + 1) },
        later: (state, now) => ({ ...state, dismissedUntil: now + 7 * 86400000 }),
        never: state => ({ ...state, never: true }),
        platform: (ua, touch, platform) => /iPad|iPhone|iPod/.test(ua) || (platform === 'MacIntel' && touch > 1) ? 'ios' : 'other'
    };
    if (typeof module !== 'undefined' && module.exports) module.exports = api;
    else root.KoffiExperience = api;
})(typeof window === 'undefined' ? globalThis : window);
