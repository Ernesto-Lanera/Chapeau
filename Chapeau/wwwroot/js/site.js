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

    document.addEventListener("DOMContentLoaded", function () {
        function removeElementWithFade(el) {
            if (!el) {
                return;
            }

            el.style.transition = "opacity 250ms ease, transform 250ms ease";
            el.style.opacity = "0";
            el.style.transform = "translateY(-6px)";

            setTimeout(function () {
                if (el && el.parentNode) {
                    el.parentNode.removeChild(el);
                }
            }, 260);
        }

        var flashAlerts = document.querySelectorAll(".flash-alert:not([data-autodismiss='false'])");

        flashAlerts.forEach(function (alert) {
            setTimeout(function () {
                removeElementWithFade(alert);
            }, 3500);
        });

        function ensureToastHost() {
            var toastHost = document.getElementById("toastHost");

            if (!toastHost) {
                toastHost = document.createElement("div");
                toastHost.id = "toastHost";
                toastHost.className = "toast-host";
                document.body.appendChild(toastHost);
            }

            return toastHost;
        }

        function removeToast(el) {
            if (!el) {
                return;
            }

            el.classList.add("toast-hide");

            setTimeout(function () {
                if (el && el.parentNode) {
                    el.parentNode.removeChild(el);
                }
            }, 300);
        }

        window.showToast = function (message, type) {
            var toastHost = ensureToastHost();

            var toast = document.createElement("div");

            if (type === "error") {
                toast.className = "toast-message toast-error";
            } else if (type === "warning") {
                toast.className = "toast-message toast-warning";
            } else {
                toast.className = "toast-message toast-success";
            }

            toast.setAttribute("role", "alert");

            var text = document.createElement("span");
            text.textContent = message;

            var closeButton = document.createElement("button");
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
    });
});