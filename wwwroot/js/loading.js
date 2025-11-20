window.addEventListener('load', function () {
    var overlay = document.getElementById('loaderOverlay');
    if (overlay) {
        overlay.classList.add('hidden');
    }
});

document.addEventListener('DOMContentLoaded', function () {
    var logoutForm = document.getElementById('logoutForm');
    var confirmBtn = document.getElementById('confirmLogoutBtn');
    var modalElement = document.getElementById('logoutConfirmModal');

    if (!logoutForm || !confirmBtn || !modalElement || typeof bootstrap === 'undefined') {
        return;
    }

    var logoutModal = new bootstrap.Modal(modalElement);

    logoutForm.addEventListener('submit', function (event) {
        event.preventDefault();
        logoutModal.show();
    });

    confirmBtn.addEventListener('click', function () {
        logoutModal.hide();
        logoutForm.submit();
    });
});

