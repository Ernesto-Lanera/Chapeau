document.addEventListener("DOMContentLoaded", function () {
    setupStockFilters();
    setupStockForms();
});

function setupStockFilters() {
    const filterForm = document.getElementById("stockFilterForm");
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

function setupStockForms() {
    document.querySelectorAll(".stock-form").forEach(function (form) {
        const row = form.closest("tr");
        if (!row) return;

        const currentInput = row.querySelector(".stock-current");
        const newStockInput = form.querySelector(".stock-new");
        const statusPill = row.querySelector(".stock-status");
        const statusText = row.querySelector(".stock-status-text");
        if (!currentInput || !newStockInput || !statusPill || !statusText) return;

        function renderServerResult(result) {
            currentInput.value = result.stock;
            newStockInput.value = result.stock;
            statusPill.classList.remove("ok", "warn", "danger");
            statusPill.classList.add(result.statusCssClass);
            statusText.textContent = result.statusText;
            row.classList.toggle("sold-out-row", result.isOutOfStock);

            const soldOutCount = document.getElementById("soldOutCount");
            const almostOutCount = document.getElementById("almostOutCount");
            if (soldOutCount) soldOutCount.textContent = result.soldOutCount;
            if (almostOutCount) almostOutCount.textContent = result.almostOutCount;
        }

        function saveStock(stockOverride) {
            let stock = parseInt(currentInput.value, 10);
            if (isNaN(stock) || stock < 0) stock = 0;
            if (typeof stockOverride === "number") stock = Math.max(0, stockOverride);

            currentInput.value = stock;
            newStockInput.value = stock;

            const body = new URLSearchParams();
            body.set("id", form.querySelector('input[name="id"]').value);
            body.set("newStock", stock.toString());
            ["cardId", "categoryId", "__RequestVerificationToken"].forEach(function (name) {
                const input = form.querySelector(`input[name="${name}"]`);
                if (input && input.value) body.set(name, input.value);
            });

            currentInput.classList.remove("is-invalid");
            fetch(form.action, {
                method: "POST",
                headers: {
                    "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8",
                    "Accept": "application/json",
                    "X-Requested-With": "fetch"
                },
                body: body.toString()
            })
                .then(function (response) {
                    if (!response.ok) throw new Error("Voorraad opslaan is mislukt.");
                    return response.json();
                })
                .then(renderServerResult)
                .catch(function () {
                    currentInput.classList.add("is-invalid");
                });
        }

        const autoSave = debounce(function () { saveStock(); }, 200);
        currentInput.addEventListener("input", autoSave);
        currentInput.addEventListener("change", function () { saveStock(); });
        currentInput.addEventListener("blur", function () { saveStock(); });

        form.querySelectorAll(".stock-delta").forEach(function (button) {
            button.addEventListener("click", function (event) {
                event.preventDefault();
                const delta = parseInt(button.getAttribute("data-delta"), 10) || 0;
                const current = parseInt(currentInput.value, 10) || 0;
                saveStock(Math.max(0, current + delta));
            });
        });
    });
}

function debounce(fn, waitMs) {
    let timer;
    return function () {
        const args = arguments;
        clearTimeout(timer);
        timer = setTimeout(function () { fn.apply(null, args); }, waitMs);
    };
}
