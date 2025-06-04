using System.ComponentModel.DataAnnotations;

namespace MiniAccountManagementSystem.Models
{
    public class Account
    {
        public int AccountId { get; set; }

        [Required(ErrorMessage = "Account Name is required.")]
        [StringLength(255, ErrorMessage = "Account Name cannot exceed 255 characters.")]
        [Display(Name = "Account Name")]
        public string AccountName { get; set; }

        [StringLength(50, ErrorMessage = "Account Number cannot exceed 50 characters.")]
        [Display(Name = "Account Number")]
        public string AccountNumber { get; set; }

        [Required(ErrorMessage = "Account Type is required.")]
        [StringLength(50, ErrorMessage = "Account Type cannot exceed 50 characters.")]
        [Display(Name = "Account Type")]
        public string AccountType { get; set; }

        public int? ParentAccountId { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; }

        [Display(Name = "Parent Account Name")]
        public string ParentAccountName { get; set; }
    }
}
