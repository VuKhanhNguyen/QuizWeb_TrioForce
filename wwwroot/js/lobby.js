const connection = new signalR.HubConnectionBuilder()
    .withUrl("/duelHub")
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();

// Khi Player 2 join match (từ database, thông qua HTTP POST)
// và sau đó connect vào SignalR group
connection.on("PlayerConnected", (data) => {
    console.log("Player connected to group:", data);
    if (!data.isPlayer1) {
        // Player 2 đã kết nối vào group
        updatePlayer2UI(data.userName, data.userName);
    }
});

// Khi nhận được thông tin match state sau khi join group
connection.on("JoinedMatchGroup", (data) => {
    console.log("Joined match group:", data);
    if (data.hasBothPlayers && data.player2UserName) {
        updatePlayer2UI(data.player2UserName, data.player2FullName);
    }
});

// Fallback: PlayerJoined event
connection.on("PlayerJoined", (data) => {
    console.log("Player joined:", data);
    updatePlayer2UI(data.player2UserName, data.player2FullName);
});

connection.on("MatchStarted", (data) => {
    console.log("Match started:", data);
    window.location.href = `/Duel/Play?matchId=${matchId}`;
});

connection.on("PlayerLeft", (data) => {
    Toastify({ text: "Đối thủ đã rời phòng", backgroundColor: "#dc3545" }).showToast();
    setTimeout(() => window.location.reload(), 1500);
});

connection.on("Error", (msg) => {
    console.error("SignalR Error:", msg);
    Toastify({ text: msg, backgroundColor: "#dc3545" }).showToast();
});

// Start connection and join match group
connection.start().then(() => {
    console.log("SignalR connected, joining match group...");
    // Use JoinMatchGroup instead of JoinRoom
    connection.invoke("JoinMatchGroup", matchId).catch(err => {
        console.error("JoinMatchGroup error:", err);
    });
}).catch(err => console.error("SignalR connection error:", err));

function updatePlayer2UI(userName, fullName) {
    const displayName = fullName || userName;
    document.getElementById('player2Slot').innerHTML = `
            <div class="player-avatar">${displayName?.charAt(0) || 'P'}</div>
            <div class="player-info">
                <div class="player-name">${displayName}</div>
                <div class="player-status">${isPlayer1 ? 'Đối thủ' : 'Bạn'}</div>
            </div>
            <i class="fa-solid fa-check-circle fa-lg"></i>
        `;
    document.getElementById('player2Slot').className = 'player-slot ready';
    document.getElementById('waitingAnim').style.display = 'none';

    if (isPlayer1) {
        const btnStart = document.getElementById('btnStart');
        if (btnStart) btnStart.disabled = false;
    }

    Toastify({ text: `${displayName} đã tham gia!`, backgroundColor: "#28a745" }).showToast();
}

// Copy match code
document.getElementById('matchCode').addEventListener('click', () => {
    navigator.clipboard.writeText('@Model.MatchCode');
    Toastify({ text: "Đã sao chép mã phòng!", backgroundColor: "#667eea" }).showToast();
});

// Start match (only for Player1)
if (isPlayer1) {
    document.getElementById('btnStart').addEventListener('click', () => {
        console.log("Starting match...");
        connection.invoke("StartMatch", matchId).catch(err => {
            console.error("StartMatch error:", err);
            Toastify({ text: "Có lỗi xảy ra khi bắt đầu", backgroundColor: "#dc3545" }).showToast();
        });
    });
}