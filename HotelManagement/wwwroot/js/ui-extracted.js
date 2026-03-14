(() => {
  const initAvatarFallbacks = () => {
    document.querySelectorAll("[data-avatar-image]").forEach((image) => {
      const fallbackSelector = image.getAttribute("data-avatar-fallback");
      const fallback =
        fallbackSelector ? document.querySelector(fallbackSelector) : image.nextElementSibling;
      const defaultSrc = image.getAttribute("data-avatar-default-src");

      const showFallback = () => {
        if (defaultSrc && image.getAttribute("src") !== defaultSrc) {
          image.setAttribute("src", defaultSrc);
          return;
        }

        image.classList.add("mq-hidden");
        if (fallback) {
          fallback.classList.add("is-visible");
        }
      };

      image.addEventListener("error", showFallback);
      if (image.complete && image.naturalWidth === 0) {
        showFallback();
      }
    });
  };

  const closeStatusToast = (toast) => {
    if (!toast) {
      return;
    }

    toast.classList.add("animate-out", "fade-out", "slide-out-to-right-full");
    window.setTimeout(() => toast.remove(), 500);
  };

  const initStatusToast = () => {
    const toast = document.querySelector("[data-status-toast]");
    if (!toast) {
      return;
    }

    const progress = toast.querySelector("[data-status-progress]");
    const closeButton = toast.querySelector("[data-status-close]");

    window.setTimeout(() => {
      if (progress) {
        progress.style.width = "0%";
      }
    }, 50);

    window.setTimeout(() => closeStatusToast(toast), 5000);
    closeButton?.addEventListener("click", () => closeStatusToast(toast));
  };

  const initBookingHistoryCards = () => {
    const scrollArea = document.querySelector("[data-booking-scroll-area]");
    if (!scrollArea) {
      return;
    }

    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            entry.target.classList.add("is-visible");
          }
        });
      },
      {
        root: scrollArea,
        threshold: 0.1,
      },
    );

    scrollArea.querySelectorAll("[data-booking-card]").forEach((card) => {
      observer.observe(card);
    });
  };

  const initServiceTotalCalculator = () => {
    const form = document.querySelector("[data-service-total-form]");
    if (!form) {
      return;
    }

    const select = form.querySelector("[data-service-select]");
    const quantity = form.querySelector("[data-service-quantity]");
    const total = form.querySelector("[data-service-total]");

    const updateTotal = () => {
      if (!select || !quantity || !total) {
        return;
      }

      const selectedOption = select.options[select.selectedIndex];
      const priceValue = selectedOption?.getAttribute("data-price");
      if (!priceValue) {
        total.textContent = "0₫";
        return;
      }

      const price = Number.parseFloat(priceValue);
      const qty = Number.parseInt(quantity.value || "1", 10);
      total.textContent = `${(price * qty).toLocaleString("vi-VN")}₫`;
    };

    select?.addEventListener("change", updateTotal);
    quantity?.addEventListener("input", updateTotal);
    updateTotal();
  };

  const initModalActions = () => {
    document.querySelectorAll("[data-show-modal]").forEach((button) => {
      button.addEventListener("click", () => {
        const target = button.getAttribute("data-show-modal");
        const modal = target ? document.getElementById(target) : null;
        modal?.showModal();
      });
    });

    document.querySelectorAll("[data-close-modal]").forEach((button) => {
      button.addEventListener("click", () => {
        const target = button.getAttribute("data-close-modal");
        const modal = target ? document.getElementById(target) : null;
        modal?.close();
      });
    });
  };

  const initClickTargets = () => {
    document.querySelectorAll("[data-click-target]").forEach((trigger) => {
      trigger.addEventListener("click", () => {
        const target = trigger.getAttribute("data-click-target");
        const element = target ? document.getElementById(target) : null;
        element?.click();
      });
    });
  };

  const initConfirmSubmit = () => {
    document.querySelectorAll("[data-confirm-submit]").forEach((button) => {
      button.addEventListener("click", (event) => {
        const message = button.getAttribute("data-confirm-submit");
        if (message && !window.confirm(message)) {
          event.preventDefault();
        }
      });
    });
  };

  const initPageActions = () => {
    document.querySelectorAll("[data-print-page]").forEach((button) => {
      button.addEventListener("click", () => window.print());
    });

    document.querySelectorAll("[data-reload-page]").forEach((button) => {
      button.addEventListener("click", () => window.location.reload());
    });
  };

  const init = () => {
    initAvatarFallbacks();
    initStatusToast();
    initBookingHistoryCards();
    initServiceTotalCalculator();
    initModalActions();
    initClickTargets();
    initConfirmSubmit();
    initPageActions();
  };

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", init, { once: true });
    return;
  }

  init();
})();
