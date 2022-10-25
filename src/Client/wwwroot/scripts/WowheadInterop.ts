
interface WowheadInterop {
    hide(): void;
}

interface Window {
    WhInterop: WowheadInterop;
    WH: any;
}

(() => {
    window.WhInterop = {
        hide: function () {
            if (window.WH) {
                if (window.WH.Tooltip && window.WH.Tooltip.hide) {
                    window.WH.Tooltip.hide();
                } else if (window.WH.Tooltips && window.WH.Tooltips.clearTouchTooltip) {
                    window.WH.Tooltips.clearTouchTooltip();
                }
            }
        }
    };
})();
