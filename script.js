
const API_URL = "https://localhost:7127/api/Events";

// Đăng nhập lại và dán JWT MỚI vào đây.
// Không thêm chữ "Bearer".
const JWT_TOKEN = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjEiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW4iLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkBnbWFpbC5jb20iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsImV4cCI6MTc5MjY3NzQ5MywiaXNzIjoiUXVhbkx5U3VLaWVuQVBJIiwiYXVkIjoiUXVhbkx5U3VLaWVuQ2xpZW50In0.MDpf9CB8N9lcBMoiQUGOVqIljQmtZECbBKdoAWi1IYQ ";

let events = [];
let editingEventId = null;

// ================================
// 1. HEADER DÙNG CHO API
// ================================
function getHeaders(includeJson = true) {
    const headers = {};

    if (includeJson) {
        headers["Content-Type"] = "application/json";
    }

    const token = JWT_TOKEN.trim();

    if (token && token !== "DAN_JWT_TOKEN_MOI_VAO_DAY") {
        headers["Authorization"] = `Bearer ${token}`;
    }

    return headers;
}

// Đọc phản hồi API, kể cả khi nội dung trả về không phải JSON
async function readResponse(response) {
    const text = await response.text();

    if (!text) return {};

    try {
        return JSON.parse(text);
    } catch {
        return { message: text };
    }
}

// ================================
// 2. TẢI DANH SÁCH SỰ KIỆN
// ================================
async function loadEvents() {
    const message = document.getElementById("message");

    if (message) {
        message.textContent = "Đang tải dữ liệu sự kiện...";
    }

    try {
        const response = await fetch(API_URL, {
            method: "GET",
            headers: getHeaders(false)
        });

        const result = await readResponse(response);

        if (!response.ok) {
            throw new Error(
                "HTTP " + response.status + ": " +
                (result.message || result.title || "")
            );
        }

        if (!Array.isArray(result)) {
            throw new Error("API không trả về danh sách sự kiện.");
        }

        // Giữ toàn bộ dữ liệu gốc để dùng khi sửa sự kiện.
        events = result.map(event => ({
            ...event,
            id: event.eventId,
            name: event.eventName || "",
            venue: "Địa điểm ID: " + event.venueId,
            status: event.status || "Chưa xác định"
        }));

        renderEvents();

        if (message) {
            message.textContent =
                "Đã tải " + events.length +
                " sự kiện từ Backend qua API Gateway.";
        }
    } catch (error) {
        console.error("Lỗi tải sự kiện:", error);

        if (message) {
            message.textContent =
                "Không tải được sự kiện: " + error.message;
        }
    }
}

// ================================
// 3. HIỂN THỊ VÀ TÌM KIẾM
// ================================
function getStatusText(status) {
    const statuses = {
        OPEN: "Đang mở",
        UPCOMING: "Sắp diễn ra",
        CLOSED: "Đã đóng",
        FINISHED: "Đã kết thúc",
        CANCELLED: "Đã hủy"
    };

    return statuses[status] || status || "Chưa xác định";
}

function getStatusClass(status) {
    const classes = {
        OPEN: "status-ongoing",
        UPCOMING: "status-upcoming",
        CLOSED: "status-completed",
        FINISHED: "status-completed",
        CANCELLED: "status-cancelled"
    };

    return classes[status] || "status-unknown";
}


function renderEvents() {
    const eventList = document.getElementById("eventList");
    const searchInput = document.getElementById("search");
    const count = document.getElementById("count");

    if (!eventList || !searchInput || !count) {
        return;
    }

   
const keyword = searchInput.value.trim().toLowerCase();

const filteredEvents = events.filter(event =>
    String(event.id ?? "").toLowerCase().includes(keyword) ||
    (event.name || "").toLowerCase().includes(keyword)
);


    eventList.innerHTML = "";

    filteredEvents.forEach(event => {
        const row = document.createElement("tr");

       // Cột mã sự kiện
const idCell = document.createElement("td");
idCell.textContent = event.id ?? "";
row.appendChild(idCell);

// Cột tên sự kiện
const nameCell = document.createElement("td");
nameCell.textContent = event.name ?? "";
row.appendChild(nameCell);

// Cột địa điểm
const venueCell = document.createElement("td");
venueCell.textContent = event.venue ?? "";
row.appendChild(venueCell);

// Cột trạng thái có màu
const statusCell = document.createElement("td");
const statusBadge = document.createElement("span");

statusBadge.className = `status-badge ${getStatusClass(event.status)}`;
statusBadge.textContent = getStatusText(event.status);

statusCell.appendChild(statusBadge);
row.appendChild(statusCell);
        const actionCell = document.createElement("td");
const editButton = document.createElement("button");
editButton.type = "button";
editButton.textContent = "Sửa";
editButton.addEventListener("click", () => editEvent(event.id));

actionCell.appendChild(editButton);

const deleteButton = document.createElement("button");
deleteButton.type = "button";
deleteButton.textContent = "Xóa";
deleteButton.style.marginLeft = "8px";
deleteButton.addEventListener("click", () => deleteEvent(event.id));

actionCell.appendChild(deleteButton);
        row.appendChild(actionCell);

        eventList.appendChild(row);
    });

    count.textContent = events.length;
}

