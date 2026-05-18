document.addEventListener("DOMContentLoaded", function () {
    setupManageMenuFilters();
    setupCreateCategoryFilter();
});

function setupManageMenuFilters() {
    const filterForm = document.getElementById("manageMenuFilterForm");
    const cardFilter = document.getElementById("cardFilter");
    const categoryFilter = document.getElementById("categoryFilter");

    if (!filterForm || !cardFilter || !categoryFilter) {
        return;
    }

    cardFilter.addEventListener("change", function () {
        categoryFilter.value = "";
        filterForm.submit();
    });

    categoryFilter.addEventListener("change", function () {
        filterForm.submit();
    });
}

function setupCreateCategoryFilter() {
    filterCategories("createMenuCardSelect", "createCategorySelect");
}

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