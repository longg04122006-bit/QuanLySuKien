/**
 * HỆ THỐNG QUẢN LÝ SỰ KIỆN (QLSK) - JAVASCRIPT FRONTEND
 * Kết nối toàn bộ chức năng qua API Gateway (YARP) đến Backend API
 */

// =========================================================
// 1. CẤU HÌNH HỆ THỐNG & GATEWAY URL
// =========================================================
const DEFAULT_GATEWAY_URL = "https://localhost:7127";
const FALLBACK_GATEWAY_URL = "http://localhost:5086";

function getGatewayUrl() {
    return localStorage.getItem("gateway_url") || DEFAULT_GATEWAY_URL;
}

function setGatewayUrl(url) {
    document.getElementById("gatewayInput").value = url;
}

function saveGatewayUrl() {
    const input = document.getElementById("gatewayInput").value.trim();
    if (!input) {
        showToast("Vui lòng nhập địa chỉ Gateway hợp lệ!", "warning");
        return;
    }
    const cleanUrl = input.replace(/\/+$/, "");
    localStorage.setItem("gateway_url", cleanUrl);
    updateGatewayIndicator();
    closeModal("gatewayModal");
    showToast(`Đã lưu Gateway URL: ${cleanUrl}`, "success");
    checkGatewayConnection();
    refreshCurrentPage();
}

function openGatewayModal() {
    document.getElementById("gatewayInput").value = getGatewayUrl();
    openModal("gatewayModal");
}

function updateGatewayIndicator() {
    const label = document.getElementById("gatewayUrlLabel");
    if (label) {
        label.textContent = getGatewayUrl();
    }
}

// Kiểm tra kết nối Gateway
async function checkGatewayConnection() {
    const dot = document.getElementById("gatewayStatusDot");
    try {
        // Thử ping qua Gateway tới endpoint Events
        const res = await fetch(`${getGatewayUrl()}/api/Events`, {
            method: "GET",
            headers: { "Accept": "application/json" }
        });
        if (dot) {
            dot.className = "status-dot online";
            dot.title = "Đã kết nối API Gateway thành công";
        }
    } catch (err) {
        console.warn("Lỗi kiểm tra Gateway:", err);
        if (dot) {
            dot.className = "status-dot offline";
            dot.title = "Không thể kết nối đến API Gateway. Bấm để kiểm tra cấu hình URL.";
        }
    }
}

// =========================================================
// 2. QUẢN LÝ XÁC THỰC & PHÂN QUYỀN (AUTHENTICATION & RBAC)
// =========================================================

// Phân quyền trang theo 3 nhóm người dùng chính + Guest:
// - Admin: Toàn quyền quản trị hệ thống, quản lý người dùng, xóa sự kiện, quản lý toàn bộ dữ liệu.
// - Manager: Quản lý sự kiện, danh mục, địa điểm, loại vé, đơn đặt vé, thanh toán, điểm danh vé. Không xóa sự kiện, không quản lý người dùng.
// - User: Khám phá sự kiện, xem chi tiết, đặt vé tham gia, xem đơn đặt vé của mình, xem vé điện tử đã mua.
// - Guest: Xem tổng quan và danh sách sự kiện mở bán.
const PAGE_PERMISSIONS = {
    "dashboard": ["Admin", "Manager", "User", "Guest"],
    "events": ["Admin", "Manager", "User", "Guest"],
    "categories": ["Admin", "Manager"],
    "venues": ["Admin", "Manager"],
    "ticket-types": ["Admin", "Manager"],
    "bookings": ["Admin", "Manager", "User"],
    "payments": ["Admin", "Manager"],
    "tickets": ["Admin", "Manager", "User"],
    "users": ["Admin"]
};

function parseJwt(token) {
    try {
        if (!token) return null;
        const parts = token.split('.');
        if (parts.length !== 3) return null;
        const base64Url = parts[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(
            atob(base64)
                .split('')
                .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
                .join('')
        );
        return JSON.parse(jsonPayload);
    } catch {
        return null;
    }
}

function getToken() {
    return localStorage.getItem("jwt_token") || "";
}

function getCurrentUser() {
    try {
        const u = localStorage.getItem("current_user");
        let parsed = u ? JSON.parse(u) : null;
        const token = getToken();

        if (token) {
            const payload = parseJwt(token);
            if (payload) {
                const roleFromJwt = payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || payload.role;
                const userIdFromJwt = payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/nameidentifier"] || payload.nameid || payload.sub;
                const usernameFromJwt = payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/name"] || payload.unique_name || payload.name;
                const emailFromJwt = payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/emailaddress"] || payload.email;

                if (!parsed) {
                    parsed = {
                        userId: Number(userIdFromJwt) || 0,
                        username: usernameFromJwt || "User",
                        fullName: usernameFromJwt || "User",
                        email: emailFromJwt || "",
                        role: roleFromJwt || "User",
                        roleId: roleFromJwt === "Admin" ? 1 : roleFromJwt === "Manager" ? 2 : 3
                    };
                } else if (roleFromJwt && !parsed.role) {
                    parsed.role = roleFromJwt;
                    parsed.roleId = roleFromJwt === "Admin" ? 1 : roleFromJwt === "Manager" ? 2 : 3;
                }
            }
        }
        return parsed;
    } catch {
        return null;
    }
}

function getCurrentRole() {
    const token = getToken();
    const user = getCurrentUser();
    if (!token || !user) return "Guest";

    let role = user.role;
    if (!role && user.roleId) {
        role = user.roleId === 1 ? "Admin" : user.roleId === 2 ? "Manager" : "User";
    }
    return role || "User";
}

function hasRole(...roles) {
    const current = getCurrentRole();
    return roles.includes(current);
}

function isAdmin() {
    return getCurrentRole() === "Admin";
}

function isManager() {
    return getCurrentRole() === "Manager";
}

function isUser() {
    return getCurrentRole() === "User";
}

function isGuest() {
    return getCurrentRole() === "Guest";
}

function isAuthenticated() {
    return Boolean(getToken() && getCurrentUser());
}

function applyRolePermissions() {
    const role = getCurrentRole();

    // 1. Cập nhật thanh Menu điều hướng Sidebar
    document.querySelectorAll(".nav-menu .nav-item").forEach(item => {
        const page = item.getAttribute("data-page");
        const allowedRoles = PAGE_PERMISSIONS[page] || ["Admin"];
        if (allowedRoles.includes(role)) {
            item.style.display = "flex";
        } else {
            item.style.display = "none";
        }
    });

    // 2. Nhãn hiển thị linh hoạt theo vai trò
    const navBookings = document.getElementById("navLabelBookings");
    if (navBookings) {
        navBookings.textContent = role === "User" ? "Đơn vé của tôi" : "Đơn đặt vé";
    }
    const navTickets = document.getElementById("navLabelTickets");
    if (navTickets) {
        navTickets.textContent = role === "User" ? "Vé của tôi" : "Vé & Check-in";
    }

    // 3. Nút hành động trên các trang
    const btnCreateEvent = document.getElementById("btnCreateEvent");
    if (btnCreateEvent) {
        btnCreateEvent.style.display = hasRole("Admin", "Manager") ? "inline-flex" : "none";
    }

    const dashCreateEvent = document.getElementById("dashCreateEventBtn");
    const dashBrowseEvents = document.getElementById("dashBrowseEventsBtn");
    if (dashCreateEvent) {
        dashCreateEvent.style.display = hasRole("Admin", "Manager") ? "inline-flex" : "none";
    }
    if (dashBrowseEvents) {
        dashBrowseEvents.style.display = (role === "User" || role === "Guest") ? "inline-flex" : "none";
    }

    const btnCreateCategory = document.getElementById("btnCreateCategory");
    if (btnCreateCategory) {
        btnCreateCategory.style.display = hasRole("Admin", "Manager") ? "inline-flex" : "none";
    }

    const btnCreateVenue = document.getElementById("btnCreateVenue");
    if (btnCreateVenue) {
        btnCreateVenue.style.display = hasRole("Admin", "Manager") ? "inline-flex" : "none";
    }

    const btnCreateTicketType = document.getElementById("btnCreateTicketType");
    if (btnCreateTicketType) {
        btnCreateTicketType.style.display = hasRole("Admin", "Manager") ? "inline-flex" : "none";
    }

    const btnCreatePayment = document.getElementById("btnCreatePayment");
    if (btnCreatePayment) {
        btnCreatePayment.style.display = hasRole("Admin", "Manager") ? "inline-flex" : "none";
    }

    const btnCreateUser = document.getElementById("btnCreateUser");
    if (btnCreateUser) {
        btnCreateUser.style.display = isAdmin() ? "inline-flex" : "none";
    }

    // 4. Các thẻ thống kê trên Dashboard
    const statUsers = document.getElementById("statCardUsers");
    if (statUsers) {
        statUsers.style.display = isAdmin() ? "flex" : "none";
    }

    const statPayments = document.getElementById("statCardPayments");
    if (statPayments) {
        statPayments.style.display = hasRole("Admin", "Manager") ? "flex" : "none";
    }

    const statVenues = document.getElementById("statCardVenues");
    if (statVenues) {
        statVenues.style.display = hasRole("Admin", "Manager") ? "flex" : "none";
    }

    const statTicketTypes = document.getElementById("statCardTicketTypes");
    if (statTicketTypes) {
        statTicketTypes.style.display = hasRole("Admin", "Manager") ? "flex" : "none";
    }

    const statMyTickets = document.getElementById("statCardMyTickets");
    if (statMyTickets) {
        statMyTickets.style.display = role === "User" ? "flex" : "none";
    }

    const statBookingsLabel = document.getElementById("statBookingsLabel");
    if (statBookingsLabel) {
        statBookingsLabel.textContent = role === "User" ? "Đơn vé của tôi" : "Đơn đặt vé";
    }

    // 5. Tiêu đề chào mừng và định hướng vai trò
    const heroTag = document.getElementById("heroRoleTag");
    const heroHeading = document.getElementById("heroHeading");
    const heroDesc = document.getElementById("heroDescription");
    if (heroTag) {
        heroTag.textContent = role === "Admin" ? "QUẢN TRỊ VIÊN HỆ THỐNG (ADMIN)" :
            role === "Manager" ? "BẢNG ĐIỀU HÀNH SỰ KIỆN (MANAGER)" :
            role === "User" ? "CỔNG KHÁCH HÀNG & ĐẶT VÉ (USER)" : "HỆ THỐNG SỰ KIỆN TRỰC TUYẾN";
    }
    if (heroHeading && heroDesc) {
        if (role === "Admin") {
            heroHeading.textContent = "Chào mừng Quản trị viên!";
            heroDesc.textContent = "Toàn quyền quản trị sự kiện, địa điểm, hạng vé, đơn hàng, doanh thu và người dùng hệ thống.";
        } else if (role === "Manager") {
            heroHeading.textContent = "Chào mừng Người quản lý sự kiện!";
            heroDesc.textContent = "Bạn có quyền tạo & cập nhật sự kiện, quản lý danh mục, địa điểm, loại vé và check-in khách tham dự.";
        } else if (role === "User") {
            heroHeading.textContent = "Chào mừng bạn đến với sự kiện!";
            heroDesc.textContent = "Khám phá các sự kiện hấp dẫn, chọn mua vé nhanh chóng và theo dõi vé điện tử của bạn.";
        } else {
            heroHeading.textContent = "Chào mừng đến với Hệ Thống Quản Lý Sự Kiện!";
            heroDesc.textContent = "Đăng nhập tài khoản để nhận JWT Token và trải nghiệm các tính năng theo phân quyền.";
        }
    }

    // 6. Tiêu đề các trang Bookings và Tickets
    const bookingTitle = document.getElementById("bookingHeaderTitle");
    const bookingSubtitle = document.getElementById("bookingHeaderSubtitle");
    if (bookingTitle && bookingSubtitle) {
        if (role === "User") {
            bookingTitle.textContent = "Đơn đặt vé của tôi";
            bookingSubtitle.textContent = "Theo dõi lịch sử đơn đặt vé và trạng thái thanh toán của bạn";
        } else {
            bookingTitle.textContent = "Quản lý đơn đặt vé";
            bookingSubtitle.textContent = "Theo dõi danh sách đặt vé, trạng thái thanh toán và thông tin khách hàng";
        }
    }

    const ticketTitle = document.getElementById("ticketHeaderTitle");
    const ticketSubtitle = document.getElementById("ticketHeaderSubtitle");
    if (ticketTitle && ticketSubtitle) {
        if (role === "User") {
            ticketTitle.textContent = "Vé tham dự của tôi";
            ticketSubtitle.textContent = "Danh sách vé điện tử đã phát hành để xuất trình tại cổng check-in";
        } else {
            ticketTitle.textContent = "Vé tham dự & Điểm danh (Check-In)";
            ticketSubtitle.textContent = "Xem danh sách vé điện tử đã phát hành và thực hiện check-in vào cổng";
        }
    }
}

function saveAuth(token, user) {
    localStorage.setItem("jwt_token", token.trim());
    if (user) {
        localStorage.setItem("current_user", JSON.stringify(user));
    }
    renderUserProfile();
    applyRolePermissions();
}

function clearAuth() {
    localStorage.removeItem("jwt_token");
    localStorage.removeItem("current_user");
    renderUserProfile();
    applyRolePermissions();
}

function renderUserProfile() {
    const userProfileArea = document.getElementById("userProfileArea");
    if (!userProfileArea) return;

    const user = getCurrentUser();
    const token = getToken();

    if (token && user) {
        const initials = (user.fullName || user.username || "U")
            .split(" ")
            .map(n => n[0])
            .join("")
            .substring(0, 2)
            .toUpperCase();

        const roleName = getCurrentRole();
        const roleClass = roleName.toLowerCase();
        const roleIcon = roleName === "Admin" ? "👑" : roleName === "Manager" ? "👔" : "👤";

        userProfileArea.innerHTML = `
            <div class="user-badge">
                <div class="user-avatar">${initials}</div>
                <div class="user-meta">
                    <strong>${escapeHtml(user.fullName || user.username)}</strong>
                    <small><span class="badge badge-role-${roleClass}">${roleIcon} ${roleName}</span></small>
                </div>
                <button class="btn btn-secondary btn-sm" onclick="handleLogout()" title="Đăng xuất tài khoản">Đăng xuất</button>
            </div>
        `;
    } else {
        userProfileArea.innerHTML = `
            <button class="btn btn-primary btn-sm" onclick="openLoginModal()">🔑 Đăng nhập</button>
        `;
    }
}

function openLoginModal() {
    openModal("loginModal");
}

async function quickLogin(username, password = "123456") {
    const userInput = document.getElementById("loginUsername");
    const passInput = document.getElementById("loginPassword");
    if (userInput && passInput) {
        userInput.value = username;
        passInput.value = password;
    }
    const fakeEvent = { preventDefault: () => {} };
    await handleLogin(fakeEvent);
}

async function handleLogin(e) {
    if (e && e.preventDefault) e.preventDefault();
    const submitBtn = document.getElementById("loginSubmitBtn");
    const username = document.getElementById("loginUsername").value.trim();
    const password = document.getElementById("loginPassword").value.trim();

    if (!username || !password) {
        showToast("Vui lòng nhập đầy đủ tài khoản và mật khẩu!", "warning");
        return;
    }

    if (submitBtn) {
        submitBtn.disabled = true;
        submitBtn.textContent = "Đang xử lý...";
    }

    try {
        const response = await fetch(`${getGatewayUrl()}/api/Auth/login`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ username, password })
        });

        const data = await response.json();

        if (!response.ok) {
            throw new Error(data.message || `Đăng nhập thất bại (HTTP ${response.status})`);
        }

        if (!data.token) {
            throw new Error("API không trả về Token");
        }

        saveAuth(data.token, data.user);
        closeModal("loginModal");
        document.getElementById("loginForm").reset();

        const role = getCurrentRole();
        const roleIcon = role === "Admin" ? "👑" : role === "Manager" ? "👔" : "👤";
        showToast(`Đăng nhập thành công! Vai trò: ${roleIcon} [${role}] - Xin chào ${data.user?.fullName || data.user?.username}`, "success");

        // Nếu trang hiện tại không có quyền truy cập với role mới, chuyển về dashboard
        const allowedRoles = PAGE_PERMISSIONS[currentPage] || ["Admin"];
        if (!allowedRoles.includes(role)) {
            showPage("dashboard");
        } else {
            showPage(currentPage);
        }

    } catch (err) {
        console.error("Lỗi đăng nhập:", err);
        showToast(err.message, "error");
    } finally {
        if (submitBtn) {
            submitBtn.disabled = false;
            submitBtn.textContent = "Đăng nhập";
        }
    }
}

