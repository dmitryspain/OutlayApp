using OutlayApp.Domain.Primitives;

namespace OutlayApp.Domain.CompanyLogoReferences;

public class InvalidReference : Entity, IAggregateRoot
{
    public string Name { get; private set; }
    public DateTime LastTimeRetrieved { get; private set; }

    private InvalidReference()
        : base(Guid.NewGuid())
    {
    }

    private InvalidReference(Guid id, string name, DateTime lastTimeRetrieved) 
        : base(id)
    {
        Name = name;
        LastTimeRetrieved = lastTimeRetrieved;
    }
    
    public static InvalidReference Create(string name, DateTime lastTimeRetrieved)
    {
        return new InvalidReference(Guid.NewGuid(), name, lastTimeRetrieved);
    }
}