(() => {
    const amount = document.getElementById('product-ml'), caffeine = document.getElementById('product-caffeine'), total = document.getElementById('product-total');
    if (!total) return;
    const update = () => { const n = Number(amount.value) * Number(caffeine.value) / 100;
        total.textContent = amount.value && caffeine.value && Number.isFinite(n) ? `${total.dataset.label}: ${n.toFixed(1)} mg` : ''; };
    amount.addEventListener('input', update); caffeine.addEventListener('input', update); update();
})();
