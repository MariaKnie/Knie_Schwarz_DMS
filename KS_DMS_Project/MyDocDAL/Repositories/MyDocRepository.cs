using Microsoft.EntityFrameworkCore;
using MyDocDAL.Data;
using MyDocDAL.Entities;

namespace MyDocDAL.Repositories
{
    public class MyDocRepository(MyDocContext context) : ItMyDocRepository
    {
        public async Task<IEnumerable<MyDoc>> GetAllAsync()
        {
            return await context.MyDocItems!.ToListAsync();
        }

        public async Task<MyDoc> GetByIdAsync(int id)
        {
            return (await context.MyDocItems!.FindAsync(id))!;
        }

        public async Task AddAsync(MyDoc item)
        {
            await context.MyDocItems!.AddAsync(item);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(MyDoc item)
        {
            context.MyDocItems!.Update(item);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var item = await context.MyDocItems!.FindAsync(id);
            if (item != null)
            {
                context.MyDocItems.Remove(item);
                await context.SaveChangesAsync();
            }
        }
    }
}