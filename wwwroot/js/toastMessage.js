(() => {
    function resolveBackground(type) {
        switch ((type || '').toLowerCase()) {
            case 'success':
                return '#07bc0c';
            case 'error':
                return '#e74c3c';
            case 'warning':
                return '#f39c12';
            default:
                return '#3498db';
        }
    }

    window.showToastMessage = function (config) {
        if (!config || typeof Toastify === 'undefined') {
            return;
        }

        const toastOptions = {
            text: config.message || '',
            duration: config.duration || 3500,
            gravity: config.gravity || 'top',
            position: config.position || 'right',
            stopOnFocus: true,
            close: typeof config.close === 'boolean' ? config.close : true,
            offset: config.offset || { x: 20, y: 20 },
            style: Object.assign(
                {
                    background: config.background || resolveBackground(config.type),
                    borderRadius: '14px',
                    fontWeight: 600,
                    boxShadow: '0 12px 30px rgba(0, 0, 0, 0.18)'
                },
                config.style || {}
            )
        };

        Toastify(toastOptions).showToast();
    };

    document.addEventListener('DOMContentLoaded', function () {
        if (window.__toastMessage) {
            window.showToastMessage(window.__toastMessage);
            window.__toastMessage = null;
        }
    });
})();

