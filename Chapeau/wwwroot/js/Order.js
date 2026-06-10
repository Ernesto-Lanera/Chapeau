let isUpdating = false;
let currentCartItems = [];
let editingCommentId = null;

document.addEventListener('DOMContentLoaded', () => {
    fetch('/Menu/GetActiveOrderItems')
        .then(res => res.json())
        .then(data => { if (data.success) updateCartUI(data.items); });
});

function showNotification(message, type = "warning") {
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} alert-dismissible fade show`;
    alertDiv.setAttribute('role', 'alert');
    alertDiv.style.cssText = 'position: fixed; top: 20px; right: 20px; z-index: 1050; min-width: 300px; box-shadow: 0 4px 6px rgba(0,0,0,0.1);';

    alertDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;

    document.body.appendChild(alertDiv);


    setTimeout(() => {
        if (alertDiv.parentElement) {
            if (typeof bootstrap !== 'undefined') {
                const bsAlert = new bootstrap.Alert(alertDiv);
                bsAlert.close();
            } else {
                alertDiv.remove();
            }
        }
    }, 2000);
}

function updateCartUI(items) {
    currentCartItems = items;
    const cart = document.getElementById('cart-container');
    let htmlContent = '';

    items.forEach(item => {
        let commentSectionHtml = '';
        let isEditing = (editingCommentId === item.menuItemId);

        if (isEditing) {
            commentSectionHtml = `
                <div class="d-flex mt-2 w-100 gap-2 align-items-center">
                    <input type="text" id="comment-input-${item.menuItemId}" class="form-control form-control-sm rounded-pill px-3" placeholder="Type a note..." value="${item.comment || ''}">
                    <button class="btn btn-sm btn-success rounded-pill fw-bold px-3" onclick="saveComment(${item.menuItemId})">Add</button>
                </div>
            `;
        }
        else if (item.comment && item.comment.trim() !== '') {
            commentSectionHtml = `
                <div class="d-flex justify-content-between align-items-center mt-2 w-100">
                    <span class="text-light small ms-1">Note: ${item.comment}</span>
                    <button class="btn btn-sm btn-danger rounded-pill fw-bold" style="font-size: 0.75rem; padding: 0.2rem 0.6rem;" onclick="deleteComment(${item.menuItemId})">Delete</button>
                </div>
            `;
        }

        htmlContent += `
            <div class="mb-2 p-2 border rounded d-flex flex-column" style="background-color: #212529; color: white;">
                <div class="d-flex flex-wrap align-items-center justify-content-between w-100">
                    <span class="fw-bold text-truncate me-2" style="font-size: 0.9rem;">${item.menuItemName}</span>
                    
                    <div class="d-flex align-items-center gap-1">
                        <button class="btn btn-sm btn-outline-light rounded-pill px-2 py-0 d-flex align-items-center border-secondary" onclick="updateQuantity(${item.menuItemId}, ${item.amountOrdered - 1})">-</button>
                        <span id="qty-${item.menuItemId}" class="fw-bold mx-1" style="min-width: 15px; text-align: center; font-size: 0.9rem;">${item.amountOrdered}</span>
                        <button class="btn btn-sm btn-outline-light rounded-pill px-2 py-0 d-flex align-items-center border-secondary" onclick="updateQuantity(${item.menuItemId}, ${item.amountOrdered + 1})">+</button>
                        
                        <button class="btn btn-sm btn-outline-primary ms-1 rounded-pill px-2 py-0 d-flex align-items-center" onclick="toggleCommentInput(${item.menuItemId})">
                            <i class="bi bi-pencil-square" style="font-size: 0.8rem;"></i>
                        </button>

                        <button class="btn btn-sm btn-danger ms-1 rounded-pill fw-bold" style="font-size: 0.8rem; padding: 0.2rem 0.6rem;" onclick="removeFromOrder(${item.menuItemId})">
                            Delete
                        </button>
                    </div>
                </div>
                ${commentSectionHtml}
            </div>
        `;
    });

    cart.innerHTML = htmlContent;
}

function toggleCommentInput(menuItemId) {
    if (editingCommentId === menuItemId) {
        editingCommentId = null;
    } else {
        editingCommentId = menuItemId;
    }
    updateCartUI(currentCartItems);
}

function saveComment(menuItemId) {
    if (isUpdating) return;

    const inputField = document.getElementById(`comment-input-${menuItemId}`);
    const commentText = inputField ? inputField.value : '';

    isUpdating = true;
    fetch('/Menu/UpdateItemComment', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: new URLSearchParams({ 'MenuItemId': menuItemId, 'Comment': commentText })
    })
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                editingCommentId = null;
                updateCartUI(data.items);
            }
        })
        .finally(() => { isUpdating = false; });
}

function deleteComment(menuItemId) {
    if (isUpdating) return;

    isUpdating = true;
    fetch('/Menu/UpdateItemComment', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: new URLSearchParams({ 'MenuItemId': menuItemId, 'Comment': '' })
    })
        .then(res => res.json())
        .then(data => {
            if (data.success) updateCartUI(data.items);
        })
        .finally(() => { isUpdating = false; });
}

function addToOrder(menuItemId, menuItemName) {
    if (isUpdating) return;

    let maxStock = window.stockLevels[menuItemId] || 0;
    if (maxStock <= 0) return;

    let existingQtySpan = document.getElementById(`qty-${menuItemId}`);
    if (existingQtySpan) {
        let currentQty = parseInt(existingQtySpan.innerText);
        if (currentQty >= maxStock) {
            showNotification(`Maximum stock reached. Only ${maxStock} available.`);
            return;
        }
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

    let maxStock = window.stockLevels[menuItemId] || 0;
    if (newQuantity > maxStock) {
        showNotification(`Maximum stock reached. Only ${maxStock} available.`);
        return;
    }

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