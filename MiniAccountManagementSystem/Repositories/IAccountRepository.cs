using MiniAccountManagementSystem.Models;

namespace MiniAccountManagementSystem.Repositories
{
    public interface IAccountRepository
    {
        Task<List<Account>> GetAllAccountsAsync();
        Task<Account> GetAccountByIdAsync(int accountId);
        Task<string> CreateAccountAsync(Account account);
        Task<string> UpdateAccountAsync(Account account);
        Task<string> DeleteAccountAsync(int accountId);

    }
}
