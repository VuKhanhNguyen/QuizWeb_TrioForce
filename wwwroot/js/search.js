// Global Search Functionality
document.addEventListener('DOMContentLoaded', function () {
    const searchInput = document.getElementById('globalSearch');
    
    if (!searchInput) return;

    // Debounce function to limit API calls
    function debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }

    // Handle search based on current page
    function handleSearch(searchTerm) {
        if (!searchTerm.trim()) {
            resetSearch();
            return;
        }

        const currentPath = window.location.pathname.toLowerCase();
        
        // Ranking page search
        if (currentPath.includes('/ranking')) {
            searchRankingTable(searchTerm);
        }
        // Profile page search
        else if (currentPath.includes('/profile')) {
            // Can be extended for profile search if needed
            console.log('Profile search:', searchTerm);
        }
        // Quiz page search
        else if (currentPath.includes('/quiz')) {
            searchQuizTable(searchTerm);
        }
        // Default: Search all tables on current page
        else {
            searchGenericTable(searchTerm);
        }
    }

    // Search in ranking table
    function searchRankingTable(searchTerm) {
        const rows = document.querySelectorAll('.ranking-table tbody tr');
        let visibleCount = 0;

        rows.forEach(row => {
            const username = row.querySelector('.username-col')?.textContent.toLowerCase() || '';
            const rank = row.querySelector('.rank-col')?.textContent.toLowerCase() || '';
            
            if (username.includes(searchTerm.toLowerCase()) || rank.includes(searchTerm.toLowerCase())) {
                row.style.display = '';
                visibleCount++;
            } else {
                row.style.display = 'none';
            }
        });

        showSearchResultMessage(visibleCount, rows.length);
    }

    // Search in quiz/question set tables
    function searchQuizTable(searchTerm) {
        const rows = document.querySelectorAll('table tbody tr');
        let visibleCount = 0;

        rows.forEach(row => {
            const text = row.textContent.toLowerCase();
            
            if (text.includes(searchTerm.toLowerCase())) {
                row.style.display = '';
                visibleCount++;
            } else {
                row.style.display = 'none';
            }
        });

        showSearchResultMessage(visibleCount, rows.length);
    }

    // Generic search for any table
    function searchGenericTable(searchTerm) {
        const tables = document.querySelectorAll('table');
        
        if (tables.length === 0) {
            console.log('No tables found on this page');
            return;
        }

        let totalVisible = 0;
        let totalRows = 0;

        tables.forEach(table => {
            const rows = table.querySelectorAll('tbody tr');
            totalRows += rows.length;

            rows.forEach(row => {
                const text = row.textContent.toLowerCase();
                
                if (text.includes(searchTerm.toLowerCase())) {
                    row.style.display = '';
                    totalVisible++;
                } else {
                    row.style.display = 'none';
                }
            });
        });

        showSearchResultMessage(totalVisible, totalRows);
    }

    // Reset search - show all rows
    function resetSearch() {
        const allRows = document.querySelectorAll('table tbody tr');
        allRows.forEach(row => {
            row.style.display = '';
        });
        removeSearchResultMessage();
    }

    // Show search result message
    function showSearchResultMessage(visibleCount, totalCount) {
        removeSearchResultMessage();

        const message = document.createElement('div');
        message.className = 'search-result-message';
        message.innerHTML = `
            <i class="fa-solid fa-search"></i>
            Tìm thấy <strong>${visibleCount}</strong> / ${totalCount} kết quả
        `;

        const container = document.querySelector('.ranking-container, .container');
        if (container && container.firstChild) {
            container.insertBefore(message, container.firstChild);
        }
    }

    // Remove search result message
    function removeSearchResultMessage() {
        const existingMessage = document.querySelector('.search-result-message');
        if (existingMessage) {
            existingMessage.remove();
        }
    }

    // Event listeners
    searchInput.addEventListener('input', debounce(function(e) {
        handleSearch(e.target.value);
    }, 300));

    // Clear search on Escape key
    searchInput.addEventListener('keydown', function(e) {
        if (e.key === 'Escape') {
            searchInput.value = '';
            resetSearch();
        }
    });

    // Clear search when input is cleared
    searchInput.addEventListener('search', function(e) {
        if (e.target.value === '') {
            resetSearch();
        }
    });

    document.addEventListener('keydown', function(e) {
        if (e.altKey && e.key.toLowerCase() === 's') {
            e.preventDefault();
            searchInput.focus();
            searchInput.select();
        }
    });
});
