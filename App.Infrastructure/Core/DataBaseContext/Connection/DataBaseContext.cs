using System.Reflection;
using App.Infrastructure.Core.DataBaseContext.Configurations.Global;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Core.DataBaseContext.Connection;

public partial class AppDataBaseContext : DbContext
{
    public AppDataBaseContext(DbContextOptions<AppDataBaseContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        EnumConfigurations.Configure(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}