namespace BTreeSlim.Tests;

public sealed class RandomizedReferenceTests
{
    [Test]
    public void Do()
    {
        var random = new Random(Seed: 124605413);

        var tree = BTree.CreateForPairs<int, string>();

        var index = new Dictionary<int, string>();

        for (var i = 0; i < 10_000; i++)
        {
            var value = string.Create(
                length: random.Next(1, 9),
                random,
                static (stringContent, random) =>
                {
                    for (var i = 0; i < stringContent.Length; i++)
                    {
                        stringContent[i] = (char)random.Next('a', 'z' + 1);
                    }
                }
            );

            int key;
            while (true)
            {
                key = random.Next(100_000);
                if (index.TryAdd(key, value))
                {
                    break;
                }
            }

            tree.TryAdd(new Pair<int, string>(key, value)).ThrowOnConflict();
        }

        var list = new SortedList<int, string>(index);

        var sut = tree.CreateCursor();
        var sutReference = new SortedListCursor<int, string>(list);

        sut.Range = new Range<int>(Border.Inclusive(55_000), Border.Inclusive(55_000));
        sutReference.Range = sut.Range;

        sut.MoveNext();
        sutReference.MoveNext();
    }

    [Test]
    public void Do2()
    {
        var random = new Random(Seed: 124605413);

        var tree = BTree.CreateForPairs<int, string>();

        var index = new Dictionary<int, string>();

        for (var i = 0; i < 10_000; i++)
        {
            var value = string.Create(
                length: random.Next(1, 9),
                random,
                static (stringContent, random) =>
                {
                    for (var i = 0; i < stringContent.Length; i++)
                    {
                        stringContent[i] = (char)random.Next('a', 'z' + 1);
                    }
                }
            );

            int key;
            while (true)
            {
                key = random.Next(100_000);
                if (index.TryAdd(key, value))
                {
                    break;
                }
            }

            tree.TryAdd(new Pair<int, string>(key, value)).ThrowOnConflict();
        }

        var list = new SortedList<int, string>(index);

        var sut = tree.CreateCursor();
        var sutReference = new SortedListCursor<int, string>(list);

        sut.Range = new Range<int>(Border.Inclusive(55_000), Border.Inclusive(75_000));
        sutReference.Range = sut.Range;

        sut.MoveNext();
        sutReference.MoveNext();
        
        for (var i = 0; i < 1000; i++)
        {
            sut.Remove(Move.Next);
        }
    }
}