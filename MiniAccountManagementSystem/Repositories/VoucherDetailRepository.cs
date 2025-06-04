using Microsoft.Data.SqlClient;
using MiniAccountManagementSystem.Models;
using System.Data;

namespace MiniAccountManagementSystem.Repositories
{
    public class VoucherDetailRepository : IVoucherDetailRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<VoucherDetailRepository> _logger;

        public VoucherDetailRepository(IConfiguration configuration, ILogger<VoucherDetailRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("con"); // Ensure this matches appsettings.json
            _logger = logger;
        }

        public async Task<List<VoucherDetail>> GetVoucherDetailsByVoucherIdAsync(int voucherId)
        {
            var voucherDetails = new List<VoucherDetail>();
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand("spGetVoucherDetailsByVoucherId", connection)) // You need to create this SP
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@VoucherId", voucherId);
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                voucherDetails.Add(MapVoucherDetailFromReader(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting voucher details for VoucherId: {VoucherId}", voucherId);
                throw;
            }
            return voucherDetails;
        }

        public async Task<VoucherDetail> GetVoucherDetailByIdAsync(int voucherDetailId)
        {
            VoucherDetail voucherDetail = null;
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand("spGetVoucherDetailById", connection)) // You need to create this SP
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@VoucherDetailId", voucherDetailId);
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                voucherDetail = MapVoucherDetailFromReader(reader);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting voucher detail by ID: {VoucherDetailId}", voucherDetailId);
                throw;
            }
            return voucherDetail;
        }

        public async Task<string> CreateVoucherDetailAsync(VoucherDetail voucherDetail)
        {
            string result = "";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand("spCreateVoucherDetail", connection)) // You need to create this SP
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@VoucherId", voucherDetail.VoucherId);
                        command.Parameters.AddWithValue("@AccountId", voucherDetail.AccountId);
                        command.Parameters.AddWithValue("@Debit", (object)voucherDetail.Debit ?? DBNull.Value); 
                        command.Parameters.AddWithValue("@Credit", (object)voucherDetail.Credit ?? DBNull.Value); 
                        command.Parameters.AddWithValue("@Remarks", (object)voucherDetail.Remarks ?? DBNull.Value); 
                        // CreatedAt will be set by the DB or passed if you want to control it from app

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
                _logger.LogError(ex, "SQL Error creating voucher detail: {ErrorMessage}", ex.Message);
                result = $"Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while creating voucher detail.");
                result = $"Error: {ex.Message}";
            }
            return result;
        }

        public async Task<string> UpdateVoucherDetailAsync(VoucherDetail voucherDetail)
        {
            string result = "";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand("spUpdateVoucherDetail", connection)) 
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@VoucherDetailId", voucherDetail.VoucherDetailId);
                        command.Parameters.AddWithValue("@VoucherId", voucherDetail.VoucherId); 
                        command.Parameters.AddWithValue("@AccountId", voucherDetail.AccountId);
                        command.Parameters.AddWithValue("@Debit", (object)voucherDetail.Debit ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Credit", (object)voucherDetail.Credit ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Remarks", (object)voucherDetail.Remarks ?? DBNull.Value);

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
                            _logger.LogInformation("Voucher Detail ID {VoucherDetailId} updated successfully.", voucherDetail.VoucherDetailId);
                        }
                        else
                        {
                            result = "Voucher detail not found or no changes made.";
                            _logger.LogWarning("Voucher Detail ID {VoucherDetailId} not found or no rows affected during update.", voucherDetail.VoucherDetailId);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL Error updating voucher detail: {ErrorMessage}", ex.Message);
                result = $"Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating voucher detail.");
                result = $"Error: {ex.Message}";
            }
            return result;
        }

        public async Task<string> DeleteVoucherDetailAsync(int voucherDetailId)
        {
            string result = "";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand("spDeleteVoucherDetail", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@VoucherDetailId", voucherDetailId);

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
                _logger.LogError(ex, "SQL Error deleting voucher detail: {ErrorMessage}", ex.Message);
                result = $"Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while deleting voucher detail.");
                result = $"Error: {ex.Message}";
            }
            return result;
        }

        public async Task<string> DeleteVoucherDetailsByVoucherIdAsync(int voucherId)
        {
            string result = "";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand("spDeleteVoucherDetailsByVoucherId", connection)) 
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
                _logger.LogError(ex, "SQL Error deleting voucher details for VoucherId {VoucherId}: {ErrorMessage}", voucherId, ex.Message);
                result = $"Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while deleting voucher details for VoucherId {VoucherId}.", voucherId);
                result = $"Error: {ex.Message}";
            }
            return result;
        }

        private VoucherDetail MapVoucherDetailFromReader(SqlDataReader reader)
        {
            return new VoucherDetail
            {
                VoucherDetailId = reader.GetInt32("VoucherDetailId"),
                VoucherId = reader.GetInt32("VoucherId"),
                AccountId = reader.GetInt32("AccountId"),
                Debit = reader.IsDBNull("Debit") ? (decimal?)null : reader.GetDecimal("Debit"), 
                Credit = reader.IsDBNull("Credit") ? (decimal?)null : reader.GetDecimal("Credit"), 
                Remarks = reader.IsDBNull("Remarks") ? null : reader.GetString("Remarks"), 
                CreatedAt = reader.GetDateTime("CreatedAt"), 
                AccountName = reader.IsDBNull("AccountName") ? null : reader.GetString("AccountName")
            };
        }
    }

}
