using BMPGAME.Models;

namespace BMPGAME.Data;

/// <summary>하드웨어 제품 데이터 접근</summary>
public interface IHardwareDataStore
{
    IReadOnlyList<HardwareProduct> GetAll();
    IReadOnlyList<HardwareProduct> GetByCategory(string category);
    HardwareProduct? GetById(int id);
    IReadOnlyList<HardwareProduct> GetByIds(IEnumerable<int> ids);
}
