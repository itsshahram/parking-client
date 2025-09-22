namespace Parking.Domain.Contracts;

public interface IUnitOfWorkFactory
{
    IUnitOfWork Create();
}
