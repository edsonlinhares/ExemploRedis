using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExemploRedis.Models;

namespace ExemploRedis.Stores
{
    public interface IEmployeeStore
    {
        Task Adicionar(Employee obj);
        Task Atualizar(Employee obj);
        Task Remover(Employee obj);
        Task<Employee> Obter(Guid id);
        Task<IEnumerable<Employee>> Listar();
    }
}
