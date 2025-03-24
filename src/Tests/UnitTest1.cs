using Xunit;
using Aslanta.Idgen.Api;

namespace Tests;

public class IdCacheTests
{


    [Fact]
    public async Task GetId_ShouldReturnId_WhenCacheIsEmpty()
    {
        // Arrange
        var idCache = new IdCache();

        // Act
        var id = await idCache.GetId();

        // Assert
        Assert.NotNull(id);
        Assert.Equal(7, id.Length); // Assuming IDs are 7 characters long
    }
}
