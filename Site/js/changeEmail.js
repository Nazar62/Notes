const changeEmailForm = document.getElementById('changeEmailForm');
changeEmailForm.addEventListener('submit', function (e) {
	e.preventDefault();
	const formData = new FormData(changeEmailForm);
	let email = formData.get('email');
	let password = formData.get('password');
    let noErrors = true;
    let data = {
        id: getCookie('userId'),
        newEmail: email,
        password: password
    };
let link = 'https://localhost:7055/api/User/change-email'
fetch('https://localhost:7055/api/User/change-email', {
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
    console.log(datas);
    if(noErrors == true){
        console.log(datas);
        alert("Check Email!");
        window.location.href = "../index.html"
    } else {
        let errort = datas.value;
        let errorText = document.getElementById('errorText');
        let errorH = document.getElementById("error");
        errorText.innerText = errort;
        errorH.classList.add("open");
        noErrors = true;
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