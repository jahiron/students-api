using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using StudentAPI.Model;
using StudentAPI.Repository;
using System.Xml.Serialization;

namespace StudentAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<Student> Get([FromQuery] int pageIndex, [FromQuery] int pageSize)
        {
            var students = new StudentRepository().GetStudents(pageIndex, pageSize);
            return students;
        }

        
        [HttpPost]
        [Route("PostStudent")]
        public void PostStudent()
        {
            var form = HttpContext.Request.Form;
            Student student = new Student
            {
                Name = form["name"],
                LastName = form["lastName"],
                Age = Convert.ToByte(form["age"]),
                BiographyFileName = form.Files[0].FileName,
                RegisterUser = form["registerUser"]
            };
        }

        [HttpPost]
        [Route("PostMultipleStudents")]
        public IActionResult PostMultipleStudents()
        {
            IEnumerable<Student> students = null;

            try
            {
                students = getDeserializeXmlStudents();
            }
            catch (Exception)
            {
                return BadRequest("Error: Can not read the xml file");
            }
            

            return Ok(students); 

        }

        private IEnumerable<Student> getDeserializeXmlStudents()
        {
            var form = HttpContext.Request.Form;
            
            var studentsFile = form.Files.GetFile("StudentXmlFile");

            string studentsData = string.Empty;

            using (StreamReader reader = new StreamReader(studentsFile.OpenReadStream()))
            {
                studentsData = reader.ReadToEnd();
            }

            XmlRootAttribute xRoot = new XmlRootAttribute
            {
                ElementName = "students",
                IsNullable = true
            };

            XmlSerializer serializer = new XmlSerializer(typeof(List<Student>), xRoot);

            List<Student> students = new List<Student>();

            using (TextReader reader = new StringReader(studentsData))
            {
                students = (List<Student>)serializer.Deserialize(reader);
            }

            return students;
        }
    }
}
