using System;
using System.Linq.Expressions;

namespace AttractionCatalog.Domain.Core.Attractions.Ports
{
    public interface IQuerySpecification<T>
    {
        Expression<Func<T, bool>> ToExpression();
    }
}
