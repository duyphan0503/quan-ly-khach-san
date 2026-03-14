(() => {
  const initTheme = () => {
    window.MQTheme?.init();
  };

  const initNavbarScrollEffect = () => {
    const nav = document.querySelector(".glass-nav-public");
    if (!nav) {
      return;
    }

    const updateScrollState = () => {
      nav.classList.toggle("is-scrolled", window.scrollY > 20);
    };

    window.addEventListener("scroll", updateScrollState, { passive: true });
    updateScrollState();
  };

  const initMobileMenu = () => {
    const btn = document.getElementById("mobile-menu-btn");
    const menu = document.getElementById("mobile-menu");
    const openIcon = document.getElementById("mobile-menu-open-icon");
    const closeIcon = document.getElementById("mobile-menu-close-icon");

    if (!btn || !menu) return;

    const toggle = (forceClose) => {
      const isOpen =
        forceClose === true ? true : !menu.classList.contains("hidden");
      menu.classList.toggle("hidden", isOpen);
      openIcon?.classList.toggle("hidden", !isOpen);
      openIcon?.classList.toggle("block", isOpen);
      closeIcon?.classList.toggle("hidden", isOpen);
      closeIcon?.classList.toggle("block", !isOpen);
      btn.setAttribute("aria-expanded", String(!isOpen));
    };

    btn.addEventListener("click", () => toggle());

    // Close mobile menu when clicking a link inside it
    menu.querySelectorAll("a").forEach((link) => {
      link.addEventListener("click", () => toggle(true));
    });
  };

  const initialize = () => {
    initTheme();
    initNavbarScrollEffect();
    initMobileMenu();
  };

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", initialize, { once: true });
    return;
  }

  initialize();
})();
