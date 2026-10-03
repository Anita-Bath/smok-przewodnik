using AB.SmokPrzewodnik.Domain.Common;
using Xunit;

namespace AB.SmokPrzewodnik.Domain.UnitTests.Common;

public sealed class EntityTests
{
    [Fact]
    public void EntitiesWithSameAssignedIdAndType_AreEqual()
    {
        var id = Guid.NewGuid();

        Assert.Equal(new TestEntity(id), new TestEntity(id));
    }

    [Fact]
    public void EntitiesWithSameIdButDifferentTypes_AreNotEqual()
    {
        var id = Guid.NewGuid();

        Assert.NotEqual<Entity<Guid>>(new TestEntity(id), new OtherTestEntity(id));
    }

    [Fact]
    public void DistinctEntitiesWithDefaultIds_AreNotEqual()
    {
        Assert.NotEqual(new TestEntity(Guid.Empty), new TestEntity(Guid.Empty));
    }

    [Fact]
    public void EntityComparedWithItself_IsEqual()
    {
        var entity = new TestEntity(Guid.Empty);

        Assert.Equal(entity, entity);
    }

    [Fact]
    public void EqualEntities_HaveEqualHashCodes()
    {
        var id = Guid.NewGuid();

        Assert.Equal(new TestEntity(id).GetHashCode(), new TestEntity(id).GetHashCode());
    }

    [Fact]
    public void EqualityOperators_FollowEntityEquality()
    {
        var id = Guid.NewGuid();
        var left = new TestEntity(id);
        var right = new TestEntity(id);

        Assert.True(left == right);
        Assert.False(left != right);
    }

    private sealed class TestEntity(Guid id) : Entity<Guid>(id);

    private sealed class OtherTestEntity(Guid id) : Entity<Guid>(id);
}