// ================================
// 4. THÊM SỰ KIỆN
// ================================

function openEventForm(event = null) {
    const panel = document.getElementById("eventFormPanel");
    const form = document.getElementById("eventForm");

    if (!panel || !form) {
        alert("Không tìm thấy biểu mẫu sự kiện trong index.html.");
        return;
    }

    form.reset();
    editingEventId = event ? event.eventId : null;

    document.getElementById("eventFormTitle").textContent =
        event ? "Sửa sự kiện" : "Thêm sự kiện";

    document.getElementById("eventId").value =
        event ? event.eventId : "";

    document.getElementById("eventName").value =
        event?.eventName || "";

    document.getElementById("categoryId").value =
        event?.categoryId ?? 1;

    document.getElementById("venueId").value =
        event?.venueId ?? 1;

    document.getElementById("organizerId").value =
        event?.organizerId ?? 1;

    document.getElementById("startDate").value =
        toLocalInput(event?.startDate);

    document.getElementById("endDate").value =
        toLocalInput(event?.endDate);

    document.getElementById("description").value =
        event?.description || "";

    document.getElementById("eventStatus").value =
    event?.status || "UPCOMING";

    document.getElementById("imageUrl").value =
        event?.imageUrl || "";

    panel.hidden = false;
    panel.scrollIntoView({ behavior: "smooth", block: "start" });
}

function toLocalInput(value) {
    if (!value) return "";

    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return "";

    const offset = date.getTimezoneOffset() * 60000;

    return new Date(date.getTime() - offset)
        .toISOString()
        .slice(0, 16);
}

function addEvent() {
    openEventForm();
}

function editEvent(id) {
    const event = events.find(
        item => Number(item.eventId) === Number(id)
    );

    if (!event) {
        alert("Không tìm thấy sự kiện cần sửa.");
        return;
    }

    openEventForm(event);
}

async function saveEventForm(event) {
    event.preventDefault();

    const name = document.getElementById("eventName").value.trim();
    const categoryId = Number(document.getElementById("categoryId").value);
    const venueId = Number(document.getElementById("venueId").value);
    const organizerId = Number(document.getElementById("organizerId").value);
    const startValue = document.getElementById("startDate").value;
    const endValue = document.getElementById("endDate").value;

    if (!name) {
        alert("Tên sự kiện không được để trống.");
        return;
    }

    if (
        !Number.isInteger(categoryId) || categoryId <= 0 ||
        !Number.isInteger(venueId) || venueId <= 0 ||
        !Number.isInteger(organizerId) || organizerId <= 0
    ) {
        alert("Mã danh mục, địa điểm và người tổ chức phải là số nguyên dương.");
        return;
    }

    if (
        !startValue || !endValue ||
        Number.isNaN(Date.parse(startValue)) ||
        Number.isNaN(Date.parse(endValue))
    ) {
        alert("Vui lòng nhập ngày bắt đầu và kết thúc hợp lệ.");
        return;
    }

    if (new Date(endValue) <= new Date(startValue)) {
        alert("Ngày kết thúc phải sau ngày bắt đầu.");
        return;
    }

    const existingEvent = editingEventId !== null
        ? events.find(item => Number(item.eventId) === Number(editingEventId))
        : null;

    if (editingEventId !== null && !existingEvent) {
        alert("Không tìm thấy dữ liệu sự kiện cần sửa. Hãy tải lại trang.");
        return;
    }

    const body = {
        ...(existingEvent || {}),
        eventId: existingEvent ? existingEvent.eventId : 0,
        eventName: name,
        categoryId,
        venueId,
        description: document.getElementById("description").value.trim(),
        startDate: new Date(startValue).toISOString(),
        endDate: new Date(endValue).toISOString(),
        organizerId,
        status: document.getElementById("eventStatus").value,
        imageUrl: document.getElementById("imageUrl").value.trim(),
        isDeleted: existingEvent?.isDeleted ?? false,
        createdAt: existingEvent?.createdAt ?? new Date().toISOString(),
        updatedAt: existingEvent ? new Date().toISOString() : null
    };

    const isEditing = editingEventId !== null;
    const url = isEditing
        ? `${API_URL}/${encodeURIComponent(editingEventId)}`
        : API_URL;

    try {
        const response = await fetch(url, {
            method: isEditing ? "PUT" : "POST",
            headers: getHeaders(true),
            body: JSON.stringify(body)
        });

        const result = await readResponse(response);

        if (!response.ok) {
            throw new Error(
                `HTTP ${response.status}: ` +
                (result.message || result.title || JSON.stringify(result))
            );
        }

        alert(isEditing
            ? "Cập nhật sự kiện thành công!"
            : `Thêm sự kiện thành công! ID: ${result.eventId ?? ""}`);

        document.getElementById("eventFormPanel").hidden = true;
        document.getElementById("eventForm").reset();
        editingEventId = null;

        await loadEvents();
    } catch (error) {
        console.error("Lỗi lưu sự kiện:", error);
        alert("Không lưu được sự kiện: " + error.message);
    }
}


