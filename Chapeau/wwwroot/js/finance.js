"use strict";

document.addEventListener("DOMContentLoaded", function () {
    initializeCustomPeriodSelection();
    initializeRevenueTrendChart();
});

function initializeCustomPeriodSelection() {
    const customButton = document.getElementById("selectCustomPeriod");
    const customForm = document.getElementById("customPeriodForm");
    const startDate = document.getElementById("startDate");
    const periodButtons = document.querySelectorAll(".finance-period-button");

    if (!customButton || !customForm || !startDate) {
        return;
    }

    customButton.addEventListener("click", function () {
        periodButtons.forEach(function (button) {
            button.classList.remove("is-selected");
        });

        customButton.classList.add("is-selected");
        customButton.setAttribute("aria-expanded", "true");
        customForm.hidden = false;
        startDate.focus();
    });
}

function initializeRevenueTrendChart() {
    const chartCanvas = document.getElementById("revenueTrendChart");
    const trendDataElement = document.getElementById("revenueTrendData");

    if (!chartCanvas || !trendDataElement || typeof Chart === "undefined") {
        return;
    }

    let trendData;

    try {
        trendData = JSON.parse(trendDataElement.textContent);
    } catch (error) {
        console.error("De omzettrendgegevens konden niet worden geladen.", error);
        return;
    }

    new Chart(chartCanvas, {
        type: "bar",
        data: {
            labels: trendData.labels || [],
            datasets: [
                {
                    label: "Dranken",
                    data: trendData.drinks || [],
                    backgroundColor: "#3b82f6",
                    borderColor: "#3b82f6",
                    borderWidth: 0,
                    borderRadius: 0
                },
                {
                    label: "Lunch",
                    data: trendData.lunch || [],
                    backgroundColor: "#13b981",
                    borderColor: "#13b981",
                    borderWidth: 0,
                    borderRadius: 0
                },
                {
                    label: "Diner",
                    data: trendData.dinner || [],
                    backgroundColor: "#a64cec",
                    borderColor: "#a64cec",
                    borderWidth: 0,
                    borderRadius: 0
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            animation: false,
            plugins: {
                legend: {
                    position: "bottom",
                    labels: {
                        boxWidth: 14,
                        boxHeight: 14,
                        padding: 10,
                        font: { size: 13 }
                    }
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
                x: {
                    grid: {
                        color: "#d1d5db",
                        borderDash: [4, 4]
                    },
                    ticks: {
                        color: "#6b7280",
                        font: { size: 13 }
                    }
                },
                y: {
                    beginAtZero: true,
                    grid: {
                        color: "#d1d5db",
                        borderDash: [4, 4]
                    },
                    ticks: {
                        color: "#6b7280",
                        font: { size: 13 },
                        callback: function (value) {
                            return Number(value).toLocaleString("nl-NL");
                        }
                    }
                }
            }
        }
    });
}
