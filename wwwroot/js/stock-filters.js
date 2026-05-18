document.addEventListener("DOMContentLoaded", function () {
    const filterForm = document.getElementById("stockFilterForm");
    const cardFilter = document.getElementById("cardFilter");
    const categoryFilter = document.getElementById("categoryFilter");

    if (filterForm && cardFilter && categoryFilter) {
        cardFilter.addEventListener("change", function () {
            categoryFilter.value = "";
            filterForm.submit();
        });

        categoryFilter.addEventListener("change", function () {
            filterForm.submit();
        });
    }
});
