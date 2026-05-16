using Microsoft.AspNetCore.Mvc;
using LSA_Dashboard.Services;

namespace LSA_Dashboard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        // Servisimizi Controller'a enjekte ediyoruz (Dependency Injection)
        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("mrr")]
        public async Task<IActionResult> GetTotalMrr()
        {
            try
            {
                // Yazdığımız MRR hesaplama metodunu çağırıyoruz
                var mrr = await _dashboardService.GetTotalMRRAsync();

                // Sonucu JSON formatında geri döndürüyoruz
                return Ok(new
                {
                    Success = true,
                    Month = DateTime.Now.ToString("MMMM yyyy"),
                    TotalMRR = mrr
                });
            }
            catch (Exception ex)
            {
                // Olası bir hatada uygulamanın çökmemesi için hata mesajı dönüyoruz
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }
        [HttpGet("health/{orgId}")]
        public async Task<IActionResult> GetCustomerHealth(int orgId)
        {
            try
            {
                var result = await _dashboardService.GetCustomerHealthScoreAsync(orgId);

                if (result == null)
                    return NotFound(new { Success = false, Message = "Müşteri bulunamadı." });

                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }
        [HttpGet("customers")]
        public async Task<IActionResult> GetAllCustomers()
        {
            try
            {
                var customers = await _dashboardService.GetAllCustomersAsync();
                return Ok(new { Success = true, Data = customers });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }
        [HttpGet("customer/{orgId}/usage")]
        public async Task<IActionResult> GetCustomerUsage(int orgId)
        {
            try
            {
                var result = await _dashboardService.GetCustomerUsageStatsAsync(orgId);
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }
        [HttpGet("analytics")]
        public async Task<IActionResult> GetAnalytics([FromQuery] string segment = "B2B", [FromQuery] string sector = "Tümü")
        {
            try
            {
                var result = await _dashboardService.GetTimeAnalyticsAsync(segment, sector);
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }
        [HttpGet("customer/{orgId}/billing")]
        public async Task<IActionResult> GetBillingIntelligence(int orgId)
        {
            try
            {
                var result = await _dashboardService.GetBillingIntelligenceAsync(orgId);
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }
        [HttpGet("customer/{orgId}/expansion")]
        public async Task<IActionResult> GetExpansionIntelligence(int orgId)
        {
            try
            {
                var result = await _dashboardService.GetExpansionIntelligenceAsync(orgId);
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }
        [HttpGet("customer/{orgId}/behavior")]
        public async Task<IActionResult> GetUserBehavior(int orgId)
        {
            try
            {
                var result = await _dashboardService.GetUserBehaviorAsync(orgId);
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }
        [HttpGet("customer/{orgId}/alerts")]
        public async Task<IActionResult> GetAlerts(int orgId)
        {
            try
            {
                var result = await _dashboardService.GetAlertsAsync(orgId);
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }
        [HttpGet("executive/kpis")]
        public async Task<IActionResult> GetExecutiveKpis()
        {
            try
            {
                // Şimdilik demo orgId 1 gönderiyoruz
                var result = await _dashboardService.GetExecutiveKpisAsync(1);
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }
    }
}