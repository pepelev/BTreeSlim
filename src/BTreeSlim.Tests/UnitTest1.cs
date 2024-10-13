using FluentAssertions;

namespace BTreeSlim.Tests;

public sealed class Tests
{
    [Test]
    public void Test1()
    {
        var bTree = BTree.Create<int>();

        for (var i = 19; i >= 0; i--)
        {
            bTree.TryAdd(i);
        }

        bTree.Should().Equal(Enumerable.Range(0, 20).Select(value => new Self<int>(value)));
    }
}