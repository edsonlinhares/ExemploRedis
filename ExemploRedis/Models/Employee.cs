using System;

namespace ExemploRedis.Models
{
    public class Employee
    {
        protected Employee() { }

        public Employee(string name, int age)
        {
            Id = Guid.NewGuid();
            Name = name;
            Age = age;
        }

        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public int Age { get; private set; }
    }
}
