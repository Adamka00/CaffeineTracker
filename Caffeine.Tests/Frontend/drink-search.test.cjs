const { test } = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

// Lightweight DOM boundary for the production script; no browser, network or extra package required.
function setup(selected = '') {
    const listeners = {};
    const input = {
        value: '',
        dataset: {
            drinkSearch: 'drinks',
            results: 'matches'
        },
        addEventListener: (name, action) =>
            listeners['input:' + name] = action,
        after: () => {}
    };

    const options = [
        {
            value: '',
            textContent: 'Choose'
        },
        {
            value: '1',
            textContent: 'Kávé Espresso'
        },
        {
            value: '2',
            textContent: 'Monster Ultra'
        },
        {
            value: '3',
            textContent: 'Saját különleges ital'
        }
    ];

    const select = {
        options,
        value: selected,
        addEventListener: (name, action) =>
            listeners['select:' + name] = action,
        replaceChildren(...children) {
            this.options = children;
        }
    };

    const status = {
        setAttribute() {},
        textContent: ''
    };

    vm.runInNewContext(
        fs.readFileSync(
            path.join(
                __dirname,
                '../..',
                'Caffeine/wwwroot/js/drink-search.js'
            ),
            'utf8'
        ),
        {
            document: {
                querySelectorAll: () => [input],
                getElementById: () => select,
                createElement: () => status
            }
        }
    );

    return {
        select,
        status,
        search(query) {
            input.value = query;
            listeners['input:input']();
        }
    };
}

test(
    'search ignores Hungarian accents and case',
    () => {
        const page = setup();

        page.search('KAVE');

        assert.deepEqual(
            page.select.options.map(o => o.value),
            ['', '1']
        );

        assert.equal(
            page.status.textContent,
            '1 matches'
        );
    }
);

test(
    'custom drink names are included and clearing search restores all choices',
    () => {
        const page = setup();

        page.search('kulonleges');

        assert.deepEqual(
            page.select.options.map(o => o.value),
            ['', '3']
        );

        page.search('');

        assert.equal(
            page.select.options.length,
            4
        );
    }
);

test(
    'filtering never changes an already selected favorite or drink',
    () => {
        const page = setup('2');

        page.search('unmatched');

        assert.deepEqual(
            page.select.options.map(o => o.value),
            ['', '2']
        );

        assert.equal(
            page.select.value,
            '2'
        );

        assert.equal(
            page.status.textContent,
            '0 matches'
        );
    }
);