function handleLogout() {
    clearAuth();
    showToast("Đã đăng xuất tài khoản thành công.", "info");
    showPage("dashboard");
}

function toggleManualTokenInput() {
    const area = document.getElementById("manualTokenArea");
    if (area) {
        area.hidden = !area.hidden;
    }
}

function applyManualToken() {
    const token = document.getElementById("manualJwtToken").value.trim();
    if (!token) {
        showToast("Vui lòng dán JWT token!", "warning");
        return;
    }
    // Giả định role Admin khi dán thủ công nếu không parse
    const dummyUser = { username: "TokenUser", fullName: "Quản trị viên (Token)", role: "Admin", roleId: 1 };
    saveAuth(token, dummyUser);
    closeModal("loginModal");
    showToast("Đã áp dụng JWT Token thủ công thành công!", "success");
    refreshCurrentPage();
}

// =========================================================
// 3. UNIVERSAL API FETCHER (GỌI QUA API GATEWAY)
// =========================================================
async function apiFetch(endpoint, options = {}) {
    const gatewayUrl = getGatewayUrl();
    const fullUrl = `${gatewayUrl}/api${endpoint.startsWith('/') ? endpoint : '/' + endpoint}`;

    const headers = {
        "Accept": "application/json",
        ...(options.headers || {})
    };

    if (options.body && typeof options.body === "object" && !(options.body instanceof FormData)) {
        headers["Content-Type"] = "application/json";
        options.body = JSON.stringify(options.body);
    }

    const token = getToken();
    if (token) {
        headers["Authorization"] = `Bearer ${token}`;
    }

    try {
        const response = await fetch(fullUrl, {
            ...options,
            headers
        });

        const text = await response.text();
        let data = null;
        try {
            data = text ? JSON.parse(text) : null;
        } catch {
            data = { message: text };
        }

        if (!response.ok) {
            if (response.status === 401) {
                showToast("Lỗi 401: Bạn chưa đăng nhập hoặc phiên làm việc đã hết hạn. Vui lòng đăng nhập lại!", "error");
                openLoginModal();
                throw new Error("Unauthorized (401)");
            } else if (response.status === 403) {
                showToast("Lỗi 403: Tài khoản không đủ quyền hạn (cần quyền Admin hoặc Manager).", "error");
                throw new Error("Forbidden (403)");
            } else {
                const errorMsg = data?.message || data?.title || `Lỗi máy chủ (HTTP ${response.status})`;
                throw new Error(errorMsg);
            }
        }

        return data;

    } catch (error) {
        if (error.name === "TypeError" && error.message.includes("fetch")) {
            showToast("Không thể kết nối đến API Gateway. Hãy kiểm tra Gateway đang chạy!", "error");
            const dot = document.getElementById("gatewayStatusDot");
            if (dot) dot.className = "status-dot offline";
        }
        throw error;
    }
}

// =========================================================
// 4. BỘ NHỚ ĐỆM DỮ LIỆU & DROPDOWNS
// =========================================================
let cachedCategories = [];
let cachedVenues = [];
let cachedEvents = [];
let cachedUsers = [];
let cachedBookings = [];
let cachedTicketTypes = [];
let cachedPayments = [];
let cachedTickets = [];

async function populateCategoriesDropdown(selectedId = null) {
    try {
        cachedCategories = await apiFetch("/EventCategories");
        const select = document.getElementById("eventCategorySelect");
        if (select) {
            select.innerHTML = '<option value="">-- Chọn danh mục --</option>' +
                cachedCategories.map(c => `<option value="${c.id}" ${selectedId == c.id ? 'selected' : ''}>${c.name}</option>`).join("");
        }
    } catch (e) {
        console.warn("Không thể tải danh mục vào dropdown:", e);
    }
}

async function populateVenuesDropdown(selectedId = null) {
    try {
        cachedVenues = await apiFetch("/Venues");
        const select = document.getElementById("eventVenueSelect");
        if (select) {
            select.innerHTML = '<option value="">-- Chọn địa điểm --</option>' +
                cachedVenues.map(v => `<option value="${v.venueId}" ${selectedId == v.venueId ? 'selected' : ''}>${v.venueName} (${v.city})</option>`).join("");
        }
    } catch (e) {
        console.warn("Không thể tải địa điểm vào dropdown:", e);
    }
}

async function populateEventsDropdown(selectedId = null) {
    try {
        cachedEvents = await apiFetch("/Events");
        const select = document.getElementById("ticketTypeEventSelect");
        if (select) {
            select.innerHTML = '<option value="">-- Chọn sự kiện --</option>' +
                cachedEvents.map(ev => `<option value="${ev.eventId}" ${selectedId == ev.eventId ? 'selected' : ''}>#${ev.eventId} - ${ev.eventName}</option>`).join("");
        }
    } catch (e) {
        console.warn("Không thể tải sự kiện vào dropdown:", e);
    }
}

async function populateUsersDropdown(selectedId = null) {
    try {
        cachedUsers = await apiFetch("/Users");
        const select = document.getElementById("bookingUserSelect");
        if (select) {
            select.innerHTML = '<option value="">-- Chọn khách hàng --</option>' +
                cachedUsers.map(u => `<option value="${u.userId}" ${selectedId == u.userId ? 'selected' : ''}>#${u.userId} - ${u.fullName || u.username} (${u.email})</option>`).join("");
        }
    } catch (e) {
        console.warn("Không thể tải users vào dropdown:", e);
    }
}

async function populateBookingsDropdown(selectedId = null) {
    try {
        cachedBookings = await apiFetch("/Bookings");
        const select = document.getElementById("paymentBookingSelect");
        if (select) {
            select.innerHTML = '<option value="">-- Chọn đơn đặt vé --</option>' +
                cachedBookings.map(b => `<option value="${b.bookingId}" ${selectedId == b.bookingId ? 'selected' : ''}>#${b.bookingId} - Mã: ${b.bookingCode || 'N/A'} (${formatCurrency(b.totalAmount)})</option>`).join("");
        }
    } catch (e) {
        console.warn("Không thể tải bookings vào dropdown:", e);
    }
}

// =========================================================
// 5. TỔNG QUAN (DASHBOARD)
// =========================================================
async function refreshDashboard() {
    showToast("Đang đồng bộ dữ liệu Tổng quan...", "info");
    applyRolePermissions();
    const role = getCurrentRole();
    const currentUser = getCurrentUser();

    try {
        const fetchTasks = [apiFetch("/Events")];

        if (hasRole("Admin", "Manager")) {
            fetchTasks.push(apiFetch("/Venues"));
            fetchTasks.push(apiFetch("/TicketTypes"));
            fetchTasks.push(apiFetch("/Payments"));
        } else {
            fetchTasks.push(Promise.resolve([]));
            fetchTasks.push(Promise.resolve([]));
            fetchTasks.push(Promise.resolve([]));
        }

        if (isAuthenticated()) {
            fetchTasks.push(apiFetch("/Bookings"));
            fetchTasks.push(apiFetch("/Tickets"));
            fetchTasks.push(apiFetch("/BookingDetails"));
        } else {
            fetchTasks.push(Promise.resolve([]));
            fetchTasks.push(Promise.resolve([]));
            fetchTasks.push(Promise.resolve([]));
        }

        if (isAdmin()) {
            fetchTasks.push(apiFetch("/Users"));
        } else {
            fetchTasks.push(Promise.resolve([]));
        }

        const [events, venues, ticketTypes, payments, bookings, tickets, bookingDetails, users] = await Promise.allSettled(fetchTasks);

        const eventsList = events.status === "fulfilled" && Array.isArray(events.value) ? events.value : [];
        const venuesList = venues.status === "fulfilled" && Array.isArray(venues.value) ? venues.value : [];
        const ticketTypesList = ticketTypes.status === "fulfilled" && Array.isArray(ticketTypes.value) ? ticketTypes.value : [];
        const paymentsList = payments.status === "fulfilled" && Array.isArray(payments.value) ? payments.value : [];
        let bookingsList = bookings.status === "fulfilled" && Array.isArray(bookings.value) ? bookings.value : [];
        let ticketsList = tickets.status === "fulfilled" && Array.isArray(tickets.value) ? tickets.value : [];
        const bookingDetailsList = bookingDetails.status === "fulfilled" && Array.isArray(bookingDetails.value) ? bookingDetails.value : [];
        const usersList = users.status === "fulfilled" && Array.isArray(users.value) ? users.value : [];

        // Lưu cache để dùng chung
        cachedEvents = eventsList;
        cachedBookings = bookingsList;
        cachedTickets = ticketsList;
        cachedBookingDetails = bookingDetailsList;
        if (venuesList.length) cachedVenues = venuesList;
        if (ticketTypesList.length) cachedTicketTypes = ticketTypesList;
        if (paymentsList.length) cachedPayments = paymentsList;
        if (usersList.length) cachedUsers = usersList;

        // Cập nhật số liệu thống kê
        const elEvents = document.getElementById("statTotalEvents");
        if (elEvents) elEvents.textContent = eventsList.length;

        const elVenues = document.getElementById("statTotalVenues");
        if (elVenues) elVenues.textContent = venuesList.length;

        const elTicketTypes = document.getElementById("statTotalTicketTypes");
        if (elTicketTypes) elTicketTypes.textContent = ticketTypesList.length;

        const elBookings = document.getElementById("statTotalBookings");
        if (elBookings) {
            if (role === "User" && currentUser) {
                const userBookings = bookingsList.filter(b => b.userId === currentUser.userId);
                elBookings.textContent = userBookings.length;
            } else {
                elBookings.textContent = bookingsList.length;
            }
        }

        const elUsers = document.getElementById("statTotalUsers");
        if (elUsers) elUsers.textContent = usersList.length;

        const elMyTickets = document.getElementById("statTotalMyTickets");
        if (elMyTickets && currentUser) {
            const userBookingIds = bookingsList.filter(b => b.userId === currentUser.userId).map(b => b.bookingId);
            const userBDIds = bookingDetailsList.filter(bd => userBookingIds.includes(bd.bookingId)).map(bd => bd.bookingDetailId);
            const userTickets = ticketsList.filter(t =>
                (t.attendeeEmail && currentUser.email && t.attendeeEmail.toLowerCase() === currentUser.email.toLowerCase()) ||
                userBDIds.includes(t.bookingDetailId) ||
                (t.attendeeName && currentUser.fullName && t.attendeeName.toLowerCase() === currentUser.fullName.toLowerCase())
            );
            elMyTickets.textContent = userTickets.length;
        }

        // Tính tổng doanh thu từ payments (Success) cho Admin / Manager
        const elRevenue = document.getElementById("statTotalRevenue");
        if (elRevenue) {
            const totalRevenue = paymentsList
                .filter(p => {
                    const st = (p.status || "").toUpperCase();
                    return st === "SUCCESS" || st === "COMPLETED" || st === "THÀNH CÔNG";
                })
                .reduce((sum, p) => sum + (Number(p.amount) || 0), 0);
            elRevenue.textContent = formatCurrency(totalRevenue);
        }

        // Recent Events Table (tối đa 5 sự kiện)
        const recentEventsTbody = document.getElementById("dashRecentEvents");
        if (recentEventsTbody) {
            if (eventsList.length === 0) {
                recentEventsTbody.innerHTML = `<tr><td colspan="4" class="text-center text-muted">Chưa có sự kiện nào.</td></tr>`;
            } else {
                recentEventsTbody.innerHTML = eventsList.slice(0, 5).map(ev => `
                    <tr>
                        <td><strong>#${ev.eventId}</strong></td>
                        <td>${escapeHtml(ev.eventName)}</td>
                        <td>${formatDateTime(ev.startDate)}</td>
                        <td>${renderStatusBadge(ev.status)}</td>
                    </tr>
                `).join("");
            }
        }

        // Recent Bookings Table (tối đa 5 đơn)
        const recentBookingsTbody = document.getElementById("dashRecentBookings");
        if (recentBookingsTbody) {
            let displayBookings = bookingsList;
            if (role === "User" && currentUser) {
                displayBookings = bookingsList.filter(b => b.userId === currentUser.userId);
            }

            if (displayBookings.length === 0) {
                recentBookingsTbody.innerHTML = `<tr><td colspan="4" class="text-center text-muted">${role === 'User' ? 'Bạn chưa có đơn đặt vé nào.' : 'Chưa có đơn đặt vé nào.'}</td></tr>`;
            } else {
                recentBookingsTbody.innerHTML = displayBookings.slice(0, 5).map(b => `
                    <tr>
                        <td><strong>${escapeHtml(b.bookingCode || '#' + b.bookingId)}</strong></td>
                        <td>${role === 'User' ? 'Chính bạn' : 'User #' + b.userId}</td>
                        <td><strong>${formatCurrency(b.totalAmount)}</strong></td>
                        <td>${renderStatusBadge(b.status)}</td>
                    </tr>
                `).join("");
            }
        }

        showToast("Đã tải dữ liệu tổng quan thành công!", "success");
    } catch (err) {
        console.error("Lỗi tải dashboard:", err);
        showToast("Có lỗi khi tải dữ liệu bảng điều khiển.", "error");
    }
}