// ================================
// 6. CHUYỂN MỤC TRÊN MENU
// ================================
function showPage(pageName) {
    const pageTitle = document.getElementById("pageTitle");
    const message = document.getElementById("message");

    if (pageTitle) {
        pageTitle.textContent = pageName;
    }

    document.querySelectorAll(".sidebar button").forEach(button => {
        button.classList.toggle(
            "active",
            button.textContent.includes(pageName)
        );
    });

    if (pageName === "Tổng quan" || pageName === "Sự kiện") {
        loadEvents();
    } else if (message) {
        message.textContent =
            "Mục " + pageName +
            " sẽ được kết nối API ở bước tiếp theo.";
    }
}

// ================================
// 7. KHỞI TẠO GIAO DIỆN
// ================================
document.addEventListener("DOMContentLoaded", () => {
    loadEvents();

    const searchInput = document.getElementById("search");

    if (searchInput) {
        searchInput.addEventListener("input", renderEvents);
    }
    const eventForm = document.getElementById("eventForm");

    if (eventForm) {
        eventForm.addEventListener("submit", saveEventForm);
    }

    const cancelButton = document.getElementById("cancelEventForm");

    if (cancelButton) {
        cancelButton.addEventListener("click", () => {
            document.getElementById("eventFormPanel").hidden = true;
            document.getElementById("eventForm").reset();
            editingEventId = null;
        });
    }});

async function deleteEvent(id) {
    const event = events.find(item => Number(item.id) === Number(id));

    if (!event) {
        alert("Không tìm thấy sự kiện cần xóa.");
        return;
    }

    const confirmed = confirm(
        `CẢNH BÁO: Bạn sắp xóa vĩnh viễn sự kiện ID ${id}:\n` +
        `${event.eventName || event.name}\n\n` +
        "Dữ liệu có thể không khôi phục được. Bạn có chắc chắn muốn xóa?"
    );

    if (!confirmed) {
        return;
    }

    try {
        const response = await fetch(
            `${API_URL}/${encodeURIComponent(id)}`,
            {
                method: "DELETE",
                headers: getHeaders(false)
            }
        );

        const result = await response.text();

        if (!response.ok) {
            console.error("Lỗi DELETE:", response.status, result);

            if (response.status === 401) {
                alert("Lỗi 401: JWT đã hết hạn hoặc không hợp lệ. Hãy đăng nhập lại.");
            } else if (response.status === 403) {
                alert("Lỗi 403: Tài khoản không có quyền xóa sự kiện.");
            } else if (response.status === 404) {
                alert("Không tìm thấy sự kiện cần xóa.");
            } else {
                alert(`Xóa thất bại (HTTP ${response.status}): ${result}`);
            }

            return;
        }

        alert(`Đã gửi yêu cầu xóa sự kiện ID ${id} thành công.`);
        await loadEvents();

    } catch (error) {
        console.error("Lỗi gửi yêu cầu DELETE:", error);
        alert("Không kết nối được API Gateway: " + error.message);
    }
}
