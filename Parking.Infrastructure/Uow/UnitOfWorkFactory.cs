using Microsoft.EntityFrameworkCore;
using Parking.Domain.Contracts;
using Parking.Infrastructure.Context;

namespace Parking.Infrastructure.Uow;

public class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly Func<IUnitOfWork> _unitOfWorkFactory;

    public UnitOfWorkFactory(Func<IUnitOfWork> unitOfWorkFactory)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public IUnitOfWork Create()
    {
        return _unitOfWorkFactory();
    }
}
