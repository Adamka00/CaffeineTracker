(() => {
    const normalize = text =>
        text
            .normalize('NFD')
            .replace(/[\u0300-\u036f]/g, '')
            .toLocaleLowerCase()
            .trim();

    document
        .querySelectorAll('[data-drink-search]')
        .forEach(input => {
            const select =
                document.getElementById(
                    input.dataset.drinkSearch);

            if (!select)
                return;

            const options =
                Array.from(select.options)
                    .map(option => ({
                        option,
                        name: normalize(
                            option.textContent)
                    }));

            let chosen = select.value;

            const status =
                document.createElement('p');

            status.className =
                'text-xs text-slate-400 mt-2';

            status.setAttribute(
                'aria-live',
                'polite');

            input.after(status);

            select.addEventListener(
                'change',
                () => {
                    chosen = select.value;
                });

            input.addEventListener(
                'input',
                () => {
                    const query =
                        normalize(input.value);

                    const matches =
                        options.filter(x =>
                            !x.option.value ||
                            x.name.includes(query) ||
                            x.option.value === chosen);

                    select.replaceChildren(
                        ...matches.map(x =>
                            x.option));

                    select.value = chosen;

                    status.textContent =
                        options.filter(x =>
                            x.option.value &&
                            x.name.includes(query))
                            .length
                        + ' '
                        + (input.dataset.results || '');
                });
        });
})();