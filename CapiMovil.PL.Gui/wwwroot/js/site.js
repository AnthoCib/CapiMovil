document.addEventListener("DOMContentLoaded", function () {
    /*
     * Inicializa los buscadores globales según el rol.
     * Cada buscador tiene su input, contenedor de resultados,
     * nodo JSON con datos y clase CSS para los items encontrados.
     */
    [
        { inputId: "adminGlobalSearch", resultsId: "adminSearchResults", dataNodeId: "admin-search-data", itemClassName: "admin-search-item" },
        { inputId: "conductorGlobalSearch", resultsId: "conductorSearchResults", dataNodeId: "conductor-search-data", itemClassName: "conductor-search-item" },
        { inputId: "padreGlobalSearch", resultsId: "padreSearchResults", dataNodeId: "padre-search-data", itemClassName: "padre-search-item" }
    ].forEach(inicializarBuscadorGlobal);

    /*
     * Inicializa los paneles de notificaciones de cada rol.
     */
    [
        { toggleId: "adminNotificationToggle", panelId: "adminNotificationPanel", listId: "adminNotificationList", dataNodeId: "admin-notification-data" },
        { toggleId: "conductorNotificationToggle", panelId: "conductorNotificationPanel", listId: "conductorNotificationList", dataNodeId: "conductor-notification-data" },
        { toggleId: "padreNotificationToggle", panelId: "padreNotificationPanel", listId: "padreNotificationList", dataNodeId: "padre-notification-data" }
    ].forEach(inicializarPanelNotificaciones);

    /*
     * Inicializa los sidebars compactos/responsivos.
     */
    inicializarSidebarsDesdeData();

    /*
     * Inicializa confirmaciones globales con SweetAlert2.
     */
    inicializarConfirmacionesGlobales();

    /*
     * Inicializa el efecto de carga en los formularios.
     * Este es el método que muestra el spinner al iniciar sesión
     * o al enviar cualquier formulario configurado.
     */
    inicializarFeedbackEnvioFormularios();
});


/**
 * Inicializa un buscador global para un rol específico.
 *
 * Requiere:
 * - Un input de búsqueda.
 * - Un contenedor donde se mostrarán los resultados.
 * - Un nodo HTML con datos JSON de los módulos disponibles.
 *
 * Permite:
 * - Filtrar opciones por texto.
 * - Mostrar resultados dinámicos.
 * - Navegar con flechas.
 * - Entrar con Enter.
 * - Cerrar con Escape o clic fuera.
 */
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

    /**
     * Oculta la lista de resultados del buscador.
     */
    const hideResults = () => {
        results.classList.add("d-none");
        results.innerHTML = "";
        activeIndex = -1;
    };

    /**
     * Filtra y renderiza los resultados según el texto ingresado.
     */
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


/**
 * Inicializa el panel de notificaciones de un rol.
 *
 * Requiere:
 * - Botón para abrir/cerrar el panel.
 * - Panel contenedor.
 * - Lista donde se mostrarán las notificaciones.
 * - Nodo con datos JSON.
 *
 * Si no existen notificaciones, muestra un mensaje vacío.
 * Si existen, muestra hasta 6 notificaciones recientes.
 */
function inicializarPanelNotificaciones(config) {
    const toggle = document.getElementById(config.toggleId);
    const panel = document.getElementById(config.panelId);
    const list = document.getElementById(config.listId);
    const dataNode = document.getElementById(config.dataNodeId);

    if (!toggle || !panel || !list || !dataNode) return;

    let notifications = [];

    try {
        notifications = JSON.parse(dataNode.textContent || "[]");
    } catch {
        notifications = [];
    }

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

    /**
     * Oculta el panel de notificaciones.
     */
    const hidePanel = () => {
        panel.classList.add("d-none");
        toggle.setAttribute("aria-expanded", "false");
    };

    toggle.addEventListener("click", (e) => {
        e.stopPropagation();

        panel.classList.toggle("d-none");

        toggle.setAttribute(
            "aria-expanded",
            panel.classList.contains("d-none") ? "false" : "true"
        );
    });

    panel.addEventListener("click", e => e.stopPropagation());

    document.addEventListener("click", hidePanel);
}


