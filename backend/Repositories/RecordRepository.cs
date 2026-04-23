using TireControl.Api.Models;

namespace TireControl.Api.Repositories;

public class RecordRepository
{
    private readonly List<Record> _records =
    [
        new()
        {
            Type = "vehicle",
            Plate = "JPO569",
            Center = "Bogotá",
            Status = "activo"
        },
        new()
        {
            Type = "tire",
            TireId = "LL-001",
            Brand = "Michelin",
            Dimension = "12R22.5",
            Status = "stock",
            Center = "Bogotá"
        }
    ];

    public IEnumerable<Record> GetAll() => _records;

    public Record Create(Record record)
    {
        record.Id = Guid.NewGuid();
        record.CreatedAt = DateTime.UtcNow;
        _records.Add(record);
        return record;
    }

    public Record? Update(Guid id, Record payload)
    {
        var index = _records.FindIndex(r => r.Id == id);
        if (index == -1) return null;

        payload.Id = id;
        payload.CreatedAt = _records[index].CreatedAt;
        _records[index] = payload;
        return payload;
    }

    public bool Delete(Guid id)
    {
        var record = _records.FirstOrDefault(r => r.Id == id);
        if (record is null) return false;
        _records.Remove(record);
        return true;
    }
}