// =========================================================
// 6. QUẢN LÝ SỰ KIỆN (EVENTS)
// =========================================================
async function loadEvents() {
    const tbody = document.getElementById("eventTableBody");
    tbody.innerHTML = `<tr><td colspan="7" class="text-center text-muted">Đang tải dữ liệu từ API Gateway...</td></tr>`;

    try {
        // Tải kèm danh mục và địa điểm để map tên hiển thị
        await Promise.allSettled([
            populateCategoriesDropdown(),
            populateVenuesDropdown()
        ]);

        const data = await apiFetch("/Events");
        cachedEvents = Array.isArray(data) ? data : [];
        filterEvents();
    } catch (err) {
        tbody.innerHTML = `<tr><td colspan="7" class="text-center text-danger">Lỗi tải sự kiện: ${escapeHtml(err.message)}</td></tr>`;
    }
}

function filterEvents() {
    const search = document.getElementById("eventSearchInput")?.value.toLowerCase().trim() || "";
    const status = document.getElementById("eventStatusFilter")?.value || "";

    const filtered = cachedEvents.filter(ev => {
        const matchesSearch = String(ev.eventId).includes(search) || (ev.eventName || "").toLowerCase().includes(search);
        const matchesStatus = !status || (ev.status || "").toUpperCase() === status.toUpperCase();
        return matchesSearch && matchesStatus;
    });

    renderEventsTable(filtered);
}

function renderEventsTable(events) {
    const tbody = document.getElementById("eventTableBody");
    if (!tbody) return;

    if (events.length === 0) {
        tbody.innerHTML = `<tr><td colspan="7" class="text-center text-muted">Không tìm thấy sự kiện nào phù hợp.</td></tr>`;
        return;
    }

    const role = getCurrentRole();

    tbody.innerHTML = events.map(ev => {
        const cat = cachedCategories.find(c => c.id === ev.categoryId);
        const venue = cachedVenues.find(v => v.venueId === ev.venueId);

        let actionHtml = '';
        if (role === 'Admin') {
            actionHtml = `
                <div class="action-buttons">
                    <button class="btn btn-secondary btn-sm" onclick="editEvent(${ev.eventId})">✏️ Sửa</button>
                    <button class="btn btn-danger btn-sm" onclick="deleteEvent(${ev.eventId}, '${escapeHtml(ev.eventName)}')">🗑️ Xóa</button>
                </div>
            `;
        } else if (role === 'Manager') {
            // Manager có quyền sửa, nhưng KHÔNG có quyền xóa sự kiện (backend [Authorize(Roles = "Admin")])
            actionHtml = `
                <div class="action-buttons">
                    <button class="btn btn-secondary btn-sm" onclick="editEvent(${ev.eventId})">✏️ Sửa</button>
                </div>
            `;
        } else if (role === 'User') {
            const isAvailable = ev.status === 'OPEN' || ev.status === 'UPCOMING';
            actionHtml = `
                <div class="action-buttons">
                    ${isAvailable
                        ? `<button class="btn btn-primary btn-sm" onclick="openUserBookingModal(${ev.eventId})">🎟️ Đặt vé</button>`
                        : `<span class="badge badge-closed">Đã đóng</span>`}
                </div>
            `;
        } else {
            // Guest (chưa đăng nhập)
            const isAvailable = ev.status === 'OPEN' || ev.status === 'UPCOMING';
            actionHtml = `
                <div class="action-buttons">
                    ${isAvailable
                        ? `<button class="btn btn-outline btn-sm" onclick="promptLoginForBooking('${escapeHtml(ev.eventName)}')">🎟️ Đặt vé</button>`
                        : `<span class="badge badge-closed">Đã đóng</span>`}
                </div>
            `;
        }

        return `
            <tr>
                <td><strong>#${ev.eventId}</strong></td>
                <td>
                    <strong>${escapeHtml(ev.eventName)}</strong>
                    ${ev.description ? `<br><small class="text-muted">${escapeHtml(ev.description.substring(0, 50))}...</small>` : ''}
                </td>
                <td><span class="badge badge-role-user">${cat ? escapeHtml(cat.name) : 'Danh mục #' + ev.categoryId}</span></td>
                <td>${venue ? escapeHtml(venue.venueName) : 'Địa điểm #' + ev.venueId}</td>
                <td>
                    <small>Từ: ${formatDateTime(ev.startDate)}</small><br>
                    <small>Đến: ${formatDateTime(ev.endDate)}</small>
                </td>
                <td>${renderStatusBadge(ev.status)}</td>
                <td class="text-right">
                    ${actionHtml}
                </td>
            </tr>
        `;
    }).join("");
}

function openEventModal(event = null) {
    if (!hasRole("Admin", "Manager")) {
        showToast("Lỗi 403: Chỉ Admin hoặc Manager mới có quyền tạo hoặc chỉnh sửa sự kiện!", "error");
        return;
    }
    document.getElementById("eventForm").reset();
    populateCategoriesDropdown(event?.categoryId);
    populateVenuesDropdown(event?.venueId);

    const title = document.getElementById("eventModalTitle");
    const idInput = document.getElementById("eventFormId");

    if (event) {
        title.textContent = `Sửa sự kiện #${event.eventId}`;
        idInput.value = event.eventId;
        document.getElementById("eventName").value = event.eventName || "";
        document.getElementById("eventStartDate").value = toLocalDatetimeInput(event.startDate);
        document.getElementById("eventEndDate").value = toLocalDatetimeInput(event.endDate);
        document.getElementById("eventOrganizerId").value = event.organizerId || 1;
        document.getElementById("eventStatus").value = event.status || "OPEN";
        document.getElementById("eventImageUrl").value = event.imageUrl || "";
        document.getElementById("eventDescription").value = event.description || "";
    } else {
        title.textContent = "Thêm sự kiện mới";
        idInput.value = "";
        document.getElementById("eventOrganizerId").value = 1;
        document.getElementById("eventStatus").value = "OPEN";
    }

    openModal("eventModal");
}

function editEvent(id) {
    const event = cachedEvents.find(e => e.eventId === id);
    if (!event) {
        showToast("Không tìm thấy sự kiện!", "error");
        return;
    }
    openEventModal(event);
}

async function handleSaveEvent(e) {
    e.preventDefault();
    const submitBtn = document.getElementById("eventSubmitBtn");
    const id = document.getElementById("eventFormId").value;
    const isEdit = Boolean(id);

    const name = document.getElementById("eventName").value.trim();
    const categoryId = Number(document.getElementById("eventCategorySelect").value);
    const venueId = Number(document.getElementById("eventVenueSelect").value);
    const startDate = document.getElementById("eventStartDate").value;
    const endDate = document.getElementById("eventEndDate").value;
    const organizerId = Number(document.getElementById("eventOrganizerId").value);
    const status = document.getElementById("eventStatus").value;
    const imageUrl = document.getElementById("eventImageUrl").value.trim();
    const description = document.getElementById("eventDescription").value.trim();

    if (!name) {
        showToast("Tên sự kiện không được để trống!", "warning");
        return;
    }
    if (!categoryId || !venueId) {
        showToast("Vui lòng chọn danh mục và địa điểm!", "warning");
        return;
    }
    if (new Date(endDate) <= new Date(startDate)) {
        showToast("Thời gian kết thúc phải sau thời gian bắt đầu!", "warning");
        return;
    }

    const payload = {
        eventId: isEdit ? Number(id) : 0,
        eventName: name,
        categoryId,
        venueId,
        startDate: new Date(startDate).toISOString(),
        endDate: new Date(endDate).toISOString(),
        organizerId,
        status,
        imageUrl,
        description,
        isDeleted: false,
        createdAt: isEdit ? (cachedEvents.find(ev => ev.eventId == id)?.createdAt || new Date().toISOString()) : new Date().toISOString(),
        updatedAt: isEdit ? new Date().toISOString() : null
    };

    submitBtn.disabled = true;
    submitBtn.textContent = "Đang lưu...";

    try {
        if (isEdit) {
            await apiFetch(`/Events/${id}`, {
                method: "PUT",
                body: payload
            });
            showToast(`Cập nhật sự kiện #${id} thành công!`, "success");
        } else {
            const res = await apiFetch("/Events", {
                method: "POST",
                body: payload
            });
            showToast(`Thêm sự kiện mới thành công (ID: ${res?.eventId || 'Mới'})!`, "success");
        }

        closeModal("eventModal");
        await loadEvents();
    } catch (err) {
        showToast(err.message, "error");
    } finally {
        submitBtn.disabled = false;
        submitBtn.textContent = "Lưu sự kiện";
    }
}

async function deleteEvent(id, name) {
    if (!isAdmin()) {
        showToast("Lỗi 403: Chỉ tài khoản Quản trị viên (Admin) mới có quyền xóa sự kiện!", "error");
        return;
    }
    if (!confirm(`Bạn có chắc chắn muốn xóa sự kiện:\n"${name}" (ID: ${id})?\n\nThao tác này yêu cầu quyền Admin.`)) {
        return;
    }

    try {
        await apiFetch(`/Events/${id}`, { method: "DELETE" });
        showToast(`Đã xóa sự kiện ID ${id} thành công!`, "success");
        await loadEvents();
    } catch (err) {
        showToast(err.message, "error");
    }
}

// =========================================================
// 7. QUẢN LÝ DANH MỤC (CATEGORIES)
// =========================================================
async function loadCategories() {
    const tbody = document.getElementById("categoryTableBody");
    tbody.innerHTML = `<tr><td colspan="4" class="text-center text-muted">Đang tải danh mục...</td></tr>`;

    try {
        const data = await apiFetch("/EventCategories");
        cachedCategories = Array.isArray(data) ? data : [];
        filterCategories();
    } catch (err) {
        tbody.innerHTML = `<tr><td colspan="4" class="text-center text-danger">Lỗi tải danh mục: ${escapeHtml(err.message)}</td></tr>`;
    }
}