/**
 * Inicializa confirmaciones globales en formularios.
 *
 * Aplica a todos los formularios que tengan:
 *
 * data-confirm
 *
 * Usa SweetAlert2 para mostrar una confirmación antes de enviar.
 */
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

            if (typeof Swal === "undefined" || typeof Swal.fire !== "function") {
                const texto = form.dataset.confirmText || "Esta acción eliminará el registro seleccionado y no se puede deshacer.";
                const confirmar = window.confirm(texto);
                if (confirmar) {
                    form.dataset.confirmed = "1";
                    form.submit();
                }
                return;
            }

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


/**
 * Inicializa el efecto visual de carga al enviar formularios.
 *
 * Este método es el responsable de mostrar el spinner cuando haces login.
 *
 * Aplica a todos los formularios que tengan:
 *
 * data-disable-on-submit
 *
 * Funcionamiento:
 * 1. Espera el evento submit del formulario.
 * 2. Verifica si el formulario es válido.
 * 3. Busca botones submit.
 * 4. Si el botón tiene data-loading-text, reemplaza el texto por un spinner.
 * 5. Desactiva el botón para evitar doble envío.
 *
 * Ejemplo:
 *
 * <form method="post" data-disable-on-submit>
 *     <button type="submit" data-loading-text="Ingresando...">
 *         Iniciar sesión
 *     </button>
 * </form>
 */
function inicializarFeedbackEnvioFormularios() {
    document.querySelectorAll("form[data-disable-on-submit]").forEach(form => {
        form.addEventListener("submit", function () {
            if (typeof form.checkValidity === "function" && !form.checkValidity()) {
                return;
            }

            const submitButtons = form.querySelectorAll('button[type="submit"], input[type="submit"]');

            submitButtons.forEach(btn => {
                if (btn.disabled) return;
                btn.disabled = true;
                btn.classList.add("disabled");
            });
        });
    });
}


/**
 * Inicializa un sidebar específico mediante su ID.
 *
 * Controla:
 * - Apertura y cierre del sidebar en móviles.
 * - Backdrop oscuro.
 * - Submenús desplegables.
 * - Modo acordeón.
 * - Cierre con tecla Escape.
 * - Cierre automático al cambiar de tamaño de pantalla.
 */
function inicializarSidebarShellById(shellId) {
    const shell = document.getElementById(shellId);

    if (!shell) return;

    const acordeonSubmenus = (shell.dataset.sidebarAccordion || "true").toLowerCase() !== "false";
    const gruposSubmenu = Array.from(shell.querySelectorAll(".admin-nav-group"));

    /**
     * Sincroniza la altura visual de los submenús abiertos o cerrados.
     */
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

    /**
     * Abre o cierra un grupo del sidebar.
     */
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

    /**
     * Sincroniza el estado visual y accesible del sidebar.
     */
    const syncSidebarState = () => {
        const isOpen = shell.classList.contains("sidebar-open");

        toggle.setAttribute("aria-expanded", isOpen ? "true" : "false");

        if (window.innerWidth < desktopBreakpoint) {
            document.body.classList.toggle("overflow-hidden", isOpen);
        } else {
            document.body.classList.remove("overflow-hidden");
        }
    };

    /**
     * Abre el sidebar.
     */
    const openSidebar = () => {
        shell.classList.add("sidebar-open");
        syncSidebarState();
    };

    /**
     * Cierra el sidebar.
     */
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


/**
 * Inicializa automáticamente todos los sidebars encontrados en la página.
 *
 * Busca elementos que tengan:
 *
 * data-sidebar-shell
 *
 * y además tengan un id.
 */
function inicializarSidebarsDesdeData() {
    document
        .querySelectorAll("[data-sidebar-shell][id]")
        .forEach(shell => inicializarSidebarShellById(shell.id));
}


/**
 * Expone funciones globales del sistema CapiMovil.
 *
 * Sirve para poder inicializar manualmente un sidebar desde otra vista
 * o desde otro script si fuera necesario.
 */
window.CapiMovilUI = {
    initSidebarShell: inicializarSidebarShellById
};
