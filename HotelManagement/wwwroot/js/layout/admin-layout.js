(() => {
  const initTheme = () => {
    window.MQTheme?.init();
  };

  const initSidebarToggle = () => {
    const btn = document.getElementById("sidebar-toggle");
    const sidebar = document.getElementById("logo-sidebar");
    const backdrop = document.getElementById("sidebar-backdrop");

    if (!btn || !sidebar) return;

    const open = () => {
      sidebar.classList.remove("-translate-x-full");
      backdrop?.classList.add("active");
      document.body.style.overflow = "hidden";
    };

    const close = () => {
      sidebar.classList.add("-translate-x-full");
      backdrop?.classList.remove("active");
      document.body.style.overflow = "";
    };

    btn.addEventListener("click", () => {
      const isHidden = sidebar.classList.contains("-translate-x-full");
      isHidden ? open() : close();
    });

    backdrop?.addEventListener("click", close);
  };

  const initialize = () => {
    initTheme();
    initSidebarToggle();
  };

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", initialize, { once: true });
    return;
  }

  initialize();
})();
