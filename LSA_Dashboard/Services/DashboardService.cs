#pragma warning disable CS8603, CS8629
using LSA_Dashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace LSA_Dashboard.Services
{
    public interface IDashboardService
    {
        Task<decimal> GetTotalMRRAsync();
        // Yeni metodumuzu arayüze ekliyoruz
        Task<object> GetCustomerHealthScoreAsync(int organizationId);
        Task<IEnumerable<object>> GetAllCustomersAsync();
        Task<object> GetCustomerUsageStatsAsync(int organizationId);
        Task<object> GetTimeAnalyticsAsync(string segment, string sector);
        Task<object> GetBillingIntelligenceAsync(int organizationId);
        Task<object> GetExpansionIntelligenceAsync(int organizationId);
        Task<object> GetUserBehaviorAsync(int organizationId);
        Task<object> GetAlertsAsync(int organizationId);
        Task<object> GetExecutiveKpisAsync(int organizationId);
    }

    public class DashboardService : IDashboardService
    {
        private readonly LsaDbContext _context;

        public DashboardService(LsaDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> GetTotalMRRAsync()
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            return await _context.CrmInvoices
                .Where(i => i.BillingPeriod.HasValue &&
                            i.BillingPeriod.Value.Month == currentMonth &&
                            i.BillingPeriod.Value.Year == currentYear)
                .SumAsync(i => i.FixedFee ?? 0);
        }

        // Yeni Health Score Metodumuz
        public async Task<object> GetCustomerHealthScoreAsync(int organizationId)
        {
            var random = new Random(); // Gerçek DB bağlanana kadar test verisi

            // 1. KULLANIM SAĞLIĞI (Maksimum 35 Puan)
            int activeUserRate = random.Next(30, 100); // %30 ile %100 arası aktiflik
            int modulesUsed = random.Next(1, 6); // 1 ile 5 modül kullanımı
            int usageTrend = random.Next(-10, 20); // Ay bazlı artış/azalış

            int usageScore = 35;
            if (activeUserRate < 50) usageScore -= 15;
            else if (activeUserRate < 75) usageScore -= 5;
            if (modulesUsed <= 2) usageScore -= 10;
            if (usageTrend < 0) usageScore -= 5;

            // 2. DESTEK SAĞLIĞI (Maksimum 30 Puan)
            int ticketCount = random.Next(0, 25);
            bool hasSlaBreach = random.NextDouble() > 0.85; // %15 ihtimalle SLA ihlali
            int csatScore = random.Next(50, 100); // 100 üzerinden memnuniyet

            int supportScore = 30;
            if (ticketCount > 15) supportScore -= 10;
            if (hasSlaBreach) supportScore -= 15; // SLA ihlali çok kritik
            if (csatScore < 70) supportScore -= 10;

            // 3. İŞ SAĞLIĞI (Maksimum 35 Puan)
            int daysToRenew = random.Next(10, 300); // Sözleşme bitimine kalan gün
            bool paymentDelayed = random.NextDouble() > 0.9; // %10 ihtimalle ödeme gecikmesi

            int businessScore = 35;
            if (daysToRenew < 45) businessScore -= 15; // Bitime az kaldıysa risk
            if (paymentDelayed) businessScore -= 20; // Ödeme yapmıyorsa çok yüksek risk

            // GENEL HESAPLAMALAR
            int totalHealthScore = usageScore + supportScore + businessScore;
            totalHealthScore = Math.Clamp(totalHealthScore, 0, 100); // Skoru 0-100 arasında tut

            // Risk Durumu Kategorizasyonu
            string riskStatus = totalHealthScore >= 80 ? "Düşük" : (totalHealthScore >= 50 ? "Orta" : "Yüksek");

            // Yapay Zeka Tahminleri (Score'a ters ve doğru orantılı sahte tahmin motoru)
            double churnPrediction = Math.Clamp(100 - totalHealthScore + (random.NextDouble() * 10 - 5), 1, 99);
            double renewalProbability = Math.Clamp(totalHealthScore + (random.NextDouble() * 10 - 5), 1, 99);

            return new
            {
                OrganizationId = organizationId,
                OverallScore = totalHealthScore,
                RiskStatus = riskStatus,
                Predictions = new
                {
                    ChurnRate = Math.Round(churnPrediction, 1),
                    RenewalRate = Math.Round(renewalProbability, 1)
                },
                Breakdown = new
                {
                    Usage = new { Score = usageScore, ActiveRate = activeUserRate, Modules = modulesUsed },
                    Support = new { Score = supportScore, Tickets = ticketCount, SlaBreach = hasSlaBreach, Csat = csatScore },
                    Business = new { Score = businessScore, DaysLeft = daysToRenew, PaymentDelayed = paymentDelayed }
                }
            };
        }
        public async Task<IEnumerable<object>> GetAllCustomersAsync()
        {
            // Müşterileri, sektörlerini ve aktif sözleşmelerini çekiyoruz
            var customers = await _context.CrmClientDetails
                .Include(c => c.Sector)
                .Include(c => c.Segment)
                .Include(c => c.CrmContracts)
                .ToListAsync();

            // Veriyi arayüzde kolayca gösterebileceğimiz bir modele (Anonim tip) çeviriyoruz
            var result = customers.Select(c => {
                var activeContract = c.CrmContracts.FirstOrDefault(contract => contract.IsActive == true);

                return new
                {
                    OrganizationId = c.OrganizationId,
                    // Mevcut yapında 'Organizations' tablosu LsaDbContext'te map'li ise c.Organization.Name yazabilirsin.
                    // Şimdilik test için ID'yi stringe çeviriyoruz veya dummy bir isim veriyoruz.
                    CustomerName = $"Müşteri {c.OrganizationId} A.Ş.",
                    AccountOwnerId = c.AccountOwnerUserId,
                    SectorName = c.Sector?.Name ?? "Belirtilmemiş",
                    SegmentName = c.Segment?.Name ?? "-",
                    CountryRegion = $"{c.Country} / {c.Region}",
                    ContractType = activeContract?.ContractType ?? "Yok",
                    StartDate = activeContract?.StartDate?.ToString("dd.MM.yyyy") ?? "-",
                    EndDate = activeContract?.EndDate?.ToString("dd.MM.yyyy") ?? "-",
                    AutoRenew = activeContract?.AutoRenew == true ? "Evet" : "Hayır",
                    PaymentMethod = activeContract?.PaymentMethod ?? "-"
                };
            });

            return result;
        }
        public async Task<object> GetCustomerUsageStatsAsync(int organizationId)
        {
            // GERÇEK SENARYODA YAPILACAKLAR:
            // 1. var users = await _context.Users.Where(u => u.OrganizationId == organizationId).ToListAsync();
            // 2. var records = await _context.Records.Where(r => users.Select(u => u.Id).Contains(r.UserId)).ToListAsync();
            // 3. Bu verileri LINQ ile gruplayarak aşağıdaki yapıya dönüştür.

            // ŞİMDİLİK: API'nin arayüzle konuşmasını sağlamak için resmi kılavuzuna uygun mock veri dönüyoruz.
            var random = new Random();
            int totalUsers = random.Next(10, 150);
            int activeUsers = (int)(totalUsers * (random.Next(40, 95) / 100.0));

            return new
            {
                OrganizationId = organizationId,
                Metrics = new
                {
                    TotalUsers = totalUsers,
                    ActiveUsers = activeUsers,
                    ActiveUserRatio = Math.Round(((double)activeUsers / totalUsers) * 100, 1),
                    NewUsersMonthly = random.Next(0, 10),
                    DeletedUsersMonthly = random.Next(0, 3),
                    AvgUsageTimeMinutes = random.Next(15, 120)
                },
                ProcessSteps = new[]
                {
            new { Step = "1) Selamlama", UsageCount = random.Next(800, 1500) },
            new { Step = "2) İhtiyaç Belirleme", UsageCount = random.Next(600, 1200) },
            new { Step = "3) Tanıtım ve Sunum", UsageCount = random.Next(500, 1000) },
            new { Step = "4) İtiraz Karşılama", UsageCount = random.Next(300, 800) },
            new { Step = "5) Büyüme - Çapraz Satış", UsageCount = random.Next(100, 400) },
            new { Step = "6) Satış Kapama", UsageCount = random.Next(200, 700) },
            new { Step = "7) Takip", UsageCount = random.Next(50, 300) }
        }.OrderByDescending(x => x.UsageCount).ToList(),

                // En çok ve en az kullanılanları otomatik bul
                DeviceSplit = new
                {
                    AndroidRatio = random.Next(30, 80),
                    // IOS oranı kalanı olacak şekilde hesaplanır
                }
            };
        } // <--- İŞTE EKSİK OLAN PARANTEZ BURADA! (GetCustomerUsageStatsAsync metodunu kapatır)

        public async Task<object> GetTimeAnalyticsAsync(string segment, string sector)
        {
            // GERÇEK SENARYO: 
            // var logs = await _context.UserLogs.Include(u => u.User.Organization)
            //    .Where(l => l.User.Organization.Segment == segment && l.User.Organization.Sector == sector).ToListAsync();
            // Ardından GroupBy(l => new { l.Date.DayOfWeek, l.Date.Hour }) ile ısı haritası çıkarılır.

            var random = new Random();

            // Segment'e göre kullanım senaryoları (B2B mesai saatlerinde, B2C akşamları yoğun)
            int kpiMultiplier = segment == "B2B" ? 2 : 1;

            // 1. KPI Verileri
            var kpis = new
            {
                MonthlyTotalHours = random.Next(300, 800) * kpiMultiplier,
                YearlyTotalHours = random.Next(4000, 9000) * kpiMultiplier,
                LastActive = $"{random.Next(1, 45)} dakika önce",
                LoginFrequency = $"{Math.Round(random.NextDouble() * 3 + 2, 1)} kez / hafta"
            };

            // 2. Ay Bazlı Trend (Son 6 Ay)
            var months = new[] { "Kasım", "Aralık", "Ocak", "Şubat", "Mart", "Nisan" };
            var trendData = months.Select(m => new { Month = m, Usage = random.Next(200, 600) * kpiMultiplier }).ToList();

            // 3. Gün/Saat Isı Haritası Verisi (7 Gün x 24 Saat)
            var heatmap = new List<object>();
            string[] days = { "Pzt", "Sal", "Çar", "Per", "Cum", "Cmt", "Paz" };

            for (int dayIndex = 0; dayIndex < 7; dayIndex++)
            {
                for (int hour = 0; hour < 24; hour++)
                {
                    int intensity = 0;
                    bool isWeekend = (dayIndex == 5 || dayIndex == 6);

                    // B2B Senaryosu (Hafta içi 09:00 - 17:00 arası çok yoğun)
                    if (segment == "B2B")
                    {
                        if (!isWeekend && hour >= 9 && hour <= 17) intensity = random.Next(60, 100);
                        else if (!isWeekend && (hour == 8 || hour == 18)) intensity = random.Next(20, 50);
                        else intensity = random.Next(0, 10);
                    }
                    // B2C Senaryosu (Akşam 18:00 - 23:00 ve Hafta sonları yoğun)
                    else
                    {
                        if (hour >= 18 && hour <= 23) intensity = random.Next(50, 100);
                        else if (isWeekend && hour >= 10 && hour <= 23) intensity = random.Next(60, 100);
                        else intensity = random.Next(5, 30);
                    }

                    heatmap.Add(new { Day = days[dayIndex], Hour = hour, Intensity = intensity });
                }
            }

            return new
            {
                Kpis = kpis,
                Trend = trendData,
                Heatmap = heatmap
            };
        }
        public async Task<object> GetBillingIntelligenceAsync(int organizationId)
        {
            // GERÇEK SENARYODA:
            // 1. O şirketin aktif kullanıcı sayısını çekersin (_context.Users.Count(u => u.OrgId == orgId && u.IsActive))
            // 2. O şirketin bu ayki toplam kullanım saatini loglardan çekersin (örneğin TotalMinutes / 60)
            // 3. Fatura tablosundan bu ayki durumunu (Ödendi/Ödenmedi) kontrol edersin.

            var random = new Random();

            // Müşterinin büyüklüğüne göre (Enterprise vs SMB) sahte veri kurgusu
            int activeUsers = organizationId == 1 ? random.Next(80, 150) : random.Next(10, 40);
            int averageHoursPerUser = random.Next(15, 60);
            int totalUsageHours = activeUsers * averageHoursPerUser;

            // Fiyatlandırma Kuralları (Kılavuza göre)
            decimal fixedFeePerUser = 10m; // Kişi başı sabit $10
            decimal hourlyRate = 1.5m;     // Saat başı $1.5

            // MRR Hesaplamaları
            decimal fixedMrr = activeUsers * fixedFeePerUser;
            decimal usageMrr = totalUsageHours * hourlyRate;
            decimal totalMrr = fixedMrr + usageMrr;
            decimal totalArr = totalMrr * 12; // Yıllık öngörülen gelir
            decimal arpu = totalMrr / activeUsers; // Kullanıcı başı ortalama gelir (Average Revenue Per User)

            // Trend Verisi (Son 12 Ayın ARPU Gelişimi - Büyüme gösteren bir grafik için)
            var arpuTrend = new List<object>();
            string[] months = { "May", "Haz", "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara", "Oca", "Şub", "Mar", "Nis" };
            decimal startArpu = arpu * 0.6m; // 12 ay önce biraz daha düşüktü
            for (int i = 0; i < 12; i++)
            {
                arpuTrend.Add(new { Month = months[i], Value = Math.Round(startArpu, 2) });
                startArpu += (arpu - startArpu) / (12 - i); // Yavaş yavaş güncel değere yaklaşsın
            }

            // Akıllı Uyarılar ve Durumlar
            bool isUsageSpiking = averageHoursPerUser > 45; // Kullanım aniden arttıysa uyarı ver
            bool isAutoInvoiceEnabled = random.NextDouble() > 0.2; // %80 ihtimalle otomatik fatura açık
            string invoiceStatus = random.NextDouble() > 0.1 ? "Paid" : "Unpaid"; // %10 ihtimalle ödenmedi

            string collectionRisk = "Düşük";
            if (invoiceStatus == "Unpaid") collectionRisk = "Yüksek";
            else if (!isAutoInvoiceEnabled) collectionRisk = "Orta"; // Otomatik ödeme yoksa riskli

            return new
            {
                OrganizationId = organizationId,
                Revenue = new
                {
                    TotalMrr = totalMrr,
                    TotalArr = totalArr,
                    FixedMrr = fixedMrr,
                    UsageMrr = usageMrr,
                    Arpu = Math.Round(arpu, 2)
                },
                Usage = new
                {
                    ActiveUsers = activeUsers,
                    TotalHours = totalUsageHours,
                    IsSpiking = isUsageSpiking
                },
                Billing = new
                {
                    AutoInvoice = isAutoInvoiceEnabled,
                    Status = invoiceStatus,
                    Risk = collectionRisk
                },
                ArpuTrend = arpuTrend
            };
        }
        public async Task<object> GetExpansionIntelligenceAsync(int organizationId)
        {
            var random = new Random();

            // 1. Temel Büyüme Metrikleri (Sahte Veri)
            int userGrowthRate = random.Next(5, 45); // %5 ile %45 arası büyüme
            int modulesUsed = random.Next(1, 5); // 5 modülden kaçı kullanılıyor
            int currentUsers = random.Next(50, 200);

            // 2. AI Tahminleri (Upsell ve Cross-sell)
            string upsellPotential = userGrowthRate > 25 ? "Yüksek" : (userGrowthRate > 10 ? "Orta" : "Düşük");
            string crossSellPotential = modulesUsed < 3 ? "Yüksek" : (modulesUsed == 3 ? "Orta" : "Düşük");

            // AI Öngörüsü (Ne kadar yeni lisans alınabilir?)
            int projectedNewUsers = (int)(currentUsers * (userGrowthRate / 100.0) * 0.8); // Büyüme hızının %80'i kadar yeni lisans öngörüsü
            string aiPrediction = projectedNewUsers > 10
                ? $"Önümüzdeki 3 ay içinde +{projectedNewUsers} yeni lisans ihtiyacı öngörülüyor."
                : "Mevcut lisans sayısı önümüzdeki çeyrek için yeterli görünüyor.";

            // 3. Departman Genişlemesi
            var allDepartments = new List<string> { "Satış", "Pazarlama", "Müşteri Hizmetleri", "İK", "Finans" };
            // Kullanılan modül sayısına göre aktif departmanları seç
            var activeDepartments = allDepartments.Take(modulesUsed).ToList();
            // Geri kalanları "Yeni Eklenebilecek" olarak işaretle
            var potentialDepartments = allDepartments.Skip(modulesUsed).ToList();

            // 4. Son 6 Ay Kullanıcı Artış Grafiği Verisi
            var growthChart = new List<object>();
            string[] months = { "Kasım", "Aralık", "Ocak", "Şubat", "Mart", "Nisan" };
            int historyUsers = currentUsers - (currentUsers * userGrowthRate / 100); // 6 ay önceki sayı

            for (int i = 0; i < 6; i++)
            {
                growthChart.Add(new { Month = months[i], Users = historyUsers });
                historyUsers += (currentUsers - historyUsers) / (6 - i); // Eğimi yavaşça güncele ulaştır
            }

            return new
            {
                OrganizationId = organizationId,
                GrowthMetrics = new
                {
                    GrowthRate = userGrowthRate,
                    CurrentModules = modulesUsed,
                    CurrentUsers = currentUsers
                },
                Intelligence = new
                {
                    Upsell = upsellPotential,
                    CrossSell = crossSellPotential,
                    PredictionText = aiPrediction,
                    ActiveDepartments = activeDepartments,
                    PotentialDepartments = potentialDepartments
                },
                ChartData = growthChart
            };
        }
        public async Task<object> GetUserBehaviorAsync(int organizationId)
        {
            var random = new Random();

            // KPI'lar
            int aiCoachAdherence = random.Next(40, 95); // AI önerilerine uyum %
            int wonDeals = random.Next(15, 120); // Kazanılan fırsat sayısı
            int demoWatchRate = random.Next(30, 90); // Müşteriye gönderilen demoların izlenme oranı %

            // Satış Süreci Adımları (Huni mantığı, giderek azalır)
            int step1 = 100; // Selamlama
            int step2 = random.Next(75, 95); // İhtiyaç Analizi
            int step3 = random.Next(50, step2); // Tanıtım
            int step4 = random.Next(30, step3); // İtiraz Karşılama
            int step5 = random.Next(10, step4); // Kapanış

            var processSteps = new List<object>
    {
        new { Step = "1. Selamlama", Rate = step1 },
        new { Step = "2. İhtiyaç Analizi", Rate = step2 },
        new { Step = "3. Tanıtım/Sunum", Rate = step3 },
        new { Step = "4. İtiraz Karşılama", Rate = step4 },
        new { Step = "5. Kapanış", Rate = step5 }
    };

            // Kayıp Nedenleri (Toplamı 100 olacak şekilde dağıt)
            int reason1 = random.Next(30, 50); // Fiyat
            int reason2 = random.Next(20, 35); // Rakip
            int reason3 = random.Next(10, 20); // Özellik Eksikliği
            int reason4 = 100 - (reason1 + reason2 + reason3); // Zamanlama/Kararsızlık

            var lostReasons = new List<object>
    {
        new { Reason = "Bütçe / Fiyat", Percentage = reason1 },
        new { Reason = "Rakip Tercihi", Percentage = reason2 },
        new { Reason = "Özellik Eksikliği", Percentage = reason3 },
        new { Reason = "Zamanlama / Askıya Alma", Percentage = reason4 }
    };

            return new
            {
                OrganizationId = organizationId,
                Kpis = new
                {
                    AiAdherence = aiCoachAdherence,
                    WonOpportunities = wonDeals,
                    DemoCompletion = demoWatchRate
                },
                Funnel = processSteps,
                LostReasons = lostReasons
            };
        }
        public async Task<object> GetAlertsAsync(int organizationId)
        {
            var random = new Random();
            var alerts = new List<object>();

            // 1. Kullanım Düşüşü Uyarısı
            int usageDrop = random.Next(0, 60);
            if (usageDrop > 40)
                alerts.Add(new { Id = 1, Severity = "High", Title = "Kullanıcı Sayısında Ani Azalma", Message = $"Son 7 günde aktif kullanımda %{usageDrop} düşüş tespit edildi.", Time = "1 saat önce" });
            else if (usageDrop > 20)
                alerts.Add(new { Id = 2, Severity = "Medium", Title = "Kullanım Düşüşü Uyarısı", Message = $"Haftalık kullanım oranında %{usageDrop} azalma var.", Time = "3 saat önce" });

            // 2. Sözleşme Bitimi Alarmları (60-30-7 gün)
            int daysLeft = random.Next(2, 90);
            if (daysLeft <= 7)
                alerts.Add(new { Id = 3, Severity = "High", Title = "Sözleşme Bitiş Alarmı", Message = $"Sözleşmenin bitimine sadece {daysLeft} gün kaldı. Acil aksiyon alınmalı.", Time = "5 saat önce" });
            else if (daysLeft <= 30)
                alerts.Add(new { Id = 4, Severity = "Medium", Title = "Yenileme Dönemi Yaklaşıyor", Message = $"Sözleşmenin bitimine {daysLeft} gün kaldı.", Time = "1 gün önce" });
            else if (daysLeft <= 60)
                alerts.Add(new { Id = 5, Severity = "Info", Title = "Sözleşme Yenileme Hatırlatması", Message = $"Sözleşme {daysLeft} gün sonra sona eriyor.", Time = "2 gün önce" });

            // 3. Fatura ve Churn Uyarısı
            bool isInvoiceUnpaid = random.NextDouble() > 0.8;
            if (isInvoiceUnpaid)
                alerts.Add(new { Id = 6, Severity = "High", Title = "Fatura Ödenmedi Uyarısı", Message = "Bu aya ait abonelik faturası son ödeme tarihini geçti.", Time = "1 gün önce" });

            int churnRisk = random.Next(10, 85);
            if (churnRisk > 70)
                alerts.Add(new { Id = 7, Severity = "High", Title = "Churn Riski Artışı", Message = $"Müşterinin iptal riski %{churnRisk} seviyesine ulaştı.", Time = "2 gün önce" });

            // 4. Bilgi / Pozitif Gelişmeler
            int newUsers = random.Next(0, 5);
            if (newUsers > 0)
                alerts.Add(new { Id = 8, Severity = "Success", Title = "Yeni Kullanıcı Eklenmesi", Message = $"Sisteme {newUsers} yeni kullanıcı lisansı tanımlandı.", Time = "Az önce" });

            // Tarihe/Öneme göre sırala
            var sortedAlerts = alerts.OrderBy(a =>
                ((dynamic)a).Severity == "High" ? 1 :
                ((dynamic)a).Severity == "Medium" ? 2 : 3).ToList();

            return new
            {
                OrganizationId = organizationId,
                TotalAlerts = sortedAlerts.Count,
                CriticalAlerts = sortedAlerts.Count(a => ((dynamic)a).Severity == "High"),
                Alerts = sortedAlerts
            };
        }
        public async Task<object> GetExecutiveKpisAsync(int organizationId)
        {
            var random = new Random();

            // Yönetim Katmanı Metrikleri (Gerçek DB'ye bağlamadan önceki son mock veriler)
            int cacPaybackMonths = random.Next(6, 18); // İdeal SaaS için 12 ay altı istenir
            double logoChurn = Math.Round(random.NextDouble() * 3 + 1, 1); // %1.0 ile %4.0 arası (Müşteri sayısı kaybı)
            double revenueChurn = Math.Round(logoChurn * 0.8, 1); // Ciro kaybı genelde logo kaybından bir tık azdır
            int expansionRevenue = random.Next(5000, 25000); // Upsell/Cross-sell'den gelen ciro

            // NRR (Net Revenue Retention): %100'ün üstü iyidir, şirket kendi kendine büyüyor demektir.
            int nrr = random.Next(95, 125);

            int grossMargin = random.Next(75, 92); // Brüt Kar Marjı
            int ltv = random.Next(15000, 60000); // Lifetime Value ($)

            return new
            {
                OrganizationId = organizationId,
                Kpis = new
                {
                    CacPayback = cacPaybackMonths,
                    LogoChurnRate = logoChurn,
                    RevenueChurn = revenueChurn,
                    ExpansionRevenue = expansionRevenue,
                    Nrr = nrr,
                    GrossMargin = grossMargin,
                    Ltv = ltv
                }
            };
        }
    } // DashboardService sınıfını kapatır
} // namespace LSA_Dashboard.Services kısmını kapatır