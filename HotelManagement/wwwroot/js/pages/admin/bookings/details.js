(() => {
  const form = document.querySelector('[data-service-total-form]');
  if (!form) {
    return;
  }

  const select = form.querySelector('[data-service-select]');
  const quantity = form.querySelector('[data-service-quantity]');
  const total = form.querySelector('[data-service-total]');

  const updateTotal = () => {
    if (!select || !quantity || !total) {
      return;
    }

    const selectedOption = select.options[select.selectedIndex];
    const priceValue = selectedOption?.getAttribute('data-price');
    if (!priceValue) {
      total.textContent = '0₫';
      return;
    }

    const price = Number.parseFloat(priceValue);
    const qty = Number.parseInt(quantity.value || '1', 10);
    total.textContent = `${(price * qty).toLocaleString('vi-VN')}₫`;
  };

  select?.addEventListener('change', updateTotal);
  quantity?.addEventListener('input', updateTotal);
  if (select.value) {
    updateTotal();
  }
})();
