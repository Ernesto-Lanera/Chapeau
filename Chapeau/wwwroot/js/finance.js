// Kleine schermhulp voor het financiële overzicht.
// De berekeningen komen uit de service en repository.
// Hier openen we alleen het datumformulier en tekenen we de grafiek.

document.addEventListener("DOMContentLoaded", function () {
    setupCustomPeriodForm();
    setupRevenueChart();
});

function setupCustomPeriodForm() {
    var customButton = document.getElementById("selectCustomPeriod");
    var customForm = document.getElementById("customPeriodForm");
    var startDate = document.getElementById("startDate");

    if (customButton == null || customForm == null || startDate == null) {
        return;
    }

    customButton.addEventListener("click", function () {
        var periodButtons = document.querySelectorAll(".finance-period-button");

        for (var i = 0; i < periodButtons.length; i++) {
            periodButtons[i].classList.remove("is-selected");
        }

        customButton.classList.add("is-selected");
        customButton.setAttribute("aria-expanded", "true");
        customForm.hidden = false;
        startDate.focus();
    });
}

function setupRevenueChart() {
    var chartCanvas = document.getElementById("revenueTrendChart");
    var dataElement = document.getElementById("revenueTrendData");

    if (chartCanvas == null || dataElement == null || typeof Chart === "undefined") {
        return;
    }

    // De view zet de omzetdata in een scriptblok. Hier lezen we die data uit.
    var chartData = readChartData(dataElement);

    if (chartData == null) {
        return;
    }

    new Chart(chartCanvas, {
        type: "bar",
        data: {
            labels: chartData.labels,
            datasets: [
                createDataset("Dranken", chartData.drinks, "#3b82f6"),
                createDataset("Lunch", chartData.lunch, "#16a34a"),
                createDataset("Diner", chartData.dinner, "#9333ea")
            ]
        },
        options: createChartOptions()
    });
}

function readChartData(dataElement) {
    try {
        return JSON.parse(dataElement.textContent);
    } catch (error) {
        console.error("De omzetgegevens voor de grafiek konden niet worden gelezen.", error);
        return null;
    }
}

function createDataset(label, values, color) {
    return {
        label: label,
        data: values || [],
        backgroundColor: color,
        borderColor: color,
        borderWidth: 0
    };
}

function createChartOptions() {
    return {
        responsive: true,
        maintainAspectRatio: false,
        animation: false,
        plugins: {
            legend: {
                position: "bottom"
            },
            tooltip: {
                callbacks: {
                    label: function (context) {
                        return context.dataset.label + ": € " + Number(context.raw).toFixed(2);
                    }
                }
            }
        },
        scales: {
            y: {
                beginAtZero: true,
                ticks: {
                    callback: function (value) {
                        return Number(value).toLocaleString("nl-NL");
                    }
                }
            }
        }
    };
}
