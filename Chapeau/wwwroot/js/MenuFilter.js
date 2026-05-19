document.addEventListener("DOMContentLoaded", function () {
    const topMenuButtons = document.querySelectorAll('.menucard-btn');
    const categoryButtons = document.querySelectorAll('.category-btn');
    const menuItems = document.querySelectorAll('.menu-item');

    topMenuButtons.forEach(button => {
        button.addEventListener('click', function () {

            topMenuButtons.forEach(btn => btn.classList.remove('active'));
            this.classList.add('active');

            categoryButtons.forEach(btn => btn.classList.remove('active'));

            const isAll = this.getAttribute('data-filter') === 'all';
            const menuCardId = this.getAttribute('data-value');

            categoryButtons.forEach(catBtn => {
                if (isAll) {
                    catBtn.classList.remove('d-none');
                } else {
                    if (catBtn.getAttribute('data-menucard') === menuCardId) {
                        catBtn.classList.remove('d-none');
                    } else {
                        catBtn.classList.add('d-none');
                    }
                }
            });

            menuItems.forEach(item => {
                if (isAll) {
                    item.classList.remove('d-none');
                } else {
                    if (item.getAttribute('data-menucard') === menuCardId) {
                        item.classList.remove('d-none');
                    } else {
                        item.classList.add('d-none');
                    }
                }
            });
        });
    });

    categoryButtons.forEach(button => {
        button.addEventListener('click', function () {

            categoryButtons.forEach(btn => btn.classList.remove('active'));
            this.classList.add('active');

            const categoryId = this.getAttribute('data-value');

            menuItems.forEach(item => {
                if (item.getAttribute('data-category') === categoryId) {
                    item.classList.remove('d-none');
                } else {
                    item.classList.add('d-none');
                }
            });
        });
    });
});