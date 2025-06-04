using System.Data;
using Microsoft.Data.SqlClient;
using MiniAccountManagementSystem.Models;
using Microsoft.Extensions.Configuration; // Add this using directive
using Microsoft.Extensions.Logging; // Add this using directive if not already there

namespace MiniAccountManagementSystem.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<AccountRepository> _logger; 

        public AccountRepository(IConfiguration configuration, ILogger<AccountRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("con");
            _logger = logger;
        }

        public async Task<List<Account>> GetAllAccountsAsync()
        {
            var accounts = new List<Account>();
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand("sp_ManageChartOfAccounts", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Action", "GET");

                        var outputMessageParam = new SqlParameter("@OutputMessage", SqlDbType.NVarChar, 255)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(outputMessageParam);

                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var account = new Account
                                {
                                    AccountId = reader.GetInt32("AccountId"),
                                    AccountName = reader.GetString("AccountName"),
                                    AccountNumber = reader.IsDBNull("AccountNumber") ? null : reader.GetString("AccountNumber"),
                                    AccountType = reader.GetString("AccountType"),
                                    ParentAccountId = reader.IsDBNull("ParentAccountId") ? (int?)null : reader.GetInt32("ParentAccountId"),
                                    IsActive = reader.GetBoolean("IsActive"),
                                    CreatedAt = reader.GetDateTime("CreatedAt"),
                                    UpdatedAt = reader.GetDateTime("UpdatedAt")
                                };
                                accounts.Add(account);
                            }
                        }
                    }
                }
                foreach (var account in accounts)
                {
                    if (account.ParentAccountId.HasValue)
                    {
                        var parentAccount = accounts.FirstOrDefault(a => a.AccountId == account.ParentAccountId.Value);
                        if (parentAccount != null)
                        {
                            account.ParentAccountName = parentAccount.AccountName;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all accounts.");
                throw;
            }
            return accounts;
        }

        public async Task<Account> GetAccountByIdAsync(int accountId)
        {
            Account account = null;
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("spGetAccountById", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@AccountId", accountId);

                await con.OpenAsync();
                using (SqlDataReader rdr = await cmd.ExecuteReaderAsync())
                {
                    if (await rdr.ReadAsync()) 
                    {
                        account = new Account 
                        {
                            AccountId = rdr.GetInt32("AccountId"),
                            AccountName = rdr.GetString("AccountName"),
                            AccountNumber = rdr.GetString("AccountNumber"),
                            AccountType = rdr.GetString("AccountType"),
                            ParentAccountId = rdr.IsDBNull("ParentAccountId") ? (int?)null : rdr.GetInt32("ParentAccountId"),
                            ParentAccountName = rdr.IsDBNull("ParentAccountName") ? null : rdr.GetString("ParentAccountName"),
                            IsActive = rdr.GetBoolean("IsActive"),
                            CreatedAt = rdr.GetDateTime("CreatedAt"),
                            UpdatedAt = rdr.GetDateTime("UpdatedAt")
                        };
                    }
                }
            }
            return account; //
        }
        public async Task<string> CreateAccountAsync(Account account)
        {
            string message = "";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand("sp_ManageChartOfAccounts", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Action", "CREATE");
                        command.Parameters.AddWithValue("@AccountName", account.AccountName);
                        command.Parameters.AddWithValue("@AccountNumber", (object)account.AccountNumber ?? DBNull.Value);
                        command.Parameters.AddWithValue("@AccountType", account.AccountType);
                        command.Parameters.AddWithValue("@ParentAccountId", (object)account.ParentAccountId ?? DBNull.Value);
                        command.Parameters.AddWithValue("@IsActive", account.IsActive);

                        var outputMessageParam = new SqlParameter("@OutputMessage", SqlDbType.NVarChar, 255)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(outputMessageParam);

                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                        message = outputMessageParam.Value.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account.");
                message = $"Error: {ex.Message}";
            }
            return message;
        }

        public async Task<string> UpdateAccountAsync(Account account)
        {
            string result = string.Empty;
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("spUpdateAccount", con)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@AccountId", account.AccountId);
                cmd.Parameters.AddWithValue("@AccountName", account.AccountName);
                cmd.Parameters.AddWithValue("@AccountNumber", account.AccountNumber);
                cmd.Parameters.AddWithValue("@AccountType", account.AccountType);

                if (account.ParentAccountId.HasValue && account.ParentAccountId.Value > 0)
                {
                    cmd.Parameters.AddWithValue("@ParentAccountId", account.ParentAccountId.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@ParentAccountId", DBNull.Value);
                }

                cmd.Parameters.AddWithValue("@IsActive", account.IsActive);
                cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now); 

                try
                {
                    await con.OpenAsync();
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    if (rowsAffected > 0)
                    {
                        result = "Account updated successfully!";
                    }
                    else
                    {
                        result = "Account not found or no changes made.";
                    }
                }
                catch (SqlException ex)
                {
                   
                    Console.WriteLine($"SQL Error: {ex.Message}");
                    result = $"Error updating account: {ex.Message}";
                }
            }
            return result;
        }
        public async Task<string> DeleteAccountAsync(int accountId)
        {
            string message = "";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand("sp_ManageChartOfAccounts", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Action", "DELETE");
                        command.Parameters.AddWithValue("@AccountId", accountId);

                        var outputMessageParam = new SqlParameter("@OutputMessage", SqlDbType.NVarChar, 255)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(outputMessageParam);

                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                        message = outputMessageParam.Value.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting account ID: {accountId}");
                message = $"Error: {ex.Message}";
            }
            return message;
        }
    }
}