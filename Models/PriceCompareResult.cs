namespace BMPGAME.Models;

/// <summary>가격 비교 결과</summary>
public class PriceCompareResult
{
    public IReadOnlyList<HardwareProduct> Products { get; set; } = [];
    public HardwareProduct? Cheapest { get; set; }
    public HardwareProduct? MostExpensive { get; set; }
    public decimal PriceDifferenceKrw { get; set; }
}
