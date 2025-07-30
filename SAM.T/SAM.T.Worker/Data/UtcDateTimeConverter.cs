using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SAM.T.Worker.Data;

public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(
            v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(), // Write
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc))           // Read
    { }
}
