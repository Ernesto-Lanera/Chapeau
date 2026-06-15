/**
 * Manages the client-side shopping cart for menu ordering.
 * Cart state is persisted in sessionStorage and can be pre-populated
 * when editing an existing order (via window.existingCart).
 */

let currentCartItems = [];
let editingCommentId = null;

document.addEventListener('DOMContentLoaded', () => {
    if (window.existingCart && Array.isArray(window.existingCart) && window.existingCart.length > 0) {
        currentCartItems = window.existingCart;
        sessionStorage.setItem('chapeau_cart', JSON.stringify(currentCartItems));
    } else {
        try {
            currentCartItems = JSON.parse(sessionStorage.getItem('chapeau_cart')) || [];
        } catch (e) {
            currentCartItems = [];
        }
    }
    updateCartUI(currentCartItems);
});
    }
});

/** Shows a dismissible toast notification in the top-right corner. */
function showNotification(message, type = "warning") {
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} alert-dismissible fade show`;
    alertDiv.setAttribute('role', 'alert');
    alertDiv.style.cssText = 'position: fixed; top: 20px; right: 20px; z-index: 1050; min-width: 300px; box-shadow: 0 4px 6px rgba(0,0,0,0.1);';
    alertDiv.innerHTML = `${message}<button type="button" class="btn-close" data-bs-dismiss="alert"></button>`;
    document.body.appendChild(alertDiv);

    setTimeout(() => {
        if (alertDiv.parentElement) {
            typeof bootstrap !== 'undefined' ? new bootstrap.Alert(alertDiv).close() : alertDiv.remove();
        }
    }, 2000);
}

function toggleLoadingState(isLoading) {
    const submitBtn = document.getElementById('submitOrderBtn');
    const submitText = document.getElementById('submitText');
    const submitSpinner = document.getElementById('submitSpinner');

    if (submitBtn) submitBtn.disabled = isLoading;
    if (submitText) submitText.textContent = isLoading ? 'Saving...' : 'Send To Kitchen';

    if (submitSpinner) {
        if (isLoading) submitSpinner.classList.remove('d-none');
        else submitSpinner.classList.add('d-none');
    }
}

/** Saves the value from the currently open comment input before a UI re-render. */
function preserveActiveComment() {
    if (editingCommentId === null) return '';
    const input = document.getElementById(`comment-input-${editingCommentId}`);
    return input ? input.value : '';
}

/** Re-focuses the comment input after a UI re-render. */
function restoreActiveComment() {
    if (editingCommentId === null) return;
    const input = document.getElementById(`comment-input-${editingCommentId}`);
    if (input) {
        input.focus();
        const val = input.value;
        input.value = '';
        input.value = val;
    }
}

/** Generates HTML for the comment section of a cart item (input field or display with delete). */
function buildCommentHtml(item, isEditing, activeCommentValue) {
    if (isEditing) {
        const displayValue = activeCommentValue || item.comment || '';
        return `
            <div class="d-flex mt-2 w-100 gap-2 align-items-center">
                <input type="text" id="comment-input-${item.menuItemId}" class="form-control form-control-sm rounded-pill px-3" value="${displayValue}">
                <button class="btn btn-sm btn-success rounded-pill fw-bold px-3" onclick="saveComment(${item.menuItemId})">Add</button>
            </div>`;
    }
    if (item.comment && item.comment.trim() !== '') {
        return `
            <div class="d-flex justify-content-between align-items-center mt-2 w-100">
                <span class="text-light small ms-1">Note: ${item.comment}</span>
                <button class="btn btn-sm btn-danger rounded-pill fw-bold" style="font-size: 0.75rem; padding: 0.2rem 0.6rem;" onclick="deleteComment(${item.menuItemId})">Delete</button>
            </div>`;
    }
    return '';
}

/** Generates the full HTML for a single cart item card (name, quantity controls, comment, delete). */
function buildCartItemHtml(item, activeCommentValue) {
    const isEditing = (editingCommentId === item.menuItemId);
    const commentHtml = buildCommentHtml(item, isEditing, activeCommentValue);

    return `
        <div class="mb-2 p-2 border rounded d-flex flex-column" style="background-color: #212529; color: white;">
            <div class="d-flex flex-wrap align-items-center justify-content-between w-100">
                <span class="fw-bold text-truncate me-2" style="font-size: 0.9rem;">${item.menuItemName}</span>
                <div class="d-flex align-items-center gap-1">
                    <button class="btn btn-sm btn-outline-light rounded-pill px-2 py-0 d-flex align-items-center border-secondary" onclick="adjustQuantity(${item.menuItemId}, -1)">-</button>
                    <span id="qty-${item.menuItemId}" class="fw-bold mx-1" style="min-width: 15px; text-align: center; font-size: 0.9rem;">${item.amountOrdered}</span>
                    <button class="btn btn-sm btn-outline-light rounded-pill px-2 py-0 d-flex align-items-center border-secondary" onclick="adjustQuantity(${item.menuItemId}, 1)">+</button>
                    <button class="btn btn-sm btn-outline-primary ms-1 rounded-pill px-2 py-0 d-flex align-items-center" onclick="toggleCommentInput(${item.menuItemId})"><i class="bi bi-pencil-square" style="font-size: 0.8rem;"></i></button>
                    <button class="btn btn-sm btn-danger ms-1 rounded-pill fw-bold" style="font-size: 0.8rem; padding: 0.2rem 0.6rem;" onclick="removeFromOrder(${item.menuItemId})">Delete</button>
                </div>
            </div>
            ${commentHtml}
        </div>`;
}

/** Re-renders the entire cart container from the currentCartItems array. */
function updateCartUI() {
    const activeCommentValue = preserveActiveComment();
    document.getElementById('cart-container').innerHTML = currentCartItems.map(item => buildCartItemHtml(item, activeCommentValue)).join('');
    restoreActiveComment();
}

/** Persists the current cart to sessionStorage and re-renders the UI. */
function saveCartState() {
    try {
        sessionStorage.setItem('chapeau_cart', JSON.stringify(currentCartItems));
    } catch (e) {
        showNotification("Warning: Could not save cart locally.", "warning");
    }
    updateCartUI();
}

function getStockLimit(menuItemId) {
    return window.stockLevels ? (window.stockLevels[menuItemId] || 0) : 999;
}

function checkStockAvailability(menuItemId, requestedQty) {
    const maxStock = getStockLimit(menuItemId);
    if (requestedQty > maxStock) {
        showNotification(`Maximum stock reached. Only ${maxStock} available.`);
        return false;
    }
    return true;
}

/** Adds an item to the cart, incrementing quantity if already present. Checks stock limits. */
function addToOrder(menuItemId, menuItemName) {
    if (getStockLimit(menuItemId) <= 0) {
        showNotification("Item out of stock.", "danger");
        return;
    }

    let existingItem = currentCartItems.find(i => i.menuItemId === menuItemId);

    if (existingItem) {
        if (!checkStockAvailability(menuItemId, existingItem.amountOrdered + 1)) return;
        existingItem.amountOrdered += 1;
    } else {
        currentCartItems.push({
            menuItemId: menuItemId,
            menuItemName: menuItemName,
            amountOrdered: 1,
            comment: ""
        });
    }

    saveCartState();
}

/** Increases or decreases the quantity of a cart item. Removes the item if quantity reaches zero. */
function adjustQuantity(menuItemId, delta) {
    let item = currentCartItems.find(i => i.menuItemId === menuItemId);

    if (item) {
        let newQty = item.amountOrdered + delta;

        if (!checkStockAvailability(menuItemId, newQty)) return;

        item.amountOrdered = newQty;
        if (item.amountOrdered <= 0) {
            removeFromOrder(menuItemId);
        } else {
            saveCartState();
        }
    }
}

/** Removes an item from the cart entirely. */
function removeFromOrder(menuItemId) {
    currentCartItems = currentCartItems.filter(i => i.menuItemId !== menuItemId);
    saveCartState();
}

/** Toggles the comment editing mode for a cart item. */
function toggleCommentInput(menuItemId) {
    editingCommentId = (editingCommentId === menuItemId) ? null : menuItemId;
    updateCartUI();
}

/** Saves the comment text from the input field to the cart item. */
function saveComment(menuItemId) {
    const inputField = document.getElementById(`comment-input-${menuItemId}`);
    let item = currentCartItems.find(i => i.menuItemId === menuItemId);

    if (item && inputField) {
        item.comment = inputField.value;
        editingCommentId = null;
        saveCartState();
    }
}

/** Clears the comment for a cart item. */
function deleteComment(menuItemId) {
    let item = currentCartItems.find(i => i.menuItemId === menuItemId);
    if (item) {
        item.comment = "";
        saveCartState();
    }
}

/** Clears the cart and redirects back to the table overview. */
function cancelLocalOrder() {
    sessionStorage.removeItem('chapeau_cart');
    currentCartItems = [];
    window.location.href = '/Table/Index';
}

function getTableIdFromUrl() {
    const urlParams = new URLSearchParams(window.location.search);
    return parseInt(urlParams.get('tableId')) || 0;
}

function buildOrderPayload() {
    return {
        TableId: getTableIdFromUrl(),
        Items: currentCartItems.map(item => ({
            MenuItemId: item.menuItemId,
            Amount: item.amountOrdered,
            Comment: item.comment
        }))
    };
}

function handleServerResponse(data) {
    if (data.success) {
        sessionStorage.removeItem('chapeau_cart');
        currentCartItems = [];
        window.location.href = data.redirectUrl;
    } else {
        showNotification(data.message || "Failed to save the order.", "danger");
        toggleLoadingState(false);
    }
}

/** Submits the current cart as a JSON payload to the server. Supports both new orders and updates to existing orders. */
function submitOrderToServer() {
    if (currentCartItems.length === 0) {
        showNotification("Cannot send an empty order.", "warning");
        return;
    }

    toggleLoadingState(true);

    fetch('/Order/SaveOrderToDb', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(buildOrderPayload())
    })
        .then(res => {
            if (!res.ok) throw new Error("Server error");
            return res.json();
        })
        .then(data => handleServerResponse(data))
        .catch(() => {
            showNotification("Network error. Please check your connection and try again.", "danger");
            toggleLoadingState(false);
        });
}