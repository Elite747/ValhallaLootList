window.valhallaLootList = {
    addThemeListener: function (interop) {
        var mql = window.matchMedia('(prefers-color-scheme: dark)');
        function schemeTest(e) {
            return interop.invokeMethodAsync('SetIsDark', e.matches);
        }
        mql.addEventListener('change', schemeTest);
        return interop.invokeMethodAsync('SetIsDark', mql.matches);
    },
    makeDialogScrollable: function (id, mode) {
        let element = document.getElementById(id);
        if (element) {
            element.classList.add('mud-dialog-scrollable');
            element.classList.add(`mud-dialog-scrollable-${mode}`);
        }
    }
};