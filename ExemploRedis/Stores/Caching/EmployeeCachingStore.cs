using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExemploRedis.Models;
using Newtonsoft.Json;

namespace ExemploRedis.Stores.Caching
{
    public class EmployeeCachingStore : IEmployeeStore
    {
        const string _key = "Employees";
        private readonly RedisCacheService _redisCacheService;
        private readonly IEmployeeStore _employeeStore;

        public EmployeeCachingStore(RedisCacheService redisCacheService, IEmployeeStore employeeStore)
        {
            _redisCacheService = redisCacheService;
            _employeeStore = employeeStore;
        }

        public async Task Adicionar(Employee obj)
        {
            var item = await Obter(obj.Id);

            if (item != null) return;

            await _employeeStore.Adicionar(obj);

            await _redisCacheService.GetDatabase()
                .ListRightPushAsync(_key, JsonConvert.SerializeObject(obj));
        }

        public async Task Atualizar(Employee obj)
        {
            var item = await Obter(obj.Id);

            if (item is null)
            {
                await Adicionar(obj);
                return;
            }

            await _employeeStore.Atualizar(obj);

            var index = IndexOf(item);

            if (index >= 0)
            {
                await _redisCacheService.GetDatabase()
                    .ListRemoveAsync(_key, JsonConvert.SerializeObject(item));

                await _redisCacheService.GetDatabase()
                    .ListRightPushAsync(_key, JsonConvert.SerializeObject(obj));
            }

        }

        public async Task Remover(Employee obj)
        {
            var item = await Obter(obj.Id);

            await _employeeStore.Remover(obj);

            var index = IndexOf(item);

            if (index >= 0)
            {
                await _redisCacheService.GetDatabase()
                    .ListRemoveAsync(_key, JsonConvert.SerializeObject(item));
            }
        }

        public async Task<Employee> Obter(Guid id)
        {
            var items = await ObterLista();

            return items.FirstOrDefault(x => x.Id == id);
        }

        public async Task<List<Employee>> Listar()
        {
            return await ObterLista();
        }

        private async Task<List<Employee>> ObterLista()
        {
            var items = await _redisCacheService.GetDatabase().Listar<Employee>(_key);

            if (items.Count == 0)
            {
                items = await _employeeStore.Listar();
            }

            return items;
        }

        public int IndexOf(Employee item)
        {
            var total = (int)_redisCacheService.GetDatabase().ListLength(_key);

            for (int i = 0; i < total; i++)
            {
                if (_redisCacheService.GetDatabase().ListGetByIndex(_key, i).ToString().Equals(JsonConvert.SerializeObject(item)))
                {
                    return i;
                }
            }
            return -1;
        }

    }

    public class EmployeeCachingDecorator<T> : IEmployeeStore
        where T : IEmployeeStore
    {
        const string _key = "Employees";
        private readonly RedisCacheService _redisCacheService;
        private readonly T _employeeStore;

        public EmployeeCachingDecorator(RedisCacheService redisCacheService, T employeeStore)
        {
            _redisCacheService = redisCacheService;
            _employeeStore = employeeStore;
        }

        public async Task Adicionar(Employee obj)
        {
            var item = await Obter(obj.Id);

            if (item != null) return;

            await _employeeStore.Adicionar(obj);

            await _redisCacheService.GetDatabase()
                .ListRightPushAsync(_key, JsonConvert.SerializeObject(obj));
        }

        public async Task Atualizar(Employee obj)
        {
            var item = await Obter(obj.Id);

            if (item is null)
            {
                await Adicionar(obj);
                return;
            }

            await _employeeStore.Atualizar(obj);

            var index = IndexOf(item);

            if (index >= 0)
            {
                await _redisCacheService.GetDatabase()
                    .ListRemoveAsync(_key, JsonConvert.SerializeObject(item));

                await _redisCacheService.GetDatabase()
                    .ListRightPushAsync(_key, JsonConvert.SerializeObject(obj));
            }

        }

        public async Task Remover(Employee obj)
        {
            var item = await Obter(obj.Id);

            await _employeeStore.Remover(obj);

            var index = IndexOf(item);

            if (index >= 0)
            {
                await _redisCacheService.GetDatabase()
                    .ListRemoveAsync(_key, JsonConvert.SerializeObject(item));
            }
        }

        public async Task<Employee> Obter(Guid id)
        {
            var items = await ObterLista();

            return items.FirstOrDefault(x => x.Id == id);
        }

        public async Task<List<Employee>> Listar()
        {
            return await ObterLista();
        }

        private async Task<List<Employee>> ObterLista()
        {
            var items = await _redisCacheService.GetDatabase().Listar<Employee>(_key);

            if (items.Count == 0)
            {
                items = await _employeeStore.Listar();
            }

            return items;
        }

        public int IndexOf(Employee item)
        {
            var total = (int)_redisCacheService.GetDatabase().ListLength(_key);

            for (int i = 0; i < total; i++)
            {
                if (_redisCacheService.GetDatabase().ListGetByIndex(_key, i).ToString().Equals(JsonConvert.SerializeObject(item)))
                {
                    return i;
                }
            }
            return -1;
        }

    }
}
