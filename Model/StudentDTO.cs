using System;
namespace StudentAPI.Model
{
    public class StudentDTO
    {
        public string Name { get; set; }
        public string LastName { get; set; }
        public byte Age { get; set; }
        public string RegisterUser { get; set; } = "Temp User";
    }
}
