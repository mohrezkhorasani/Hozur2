



window.setCookie = (name, value, days) => {
    let expires = "";
    if (days) {
        const date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        expires = "; expires=" + date.toUTCString();
    }
    document.cookie = name + "=" + (value || "") + expires + "; path=/";
}


window.renderBarChart = (canvasId, config) => {
    const ctx = document.getElementById(canvasId).getContext('2d');

    // اگر قبلاً نمودار وجود داشت، نابود کن
    if (window[canvasId + 'Chart']) {
        window[canvasId + 'Chart'].destroy();
    }

    window[canvasId + 'Chart'] = new Chart(ctx, config);
};