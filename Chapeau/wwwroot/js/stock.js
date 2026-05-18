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

        if (!row) {
            return;
        }

        const currentInput = row.querySelector(".stock-current");
        const newStockInput = form.querySelector(".stock-new");
        const statusPill = row.querySelector(".stock-status");
        const statusText = row.querySelector(".stock-status-text");

        if (!currentInput || !newStockInput || !statusPill || !statusText) {
            return;
        }

        function updateStatusUi(stock) {
            const status = getStatusData(stock);

            statusPill.classList.remove("ok", "warn", "danger");
            statusPill.classList.add(status.cls);

            statusText.textContent = status.text;

            if (stock === 0) {
                row.classList.add("sold-out-row");
            } else {
                row.classList.remove("sold-out-row");
            }
        }

        function saveStock(stockOverride) {
            let stock = parseInt(currentInput.value, 10);

            if (isNaN(stock) || stock < 0) {
                stock = 0;
            }

            if (typeof stockOverride === "number") {
                stock = Math.max(0, stockOverride);
            }

            currentInput.value = stock;
            newStockInput.value = stock;

            const body = new URLSearchParams();
            body.set("id", form.querySelector('input[name="id"]').value);
            body.set("newStock", stock.toString());

            const tokenInput = form.querySelector('input[name="__RequestVerificationToken"]');

            if (tokenInput) {
                body.set("__RequestVerificationToken", tokenInput.value);
            }

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
                    if (!response.ok) {
                        currentInput.classList.add("is-invalid");
                        return;
                    }

                    updateStatusUi(stock);
                    recalcTopCounters();
                })
                .catch(function () {
                    currentInput.classList.add("is-invalid");
                });
        }

        const autoSave = debounce(function () {
            saveStock();
        }, 200);

        currentInput.addEventListener("input", autoSave);

        currentInput.addEventListener("change", function () {
            saveStock();
        });

        currentInput.addEventListener("blur", function () {
            saveStock();
        });

        form.querySelectorAll(".stock-delta").forEach(function (button) {
            button.addEventListener("click", function (event) {
                event.preventDefault();

                const delta = parseInt(button.getAttribute("data-delta"), 10) || 0;
                const current = parseInt(currentInput.value, 10) || 0;
                const next = Math.max(0, current + delta);

                saveStock(next);
            });
        });
    });
}

function getStatusData(stock) {
    if (stock <= 0) {
        return {
            cls: "danger",
            text: "Uitverkocht"
        };
    }

    if (stock <= 10) {
        return {
            cls: "warn",
            text: "Bijna op"
        };
    }

    return {
        cls: "ok",
        text: "Op voorraad"
    };
}

function recalcTopCounters() {
    let soldOut = 0;
    let almostOut = 0;

    document.querySelectorAll(".stock-current").forEach(function (input) {
        let value = parseInt(input.value, 10);

        if (isNaN(value) || value < 0) {
            value = 0;
        }

        if (value === 0) {
            soldOut++;
        } else if (value <= 10) {
            almostOut++;
        }
    });

    const soldOutCount = document.getElementById("soldOutCount");
    const almostOutCount = document.getElementById("almostOutCount");

    if (soldOutCount) {
        soldOutCount.textContent = soldOut;
    }

    if (almostOutCount) {
        almostOutCount.textContent = almostOut;
    }
}

function debounce(fn, waitMs) {
    let timer;

    return function () {
        const args = arguments;

        clearTimeout(timer);

        timer = setTimeout(function () {
            fn.apply(null, args);
        }, waitMs);
    };
}