using TerribleSettingsAuditor.Helpers;

namespace TerribleSettingsAuditor.Core.Tests.Unit.Helpers;

public class MaskingHelperTest
{
    private static readonly string LongValue = new string('a', 80) + new string('b', 80);

    [Fact]
    public void Truncate_UnderLimit_ReturnsValue()
    {
        Assert.Equal("short", MaskingHelper.Truncate("short", 100));
    }

    [Fact]
    public void Truncate_OverLimit_AddsEllipsis()
    {
        var result = MaskingHelper.Truncate(LongValue, 100);

        Assert.Equal(100, result.Length);
        Assert.EndsWith("...", result);
        Assert.StartsWith(new string('a', 80), result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    public void Truncate_NoLimit_ReturnsValue(int? maxLength)
    {
        Assert.Equal(LongValue, MaskingHelper.Truncate(LongValue, maxLength));
    }

    [Fact]
    public void MaskMiddle_OverLimit_KeepsEdgesAndShrinksMask()
    {
        var result = MaskingHelper.MaskMiddle(LongValue, 4, 4, '*', 50);

        Assert.Equal(50, result.Length);
        Assert.Equal("aaaa" + new string('*', 42) + "bbbb", result);
    }

    [Fact]
    public void MaskMiddle_FullyMasked_RespectsLimit()
    {
        Assert.Equal(new string('*', 50), MaskingHelper.MaskMiddle(LongValue, 0, 0, '*', 50));
    }

    [Fact]
    public void MaskMiddle_EdgesExceedLimit_ShrinksEdges()
    {
        var result = MaskingHelper.MaskMiddle(LongValue, 60, 0, '*', 50);

        Assert.Equal(50, result.Length);
        Assert.EndsWith("*", result);
    }

    [Fact]
    public void MaskMiddle_NoLimit_PreservesLength()
    {
        Assert.Equal(LongValue.Length, MaskingHelper.MaskMiddle(LongValue, 4, 4).Length);
    }

    [Fact]
    public void MaskMiddleWithLimits_Secret_RespectsLimit()
    {
        var result = MaskingHelper.MaskMiddleWithLimits(LongValue, 25, 0, '*', 50);

        Assert.Equal(50, result.Length);
        Assert.StartsWith(new string('a', 25), result);
    }

    [Fact]
    public void MaskMiddleWithLimits_EmptySecret_RespectsLimit()
    {
        Assert.Equal(new string('*', 50), MaskingHelper.MaskMiddleWithLimits(string.Empty, 25, 0, '*', 50));
    }
}
