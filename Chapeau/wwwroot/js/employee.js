document.addEventListener("DOMContentLoaded", function () {
    setupEmployeeForms();
});

function setupEmployeeForms() {
    const forms = document.querySelectorAll(".employee-form");

    forms.forEach(function (form) {
        form.addEventListener("submit", function (event) {
            const nameInput = form.querySelector('input[name="Name"]');
            const passwordInput = form.querySelector('input[name="Password"]');
            const roleSelect = form.querySelector('select[name="RoleID"]');

            clearValidation(form);
            let isValid = true;

            if (!nameInput || nameInput.value.trim() === "") {
                showInputError(nameInput, "Naam is verplicht.");
                isValid = false;
            }

            if (roleSelect && roleSelect.value === "") {
                showInputError(roleSelect, "Kies een rol.");
                isValid = false;
            }

            const isCreateForm = form.action.toLowerCase().includes("/create");
            if (isCreateForm && passwordInput && passwordInput.value.trim() === "") {
                showInputError(passwordInput, "Wachtwoord/pincode is verplicht bij toevoegen.");
                isValid = false;
            }

            if (!isValid) {
                event.preventDefault();
            }
        });
    });
}

function clearValidation(form) {
    form.querySelectorAll(".is-invalid").forEach(function (input) {
        input.classList.remove("is-invalid");
    });

    form.querySelectorAll(".client-validation-message").forEach(function (message) {
        message.remove();
    });
}

function showInputError(input, message) {
    if (!input) return;

    input.classList.add("is-invalid");
    const error = document.createElement("div");
    error.className = "text-danger small mt-1 client-validation-message";
    error.textContent = message;
    input.insertAdjacentElement("afterend", error);
}
