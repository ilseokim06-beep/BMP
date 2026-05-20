using BMPGAME.Data;
using BMPGAME.Models;
using Microsoft.AspNetCore.Mvc;

namespace BMPGAME.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompareController(IHardwareDataStore dataStore) : ControllerBase
{
    /// <summary>선택한 제품들의 가격 비교</summary>
    [HttpPost]
    public ActionResult<PriceCompareResult> Compare([FromBody] PriceCompareRequest request)
    {
        if (request.ProductIds.Count < 2)
            return BadRequest(new { message = "비교하려면 제품 ID를 2개 이상 지정하세요." });

        var products = dataStore.GetByIds(request.ProductIds);
        if (products.Count < 2)
            return NotFound(new { message = "유효한 제품을 2개 이상 찾을 수 없습니다." });

        var ordered = products.OrderBy(p => p.PriceKrw).ToList();
        var cheapest = ordered[0];
        var mostExpensive = ordered[^1];

        return Ok(new PriceCompareResult
        {
            Products = ordered,
            Cheapest = cheapest,
            MostExpensive = mostExpensive,
            PriceDifferenceKrw = mostExpensive.PriceKrw - cheapest.PriceKrw
        });
    }

    /// <summary>쿼리로 ID 목록 전달 (GET 비교)</summary>
    [HttpGet]
    public ActionResult<PriceCompareResult> CompareByQuery([FromQuery] int[] ids)
    {
        if (ids.Length < 2)
            return BadRequest(new { message = "비교하려면 ids 쿼리에 2개 이상의 ID를 넣으세요. 예: ?ids=1&ids=2" });

        return Compare(new PriceCompareRequest { ProductIds = ids });
    }
}
