
{
    //remember url and key
    const gotoEditButton = document.getElementById('gotoEdit');
    if (gotoEditButton) {
        gotoEditButton.addEventListener('click', () => {
            const newUrl = document.querySelector('input[name="url"]').value;
            const newToken = document.querySelector('input[name="accessToken"]').value
            localStorage.setItem('url', newUrl);
            localStorage.setItem('token', newToken);

        });
        const token = localStorage.getItem('token');
        const url = localStorage.getItem('url');
        document.querySelector('input[name="url"]').value = url;
        document.querySelector('input[name="accessToken"]').value = token;
    }
}


{
    //remove saved class
    const allSaved = document.querySelectorAll('tr.saved');
    if (allSaved.length > 0) {
        console.log('saved');
        for (let i = 0; i < allSaved.length; i++) {
            const saved = allSaved[i];
            setTimeout(() => {
                saved.className = '';
            }, 2000);
        }
    }
}
{
    //add ctrl+click to password fields to show/hide them
    const pwInputs = document.querySelectorAll('input[type="password"]');
    for (let i = 0; i < pwInputs.length; i++) {
        const input = pwInputs[i];
        input.title = 'Ctrl+click to show'
        input.addEventListener('click', (e) => {
            if (e.ctrlKey) {
                e.target.type = e.target.type == 'text' ? 'password' : 'text';
            }
        });
    }
}