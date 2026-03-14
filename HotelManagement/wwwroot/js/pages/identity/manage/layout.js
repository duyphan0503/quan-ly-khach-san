(() => {
  const init = () => {
    const input = document.getElementById("avatar-upload-global");
    const form = document.getElementById("avatar-form-global");
    const overlay = document.getElementById("upload-overlay-global");

    if (!input || !form) {
      return;
    }

    input.addEventListener("change", () => {
      if (!input.files || !input.files[0]) {
        return;
      }

      overlay?.classList.remove("hidden");
      form.submit();
    });
  };

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", init, { once: true });
    return;
  }

  init();
})();
