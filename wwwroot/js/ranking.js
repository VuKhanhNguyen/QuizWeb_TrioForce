

function scrollToCurrentUser() {
    const currentUserRow = document.querySelector('.current-user');
    if (currentUserRow) {
        const tbody = document.querySelector('.ranking-tbody-wrapper');
        const rowTop = currentUserRow.offsetTop;
        const rowHeight = currentUserRow.offsetHeight;
        const tbodyHeight = tbody.offsetHeight;
        
       
        tbody.scrollTo({
            top: rowTop - (tbodyHeight / 2) + (rowHeight / 2),
            behavior: 'smooth'
        });
    }
}

// Add click handlers to username cells
document.addEventListener('DOMContentLoaded', function() {
    const usernameCells = document.querySelectorAll('.username-col');
    
    usernameCells.forEach(cell => {
        cell.style.cursor = 'pointer';
        cell.addEventListener('click', async function() {
            const row = this.closest('tr');
            const username = row.dataset.username;
            
            if (username) {
                await showPlayerStats(username);
            }
        });
    });
});

async function showPlayerStats(username) {
    try {
        const response = await fetch(`/Ranking/GetPlayerStats?username=${encodeURIComponent(username)}`);
        const result = await response.json();
        
        if (result.success && result.data) {
            const data = result.data;
            
            // Populate modal with data
            document.getElementById('modal-fullname').textContent = data.fullName;
            document.getElementById('modal-username').textContent = '@' + data.username;
            document.getElementById('modal-rank').textContent = '#' + data.rank;
            document.getElementById('modal-score').textContent = data.totalScore.toLocaleString();
            document.getElementById('modal-games').textContent = data.totalGamesPlayed.toLocaleString();
            document.getElementById('modal-questions').textContent = data.totalQuestionsAnswered.toLocaleString();
            document.getElementById('modal-correct').textContent = data.correctAnswers.toLocaleString();
            document.getElementById('modal-accuracy').textContent = data.accuracyRate + '%';
            document.getElementById('modal-email').textContent = data.email;
            document.getElementById('modal-created').textContent = data.questionSetsCreated.toLocaleString();
            
            // Show modal
            document.getElementById('playerStatsModal').classList.add('active');
            document.body.style.overflow = 'hidden';
        } else {
            alert('Không thể tải thông tin người chơi');
        }
    } catch (error) {
        console.error('Error fetching player stats:', error);
        alert('Đã xảy ra lỗi khi tải thông tin');
    }
}

function closeModal() {
    document.getElementById('playerStatsModal').classList.remove('active');
    document.body.style.overflow = '';
}

// Close modal on Escape key
document.addEventListener('keydown', function(e) {
    if (e.key === 'Escape') {
        closeModal();
    }
});


