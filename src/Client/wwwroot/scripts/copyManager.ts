interface CopyManager {
    copy(text: string): Promise<boolean>;
}

interface Window {
    copyManager: CopyManager;
}

(() => {
    function copy(text: string): Promise<boolean> {
        return navigator.clipboard.writeText(text).then(() => true, () => false);
    }

    window.copyManager = { copy };
})();
