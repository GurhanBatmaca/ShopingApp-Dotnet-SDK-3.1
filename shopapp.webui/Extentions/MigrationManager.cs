using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using shopapp.data.Concrete.EfCore;
using shopapp.webui.Identity;

namespace shopapp.webui.Extentions
{
    public static class MigrationManager
    {
        public static IHost MigrateDatabase(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                using (var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>())
                {
                    try
                    {
                        applicationContext.Database.Migrate();
                    }
                    catch (System.Exception)
                    {
                        
                        throw;
                    }
                }

                using (var shopContext = scope.ServiceProvider.GetRequiredService<ShopContext>())
                {
                    try
                    {
                        shopContext.Database.Migrate();
                    }
                    catch (System.Exception)
                    {
                        
                        throw;
                    }
                }
            }

            return host;
        }
    }
}