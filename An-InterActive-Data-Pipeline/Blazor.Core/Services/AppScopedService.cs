namespace Blazor.Core.Services;

public class AppScopedService
{
    private Guid _uid = Guid.NewGuid();

    public string SPASessionId => _uid.ToString().Substring(0, 8);
}
