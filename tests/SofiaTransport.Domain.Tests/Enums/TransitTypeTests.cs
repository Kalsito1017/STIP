using Xunit;
using SofiaTransport.Domain.Enums;

namespace SofiaTransport.Domain.Tests.Enums;

public class TransitTypeTests
{
    [Theory]
    [InlineData(TransitType.Tram, 0)]
    [InlineData(TransitType.Metro, 1)]
    [InlineData(TransitType.Bus, 3)]
    [InlineData(TransitType.Trolley, 11)]
    public void TransitType_HasCorrectValues(TransitType type, int expectedValue)
    {
        Assert.Equal(expectedValue, (int)type);
    }
}

public class DelayBucketTests
{
    [Fact]
    public void DelayBucket_HasFourValues()
    {
        var values = Enum.GetValues<DelayBucket>();
        Assert.Equal(4, values.Length);
    }

    [Theory]
    [InlineData(DelayBucket.OnTime, 0)]
    [InlineData(DelayBucket.Slight, 1)]
    [InlineData(DelayBucket.Moderate, 2)]
    [InlineData(DelayBucket.Severe, 3)]
    public void DelayBucket_ValuesOrderCorrect(DelayBucket bucket, int expectedValue)
    {
        Assert.Equal(expectedValue, (int)bucket);
    }
}
