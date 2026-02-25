using Microsoft.EntityFrameworkCore;
using Template.Api.Domain;

namespace Template.Api.Persistence;

public interface IApplicationDbContext
{
    DbSet<TaskEntity> Tasks { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
