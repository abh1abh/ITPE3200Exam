
using Microsoft.AspNetCore.Identity;

namespace api.DAL
{
    public static class AuthDbInit
    {
        // Seed method to initialize the Auth database
        // Helped by ChatGPT to return created user IDs for later use in App DbInit
        public static async Task<SeedResult> SeedAsync(IServiceProvider sp) 
        {

            var roleMgr = sp.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = sp.GetRequiredService<UserManager<AuthUser>>();

            var roles = new[] { "Admin", "HealthcareWorker", "Client" };
            foreach (var r in roles)
                if (!await roleMgr.RoleExistsAsync(r))
                    await roleMgr.CreateAsync(new IdentityRole(r));

            // Seedresult to hold created user IDs for later use in App DbInit
            var result = new SeedResult();

            // Admin
            var admin = await EnsureUserInRole(userMgr, "admin@homecare.local", "Admin123!", "Admin");

            result.UserIds["Admin"] = admin.Id;

            // Client   
            var client = await EnsureUserInRole(userMgr, "client@homecare.local", "Client123!", "Client");
            result.UserIds["Client"] = client.Id;

            // Worker 
            var worker = await EnsureUserInRole(userMgr, "worker@homecare.local", "Worker123!", "HealthcareWorker");
            result.UserIds["HealthcareWorker"] = worker.Id;

            return result;
        }

        // Private helper method to ensure a user exists and is assigned to a role
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
            // Ensure user is in the specified role
            if (!await userMgr.IsInRoleAsync(user, role))
            {
                await userMgr.AddToRoleAsync(user, role);
            }
            return user;
        }

    }

}