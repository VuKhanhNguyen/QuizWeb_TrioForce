document.addEventListener('DOMContentLoaded', function () {
    const levelModal = document.getElementById('levelModal');

    if (levelModal) {
        const categoryNameSpan = document.getElementById('categoryName');
        const levelLinks = levelModal.querySelectorAll('.level-card');

        levelModal.addEventListener('show.bs.modal', function (event) {
            const button = event.relatedTarget;
            const categoryId = button.getAttribute('data-category-id');
            const categoryName = button.getAttribute('data-category-name');

            categoryNameSpan.textContent = categoryName;

            levelLinks.forEach(function (link) {
                const baseUrl = link.href.split('?')[0];
                const levelId = link.getAttribute('data-level-id');
                link.href = `${baseUrl}?categoryId=${categoryId}&levelId=${levelId}`;
            });
        });
    }
});