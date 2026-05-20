using System.Text.Json;
using BMPGAME.Models;
using Microsoft.AspNetCore.Mvc;

namespace BMPGAME.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(IWebHostEnvironment environment, ILogger<ProductsController> logger) : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private string JsonFilePath => Path.Combine(environment.ContentRootPath, "Data", "parts.json");

    /// <summary>전체 제품 목록</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Part>>> GetAll() =>
        ToListResult(await TryLoadPartsAsync());

    /// <summary>CPU 제품만 반환</summary>
    [HttpGet("cpu")]
    public async Task<ActionResult<IReadOnlyList<Part>>> GetCpu() =>
        ToListResult(await TryLoadPartsAsync(), p => string.Equals(p.Category, "CPU", StringComparison.OrdinalIgnoreCase));

    /// <summary>GPU 제품만 반환</summary>
    [HttpGet("gpu")]
    public async Task<ActionResult<IReadOnlyList<Part>>> GetGpu() =>
        ToListResult(await TryLoadPartsAsync(), p => string.Equals(p.Category, "GPU", StringComparison.OrdinalIgnoreCase));

    /// <summary>제품명 부분 검색 (대소문자 무시)</summary>
    [HttpGet("search")]
    public async Task<ActionResult<IReadOnlyList<Part>>> Search([FromQuery] string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(new { message = "name 쿼리 파라미터를 지정하세요. 예: ?name=5600" });

        return ToListResult(
            await TryLoadPartsAsync(),
            p => p.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>카테고리·브랜드·가격 범위로 필터 (쿼리 조합 가능)</summary>
    [HttpGet("filter")]
    public async Task<ActionResult<IReadOnlyList<Part>>> Filter(
        [FromQuery] string? category,
        [FromQuery] string? brand,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice)
    {
        if (minPrice is not null && maxPrice is not null && minPrice > maxPrice)
            return BadRequest(new { message = "minPrice는 maxPrice보다 클 수 없습니다." });

        return ToListResult(
            await TryLoadPartsAsync(),
            p =>
                (string.IsNullOrWhiteSpace(category) ||
                 string.Equals(p.Category, category, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrWhiteSpace(brand) ||
                 string.Equals(p.Brand, brand, StringComparison.OrdinalIgnoreCase)) &&
                (!minPrice.HasValue || p.Price >= minPrice.Value) &&
                (!maxPrice.HasValue || p.Price <= maxPrice.Value));
    }

    /// <summary>정렬된 전체 제품 목록</summary>
    [HttpGet("sort")]
    public async Task<ActionResult<IReadOnlyList<Part>>> Sort([FromQuery] string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return BadRequest(new
            {
                message = "sortBy 쿼리 파라미터를 지정하세요.",
                allowed = new[] { "priceAsc", "priceDesc", "scoreDesc", "nameAsc" }
            });

        var (error, parts) = await TryLoadPartsAsync();
        if (error is not null)
            return error;

        var key = sortBy.Trim();
        IEnumerable<Part> ordered = key switch
        {
            _ when string.Equals(key, "priceAsc", StringComparison.OrdinalIgnoreCase) =>
                parts!.OrderBy(p => p.Price).ThenBy(p => p.Id),
            _ when string.Equals(key, "priceDesc", StringComparison.OrdinalIgnoreCase) =>
                parts!.OrderByDescending(p => p.Price).ThenBy(p => p.Id),
            _ when string.Equals(key, "scoreDesc", StringComparison.OrdinalIgnoreCase) =>
                parts!.OrderByDescending(p => p.Score).ThenBy(p => p.Id),
            _ when string.Equals(key, "nameAsc", StringComparison.OrdinalIgnoreCase) =>
                parts!.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ThenBy(p => p.Id),
            _ => null!
        };

        if (ordered is null)
        {
            return BadRequest(new
            {
                message = $"지원하지 않는 sortBy 값입니다: {sortBy}",
                allowed = new[] { "priceAsc", "priceDesc", "scoreDesc", "nameAsc" }
            });
        }

        return Ok(ordered.ToList());
    }

    /// <summary>두 상품 정보 비교 (성능 점수·가격 등)</summary>
    [HttpGet("compare")]
    public async Task<IActionResult> Compare([FromQuery] int? id1, [FromQuery] int? id2)
    {
        if (id1 is null || id2 is null)
            return BadRequest(new { message = "id1, id2 쿼리를 모두 지정하세요. 예: ?id1=1&id2=2" });

        var (error, parts) = await TryLoadPartsAsync();
        if (error is not null)
            return error;

        var product1 = parts!.FirstOrDefault(p => p.Id == id1.Value);
        var product2 = parts.FirstOrDefault(p => p.Id == id2.Value);

        if (product1 is null || product2 is null)
        {
            return NotFound(new
            {
                message = "지정한 id에 해당하는 제품을 찾을 수 없습니다.",
                id1 = id1.Value,
                id2 = id2.Value,
                missingId1 = product1 is null,
                missingId2 = product2 is null
            });
        }

        return Ok(new
        {
            id1 = id1.Value,
            product1,
            id2 = id2.Value,
            product2
        });
    }

    /// <summary>제품 단건 조회</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Part>> GetById(int id)
    {
        var (error, parts) = await TryLoadPartsAsync();
        if (error is not null)
            return error;

        var part = parts!.FirstOrDefault(p => p.Id == id);
        return part is null
            ? NotFound(new { message = $"id {id}에 해당하는 제품을 찾을 수 없습니다." })
            : Ok(part);
    }

    private ActionResult<IReadOnlyList<Part>> ToListResult(
        (ActionResult? Error, List<Part>? Parts) loadResult,
        Func<Part, bool>? filter = null)
    {
        if (loadResult.Error is not null)
            return loadResult.Error;

        var items = filter is null
            ? loadResult.Parts!
            : loadResult.Parts!.Where(filter).ToList();

        return Ok((IReadOnlyList<Part>)items);
    }

    private async Task<(ActionResult? Error, List<Part>? Parts)> TryLoadPartsAsync()
    {
        try
        {
            if (!System.IO.File.Exists(JsonFilePath))
            {
                logger.LogWarning("parts.json 없음: {Path}", JsonFilePath);
                return (NotFound(new { message = "Data/parts.json 파일을 찾을 수 없습니다." }), null);
            }

            await using var stream = System.IO.File.OpenRead(JsonFilePath);
            var parts = await JsonSerializer.DeserializeAsync<List<Part>>(stream, JsonOptions);

            if (parts is null)
            {
                logger.LogWarning("parts.json 역직렬화 결과가 null입니다.");
                return (StatusCode(500, new { message = "부품 데이터를 읽을 수 없습니다." }), null);
            }

            return (null, parts);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "parts.json JSON 파싱 오류");
            return (StatusCode(500, new { message = "parts.json 형식이 올바르지 않습니다.", detail = ex.Message }), null);
        }
        catch (IOException ex)
        {
            logger.LogError(ex, "parts.json 파일 읽기 오류");
            return (StatusCode(500, new { message = "parts.json 파일을 읽는 중 오류가 발생했습니다." }), null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "parts.json 처리 중 예기치 않은 오류");
            return (StatusCode(500, new { message = "부품 데이터 처리 중 오류가 발생했습니다." }), null);
        }
    }
}
