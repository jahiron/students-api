using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace StudentAPI.Model
{
    [XmlType(TypeName = "student")]
    public class Student
    {
        public int Id { get; set; }

        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "lastname")]
        public string LastName { get; set; }

        [XmlElement(ElementName = "age")]
        public byte Age { get; set; }
        public string BiographyFileName { get; set; }
        public string RegisterUser { get; set; }
    }
}
