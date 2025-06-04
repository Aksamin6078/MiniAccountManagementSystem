using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniAccountManagementSystem.Models;
using MiniAccountManagementSystem.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.AspNetCore.Mvc;

namespace MiniAccountManagementSystem.Pages.Accounts
{
    public class IndexModel : PageModel
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IAccountRepository accountRepository, ILogger<IndexModel> logger)
        {
            _accountRepository = accountRepository;
            _logger = logger;
        }

        public IList<MiniAccountManagementSystem.Models.Account> Accounts { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                Accounts = await _accountRepository.GetAllAccountsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all accounts for Index page.");
                ModelState.AddModelError(string.Empty, "An error occurred while loading accounts. Please try again later.");
                Accounts = new List<MiniAccountManagementSystem.Models.Account>(); 
            }
        }
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            string result = await _accountRepository.DeleteAccountAsync(id);
            if (result.StartsWith("Error"))
            {
                TempData["ErrorMessage"] = result;
            }
            else
            {
                TempData["SuccessMessage"] = result;
            }
            return RedirectToPage("./Index");
        }
    }
}