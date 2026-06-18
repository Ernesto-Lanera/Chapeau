// Kleine schermhulp voor menu beheer.
// De echte controles staan in MenuService.
// Hier regelen we alleen filters, categorieën en het alcoholveld.

document.addEventListener("DOMContentLoaded", function () {
    setupMenuFilters();
    setupCreateMenuForm();
    setupAlcoholFields();
});

function setupMenuFilters() {
    var form = document.getElementById("manageMenuFilterForm");
    var cardFilter = document.getElementById("cardFilter");
    var categoryFilter = document.getElementById("categoryFilter");

    if (form == null || cardFilter == null || categoryFilter == null) {
        return;
    }

    cardFilter.addEventListener("change", function () {
        // Bij een nieuwe kaart beginnen we weer met alle categorieën.
        categoryFilter.value = "";
        form.submit();
    });

    categoryFilter.addEventListener("change", function () {
        form.submit();
    });
}

function setupCreateMenuForm() {
    var cardSelect = document.getElementById("createMenuCardSelect");
    var categorySelect = document.getElementById("createCategorySelect");

    if (cardSelect == null || categorySelect == null) {
        return;
    }

    showCorrectCategories(cardSelect, categorySelect);

    cardSelect.addEventListener("change", function () {
        categorySelect.value = "";
        showCorrectCategories(cardSelect, categorySelect);
    });
}

function showCorrectCategories(cardSelect, categorySelect) {
    var selectedCardId = cardSelect.value;
    var options = categorySelect.options;

    for (var i = 0; i < options.length; i++) {
        var option = options[i];
        var categoryCardId = option.getAttribute("data-cardid");
        var belongsToSelectedCard = categoryCardId === selectedCardId;
        var isEmptyOption = option.value === "";

        // Alleen categorieën van de gekozen kaart blijven zichtbaar.
        option.hidden = !(isEmptyOption || belongsToSelectedCard);
        option.disabled = !(isEmptyOption || belongsToSelectedCard);
    }

    updateAlcoholField(categorySelect);
}

function setupAlcoholFields() {
    var categorySelects = document.querySelectorAll(".menu-item-form select[name='CategoryID']");

    for (var i = 0; i < categorySelects.length; i++) {
        var categorySelect = categorySelects[i];

        updateAlcoholField(categorySelect);

        categorySelect.addEventListener("change", function () {
            updateAlcoholField(this);
        });
    }
}

function updateAlcoholField(categorySelect) {
    var form = categorySelect.closest("form");

    if (form == null) {
        return;
    }

    var alcoholGroup = form.querySelector("[data-alcoholic-choice-group]");
    var alcoholCheckbox = form.querySelector("input[name='IsAlcoholic'][type='checkbox']");
    var selectedOption = categorySelect.options[categorySelect.selectedIndex];

    if (alcoholGroup == null || alcoholCheckbox == null || selectedOption == null) {
        return;
    }

    var categoryAllowsAlcohol = selectedOption.getAttribute("data-allows-alcohol") === "true";

    alcoholGroup.hidden = !categoryAllowsAlcohol;

    if (!categoryAllowsAlcohol) {
        // Geen alcoholcategorie gekozen, dus het vinkje mag niet per ongeluk blijven staan.
        alcoholCheckbox.checked = false;
    }
}
