(() => {
  const root = document.getElementById('dashboard-chart-data');
  if (!root || typeof window.Chart === 'undefined') {
    return;
  }

  const parseNumberList = (value) =>
    (value || '')
      .split(',')
      .map((item) => Number.parseFloat(item))
      .filter((item) => !Number.isNaN(item));

  const parseStringList = (value) =>
    (value || '').split('|').filter((item) => item.length > 0);

  const monthlyRevenue = parseNumberList(root.dataset.monthlyRevenue);
  const roomLabels = parseStringList(root.dataset.roomLabels);
  const roomValues = parseNumberList(root.dataset.roomValues);
  const revenueCanvas = document.getElementById('revenueChart');
  const roomCanvas = document.getElementById('roomChart');

  if (revenueCanvas) {
    new window.Chart(revenueCanvas, {
      type: 'line',
      data: {
        labels: ['T1', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'T8', 'T9', 'T10', 'T11', 'T12'],
        datasets: [
          {
            label: 'Doanh thu (VNĐ)',
            data: monthlyRevenue,
            borderColor: '#6366f1',
            backgroundColor: 'rgba(99, 102, 241, 0.1)',
            fill: 'origin',
            tension: 0.4,
            pointRadius: 0,
            pointHoverRadius: 6,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false },
        },
        scales: {
          x: {
            ticks: { color: '#94a3b8' },
            grid: { display: false },
          },
          y: {
            ticks: { color: '#94a3b8' },
            grid: { color: 'rgba(255, 255, 255, 0.05)' },
          },
        },
      },
    });
  }

  if (roomCanvas) {
    new window.Chart(roomCanvas, {
      type: 'doughnut',
      data: {
        labels: roomLabels,
        datasets: [
          {
            data: roomValues,
            backgroundColor: ['#6366f1', '#ec4899', '#10b981', '#f59e0b'],
            borderWidth: 0,
            hoverOffset: 15,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        cutout: '75%',
        plugins: {
          legend: {
            position: 'bottom',
            labels: {
              color: '#94a3b8',
              usePointStyle: true,
            },
          },
        },
      },
    });
  }
})();
