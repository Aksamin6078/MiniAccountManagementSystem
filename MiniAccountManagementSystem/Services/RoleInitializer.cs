using Microsoft.AspNetCore.Identity;
using MiniAccountManagementSystem.Data;

namespace MiniAccountManagementSystem.Services
{
    public class RoleInitializer
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleInitializer(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task SeedRolesAsync()
        {
            string[] roleNames = { "Admin", "Accountant", "Viewer" };
            foreach (var roleName in roleNames)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        public async Task SeedAdminUserAsync()
        {
            if (await _userManager.FindByNameAsync("admin@example.com") == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin1@example.com",
                    Email = "admin@example.com",
                    EmailConfirmed = true
                };
                var result = await _userManager.CreateAsync(adminUser, "Adminpssword0@"); 
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}