using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using StudentAPI.Model;
using StudentAPI.Repository;
using System.Xml.Serialization;
using AutoMapper;
using System.Threading.Tasks;

namespace StudentAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IMapper _mapper;

        public StudentController(IMapper mapper)
        {
            _mapper = mapper;
        }

        [HttpGet]
        public IEnumerable<Student> Get([FromQuery] int pageIndex, [FromQuery] int pageSize)
        {
            var students = new StudentRepository().GetStudents(pageIndex, pageSize);
            return students;
        }

        
        [HttpPost]
        [Route("PostStudent")]
        public ActionResult<Student> PostStudent()
        {
            var form = HttpContext.Request.Form;
            Student student = new Student
            {
                Name = form["name"],
                LastName = form["lastName"],
                Age = Convert.ToByte(form["age"]),
                BioFile = form.Files.GetFile("File"),
                RegisterUser = form["registerUser"]
            };

            var studentResult = new StudentRepository().SaveStudent(student);

            return Ok(studentResult);
        }

        [Route("DownloadBioStudent")]
        public async Task<ActionResult> DownloadBioStudent([FromQuery] string bioUrl)
        {
            var mimeType = getMimeType(bioUrl);
            
            var bytes = await System.IO.File.ReadAllBytesAsync(bioUrl);

            return File(bytes, mimeType, Path.GetFileName(bioUrl));
        }

        private string getMimeType(string bioUrl)
        {
            var extension = Path.GetExtension(bioUrl);

            return extension == ".pdf" ? "application/pdf" :
                   extension == ".txt" ? "text/plain" :
                   extension == ".doc" ? "application/msword" :
                   extension == ".docx" ? "application/msword" : "Not Soported";
                   
        }

        [HttpPost]
        [Route("PostMultipleStudents")]
        public ActionResult<List<Student>> PostMultipleStudents()
        {

            try
            {
                List<Student> students = null;

                try
                {
                    students = getDeserializeXmlStudents();
                }
                catch (Exception)
                {
                    return BadRequest("Couldn't read the xml file");
                }

                var studenntsDTO = _mapper.Map<List<StudentDTO>>(students);

                var insertedStudents = new StudentRepository().SaveMultipleStudents(studenntsDTO);

                return Ok(insertedStudents);
            }
            catch (Exception)
            {
                return BadRequest("Couldn't insert the students");
            }

        }

        private List<Student> getDeserializeXmlStudents()
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
