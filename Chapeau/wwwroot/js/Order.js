function updateCartUI(items) {
    const cart = document.getElementById('cart-container');
    cart.innerHTML = '';

    items.forEach(item => {
        let commentHtml = item.Comment
            ? `<div class="w-100 text-warning mt-1" style="font-size: 0.75rem; font-style: italic;">Note: ${item.Comment}</div>`
            : '';

        cart.innerHTML += `
                <div class="mb-2 p-2 border rounded d-flex flex-wrap align-items-center justify-content-between" style="background-color: #212529; color: white;">
                    <span class="fw-bold text-truncate me-2" style="max-width: 90px; font-size: 0.9rem;">${item.MenuItemName}</span>
                    
                    <div class="d-flex align-items-center gap-1">
                        <button class="btn btn-sm btn-outline-danger rounded-circle p-0 d-flex justify-content-center align-items-center fw-bold" 
                                style="width: 22px; height: 22px; font-size: 14px;" 
                                onclick="updateQuantity(${item.MenuItemId}, ${item.Amount - 1})">-</button>
                        
                        <span class="fw-bold mx-1" style="min-width: 15px; text-align: center; font-size: 0.9rem;">${item.Amount}</span>
                        
                        <button class="btn btn-sm btn-outline-success rounded-circle p-0 d-flex justify-content-center align-items-center fw-bold" 
                                style="width: 22px; height: 22px; font-size: 14px;" 
                                onclick="updateQuantity(${item.MenuItemId}, ${item.Amount + 1})">+</button>
                        
                        <button class="btn btn-sm btn-outline-danger ms-2 d-flex justify-content-center align-items-center" 
                                style="width: 26px; height: 26px;" 
                                onclick="removeFromOrder(${item.MenuItemId})">
                            <i class="bi bi-trash"></i> 
                        </button>
                        
                        <button class="btn btn-sm btn-outline-primary d-flex justify-content-center align-items-center" 
                                style="width: 26px; height: 26px;"
                                onclick="addComment(${item.MenuItemId})">
                            <i class="bi bi-pencil-square"></i>
                        </button>
                    </div>
                    ${commentHtml}
                </div>
            `;
    });
}

function addToOrder(menuCardId, menuItemName) {
    fetch('/Menu/AddMenuItemToOrder', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: new URLSearchParams({ 'MenuCardId': menuCardId, 'MenuItemName': menuItemName })
    }).then(res => res.json()).then(data => { if (data.success) updateCartUI(data.items); });
}

function removeFromOrder(menuCardId) {
    fetch('/Menu/RemoveMenuItemFromOrder', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: new URLSearchParams({ 'MenuCardId': menuCardId })
    }).then(res => res.json()).then(data => { if (data.success) updateCartUI(data.items); });
}

function updateQuantity(menuCardId, newQuantity) {
    if (newQuantity <= 0) {
        removeFromOrder(menuCardId);
        return;
    }
    fetch('/Menu/UpdateMenuItemQuantity', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: new URLSearchParams({ 'MenuCardId': menuCardId, 'NewQuantity': newQuantity })
    }).then(res => res.json()).then(data => { if (data.success) updateCartUI(data.items); });
}

function addComment(menuCardId) {
    const comment = prompt("Enter a note for the kitchen/bar:");
    if (comment !== null) {
        fetch('/Menu/AddCommentToItem', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: new URLSearchParams({ 'MenuCardId': menuCardId, 'Comment': comment })
        }).then(res => res.json()).then(data => { if (data.success) updateCartUI(data.items); });
    }
}