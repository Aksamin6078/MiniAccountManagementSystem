using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace MiniAccountManagementSystem.Data
{

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        //protected override void OnModelCreating(ModelBuilder builder)
        //{
        //    base.OnModelCreating(builder);

        //    string adminRoleId = "8cc45030-ccf4-48ae-9784-cfd3c22facd2";
        //    string accountantRoleId = "115f091d-2a4b-4718-81bc-44056e5d3d49";
        //    string viewerRoleId = "4e5e174e-323b-4171-872f-a2e086118b78";

        //    string adminUserId = "8e445865-a24d-4543-a6c6-9443d048cdb9";
        //    string viewerUserId = "9f445865-a24d-4543-a6c6-9443d048cdba";
        //    string accountantUserId = "ae445865-a24d-4543-a6c6-9443d048cdbb";

        //    // Seed Roles
        //    builder.Entity<ApplicationRole>().HasData(
        //        new ApplicationRole { Id = adminRoleId, Name = "Admin", NormalizedName = "ADMIN", ConcurrencyStamp = Guid.NewGuid().ToString() },
        //        new ApplicationRole { Id = accountantRoleId, Name = "Accountant", NormalizedName = "ACCOUNTANT", ConcurrencyStamp = Guid.NewGuid().ToString() },
        //        new ApplicationRole { Id = viewerRoleId, Name = "Viewer", NormalizedName = "VIEWER", ConcurrencyStamp = Guid.NewGuid().ToString() }
        //    );
        //    var hasher = new PasswordHasher<ApplicationUser>();

        //    builder.Entity<ApplicationUser>().HasData(
        //        new ApplicationUser
        //        {
        //            Id = adminUserId,
        //            UserName = "admin",
        //            NormalizedUserName = "ADMIN",
        //            Email = "admin@example.com",
        //            NormalizedEmail = "ADMIN@EXAMPLE.COM",
        //            EmailConfirmed = true,
        //            PasswordHash = hasher.HashPassword(null, "Admin123@"),
        //            SecurityStamp = Guid.NewGuid().ToString(),
        //            ConcurrencyStamp = Guid.NewGuid().ToString(),
        //            CreatedAt = DateTime.UtcNow
        //        },
        //        new ApplicationUser
        //        {
        //            Id = viewerUserId,
        //            UserName = "viewer",
        //            NormalizedUserName = "VIEWER",
        //            Email = "viewer@example.com",
        //            NormalizedEmail = "VIEWER@EXAMPLE.COM",
        //            EmailConfirmed = true,
        //            PasswordHash = hasher.HashPassword(null, "Viewer123@"),
        //            SecurityStamp = Guid.NewGuid().ToString(),
        //            ConcurrencyStamp = Guid.NewGuid().ToString(),
        //            CreatedAt = DateTime.UtcNow
        //        },
        //        new ApplicationUser
        //        {
        //            Id = accountantUserId,
        //            UserName = "accountant",
        //            NormalizedUserName = "ACCOUNTANT",
        //            Email = "accountant@example.com",
        //            NormalizedEmail = "ACCOUNTANT@EXAMPLE.COM",
        //            EmailConfirmed = true,
        //            PasswordHash = hasher.HashPassword(null, "Accountant123@"),
        //            SecurityStamp = Guid.NewGuid().ToString(),
        //            ConcurrencyStamp = Guid.NewGuid().ToString(),
        //            CreatedAt = DateTime.UtcNow
        //        }
        //    );

        //    builder.Entity<IdentityUserRole<string>>().HasData(
        //        new IdentityUserRole<string> { RoleId = adminRoleId, UserId = adminUserId },
        //        new IdentityUserRole<string> { RoleId = viewerRoleId, UserId = viewerUserId },
        //        new IdentityUserRole<string> { RoleId = accountantRoleId, UserId = accountantUserId }
        //    );
        //}
    }

    public class ApplicationUser : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ApplicationRole : IdentityRole
    {

    }
}
