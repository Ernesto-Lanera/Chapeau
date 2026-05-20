// =========================
// Toast Host Management
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

// =========================
// Public Toast Function
// =========================

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
