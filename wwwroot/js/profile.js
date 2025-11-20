// Profile Page JavaScript

function validateBirthday(dateStr) {
    // Kiểm tra định dạng dd/MM/yyyy
    const datePattern = /^(\d{2})\/(\d{2})\/(\d{4})$/;
    const match = dateStr.match(datePattern);
    
    if (!match) {
        return { valid: false, message: 'Định dạng ngày sinh phải là dd/MM/yyyy' };
    }

    const day = parseInt(match[1], 10);
    const month = parseInt(match[2], 10);
    const year = parseInt(match[3], 10);

    const currentYear = new Date().getFullYear();
    if (year < 1900 || year > currentYear) {
        return { valid: false, message: `Năm sinh phải từ 1900 đến ${currentYear}` };
    }

    if (month < 1 || month > 12) {
        return { valid: false, message: 'Tháng không hợp lệ (1-12)' };
    }

    const daysInMonth = new Date(year, month, 0).getDate();
    if (day < 1 || day > daysInMonth) {
        return { valid: false, message: `Ngày không hợp lệ cho tháng ${month}` };
    }

    const birthDate = new Date(year, month - 1, day);
    const today = new Date();
    let age = today.getFullYear() - birthDate.getFullYear();
    const monthDiff = today.getMonth() - birthDate.getMonth();
    if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
        age--;
    }

    if (age < 10) {
        return { valid: false, message: 'Người dùng phải từ 10 tuổi trở lên' };
    }
    if (age > 120) {
        return { valid: false, message: 'Tuổi không hợp lệ' };
    }

    return { valid: true };
}

async function updateField(fieldName, value) {
    try {
        const response = await fetch('/Profile/UpdateField', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify({
                fieldName: fieldName,
                value: value
            })
        });
        return await response.json();
    } catch (error) {
        return { success: false, message: 'Lỗi kết nối' };
    }
}

async function saveProfile() {
    const birthdayInput = document.getElementById('birthday');
    const birthdayValue = birthdayInput.value.trim();

    if (birthdayValue) {
        const validation = validateBirthday(birthdayValue);
        if (!validation.valid) {
            Toastify({
                text: "❌ " + validation.message,
                duration: 4000,
                gravity: "top",
                position: "right",
                style: {
                    background: "#e74c3c"
                }
            }).showToast();
            birthdayInput.focus();
            return;
        }
    }

    const fields = [
        { element: document.getElementById('fullname'), field: 'FullName' },
        { element: document.getElementById('email'), field: 'Email' },
        { element: birthdayInput, field: 'BirthDay' },
        { element: document.getElementById('sex'), field: 'Sex' }
    ];

    let allSuccess = true;
    for (const { element, field } of fields) {
        const result = await updateField(field, element.value);
        if (!result.success) {
            allSuccess = false;
            Toastify({
                text: "❌ Lỗi: " + result.message,
                duration: 3000,
                gravity: "top",
                position: "right",
                style: {
                    background: "#e74c3c"
                }
            }).showToast();
            break;
        }
    }

    if (allSuccess) {
        Toastify({
            text: "✅ Cập nhật thành công!",
            duration: 3000,
            gravity: "top",
            position: "right",
            style: {
                background: "#07bc0c"
            }
        }).showToast();
    }
}
