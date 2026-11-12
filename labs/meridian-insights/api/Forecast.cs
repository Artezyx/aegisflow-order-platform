namespace Meridian.Api;
public static class Forecast
{
    public static decimal Weighted(decimal amount, decimal probabilityPercent)
    {
        if (amount < 0 || probabilityPercent < 0 || probabilityPercent > 100)
            throw new ArgumentOutOfRangeException();
        return decimal.Round(amount * probabilityPercent / 100m, 2, MidpointRounding.AwayFromZero);
    }
    public static decimal MarginPercent(decimal revenue, decimal cost)
    {
        if (revenue < 0 || cost < 0) throw new ArgumentOutOfRangeException();
        return revenue == 0m ? 0m : decimal.Round((revenue - cost) * 100m / revenue, 2);
    }
}
