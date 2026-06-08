using System;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using GymFitnessSystem.Data;

namespace GymFitnessSystem.Services
{
    // implementarea regulilor din IRepository<T>
    public class GenericRepository<T> : IRepository<T> where T : class
    {
        private readonly GymContext _context; // Conexiunea la baza de date
        private readonly DbSet<T> _dbSet;     // Tabelul specific ales

        // Constructorul: cere contextul ca să poată lucra cu SQL
        public GenericRepository(GymContext context)
        {
            _context = context;
            _dbSet = context.Set<T>(); // tabel de tip T 
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync(); // Rulează SELECT * FROM Tabel asincron
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id); // Caută după cheia primară
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity); // Pregătește insert-ul
            await _context.SaveChangesAsync(); // Salvează efectiv în baza de date
        }

        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity); // Pregătește update-ul
            await _context.SaveChangesAsync(); // Salvează modificările
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity); // Șterge rândul
                await _context.SaveChangesAsync(); // Salvează modificarea în SQL
            }
        }
    }
}
