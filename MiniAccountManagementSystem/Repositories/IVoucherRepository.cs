using MiniAccountManagementSystem.Models;

namespace MiniAccountManagementSystem.Repositories
{
    public interface IVoucherRepository
    {
        Task<List<Voucher>> GetAllVouchersAsync();
        Task<Voucher> GetVoucherByIdAsync(int voucherId);
        Task<string> CreateVoucherAsync(Voucher voucher);
        Task<string> UpdateVoucherAsync(Voucher voucher);
        Task<string> DeleteVoucherAsync(int voucherId);
    }
}
