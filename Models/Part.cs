namespace BMPGAME.Models;

/// <summary>CPU/GPU 부품 정보</summary>
public class Part
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Usage { get; set; }
    public string? Tag { get; set; }
    public double Score { get; set; }
}
