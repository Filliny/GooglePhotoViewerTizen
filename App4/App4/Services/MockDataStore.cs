using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App4.Models;

namespace App4.Services
{
    public class MockDataStore : IDataStore<ItemInst>
    {
        readonly List<ItemInst> items;

        public MockDataStore()
        {
            items = new List<ItemInst>()
            {
                new ItemInst { Id = Guid.NewGuid().ToString(), Text = "First itemInst", Description="This is an itemInst description." },
                new ItemInst { Id = Guid.NewGuid().ToString(), Text = "Second itemInst", Description="This is an itemInst description." },
                new ItemInst { Id = Guid.NewGuid().ToString(), Text = "Third itemInst", Description="This is an itemInst description." },
                new ItemInst { Id = Guid.NewGuid().ToString(), Text = "Fourth itemInst", Description="This is an itemInst description." },
                new ItemInst { Id = Guid.NewGuid().ToString(), Text = "Fifth itemInst", Description="This is an itemInst description." },
                new ItemInst { Id = Guid.NewGuid().ToString(), Text = "Sixth itemInst", Description="This is an itemInst description." }
            };
        }

        public async Task<bool> AddItemAsync(ItemInst itemInst)
        {
            items.Add(itemInst);

            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateItemAsync(ItemInst itemInst)
        {
            var oldItem = items.Where((ItemInst arg) => arg.Id == itemInst.Id).FirstOrDefault();
            items.Remove(oldItem);
            items.Add(itemInst);

            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteItemAsync(string id)
        {
            var oldItem = items.Where((ItemInst arg) => arg.Id == id).FirstOrDefault();
            items.Remove(oldItem);

            return await Task.FromResult(true);
        }

        public async Task<ItemInst> GetItemAsync(string id)
        {
            return await Task.FromResult(items.FirstOrDefault(s => s.Id == id));
        }

        public async Task<IEnumerable<ItemInst>> GetItemsAsync(bool forceRefresh = false)
        {
            return await Task.FromResult(items);
        }
    }
}