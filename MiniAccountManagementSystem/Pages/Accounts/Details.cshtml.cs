using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniAccountManagementSystem.Models;
using MiniAccountManagementSystem.Repositories;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System; 

namespace MiniAccountManagementSystem.Pages.Accounts
{
    public class DetailsModel : PageModel
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(IAccountRepository accountRepository, ILogger<DetailsModel> logger)
        {
            _accountRepository = accountRepository;
            _logger = logger;
        }

        public MiniAccountManagementSystem.Models.Account Account { get; set; } 

        public async Task<IActionResult> OnGetAsync(int? id) 
        {
            if (id == null)
            {
                return NotFound(); 
            }

            try
            {
                Account = await _accountRepository.GetAccountByIdAsync(id.Value);

                if (Account == null)
                {
                    return NotFound(); 
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving account details for AccountId: {AccountId}", id);
                return RedirectToPage("./Index"); 
            }

            return Page(); 
        }
    }
}