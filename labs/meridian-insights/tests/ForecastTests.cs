using Meridian.Api;
using Xunit;
public class ForecastTests
{
    [Fact] public void CalculatesWeightedAmount() => Assert.Equal(7000m,Forecast.Weighted(20000m,35m));
    [Fact] public void RejectsInvalidProbability() => Assert.Throws<ArgumentOutOfRangeException>(()=>Forecast.Weighted(100m,101m));
    [Fact] public void ComputesMargin() => Assert.Equal(20m,Forecast.MarginPercent(100m,80m));
    [Fact] public void ZeroRevenueHasZeroMargin() => Assert.Equal(0m,Forecast.MarginPercent(0m,0m));
}
