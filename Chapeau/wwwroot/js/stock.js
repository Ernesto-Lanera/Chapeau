// Voorraad beheer
// JavaScript doet hier alleen het schermwerk. De controller slaat de voorraad op.

document.addEventListener("DOMContentLoaded", function () {
    setupStockFilters();
    setupStockTable();
});

function setupStockFilters() {
    var form = document.getElementById("stockFilterForm");
    var cardFilter = document.getElementById("cardFilter");
    var categoryFilter = document.getElementById("categoryFilter");

    if (form == null || cardFilter == null || categoryFilter == null) {
        return;
    }

    cardFilter.addEventListener("change", function () {
        categoryFilter.value = "";
        form.submit();
    });

    categoryFilter.addEventListener("change", function () {
        form.submit();
    });
}

function setupStockTable() {
    var table = document.getElementById("stockTable");

    if (table == null) {
        return;
    }

    table.addEventListener("input", function (event) {
        if (event.target.classList.contains("stock-current")) {
            changeStockFromInput(event.target);
        }
    });

    table.addEventListener("keydown", function (event) {
        if (event.key == "Enter" && event.target.classList.contains("stock-current")) {
            event.preventDefault();
            changeStockFromInput(event.target);
        }
    });

    table.addEventListener("click", function (event) {
        if (event.target.classList.contains("stock-delta")) {
            changeStockWithButton(event.target);
        }
    });
}

function changeStockFromInput(input) {
    if (input.disabled) {
        return;
    }

    var stock = getStockNumber(input.value);
    var row = input.closest("tr");

    // Het scherm reageert meteen, zodat de manager direct ziet wat er gebeurt.
    updateStockRow(row, stock);
    updateStockSummary();

    clearTimeout(input.saveTimer);
    input.saveTimer = setTimeout(function () {
        input.value = stock;
        saveStock(input, stock);
    }, 350);
}

function changeStockWithButton(button) {
    var row = button.closest("tr");
    var input = row.querySelector(".stock-current");

    if (input == null || input.disabled) {
        return;
    }

    var currentStock = getStockNumber(input.value);
    var change = Number(button.getAttribute("data-delta"));
    var newStock = currentStock + change;

    if (newStock < 0 || isNaN(newStock)) {
        newStock = 0;
    }

    input.value = newStock;
    updateStockRow(row, newStock);
    updateStockSummary();
    saveStock(input, newStock);
}

function saveStock(input, stock) {
    var menuItemId = input.getAttribute("data-stock-id");
    var url = input.getAttribute("data-stock-url");

    if (menuItemId == null || url == null) {
        return;
    }

    fetch(url, {
        method: "POST",
        headers: {
            "Content-Type": "application/x-www-form-urlencoded"
        },
        body: "id=" + encodeURIComponent(menuItemId) + "&newStock=" + encodeURIComponent(stock)
    })
        .then(function (response) {
            if (!response.ok) {
                throw new Error("Voorraad niet opgeslagen");
            }

            return response.json();
        })
        .then(function (result) {
            if (result.success == true) {
                input.value = result.stock;
                updateStockRow(input.closest("tr"), result.stock);
                updateStockSummary();
            }
        })
        .catch(function () {
            // Geen melding op het scherm, anders wordt typen irritant.
            console.log("Voorraad kon niet worden opgeslagen.");
        });
}

function updateStockRow(row, stock) {
    if (row == null) {
        return;
    }

    row.classList.remove("sold-out-row", "inactive-stock-row");

    if (stock == 0) {
        row.classList.add("sold-out-row");
    }

    var status = row.querySelector(".stock-status");
    var statusText = row.querySelector(".stock-status-text");

    if (status == null || statusText == null) {
        return;
    }

    status.classList.remove("ok", "warn", "danger", "inactive");

    if (stock == 0) {
        status.classList.add("danger");
        statusText.textContent = "Uitverkocht";
    } else if (stock <= 10) {
        status.classList.add("warn");
        statusText.textContent = "Bijna op";
    } else {
        status.classList.add("ok");
        statusText.textContent = "Op voorraad";
    }
}

function updateStockSummary() {
    var soldOutCount = document.getElementById("soldOutCount");
    var almostOutCount = document.getElementById("almostOutCount");

    if (soldOutCount == null || almostOutCount == null) {
        return;
    }

    soldOutCount.textContent = document.querySelectorAll(".stock-status.danger").length;
    almostOutCount.textContent = document.querySelectorAll(".stock-status.warn").length;
}

function getStockNumber(value) {
    var stock = Number(value);

    if (stock < 0 || isNaN(stock)) {
        return 0;
    }

    return Math.floor(stock);
}
