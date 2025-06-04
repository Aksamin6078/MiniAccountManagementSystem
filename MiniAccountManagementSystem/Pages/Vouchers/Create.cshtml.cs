using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectList
using MiniAccountManagementSystem.Models;
using MiniAccountManagementSystem.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniAccountManagementSystem.Pages.Vouchers
{
    public class CreateModel : PageModel
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly IVoucherDetailRepository _voucherDetailRepository;
        private readonly IAccountRepository _accountRepository; // To get accounts for dropdown
        private readonly ILogger<CreateModel> _logger; // Inject logger

        public CreateModel(
            IVoucherRepository voucherRepository,
            IVoucherDetailRepository voucherDetailRepository,
            IAccountRepository accountRepository,
            ILogger<CreateModel> logger)
        {
            _voucherRepository = voucherRepository;
            _voucherDetailRepository = voucherDetailRepository;
            _accountRepository = accountRepository;
            _logger = logger;
        }

        [BindProperty]
        public Voucher Voucher { get; set; }

        [BindProperty]
        public List<VoucherDetail> VoucherDetails { get; set; } = new List<VoucherDetail>();

        public SelectList AccountsSelectList { get; set; }

        [BindProperty]
        public string NextVoucherNumber { get; set; }

        public async Task<IActionResult> OnGet()
        {
            Voucher = new Voucher
            {
                VoucherDate = DateTime.Today // Default to today's date
            };

            VoucherDetails.Add(new VoucherDetail { CreatedAt = DateTime.Now });
            VoucherDetails.Add(new VoucherDetail { CreatedAt = DateTime.Now }); 

            await PopulateAccountsSelectList();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await PopulateAccountsSelectList();
            decimal totalDebitDetails = VoucherDetails.Sum(vd => vd.Debit ?? 0);
            decimal totalCreditDetails = VoucherDetails.Sum(vd => vd.Credit ?? 0);

            if (totalDebitDetails != totalCreditDetails)
            {
                ModelState.AddModelError(string.Empty, "Total Debits must equal Total Credits for the voucher details.");
            }
            if (Voucher.TotalDebit != totalDebitDetails)
            {
                ModelState.AddModelError(nameof(Voucher.TotalDebit), "Voucher Total Debit must match sum of detail debits.");
            }
            if (Voucher.TotalCredit != totalCreditDetails)
            {
                ModelState.AddModelError(nameof(Voucher.TotalCredit), "Voucher Total Credit must match sum of detail credits.");
            }

            VoucherDetails = VoucherDetails
                .Where(vd => vd.AccountId != 0 || vd.Debit != null || vd.Credit != null || !string.IsNullOrWhiteSpace(vd.Remarks))
                .ToList();

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model validation failed during voucher creation.");
                return Page();
            }

            string voucherCreationResult = await _voucherRepository.CreateVoucherAsync(Voucher);

            if (voucherCreationResult.StartsWith("Error:"))
            {
                ModelState.AddModelError(string.Empty, voucherCreationResult);
                _logger.LogError("Failed to create voucher header: {Result}", voucherCreationResult);
                return Page();
            }

            if (Voucher.VoucherId == 0) 
            {
                ModelState.AddModelError(string.Empty, "Error: Voucher header saved but ID not returned. Cannot save details. Contact administrator.");
                _logger.LogError("Voucher header created, but new VoucherId was not returned by spCreateVoucher. VoucherNo: {VoucherNo}", Voucher.VoucherNo);

                return Page();
            }


            foreach (var detail in VoucherDetails)
            {
                detail.VoucherId = Voucher.VoucherId; 
                detail.CreatedAt = DateTime.Now; 

                string detailCreationResult = await _voucherDetailRepository.CreateVoucherDetailAsync(detail);
                if (detailCreationResult.StartsWith("Error:"))
                {
                    ModelState.AddModelError(string.Empty, $"Error saving detail for Account ID {detail.AccountId}: {detailCreationResult}");
                    _logger.LogError("Failed to create voucher detail for Voucher ID {VoucherId}, Account ID {AccountId}: {Result}", Voucher.VoucherId, detail.AccountId, detailCreationResult);
                    return Page();
                }
            }            
            TempData["SuccessMessage"] = "Voucher created successfully!";
            return RedirectToPage("./Index"); 
        }

        private async Task PopulateAccountsSelectList()
        {
            var accounts = await _accountRepository.GetAllAccountsAsync();
            AccountsSelectList = new SelectList(accounts, "AccountId", "AccountName");
        }
    }
}