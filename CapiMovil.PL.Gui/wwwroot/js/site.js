document.addEventListener("DOMContentLoaded", function () {
    [
        { inputId: "adminGlobalSearch", resultsId: "adminSearchResults", dataNodeId: "admin-search-data", itemClassName: "admin-search-item" },
        { inputId: "conductorGlobalSearch", resultsId: "conductorSearchResults", dataNodeId: "conductor-search-data", itemClassName: "conductor-search-item" },
        { inputId: "padreGlobalSearch", resultsId: "padreSearchResults", dataNodeId: "padre-search-data", itemClassName: "padre-search-item" }
    ].forEach(inicializarBuscadorGlobal);

    [
        { toggleId: "adminNotificationToggle", panelId: "adminNotificationPanel", listId: "adminNotificationList", dataNodeId: "admin-notification-data" },
        { toggleId: "conductorNotificationToggle", panelId: "conductorNotificationPanel", listId: "conductorNotificationList", dataNodeId: "conductor-notification-data" },
        { toggleId: "padreNotificationToggle", panelId: "padreNotificationPanel", listId: "padreNotificationList", dataNodeId: "padre-notification-data" }
    ].forEach(inicializarPanelNotificaciones);

    inicializarSidebarsDesdeData();
    inicializarConfirmacionesGlobales();
    inicializarFeedbackEnvioFormularios();
});

function inicializarBuscadorGlobal(config) {
    const input = document.getElementById(config.inputId);
    const results = document.getElementById(config.resultsId);
    const dataNode = document.getElementById(config.dataNodeId);

    if (!input || !results || !dataNode) {
        return;
    }

    let modules = [];
    try {
        modules = JSON.parse(dataNode.textContent || "[]");
    } catch {
        modules = [];
    }

    if (!Array.isArray(modules) || modules.length === 0) {
        return;
    }

    let activeIndex = -1;

    const hideResults = () => {
        results.classList.add("d-none");
        results.innerHTML = "";
        activeIndex = -1;
    };

    const renderResults = (query) => {
        const term = (query || "").trim().toLowerCase();
        if (!term) {
            hideResults();
            return;
        }

        const filtrados = modules
            .map(item => ({
                texto: item.texto || item.Texto || "",
                url: item.url || item.Url || "",
                keywords: (item.keywords || item.Keywords || "").toLowerCase()
            }))
            .filter(item => (`${item.texto} ${item.keywords}`.toLowerCase()).includes(term))
            .slice(0, 8);

        if (filtrados.length === 0) {
            results.innerHTML = '<button type="button" class="list-group-item list-group-item-action disabled">Sin resultados para este rol</button>';
            results.classList.remove("d-none");
            activeIndex = -1;
            input.dataset.filtered = "[]";
            return;
        }

        results.innerHTML = filtrados
            .map((item, index) =>
                `<button type="button" class="list-group-item list-group-item-action ${config.itemClassName}" data-index="${index}" data-url="${item.url}">
                    ${item.texto}
                </button>`)
            .join("");

        results.classList.remove("d-none");
        activeIndex = -1;

        const rows = results.querySelectorAll(`.${config.itemClassName}`);
        rows.forEach(row => {
            row.addEventListener("click", () => {
                const url = row.getAttribute("data-url");
                if (url) window.location.href = url;
            });
        });

        input.dataset.filtered = JSON.stringify(filtrados);
    };

    input.addEventListener("input", (e) => renderResults(e.target.value));

    input.addEventListener("keydown", (e) => {
        const items = results.querySelectorAll(`.${config.itemClassName}`);
        const list = JSON.parse(input.dataset.filtered || "[]");

        if (e.key === "Enter") {
            e.preventDefault();
            const target = activeIndex >= 0 ? list[activeIndex] : list[0];
            if (target?.url) window.location.href = target.url;
            return;
        }

        if (results.classList.contains("d-none") || items.length === 0) return;

        if (e.key === "ArrowDown") {
            e.preventDefault();
            activeIndex = (activeIndex + 1) % items.length;
        } else if (e.key === "ArrowUp") {
            e.preventDefault();
            activeIndex = (activeIndex - 1 + items.length) % items.length;
        } else if (e.key === "Escape") {
            hideResults();
            return;
        }

        items.forEach((item, i) => item.classList.toggle("active", i === activeIndex));
    });

    input.addEventListener("focus", () => {
        if (input.value.trim()) renderResults(input.value);
    });

    document.addEventListener("click", (e) => {
        if (!results.contains(e.target) && e.target !== input) hideResults();
    });
}

