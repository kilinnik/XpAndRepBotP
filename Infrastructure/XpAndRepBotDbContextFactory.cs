using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure;

public class XpAndRepBotDbContextFactory : IDesignTimeDbContextFactory<XpAndRepBotDbContext>
{
    public XpAndRepBotDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<XpAndRepBotDbContext>();
        optionsBuilder.UseSqlServer(
            "Data Source=KOMPUTER;Initial Catalog=BD_Users;Integrated Security=True;Connect Timeout=30;" +
            "Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"
        );

        return new XpAndRepBotDbContext(optionsBuilder.Options);
    }
}
