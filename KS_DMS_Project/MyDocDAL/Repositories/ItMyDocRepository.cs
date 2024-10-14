using System.Collections.Generic;
using System.Threading.Tasks;
using MyDocDAL.Entities;

namespace MyDocDAL.Repositories
{
    public interface ItMyDocRepository
    {
        Task<IEnumerable<MyDoc>> GetAllAsync();
        Task<MyDoc> GetByIdAsync(int id);
        Task AddAsync(MyDoc item);
        Task UpdateAsync(MyDoc item);
        Task DeleteAsync(int id);
    }
}