window.initializeBudgetSlider = (minVal, maxVal, initialMin, initialMax, dotNetHelper) => {
    $("#slider-range").slider({
        range: true,
        min: minVal,
        max: maxVal,
        values: [initialMin, initialMax],
        step: 1000000,
        slide: function (event, ui) {
            $("#amount").val(formatNumber(ui.values[0]) + " تا " + formatNumber(ui.values[1]));
        },
        change: function (event, ui) {
            // ارسال به C# (Blazor Server)
            dotNetHelper.invokeMethodAsync('UpdateBudget', ui.values[0], ui.values[1]);
        }
    });

    // مقدار اولیه
    $("#amount").val(formatNumber(initialMin) + " تا " + formatNumber(initialMax));
};

function formatNumber(num) {
    return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}
window.addClickOutsideListener = (dotNetHelper, callbackName) => {
    const handler = (e) => {
        const dropdown = e.target.closest('.dropdown');
        if (!dropdown) {
            dotNetHelper.invokeMethodAsync(callbackName);
        }
    };

    document.addEventListener('click', handler);

    // فقط یک بار اضافه بشه
    if (!window.clickOutsideHandlers) {
        window.clickOutsideHandlers = [];
    }
    window.clickOutsideHandlers.push({ dotNetHelper, handler });
};

// برای جلوگیری از memory leak (اختیاری)
window.removeClickOutsideListeners = () => {
    if (window.clickOutsideHandlers) {
        window.clickOutsideHandlers.forEach(({ handler }) => {
            document.removeEventListener('click', handler);
        });
        window.clickOutsideHandlers = [];
    }
};

window.initUserDropdown = (dotNetHelper) => {
    // فقط یک بار اجرا بشه
    if (window.userDropdownListenerAdded) return;
    window.userDropdownListenerAdded = true;

    document.addEventListener('click', function () {
        dotNetHelper.invokeMethodAsync('CloseUserDropdown');
    });
};



window.setCookie = (name, value, days) => {
    let expires = "";
    if (days) {
        const date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        expires = "; expires=" + date.toUTCString();
    }
    document.cookie = name + "=" + (value || "") + expires + "; path=/";
}


window.deleteCookie = (name) => {
    document.cookie = name + "=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
};
