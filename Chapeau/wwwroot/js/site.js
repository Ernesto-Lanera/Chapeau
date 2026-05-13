<<<<<<< HEAD
﻿// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener("DOMContentLoaded", function () {
  // Auto-dismiss all alerts/popups after 3.5 seconds.
  // Exclusions: alerts marked with data-autodismiss="false"
  var alerts = document.querySelectorAll(".alert:not([data-autodismiss='false'])");
  function fadeRemove(el) {
    try {
      el.style.transition = "opacity 250ms ease, transform 250ms ease";
      el.style.opacity = "0";
      el.style.transform = "translateY(-4px)";
      setTimeout(function () {
        if (el && el.parentNode) el.parentNode.removeChild(el);
      }, 260);
    } catch {
      // ignore
    }
  }
=======
﻿// =========================
// Chapeau Toast Popup
// =========================
>>>>>>> parent of 9c2d454 (Revert "popup beter gemaakt en de bewerk positief gefixt")

function closeToast() {
    const toast = document.getElementById("toastMessage");

<<<<<<< HEAD
  // Toasts for AJAX updates
  window.showToast = function (message, type) {
    var toastHost = document.getElementById("toastHost");
    if (!toastHost) {
      toastHost = document.createElement("div");
      toastHost.id = "toastHost";
      toastHost.style.position = "fixed";
      toastHost.style.top = "16px";
      toastHost.style.right = "16px";
      toastHost.style.zIndex = "1080";
      toastHost.style.display = "flex";
      toastHost.style.flexDirection = "column";
      toastHost.style.gap = "8px";
      document.body.appendChild(toastHost);
=======
    if (!toast) {
        return;
>>>>>>> parent of 9c2d454 (Revert "popup beter gemaakt en de bewerk positief gefixt")
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