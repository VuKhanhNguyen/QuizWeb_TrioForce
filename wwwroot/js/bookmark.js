
document.addEventListener("DOMContentLoaded", function () {
    const buttons = document.querySelectorAll(".view-detail");
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');

    buttons.forEach(btn => {
        btn.addEventListener("click", function (e) {
            e.preventDefault();

            // Lấy dữ liệu từ data-attributes
            const q = this.getAttribute("data-question");
            const t = this.getAttribute("data-time");
            const a = this.getAttribute("data-answer");

            // Gắn vào modal
            document.getElementById("modalQuestionText").innerText = q;
            document.getElementById("modalMarkedTime").innerText = t;
            document.getElementById("modalAnswerTrue").innerText = a;

            // Mở modal (Bootstrap 5)
            const modal = new bootstrap.Modal(document.getElementById("questionModal"));
            modal.show();
        });
    });

    document.querySelectorAll('.remove-created-question-set').forEach(button => {
        button.addEventListener('click', function (ev) {
            const questionSetId = parseInt(ev.target.dataset.questionSetId);

            if (confirm('Xác nhận xóa bộ câu hỏi này?')) {
                fetch('/Quiz/Delete', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'X-Requested-With': 'XMLHttpRequest',
                        'RequestVerificationToken': tokenInput ? tokenInput.value : ''
                    },
                    body: questionSetId,
                    credentials: 'same-origin'
                }).then(response => {
                    if (!response.ok) throw new Error('RemoveQuestionSet failed: ' + response.status);
                    // Remove the question set from the UI
                    button.closest('.quiz-set-card').remove();
                }).catch(err => {
                    alert('Không thể xóa bộ câu hỏi: ' + err.message);
                });
            }
        });
    });


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
                        alert('Không thể xóa đánh dấu: ' + err.message);
                    });
            }
        });
    });
});