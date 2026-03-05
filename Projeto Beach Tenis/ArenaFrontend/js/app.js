// Common UI Functions
document.addEventListener('DOMContentLoaded', () => {
    // ---- Dashboard Logic ----
    if (window.location.pathname.endsWith('index.html') || window.location.pathname === '/') {
        loadDashboardData();
    }
});

// Utility to toggle modals
function openModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) modal.classList.add('active');
}

function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) modal.classList.remove('active');
}

// Format Currency
function formatCurrency(value) {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
}

// Format Date
function formatDate(dateStr) {
    if (!dateStr) return '-';
    const d = new Date(dateStr);
    return new Intl.DateTimeFormat('pt-BR').format(d);
}

// Dashboard Data Loading
async function loadDashboardData() {
    try {
        // Fetch sizes
        const students = await api.getStudents();
        const reservations = await api.getReservations();
        const sales = await api.getSales();

        // Update Stats
        document.getElementById('totalStudents').innerText = students.length || 0;

        const today = new Date().toISOString().split('T')[0];
        const todaysReservations = reservations.filter(r => r.reservationDate && r.reservationDate.startsWith(today));
        document.getElementById('todayReservations').innerText = todaysReservations.length || 0;

        const todaysSales = sales.filter(s => s.saleDate && s.saleDate.startsWith(today));
        const totalSales = todaysSales.reduce((sum, s) => sum + (s.totalAmount || 0), 0);
        document.getElementById('todaySales').innerText = formatCurrency(totalSales);

        // Populate Table
        const tbody = document.getElementById('nextReservationsBody');
        if (tbody) {
            tbody.innerHTML = '';

            if (todaysReservations.length === 0) {
                tbody.innerHTML = '<tr><td colspan="4" style="text-align: center; color: var(--text-muted);">Nenhuma reserva para hoje</td></tr>';
            } else {
                todaysReservations.slice(0, 5).forEach(r => {
                    const tr = document.createElement('tr');
                    tr.innerHTML = `
                        <td>${r.startTime.substring(0, 5)} - ${r.endTime.substring(0, 5)}</td>
                        <td>Quadra ${r.courtId}</td>
                        <td>${r.customerName}</td>
                        <td><span style="color: var(--accent);">Confirmada</span></td>
                    `;
                    tbody.appendChild(tr);
                });
            }
        }
    } catch (error) {
        console.error('Erro ao carregar dados do dashboard:', error);
    }
}
