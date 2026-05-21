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
        let checkbox = null;

        // Bepaal welke checkbox container
        if (selectElement.id === 'createCategorySelect') {
            checkboxGroup = document.getElementById('createIsAlcoholicGroup');
            checkbox = document.getElementById('createIsAlcoholicCheck');
        } else {
            // Voor edit formulieren
            const menuItemInput = form.querySelector('input[name="MenuItemID"]');
            if (menuItemInput) {
                const itemId = menuItemInput.value;
                checkboxGroup = document.getElementById(`editIsAlcoholicGroup-${itemId}`);
                checkbox = document.getElementById(`editIsAlcoholicCheck-${itemId}`);
            }
        }

        // Toon/verberg de checkbox
        if (checkboxGroup) {
            // Als checkbox is aangevinkt, toon altijd
            const isChecked = checkbox && checkbox.checked;
            checkboxGroup.style.display = (isAlcoholic || isChecked) ? 'block' : 'none';
            console.log('Checkbox group updated:', checkboxGroup.id, 'display:', (isAlcoholic || isChecked) ? 'block' : 'none');
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
            // Initiële state - belangrijk voor edit formulieren
            setTimeout(() => toggleAlcoholicCheckbox(select), 100);
        });

        // Ook luisteren naar checkbox changes zodat het visibility onmiddellijk updates
        const allCheckboxes = document.querySelectorAll('input[name="IsAlcoholic"][type="checkbox"]');
        allCheckboxes.forEach(checkbox => {
            checkbox.addEventListener('change', function () {
                const form = this.closest('form');
                const categorySelect = form.querySelector('select[name="CategoryID"]');
                if (categorySelect) {
                    toggleAlcoholicCheckbox(categorySelect);
                }
            });
        });
    }

    setupListeners();
});
