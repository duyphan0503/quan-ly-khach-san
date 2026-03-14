document.addEventListener("DOMContentLoaded", () => {
    const wrappers = document.querySelectorAll("[data-password-toggle]");

    wrappers.forEach((wrapper) => {
        const input = wrapper.querySelector("[data-password-toggle-field]");
        const button = wrapper.querySelector("[data-password-toggle-btn]");
        const icon = wrapper.querySelector("[data-password-toggle-icon]");

        if (!input || !button || !icon) {
            return;
        }

        button.addEventListener("click", () => {
            const isHidden = input.type === "password";
            input.type = isHidden ? "text" : "password";
            button.setAttribute("aria-pressed", isHidden ? "true" : "false");
            button.setAttribute("aria-label", isHidden ? "Ẩn mật khẩu" : "Hiện mật khẩu");
            icon.setAttribute("icon", isHidden ? "lucide:eye-off" : "lucide:eye");
        });
    });
});
