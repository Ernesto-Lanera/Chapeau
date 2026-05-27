document.addEventListener("DOMContentLoaded", function () {
    setupManageMenuFilters();
    setupCreateCategoryFilter();
    setupAlcoholicChoiceVisibility();
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
    const cardSelect = document.getElementById("createMenuCardSelect");
    const categorySelect = document.getElementById("createCategorySelect");

    if (!cardSelect || !categorySelect) {
        return;
    }

    function applySelectedCard(resetCategory) {
        const selectedCardId = cardSelect.value;

        Array.from(categorySelect.options).forEach(function (option) {
            if (option.value === "") {
                option.hidden = false;
                option.disabled = false;
                return;
            }

            const belongsToSelectedCard = option.dataset.cardid === selectedCardId;
            option.hidden = !belongsToSelectedCard;
            option.disabled = !belongsToSelectedCard;
        });

        const selectedCategory = categorySelect.options[categorySelect.selectedIndex];
        if (resetCategory || (selectedCategory && selectedCategory.disabled)) {
            categorySelect.value = "";
        }

        updateAlcoholicChoiceVisibility(categorySelect);
    }

    cardSelect.addEventListener("change", function () {
        applySelectedCard(true);
    });

    applySelectedCard(false);
}

function setupAlcoholicChoiceVisibility() {
    const categorySelects = document.querySelectorAll(".menu-item-form select[name='CategoryID']");

    categorySelects.forEach(function (categorySelect) {
        categorySelect.addEventListener("change", function () {
            updateAlcoholicChoiceVisibility(categorySelect);
        });

        updateAlcoholicChoiceVisibility(categorySelect);
    });
}

function updateAlcoholicChoiceVisibility(categorySelect) {
    const form = categorySelect.closest("form");
    if (!form) {
        return;
    }

    const alcoholicGroup = form.querySelector("[data-alcoholic-choice-group]");
    const alcoholicCheckbox = form.querySelector("input[type='checkbox'][name='IsAlcoholic']");

    if (!alcoholicGroup || !alcoholicCheckbox) {
        return;
    }

    const selectedOption = categorySelect.options[categorySelect.selectedIndex];
    const canChooseAlcohol = selectedOption && selectedOption.dataset.allowsAlcohol === "true";

    alcoholicGroup.hidden = !canChooseAlcohol;

    if (!canChooseAlcohol) {
        alcoholicCheckbox.checked = false;
    }
}
