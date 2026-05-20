namespace BMPGAME.Models;

/// <summary>가격 비교 요청 (제품 ID 목록)</summary>
public class PriceCompareRequest
{
    public IReadOnlyList<int> ProductIds { get; set; } = [];
}
