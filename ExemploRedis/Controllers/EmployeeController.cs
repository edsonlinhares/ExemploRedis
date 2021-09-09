using System;
using System.Threading.Tasks;
using ExemploRedis.Models;
using ExemploRedis.Stores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ExemploRedis.Controllers
{
    [ApiController]
    [Route("/api/employee")]
    public class EmployeeController : ControllerBase
    {
        private readonly ILogger<EmployeeController> _logger;
        private readonly IEmployeeStore _employeeStore;

        public EmployeeController(ILogger<EmployeeController> logger, IEmployeeStore employeeStore)
        {
            _logger = logger;
            _employeeStore = employeeStore;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var model = await _employeeStore.Obter(id);
            return Ok(model);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var lista = await _employeeStore.Listar();
            return Ok(lista);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Employee employee)
        {
            await _employeeStore.Adicionar(employee);
            return Ok(employee);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Employee employee)
        {
            await _employeeStore.Atualizar(employee);
            return Ok(employee);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] Employee employee)
        {
            await _employeeStore.Remover(employee);
            return Ok(employee);
        }
    }
}
