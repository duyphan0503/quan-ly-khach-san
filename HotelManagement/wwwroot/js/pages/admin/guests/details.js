(() => {
    const SCROLL_BOTTOM_THRESHOLD_PX = 40;

    const toInt = (value, fallback = 0) => {
        const parsed = Number.parseInt(value ?? "", 10);
        return Number.isFinite(parsed) ? parsed : fallback;
    };

    const escapeHtml = (value) => {
        return String(value ?? "")
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#39;");
    };

    const buildRoomMarkup = (item) => {
        if (!item.roomNumber) {
            return '<span class="text-slate-500 italic text-sm">Không có dữ liệu phòng</span>';
        }

        const roomType = item.roomTypeName
            ? `<div class="text-[10px] text-slate-500 font-bold uppercase tracking-tight">${escapeHtml(item.roomTypeName)}</div>`
            : "";

        return `
            <div class="font-bold text-white text-base">Phòng ${escapeHtml(item.roomNumber)}</div>
            ${roomType}
        `;
    };

    const buildActivityRow = (item, detailsBaseUrl) => {
        const detailsUrl = `${detailsBaseUrl}${encodeURIComponent(item.id)}`;

        return `
            <tr class="booking-history-row border-none hover:bg-slate-800/20 h-20 transition-all">
                <td class="pl-8 font-mono text-xs font-bold text-indigo-400">#${escapeHtml(item.idDisplay)}</td>
                <td>${buildRoomMarkup(item)}</td>
                <td>
                    <div class="flex items-center gap-3">
                        <div class="text-right">
                            <div class="text-sm font-semibold text-slate-200">${escapeHtml(item.checkIn)}</div>
                            <div class="text-[10px] text-slate-600 font-bold text-right uppercase italic">Check-in</div>
                        </div>
                        <iconify-icon icon="lucide:arrow-right" class="text-slate-700"></iconify-icon>
                        <div>
                            <div class="text-sm font-semibold text-slate-200">${escapeHtml(item.checkOut)}</div>
                            <div class="text-[10px] text-slate-600 font-bold uppercase italic">Check-out</div>
                        </div>
                    </div>
                </td>
                <td>
                    <span class="badge-glass inline-flex items-center gap-1.5 px-3 py-1.5 rounded-full border ${escapeHtml(item.statusClass)} text-[11px] font-black uppercase tracking-wider">
                        <iconify-icon icon="${escapeHtml(item.statusIcon)}" class="text-sm"></iconify-icon>
                        ${escapeHtml(item.statusLabel)}
                    </span>
                </td>
                <td class="pr-8 text-right">
                    <a href="${detailsUrl}" class="btn btn-sm btn-ghost hover:bg-white/5 rounded-xl group transition-all">
                        <span class="text-indigo-400 group-hover:text-white transition-colors">Chi tiết</span>
                        <iconify-icon icon="lucide:chevron-right" class="group-hover:translate-x-1 transition-transform"></iconify-icon>
                    </a>
                </td>
            </tr>
        `;
    };

    function initActivityLazyLoad() {
        const scrollContainer = document.querySelector("[data-activity-scroll]");
        const body = document.querySelector("[data-activity-body]");
        const hint = document.querySelector("[data-activity-hint]");
        const loadingHint = document.querySelector("[data-activity-loading]");

        if (!scrollContainer || !body || !hint) {
            return;
        }

        const endpoint = scrollContainer.getAttribute("data-endpoint");
        const detailsBaseUrl = scrollContainer.getAttribute("data-booking-details-base-url") || "/admin/bookings/details/";
        const pageSize = Math.max(1, toInt(scrollContainer.getAttribute("data-page-size"), 8));
        const total = Math.max(0, toInt(hint.getAttribute("data-total"), 0));
        let visible = Math.max(0, toInt(hint.getAttribute("data-visible"), 0));
        let pageNumber = Math.max(1, Math.ceil(Math.max(visible, 1) / pageSize));
        let isLoading = false;

        if (!endpoint || visible >= total) {
            return;
        }

        const updateHint = () => {
            if (visible >= total) {
                hint.textContent = "ĐÃ HIỂN THỊ TOÀN BỘ LỊCH SỬ HOẠT ĐỘNG";
                return;
            }

            hint.textContent = `ĐANG HIỂN THỊ ${visible}/${total} - CUỘN TRONG KHUNG ĐỂ TẢI THÊM`;
        };

        const toggleLoading = (show) => {
            if (!loadingHint) {
                return;
            }

            loadingHint.classList.toggle("hidden", !show);
        };

        const shouldLoadMore = () => {
            if (visible >= total || isLoading) {
                return false;
            }

            const { scrollTop, scrollHeight, clientHeight } = scrollContainer;
            return scrollTop + clientHeight >= scrollHeight - SCROLL_BOTTOM_THRESHOLD_PX;
        };

        const loadMore = async () => {
            if (!shouldLoadMore()) {
                return;
            }

            isLoading = true;
            toggleLoading(true);

            try {
                const nextPage = pageNumber + 1;
                const separator = endpoint.includes("?") ? "&" : "?";
                const url = `${endpoint}${separator}pageNumber=${nextPage}`;
                const response = await fetch(url, { method: "GET", headers: { "X-Requested-With": "XMLHttpRequest" } });

                if (!response.ok) {
                    throw new Error(`Cannot load activity page ${nextPage}`);
                }

                const payload = await response.json();
                const items = Array.isArray(payload.items) ? payload.items : [];
                if (!items.length) {
                    visible = total;
                    updateHint();
                    return;
                }

                const html = items.map((item) => buildActivityRow(item, detailsBaseUrl)).join("");
                body.insertAdjacentHTML("beforeend", html);
                visible = Math.min(total, visible + items.length);
                pageNumber = nextPage;
                updateHint();
            } catch (_error) {
                hint.textContent = "TẢI DỮ LIỆU THẤT BẠI - VUI LÒNG THỬ LẠI";
            } finally {
                isLoading = false;
                toggleLoading(false);
            }
        };

        updateHint();
        scrollContainer.addEventListener("scroll", () => {
            void loadMore();
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initActivityLazyLoad);
    } else {
        initActivityLazyLoad();
    }
})();
