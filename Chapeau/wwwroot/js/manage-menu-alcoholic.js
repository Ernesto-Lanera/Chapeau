document.addEventListener('DOMContentLoaded', function () {
    // Alcoholische categorieën
    const alcoholicCategories = ['Bieren van de tap', 'Champagne', 'Gedistilleerde dranken', 'Wijnen'];

    function toggleAlcoholicCheckbox(selectElement) {
        // Haal geselecteerde text
        const selectedOption = selectElement.options[selectElement.selectedIndex];
        const selectedText = selectedOption.innerText.trim();

        console.log('Selected category:', selectedText);

        // Check of het alcoholisch is
        const isAlcoholic = alcoholicCategories.some(cat => selectedText.includes(cat));

        console.log('Is alcoholic:', isAlcoholic);

        // Vind de checkbox container
        const form = selectElement.closest('form');
        if (!form) return;

        const formId = form.id || form.action;
        let checkboxGroup = null;

        // Bepaal welke checkbox container
        if (selectElement.id === 'createCategorySelect') {
            checkboxGroup = document.getElementById('createIsAlcoholicGroup');
        } else {
            // Voor edit formulieren
            const menuItemInput = form.querySelector('input[name="MenuItemID"]');
            if (menuItemInput) {
                const itemId = menuItemInput.value;
                checkboxGroup = document.getElementById(`editIsAlcoholicGroup-${itemId}`);
            }
        }

        // Toon/verberg de checkbox
        if (checkboxGroup) {
            checkboxGroup.style.display = isAlcoholic ? 'block' : 'none';
            console.log('Checkbox group updated:', checkboxGroup.id, 'display:', isAlcoholic ? 'block' : 'none');
        } else {
            console.log('Checkbox group not found');
        }
    }

    // Setup event listeners
    function setupListeners() {
        // Create formulier
        const createSelect = document.getElementById('createCategorySelect');
        if (createSelect) {
            createSelect.addEventListener('change', function () {
                toggleAlcoholicCheckbox(this);
            });
            // Initiële state
            setTimeout(() => toggleAlcoholicCheckbox(createSelect), 100);
        }

        // Edit formulieren
        const editSelects = document.querySelectorAll('form[action*="Edit"] select[name="CategoryID"]');
        editSelects.forEach(select => {
            select.addEventListener('change', function () {
                toggleAlcoholicCheckbox(this);
            });
            // Initiële state
            setTimeout(() => toggleAlcoholicCheckbox(select), 100);
        });
    }

    setupListeners();
});