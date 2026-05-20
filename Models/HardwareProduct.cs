namespace BMPGAME.Models;

/// <summary>CPU 또는 GPU 제품 정보</summary>
public class HardwareProduct
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public decimal PriceKrw { get; set; }
    public string? SpecSummary { get; set; }
}
