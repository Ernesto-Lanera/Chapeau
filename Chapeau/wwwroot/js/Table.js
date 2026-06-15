/**
 * Manages the guest count modal on the table overview page.
 * Handles selecting guest count, entering guest names, and submitting the form.
 */

var currentTableId = 0;

/** Opens the guest modal for a specific table and defaults to 2 guests. */
function openGuestModal(tableId, tableNumber) {
    currentTableId = tableId;
    document.getElementById('selected-guests').value = 2;
    var options = document.querySelectorAll('.guest-option');
    options.forEach(function(btn) {
        btn.classList.remove('selected');
    });
    options[1].classList.add('selected');
    document.getElementById('guest-modal').classList.add('show');
    renderGuestNameFields(2);
}

/** Closes the guest modal without submitting. */
function closeGuestModal() {
    document.getElementById('guest-modal').classList.remove('show');
}

/** Sets the selected guest count and re-renders name input fields. */
function setGuestCount(count) {
    document.getElementById('selected-guests').value = count;
    var options = document.querySelectorAll('.guest-option');
    options.forEach(function(btn, index) {
        if (index + 1 === count) {
            btn.classList.add('selected');
        } else {
            btn.classList.remove('selected');
        }
    });
    renderGuestNameFields(count);
}

/** Dynamically creates name input fields for each guest. */
function renderGuestNameFields(count) {
    var container = document.getElementById('guest-name-fields');
    var html = '';
    for (var i = 1; i <= count; i++) {
        html += '<div class="guest-field-container">' +
            '<label class="guest-field-label" for="guest-name-' + i + '">Gast ' + i + '</label>' +
            '<input type="text" id="guest-name-' + i + '" class="guest-field-input" placeholder="Naam gast ' + i + '" />' +
            '</div>';
    }
    container.innerHTML = html;
    var firstInput = document.getElementById('guest-name-1');
    if (firstInput) firstInput.focus();
}

/** Collects guest names, stores them in the hidden field, and submits the form. */
function confirmGuest() {
    if (currentTableId === 0) return;
    var count = parseInt(document.getElementById('selected-guests').value);
    var names = [];
    for (var i = 1; i <= count; i++) {
        var input = document.getElementById('guest-name-' + i);
        var name = input ? input.value.trim() : '';
        names.push(name || 'Gast ' + i);
    }
    document.getElementById('guest-names-' + currentTableId).value = names.join(', ');
    closeGuestModal();
    var form = document.getElementById('available-form-' + currentTableId);
    if (form) {
        form.submit();
    }
}
