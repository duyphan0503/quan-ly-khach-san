document.addEventListener("DOMContentLoaded", function () {
    const avatarInput = document.getElementById("avatar-upload");
    const avatarPreview = document.getElementById("avatar-preview");
    const avatarPlaceholder = document.getElementById("avatar-placeholder");
    const editForm = document.getElementById("guest-edit-form");
    const saveButton = document.getElementById("save-button");
    const saveButtonLabel = document.getElementById("save-button-label");

    if (avatarInput && avatarPreview) {
        avatarInput.addEventListener("change", function (event) {
            const file = event.target.files && event.target.files[0];
            if (!file) return;

            if (file.size > 5 * 1024 * 1024) {
                alert("Kích thước ảnh không được vượt quá 5MB.");
                this.value = "";
                return;
            }

            const reader = new FileReader();
            reader.onload = function (e) {
                avatarPreview.src = e.target.result;
                avatarPreview.classList.remove("hidden");
                if (avatarPlaceholder) {
                    avatarPlaceholder.classList.add("hidden");
                }
                
                avatarPreview.parentElement.animate([
                    { transform: 'scale(0.95)', opacity: 0.5 },
                    { transform: 'scale(1)', opacity: 1 }
                ], {
                    duration: 600,
                    easing: 'cubic-bezier(0.22, 1, 0.36, 1)'
                });
            };
            reader.readAsDataURL(file);
        });
    }

    // Delete logic removed as requested by UI changes

    if (editForm && saveButton && saveButtonLabel) {
        editForm.addEventListener("submit", function (e) {
            if (saveButton.classList.contains("is-submitting")) {
                e.preventDefault();
                return;
            }

            saveButton.classList.add("is-submitting");
            saveButtonLabel.textContent = "ĐANG LƯU HỒ SƠ...";
        });
    }
});
