using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace GitInsight;

//context factory here
/*Program.cs skal kalde den her, for at lave database.
den laver en instans af GitInsightContext og returnerer den.
*/

internal class GitInsightContextFactory : IDesignTimeDbContextFactory<GitInsightContext>
{
    public GitInsightContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder().AddUserSecrets<GitInsight>().Build();
        var connectionString = configuration.GetConnectionString("GitIn");

        var optionsBuilder = new DbContextOptionsBuilder<GitInsightContext>();
        optionsBuilder.UseNpgsql(connectionString);

        var context = new GitInsightContext(optionsBuilder.Options);
        return context;
    }
}
