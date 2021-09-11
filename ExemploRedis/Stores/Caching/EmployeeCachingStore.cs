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

        TimeSpan expirationTime = TimeSpan.FromHours(4);

        private readonly ICacheService _cacheService;
        private readonly IEmployeeStore _employeeStore;

        public EmployeeCachingStore(ICacheService cacheService, IEmployeeStore employeeStore)
        {
            _cacheService = cacheService;
            _employeeStore = employeeStore;
        }

        public Task Adicionar(Employee obj)
        {
            var ts0 = new Task(() =>
            {
                _cacheService.Set<Employee>($"{_key}:{obj.Id}", obj, expirationTime);
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
                _cacheService.Set<Employee>($"{_key}:{obj.Id}", obj, expirationTime);
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
                _cacheService.Clear($"{_key}:{obj.Id}");
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
            var item = _cacheService.Get<Employee>($"{_key}:{id}");

            if (item is null)
            {
                item = await _employeeStore.Obter(id);
                if (item != null)
                {
                    var ts0 = new Task(() =>
                    {
                        _cacheService.Set<Employee>($"{_key}:{item.Id}", item, expirationTime);
                    });

                    ts0.Start();
                }
            }

            return item;
        }

        public async Task<IEnumerable<Employee>> Listar()
        {
            var items = _cacheService.GetAll<Employee>($"{_key}");

            if (items.Count() == 0)
            {
                items = await _employeeStore.Listar();

                var ts = new Task(() =>
                {
                    foreach (var item in items)
                    {
                        _cacheService.Set<Employee>($"{_key}:{item.Id}", item, expirationTime);
                    }
                });

                ts.Start();
            }

            return items;
        }

    }

}
