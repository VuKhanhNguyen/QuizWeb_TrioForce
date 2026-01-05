const connection = new signalR.HubConnectionBuilder()
    .withUrl("/duelHub")
    .withAutomaticReconnect()
    .build();

connection.start().catch(err => console.error(err));

// Tạo phòng
document.getElementById('btnCreateRoom').addEventListener('click', async () => {
    const qSetId = document.getElementById('questionSetSelect').value;

    try {
        const response = await fetch('/Duel/CreateRoom', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: `qSetId=${qSetId}`
        });
        const data = await response.json();

        if (data.success) {
            window.location.href = `/Duel/Lobby?matchId=${data.matchId}`;
        } else {
            Toastify({ text: data.message, backgroundColor: "#dc3545" }).showToast();
        }
    } catch (err) {
        console.error(err);
        Toastify({ text: "Có lỗi xảy ra", backgroundColor: "#dc3545" }).showToast();
    }
});

// Tham gia phòng
document.getElementById('btnJoinRoom').addEventListener('click', async () => {
    const matchCode = document.getElementById('matchCodeInput').value.trim().toUpperCase();
    if (matchCode.length !== 6) {
        Toastify({ text: "Mã phòng phải có 6 ký tự", backgroundColor: "#ffc107" }).showToast();
        return;
    }

    try {
        const response = await fetch('/Duel/JoinRoom', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: `matchCode=${matchCode}`
        });
        const data = await response.json();

        if (data.success) {
            window.location.href = `/Duel/Lobby?matchId=${data.matchId}`;
        } else {
            Toastify({ text: data.message, backgroundColor: "#dc3545" }).showToast();
        }
    } catch (err) {
        console.error(err);
        Toastify({ text: "Có lỗi xảy ra", backgroundColor: "#dc3545" }).showToast();
    }
});

// Auto uppercase for match code
document.getElementById('matchCodeInput').addEventListener('input', function () {
    this.value = this.value.toUpperCase();
});