window.valhallaLootList = {
    addThemeListener: function (interop) {
        var mql = window.matchMedia('(prefers-color-scheme: dark)');
        function schemeTest(e) {
            return interop.invokeMethodAsync('SetIsDark', e.matches);
        }
        mql.addEventListener('change', schemeTest);
        return interop.invokeMethodAsync('SetIsDark', mql.matches);
    }
};