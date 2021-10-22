type SystemTheme = "dark" | "light";

interface ThemeManager {
    getTheme(): SystemTheme;
    setTheme(theme: SystemTheme): void;
}

interface Window {
    themeManager: ThemeManager;
}

(() => {
    const darkThemeQuery = window.matchMedia("(prefers-color-scheme: dark)"),
        themeKey = "theme",
        dark: SystemTheme = "dark",
        light: SystemTheme = "light";

    function getSystemTheme() {
        if (darkThemeQuery.matches) {
            return dark;
        } else {
            return light;
        }
    }
    
    function updateTheme() {
        const current = document.documentElement.getAttribute(themeKey);
        const manual = localStorage.getItem(themeKey);
        if (manual) {
            if (current !== manual) {
                document.documentElement.setAttribute(themeKey, manual);
            }
        } else {
            const system = getSystemTheme();
            if (current !== system) {
                document.documentElement.setAttribute(themeKey, system);
            }
        }
    }

    function sanitizeTheme(theme: string): SystemTheme {
        return theme === dark || theme === light ? theme : null;
    }

    function getTheme(): SystemTheme {
        return sanitizeTheme(localStorage.getItem(themeKey));
    }

    function setTheme(theme: SystemTheme) {
        if (typeof theme === "string") {
            try {
                localStorage.setItem(themeKey, theme);
            }
            catch (e) {
                console.error(e);
            }
        } else {
            localStorage.removeItem(themeKey);
        }
        updateTheme();
    }

    window.themeManager = { getTheme, setTheme };
    darkThemeQuery.addEventListener("change", updateTheme);
    updateTheme();
})();
