// Payment Index page functionality
document.addEventListener('DOMContentLoaded', function() {
    // Attach event listeners to all checkout buttons
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

    // Redirect to ViewOrder page (open checkout directly)
    window.location.href = `/Payment/ViewOrder?tableId=${tableId}&checkout=true`;
}