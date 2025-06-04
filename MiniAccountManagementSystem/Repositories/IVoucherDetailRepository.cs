using MiniAccountManagementSystem.Models;

namespace MiniAccountManagementSystem.Repositories
{
    public interface IVoucherDetailRepository
    {
        Task<List<VoucherDetail>> GetVoucherDetailsByVoucherIdAsync(int voucherId);
        Task<VoucherDetail> GetVoucherDetailByIdAsync(int voucherDetailId);
        Task<string> CreateVoucherDetailAsync(VoucherDetail voucherDetail);
        Task<string> UpdateVoucherDetailAsync(VoucherDetail voucherDetail);
        Task<string> DeleteVoucherDetailAsync(int voucherDetailId);
        Task<string> DeleteVoucherDetailsByVoucherIdAsync(int voucherId); 
    }
}
