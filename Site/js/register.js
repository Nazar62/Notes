const registerForm = document.getElementById('registerForm');
registerForm.addEventListener('submit', function (e) {
	e.preventDefault();
	const formData = new FormData(registerForm);
	let namel = formData.get('name');
	let passwordl = formData.get('password');
    let emaill = formData.get('email');
    let noErrors = true;
    let data = {
            name: namel,
            password: passwordl,
            email: emaill
    };
let link = 'https://localhost:7055/api/User/Create'
fetch('https://localhost:7055/api/User/Create', {
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
        setCookie('userId', datas.id);
        setCookie('name', datas.name);
        setCookie('password', document.getElementById('password').value);
        setCookie('verificationToken', datas.verificationToken);
        console.log('SUCCES');
        console.log(getCookie('userId'));
        window.location = "../index.html";
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