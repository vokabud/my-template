using Microsoft.EntityFrameworkCore;
using Template.Api.Persistence;

namespace Template.Api.UnitTests;

internal static class TestApplicationDbContext
{
    internal static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }
}
