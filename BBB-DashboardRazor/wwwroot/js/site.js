window.renderBarChart = (canvasId, config) => {
    const ctx = document.getElementById(canvasId).getContext('2d');

    // اگر قبلاً نمودار وجود داشت، نابود کن
    if (window[canvasId + 'Chart']) {
        window[canvasId + 'Chart'].destroy();
    }

    window[canvasId + 'Chart'] = new Chart(ctx, config);
};



