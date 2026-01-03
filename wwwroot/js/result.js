// ========== INITIALIZATION ==========
document.addEventListener('DOMContentLoaded', () => {
    animateScoreCircle();
    checkForConfetti();
    setupDetailsToggle();
});

// ========== SCORE CIRCLE ANIMATION ==========
function animateScoreCircle() {
    const progressCircle = document.querySelector('.circle-progress');
    const scoreNumber = document.querySelector('.score-number');
    const scoreTotal = document.querySelector('.score-total');

    if (!progressCircle || !scoreNumber || !scoreTotal) return;

    const rawScore = parseInt(progressCircle.dataset.score, 10);   // correctCount
    const rawTotal = parseInt(progressCircle.dataset.total, 10);   // TotalQuestions
    if (!rawTotal) return;

    // progress của vòng tròn vẫn tính theo số câu đúng / tổng câu
    const percentage = (rawScore / rawTotal) * 100;
    const circumference = 2 * Math.PI * 85;
    const offset = circumference - (percentage / 100) * circumference;

    setTimeout(() => {
        progressCircle.style.strokeDashoffset = offset;
    }, 100);

    // HIỂN THỊ: nhân 10
    const displayScore = rawScore * 10;
    animateCounter(scoreNumber, 0, displayScore, 2000);
}


// ========== COUNTER ANIMATION ==========
function animateCounter(element, start, end, duration) {
    const startTime = performance.now();
    
    function update(currentTime) {
        const elapsed = currentTime - startTime;
        const progress = Math.min(elapsed / duration, 1);
        const easeOutQuart = 1 - Math.pow(1 - progress, 4);
        const current = Math.floor(start + (end - start) * easeOutQuart);
        
        element.textContent = current;
        
        if (progress < 1) {
            requestAnimationFrame(update);
        }
    }
    
    requestAnimationFrame(update);
}

// ========== CONFETTI ==========
function checkForConfetti() {
    const progressCircle = document.querySelector('.circle-progress');
    if (!progressCircle) return;
    
    const score = parseInt(progressCircle.dataset.score);
    const total = parseInt(progressCircle.dataset.total);
    const percentage = (score / total) * 100;
    
    if (percentage >= 60) {
        setTimeout(() => createConfetti(), 2000);
    }
}

function createConfetti() {
    const canvas = document.getElementById('confettiCanvas');
    if (!canvas) return;
    
    const ctx = canvas.getContext('2d');
    canvas.width = window.innerWidth;
    canvas.height = window.innerHeight;
    
    const particles = [];
    const colors = ['#ff6b6b', '#4ecdc4', '#45b7d1', '#ffd93d', '#a8e6cf', '#ff8b94'];
    
    // Create particles
    for (let i = 0; i < 150; i++) {
        particles.push({
            x: Math.random() * canvas.width,
            y: Math.random() * canvas.height - canvas.height,
            size: Math.random() * 8 + 4,
            speedY: Math.random() * 3 + 2,
            speedX: Math.random() * 4 - 2,
            color: colors[Math.floor(Math.random() * colors.length)],
            rotation: Math.random() * 360,
            rotationSpeed: Math.random() * 10 - 5
        });
    }
    
    function animate() {
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        
        let hasActive = false;
        
        particles.forEach((p, index) => {
            p.y += p.speedY;
            p.x += p.speedX;
            p.rotation += p.rotationSpeed;
            
            if (p.y > canvas.height) {
                particles.splice(index, 1);
                return;
            }
            
            hasActive = true;
            
            ctx.save();
            ctx.translate(p.x, p.y);
            ctx.rotate(p.rotation * Math.PI / 180);
            ctx.fillStyle = p.color;
            ctx.fillRect(-p.size / 2, -p.size / 2, p.size, p.size);
            ctx.restore();
        });
        
        if (hasActive) {
            requestAnimationFrame(animate);
        } else {
            canvas.style.display = 'none';
        }
    }
    
    animate();
}

// ========== DETAILS TOGGLE ==========
function setupDetailsToggle() {
    const toggleBtn = document.getElementById('detailsToggle');
    const detailsSection = document.getElementById('detailsSection');
    
    if (!toggleBtn || !detailsSection) return;
    
    toggleBtn.addEventListener('click', () => {
        toggleBtn.classList.toggle('active');
        detailsSection.classList.toggle('hidden');
    });
}
