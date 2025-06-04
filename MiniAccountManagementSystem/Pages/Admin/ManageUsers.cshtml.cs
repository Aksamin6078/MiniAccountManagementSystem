using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniAccountManagementSystem.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace MiniAccountManagementSystem.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ManageUsersModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        // Corrected: Use ApplicationRole for consistency with your Identity setup
        private readonly RoleManager<ApplicationRole> _roleManager;

        public ManageUsersModel(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public List<UserViewModel> Users { get; set; } = new List<UserViewModel>();
        public List<string> Roles { get; set; } = new List<string>();

        [BindProperty]
        public string SelectedUserId { get; set; }

        [BindProperty]
        public string SelectedRole { get; set; }

        public async Task OnGetAsync()
        {
            Roles = _roleManager.Roles.Select(r => r.Name).ToList();
            var allUsers = _userManager.Users.ToList();
            foreach (var user in allUsers)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                Users.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = userRoles.ToList()
                });
            }
        }

        // This method handles the form submission for assigning a role (HTTP POST request)
        public async Task<IActionResult> OnPostAssignRoleAsync()
        {
            if (string.IsNullOrEmpty(SelectedUserId) || string.IsNullOrEmpty(SelectedRole))
            {
                TempData["ErrorMessage"] = "User and Role must be selected.";
                // Reload the page data
                await OnGetAsync();
                return Page(); 
            }

            var user = await _userManager.FindByIdAsync(SelectedUserId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                await OnGetAsync();
                return Page();
            }

            if (!await _roleManager.RoleExistsAsync(SelectedRole))
            {
                TempData["ErrorMessage"] = "Role does not exist.";
                await OnGetAsync();
                return Page();
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                TempData["ErrorMessage"] = "Failed to remove existing roles.";
                foreach (var error in removeResult.Errors)
                {
                    TempData["ErrorMessage"] += $" {error.Description}";
                }
                await OnGetAsync();
                return Page();
            }

            var addResult = await _userManager.AddToRoleAsync(user, SelectedRole);
            if (addResult.Succeeded)
            {
                TempData["SuccessMessage"] = $"Role '{SelectedRole}' assigned to '{user.UserName}' successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to assign role.";
                foreach (var error in addResult.Errors)
                {
                    TempData["ErrorMessage"] += $" {error.Description}";
                }
            }

            return RedirectToPage(); 
        }
    }

    public class UserViewModel
    {
        public string Id { get; set; } 
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; } 
    }
}
