using OutlayApp.Domain.Primitives;

namespace OutlayApp.Domain.CompanyLogoReferences;

public class LogoReference : Entity, IAggregateRoot
{
    public string Name { get; private set; }
    public string Url { get; private set; }
    public DateTime LastTimeRetrieved { get; private set; }

    private LogoReference()
        : base(Guid.NewGuid())
    {
    }

    private LogoReference(Guid id, string name, string url, DateTime lastTimeRetrieved) 
        : base(id)
    {
        Name = name;
        Url = url;
        LastTimeRetrieved = lastTimeRetrieved;
    }
    
    public static LogoReference Create(string name, string url, DateTime lastTimeRetrieved)
    {
        return new LogoReference(Guid.NewGuid(), name, url, lastTimeRetrieved);
    }
}