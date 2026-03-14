(() => {
  const modal = document.getElementById('delete_modal');
  if (!modal) {
    return;
  }

  document.querySelectorAll('[data-guest-delete-trigger]').forEach((button) => {
    button.addEventListener('click', () => {
      const id = button.getAttribute('data-guest-id');
      const name = button.getAttribute('data-guest-name');
      const idField = document.getElementById('delete_guest_id');
      const nameField = document.getElementById('delete_guest_name');

      if (idField) {
        idField.value = id || '';
      }

      if (nameField) {
        nameField.textContent = name || '';
      }
      if (typeof modal.showModal === 'function') {
        modal.showModal();
      }
    });
  });
})();
