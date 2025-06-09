let appVerifier;

//document.addEventListener('DOMContentLoaded', () => {
//    initializeRecaptcha();
//    document.getElementById('otp').addEventListener('input', () => {
//        const otp = document.getElementById('otp').value;
//        document.getElementById('verifyOtpBtn').disabled = otp.length < 6; 
//    });
//});

function initializeRecaptcha() {
    appVerifier = new firebase.auth.RecaptchaVerifier('recaptcha-container', {
        size: 'invisible',
        callback: (response) => {
            console.log('reCAPTCHA verified');
        },
        'expired-callback': () => {
            console.log('reCAPTCHA expired');
        }
    });
    appVerifier.render();
}
function sendOTP() {
    initializeRecaptcha();
    let phoneNumber = document.getElementById('phoneNumber').value;
    if (phoneNumber.startsWith('0')) {
        phoneNumber = phoneNumber.replace(/^0/, '+84');
    }
    const statusDiv = document.getElementById('status');

    //if (!phoneNumber) {
    //    statusDiv.textContent = 'Vui lòng nhập số điện thoại!';
    //    return;
    //}
    firebase.auth().signInWithPhoneNumber(phoneNumber, appVerifier)
        .then((confirmationResult) => {
            window.confirmationResult = confirmationResult;
            statusDiv.textContent = 'Mã OTP đã được gửi! Vui lòng kiểm tra.';
            statusDiv.className = 'mt-3 text-center text-success';
         /*   document.getElementById('sendOtpBtn').disabled = true;*/
            document.getElementById('verifyOtpBtn').disabled = false;
        })
        .catch((error) => {
            statusDiv.textContent = 'Lỗi: Gửi OTP thất bại' /*+ error.message*/;
            console.error('Error sending OTP:', error);
            /*appVerifier.render();*/ 
        });
}

function verifyOTP() {
    const otp = document.getElementById('otp').value;
    const statusDiv = document.getElementById('status');

    if (!otp || otp.length < 6) {
        statusDiv.textContent = 'Vui lòng nhập mã OTP hợp lệ!';
        return;
    }

    if (!window.confirmationResult) {
        statusDiv.textContent = 'Vui lòng gửi OTP trước!';
        return;
    }
    window.confirmationResult.confirm(otp)
        .then((result) => {
            const user = result.user;
            //const token = user.getIdToken(); 
            //localStorage.setItem('jwtToken', token); 
            statusDiv.textContent = 'Xác thực thành công! Chuyển hướng...';
            statusDiv.className = 'mt-3 text-center text-success';

            Register();

            //setTimeout(() => {
            //    window.location.href = '/HomePage'; 
            //}, 1000);
        })
        .catch((error) => {
            statusDiv.textContent = 'Lỗi xác minh OTP: ' + error.message;
            console.error('Error verifying OTP:', error);
        });
}

async function Register() {

    const phoneNumber = document.getElementById('phoneNumber').value;
    const password = document.getElementById('password').value;
    const firstName = document.getElementById('firstName').value;
    const lastName = document.getElementById('lastName').value;
    const userType = document.getElementById('userType').value;
    const statusDiv = document.getElementById('status');
     try {
         const response = await fetch('http://localhost:8888/api/User/register', {
             method: 'POST',
             headers: {
                 'Content-Type': 'application/json'
             },
             body: JSON.stringify({ phoneNumber, password, firstName, lastName, userType })
         }); 
         const data = await response.json();
         if (response.ok) {
             console.log(`Data is: ${data.data}`);
             window.location.href = `http://localhost:5050/HomePage/Login?token=${data.data}`;
         } else {
             statusDiv.classList.add("text-danger");
             statusDiv.textContent = data.message || 'Đăng ký thất bại';
             
           
         }
     } catch (error) {

         statusDiv.textContent = 'Lỗi kết nối đến API';
     }



}