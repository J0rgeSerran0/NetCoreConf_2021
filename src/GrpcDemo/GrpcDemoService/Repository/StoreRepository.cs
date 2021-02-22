using GrpcDemoService.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcDemoService.Repository
{
    public class StoreRepository
    {
        private readonly List<StoreEntity> _store = new List<StoreEntity>();

        public StoreRepository()
        {
            InitializeStore();
        }

        private void InitializeStore()
        {
            _store.Add(new StoreEntity("apples", 3, "EUR"));
            _store.Add(new StoreEntity("lemons", 2, "EUR"));
            _store.Add(new StoreEntity("oranges", 1, "EUR"));
        }

        public async Task<StoreEntity> GetByNameAsync(string name)
        {
            var store = _store.SingleOrDefault(n => n.Name == name);
            return await Task.FromResult(store);
        }

        public List<StoreEntity> GetAll() => _store;

        public void Add(StoreEntity entity) => _store.Add(entity);
    }
}