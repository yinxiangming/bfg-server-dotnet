using Microsoft.EntityFrameworkCore;

namespace Bfg.Core.Common;

/// <summary>
/// Read-only mapping to django_content_type for GenericForeignKey resolution.
/// </summary>
[Keyless]
public class DjangoContentType
{
    public int Id { get; set; }
    public string AppLabel { get; set; } = "";
    public string Model { get; set; } = "";
}
