// Backend API adresimiz (Kendi portunla aynı olduğundan emin ol)
const API_BASE_URL = '/api/dashboard';

document.addEventListener('DOMContentLoaded', () => {
    fetchMRR();
    fetchHealthScore(1); // 1 ID'li Enterprise müşterisini test ediyoruz
    renderChart();
});

// Toplam MRR Çekme
async function fetchMRR() {
    try {
        const response = await fetch(`${API_BASE_URL}/mrr`);
        const result = await response.json();

        if (result.success) {
            // Rakamı dolar formatına çeviriyoruz
            document.getElementById('mrrValue').innerText = `$${result.totalMRR.toLocaleString()}`;
            document.getElementById('mrrMonth').innerText = `Dönem: ${result.month}`;
        }
    } catch (error) {
        console.error("MRR Verisi Çekilemedi:", error);
        document.getElementById('mrrValue').innerText = "Hata!";
    }
}

// Sağlık Skoru Çekme
async function fetchHealthScore(orgId) {
    try {
        const response = await fetch(`${API_BASE_URL}/health/${orgId}`);
        const result = await response.json();

        if (result.success) {
            const data = result.data;
            document.getElementById('healthScore').innerText = `${data.overallScore} / 100`;

            // Risk Durumuna göre Badge Rengini Ayarla
            const badge = document.getElementById('riskStatus');
            badge.innerText = data.riskStatus;

            if (data.healthScore >= 80) badge.className = 'badge low-risk';
            else if (data.healthScore >= 50) badge.className = 'badge mid-risk';
            else badge.className = 'badge high-risk';

            // Ödenmemiş fatura uyarısı
            if (data.unpaidInvoiceCount > 0) {
                document.getElementById('unpaidCount').innerText = `${data.unpaidInvoiceCount} Ödenmemiş Fatura!`;
            }
        }
    } catch (error) {
        console.error("Health Score Verisi Çekilemedi:", error);
        document.getElementById('healthScore').innerText = "Hata!";
    }
}

// Chart.js ile Örnek Grafik Çizimi
function renderChart() {
    const ctx = document.getElementById('growthChart').getContext('2d');
    new Chart(ctx, {
        type: 'line',
        data: {
            labels: ['Ocak', 'Şubat', 'Mart', 'Nisan', 'Mayıs', 'Haziran'],
            datasets: [{
                label: 'MRR Büyümesi ($)',
                data: [500, 600, 800, 1000, 1200, 1500],
                borderColor: '#7b68a3',
                backgroundColor: 'rgba(123, 104, 163, 0.2)',
                borderWidth: 2,
                fill: true,
                tension: 0.4 // Çizgiyi yumuşatır (kavisli yapar)
            }]
        },
        options: {
            responsive: true,
            plugins: { legend: { display: false } },
            scales: { y: { beginAtZero: true } }
        }
    });
}