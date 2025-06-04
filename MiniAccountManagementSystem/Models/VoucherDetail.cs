using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MiniAccountManagementSystem.Models
{
    public class VoucherDetail
    {
        [Key]
        [Display(Name = "Voucher Detail ID")]
        public int VoucherDetailId { get; set; }

        [Required(ErrorMessage = "Voucher ID is required.")]
        [Display(Name = "Voucher ID")]
        public int VoucherId { get; set; }

        [Required(ErrorMessage = "Account is required.")]
        [Display(Name = "Account")]
        public int AccountId { get; set; }

        [Display(Name = "Debit")]
        [DataType(DataType.Currency)]
        public decimal? Debit { get; set; } 

        [Display(Name = "Credit")]
        [DataType(DataType.Currency)]
        public decimal? Credit { get; set; }

        [StringLength(4000, ErrorMessage = "Remarks cannot exceed 4000 characters.")]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; } 

        [Required] 
        [DataType(DataType.DateTime)]
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now; 

        [NotMapped]
        [Display(Name = "Account Name")]
        public string? AccountName { get; set; } 

    }

}