function filterCategories() {
    const search = document.getElementById("categorySearchInput")?.value.toLowerCase().trim() || "";
    const filtered = cachedCategories.filter(c => String(c.id).includes(search) || (c.name || "").toLowerCase().includes(search));
    renderCategoriesTable(filtered);
}

function renderCategoriesTable(categories) {
    const tbody = document.getElementById("categoryTableBody");
    if (!tbody) return;

    if (categories.length === 0) {
        tbody.innerHTML = `<tr><td colspan="4" class="text-center text-muted">Chưa có danh mục nào.</td></tr>`;
        return;
    }

    const isAdm = isAdmin();

    tbody.innerHTML = categories.map(c => `
        <tr>
            <td><strong>#${c.id}</strong></td>
            <td><strong>${escapeHtml(c.name)}</strong></td>
            <td>${escapeHtml(c.description || 'Không có mô tả')}</td>
            <td class="text-right">
                <div class="action-buttons">
                    <button class="btn btn-secondary btn-sm" onclick="editCategory(${c.id})">✏️ Sửa</button>
                    ${isAdm ? `<button class="btn btn-danger btn-sm" onclick="deleteCategory(${c.id}, '${escapeHtml(c.name)}')">🗑️ Xóa</button>` : ''}
                </div>
            </td>
        </tr>
    `).join("");
}

function openCategoryModal(cat = null) {
    if (!hasRole("Admin", "Manager")) {
        showToast("Lỗi 403: Chỉ Admin hoặc Manager mới có quyền quản lý danh mục sự kiện!", "error");
        return;
    }
    document.getElementById("categoryForm").reset();
    const title = document.getElementById("categoryModalTitle");
    const idInput = document.getElementById("categoryFormId");

    if (cat) {
        title.textContent = `Sửa danh mục #${cat.id}`;
        idInput.value = cat.id;
        document.getElementById("categoryName").value = cat.name || "";
        document.getElementById("categoryDescription").value = cat.description || "";
    } else {
        title.textContent = "Thêm danh mục mới";
        idInput.value = "";
    }

    openModal("categoryModal");
}

function editCategory(id) {
    const cat = cachedCategories.find(c => c.id === id);
    if (cat) openCategoryModal(cat);
}

async function handleSaveCategory(e) {
    e.preventDefault();
    const submitBtn = document.getElementById("categorySubmitBtn");
    const id = document.getElementById("categoryFormId").value;
    const isEdit = Boolean(id);

    const name = document.getElementById("categoryName").value.trim();
    const description = document.getElementById("categoryDescription").value.trim();

    if (!name) {
        showToast("Tên danh mục không được để trống!", "warning");
        return;
    }

    const payload = {
        id: isEdit ? Number(id) : 0,
        name,
        description,
        isDeleted: false,
        createdAt: new Date().toISOString()
    };

    submitBtn.disabled = true;
    submitBtn.textContent = "Đang lưu...";

    try {
        if (isEdit) {
            await apiFetch(`/EventCategories/${id}`, { method: "PUT", body: payload });
            showToast(`Cập nhật danh mục #${id} thành công!`, "success");
        } else {
            await apiFetch("/EventCategories", { method: "POST", body: payload });
            showToast("Thêm danh mục mới thành công!", "success");
        }
        closeModal("categoryModal");
        await loadCategories();
    } catch (err) {
        showToast(err.message, "error");
    } finally {
        submitBtn.disabled = false;
        submitBtn.textContent = "Lưu danh mục";
    }
}

async function deleteCategory(id, name) {
    if (!confirm(`Bạn có chắc muốn xóa danh mục "${name}" (ID: ${id})?`)) return;

    try {
        await apiFetch(`/EventCategories/${id}`, { method: "DELETE" });
        showToast(`Đã xóa danh mục #${id} thành công!`, "success");
        await loadCategories();
    } catch (err) {
        showToast(err.message, "error");
    }
}

// =========================================================
// 8. QUẢN LÝ ĐỊA ĐIỂM (VENUES)
// =========================================================
async function loadVenues() {
    const tbody = document.getElementById("venueTableBody");
    tbody.innerHTML = `<tr><td colspan="7" class="text-center text-muted">Đang tải địa điểm...</td></tr>`;

    try {
        const data = await apiFetch("/Venues");
        cachedVenues = Array.isArray(data) ? data : [];
        filterVenues();
    } catch (err) {
        tbody.innerHTML = `<tr><td colspan="7" class="text-center text-danger">Lỗi tải địa điểm: ${escapeHtml(err.message)}</td></tr>`;
    }
}

function filterVenues() {
    const search = document.getElementById("venueSearchInput")?.value.toLowerCase().trim() || "";
    const filtered = cachedVenues.filter(v =>
        String(v.venueId).includes(search) ||
        (v.venueName || "").toLowerCase().includes(search) ||
        (v.city || "").toLowerCase().includes(search)
    );
    renderVenuesTable(filtered);
}

function renderVenuesTable(venues) {
    const tbody = document.getElementById("venueTableBody");
    if (!tbody) return;

    if (venues.length === 0) {
        tbody.innerHTML = `<tr><td colspan="7" class="text-center text-muted">Chưa có địa điểm nào.</td></tr>`;
        return;
    }

    const isAdm = isAdmin();

    tbody.innerHTML = venues.map(v => `
        <tr>
            <td><strong>#${v.venueId}</strong></td>
            <td><strong>${escapeHtml(v.venueName)}</strong></td>
            <td>${escapeHtml(v.address)}</td>
            <td><span class="badge badge-upcoming">${escapeHtml(v.city)}</span></td>
            <td><strong>${(v.capacity || 0).toLocaleString('vi-VN')} người</strong></td>
            <td><small class="text-muted">${escapeHtml(v.description || 'Không có mô tả')}</small></td>
            <td class="text-right">
                <div class="action-buttons">
                    <button class="btn btn-secondary btn-sm" onclick="editVenue(${v.venueId})">✏️ Sửa</button>
                    ${isAdm ? `<button class="btn btn-danger btn-sm" onclick="deleteVenue(${v.venueId}, '${escapeHtml(v.venueName)}')">🗑️ Xóa</button>` : ''}
                </div>
            </td>
        </tr>
    `).join("");
}

function openVenueModal(venue = null) {
    if (!hasRole("Admin", "Manager")) {
        showToast("Lỗi 403: Chỉ Admin hoặc Manager mới có quyền quản lý địa điểm!", "error");
        return;
    }
    document.getElementById("venueForm").reset();
    const title = document.getElementById("venueModalTitle");
    const idInput = document.getElementById("venueFormId");

    if (venue) {
        title.textContent = `Sửa địa điểm #${venue.venueId}`;
        idInput.value = venue.venueId;
        document.getElementById("venueName").value = venue.venueName || "";
        document.getElementById("venueAddress").value = venue.address || "";
        document.getElementById("venueCity").value = venue.city || "";
        document.getElementById("venueCapacity").value = venue.capacity || 1000;
        document.getElementById("venueDescription").value = venue.description || "";
    } else {
        title.textContent = "Thêm địa điểm mới";
        idInput.value = "";
        document.getElementById("venueCapacity").value = 1000;
    }

    openModal("venueModal");
}

function editVenue(id) {
    const v = cachedVenues.find(item => item.venueId === id);
    if (v) openVenueModal(v);
}

async function handleSaveVenue(e) {
    e.preventDefault();
    const submitBtn = document.getElementById("venueSubmitBtn");
    const id = document.getElementById("venueFormId").value;
    const isEdit = Boolean(id);

    const venueName = document.getElementById("venueName").value.trim();
    const address = document.getElementById("venueAddress").value.trim();
    const city = document.getElementById("venueCity").value.trim();
    const capacity = Number(document.getElementById("venueCapacity").value);
    const description = document.getElementById("venueDescription").value.trim();

    if (!venueName || !address || !city || capacity <= 0) {
        showToast("Vui lòng điền đúng và đủ thông tin địa điểm!", "warning");
        return;
    }

    const payload = {
        venueId: isEdit ? Number(id) : 0,
        venueName,
        address,
        city,
        capacity,
        description,
        isDeleted: false,
        createdAt: new Date().toISOString(),
        updatedAt: isEdit ? new Date().toISOString() : null
    };

    submitBtn.disabled = true;
    submitBtn.textContent = "Đang lưu...";

    try {
        if (isEdit) {
            await apiFetch(`/Venues/${id}`, { method: "PUT", body: payload });
            showToast(`Cập nhật địa điểm #${id} thành công!`, "success");
        } else {
            await apiFetch("/Venues", { method: "POST", body: payload });
            showToast("Thêm địa điểm mới thành công!", "success");
        }
        closeModal("venueModal");
        await loadVenues();
    } catch (err) {
        showToast(err.message, "error");
    } finally {
        submitBtn.disabled = false;
        submitBtn.textContent = "Lưu địa điểm";
    }
}

async function deleteVenue(id, name) {
    if (!confirm(`Bạn có chắc muốn xóa địa điểm "${name}" (ID: ${id})?`)) return;

    try {
        await apiFetch(`/Venues/${id}`, { method: "DELETE" });
        showToast(`Đã xóa địa điểm #${id} thành công!`, "success");
        await loadVenues();
    } catch (err) {
        showToast(err.message, "error");
    }
}

// =========================================================
// 9. QUẢN LÝ LOẠI VÉ (TICKET TYPES)
// =========================================================
async function loadTicketTypes() {
    const tbody = document.getElementById("ticketTypeTableBody");
    tbody.innerHTML = `<tr><td colspan="9" class="text-center text-muted">Đang tải loại vé...</td></tr>`;

    try {
        await populateEventsDropdown();
        const data = await apiFetch("/TicketTypes");
        cachedTicketTypes = Array.isArray(data) ? data : [];
        filterTicketTypes();
    } catch (err) {
        tbody.innerHTML = `<tr><td colspan="9" class="text-center text-danger">Lỗi tải loại vé: ${escapeHtml(err.message)}</td></tr>`;
    }
}

function filterTicketTypes() {
    const search = document.getElementById("ticketTypeSearchInput")?.value.toLowerCase().trim() || "";
    const filtered = cachedTicketTypes.filter(t =>
        String(t.ticketTypeId).includes(search) ||
        String(t.eventId).includes(search) ||
        (t.ticketName || "").toLowerCase().includes(search)
    );
    renderTicketTypesTable(filtered);
}

function renderTicketTypesTable(ticketTypes) {
    const tbody = document.getElementById("ticketTypeTableBody");
    if (!tbody) return;

    if (ticketTypes.length === 0) {
        tbody.innerHTML = `<tr><td colspan="9" class="text-center text-muted">Chưa có loại vé nào.</td></tr>`;
        return;
    }

    const isAdm = isAdmin();

    tbody.innerHTML = ticketTypes.map(t => {
        const ev = cachedEvents.find(e => e.eventId === t.eventId);
        const remaining = (t.quantity || 0) - (t.soldQuantity || 0);

        return `
            <tr>
                <td><strong>#${t.ticketTypeId}</strong></td>
                <td>${ev ? escapeHtml(ev.eventName) : 'Sự kiện #' + t.eventId}</td>
                <td><span class="badge badge-role-manager">${escapeHtml(t.ticketName)}</span></td>
                <td><strong class="text-primary">${formatCurrency(t.price)}</strong></td>
                <td>${(t.quantity || 0).toLocaleString('vi-VN')}</td>
                <td><span class="badge badge-open">${(t.soldQuantity || 0).toLocaleString('vi-VN')}</span></td>
                <td><strong>${remaining.toLocaleString('vi-VN')}</strong></td>
                <td>
                    <small>Mở: ${formatDateTime(t.saleStart)}</small><br>
                    <small>Đóng: ${formatDateTime(t.saleEnd)}</small>
                </td>
                <td class="text-right">
                    <div class="action-buttons">
                        <button class="btn btn-secondary btn-sm" onclick="editTicketType(${t.ticketTypeId})">✏️ Sửa</button>
                        ${isAdm ? `<button class="btn btn-danger btn-sm" onclick="deleteTicketType(${t.ticketTypeId}, '${escapeHtml(t.ticketName)}')">🗑️ Xóa</button>` : ''}
                    </div>
                </td>
            </tr>
        `;
    }).join("");
}

function openTicketTypeModal(ticketType = null) {
    if (!hasRole("Admin", "Manager")) {
        showToast("Lỗi 403: Chỉ Admin hoặc Manager mới có quyền quản lý loại vé!", "error");
        return;
    }
    document.getElementById("ticketTypeForm").reset();
    populateEventsDropdown(ticketType?.eventId);

    const title = document.getElementById("ticketTypeModalTitle");
    const idInput = document.getElementById("ticketTypeFormId");

    if (ticketType) {
        title.textContent = `Sửa loại vé #${ticketType.ticketTypeId}`;
        idInput.value = ticketType.ticketTypeId;
        document.getElementById("ticketTypeName").value = ticketType.ticketName || "";
        document.getElementById("ticketTypePrice").value = ticketType.price || 0;
        document.getElementById("ticketTypeQuantity").value = ticketType.quantity || 100;
        document.getElementById("ticketTypeSoldQuantity").value = ticketType.soldQuantity || 0;
        document.getElementById("ticketTypeSaleStart").value = toLocalDatetimeInput(ticketType.saleStart);
        document.getElementById("ticketTypeSaleEnd").value = toLocalDatetimeInput(ticketType.saleEnd);
    } else {
        title.textContent = "Thêm loại vé mới";
        idInput.value = "";
        document.getElementById("ticketTypePrice").value = 100000;
        document.getElementById("ticketTypeQuantity").value = 100;
        document.getElementById("ticketTypeSoldQuantity").value = 0;
    }

    openModal("ticketTypeModal");
}

