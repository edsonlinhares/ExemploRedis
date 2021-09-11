using System;
using System.Threading.Tasks;
using AutoMapper;
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
        private readonly IMapper mapper;
        private readonly ILogger<EmployeeController> _logger;
        private readonly IEmployeeStore _employeeStore;

        public EmployeeController(IMapper mapper, ILogger<EmployeeController> logger, IEmployeeStore employeeStore)
        {
            this.mapper = mapper;
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
        public async Task<IActionResult> Post([FromBody] EmployeeViewModel employee)
        {
            var model = mapper.Map<Employee>(employee);
            await _employeeStore.Adicionar(model);
            return Ok(employee);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] EmployeeViewModel employee)
        {
            var model = mapper.Map<Employee>(employee);
            await _employeeStore.Atualizar(model);
            return Ok(employee);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var employee = await _employeeStore.Obter(id);
            if (employee != null)
                await _employeeStore.Remover(employee);
            return Ok(employee);
        }
    }
}
