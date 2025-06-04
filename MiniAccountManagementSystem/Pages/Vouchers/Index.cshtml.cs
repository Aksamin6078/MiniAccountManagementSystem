using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniAccountManagementSystem.Models;
using MiniAccountManagementSystem.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // Don't forget to inject ILogger

namespace MiniAccountManagementSystem.Pages.Vouchers
{
    public class IndexModel : PageModel
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly ILogger<IndexModel> _logger; // Inject logger

        public IndexModel(IVoucherRepository voucherRepository, ILogger<IndexModel> logger)
        {
            _voucherRepository = voucherRepository;
            _logger = logger;
        }

        public IList<Voucher> Vouchers { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                Vouchers = await _voucherRepository.GetAllVouchersAsync();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all vouchers for Index page.");
                TempData["ErrorMessage"] = "Failed to load vouchers. Please try again later.";
                Vouchers = new List<Voucher>(); // Ensure Vouchers is not null
            }
        }
    }
}