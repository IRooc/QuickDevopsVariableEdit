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
    function removeAllSavedClasses() {
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
    removeAllSavedClasses();
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
{
    const saveAll = document.getElementById('saveAll');
    if (saveAll) {

        saveAll.addEventListener('click', () => {
            const vars = {};
            document.querySelectorAll('input.value').forEach(el => {
                vars[el.previousElementSibling.getAttribute('value')] = el.getAttribute('value');
            });
            const body = {
                groupId: new Number(document.querySelector('input[name=groupid]').value),
                variables: vars
            }
            fetch('?handler=SaveAll', {
                method: 'Post',
                body: JSON.stringify(body),
                headers: {
                    'Content-Type': 'application/json; charset=utf-8',
                    RequestVerificationToken: document.querySelector('input[name=__RequestVerificationToken]').value,
                }
            }).then(response => response.json())
                .then(json => {
                    if (json.success) {
                        document.querySelectorAll('tr').forEach(tr => {
                            tr.classList.add('saved');
                        })
                        removeAllSavedClasses();
                    }
                });
        });
    }
}