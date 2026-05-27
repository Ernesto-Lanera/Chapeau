// Payment Index page functionality
document.addEventListener('DOMContentLoaded', function() {
    // Attach event listeners to all checkout buttons
    const checkoutButtons = document.querySelectorAll('[data-checkout-btn]');
    checkoutButtons.forEach(button => {
        button.addEventListener('click', function() {
            const tableNumber = this.getAttribute('data-table-number');
            checkout(tableNumber);
        });
    });
});

function checkout(tableNumber) {
    if (!tableNumber || isNaN(tableNumber)) {
        alert('Ongeldig tafel nummer.');
        return;
    }

    // Redirect to ViewOrder page
    window.location.href = `/Payment/ViewOrder?tableId=${tableNumber}`;
}