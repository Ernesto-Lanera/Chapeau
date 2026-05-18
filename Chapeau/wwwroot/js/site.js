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
        if (toast.parentNode) {
            toast.parentNode.removeChild(toast);
        }
    }, 250);
}

function setupToastMessage() {
    const toast = document.getElementById("toastMessage");

    if (!toast) {
        return;
    }

    setTimeout(function () {
        closeToast();
    }, 3500);
}

// =========================
// Scroll position
// =========================

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

function setupPreserveScroll() {
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
}

// =========================
// Flash alerts
// =========================

function removeElementWithFade(element) {
    if (!element) {
        return;
    }

    element.style.transition = "opacity 250ms ease, transform 250ms ease";
    element.style.opacity = "0";
    element.style.transform = "translateY(-6px)";

    setTimeout(function () {
        if (element.parentNode) {
            element.parentNode.removeChild(element);
        }
    }, 260);
}

function setupFlashAlerts() {
    const flashAlerts = document.querySelectorAll(".flash-alert:not([data-autodismiss='false'])");

    flashAlerts.forEach(function (alert) {
        setTimeout(function () {
            removeElementWithFade(alert);
        }, 3500);
    });
}

// =========================
// Dynamic toast function
// =========================

function ensureToastHost() {
    let toastHost = document.getElementById("toastHost");

    if (!toastHost) {
        toastHost = document.createElement("div");
        toastHost.id = "toastHost";
        toastHost.className = "toast-host";
        document.body.appendChild(toastHost);
    }

    return toastHost;
}

function removeToast(element) {
    if (!element) {
        return;
    }

    element.classList.add("toast-hide");

    setTimeout(function () {
        if (element.parentNode) {
            element.parentNode.removeChild(element);
        }
    }, 300);
}

window.showToast = function (message, type) {
    const toastHost = ensureToastHost();
    const toast = document.createElement("div");

    if (type === "error") {
        toast.className = "toast-message toast-error";
    } else if (type === "warning") {
        toast.className = "toast-message toast-warning";
    } else {
        toast.className = "toast-message toast-success";
    }

    toast.setAttribute("role", "alert");

    const text = document.createElement("span");
    text.textContent = message;

    const closeButton = document.createElement("button");
    closeButton.type = "button";
    closeButton.className = "toast-close";
    closeButton.innerHTML = "&times;";

    closeButton.addEventListener("click", function () {
        removeToast(toast);
    });

    toast.appendChild(text);
    toast.appendChild(closeButton);

    toastHost.appendChild(toast);

    setTimeout(function () {
        removeToast(toast);
    }, 3500);
};

// =========================
// Init
// =========================

document.addEventListener("DOMContentLoaded", function () {
    restoreChapeauScrollPosition();
    setupPreserveScroll();
    setupToastMessage();
    setupFlashAlerts();
});