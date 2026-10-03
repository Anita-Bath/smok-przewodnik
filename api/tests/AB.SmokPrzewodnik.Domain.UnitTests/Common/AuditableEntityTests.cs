using AB.SmokPrzewodnik.Domain.Common;
using Xunit;

namespace AB.SmokPrzewodnik.Domain.UnitTests.Common;

public sealed class AuditableEntityTests
{
    [Fact]
    public void Constructor_ForwardsIdentityAndInitializesAuditState()
    {
        var id = Guid.NewGuid();
        var createdAt = DateTimeOffset.Parse("2026-10-03T12:00:00+02:00");

        var entity = new TestAuditableEntity(id, createdAt);

        Assert.Equal(id, entity.Id);
        Assert.Equal(createdAt, entity.CreatedAt);
        Assert.Null(entity.UpdatedAt);
    }

    [Fact]
    public void MarkUpdated_StoresProvidedTimestamp()
    {
        var createdAt = DateTimeOffset.Parse("2026-10-03T12:00:00+02:00");
        var entity = new TestAuditableEntity(Guid.NewGuid(), createdAt);
        var updatedAt = DateTimeOffset.Parse("2026-10-03T13:00:00+02:00");

        entity.MarkUpdated(updatedAt);

        Assert.Equal(updatedAt, entity.UpdatedAt);
    }

    private sealed class TestAuditableEntity(Guid id, DateTimeOffset createdAt)
        : AuditableEntity<Guid>(id, createdAt);
}
