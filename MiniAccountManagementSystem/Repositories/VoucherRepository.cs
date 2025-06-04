using Microsoft.Data.SqlClient;
using MiniAccountManagementSystem.Models;
using System.Data;

namespace MiniAccountManagementSystem.Repositories
{
    public class VoucherRepository : IVoucherRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<VoucherRepository> _logger;

        public VoucherRepository(IConfiguration configuration, ILogger<VoucherRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("con"); 
            _logger = logger;
        }

        public async Task<List<Voucher>> GetAllVouchersAsync()
        {
            var vouchers = new List<Voucher>();
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand("spGetAllVouchers", connection)) 
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                vouchers.Add(MapVoucherFromReader(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all vouchers.");
                throw; 
            }
            return vouchers;
        }

        public async Task<Voucher> GetVoucherByIdAsync(int voucherId)
        {
            Voucher voucher = null;
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand("spGetVoucherById", connection)) 
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@VoucherId", voucherId);
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                voucher = MapVoucherFromReader(reader);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting voucher by ID: {VoucherId}", voucherId);
                throw;
            }
            return voucher;
        }

        public async Task<string> CreateVoucherAsync(Voucher voucher)
        {
            string result = "";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand("spCreateVoucher", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@VoucherNo", voucher.VoucherNo);
                        command.Parameters.AddWithValue("@VoucherType", voucher.VoucherType);
                        command.Parameters.AddWithValue("@VoucherDate", voucher.VoucherDate);
                        command.Parameters.AddWithValue("@Description", (object)voucher.Description ?? DBNull.Value);
                        command.Parameters.AddWithValue("@TotalDebit", voucher.TotalDebit);
                        command.Parameters.AddWithValue("@TotalCredit", voucher.TotalCredit);
                        command.Parameters.AddWithValue("@CreatedByUserId", (object)voucher.CreatedByUserId ?? DBNull.Value);

                        var outputMessageParam = new SqlParameter("@OutputMessage", SqlDbType.NVarChar, 255) { Direction = ParameterDirection.Output };
                        command.Parameters.Add(outputMessageParam);

                        var newVoucherIdParam = new SqlParameter("@NewVoucherId", SqlDbType.Int) { Direction = ParameterDirection.Output }; // ADD THIS
                        command.Parameters.Add(newVoucherIdParam); // ADD THIS

                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                        result = outputMessageParam.Value.ToString();


                        if (!result.StartsWith("Error:") && newVoucherIdParam.Value != DBNull.Value)
                        {
                            voucher.VoucherId = (int)newVoucherIdParam.Value; // Set the ID here
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL Error creating voucher: {ErrorMessage}", ex.Message);
                result = $"Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while creating voucher.");
                result = $"Error: {ex.Message}";
            }
            return result;
        }

        public async Task<string> UpdateVoucherAsync(Voucher voucher)
        {
            string result = "";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand("spUpdateVoucher", connection)) 
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@VoucherId", voucher.VoucherId);
                        command.Parameters.AddWithValue("@VoucherNo", voucher.VoucherNo); 
                        command.Parameters.AddWithValue("@VoucherType", voucher.VoucherType);
                        command.Parameters.AddWithValue("@VoucherDate", voucher.VoucherDate);
                        command.Parameters.AddWithValue("@Description", (object)voucher.Description ?? DBNull.Value);
                        command.Parameters.AddWithValue("@TotalDebit", voucher.TotalDebit); 
                        command.Parameters.AddWithValue("@TotalCredit", voucher.TotalCredit);
                        var outputMessageParam = new SqlParameter("@OutputMessage", SqlDbType.NVarChar, 255)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(outputMessageParam);

                        await connection.OpenAsync();
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            result = outputMessageParam.Value.ToString();
                            _logger.LogInformation("Voucher ID {VoucherId} updated successfully.", voucher.VoucherId);
                        }
                        else
                        {
                            result = "Voucher not found or no changes made.";
                            _logger.LogWarning("Voucher ID {VoucherId} not found or no rows affected during update.", voucher.VoucherId);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL Error updating voucher: {ErrorMessage}", ex.Message);
                result = $"Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating voucher.");
                result = $"Error: {ex.Message}";
            }
            return result;
        }

        public async Task<string> DeleteVoucherAsync(int voucherId)
        {
            string result = "";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand("spDeleteVoucher", connection)) 
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@VoucherId", voucherId);

                        var outputMessageParam = new SqlParameter("@OutputMessage", SqlDbType.NVarChar, 255)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(outputMessageParam);

                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                        result = outputMessageParam.Value.ToString();
                    }
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL Error deleting voucher: {ErrorMessage}", ex.Message);
                result = $"Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while deleting voucher.");
                result = $"Error: {ex.Message}";
            }
            return result;
        }

        private Voucher MapVoucherFromReader(SqlDataReader reader)
        {
            return new Voucher
            {
                VoucherId = reader.GetInt32("VoucherId"),
                VoucherNo = reader.GetString("VoucherNo"), 
                VoucherType = reader.GetString("VoucherType"),
                VoucherDate = reader.GetDateTime("VoucherDate"),
                Description = reader.IsDBNull("Description") ? null : reader.GetString("Description"), 
                TotalDebit = reader.GetDecimal("TotalDebit"), 
                TotalCredit = reader.GetDecimal("TotalCredit"),
                CreatedByUserId = reader.IsDBNull("CreatedByUserId") ? (int?)null : reader.GetInt32("CreatedByUserId"),
                CreatedAt = reader.IsDBNull("CreatedAt") ? (DateTime?)null : reader.GetDateTime("CreatedAt"), 
                
            };
        }
    }

}
