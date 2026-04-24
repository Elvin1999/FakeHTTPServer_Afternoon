using FakeHTTPServer.DataAccess;
using FakeHTTPServer.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeHTTPServer.Repository
{
   public  class ProductRepository
    {
        private readonly StepDBContext _context;

        public ProductRepository(StepDBContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetAllAsync()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<Product?> GetAsync(int id)
        {
            return await _context.Products.SingleOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            return (await _context.SaveChangesAsync()) > 0;
        }

        public async Task<bool> UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            return (await _context.SaveChangesAsync()) > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var item = await _context.Products.SingleOrDefaultAsync(p => p.Id == id);
            if (item != null)
            {
                _context.Products.Remove(item);
            }

            return (await _context.SaveChangesAsync()) > 0;
        }
    }
}
