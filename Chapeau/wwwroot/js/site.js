// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
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

  if (alerts.length) {
    setTimeout(function () {
      alerts.forEach(fadeRemove);
    }, 3500);
  }

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
    }

    var el = document.createElement("div");
    el.className = "alert flash-alert " + (type === "error" ? "alert-danger" : "alert-success");
    el.setAttribute("role", "alert");
    el.style.margin = "0";
    el.style.minWidth = "260px";
    el.style.maxWidth = "360px";
    el.style.opacity = "1";
    el.textContent = message;
    toastHost.appendChild(el);

    setTimeout(function () {
      fadeRemove(el);
    }, 3500);
  };
});
