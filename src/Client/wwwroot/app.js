window.valhallaLootList = {
    addThemeListener: function (interop) {
        var mql = window.matchMedia('(prefers-color-scheme: dark)');
        function schemeTest(e) {
            return interop.invokeMethodAsync('SetIsDark', e.matches);
        }
        mql.addEventListener('change', schemeTest);
        return interop.invokeMethodAsync('SetIsDark', mql.matches);
    },
    makeDialogScrollable: function (id) {
        let element = document.getElementById(id);
        if (element) {
            element.classList.add('mud-dialog-scrollable');
        }
    },
    updateUserRoles: async function (prop, claims) {
        let user = await AuthenticationService.instance._userManager.getUser();
        if (claims) {
            user.profile[prop] = claims;
        }
        else {
            delete user.profile[prop];
        }
        await AuthenticationService.instance._userManager.storeUser(user);
    }
};