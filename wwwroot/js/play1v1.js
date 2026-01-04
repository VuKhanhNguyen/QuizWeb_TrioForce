const matchId = @Model.MatchId;
const isPlayer1 = @(isPlayer1 ? "true" : "false");
let currentQuestionId = 0;
let timerInterval = null;
let timeLeft = 10;
let answered = false;
let startTime = null;

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/duelHub")
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();

// Receive new question
connection.on("NewQuestion", (data) => {
    console.log("New question:", data);
    if (data) {
        showQuestion(data);
    }
});

// Opponent answered
connection.on("OpponentAnswered", (data) => {
    document.getElementById('opponentStatus').innerHTML =
        '<i class="fa-solid fa-check text-success me-2"></i>Đối thủ đã trả lời!';
});

// Question result
connection.on("QuestionResult", (data) => {
    console.log("Question result:", data);
    showCorrectAnswer(data.correctAnswerId);
    updateScores(data.player1TotalScore, data.player2TotalScore);
});

// Answer result (for current player)
connection.on("AnswerResult", (data) => {
    console.log("Answer result:", data);
    // Score update is handled here too
    if (isPlayer1) {
        document.getElementById('myScore').textContent = data.totalScore;
    } else {
        document.getElementById('myScore').textContent = data.totalScore;
    }
});

// Match ended
connection.on("MatchEnded", (data) => {
    console.log("Match ended:", data);
    document.getElementById('waitingOverlay').style.display = 'flex';
    document.getElementById('overlayMessage').textContent =
        data.isDraw ? 'Hòa!' : (data.winnerUserName === '@User.Identity?.Name' ? 'Bạn thắng!' : 'Bạn thua!');

    setTimeout(() => {
        window.location.href = `/Duel/Result?matchId=${matchId}`;
    }, 2000);
});

connection.on("Error", (msg) => {
    console.error("Error:", msg);
    Toastify({ text: msg, backgroundColor: "#dc3545" }).showToast();
});

// Start connection
connection.start().then(() => {
    console.log("Connected to SignalR");
    // Join match group first
    connection.invoke("JoinMatchGroup", matchId).then(() => {
        console.log("Joined match group, waiting for questions...");
    }).catch(err => console.error("JoinMatchGroup error:", err));
}).catch(err => console.error(err));

function showQuestion(data) {
    answered = false;
    currentQuestionId = data.questionId;
    timeLeft = data.timeLimit || 10;
    startTime = Date.now();

    document.getElementById('waitingOverlay').style.display = 'none';
    document.getElementById('questionNumber').textContent = `Câu ${data.questionIndex} / ${data.totalQuestions}`;
    document.getElementById('questionText').textContent = data.questionText;
    document.getElementById('opponentStatus').innerHTML =
        '<i class="fa-solid fa-hourglass-half me-2"></i>Đợi đối thủ trả lời...';

    // Render answers
    const grid = document.getElementById('answersGrid');
    grid.innerHTML = data.answers.map(a => `
            <button class="answer-btn" data-answer-id="${a.answerId}" onclick="selectAnswer(${a.answerId})">
                ${a.answerText}
            </button>
        `).join('');

    // Start timer
    startTimer();
}

function startTimer() {
    clearInterval(timerInterval);
    updateTimerDisplay();

    timerInterval = setInterval(() => {
        timeLeft--;
        updateTimerDisplay();

        if (timeLeft <= 0) {
            clearInterval(timerInterval);
            if (!answered) {
                submitAnswer(-1); // Timeout
            }
        }
    }, 1000);
}

function updateTimerDisplay() {
    const timerValue = document.getElementById('timerValue');
    const timerCircle = document.getElementById('timerCircle');

    timerValue.textContent = timeLeft;
    timerValue.className = timeLeft <= 3 ? 'timer-value danger' : 'timer-value';
    timerCircle.style.setProperty('--progress', `${(timeLeft / 10) * 100}%`);
}

function selectAnswer(answerId) {
    if (answered) return;

    // Highlight selected
    document.querySelectorAll('.answer-btn').forEach(btn => {
        btn.classList.remove('selected');
        btn.classList.add('disabled');
    });
    document.querySelector(`[data-answer-id="${answerId}"]`).classList.add('selected');

    submitAnswer(answerId);
}

function submitAnswer(answerId) {
    if (answered) return;
    answered = true;
    clearInterval(timerInterval);

    const responseTime = (Date.now() - startTime) / 1000;

    connection.invoke("SubmitAnswer", matchId, currentQuestionId, answerId, responseTime)
        .catch(err => {
            console.error(err);
            Toastify({ text: "Lỗi gửi câu trả lời", backgroundColor: "#dc3545" }).showToast();
        });
}

function showCorrectAnswer(correctAnswerId) {
    document.querySelectorAll('.answer-btn').forEach(btn => {
        const id = parseInt(btn.dataset.answerId);
        btn.classList.add('disabled');

        if (id === correctAnswerId) {
            btn.classList.add('correct');
        } else if (btn.classList.contains('selected')) {
            btn.classList.add('wrong');
        }
    });
}

function updateScores(p1Score, p2Score) {
    const myScore = isPlayer1 ? p1Score : p2Score;
    const oppScore = isPlayer1 ? p2Score : p1Score;

    document.getElementById('myScore').textContent = myScore;
    document.getElementById('opponentScore').textContent = oppScore;
}

// Handle page unload
window.addEventListener('beforeunload', (e) => {
    connection.invoke("LeaveMatch", matchId).catch(() => { });
});