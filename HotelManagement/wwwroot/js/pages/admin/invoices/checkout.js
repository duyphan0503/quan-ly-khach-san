(() => {
  const form = document.getElementById("checkoutForm");
  const subTotalElement = document.getElementById("SubTotalVal");
  const groupSubTotalElement = document.getElementById("GroupSubTotalVal");
  const grandTotalDisplay = document.getElementById("GrandTotalDisplay");

  if (!form || !subTotalElement || !grandTotalDisplay) {
    return;
  }

  const taxInput = document.getElementById("Input_Tax");
  const discountInput = document.getElementById("Input_Discount");
  const checkoutWholeGroupInput = document.getElementById("Input_CheckoutWholeGroup");
  const calculationInputs = form.querySelectorAll(".calculate-total");
  const subTotal = Number.parseFloat(subTotalElement.value) || 0;
  const groupSubTotal = Number.parseFloat(groupSubTotalElement?.value ?? "") || subTotal;

  const formatCurrency = (number) =>
    `${new Intl.NumberFormat("vi-VN").format(number)} <span class="text-xl">₫</span>`;

  const updateTotal = () => {
    const tax = Number.parseFloat(taxInput?.value ?? "") || 0;
    const discount = Number.parseFloat(discountInput?.value ?? "") || 0;
    const effectiveSubTotal =
      checkoutWholeGroupInput?.checked ? groupSubTotal : subTotal;
    const grandTotal = Math.max(effectiveSubTotal + tax - discount, 0);

    grandTotalDisplay.innerHTML = formatCurrency(grandTotal);
  };

  calculationInputs.forEach((input) => {
    input.addEventListener("input", updateTotal);
  });

  checkoutWholeGroupInput?.addEventListener("change", updateTotal);

  updateTotal();
})();
