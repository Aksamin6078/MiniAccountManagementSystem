using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectListItem
using MiniAccountManagementSystem.Models;
using MiniAccountManagementSystem.Repositories;
using System.Collections.Generic;
using System.Linq; // For .Any() and .Select()
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System; // For Exception

namespace MiniAccountManagementSystem.Pages.Accounts
{
    public class CreateModel : PageModel
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(IAccountRepository accountRepository, ILogger<CreateModel> logger)
        {
            _accountRepository = accountRepository;
            _logger = logger;
        }

        [BindProperty]
        public MiniAccountManagementSystem.Models.Account Account { get; set; }

        public SelectList ParentAccounts { get; set; }
        public SelectList AccountTypes { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadDataForFormAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Account.AccountId");
            ModelState.Remove("Account.CreatedAt");
            ModelState.Remove("Account.UpdatedAt");
            ModelState.Remove("Account.ParentAccountName"); 

            if (!ModelState.IsValid)
            {
                await LoadDataForFormAsync(); 
                return Page();
            }

            try
            {

                string message = await _accountRepository.CreateAccountAsync(Account);

                if (message.StartsWith("Error"))
                {
                    ModelState.AddModelError(string.Empty, message);
                    await LoadDataForFormAsync();
                    return Page();
                }

                TempData["SuccessMessage"] = message; 
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while creating the account.");
                await LoadDataForFormAsync();
                return Page();
            }
        }

        private async Task LoadDataForFormAsync()
        {
            var allAccounts = await _accountRepository.GetAllAccountsAsync();
            ParentAccounts = new SelectList(allAccounts, "AccountId", "AccountName");
            var accountTypes = new List<string>
            {
                "Asset", "Liability", "Equity", "Revenue", "Expense"
            };
            AccountTypes = new SelectList(accountTypes);
        }
    }
}