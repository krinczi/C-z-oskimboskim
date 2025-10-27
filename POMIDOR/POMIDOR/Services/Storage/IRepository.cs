using System.Collections.Generic;

namespace POMIDOR.Services.Storage
{
    public interface IRepository<T>
    {
        IReadOnlyList<T> LoadAll();
        void SaveAll(IEnumerable<T> items);
        void Append(T item);
    }
}
