using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using shopapp.business.Abstact;

namespace shopapp.webui.Identity
{
    public static class SeedIdentity
    {
        public static async Task Seed(UserManager<User> userManager,RoleManager<IdentityRole> roleManager,ICartService cartService,IConfiguration configuration)
        {

            var roles = configuration.GetSection("<>:<>").GetChildren().Select(x =>x.Value).ToArray();

            foreach (var role in roles)
            {
                if(!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }              
            }

            var users = configuration.GetSection("<>:<>");

            foreach (var section in users.GetChildren())
            {
                var username = section.GetValue<string>("<>");
                var password = section.GetValue<string>("<>");
                var email = section.GetValue<string>("<>");
                var role = section.GetValue<string>("<>");
                var firstName = section.GetValue<string>("<>");
                var lastName = section.GetValue<string>("<>");

                if(await userManager.FindByNameAsync(username) == null)
                {
                    var user = new User()
                    {
                        UserName = username,
                        Email = email,
                        FirsName = firstName,
                        LastName = lastName,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(user,password);

                    if(result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user,role);
                        cartService.InitializeCart(user.Id);
                    }
                }
            }        
        }
    }
}