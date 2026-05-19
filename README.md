# 🌐 Unity Multiplayer - Photon Fusion/PUN2 Project

![Unity](https://img.shields.io/badge/Unity-2022.3+-black?style=for-the-badge&logo=unity)
![Photon](https://img.shields.io/badge/Photon-Networking-blue?style=for-the-badge)
![Status](https://img.shields.io/badge/Status-Completed-green?style=for-the-badge)

Dự án nghiên cứu và triển khai cơ chế chơi mạng (Multiplayer) thời gian thực sử dụng giải pháp **Photon**. Tập trung vào việc giải quyết các bài toán đồng bộ hóa dữ liệu giữa các máy khách (Clients) và xây dựng hệ thống phòng chờ (Lobby).

---

## 📸 Demo Hình ảnh
*(Hãy chèn ảnh chụp màn hình lúc có 2-3 nhân vật cùng trong một phòng)*

| Network Synchronization | Lobby System |
| :---: | :---: |
| ![Sync](https://via.placeholder.com/400x225.png?text=Sync+Movement) | ![Lobby](https://via.placeholder.com/400x225.png?text=Room+Management) |

---

## ⚙️ Các tính năng kỹ thuật (Technical Features)

### 1. Quản lý kết nối (Connection Management)
* **Master Server Connection:** Xử lý logic kết nối tới Server của Photon, quản lý trạng thái Offline/Online.
* **Lobby & Room System:** Tạo phòng (Create Room), tham gia phòng có sẵn (Join Room) và liệt kê danh sách phòng đang hoạt động.
* **Player Customization:** Đồng bộ tên người chơi và màu sắc nhân vật thông qua **Custom Properties**.

### 2. Đồng bộ hóa thời gian thực (Real-time Synchronization)
* **Transform Synchronization:** Sử dụng `Photon View` và `Photon Transform View` để đồng bộ vị trí, vòng quay của nhân vật mượt mà giữa các máy.
* **Animation Sync:** Đồng bộ các trạng thái Animator (Run, Jump, Attack) thông qua `Photon Animator View`.
* **RPCs (Remote Procedure Calls):** Sử dụng RPC để gửi các sự kiện tức thời như bắn súng, gây sát thương hoặc kích hoạt hiệu ứng đặc biệt.

### 3. Logic Gameplay mạng
* **Ownership Transfer:** Xử lý quyền điều khiển đối tượng (Authority) khi người chơi tương tác với vật phẩm trong môi trường.
* **Latency Compensation:** Kỹ thuật nội suy (Interpolation) để giảm thiểu hiện tượng giật lag do độ trễ mạng.
* **Network Object Pooling:** Tối ưu hóa việc khởi tạo (Instantiate) và hủy (Destroy) các đối tượng mạng như đạn hoặc hiệu ứng.

---

## 🛠️ Công cụ & Thư viện
* **Engine:** Unity 2022.3+
* **Networking SDK:** Photon Unity Networking 2 (PUN2) / Photon Fusion (Tùy bản bạn dùng).
* **Language:** C# nâng cao (Xử lý các tiến trình bất đồng bộ).

---

## 🚀 Hướng dẫn chạy thử
1. **Clone Repo:**
   ```bash
   git clone [https://github.com/Ridotakarin/Multiplayer_Photon.git](https://github.com/Ridotakarin/Multiplayer_Photon.git)
