using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectList
using MiniAccountManagementSystem.Models;
using MiniAccountManagementSystem.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;

namespace MiniAccountManagementSystem.Pages.Accounts
{
    public class EditModel : PageModel
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<EditModel> _logger;

        public EditModel(IAccountRepository accountRepository, ILogger<EditModel> logger)
        {
            _accountRepository = accountRepository;
            _logger = logger;
        }


        [BindProperty]
        public MiniAccountManagementSystem.Models.Account Account { get; set; }

        public SelectList ParentAccounts { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("EditModel.OnGetAsync called with null ID.");
                return NotFound();
            }
            try
            {
                Account = await _accountRepository.GetAccountByIdAsync(id.Value);
                if (Account == null)
                {
                    _logger.LogWarning("EditModel.OnGetAsync: Account with ID {AccountId} not found.", id.Value);
                    return NotFound();
                }
                await PopulateParentAccountsDropdown(Account.AccountId);
            }
            catch (Exception ex)
            {
                
                _logger.LogError(ex, "Error fetching account for edit (ID: {AccountId}).", id.Value);
                TempData["ErrorMessage"] = "Error loading account for editing. Please try again.";
                return RedirectToPage("./Index");
            }

            return Page(); 
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await PopulateParentAccountsDropdown(Account.AccountId);

            try
            {
                string result = await _accountRepository.UpdateAccountAsync(Account);
                if (result.StartsWith("Error", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(string.Empty, result);
                    _logger.LogError("EditModel.OnPostAsync: Repository returned an error for Account ID {AccountId}: {Result}", Account.AccountId, result);
                    return Page(); 
                }
                TempData["SuccessMessage"] = result;
                _logger.LogInformation("Account ID {AccountId} updated successfully. Message: {Result}", Account.AccountId, result);

                return RedirectToPage("./Details", new { id = Account.AccountId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating account ID {AccountId}.", Account.AccountId);
                ModelState.AddModelError(string.Empty, $"An unexpected error occurred while updating the account. Please try again. Details: {ex.Message}");
                return Page(); 
            }
        }

        private async Task PopulateParentAccountsDropdown(int currentAccountId)
        {
            var allAccounts = await _accountRepository.GetAllAccountsAsync();
            var nonSelfAccounts = allAccounts.Where(a => a.AccountId != currentAccountId).ToList();
            var parentAccountOptions = new List<MiniAccountManagementSystem.Models.Account>
            {
                new MiniAccountManagementSystem.Models.Account { AccountId = 0, AccountName = "--- No Parent Account ---" }
            };
            parentAccountOptions.AddRange(nonSelfAccounts);
            ParentAccounts = new SelectList(parentAccountOptions, "AccountId", "AccountName", Account.ParentAccountId);

        }
    }
}