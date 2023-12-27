let popupTitle = document.getElementById('popupTitle');
let popupDescription = document.getElementById('popupDescription');
let popup = document.getElementById('popup');

popup.addEventListener('click', function(e) {
    if(!e.target.closest('.popupContent')){
        popup.classList.remove('open');
        window.location = "../index.html";
    }
});

const registerForm = document.getElementById('resetForm');
registerForm.addEventListener('submit', function (e) {
	e.preventDefault();
	const formData = new FormData(registerForm);
	let passwordl = formData.get('password');
	let password2l = formData.get('password2');
    let tokenl = getCookie('resetToken');
    if(passwordl != password2l){
        console.log(passwordl + " " + password2l);
        let errort = "Error in confirm password";
        let errorText = document.getElementById('errorText');
        let errorH = document.getElementById("error");
        errorText.innerText = errort;
        errorH.classList.add("open");
        return e.preventDefault();
    }
    let noErrors = true;
    let data = {
        token: tokenl,
        newPassword: passwordl
    };
let link = 'https://localhost:7055/api/User/reset-password';
fetch('https://localhost:7055/api/User/reset-password', {
    method: "POST",
    body: JSON.stringify(data),
    headers:{
        "Content-type": "application/json"
    }
})
.then((resu) => {
    if(resu.status == 400){
        noErrors = false;
    }
    return resu.json();
})
.then((datas) => {
    if(noErrors) {
        console.log(getCookie('userId'));

        popupTitle.value = "Confirmed";
        popupDescription.value = "Password reseted";
            popup.classList.add("open");
            document.body.style.overflowY = "hidden";
            popupTitle.readOnly = true;
    popupDescription.readOnly = true;
    } else {
        let errort = datas.value;
        let errorText = document.getElementById('errorText');
        let errorH = document.getElementById("error");
        errorText.innerText = errort;
        errorH.classList.add("open");
    }

})
});

function setCookie(name, value) {
    document.cookie = `${name}=${value}; expires=Fri, 31 Dec 9999 23:59:59 GMT; path=/`;
}
function deleteCookie(name) {
    setCookie(name, null);
}
function getCookie(name){
    const cDecoded = decodeURIComponent(document.cookie);
    const cArray = cDecoded.split("; ");
    let result = null;
    
    cArray.forEach(element => {
        if(element.indexOf(name) == 0){
            result = element.substring(name.length + 1)
        }
    })
    return result;
}

console.log("connected");