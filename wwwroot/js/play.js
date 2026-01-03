// ========== INITIALIZATION ==========
document.addEventListener('DOMContentLoaded', () => {
    // DOM Elements
    const form = document.getElementById('quizForm');
    const nextBtn = document.getElementById('nextBtn');
    const submitBtn = document.getElementById('submitBtn');
    const cancelBtn = document.getElementById('cancelBtn');
    const progressContainer = document.getElementById('progress-container');
    const progressSegments = progressContainer
        ? Array.from(progressContainer.querySelectorAll('.progress-segment'))
        : [];
    const counter = document.getElementById('question-counter');
    const questions = Array.from(document.querySelectorAll('.question-container'));
    const questionLastId = document.getElementById('QuestionLastId');
    const questionCount = document.getElementById('QuestionCount');

    if (!form || !nextBtn || !submitBtn || questions.length === 0) return;

    // State
    const totalQuestions = questions.length;
    let currentIndex = 0;
    let isDirty = false;
    const saveUrl = form.dataset.saveUrl;

    // ===== RESUME META FROM SERVER (Razor data-*) =====
    const serverStartIndex = Number.parseInt(form.dataset.startIndex || '0', 10);
    const serverLastId = Number.parseInt(form.dataset.lastId || '0', 10);

    // ========== CORE HELPERS ==========

    function isQuestionAnsweredByIndex(index) {
        const q = questions[index];
        return !!q.querySelector('.answer-radio:checked');
    }

    function getAnsweredCount() {
        return questions.filter((_, i) => isQuestionAnsweredByIndex(i)).length;
    }

    function getLastAnsweredQuestionId() {
        // ưu tiên .last-question (do server hoặc do user chọn)
        const last = document.querySelector('.question-container.last-question');
        if (last && last.querySelector('.answer-radio:checked')) return last.dataset.questionId;

        // fallback: câu cuối cùng có radio checked
        const answered = questions.filter(q => q.querySelector('.answer-radio:checked'));
        return answered.length ? answered[answered.length - 1].dataset.questionId : '0';
    }

    function updateFormData() {
        if (questionLastId) questionLastId.value = getLastAnsweredQuestionId();
        if (questionCount) questionCount.value = getAnsweredCount().toString();
    }

    function computeResumeIndex() {
        // 1) Nếu có lastId từ server -> tìm index theo data-question-id
        if (serverLastId) {
            const idx = questions.findIndex(q => Number(q.dataset.questionId) === serverLastId);
            if (idx >= 0) return idx;
        }

        // 2) Nếu serverStartIndex hợp lệ
        if (Number.isInteger(serverStartIndex) && serverStartIndex >= 0 && serverStartIndex < totalQuestions) {
            return serverStartIndex;
        }

        // 3) Nếu có .last-question sẵn
        const lastEl = document.querySelector('.question-container.last-question');
        if (lastEl) {
            const idx = questions.indexOf(lastEl);
            if (idx >= 0) return idx;
        }

        // 4) Fallback: câu cuối đã trả lời
        for (let i = totalQuestions - 1; i >= 0; i--) {
            if (isQuestionAnsweredByIndex(i)) return i;
        }

        return 0;
    }

    // ========== RESUME: HYDRATE UI FROM SERVER ==========
    function hydrateFromServer() {
        // 1) Set .answered + .selected theo radio checked
        questions.forEach(q => {
            const checked = q.querySelector('.answer-radio:checked');
            if (checked) {
                q.classList.add('answered');

                // set selected UI cho option label (nếu bạn dùng class selected)
                q.querySelectorAll('.option-label').forEach(label => label.classList.remove('selected'));
                const label = checked.closest('.option-label');
                if (label) label.classList.add('selected');
            } else {
                q.classList.remove('answered');
            }
        });

        // 2) Chuẩn hoá .last-question theo serverLastId hoặc câu cuối đã trả lời
        questions.forEach(q => q.classList.remove('last-question'));

        if (serverLastId) {
            const lastQ = questions.find(q => Number(q.dataset.questionId) === serverLastId);
            if (lastQ) lastQ.classList.add('last-question');
        } else {
            // fallback: câu cuối có checked
            for (let i = totalQuestions - 1; i >= 0; i--) {
                if (isQuestionAnsweredByIndex(i)) {
                    questions[i].classList.add('last-question');
                    break;
                }
            }
        }

        // 3) Update hidden fields ngay lúc load (resume)
        updateFormData();
    }

    // ========== CORE FUNCTIONS ==========

    function showQuestion(index) {
        if (index < 0) index = 0;
        if (index >= totalQuestions) index = totalQuestions - 1;

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

    function updateCounter() {
        if (counter) {
            counter.textContent = `${currentIndex + 1} / ${totalQuestions}`;
        }
    }

    function updateProgressBar() {
        progressSegments.forEach((segment, index) => {
            segment.classList.remove('active', 'current', 'answered', 'unanswered', 'skipped');

            const answered = isQuestionAnsweredByIndex(index);

            // trạng thái trả lời/chưa trả lời
            if (answered) segment.classList.add('answered');
            else segment.classList.add('unanswered');

            // trạng thái current/đã đi qua
            if (index === currentIndex) {
                segment.classList.add('current');
            } else if (index < currentIndex) {
                segment.classList.add('active');

                // nếu đã đi qua nhưng chưa trả lời => highlight mạnh hơn
                if (!answered) segment.classList.add('skipped');
            }
        });
    }


    function updateButtonVisibility() {
        const isLastQuestion = currentIndex === totalQuestions - 1;

        if (isLastQuestion) {
            nextBtn.classList.add('hidden');
            submitBtn.classList.remove('hidden');
            if (cancelBtn) cancelBtn.classList.add('hidden');
        } else {
            nextBtn.classList.remove('hidden');
            submitBtn.classList.add('hidden');
            if (cancelBtn) cancelBtn.classList.remove('hidden');
        }
    }

    function updateButtonState() {
        const isAnswered = isCurrentQuestionAnswered();
        nextBtn.disabled = !isAnswered;
        submitBtn.disabled = !isAnswered;
    }

    function isCurrentQuestionAnswered() {
        const currentQuestion = questions[currentIndex];
        return !!currentQuestion.querySelector('.answer-radio:checked');
    }

    // ========== EVENT HANDLERS ==========

    nextBtn.addEventListener('click', () => {
        if (currentIndex < totalQuestions - 1) {
            showQuestion(currentIndex + 1);
        }
    });

    document.querySelectorAll('.answer-radio').forEach(radio => {
        radio.addEventListener('change', (e) => {
            isDirty = true;

            const questionContainer = e.target.closest('.question-container');

            // Update selected UI
            questionContainer.querySelectorAll('.option-label').forEach(label => {
                label.classList.remove('selected');
            });
            const label = e.target.closest('.option-label');
            if (label) label.classList.add('selected');

            // Mark answered + last-question
            questionContainer.classList.add('answered');
            questions.forEach(q => q.classList.remove('last-question'));
            questionContainer.classList.add('last-question');

            // Update hidden fields + progress UI
            updateFormData();
            updateProgressBar();

            // Update button state
            updateButtonState();
        });
    });

    progressSegments.forEach((segment, index) => {
        segment.addEventListener('click', () => {
            showQuestion(index);
        });
    });

    form.addEventListener('submit', () => {
        isDirty = false;
        updateFormData();
    });

    window.addEventListener('beforeunload', (e) => {
        if (!isDirty) return;
        const message = 'Bạn có thay đổi chưa lưu. Bạn có chắc muốn rời đi?';
        e.returnValue = message;
        return message;
    });

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
    hydrateFromServer();
    const resumeIndex = computeResumeIndex();
    showQuestion(resumeIndex);
});
