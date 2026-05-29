// Payment Index page functionality
document.addEventListener('DOMContentLoaded', function() {
    const checkoutButtons = document.querySelectorAll('[data-checkout-btn]');
    checkoutButtons.forEach(button => {
        button.addEventListener('click', function() {
            const tableId = this.getAttribute('data-table-id');
            checkout(tableId);
        });
    });
});

function checkout(tableId) {
    if (!tableId || isNaN(tableId)) {
        alert('Ongeldig tafel nummer.');
        return;
    }

    window.location.href = `/Payment/ViewOrder?tableId=${tableId}&checkout=true`;
}