(() => {
    const start = document.getElementById('scan-start'), stop = document.getElementById('scan-stop'), video = document.getElementById('scan-video'), status = document.getElementById('scan-status');
    if (!start) return;
    let reader, stream, timer, generation = 0, active = false;
    const cleanup = () => {
        active = false; generation++; clearTimeout(timer); reader?.reset(); reader = null;
        (stream?.getTracks() || video.srcObject?.getTracks() || []).forEach(t => t.stop()); stream = null; video.srcObject = null;
        video.hidden = true; stop.hidden = true; start.disabled = false;
    };
    const found = code => {
        if (!/^\d{8,14}$/.test(code) || !active) return;
        document.getElementById('Code').value = code; cleanup();
        document.getElementById('barcode-form').requestSubmit();
    };
    const failure = () => { cleanup(); status.textContent = status.dataset.cameraError; document.getElementById('Code').focus(); };
    start.addEventListener('click', async () => {
        cleanup(); active = true; const attempt = generation; start.disabled = true; stop.hidden = false; video.hidden = false;
        status.textContent = status.dataset.scanning;
        try {
            if (!navigator.mediaDevices?.getUserMedia || !window.isSecureContext) throw new Error('camera');
            // ZXing is self-hosted: supports Safari without native BarcodeDetector.
            if (window.ZXing?.BrowserMultiFormatReader) {
                reader = new ZXing.BrowserMultiFormatReader(); const current = reader;
                await current.decodeFromConstraints({ video: { facingMode: { ideal: 'environment' } }, audio: false }, video,
                    (result) => { if (result && active && attempt === generation) found(result.getText()); });
                if (!active || attempt !== generation) current.reset();
            } else if ('BarcodeDetector' in window) {
                const detector = new BarcodeDetector({ formats: ['ean_13', 'ean_8', 'upc_a', 'itf'] });
                const acquired = await navigator.mediaDevices.getUserMedia({ video: { facingMode: { ideal: 'environment' } }, audio: false });
                if (!active || attempt !== generation) { acquired.getTracks().forEach(t => t.stop()); return; }
                stream = acquired; video.srcObject = stream; await video.play();
                const scan = async () => { if (!active) return; try { const codes = await detector.detect(video); if (codes[0]) found(codes[0].rawValue); } catch {} if (active) timer = setTimeout(scan, 250); }; scan();
            } else throw new Error('decoder');
        } catch { if (attempt === generation) failure(); }
    });
    stop.addEventListener('click', cleanup); window.addEventListener('pagehide', cleanup);
    document.addEventListener('visibilitychange', () => { if (document.hidden) cleanup(); });
})();