function editTicketType(id) {
    const t = cachedTicketTypes.find(item => item.ticketTypeId === id);
    if (t) openTicketTypeModal(t);
}

async function handleSaveTicketType(e) {
    e.preventDefault();
    const submitBtn = document.getElementById("ticketTypeSubmitBtn");
    const id = document.getElementById("ticketTypeFormId").value;
    const isEdit = Boolean(id);

    const eventId = Number(document.getElementById("ticketTypeEventSelect").value);
    const ticketName = document.getElementById("ticketTypeName").value.trim();
    const price = Number(document.getElementById("ticketTypePrice").value);
    const quantity = Number(document.getElementById("ticketTypeQuantity").value);
    const soldQuantity = Number(document.getElementById("ticketTypeSoldQuantity").value) || 0;
    const saleStart = document.getElementById("ticketTypeSaleStart").value;
    const saleEnd = document.getElementById("ticketTypeSaleEnd").value;

    if (!eventId || !ticketName || price < 0 || quantity < 0) {
        showToast("Vui lòng điền đúng thông tin loại vé!", "warning");
        return;
    }
    if (soldQuantity > quantity) {
        showToast("Số lượng đã bán không được vượt quá tổng số lượng!", "warning");
        return;
    }

    const payload = {
        ticketTypeId: isEdit ? Number(id) : 0,
        eventId,
        ticketName,
        price,
        quantity,
        soldQuantity,
        saleStart: saleStart ? new Date(saleStart).toISOString() : null,
        saleEnd: saleEnd ? new Date(saleEnd).toISOString() : null,
        isDeleted: false,
        createdAt: new Date().toISOString()
    };

    submitBtn.disabled = true;
    submitBtn.textContent = "Đang lưu...";

    try {
        if (isEdit) {
            await apiFetch(`/TicketTypes/${id}`, { method: "PUT", body: payload });
            showToast(`Cập nhật loại vé #${id} thành công!`, "success");
        } else {
            await apiFetch("/TicketTypes", { method: "POST", body: payload });
            showToast("Thêm loại vé mới thành công!", "success");
        }
        closeModal("ticketTypeModal");
        await loadTicketTypes();
    } catch (err) {
        showToast(err.message, "error");
    } finally {
        submitBtn.disabled = false;
        submitBtn.textContent = "Lưu loại vé";
    }
}

async function deleteTicketType(id, name) {
    if (!confirm(`Bạn có chắc muốn xóa loại vé "${name}" (ID: ${id})?`)) return;

    try {
        await apiFetch(`/TicketTypes/${id}`, { method: "DELETE" });
        showToast(`Đã xóa loại vé #${id} thành công!`, "success");
        await loadTicketTypes();
    } catch (err) {
        showToast(err.message, "error");
    }
}

// =========================================================
// 10. QUẢN LÝ ĐẶT VÉ (BOOKINGS)
// =========================================================
async function loadBookings() {
    const tbody = document.getElementById("bookingTableBody");
    tbody.innerHTML = `<tr><td colspan="7" class="text-center text-muted">Đang tải đơn đặt vé...</td></tr>`;

    try {
        if (isAdmin() || isManager()) {
            await populateUsersDropdown();
        }
        const data = await apiFetch("/Bookings");
        cachedBookings = Array.isArray(data) ? data : [];
        filterBookings();
    } catch (err) {
        tbody.innerHTML = `<tr><td colspan="7" class="text-center text-danger">Lỗi tải đơn đặt vé: ${escapeHtml(err.message)}</td></tr>`;
    }
}

function filterBookings() {
    const search = document.getElementById("bookingSearchInput")?.value.toLowerCase().trim() || "";
    const status = document.getElementById("bookingStatusFilter")?.value || "";
    const role = getCurrentRole();
    const currentUser = getCurrentUser();

    let list = cachedBookings;
    if (role === "User" && currentUser) {
        list = list.filter(b => b.userId === currentUser.userId);
    }

    const filtered = list.filter(b => {
        const matchesSearch = String(b.bookingId).includes(search) ||
            String(b.userId).includes(search) ||
            (b.bookingCode || "").toLowerCase().includes(search);
        const matchesStatus = !status || (b.status || "").toLowerCase() === status.toLowerCase();
        return matchesSearch && matchesStatus;
    });

    renderBookingsTable(filtered);
}

function renderBookingsTable(bookings) {
    const tbody = document.getElementById("bookingTableBody");
    if (!tbody) return;

    if (bookings.length === 0) {
        const role = getCurrentRole();
        tbody.innerHTML = `<tr><td colspan="7" class="text-center text-muted">${role === 'User' ? 'Bạn chưa có đơn đặt vé nào. Hãy bấm "Đặt vé" tại trang Sự kiện!' : 'Chưa có đơn đặt vé nào.'}</td></tr>`;
        return;
    }

    const role = getCurrentRole();

    tbody.innerHTML = bookings.map(b => {
        const user = cachedUsers.find(u => u.userId === b.userId);

        let actionHtml = '';
        if (role === 'Admin') {
            actionHtml = `
                <div class="action-buttons">
                    <button class="btn btn-secondary btn-sm" onclick="editBooking(${b.bookingId})">✏️ Sửa</button>
                    <button class="btn btn-danger btn-sm" onclick="deleteBooking(${b.bookingId}, '${escapeHtml(b.bookingCode || '#' + b.bookingId)}')">🗑️ Xóa</button>
                </div>
            `;
        } else if (role === 'Manager') {
            actionHtml = `
                <div class="action-buttons">
                    <button class="btn btn-secondary btn-sm" onclick="editBooking(${b.bookingId})">✏️ Sửa</button>
                </div>
            `;
        } else {
            // User
            if ((b.status || '').toLowerCase() === 'pending') {
                actionHtml = `
                    <div class="action-buttons">
                        <button class="btn btn-success btn-sm" onclick="openUserPaymentModal(${b.bookingId})">💳 Thanh toán</button>
                        <button class="btn btn-danger btn-sm" onclick="cancelUserBooking(${b.bookingId})">Hủy đơn</button>
                    </div>
                `;
            } else if ((b.status || '').toLowerCase() === 'cancelled') {
                actionHtml = `<span class="badge badge-cancelled">Đã hủy</span>`;
            } else {
                actionHtml = `<span class="badge badge-open">✓ Đã thanh toán</span>`;
            }
        }

        return `
            <tr>
                <td><strong>#${b.bookingId}</strong></td>
                <td><span class="badge badge-role-admin">${escapeHtml(b.bookingCode || 'N/A')}</span></td>
                <td>${role === 'User' ? 'Chính bạn' : (user ? escapeHtml(user.fullName || user.username) : 'User #' + b.userId)}</td>
                <td>${formatDateTime(b.bookingDate || b.createdAt)}</td>
                <td><strong class="text-primary">${formatCurrency(b.totalAmount)}</strong></td>
                <td>${renderStatusBadge(b.status)}</td>
                <td class="text-right">
                    ${actionHtml}
                </td>
            </tr>
        `;
    }).join("");
}

async function cancelUserBooking(id) {
    if (!confirm(`Bạn có chắc muốn hủy đơn đặt vé #${id}?`)) return;
    try {
        const booking = cachedBookings.find(b => b.bookingId === id);
        if (!booking) return;
        const payload = { ...booking, status: "Cancelled" };
        await apiFetch(`/Bookings/${id}`, { method: "PUT", body: payload });
        showToast(`Đã hủy đơn đặt vé #${id} thành công!`, "info");
        await loadBookings();
    } catch (err) {
        showToast(err.message, "error");
    }
}

function openBookingModal(booking = null) {
    document.getElementById("bookingForm").reset();
    const isU = isUser();
    const currentUser = getCurrentUser();

    const userSelect = document.getElementById("bookingUserSelect");
    if (isU && currentUser) {
        if (userSelect) {
            userSelect.innerHTML = `<option value="${currentUser.userId}" selected>${escapeHtml(currentUser.fullName || currentUser.username)} (${currentUser.email || ''})</option>`;
            userSelect.disabled = true;
        }
    } else {
        if (userSelect) userSelect.disabled = false;
        populateUsersDropdown(booking?.userId);
    }

    const title = document.getElementById("bookingModalTitle");
    const idInput = document.getElementById("bookingFormId");

    if (booking) {
        title.textContent = `Sửa đơn đặt vé #${booking.bookingId}`;
        idInput.value = booking.bookingId;
        document.getElementById("bookingCode").value = booking.bookingCode || "";
        document.getElementById("bookingDate").value = toLocalDatetimeInput(booking.bookingDate);
        document.getElementById("bookingTotalAmount").value = booking.totalAmount || 0;
        document.getElementById("bookingStatus").value = booking.status || "Pending";
    } else {
        title.textContent = "Tạo đơn đặt vé mới";
        idInput.value = "";
        const randomCode = "BK-" + new Date().getFullYear() + ("0" + (new Date().getMonth() + 1)).slice(-2) + "-" + Math.floor(1000 + Math.random() * 9000);
        document.getElementById("bookingCode").value = randomCode;
        document.getElementById("bookingDate").value = toLocalDatetimeInput(new Date().toISOString());
        document.getElementById("bookingStatus").value = "Pending";
        document.getElementById("bookingTotalAmount").value = 0;
    }

    openModal("bookingModal");
}

function editBooking(id) {
    const b = cachedBookings.find(item => item.bookingId === id);
    if (b) openBookingModal(b);
}

async function handleSaveBooking(e) {
    e.preventDefault();
    const submitBtn = document.getElementById("bookingSubmitBtn");
    const id = document.getElementById("bookingFormId").value;
    const isEdit = Boolean(id);

    const bookingCode = document.getElementById("bookingCode").value.trim();
    const userSelect = document.getElementById("bookingUserSelect");
    const userId = Number(userSelect?.value) || (isUser() ? getCurrentUser()?.userId : 0);
    const bookingDate = document.getElementById("bookingDate").value;
    const totalAmount = Number(document.getElementById("bookingTotalAmount").value);
    const status = document.getElementById("bookingStatus").value;

    if (!bookingCode || !userId || totalAmount < 0) {
        showToast("Vui lòng điền đầy đủ thông tin đơn đặt vé!", "warning");
        return;
    }

    const payload = {
        bookingId: isEdit ? Number(id) : 0,
        bookingCode,
        userId,
        bookingDate: bookingDate ? new Date(bookingDate).toISOString() : new Date().toISOString(),
        totalAmount,
        status,
        isDeleted: false,
        createdAt: new Date().toISOString()
    };

    submitBtn.disabled = true;
    submitBtn.textContent = "Đang lưu...";

    try {
        if (isEdit) {
            await apiFetch(`/Bookings/${id}`, { method: "PUT", body: payload });
            showToast(`Cập nhật đơn đặt vé #${id} thành công!`, "success");
        } else {
            await apiFetch("/Bookings", { method: "POST", body: payload });
            showToast("Tạo đơn đặt vé mới thành công!", "success");
        }
        closeModal("bookingModal");
        await loadBookings();
    } catch (err) {
        showToast(err.message, "error");
    } finally {
        submitBtn.disabled = false;
        submitBtn.textContent = "Lưu đơn đặt vé";
    }
}

async function deleteBooking(id, code) {
    if (!isAdmin()) {
        showToast("Lỗi 403: Chỉ tài khoản Quản trị viên (Admin) mới có quyền xóa đơn đặt vé!", "error");
        return;
    }
    if (!confirm(`Bạn có chắc muốn xóa đơn đặt vé "${code}" (ID: ${id})?`)) return;

    try {
        await apiFetch(`/Bookings/${id}`, { method: "DELETE" });
        showToast(`Đã xóa đơn đặt vé #${id} thành công!`, "success");
        await loadBookings();
    } catch (err) {
        showToast(err.message, "error");
    }
}

// =========================================================
// 11. QUẢN LÝ THANH TOÁN (PAYMENTS)
// =========================================================
async function loadPayments() {
    const tbody = document.getElementById("paymentTableBody");
    tbody.innerHTML = `<tr><td colspan="8" class="text-center text-muted">Đang tải lịch sử thanh toán...</td></tr>`;

    try {
        await populateBookingsDropdown();
        const data = await apiFetch("/Payments");
        cachedPayments = Array.isArray(data) ? data : [];
        filterPayments();
    } catch (err) {
        tbody.innerHTML = `<tr><td colspan="8" class="text-center text-danger">Lỗi tải thanh toán: ${escapeHtml(err.message)}</td></tr>`;
    }
}

