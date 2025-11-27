document.addEventListener("DOMContentLoaded", function () {
    // --- DOM Element Selection ---
    const quizContainer = document.getElementById('quiz-container');
    const nextBtn = document.getElementById('nextBtn');
    const prevBtn = document.getElementById('prevBtn');
    const submitBtn = document.getElementById('submitBtn');
    const questions = quizContainer.querySelectorAll('.question-container');
    const form = document.getElementById('quizForm');
    const progressBar = document.getElementById('quiz-progress-bar');
    const tokenInput = form.querySelector('input[name="__RequestVerificationToken"]');
    const questionLastId = document.getElementById('QuestionLastId');
    const questionCount = document.getElementById('QuestionCount');

    // --- State Variables ---
    const totalQuestions = questions.length;
    let currentQuestionIndex = 0;
    let dirty = false;
    const saveUrl = form.dataset.saveUrl;

    // --- Progress Bar ---
    function updateProgressBar() {
        const answeredCount = document.querySelectorAll('.question-container.answered').length;
        const progress = totalQuestions > 0 ? (answeredCount / totalQuestions) * 100 : 0;
        if (progressBar) {
            progressBar.style.width = `${progress}%`;
            progressBar.setAttribute('aria-valuenow', progress);
            progressBar.textContent = `${Math.round(progress)}%`;
        }
    }

    // --- Question Navigation ---
    function showQuestion(index) {
        questions.forEach((question, i) => {
            question.classList.toggle('active', i === index);
            question.classList.toggle('hidden', i !== index);
        });

        prevBtn.classList.toggle('hidden', index === 0);
        nextBtn.classList.toggle('hidden', index === totalQuestions - 1);
        submitBtn.classList.toggle('hidden', index !== totalQuestions - 1);
    }

    nextBtn.addEventListener('click', () => {
        if (currentQuestionIndex < totalQuestions - 1) {
            currentQuestionIndex++;
            showQuestion(currentQuestionIndex);
        }
    });

    prevBtn.addEventListener('click', () => {
        if (currentQuestionIndex > 0) {
            currentQuestionIndex--;
            showQuestion(currentQuestionIndex);
        }
    });

    // --- Answer Handling & Progress ---
    function getAnsweredQuestionCount() {
        return document.querySelectorAll('.question-container.answered').length;
    }

    function getLastAnsweredQuestionId() {
        const lastQuestionDiv = document.querySelector('.last-question');
        return lastQuestionDiv ? lastQuestionDiv.dataset.questionId : '0';
    }

    function updateLastAnsweredQuestion() {
        questionLastId.value = getLastAnsweredQuestionId();
        questionCount.value = getAnsweredQuestionCount();
    }

    document.querySelectorAll('.answer-radio').forEach(radio => {
        radio.addEventListener('change', (ev) => {
            dirty = true;

            const currentQuestionContainer = ev.target.closest('.question-container');
            currentQuestionContainer.querySelectorAll('.option-label').forEach(label => {
                label.classList.remove('selected');
            });
            ev.target.closest('.option-label').classList.add('selected');

            document.querySelectorAll('.last-question').forEach(e => e.classList.remove('last-question'));

            const questionContainer = ev.target.closest('.question-container');
            if (questionContainer) {
                if (!questionContainer.classList.contains('answered')) {
                    questionContainer.classList.add('answered');
                    updateProgressBar(); // Update progress on new answer
                }
                questionContainer.classList.add('last-question');
            }
        });
    });

    // --- Form Submission & Data Handling ---
    form.addEventListener('submit', () => {
        dirty = false;
        updateLastAnsweredQuestion();
    });

    window.addEventListener('beforeunload', function (e) {
        if (!dirty) return;
        const confirmationMessage = 'You have unsaved progress. Do you really want to leave?';
        (e || window.event).returnValue = confirmationMessage;
        return confirmationMessage;
    });

    function gatherFormData() {
        const fd = new FormData(form); // More efficient way to get form data
        // Update values that might have changed
        fd.set('QuestionCount', getAnsweredQuestionCount().toString());
        fd.set('QuestionLastId', getLastAnsweredQuestionId());

        // FormData(form) automatically includes checked radios and token, so manual appending is not always needed.
        // However, to be safe and explicit:
        if (tokenInput && !fd.has('__RequestVerificationToken')) {
            fd.append('__RequestVerificationToken', tokenInput.value);
        }
        return fd;
    }

    function saveProgress() {
        const fd = gatherFormData();
        return fetch(saveUrl, {
            method: 'POST',
            body: fd,
            headers: { 'X-Requested-With': 'XMLHttpRequest' },
            credentials: 'same-origin'
        }).then(response => {
            if (!response.ok) throw new Error('Save failed: ' + response.status);
            return response;
        });
    }

    // --- Navigation Confirmation ---
    function confirmAndNavigate(targetUrl) {
        if (!dirty) {
            window.location.href = targetUrl;
            return;
        }
        const confirmMessage = 'Bạn có thay đổi chưa được lưu. Bạn có muốn lưu lại trước khi rời đi không?\n\n- Nhấn OK để LƯU và rời đi.\n- Nhấn Cancel để HỦY BỎ thao tác rời đi.';
        const shouldSave = confirm(confirmMessage);

        if (shouldSave) {
            saveProgress()
                .then(() => {
                    dirty = false;
                    window.location.href = targetUrl;
                })
                .catch(err => {
                    console.error(err);
                    const navigateAnyway = confirm('Lưu tiến trình thất bại. Bạn vẫn muốn rời đi?');                    if (navigateAnyway) {
                        if (navigateAnyway) {
                            dirty = false;
                            window.location.href = targetUrl;
                        }
                    }
                });
        } 
    }

    document.querySelectorAll('a.nav-link, a[data-confirm="true"]').forEach(a => {
        if (!a.href || a.getAttribute('href').startsWith('#') || a.getAttribute('href').startsWith('javascript:')) return;
        a.addEventListener('click', function (ev) {
            const linkUrl = a.href;
            const samePage = (linkUrl.split('#')[0] === window.location.href.split('#')[0]);
            if (samePage) return;
            ev.preventDefault();
            confirmAndNavigate(linkUrl);
        });
    });

    // --- Bookmark Handling ---
    document.querySelectorAll('.save-question-checkbox').forEach(checkbox => {
        checkbox.addEventListener('change', function (ev) {
            const questionId = parseInt(ev.target.dataset.questionId, 10);
            const isChecked = checkbox.checked;
            const url = isChecked ? '/Bookmark/SaveQuestion' : '/Bookmark/UnsaveQuestion';
            const action = isChecked ? 'save' : 'unsave';

            fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest',
                    'RequestVerificationToken': tokenInput ? tokenInput.value : ''
                },
                body: JSON.stringify(questionId), // Send as JSON
                credentials: 'same-origin'
            })
                .then(response => {
                    if (!response.ok) throw new Error(`${action} failed: ${response.status}`);
                })
                .catch(err => {
                    alert(`Could not ${action} question: ${err.message}`);
                    // Revert checkbox state on failure
                    checkbox.checked = !isChecked;
                });
        });
    });

    // --- Initial UI Setup ---
    function initializeUI() {
        showQuestion(0);

        document.querySelectorAll('.question-container').forEach(container => {
            const checkedRadio = container.querySelector('.answer-radio:checked');
            if (checkedRadio) {
                container.classList.add('answered');
                checkedRadio.closest('.option-label').classList.add('selected');
            }
        });
        updateProgressBar();
    }

    initializeUI();
});