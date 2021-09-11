using System;

namespace ExemploRedis.Models
{
    public class Employee
    {
        protected Employee() { }

        public Employee(Guid id, string name, int age)
        {
            Id = id;
            Name = name;
            Age = age;
        }

        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public int Age { get; private set; }
    }

    public class EmployeeViewModel
    {
        public EmployeeViewModel()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
