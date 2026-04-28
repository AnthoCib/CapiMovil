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
    document.querySelectorAll("form[data-confirm], form.form-eliminar").forEach(form => {
        if (!form.dataset.confirm && form.classList.contains("form-eliminar")) {
            form.dataset.confirm = "true";
        }

        form.addEventListener("submit", function (e) {
            if (form.dataset.confirmed === "1") {
                form.dataset.confirmed = "0";
                return;
            }

            e.preventDefault();

            Swal.fire({
                title: form.dataset.confirmTitle || "¿Estás seguro?",
                text: form.dataset.confirmText || "Esta acción eliminará el registro seleccionado y no se puede deshacer.",
                icon: form.dataset.confirmIcon || "warning",
                showCancelButton: true,
                confirmButtonText: form.dataset.confirmConfirmText || "Sí, eliminar",
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

    const acordeonSubmenus = (shell.dataset.sidebarAccordion || "true").toLowerCase() !== "false";
    const gruposSubmenu = Array.from(shell.querySelectorAll(".admin-nav-group"));

    const sincronizarAlturasSubmenu = () => {
        gruposSubmenu.forEach(group => {
            const panel = group.querySelector(".admin-submenu");
            if (!panel) return;

            if (group.classList.contains("open")) {
                panel.style.maxHeight = `${panel.scrollHeight}px`;
                panel.style.opacity = "1";
            } else {
                panel.style.maxHeight = "0px";
                panel.style.opacity = "0";
            }
        });
    };

    const cambiarEstadoSubmenu = (group, expandir) => {
        if (!group) return;

        const boton = group.querySelector("[data-sidebar-submenu-toggle]");
        if (expandir) {
            group.classList.add("open");
            if (boton) boton.setAttribute("aria-expanded", "true");
        } else {
            group.classList.remove("open");
            if (boton) boton.setAttribute("aria-expanded", "false");
        }
    };

    gruposSubmenu.forEach(group => {
        const boton = group.querySelector("[data-sidebar-submenu-toggle]");
        const panel = group.querySelector(".admin-submenu");
        if (!boton || !panel) return;

        const tieneRutaActiva = panel.querySelector(".nav-link.active") !== null;
        if (tieneRutaActiva) {
            group.classList.add("open");
            boton.setAttribute("aria-expanded", "true");
        }

        boton.addEventListener("click", () => {
            const estaAbierto = group.classList.contains("open");
            if (estaAbierto) {
                cambiarEstadoSubmenu(group, false);
                sincronizarAlturasSubmenu();
                return;
            }

            if (acordeonSubmenus) {
                gruposSubmenu
                    .filter(other => other !== group && other.querySelector(".admin-submenu"))
                    .forEach(other => cambiarEstadoSubmenu(other, false));
            }

            cambiarEstadoSubmenu(group, true);
            sincronizarAlturasSubmenu();
        });
    });

    sincronizarAlturasSubmenu();

    const toggle = shell.querySelector("[data-sidebar-toggle]");
    const backdrop = shell.querySelector("[data-sidebar-backdrop]");
    const navSelector = shell.dataset.sidebarNav || ".nav-link";
    const desktopBreakpoint = Number(shell.dataset.sidebarBreakpoint || 1200);

    if (!toggle || !backdrop) return;

    const syncSidebarState = () => {
        const isOpen = shell.classList.contains("sidebar-open");
        toggle.setAttribute("aria-expanded", isOpen ? "true" : "false");

        if (window.innerWidth < desktopBreakpoint) {
            document.body.classList.toggle("overflow-hidden", isOpen);
        } else {
            document.body.classList.remove("overflow-hidden");
        }
    };

    const openSidebar = () => {
        shell.classList.add("sidebar-open");
        syncSidebarState();
    };

    const closeSidebar = () => {
        shell.classList.remove("sidebar-open");
        syncSidebarState();
    };

    toggle.addEventListener("click", () => {
        if (shell.classList.contains("sidebar-open")) {
            closeSidebar();
            return;
        }

        openSidebar();
    });

    backdrop.addEventListener("click", closeSidebar);

    shell.querySelectorAll(navSelector).forEach(link => {
        link.addEventListener("click", () => {
            if (window.innerWidth < desktopBreakpoint) {
                closeSidebar();
            }
        });
    });

    window.addEventListener("resize", () => {
        sincronizarAlturasSubmenu();

        if (window.innerWidth >= desktopBreakpoint) {
            closeSidebar();
        } else {
            syncSidebarState();
        }
    });

    document.addEventListener("keydown", (event) => {
        if (event.key === "Escape" && shell.classList.contains("sidebar-open")) {
            closeSidebar();
        }
    });

    syncSidebarState();
}

function inicializarSidebarsDesdeData() {
    document.querySelectorAll("[data-sidebar-shell][id]").forEach(shell => inicializarSidebarShellById(shell.id));
}

window.CapiMovilUI = {
    initSidebarShell: inicializarSidebarShellById
};