function filterPayments() {
    const search = document.getElementById("paymentSearchInput")?.value.toLowerCase().trim() || "";
    const status = document.getElementById("paymentStatusFilter")?.value || "";

    const filtered = cachedPayments.filter(p => {
        const matchesSearch = String(p.paymentId).includes(search) ||
            String(p.bookingId).includes(search) ||
            (p.paymentCode || "").toLowerCase().includes(search) ||
            (p.transactionCode || "").toLowerCase().includes(search);
        const matchesStatus = !status || (p.status || "").toLowerCase() === status.toLowerCase();
        return matchesSearch && matchesStatus;
    });

    renderPaymentsTable(filtered);
}

function renderPaymentsTable(payments) {
    const tbody = document.getElementById("paymentTableBody");
    if (!tbody) return;

    if (payments.length === 0) {
        tbody.innerHTML = `<tr><td colspan="8" class="text-center text-muted">Chưa có giao dịch thanh toán nào.</td></tr>`;
        return;
    }

    const isAdm = isAdmin();

    tbody.innerHTML = payments.map(p => `
        <tr>
            <td><strong>#${p.paymentId}</strong></td>
            <td><code>${escapeHtml(p.paymentCode || 'N/A')}</code></td>
            <td><span class="badge badge-role-manager">Đơn #${p.bookingId}</span></td>
            <td><strong class="text-primary">${formatCurrency(p.amount)}</strong></td>
            <td>${escapeHtml(p.paymentMethod || 'Tiền mặt')}</td>
            <td>${formatDateTime(p.paymentDate || p.createdAt)}</td>
            <td>${renderStatusBadge(p.status)}</td>
            <td class="text-right">
                <div class="action-buttons">
                    <button class="btn btn-secondary btn-sm" onclick="editPayment(${p.paymentId})">✏️ Sửa</button>
                    ${isAdm ? `<button class="btn btn-danger btn-sm" onclick="deletePayment(${p.paymentId}, '${escapeHtml(p.paymentCode || '#' + p.paymentId)}')">🗑️ Xóa</button>` : ''}
                </div>
            </td>
        </tr>
    `).join("");
}

function openPaymentModal(payment = null) {
    if (!hasRole("Admin", "Manager")) {
        showToast("Lỗi 403: Chỉ Admin hoặc Manager mới có quyền tạo giao dịch thanh toán!", "error");
        return;
    }
    document.getElementById("paymentForm").reset();
    populateBookingsDropdown(payment?.bookingId);

    const title = document.getElementById("paymentModalTitle");
    const idInput = document.getElementById("paymentFormId");

    if (payment) {
        title.textContent = `Sửa giao dịch thanh toán #${payment.paymentId}`;
        idInput.value = payment.paymentId;
        document.getElementById("paymentCode").value = payment.paymentCode || "";
        document.getElementById("paymentAmount").value = payment.amount || 0;
        document.getElementById("paymentMethod").value = payment.paymentMethod || "Tiền mặt";
        document.getElementById("paymentStatus").value = payment.status || "Success";
        document.getElementById("paymentTransactionCode").value = payment.transactionCode || "";
    } else {
        title.textContent = "Thêm giao dịch thanh toán";
        idInput.value = "";
        const randomCode = "PAY-" + new Date().getFullYear() + "-" + Math.floor(10000 + Math.random() * 90000);
        document.getElementById("paymentCode").value = randomCode;
        document.getElementById("paymentStatus").value = "Success";
    }

    openModal("paymentModal");
}

function editPayment(id) {
    const p = cachedPayments.find(item => item.paymentId === id);
    if (p) openPaymentModal(p);
}

async function handleSavePayment(e) {
    e.preventDefault();
    const submitBtn = document.getElementById("paymentSubmitBtn");
    const id = document.getElementById("paymentFormId").value;
    const isEdit = Boolean(id);

    const bookingId = Number(document.getElementById("paymentBookingSelect").value);
    const paymentCode = document.getElementById("paymentCode").value.trim();
    const amount = Number(document.getElementById("paymentAmount").value);
    const paymentMethod = document.getElementById("paymentMethod").value;
    const status = document.getElementById("paymentStatus").value;
    const transactionCode = document.getElementById("paymentTransactionCode").value.trim();

    if (!bookingId || !paymentCode || amount < 0) {
        showToast("Vui lòng điền đúng thông tin thanh toán!", "warning");
        return;
    }

    const payload = {
        paymentId: isEdit ? Number(id) : 0,
        bookingId,
        paymentCode,
        amount,
        paymentMethod,
        paymentDate: new Date().toISOString(),
        status,
        transactionCode,
        createdAt: new Date().toISOString()
    };

    submitBtn.disabled = true;
    submitBtn.textContent = "Đang lưu...";

    try {
        if (isEdit) {
            await apiFetch(`/Payments/${id}`, { method: "PUT", body: payload });
            showToast(`Cập nhật thanh toán #${id} thành công!`, "success");
        } else {
            await apiFetch("/Payments", { method: "POST", body: payload });
            showToast("Thêm giao dịch thanh toán thành công!", "success");
        }
        closeModal("paymentModal");
        await loadPayments();
    } catch (err) {
        showToast(err.message, "error");
    } finally {
        submitBtn.disabled = false;
        submitBtn.textContent = "Lưu thanh toán";
    }
}

async function deletePayment(id, code) {
    if (!isAdmin()) {
        showToast("Lỗi 403: Chỉ tài khoản Quản trị viên (Admin) mới có quyền xóa giao dịch thanh toán!", "error");
        return;
    }
    if (!confirm(`Bạn có chắc muốn xóa giao dịch thanh toán "${code}" (ID: ${id})?`)) return;

    try {
        await apiFetch(`/Payments/${id}`, { method: "DELETE" });
        showToast(`Đã xóa thanh toán #${id} thành công!`, "success");
        await loadPayments();
    } catch (err) {
        showToast(err.message, "error");
    }
}

// =========================================================
// 12. QUẢN LÝ VÉ & CHECK-IN (TICKETS & CHECK-IN)
// =========================================================
let cachedBookingDetails = [];

async function loadTickets() {
    const tbody = document.getElementById("ticketTableBody");
    tbody.innerHTML = `<tr><td colspan="7" class="text-center text-muted">Đang tải danh sách vé...</td></tr>`;

    try {
        // Tải vé và BookingDetails song song
        const [ticketData, bdData] = await Promise.allSettled([
            apiFetch("/Tickets"),
            apiFetch("/BookingDetails")
        ]);
        cachedTickets = ticketData.status === "fulfilled" && Array.isArray(ticketData.value) ? ticketData.value : [];
        cachedBookingDetails = bdData.status === "fulfilled" && Array.isArray(bdData.value) ? bdData.value : [];

        // Nếu chưa có cachedBookings thì tải luôn (cần cho filter User)
        if (!cachedBookings || cachedBookings.length === 0) {
            try {
                cachedBookings = await apiFetch("/Bookings");
            } catch { /* bỏ qua */ }
        }

        filterTickets();
    } catch (err) {
        tbody.innerHTML = `<tr><td colspan="7" class="text-center text-danger">Lỗi tải danh sách vé: ${escapeHtml(err.message)}</td></tr>`;
    }
}

function filterTickets() {
    const search = document.getElementById("ticketSearchInput")?.value.toLowerCase().trim() || "";
    const role = getCurrentRole();
    const currentUser = getCurrentUser();

    let list = cachedTickets;
    if (role === "User" && currentUser) {
        // Lấy danh sách bookingId của user
        const userBookingIds = (cachedBookings || []).filter(b => b.userId === currentUser.userId).map(b => b.bookingId);
        // Lấy danh sách bookingDetailId thuộc các booking của user
        const userBDIds = (cachedBookingDetails || []).filter(bd => userBookingIds.includes(bd.bookingId)).map(bd => bd.bookingDetailId);

        list = list.filter(t =>
            // Match qua email
            (t.attendeeEmail && currentUser.email && t.attendeeEmail.toLowerCase() === currentUser.email.toLowerCase()) ||
            // Match qua bookingDetailId → BookingDetails → Booking của user
            userBDIds.includes(t.bookingDetailId) ||
            // Match qua tên
            (t.attendeeName && currentUser.fullName && t.attendeeName.toLowerCase() === currentUser.fullName.toLowerCase())
        );
    }

    const filtered = list.filter(t =>
        String(t.ticketId).includes(search) ||
        (t.ticketCode || "").toLowerCase().includes(search) ||
        (t.attendeeName || "").toLowerCase().includes(search) ||
        (t.attendeeEmail || "").toLowerCase().includes(search)
    );
    renderTicketsTable(filtered);
}

function renderTicketsTable(tickets) {
    const tbody = document.getElementById("ticketTableBody");
    if (!tbody) return;

    const role = getCurrentRole();

    if (tickets.length === 0) {
        tbody.innerHTML = `<tr><td colspan="7" class="text-center text-muted">${role === 'User' ? 'Bạn chưa có vé tham dự nào. Hãy đặt vé tại trang Sự kiện!' : 'Chưa có vé nào được phát hành.'}</td></tr>`;
        return;
    }

    tbody.innerHTML = tickets.map(t => {
        let checkInBadge = '';
        if (role === 'User') {
            checkInBadge = t.checkedIn
                ? `<span class="badge badge-open">✓ Đã check-in (${formatDateTime(t.checkedInAt)})</span>`
                : `<span class="badge badge-upcoming">Chưa điểm danh</span>`;
        } else {
            // Admin hoặc Manager có quyền điểm danh khách
            checkInBadge = t.checkedIn
                ? `<span class="badge badge-open">✓ Đã check-in (${formatDateTime(t.checkedInAt)})</span>`
                : `<button class="btn btn-primary btn-sm" onclick="handleCheckIn(${t.ticketId})">👉 Điểm danh</button>`;
        }

        let actionHtml = '';
        if (role === 'Admin') {
            actionHtml = `
                <div class="action-buttons">
                    <button class="btn btn-secondary btn-sm" onclick="showTicketDetailModal(${t.ticketId})" title="Xem chi tiết vé">👁️</button>
                    <button class="btn btn-danger btn-sm" onclick="deleteTicket(${t.ticketId})" title="Xóa vé">🗑️ Xóa</button>
                </div>
            `;
        } else if (role === 'Manager') {
            actionHtml = `
                <div class="action-buttons">
                    <button class="btn btn-secondary btn-sm" onclick="showTicketDetailModal(${t.ticketId})" title="Xem chi tiết vé">👁️ Xem</button>
                </div>
            `;
        } else {
            // User
            actionHtml = `
                <div class="action-buttons">
                    <button class="btn btn-secondary btn-sm" onclick="showTicketDetailModal(${t.ticketId})">🎫 Xem mã vé</button>
                </div>
            `;
        }

        return `
            <tr>
                <td><strong>#${t.ticketId}</strong></td>
                <td><code>${escapeHtml(t.ticketCode || 'N/A')}</code></td>
                <td>${escapeHtml(((cachedEvents || []).find(e => e.eventId === t.eventId) || {}).eventName || 'Sự kiện #' + t.eventId)}</td>
                <td><strong>${escapeHtml(t.attendeeName || 'Khách vãng lai')}</strong></td>
                <td>${escapeHtml(t.attendeeEmail || 'N/A')}</td>
                <td>${checkInBadge}</td>
                <td class="text-right">
                    ${actionHtml}
                </td>
            </tr>
        `;
    }).join("");
}

async function deleteTicket(id) {
    if (!isAdmin()) {
        showToast("Lỗi 403: Chỉ tài khoản Quản trị viên (Admin) mới có quyền xóa vé!", "error");
        return;
    }
    if (!confirm(`Bạn có chắc muốn xóa vé #${id}?`)) return;

    try {
        await apiFetch(`/Tickets/${id}`, { method: "DELETE" });
        showToast(`Đã xóa vé #${id} thành công!`, "success");
        await loadTickets();
    } catch (err) {
        showToast(err.message, "error");
    }
}

// Điểm danh (Check-In) — Admin/Manager bấm nút để check-in khách
async function handleCheckIn(ticketId) {
    if (!hasRole("Admin", "Manager")) {
        showToast("Lỗi 403: Chỉ Admin hoặc Manager mới có quyền điểm danh!", "error");
        return;
    }

    const ticket = (cachedTickets || []).find(t => t.ticketId === ticketId);
    if (!ticket) {
        showToast("Không tìm thấy vé!", "error");
        return;
    }

    if (ticket.checkedIn) {
        showToast("Vé này đã được điểm danh rồi!", "warning");
        return;
    }

    if (!confirm(`Xác nhận điểm danh (check-in) vé #${ticketId} — Khách: ${ticket.attendeeName || 'N/A'}?`)) return;

    try {
        const currentUser = getCurrentUser();

        // 1. Tạo bản ghi CheckIn qua POST /api/CheckIns
        await apiFetch("/CheckIns", {
            method: "POST",
            body: {
                checkInId: 0,
                ticketId: ticketId,
                checkInTime: new Date().toISOString(),
                checkedInBy: currentUser?.userId || 0,
                status: "Success"
            }
        });

        // 2. Cập nhật trạng thái vé thành đã check-in qua PUT /api/Tickets/{id}
        const updatedTicket = {
            ...ticket,
            checkedIn: true,
            checkedInAt: new Date().toISOString(),
            status: "Used"
        };
        await apiFetch(`/Tickets/${ticketId}`, { method: "PUT", body: updatedTicket });

        showToast(`✅ Điểm danh thành công vé #${ticketId} — ${ticket.attendeeName || 'Khách'}!`, "success");
        await loadTickets();
    } catch (err) {
        console.error("Lỗi khi check-in:", err);
        showToast(err.message || "Điểm danh thất bại!", "error");
    }
}

