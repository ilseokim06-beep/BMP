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
    public Task<ActionResult<IReadOnlyList<Part>>> GetAll() =>
        LoadPartsAsync();

    /// <summary>CPU 제품만 반환</summary>
    [HttpGet("cpu")]
    public Task<ActionResult<IReadOnlyList<Part>>> GetCpu() =>
        LoadPartsAsync(p => string.Equals(p.Category, "CPU", StringComparison.OrdinalIgnoreCase));

    /// <summary>GPU 제품만 반환</summary>
    [HttpGet("gpu")]
    public Task<ActionResult<IReadOnlyList<Part>>> GetGpu() =>
        LoadPartsAsync(p => string.Equals(p.Category, "GPU", StringComparison.OrdinalIgnoreCase));

    /// <summary>제품명 부분 검색 (대소문자 무시)</summary>
    [HttpGet("search")]
    public async Task<ActionResult<IReadOnlyList<Part>>> Search([FromQuery] string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(new { message = "name 쿼리 파라미터를 지정하세요. 예: ?name=5600" });

        return await LoadPartsAsync(p =>
            p.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<ActionResult<IReadOnlyList<Part>>> LoadPartsAsync(Func<Part, bool>? filter = null)
    {
        try
        {
            if (!System.IO.File.Exists(JsonFilePath))
            {
                logger.LogWarning("parts.json 없음: {Path}", JsonFilePath);
                return NotFound(new { message = "Data/parts.json 파일을 찾을 수 없습니다." });
            }

            await using var stream = System.IO.File.OpenRead(JsonFilePath);
            var parts = await JsonSerializer.DeserializeAsync<List<Part>>(stream, JsonOptions);

            if (parts is null)
            {
                logger.LogWarning("parts.json 역직렬화 결과가 null입니다.");
                return StatusCode(500, new { message = "부품 데이터를 읽을 수 없습니다." });
            }

            var result = filter is null
                ? parts
                : parts.Where(filter).ToList();

            return Ok((IReadOnlyList<Part>)result);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "parts.json JSON 파싱 오류");
            return StatusCode(500, new { message = "parts.json 형식이 올바르지 않습니다.", detail = ex.Message });
        }
        catch (IOException ex)
        {
            logger.LogError(ex, "parts.json 파일 읽기 오류");
            return StatusCode(500, new { message = "parts.json 파일을 읽는 중 오류가 발생했습니다." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "parts.json 처리 중 예기치 않은 오류");
            return StatusCode(500, new { message = "부품 데이터 처리 중 오류가 발생했습니다." });
        }
    }
}
