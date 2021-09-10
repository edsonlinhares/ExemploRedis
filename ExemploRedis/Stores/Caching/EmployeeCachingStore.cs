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

        public Task Adicionar(Employee obj)
        {
            var ts0 = new Task(() =>
            {
                _redisCacheService.Adicionar(_key, obj);
            });

            var ts1 = new Task(() =>
            {
                _employeeStore.Adicionar(obj);
            });

            ts0.Start();
            ts1.Start();

            return Task.CompletedTask;
        }

        public Task Atualizar(Employee obj)
        {
            var ts0 = new Task(() =>
            {
                _redisCacheService.Adicionar(_key, obj);
            });

            var ts1 = new Task(() =>
            {
                _employeeStore.Atualizar(obj);
            });

            ts0.Start();
            ts1.Start();

            return Task.CompletedTask;
        }

        public Task Remover(Employee obj)
        {
            var ts0 = new Task(() =>
            {
                _redisCacheService.Remover(_key);
            });

            var ts1 = new Task(() =>
            {
                _employeeStore.Remover(obj);
            });

            ts0.Start();
            ts1.Start();

            return Task.CompletedTask;
        }

        public async Task<Employee> Obter(Guid id)
        {
            var item = await _redisCacheService.Obter<Employee>(_key);

            if (item is null)
            {
                item = await _employeeStore.Obter(id);
                if (item != null)
                {
                    var ts0 = new Task(() =>
                    {
                        _redisCacheService.Adicionar(_key, item);
                    });

                    ts0.Start();
                }
            }

            return item;
        }

        public async Task<IEnumerable<Employee>> Listar()
        {
            var items = await _redisCacheService.Listar<Employee>(_key);

            if (items.Count() == 0)
            {
                items = await _employeeStore.Listar();
            }

            return items;
        }

    }

}