// =========================================================
// 13. QUẢN LÝ NGƯỜI DÙNG (USERS)
// =========================================================
async function loadUsers() {
    if (!isAdmin()) {
        showToast("Lỗi 403: Bạn không có quyền truy cập quản lý người dùng!", "error");
        showPage("dashboard");
        return;
    }

    const tbody = document.getElementById("userTableBody");
    tbody.innerHTML = `<tr><td colspan="8" class="text-center text-muted">Đang tải danh sách người dùng...</td></tr>`;

    try {
        const data = await apiFetch("/Users");
        cachedUsers = Array.isArray(data) ? data : [];
        filterUsers();
    } catch (err) {
        tbody.innerHTML = `<tr><td colspan="8" class="text-center text-danger">Lỗi tải người dùng: ${escapeHtml(err.message)}</td></tr>`;
    }
}

function filterUsers() {
    const search = document.getElementById("userSearchInput")?.value.toLowerCase().trim() || "";
    const roleId = document.getElementById("userRoleFilter")?.value || "";

    const filtered = cachedUsers.filter(u => {
        const matchesSearch = String(u.userId).includes(search) ||
            (u.username || "").toLowerCase().includes(search) ||
            (u.fullName || "").toLowerCase().includes(search) ||
            (u.email || "").toLowerCase().includes(search);
        const matchesRole = !roleId || String(u.roleId) === String(roleId);
        return matchesSearch && matchesRole;
    });

    renderUsersTable(filtered);
}

function renderUsersTable(users) {
    const tbody = document.getElementById("userTableBody");
    if (!tbody) return;

    if (users.length === 0) {
        tbody.innerHTML = `<tr><td colspan="8" class="text-center text-muted">Không tìm thấy người dùng nào.</td></tr>`;
        return;
    }

    tbody.innerHTML = users.map(u => {
        const roleName = u.roleId === 1 ? "Admin" : u.roleId === 2 ? "Manager" : "User";
        const roleBadgeClass = u.roleId === 1 ? "badge-role-admin" : u.roleId === 2 ? "badge-role-manager" : "badge-role-user";
        const statusBadge = u.isActive
            ? `<span class="badge badge-active">Hoạt động</span>`
            : `<span class="badge badge-inactive">Bị khóa</span>`;

        return `
            <tr>
                <td><strong>#${u.userId}</strong></td>
                <td><code>${escapeHtml(u.username)}</code></td>
                <td><strong>${escapeHtml(u.fullName)}</strong></td>
                <td>${escapeHtml(u.email)}</td>
                <td>${escapeHtml(u.phone || 'N/A')}</td>
                <td><span class="badge ${roleBadgeClass}">${roleName}</span></td>
                <td>${statusBadge}</td>
                <td class="text-right">
                    <div class="action-buttons">
                        <button class="btn btn-secondary btn-sm" onclick="editUser(${u.userId})">✏️ Sửa</button>
                        <button class="btn btn-danger btn-sm" onclick="deleteUser(${u.userId}, '${escapeHtml(u.username)}')">🗑️ Xóa</button>
                    </div>
                </td>
            </tr>
        `;
    }).join("");
}

function openUserModal(user = null) {
    if (!isAdmin()) {
        showToast("Lỗi 403: Chỉ Quản trị viên (Admin) mới có quyền thêm hoặc sửa người dùng!", "error");
        return;
    }
    document.getElementById("userForm").reset();
    const title = document.getElementById("userModalTitle");
    const idInput = document.getElementById("userFormId");
    const usernameInput = document.getElementById("userUsername");

    if (user) {
        title.textContent = `Sửa người dùng #${user.userId}`;
        idInput.value = user.userId;
        usernameInput.value = user.username || "";
        usernameInput.readOnly = true;
        document.getElementById("userPassword").placeholder = "Để trống nếu không đổi mật khẩu";
        document.getElementById("userPassword").required = false;
        document.getElementById("userFullName").value = user.fullName || "";
        document.getElementById("userEmail").value = user.email || "";
        document.getElementById("userPhone").value = user.phone || "";
        document.getElementById("userRole").value = user.roleId || 3;
        document.getElementById("userIsActive").value = user.isActive ? "true" : "false";
    } else {
        title.textContent = "Thêm người dùng mới";
        idInput.value = "";
        usernameInput.readOnly = false;
        document.getElementById("userPassword").placeholder = "Nhập mật khẩu";
        document.getElementById("userPassword").required = true;
        document.getElementById("userRole").value = "3";
        document.getElementById("userIsActive").value = "true";
    }

    openModal("userModal");
}

function editUser(id) {
    const u = cachedUsers.find(item => item.userId === id);
    if (u) openUserModal(u);
}

async function handleSaveUser(e) {
    e.preventDefault();
    const submitBtn = document.getElementById("userSubmitBtn");
    const id = document.getElementById("userFormId").value;
    const isEdit = Boolean(id);

    const username = document.getElementById("userUsername").value.trim();
    const password = document.getElementById("userPassword").value.trim();
    const fullName = document.getElementById("userFullName").value.trim();
    const email = document.getElementById("userEmail").value.trim();
    const phone = document.getElementById("userPhone").value.trim();
    const roleId = Number(document.getElementById("userRole").value);
    const isActive = document.getElementById("userIsActive").value === "true";

    if (!username || !fullName || !email) {
        showToast("Vui lòng điền đầy đủ tài khoản, họ tên và email!", "warning");
        return;
    }
    if (!isEdit && !password) {
        showToast("Mật khẩu không được để trống khi tạo tài khoản mới!", "warning");
        return;
    }

    const existingUser = isEdit ? cachedUsers.find(u => u.userId == id) : null;

    const payload = {
        userId: isEdit ? Number(id) : 0,
        username,
        passwordHash: password ? password : (existingUser?.passwordHash || "123456"),
        fullName,
        email,
        phone,
        roleId,
        isActive,
        isDeleted: false,
        createdAt: isEdit ? (existingUser?.createdAt || new Date().toISOString()) : new Date().toISOString(),
        updatedAt: isEdit ? new Date().toISOString() : null
    };

    submitBtn.disabled = true;
    submitBtn.textContent = "Đang lưu...";

    try {
        if (isEdit) {
            await apiFetch(`/Users/${id}`, { method: "PUT", body: payload });
            showToast(`Cập nhật người dùng #${id} thành công!`, "success");
        } else {
            await apiFetch("/Users", { method: "POST", body: payload });
            showToast("Thêm người dùng mới thành công!", "success");
        }
        closeModal("userModal");
        await loadUsers();
    } catch (err) {
        showToast(err.message, "error");
    } finally {
        submitBtn.disabled = false;
        submitBtn.textContent = "Lưu người dùng";
    }
}

async function deleteUser(id, username) {
    if (!isAdmin()) {
        showToast("Lỗi 403: Chỉ Quản trị viên (Admin) mới có quyền xóa người dùng!", "error");
        return;
    }
    if (!confirm(`Bạn có chắc muốn xóa người dùng "${username}" (ID: ${id})?`)) return;

    try {
        await apiFetch(`/Users/${id}`, { method: "DELETE" });
        showToast(`Đã xóa người dùng #${id} thành công!`, "success");
        await loadUsers();
    } catch (err) {
        showToast(err.message, "error");
    }
}

// =========================================================
// 14. ĐIỀU HƯỚNG TRANG & GIAO DIỆN CHUNG
// =========================================================
let currentPage = "dashboard";

const PAGE_TITLES = {
    "dashboard": "Tổng quan hệ thống",
    "events": "Quản lý sự kiện",
    "categories": "Danh mục sự kiện",
    "venues": "Quản lý địa điểm",
    "ticket-types": "Quản lý loại vé",
    "bookings": "Quản lý đơn đặt vé",
    "payments": "Lịch sử thanh toán",
    "tickets": "Vé tham dự & Check-In",
    "users": "Quản lý người dùng"
};

function showPage(pageId) {
    const role = getCurrentRole();
    const allowedRoles = PAGE_PERMISSIONS[pageId] || ["Admin"];

    if (!allowedRoles.includes(role)) {
        if (role === "Guest") {
            showToast(`Vui lòng đăng nhập để truy cập trang "${PAGE_TITLES[pageId] || pageId}"!`, "warning");
            openLoginModal();
        } else {
            showToast(`Bạn không có quyền truy cập trang "${PAGE_TITLES[pageId] || pageId}"! Chức năng này chỉ dành cho: ${allowedRoles.join(', ')}`, "error");
        }
        return;
    }

    currentPage = pageId;

    // Cập nhật title
    const titleEl = document.getElementById("pageTitle");
    if (titleEl) {
        if (role === "User" && pageId === "bookings") {
            titleEl.textContent = "Đơn đặt vé của tôi";
        } else if (role === "User" && pageId === "tickets") {
            titleEl.textContent = "Vé tham dự của tôi";
        } else {
            titleEl.textContent = PAGE_TITLES[pageId] || "Quản trị";
        }
    }

    // Cập nhật menu active
    document.querySelectorAll(".nav-item").forEach(item => {
        item.classList.toggle("active", item.getAttribute("data-page") === pageId);
    });

    // Cập nhật view active
    document.querySelectorAll(".page-view").forEach(view => {
        view.classList.toggle("active", view.id === `view-${pageId}`);
    });

    // Cập nhật phân quyền hiển thị giao diện
    applyRolePermissions();

    // Tự động tải dữ liệu theo trang
    refreshCurrentPage();

    // Ẩn sidebar trên mobile nếu đang mở
    const sidebar = document.querySelector(".sidebar");
    if (sidebar && sidebar.classList.contains("open")) {
        sidebar.classList.remove("open");
    }
}

function refreshCurrentPage() {
    switch (currentPage) {
        case "dashboard":
            refreshDashboard();
            break;
        case "events":
            loadEvents();
            break;
        case "categories":
            loadCategories();
            break;
        case "venues":
            loadVenues();
            break;
        case "ticket-types":
            loadTicketTypes();
            break;
        case "bookings":
            loadBookings();
            break;
        case "payments":
            loadPayments();
            break;
        case "tickets":
            loadTickets();
            break;
        case "users":
            loadUsers();
            break;
    }
}

function toggleSidebar() {
    const sidebar = document.querySelector(".sidebar");
    if (sidebar) {
        sidebar.classList.toggle("open");
    }
}

function openModal(id) {
    const modal = document.getElementById(id);
    if (modal) {
        modal.hidden = false;
    }
}

function closeModal(id) {
    const modal = document.getElementById(id);
    if (modal) {
        modal.hidden = true;
    }
}

// Bấm Escape để đóng modal
window.addEventListener("keydown", (e) => {
    if (e.key === "Escape") {
        document.querySelectorAll(".modal-backdrop").forEach(m => m.hidden = true);
    }
});

// =========================================================
// 15. TOAST NOTIFICATIONS & TIỆN ÍCH ĐỊNH DẠNG
// =========================================================
function showToast(message, type = "info") {
    const container = document.getElementById("toastContainer");
    if (!container) return;

    const toast = document.createElement("div");
    toast.className = `toast toast-${type}`;

    const icon = type === "success" ? "✅" : type === "error" ? "❌" : type === "warning" ? "⚠️" : "ℹ️";
    toast.innerHTML = `<span>${icon}</span> <span>${escapeHtml(message)}</span>`;

    container.appendChild(toast);

    setTimeout(() => {
        toast.style.opacity = "0";
        toast.style.transform = "translateX(50px)";
        toast.style.transition = "all 0.3s ease";
        setTimeout(() => toast.remove(), 300);
    }, 4000);
}

function formatCurrency(amount) {
    const num = Number(amount) || 0;
    return num.toLocaleString("vi-VN") + " đ";
}

function formatDateTime(isoString) {
    if (!isoString) return "N/A";
    const date = new Date(isoString);
    if (isNaN(date.getTime())) return isoString;

    const day = ("0" + date.getDate()).slice(-2);
    const month = ("0" + (date.getMonth() + 1)).slice(-2);
    const year = date.getFullYear();
    const hours = ("0" + date.getHours()).slice(-2);
    const minutes = ("0" + date.getMinutes()).slice(-2);

    return `${hours}:${minutes} ${day}/${month}/${year}`;
}

function toLocalDatetimeInput(isoString) {
    if (!isoString) return "";
    const date = new Date(isoString);
    if (isNaN(date.getTime())) return "";

    const offset = date.getTimezoneOffset() * 60000;
    return new Date(date.getTime() - offset).toISOString().slice(0, 16);
}

function escapeHtml(str) {
    if (str === null || str === undefined) return "";
    return String(str)
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}

