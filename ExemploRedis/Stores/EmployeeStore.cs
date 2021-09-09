using Bogus;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using ExemploRedis.Models;

namespace ExemploRedis.Stores
{
    public class EmployeeStore : IEmployeeStore
    {
        private List<Employee> _employees;

        public EmployeeStore()
        {
            var testOrders = new Faker<Employee>()
                .RuleFor(o => o.Id, f => Guid.NewGuid())
                .RuleFor(o => o.Age, f => f.Random.Int(18, 55))
                .RuleFor(o => o.Name, f => f.Person.FullName);

            _employees = testOrders.Generate(2);
        }

        public Task Adicionar(Employee obj)
        {
            var _obj = _employees.FirstOrDefault(x => x.Id == obj.Id);
            if (_obj is null)
            {
                _employees.Add(obj);
            }

            return Task.CompletedTask;
        }

        public Task Atualizar(Employee obj)
        {
            var _obj = _employees.FirstOrDefault(x => x.Id == obj.Id);
            if (_obj != null)
            {
                _employees.Remove(_obj);
                _employees.Add(obj);
            }
            return Task.CompletedTask;
        }

        public Task Remover(Employee obj)
        {
            var _obj = _employees.FirstOrDefault(x => x.Id == obj.Id);
            if (_obj != null)
            {
                _employees.Remove(_obj);
            }
            return Task.CompletedTask;
        }

        public Task<Employee> Obter(Guid id)
        {
            return Task.FromResult(_employees.FirstOrDefault(x => x.Id == id));
        }

        public Task<List<Employee>> Listar()
        {
            return Task.FromResult(_employees);
        }
    }
}
