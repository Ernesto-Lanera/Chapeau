let isUpdating = false;

document.addEventListener('DOMContentLoaded', () => {
    fetch('/Menu/GetActiveOrderItems')
        .then(res => res.json())
        .then(data => { if (data.success) updateCartUI(data.items); });
});

function updateCartUI(items) {
    const cart = document.getElementById('cart-container');
    let htmlContent = '';

    items.forEach(item => {
        htmlContent += `
                <div class="mb-2 p-2 border rounded d-flex flex-wrap align-items-center justify-content-between" style="background-color: #212529; color: white;">
                    <span class="fw-bold text-truncate me-2"  font-size: 0.9rem;">${item.menuItemName}</span>
                    
                    <div class="d-flex align-items-center gap-1">
                        <button class="btn btn-sm btn-outline-danger rounded-circle p-0 d-flex justify-content-center align-items-center fw-bold" 
                                style="width: 22px; height: 22px; font-size: 14px;" 
                                onclick="updateQuantity(${item.menuItemId}, ${item.amount - 1})">-</button>
                        
                        <span id="qty-${item.menuItemId}" class="fw-bold mx-1" style="min-width: 15px; text-align: center; font-size: 0.9rem;">${item.amount}</span>
                        
                        <button class="btn btn-sm btn-outline-success rounded-circle p-0 d-flex justify-content-center align-items-center fw-bold" 
                                style="width: 22px; height: 22px; font-size: 14px;" 
                                onclick="updateQuantity(${item.menuItemId}, ${item.amount + 1})">+</button>
                        
                        <button class="btn btn-sm btn-outline-danger ms-2 d-flex justify-content-center align-items-center" 
                                style="width: 26px; height: 26px;" 
                                onclick="removeFromOrder(${item.menuItemId})">
                            <i class="bi bi-trash"></i> 
                        </button>
                    </div>
                </div>
            `;
    });

    cart.innerHTML = htmlContent;
}

function addToOrder(menuItemId, menuItemName) {
    if (isUpdating) return;

    let existingQtySpan = document.getElementById(`qty-${menuItemId}`);
    if (existingQtySpan) {
        let currentQty = parseInt(existingQtySpan.innerText);
        updateQuantity(menuItemId, currentQty + 1);
        return;
    }

    isUpdating = true;
    fetch('/Menu/AddMenuItemToOrder', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: new URLSearchParams({ 'MenuItemId': menuItemId, 'MenuItemName': menuItemName })
    })
        .then(res => res.json())
        .then(data => { if (data.success) updateCartUI(data.items); })
        .finally(() => { isUpdating = false; });
}

function removeFromOrder(menuItemId) {
    if (isUpdating) return;

    isUpdating = true;
    fetch('/Menu/RemoveMenuItemFromOrder', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: new URLSearchParams({ 'MenuItemId': menuItemId })
    })
        .then(res => res.json())
        .then(data => { if (data.success) updateCartUI(data.items); })
        .finally(() => { isUpdating = false; });
}

function updateQuantity(menuItemId, newQuantity) {
    if (isUpdating) return;

    if (newQuantity <= 0) {
        removeFromOrder(menuItemId);
        return;
    }

    isUpdating = true;
    fetch('/Menu/UpdateMenuItemQuantity', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: new URLSearchParams({ 'MenuItemId': menuItemId, 'NewQuantity': newQuantity })
    })
        .then(res => res.json())
        .then(data => { if (data.success) updateCartUI(data.items); })
        .finally(() => { isUpdating = false; });
}

function sendOrder() {
    if (isUpdating) return;
    isUpdating = true;
    fetch('/Menu/PlaceOrder', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
    })
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                window.location.href = '/Table/Index';
            } else {
                alert(data.message || 'Er is een fout opgetreden.');
            }
        })
        .finally(() => { isUpdating = false; });
}