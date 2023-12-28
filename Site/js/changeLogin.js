const changeLoginForm = document.getElementById('changeLoginForm');
changeLoginForm.addEventListener('submit', function (e) {
	e.preventDefault();
	const formData = new FormData(changeLoginForm);
	let login = formData.get('login');
	let password = formData.get('password');
    let noErrors = true;
    let data = {
        oldName: getCookie('name'),
        newName: login,
        password: password
    };
let link = 'https://localhost:7055/api/User/change-userName'
fetch('https://localhost:7055/api/User/change-userName', {
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
        setCookie('name', login);
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