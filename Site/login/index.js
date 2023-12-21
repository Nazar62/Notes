const loginForm = document.getElementById('loginForm');
loginForm.addEventListener('submit', function (e) {
	e.preventDefault();
	const formData = new FormData(loginForm);
	let name = formData.get('name');
	let password = formData.get('password');
let link = 'http://localhost'
	fetch(link, {
		method: 'POST',
		body: {
			Name: name,
			Password: password
		}
	}).then(res => res.json())
	.then(data => console.log(data))
	.catch(error => console.log(error));
});