function renderStatusBadge(status) {
    const s = (status || "").toUpperCase();
    switch (s) {
        case "OPEN":
            return `<span class="badge badge-open">Đang mở bán</span>`;
        case "UPCOMING":
            return `<span class="badge badge-upcoming">Sắp diễn ra</span>`;
        case "CLOSED":
            return `<span class="badge badge-closed">Đã đóng</span>`;
        case "FINISHED":
            return `<span class="badge badge-finished">Đã kết thúc</span>`;
        case "CANCELLED":
            return `<span class="badge badge-cancelled">Đã hủy</span>`;
        case "CONFIRMED":
        case "COMPLETED":
        case "PAID":
            return `<span class="badge badge-confirmed">Đã thanh toán</span>`;
        case "SUCCESS":
            return `<span class="badge badge-confirmed">Thành công</span>`;
        case "PENDING":
            return `<span class="badge badge-pending">Chờ xử lý</span>`;
        case "FAILED":
            return `<span class="badge badge-failed">Thất bại</span>`;
        case "REFUNDED":
            return `<span class="badge badge-cancelled">Đã hoàn tiền</span>`;
        case "VALID":
            return `<span class="badge badge-open">Hợp lệ</span>`;
        case "USED":
            return `<span class="badge badge-closed">Đã sử dụng</span>`;
        default:
            return `<span class="badge badge-closed">${escapeHtml(status || 'N/A')}</span>`;
    }
}

// =========================================================
// 15. TÍNH NĂNG ĐẶT VÉ DÀNH CHO USER & CHI TIẾT VÉ
// =========================================================
let currentBookingEvent = null;

async function openUserBookingModal(eventId) {
    if (!isAuthenticated()) {
        showToast("Vui lòng đăng nhập để đặt vé sự kiện!", "warning");
        openLoginModal();
        return;
    }

    currentBookingEvent = cachedEvents.find(e => e.eventId === eventId);
    if (!currentBookingEvent) {
        showToast("Không tìm thấy thông tin sự kiện!", "error");
        return;
    }

    const currentUser = getCurrentUser();
    document.getElementById("ubEventId").value = eventId;
    document.getElementById("ubEventName").value = `${currentBookingEvent.eventName} (#${eventId})`;
    document.getElementById("ubAttendeeName").value = currentUser?.fullName || currentUser?.username || "";
    document.getElementById("ubAttendeeEmail").value = currentUser?.email || "";
    document.getElementById("ubAttendeePhone").value = currentUser?.phone || "";
    document.getElementById("ubQuantity").value = 1;

    // Tải các loại vé của sự kiện
    const select = document.getElementById("ubTicketTypeSelect");
    select.innerHTML = '<option value="">-- Đang tải các hạng vé --</option>';

    try {
        if (!cachedTicketTypes || cachedTicketTypes.length === 0) {
            cachedTicketTypes = await apiFetch("/TicketTypes");
        }
        const eventTicketTypes = (cachedTicketTypes || []).filter(t => t.eventId === eventId);
        if (eventTicketTypes.length === 0) {
            select.innerHTML = '<option value="">Không có hạng vé nào của sự kiện này</option>';
        } else {
            select.innerHTML = eventTicketTypes.map(t =>
                `<option value="${t.ticketTypeId}" data-price="${t.price}">${escapeHtml(t.ticketName)} - ${formatCurrency(t.price)} (Còn: ${(t.quantity || 0) - (t.soldQuantity || 0)})</option>`
            ).join("");
        }
    } catch (err) {
        select.innerHTML = '<option value="">Lỗi khi tải hạng vé</option>';
    }

    calculateUserBookingTotal();
    openModal("userBookingModal");
}

function calculateUserBookingTotal() {
    const select = document.getElementById("ubTicketTypeSelect");
    const quantity = Math.max(1, Number(document.getElementById("ubQuantity").value) || 1);
    const selectedOption = select.options[select.selectedIndex];
    const unitPrice = selectedOption ? Number(selectedOption.getAttribute("data-price")) || 0 : 0;
    const total = unitPrice * quantity;

    document.getElementById("ubUnitPrice").value = formatCurrency(unitPrice);
    document.getElementById("ubTotalAmount").textContent = formatCurrency(total);
}

async function handleUserBookingSubmit(e) {
    e.preventDefault();
    const currentUser = getCurrentUser();
    if (!currentUser) {
        showToast("Vui lòng đăng nhập để hoàn tất đặt vé!", "warning");
        openLoginModal();
        return;
    }

    const eventId = Number(document.getElementById("ubEventId").value);
    const select = document.getElementById("ubTicketTypeSelect");
    const ticketTypeId = Number(select.value);
    const quantity = Number(document.getElementById("ubQuantity").value);
    const attendeeName = document.getElementById("ubAttendeeName").value.trim();
    const attendeeEmail = document.getElementById("ubAttendeeEmail").value.trim();

    if (!ticketTypeId) {
        showToast("Vui lòng chọn hạng vé bạn muốn mua!", "warning");
        return;
    }

    const selectedOption = select.options[select.selectedIndex];
    const unitPrice = selectedOption ? Number(selectedOption.getAttribute("data-price")) || 0 : 0;
    const totalAmount = unitPrice * quantity;
    const randomCode = "BK-" + new Date().getFullYear() + Math.floor(100000 + Math.random() * 900000);

    const submitBtn = document.getElementById("ubSubmitBtn");
    submitBtn.disabled = true;
    submitBtn.textContent = "Đang xử lý...";

    try {
        // 1. Gửi request tạo đơn đặt vé
        const bookingPayload = {
            bookingId: 0,
            bookingCode: randomCode,
            userId: currentUser.userId,
            bookingDate: new Date().toISOString(),
            totalAmount: totalAmount,
            status: "Pending",
            createdAt: new Date().toISOString()
        };

        const newBooking = await apiFetch("/Bookings", { method: "POST", body: bookingPayload });
        const bookingId = newBooking?.bookingId || 0;

        // 2. Tạo chi tiết đơn đặt vé (BookingDetail) — bắt buộc vì Tickets.BookingDetailId FK tới BookingDetails
        let bookingDetailId = 0;
        try {
            const bdPayload = {
                bookingDetailId: 0,
                bookingId: bookingId,
                ticketTypeId: ticketTypeId,
                quantity: quantity,
                unitPrice: unitPrice,
                amount: totalAmount
            };
            const newBD = await apiFetch("/BookingDetails", { method: "POST", body: bdPayload });
            bookingDetailId = newBD?.bookingDetailId || 0;
        } catch (bdErr) {
            console.warn("Lỗi tạo BookingDetail:", bdErr);
        }

        // 3. Tạo vé tham dự (Ticket) — dùng bookingDetailId đúng từ BookingDetails
        try {
            const ticketCode = "TCK-" + Math.floor(10000000 + Math.random() * 90000000);
            await apiFetch("/Tickets", {
                method: "POST",
                body: {
                    ticketId: 0,
                    ticketCode: ticketCode,
                    bookingDetailId: bookingDetailId || bookingId,
                    eventId: eventId,
                    ticketTypeId: ticketTypeId,
                    attendeeName: attendeeName,
                    attendeeEmail: attendeeEmail,
                    qrCode: "QR-" + randomCode,
                    status: "Valid",
                    checkedIn: false,
                    isDeleted: false,
                    createdAt: new Date().toISOString()
                }
            });
        } catch (ticketErr) {
            console.warn("Lỗi tạo vé qua API Tickets:", ticketErr);
        }

        closeModal("userBookingModal");
        showToast(`Đặt vé thành công! Mã đơn: ${randomCode}. Đang chuyển đến thanh toán...`, "success");
        await loadBookings();
        showPage("bookings");

        // Tự động mở cửa sổ thanh toán để khách hàng bấm thanh toán ngay
        setTimeout(() => {
            if (bookingId) {
                openUserPaymentModal(bookingId);
            }
        }, 350);

    } catch (err) {
        console.error("Lỗi đặt vé:", err);
        showToast(err.message || "Đặt vé không thành công", "error");
    } finally {
        submitBtn.disabled = false;
        submitBtn.textContent = "Xác nhận đặt vé";
    }
}

function showTicketDetailModal(ticketId) {
    const ticket = cachedTickets.find(t => t.ticketId === ticketId);
    if (!ticket) {
        showToast("Không tìm thấy thông tin vé!", "error");
        return;
    }

    const event = (cachedEvents || []).find(e => e.eventId === ticket.eventId);
    document.getElementById("tdEventName").textContent = event ? event.eventName : `SỰ KIỆN #${ticket.eventId}`;
    document.getElementById("tdTicketCode").textContent = ticket.ticketCode || `TCK-${ticket.ticketId}`;
    document.getElementById("tdAttendeeInfo").innerHTML = `Khách tham dự: <strong>${escapeHtml(ticket.attendeeName || 'Khách vãng lai')}</strong> (${escapeHtml(ticket.attendeeEmail || 'N/A')})`;
    document.getElementById("tdStatusBadge").innerHTML = ticket.checkedIn
        ? `<span class="badge badge-open">✓ Đã check-in (${formatDateTime(ticket.checkedInAt)})</span>`
        : `<span class="badge badge-upcoming">Chưa điểm danh</span>`;

    openModal("ticketDetailModal");
}

function promptLoginForBooking(eventName) {
    showToast(`Vui lòng đăng nhập để đặt vé sự kiện "${eventName}"!`, "info");
    openLoginModal();
}

// =========================================================
// THANH TOÁN MÔ PHỎNG PHÍA NGƯỜI DÙNG (USER PAYMENT)
// =========================================================
async function openUserPaymentModal(bookingId) {
    let booking = (cachedBookings || []).find(b => b.bookingId === Number(bookingId));
    if (!booking) {
        try {
            booking = await apiFetch(`/Bookings/${bookingId}`);
        } catch (err) {
            showToast("Không tìm thấy thông tin đơn đặt vé để thanh toán!", "error");
            return;
        }
    }

    if (!booking) {
        showToast("Không tìm thấy thông tin đơn đặt vé!", "error");
        return;
    }

    const currentUser = getCurrentUser();
    document.getElementById("upBookingId").value = booking.bookingId;
    document.getElementById("upBookingCode").textContent = booking.bookingCode || `#${booking.bookingId}`;
    document.getElementById("upCustomerName").textContent = currentUser?.fullName || currentUser?.username || `User #${booking.userId}`;
    document.getElementById("upAmount").textContent = formatCurrency(booking.totalAmount);

    openModal("userPaymentModal");
}

async function executeUserPayment(e) {
    if (e && e.preventDefault) e.preventDefault();
    const bookingId = Number(document.getElementById("upBookingId").value);
    if (!bookingId) {
        showToast("Lỗi: Mã đơn không hợp lệ!", "error");
        return;
    }

    let booking = (cachedBookings || []).find(b => b.bookingId === bookingId);
    if (!booking) {
        try {
            booking = await apiFetch(`/Bookings/${bookingId}`);
        } catch {
            showToast("Không thể tải thông tin đơn hàng!", "error");
            return;
        }
    }

    const submitBtn = document.getElementById("upSubmitBtn");
    if (submitBtn) {
        submitBtn.disabled = true;
        submitBtn.textContent = "⏳ Đang xác nhận thanh toán...";
    }

    const methodInput = document.querySelector('input[name="upPaymentMethod"]:checked');
    const paymentMethod = methodInput ? methodInput.value : "Ví MoMo";
    const paymentCode = "PAY-" + new Date().getFullYear() + Math.floor(100000 + Math.random() * 900000);
    const txnCode = "TXN-" + Math.floor(10000000 + Math.random() * 90000000);

    try {
        // 1. Tạo bản ghi thanh toán qua POST /api/Payments (Status phải là Success/Pending/Failed/Refunded theo DB constraint)
        const paymentPayload = {
            paymentId: 0,
            bookingId: bookingId,
            paymentCode: paymentCode,
            amount: Number(booking.totalAmount) || 0,
            paymentMethod: paymentMethod,
            paymentDate: new Date().toISOString(),
            status: "Success",
            transactionCode: txnCode,
            createdAt: new Date().toISOString()
        };

        await apiFetch("/Payments", { method: "POST", body: paymentPayload });

        // 2. Cập nhật trạng thái Booking thành "Paid" qua PUT /api/Bookings/{id} (Status phải là Paid/Pending/Cancelled/Refunded)
        const updatedBooking = {
            ...booking,
            status: "Paid"
        };

        await apiFetch(`/Bookings/${bookingId}`, { method: "PUT", body: updatedBooking });

        closeModal("userPaymentModal");
        showToast(`🎉 Thanh toán thành công ${formatCurrency(booking.totalAmount)} qua ${paymentMethod}! Đơn vé đã được thanh toán hoàn tất.`, "success");

        // Cập nhật lại dữ liệu đơn đặt vé và tổng quan
        await loadBookings();
        if (currentPage === "dashboard") {
            await refreshDashboard();
        }

    } catch (err) {
        console.error("Lỗi khi thanh toán:", err);
        showToast(err.message || "Thanh toán không thành công", "error");
    } finally {
        if (submitBtn) {
            submitBtn.disabled = false;
            submitBtn.textContent = "✅ Thanh toán ngay";
        }
    }
}

// =========================================================
// 16. KHỞI CHẠY HỆ THỐNG KHI TẢI TRANG
// =========================================================
document.addEventListener("DOMContentLoaded", () => {
    updateGatewayIndicator();
    renderUserProfile();
    checkGatewayConnection();
    showPage("dashboard");
});
