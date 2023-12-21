document.addEventListener('click', function(e) {
    if(this.getElementById(e.target.id).classList.contains("title") ) {
        let x = this.getElementById(e.target.id + 'd');
        if(x.classList.contains("visible")){
            x.classList.remove("visible");
        } else {
            x.classList.add("visible");
        }
    }
});
if(navigator.cookieEnabled == false){
    navigator.cookieEnabled = true;
}


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

if(document.cookie != null && getCookie('userId') != 'null') {
    document.getElementById('loginForm').style.display = "none";
    document.getElementById('noteContainer').style.display = "block";
    console.log(getCookie('userId'));
    const userid = getCookie('userId');
    fetch(`https://localhost:7055/api/Notes/GetUserNotes/${getCookie('userId')}`)
.then((resu) => resu.json())
.then(data => {
    data.forEach(note => {
        const mackup = `
        <h3 class="title" id="${note.id}">${note.title}</h3>
        <div class="noteDescription" id="${note.id}d">${note.description}</div>`;
        document.getElementById('noteContainer').insertAdjacentHTML('beforeend',mackup);
        console.log(data);
    })
})

} else {
    document.getElementById('loginForm').style.display = "block";
    document.getElementById('noteContainer').style.display = "none";
const loginForm = document.getElementById('loginForm');
loginForm.addEventListener('submit', function (e) {
	e.preventDefault();
	const formData = new FormData(loginForm);
	let namel = formData.get('name');
	let passwordl = formData.get('password');
    let data = {
            name: namel,
            password: passwordl
    }
let link = 'https://localhost:7055/api/User/login'
fetch('https://localhost:7055/api/User/login', {
    method: "POST",
    body: JSON.stringify(data),
    headers:{
        "Content-type": "application/json"
    }
})
.then((resu) => resu.json())
.then((datas) => {
    setCookie('userId', datas.id);
    setCookie('name', datas.name);
    setCookie('password', document.getElementById('password').value);
    console.log('SUCCES');
    console.log(getCookie('userId'));
    window.location.reload();
})
})
};