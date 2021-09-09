using System;

namespace ExemploRedis.Models
{
    public class Employee
    {
        public Employee()
        {
            Id = Guid.NewGuid();
        }

        public Employee(string name, int age)
        {
            Id = Guid.NewGuid();
            Name = name;
            Age = age;
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
