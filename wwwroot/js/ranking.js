
function scrollToCurrentUser() {
    const currentUserRow = document.querySelector('.current-user');
    if (currentUserRow) {
        const tbody = document.querySelector('.ranking-tbody-wrapper');
        const rowTop = currentUserRow.offsetTop;
        const rowHeight = currentUserRow.offsetHeight;
        const tbodyHeight = tbody.offsetHeight;
        
       
        tbody.scrollTo({
            top: rowTop - (tbodyHeight / 2) + (rowHeight / 2),
            behavior: 'smooth'
        });
    }
}