function inicializarPanelNotificaciones(config) {
    const toggle = document.getElementById(config.toggleId);
    const panel = document.getElementById(config.panelId);
    const list = document.getElementById(config.listId);
    const dataNode = document.getElementById(config.dataNodeId);

    if (!toggle || !panel || !list || !dataNode) return;

    let notifications = [];
    try { notifications = JSON.parse(dataNode.textContent || "[]"); } catch { notifications = []; }

    if (!Array.isArray(notifications) || notifications.length === 0) {
        list.innerHTML = `
            <div class="list-group-item text-center py-3">
                <i class="bi bi-bell-slash text-muted d-block mb-1"></i>
                <small class="text-muted">Sin notificaciones recientes</small>
            </div>`;
    } else {
        list.innerHTML = notifications
            .slice(0, 6)
            .map(item => {
                const texto = item.texto || item.Texto || "Notificación";
                const url = item.url || item.Url || "#";
                const tiempo = item.tiempo || item.Tiempo || "Reciente";
                return `<a href="${url}" class="list-group-item list-group-item-action">
                            <div class="fw-semibold small">${texto}</div>
                            <small class="text-muted">${tiempo}</small>
                        </a>`;
            }).join("");
    }

    const hidePanel = () => {
        panel.classList.add("d-none");
        toggle.setAttribute("aria-expanded", "false");
    };

    toggle.addEventListener("click", (e) => {
        e.stopPropagation();
        panel.classList.toggle("d-none");
        toggle.setAttribute("aria-expanded", panel.classList.contains("d-none") ? "false" : "true");
    });

    panel.addEventListener("click", e => e.stopPropagation());
    document.addEventListener("click", hidePanel);
}

function inicializarConfirmacionesGlobales() {
    document.querySelectorAll("form[data-confirm]").forEach(form => {
        form.addEventListener("submit", function (e) {
            if (form.dataset.confirmed === "1") {
                form.dataset.confirmed = "0";
                return;
            }

            e.preventDefault();

            Swal.fire({
                title: form.dataset.confirmTitle || "¿Confirmar acción?",
                text: form.dataset.confirmText || "Esta acción requiere confirmación.",
                icon: form.dataset.confirmIcon || "question",
                showCancelButton: true,
                confirmButtonText: form.dataset.confirmConfirmText || "Confirmar",
                cancelButtonText: form.dataset.confirmCancelText || "Cancelar",
                reverseButtons: true
            }).then((result) => {
                if (result.isConfirmed) {
                    form.dataset.confirmed = "1";
                    if (typeof form.requestSubmit === "function") {
                        form.requestSubmit();
                    } else {
                        form.submit();
                    }
                }
            });
        });
    });
}

function inicializarFeedbackEnvioFormularios() {
    document.querySelectorAll("form[data-disable-on-submit]").forEach(form => {
        form.addEventListener("submit", function () {
            if (typeof form.checkValidity === "function" && !form.checkValidity()) {
                return;
            }

            const submitButtons = form.querySelectorAll('button[type="submit"], input[type="submit"]');
            submitButtons.forEach(btn => {
                if (btn.disabled) return;
                if (btn.dataset.loadingText) {
                    btn.dataset.originalHtml = btn.innerHTML;
                    btn.innerHTML = `<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>${btn.dataset.loadingText}`;
                }
                btn.disabled = true;
                btn.classList.add("disabled");
            });
        });
    });
}

function inicializarSidebarShellById(shellId) {
    const shell = document.getElementById(shellId);
    if (!shell) return;

    const toggle = shell.querySelector("[data-sidebar-toggle]");
    const backdrop = shell.querySelector("[data-sidebar-backdrop]");
    const navSelector = shell.dataset.sidebarNav || ".nav-link";
    const desktopBreakpoint = Number(shell.dataset.sidebarBreakpoint || 1200);

    if (!toggle || !backdrop) return;

    toggle.addEventListener("click", () => shell.classList.toggle("sidebar-open"));
    backdrop.addEventListener("click", () => shell.classList.remove("sidebar-open"));

    shell.querySelectorAll(navSelector).forEach(link => {
        link.addEventListener("click", () => shell.classList.remove("sidebar-open"));
    });

    window.addEventListener("resize", () => {
        if (window.innerWidth >= desktopBreakpoint) shell.classList.remove("sidebar-open");
    });
}

function inicializarSidebarsDesdeData() {
    document.querySelectorAll("[data-sidebar-shell][id]").forEach(shell => inicializarSidebarShellById(shell.id));
}

window.CapiMovilUI = {
    initSidebarShell: inicializarSidebarShellById
};
