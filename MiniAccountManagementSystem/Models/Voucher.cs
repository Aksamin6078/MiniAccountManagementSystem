using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MiniAccountManagementSystem.Models
{
    public class Voucher
    {
        [Key]
        [Display(Name = "Voucher ID")]
        public int VoucherId { get; set; }

        [Required(ErrorMessage = "Voucher Number is required.")]
        [StringLength(50, ErrorMessage = "Voucher Number cannot exceed 50 characters.")]
        [Display(Name = "Voucher Number")]
        public string VoucherNo { get; set; } 

        [Required(ErrorMessage = "Voucher Date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Voucher Date")]
        public DateTime VoucherDate { get; set; }

        [Required(ErrorMessage = "Voucher Type is required.")]
        [StringLength(20, ErrorMessage = "Voucher Type cannot exceed 20 characters.")] 
        [Display(Name = "Voucher Type")]
        public string VoucherType { get; set; }

        [StringLength(4000, ErrorMessage = "Description cannot exceed 4000 characters.")] 
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Total Debit is required.")]
        [Range(0.00, double.MaxValue, ErrorMessage = "Total Debit must be a non-negative value.")]
        [DataType(DataType.Currency)]
        [Display(Name = "Total Debit")]
        public decimal TotalDebit { get; set; } 

        [Required(ErrorMessage = "Total Credit is required.")]
        [Range(0.00, double.MaxValue, ErrorMessage = "Total Credit must be a non-negative value.")]
        [DataType(DataType.Currency)]
        [Display(Name = "Total Credit")]
        public decimal TotalCredit { get; set; }

        [Display(Name = "Created By User ID")]
        public int? CreatedByUserId { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Created At")]
        public DateTime? CreatedAt { get; set; }

    }

}
