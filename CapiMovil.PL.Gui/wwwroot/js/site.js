document.addEventListener("DOMContentLoaded", function () {
    const formulariosEliminar = document.querySelectorAll(".form-eliminar");

    formulariosEliminar.forEach(form => {
        form.addEventListener("submit", function (e) {
            e.preventDefault();

            Swal.fire({
                title: "¿Estás seguro?",
                text: "Esta acción no se puede deshacer.",
                icon: "warning",
                showCancelButton: true,
                confirmButtonText: "Sí, eliminar",
                cancelButtonText: "Cancelar",
                reverseButtons: true
            }).then((result) => {
                if (result.isConfirmed) {
                    form.submit();
                }
            });
        });
    });

    inicializarBuscadorGlobal({
        inputId: "adminGlobalSearch",
        resultsId: "adminSearchResults",
        dataNodeId: "admin-search-data",
        itemClassName: "admin-search-item"
    });

    inicializarBuscadorGlobal({
        inputId: "conductorGlobalSearch",
        resultsId: "conductorSearchResults",
        dataNodeId: "conductor-search-data",
        itemClassName: "conductor-search-item"
    });

    inicializarBuscadorGlobal({
        inputId: "padreGlobalSearch",
        resultsId: "padreSearchResults",
        dataNodeId: "padre-search-data",
        itemClassName: "padre-search-item"
    });

    inicializarPanelNotificaciones({
        toggleId: "adminNotificationToggle",
        panelId: "adminNotificationPanel",
        listId: "adminNotificationList",
        dataNodeId: "admin-notification-data"
    });

    inicializarPanelNotificaciones({
        toggleId: "conductorNotificationToggle",
        panelId: "conductorNotificationPanel",
        listId: "conductorNotificationList",
        dataNodeId: "conductor-notification-data"
    });

    inicializarPanelNotificaciones({
        toggleId: "padreNotificationToggle",
        panelId: "padreNotificationPanel",
        listId: "padreNotificationList",
        dataNodeId: "padre-notification-data"
    });
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

    const goToIndex = (index) => {
        if (index < 0 || index >= modules.length) {
            return;
        }

        const destino = modules[index]?.url || modules[index]?.Url;
        if (destino) {
            window.location.href = destino;
        }
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
            .filter(item => {
                const target = `${item.texto} ${item.keywords}`.toLowerCase();
                return target.includes(term);
            })
            .slice(0, 8);

        if (filtrados.length === 0) {
            results.innerHTML = '<button type="button" class="list-group-item list-group-item-action disabled">Sin resultados</button>';
            results.classList.remove("d-none");
            activeIndex = -1;
            return;
        }

        results.innerHTML = filtrados
            .map((item, index) =>
                `<button type="button" class="list-group-item list-group-item-action ${config.itemClassName}" data-index="${index}" data-url="${item.url}">
                    <i class="bi bi-arrow-up-right-square me-2 text-muted"></i>${item.texto}
                </button>`)
            .join("");

        results.classList.remove("d-none");
        activeIndex = -1;

        const rows = results.querySelectorAll(`.${config.itemClassName}`);
        rows.forEach(row => {
            row.addEventListener("click", () => {
                const url = row.getAttribute("data-url");
                if (url) {
                    window.location.href = url;
                }
            });
        });

        input.dataset.filtered = JSON.stringify(filtrados);
    };

    input.addEventListener("input", (e) => {
        renderResults(e.target.value);
    });

    input.addEventListener("keydown", (e) => {
        const items = results.querySelectorAll(`.${config.itemClassName}`);
        if (results.classList.contains("d-none") || items.length === 0) {
            if (e.key === "Enter") {
                e.preventDefault();
                const list = JSON.parse(input.dataset.filtered || "[]");
                if (list.length > 0 && list[0].url) {
                    window.location.href = list[0].url;
                }
            }
            return;
        }

        if (e.key === "ArrowDown") {
            e.preventDefault();
            activeIndex = (activeIndex + 1) % items.length;
        } else if (e.key === "ArrowUp") {
            e.preventDefault();
            activeIndex = (activeIndex - 1 + items.length) % items.length;
        } else if (e.key === "Enter") {
            e.preventDefault();
            const list = JSON.parse(input.dataset.filtered || "[]");
            const target = activeIndex >= 0 ? list[activeIndex] : list[0];
            if (target?.url) {
                window.location.href = target.url;
            }
            return;
        } else if (e.key === "Escape") {
            hideResults();
            return;
        }

        items.forEach((item, i) => {
            item.classList.toggle("active", i === activeIndex);
        });
    });

    document.addEventListener("click", (e) => {
        if (!results.contains(e.target) && e.target !== input) {
            hideResults();
        }
    });
}

function inicializarPanelNotificaciones(config) {
    const toggle = document.getElementById(config.toggleId);
    const panel = document.getElementById(config.panelId);
    const list = document.getElementById(config.listId);
    const dataNode = document.getElementById(config.dataNodeId);

    if (!toggle || !panel || !list || !dataNode) {
        return;
    }

    let notifications = [];
    try {
        notifications = JSON.parse(dataNode.textContent || "[]");
    } catch {
        notifications = [];
    }

    if (!Array.isArray(notifications) || notifications.length === 0) {
        list.innerHTML = '<div class="list-group-item text-muted small">Sin notificaciones recientes</div>';
    } else {
        list.innerHTML = notifications
            .slice(0, 5)
            .map(item => {
                const texto = item.texto || item.Texto || "Notificación";
                const url = item.url || item.Url || "#";
                const tiempo = item.tiempo || item.Tiempo || "Reciente";
                return `<a href="${url}" class="list-group-item list-group-item-action">
                            <div class="fw-semibold small">${texto}</div>
                            <small class="text-muted">${tiempo}</small>
                        </a>`;
            })
            .join("");
    }

    const hidePanel = () => {
        panel.classList.add("d-none");
        toggle.setAttribute("aria-expanded", "false");
    };

    toggle.addEventListener("click", (e) => {
        e.stopPropagation();
        panel.classList.toggle("d-none");
        const expanded = !panel.classList.contains("d-none");
        toggle.setAttribute("aria-expanded", expanded ? "true" : "false");
    });

    panel.addEventListener("click", e => e.stopPropagation());
    document.addEventListener("click", hidePanel);
}
