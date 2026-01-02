// ========== INITIALIZATION ==========
document.addEventListener('DOMContentLoaded', () => {
    // DOM Elements
    const form = document.getElementById('quizForm');
    const nextBtn = document.getElementById('nextBtn');
    const submitBtn = document.getElementById('submitBtn');
    const cancelBtn = document.getElementById('cancelBtn');
    const progressContainer = document.getElementById('progress-container');
    const progressSegments = progressContainer ? Array.from(progressContainer.querySelectorAll('.progress-segment')) : [];
    const counter = document.getElementById('question-counter');
    const questions = Array.from(document.querySelectorAll('.question-container'));
    const questionLastId = document.getElementById('QuestionLastId');
    const questionCount = document.getElementById('QuestionCount');
    
    // State
    const totalQuestions = questions.length;
    let currentIndex = 0;
    let isDirty = false;
    const saveUrl = form.dataset.saveUrl;

    // ========== CORE FUNCTIONS ==========
    
    // Show specific question by index
    function showQuestion(index) {
        // Hide all questions, show only current
        questions.forEach((q, i) => {
            if (i === index) {
                q.classList.add('active');
                q.classList.remove('hidden');
            } else {
                q.classList.add('hidden');
                q.classList.remove('active');
            }
        });
        
        currentIndex = index;
        updateCounter();
        updateProgressBar();
        updateButtonVisibility();
        updateButtonState();
    }
    
    // Update counter text (1 / 3)
    function updateCounter() {
        if (counter) {
            counter.textContent = `${currentIndex + 1} / ${totalQuestions}`;
        }
    }
    
    // Update progress bar segments
    function updateProgressBar() {
        progressSegments.forEach((segment, index) => {
            segment.classList.remove('active', 'current');
            if (index < currentIndex) {
                segment.classList.add('active');
            } else if (index === currentIndex) {
                segment.classList.add('current');
            }
        });
    }
    
    // Show/hide buttons based on current question
    function updateButtonVisibility() {
        const isLastQuestion = currentIndex === totalQuestions - 1;
        
        if (isLastQuestion) {
            // Last question: only show HOÀN THÀNH
            nextBtn.classList.add('hidden');
            submitBtn.classList.remove('hidden');
            if (cancelBtn) cancelBtn.classList.add('hidden');
        } else {
            // Other questions: show BỎ QUA + TIẾP TỤC
            nextBtn.classList.remove('hidden');
            submitBtn.classList.add('hidden');
            if (cancelBtn) cancelBtn.classList.remove('hidden');
        }
    }
    
    // Enable/disable buttons based on answer selection
    function updateButtonState() {
        const isAnswered = isCurrentQuestionAnswered();
        nextBtn.disabled = !isAnswered;
        submitBtn.disabled = !isAnswered;
    }
    
    // Check if current question has selected answer
    function isCurrentQuestionAnswered() {
        const currentQuestion = questions[currentIndex];
        const radios = currentQuestion.querySelectorAll('.answer-radio');
        return Array.from(radios).some(radio => radio.checked);
    }
    
    // Get count of answered questions
    function getAnsweredCount() {
        return document.querySelectorAll('.question-container.answered').length;
    }
    
    // Get last answered question ID
    function getLastAnsweredQuestionId() {
        const lastQuestion = document.querySelector('.last-question');
        return lastQuestion ? lastQuestion.dataset.questionId : '0';
    }
    
    // Update hidden form fields
    function updateFormData() {
        questionLastId.value = getLastAnsweredQuestionId();
        questionCount.value = getAnsweredCount();
    }

    // ========== EVENT HANDLERS ==========
    
    // Next button click
    nextBtn.addEventListener('click', () => {
        if (currentIndex < totalQuestions - 1) {
            showQuestion(currentIndex + 1);
        }
    });
    
    // Answer selection
    document.querySelectorAll('.answer-radio').forEach(radio => {
        radio.addEventListener('change', (e) => {
            isDirty = true;
            
            const questionContainer = e.target.closest('.question-container');
            
            // Update visual selection
            questionContainer.querySelectorAll('.option-label').forEach(label => {
                label.classList.remove('selected');
            });
            e.target.closest('.option-label').classList.add('selected');
            
            // Mark as answered
            questionContainer.classList.add('answered');
            questionContainer.classList.remove('last-question');
            questionContainer.classList.add('last-question');
            
            // Update button state
            updateButtonState();
        });
    });
    
    // Progress segment click navigation
    progressSegments.forEach((segment, index) => {
        segment.addEventListener('click', () => {
            showQuestion(index);
        });
    });
    
    // Form submission
    form.addEventListener('submit', () => {
        isDirty = false;
        updateFormData();
    });
    
    // Prevent accidental navigation
    window.addEventListener('beforeunload', (e) => {
        if (!isDirty) return;
        const message = 'Bạn có thay đổi chưa lưu. Bạn có chắc muốn rời đi?';
        e.returnValue = message;
        return message;
    });
    
    // Cancel button confirmation
    if (cancelBtn) {
        cancelBtn.addEventListener('click', (e) => {
            if (isDirty && cancelBtn.dataset.confirm === 'true') {
                if (!confirm('Bạn có chắc muốn bỏ qua? Tiến trình sẽ không được lưu.')) {
                    e.preventDefault();
                }
            }
        });
    }

    // ========== INITIALIZE ==========
    showQuestion(0);
});
