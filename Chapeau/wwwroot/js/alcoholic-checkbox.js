document.addEventListener('DOMContentLoaded', function () {
    // Setup event listeners voor alcoholic checkboxes
    function setupAlcoholicCheckboxes() {
        // Create formulier checkbox
        const createCheckbox = document.getElementById('createIsAlcoholicCheck');
        if (createCheckbox) {
            setupCheckboxListener(createCheckbox, 'createIsAlcoholicHidden');
        }

        // Edit formulieren checkboxes
        const editCheckboxes = document.querySelectorAll('input[id^="editIsAlcoholicCheck-"]');
        editCheckboxes.forEach(checkbox => {
            const menuItemId = checkbox.id.replace('editIsAlcoholicCheck-', '');
            setupCheckboxListener(checkbox, `editIsAlcoholicHidden-${menuItemId}`);
        });
    }

    function setupCheckboxListener(checkbox, hiddenId) {
        const hiddenInput = document.getElementById(hiddenId);
        if (!hiddenInput) return;

        // Update hidden input when checkbox changes
        checkbox.addEventListener('change', function () {
            hiddenInput.value = this.checked ? 'true' : 'false';
        });

        // Set initial state
        hiddenInput.value = checkbox.checked ? 'true' : 'false';
    }

    setupAlcoholicCheckboxes();
});
