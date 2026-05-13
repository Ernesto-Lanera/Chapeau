// =========================
// Chapeau Toast Popup
// =========================

function closeToast() {
    const toast = document.getElementById("toastMessage");

    if (!toast) {
        return;
    }

    toast.style.opacity = "0";
    toast.style.transform = "translateX(30px)";

    setTimeout(function () {
        toast.remove();
    }, 250);
}

document.addEventListener("DOMContentLoaded", function () {
    const toast = document.getElementById("toastMessage");

    if (toast) {
        setTimeout(function () {
            closeToast();
        }, 3500);
    }
});

const chapeauScrollStorageKey = "chapeau-scroll-position";

function saveChapeauScrollPosition() {
    sessionStorage.setItem(chapeauScrollStorageKey, window.scrollY.toString());
}

function restoreChapeauScrollPosition() {
    const savedPosition = sessionStorage.getItem(chapeauScrollStorageKey);

    if (savedPosition === null) {
        return;
    }

    const position = parseInt(savedPosition, 10);

    if (!isNaN(position)) {
        window.scrollTo({
            top: position,
            left: 0,
            behavior: "auto"
        });
    }

    sessionStorage.removeItem(chapeauScrollStorageKey);
}

document.addEventListener("DOMContentLoaded", function () {
    restoreChapeauScrollPosition();

    document.querySelectorAll("form").forEach(function (form) {
        form.addEventListener("submit", function () {
            saveChapeauScrollPosition();
        });
    });

    document.querySelectorAll(".preserve-scroll").forEach(function (element) {
        element.addEventListener("click", function () {
            saveChapeauScrollPosition();
        });
    });

    document.querySelectorAll(".preserve-scroll-form").forEach(function (form) {
        form.addEventListener("submit", function () {
            saveChapeauScrollPosition();
        });
    });
});