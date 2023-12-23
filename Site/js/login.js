const loginForm = document.getElementById('loginForm');
loginForm.addEventListener('submit', function (e) {
	e.preventDefault();
	const formData = new FormData(loginForm);
	let namel = formData.get('name');
	let passwordl = formData.get('password');
    let noErrors = true;
    let data = {
            name: namel,
            password: passwordl
    };
let link = 'https://localhost:7055/api/User/login'
fetch('https://localhost:7055/api/User/login', {
    method: "POST",
    body: JSON.stringify(data),
    headers:{
        "Content-type": "application/json"
    }
})
.then((resu) => {
    if(resu.status == 400){
        noErrors = false;
    };
    return resu.json();
})
.then((datas) => {
    console.log(datas.error);
    if(noErrors == true){
        console.log(datas);
        setCookie('userId', datas.id);
        setCookie('name', datas.name);
        setCookie('password', document.getElementById('password').value);
        console.log('SUCCES');
        console.log(getCookie('userId'));
        window.location = "../index.html";
    } else {
        let errort = datas.error;
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
