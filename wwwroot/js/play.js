document.addEventListener("DOMContentLoaded", function () {
    const quizContainer = document.getElementById('quiz-container');
    const nextBtn = document.getElementById('nextBtn');
    const prevBtn = document.getElementById('prevBtn');
    const submitBtn = document.getElementById('submitBtn');
    const questions = quizContainer.querySelectorAll('.question-container');
    const totalQuestions = questions.length;
    let currentQuestionIndex = 0;

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

    // Initial setup
    showQuestion(0);
});


(function () {
    const form = document.getElementById('quizForm');
    const saveUrl = form.dataset.saveUrl; // SaveProgress endpoint
    const tokenInput = form.querySelector('input[name="__RequestVerificationToken"]');

    let dirty = false;

    const questionLastId = document.getElementById('QuestionLastId');
    const questionCount = document.getElementById('QuestionCount');


    function updateLastAnsweredQuestion() {
        questionLastId.value = getLastAnsweredQuestionId();
        questionCount.value = getAnsweredQuestionCount();
    }


    // 1) Mark dirty when any answer selected
    document.querySelectorAll('.answer-radio').forEach(radio => {
        radio.addEventListener('change',
            (ev) => {
                dirty = true;

                document.querySelectorAll('.last-question').forEach(e => {
                    e.classList.remove('last-question');
                });

                const questionContainer = ev.target.closest('.question-container');
                if (questionContainer) {
                    questionContainer.classList.add('last-question');
                    questionContainer.classList.add('answered');
                }
            });
    });

    function getAnsweredQuestionCount() {
        return document.querySelectorAll('.question-container.answered').length;
    }

    function getLastAnsweredQuestionId() {
        const lastQuestionDiv = document.querySelector('.last-question');
        return lastQuestionDiv ? lastQuestionDiv.dataset.questionId : '0';
    }

    // 2) On standard submit, clear dirty (server will handle final submission)
    form.addEventListener('submit',
        () => {
            dirty = false;
            updateLastAnsweredQuestion();
            // allow normal submit
        });

    // 3) beforeunload native prompt
    this.window.addEventListener('beforeunload',
        function (e) {
            if (!dirty) return;
            // Standard message not customizable in modern browsers
            const confirmationMessage = 'You have unsaved progress. Do you really want to leave?';
            (e || this.window.event).returnValue = confirmationMessage;
            return confirmationMessage;
        });

    // Helper: gather current form data into FormData
    function gatherFormData() {
        const fd = new FormData();

        fd.set('QSetId', '@Model.QSetId');
        fd.set('QuestionCount', getAnsweredQuestionCount().toString());

        const lastQuestionId = getLastAnsweredQuestionId();
        fd.set('QuestionLastId', lastQuestionId === '0' ? '0' : lastQuestionId);

        const questionContainers = document.querySelectorAll('.question-container');
        let answerIndex = 0;

        questionContainers.forEach(container => {
            const questionId = container.dataset.questionId;
            const selectedRadio = container.querySelector('.answer-radio:checked');

            if (selectedRadio) {
                fd.append(`UserAnswers[${answerIndex}].QuestionId`, questionId);
                fd.append(`UserAnswers[${answerIndex}].SelectedAnswerId`, selectedRadio.value);
            }
            answerIndex++;

        });


        if (tokenInput && !fd.has('__RequestVerificationToken')) {
            fd.append('__RequestVerificationToken', tokenInput.value);
        }

        return fd;
    }


    // 4) save progress via AJAX POST
    function saveProgress() {
        const fd = gatherFormData();

        return fetch(saveUrl,
            {
                method: 'POST',
                body: fd,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest' // indicates AJAX
                },
                credentials: 'same-origin'
            }).then(response => {
                if (!response.ok) throw new Error('Save failed: ' + response.status);
                return response;
            });
    }

    // 5) Confirm + navigation helper
    function confirmAndNavigate(targetUrl) {
        if (!dirty) {
            window.location.href = targetUrl;
            return;
        }

        const shouldSave = confirm('You have unsaved progress. Save progress before leaving? Click OK to save, Cancel to leave without saving.');
        if (shouldSave) {
            // optionally show a simple saving indicator
            const originalText = document.activeElement && document.activeElement.innerText;
            // perform save
            saveProgress()
                .then(() => {
                    dirty = false;
                    window.location.href = targetUrl;
                })
                .catch(err => {
                    // if save failed, ask user whether to still navigate
                    const navigateAnyway = confirm('Saving progress failed. Leave anyway?');
                    if (navigateAnyway) {
                        dirty = false;
                        window.location.href = targetUrl;
                    }
                });
        } else {
            // user chose not to save
            dirty = false;
            window.location.href = targetUrl;
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

    // --- ADDITION: Send QuestionId to Quiz/SaveQuestion when checkbox is ticked ---
    document.querySelectorAll('.save-question-checkbox').forEach(checkbox => {
        checkbox.addEventListener('change', function (ev) {
            const questionId = parseInt(ev.target.dataset.questionId);

            console.log("Question ID from HTML:", questionId); // Kiểm tra xem ở đây có hiện 4 không?
            console.log("Type:", typeof questionId);

            if (checkbox.checked) {
                fetch('/Bookmark/SaveQuestion', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'X-Requested-With': 'XMLHttpRequest',
                        'RequestVerificationToken': tokenInput ? tokenInput.value : ''
                    },
                    body: questionId,
                    credentials: 'same-origin'
                })
                    .then(response => {
                        if (!response.ok) throw new Error('SaveQuestion failed: ' + response.status);
                    })
                    .catch(err => {
                        alert('Could not save question: ' + err.message);
                    });
            } else {
                fetch('/Bookmark/UnsaveQuestion', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'X-Requested-With': 'XMLHttpRequest',
                        'RequestVerificationToken': tokenInput ? tokenInput.value : ''
                    },
                    body: questionId,
                    credentials: 'same-origin'
                })
                    .then(response => {
                        if (!response.ok) throw new Error('UnsaveQuestion failed: ' + response.status);
                    })
                    .catch(err => {
                        alert('Could not unsave question: ' + err.message);
                    });
            }
        });
    });

})();

