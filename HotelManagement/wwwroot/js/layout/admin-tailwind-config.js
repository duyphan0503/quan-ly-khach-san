tailwind.config = {
  theme: {
    extend: {
      colors: {
        primary: "#4f46e5",
        "primary-600": "#4f46e5",
        "primary-700": "#4338ca",
        "primary-50": "#eef2ff",
        surface: {
          1: "#0f172a",
          2: "#1e293b",
          3: "#334155",
        },
        content: {
          primary: "#f8fafc",
          secondary: "#cbd5e1",
          muted: "#64748b",
        },
        bdr: {
          default: "rgba(255, 255, 255, 0.05)",
          strong: "rgba(255, 255, 255, 0.1)",
        },
        accent: "#f59e0b",
        danger: "#ef4444",
        success: "#22c55e",
        warning: "#f59e0b",
        info: "#3b82f6",
      },
      fontFamily: {
        luxury: ['"Cormorant Garamond"', "serif"],
        sans: ["Montserrat", "Inter", "sans-serif"],
        inter: ["Inter", "sans-serif"],
        outfit: ["Outfit", "sans-serif"],
      },
    },
  },
};
