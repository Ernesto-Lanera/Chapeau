document.addEventListener('DOMContentLoaded', function () {
    const cardSelect = document.getElementById('menuCardSelect');
    const categorySelect = document.getElementById('categorySelect');
    const form = document.getElementById('menuItemEditCreateForm');

    function filterCategories() {
        if (!cardSelect || !categorySelect) {
            return;
        }

        const selectedCardId = cardSelect.value;

        Array.from(categorySelect.options).forEach(function (option) {
            if (option.value === '') {
                option.hidden = false;
                return;
            }

            const optionCardId = option.getAttribute('data-cardid');

            if (!selectedCardId) {
                option.hidden = false;
                return;
            }

            option.hidden = optionCardId !== selectedCardId;
        });

        const selectedOption = categorySelect.options[categorySelect.selectedIndex];

        if (selectedOption && selectedOption.hidden) {
            categorySelect.value = '';
        }
    }

    if (cardSelect && categorySelect) {
        filterCategories();

        cardSelect.addEventListener('change', function () {
            categorySelect.value = '';
            filterCategories();
        });
    }

    if (form) {
        form.addEventListener('submit', function (e) {
            if (!categorySelect || !categorySelect.value) {
                alert('Selecteer een geldige categorie.');
                e.preventDefault();
                return false;
            }

            if (!cardSelect) {
                return true;
            }

            const selectedOption = categorySelect.options[categorySelect.selectedIndex];

            if (selectedOption) {
                const optionCardId = selectedOption.getAttribute('data-cardid');

                if (optionCardId !== cardSelect.value) {
                    alert('De geselecteerde categorie hoort niet bij deze kaart.');
                    e.preventDefault();
                    return false;
                }
            }
        });
    }
});