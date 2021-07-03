window.valhallaLootList = {
    darkThemeQuery: window.matchMedia('(prefers-color-scheme: dark)'),
    getSystemTheme: function () {
        if (this.darkThemeQuery.matches) {
            return 'dark';
        } else {
            return 'light';
        }
    },
    updateTheme: function () {
        let current = document.documentElement.getAttribute('theme');
        let manual = localStorage.getItem('theme');
        if (manual) {
            if (current !== manual) {
                document.documentElement.setAttribute('theme', manual);
            }
        } else {
            let system = valhallaLootList.getSystemTheme();

            if (current !== system) {
                document.documentElement.setAttribute('theme', system);
            }
        }
    },
    getTheme: function () {
        return localStorage.getItem('theme');
    },
    setTheme: function (theme) {
        if (theme) {
            localStorage.setItem('theme', theme);
        } else {
            localStorage.removeItem('theme');
        }
        this.updateTheme();
    },
    makeDialogScrollable: function (id, mode) {
        let element = document.getElementById(id);
        if (element) {
            element.classList.add('mud-dialog-scrollable');
            element.classList.add(`mud-dialog-scrollable-${mode}`);
        }
    }
};

valhallaLootList.darkThemeQuery.addEventListener('change', valhallaLootList.updateTheme);
valhallaLootList.updateTheme();