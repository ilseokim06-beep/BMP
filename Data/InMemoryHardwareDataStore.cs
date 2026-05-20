using BMPGAME.Models;

namespace BMPGAME.Data;

/// <summary>메모리 기반 샘플 데이터 (추후 DB로 교체 가능)</summary>
public class InMemoryHardwareDataStore : IHardwareDataStore
{
    private static readonly List<HardwareProduct> Products =
    [
        new() { Id = 1, Category = "Cpu", Brand = "AMD", Name = "Ryzen 5 7600", PriceKrw = 249_000, SpecSummary = "6코어 / 12스레드" },
        new() { Id = 2, Category = "Cpu", Brand = "Intel", Name = "Core i5-14600K", PriceKrw = 329_000, SpecSummary = "14코어 (6P+8E)" },
        new() { Id = 3, Category = "Cpu", Brand = "AMD", Name = "Ryzen 7 7800X3D", PriceKrw = 489_000, SpecSummary = "8코어 / 3D V-Cache" },
        new() { Id = 4, Category = "Gpu", Brand = "NVIDIA", Name = "GeForce RTX 4060", PriceKrw = 449_000, SpecSummary = "8GB GDDR6" },
        new() { Id = 5, Category = "Gpu", Brand = "AMD", Name = "Radeon RX 7600", PriceKrw = 399_000, SpecSummary = "8GB GDDR6" },
        new() { Id = 6, Category = "Gpu", Brand = "NVIDIA", Name = "GeForce RTX 4070 Super", PriceKrw = 899_000, SpecSummary = "12GB GDDR6X" },
    ];

    public IReadOnlyList<HardwareProduct> GetAll() => Products;

    public IReadOnlyList<HardwareProduct> GetByCategory(string category) =>
        Products.Where(p => string.Equals(p.Category, category, StringComparison.OrdinalIgnoreCase)).ToList();

    public HardwareProduct? GetById(int id) =>
        Products.FirstOrDefault(p => p.Id == id);

    public IReadOnlyList<HardwareProduct> GetByIds(IEnumerable<int> ids)
    {
        var idSet = ids.ToHashSet();
        return Products.Where(p => idSet.Contains(p.Id)).ToList();
    }
}
