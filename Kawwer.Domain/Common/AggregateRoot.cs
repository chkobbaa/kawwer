namespace Kawwer.Domain.Common;

/// <summary>
/// Marks an entity as an aggregate root. Repositories exist only for aggregate roots.
/// </summary>
public abstract class AggregateRoot : Entity
{
}
