document.addEventListener("DOMContentLoaded", function () {
    function filterCategories(cardSelectId, categorySelectId) {
        const cardSelect = document.getElementById(cardSelectId);
        const categorySelect = document.getElementById(categorySelectId);

        if (!cardSelect || !categorySelect) {
            return;
        }

        function applyFilter() {
            const selectedCardId = cardSelect.value;

            Array.from(categorySelect.options).forEach(function (option) {
                if (option.value === "") {
                    option.hidden = false;
                    return;
                }

                const optionCardId = option.getAttribute("data-cardid");

                if (!selectedCardId) {
                    option.hidden = false;
                } else {
                    option.hidden = optionCardId !== selectedCardId;
                }
            });

            const selectedOption = categorySelect.options[categorySelect.selectedIndex];

            if (selectedOption && selectedOption.hidden) {
                categorySelect.value = "";
            }
        }

        cardSelect.addEventListener("change", function () {
            categorySelect.value = "";
            applyFilter();
        });

        applyFilter();
    }

    filterCategories("createMenuCardSelect", "createCategorySelect");
});
