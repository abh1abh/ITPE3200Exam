
using Microsoft.AspNetCore.Identity;

namespace HomecareAppointmentManagement.DAL
{
    public static class AuthDbInit
    {
        public static async Task<SeedResult> SeedAsync(IServiceProvider sp) // Seed method to initialize the database
        {

            var roleMgr = sp.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = sp.GetRequiredService<UserManager<AuthUser>>();

            var roles = new[] { "Admin", "HealthcareWorker", "Client" };
            foreach (var r in roles)
                if (!await roleMgr.RoleExistsAsync(r))
                    await roleMgr.CreateAsync(new IdentityRole(r));

            var result = new SeedResult();

            // admin
            var admin = await EnsureUserInRole(userMgr, "admin@homecare.local", "Admin123!", "Admin");

            result.UserIds["Admin"] = admin.Id;

            // client
            var client = await EnsureUserInRole(userMgr, "client@homecare.local", "Client123!", "Client");
            result.UserIds["Client"] = client.Id;

            // worker (different email than client)
            var worker = await EnsureUserInRole(userMgr, "worker@homecare.local", "Worker123!", "HealthcareWorker");
            result.UserIds["HealthcareWorker"] = worker.Id;

            return result;
        }
        private static async Task<AuthUser> EnsureUserInRole(UserManager<AuthUser> userMgr, string email, string password, string role)
        {
            var user = await userMgr.FindByEmailAsync(email);
            if (user == null)
            {
                user = new AuthUser { UserName = email, Email = email, EmailConfirmed = true };
                var create = await userMgr.CreateAsync(user, password);
                if (!create.Succeeded)
                    throw new Exception($"Failed creating user {email}: {create.Errors}");
            }

            if (!await userMgr.IsInRoleAsync(user, role))
            {
                await userMgr.AddToRoleAsync(user, role);
            }
            return user;
        }

    }

}