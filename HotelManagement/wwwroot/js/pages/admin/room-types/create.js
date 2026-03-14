(() => {
  const input = document.getElementById('UploadImage');
  const preview = document.getElementById('preview-img');
  const overlay = document.getElementById('preview-overlay');
  const instructions = document.querySelector('#image-preview div:first-child');

  if (!input || !preview) {
    return;
  }

  input.addEventListener('change', () => {
    const file = input.files?.[0];
    if (!file) {
      return;
    }

    const reader = new FileReader();
    reader.onload = (event) => {
      const src = String(event.target?.result || '');
      if (!src) {
        return;
      }

      preview.src = src;
      preview.classList.remove('hidden');
      instructions?.classList.add('hidden');
      overlay?.classList.remove('hidden');
    };

    reader.readAsDataURL(file);
  });
})();
