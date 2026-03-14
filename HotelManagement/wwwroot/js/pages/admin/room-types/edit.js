(() => {
  const bindText = (inputId, outputId, fallback) => {
    const input = document.getElementById(inputId);
    const output = document.getElementById(outputId);
    if (!input || !output) {
      return;
    }

    input.addEventListener('input', (event) => {
      const value = event.target.value?.trim();
      output.textContent = value || fallback;
    });
  };

  const bindPrice = () => {
    const input = document.getElementById('RoomType_BasePrice');
    const output = document.getElementById('right-side-price');
    if (!input || !output) {
      return;
    }

    input.addEventListener('input', (event) => {
      const value = Number.parseInt(event.target.value || '', 10);
      if (!Number.isNaN(value)) {
        output.textContent = value.toLocaleString('vi-VN');
      }
    });
  };

  const bindImagePreview = () => {
    const input = document.getElementById('UploadImage');
    const preview = document.getElementById('preview-img');
    const overlay = document.getElementById('preview-overlay');
    const placeholder = document.getElementById('upload-placeholder');
    const rightPreview = document.getElementById('right-side-preview');
    const rightEmptyPreview = document.getElementById('right-side-empty-preview');
    const rightHasPreview = document.getElementById('right-side-has-preview');

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
        placeholder?.classList.add('hidden');
        overlay?.classList.remove('hidden');
        if (rightPreview) {
          rightPreview.src = src;
        }
        rightEmptyPreview?.classList.add('hidden');
        rightHasPreview?.classList.remove('hidden');
      };

      reader.readAsDataURL(file);
    });
  };

  bindText('RoomType_Name', 'right-side-name', 'Tên loại phòng');
  bindText('RoomType_MaxOccupancy', 'right-side-occupancy', '0');
  bindPrice();
  bindImagePreview();
})();
