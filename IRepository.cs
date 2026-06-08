using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GymFitnessSystem.Services
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(); // Aduce toate rândurile din tabel
        Task<T> GetByIdAsync(int id);       // Caută un rând după ID
        Task AddAsync(T entity);            // Inserează un rând nou
        Task UpdateAsync(T entity);         // Salvează modificările aduse unui rând
        Task DeleteAsync(int id);           // Șterge un rând după ID
    }
}
