using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using StudentAPI.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace StudentAPI.Repository
{
    public class StudentRepository
    {
        readonly IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        public List<Student> GetStudents(int pageIndex, int pageSize)
        {
            string connectionString = config["ConnectionStrings:StudentDB"];

            List<Student> students = null;

            using (SqlConnection sqlConn = new SqlConnection(connectionString))
            {
                using (SqlCommand sqlCmd = new SqlCommand("Proc_Student_Consult", sqlConn))
                {
                    sqlConn.Open();
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("PageIndex", pageIndex);
                    sqlCmd.Parameters.AddWithValue("pageSize", pageSize);

                    using (SqlDataAdapter sqlAdapter = new SqlDataAdapter(sqlCmd))
                    {
                        DataTable dt = new DataTable();
                        sqlAdapter.Fill(dt);

                        students = (from d in dt.AsEnumerable()
                                         select new Student
                                         {
                                             Id = d.Field<int>("id"),
                                             Name = d.Field<string>("name"),
                                             LastName = d.Field<string>("lastName"),
                                             Age = d.Field<byte>("age"),
                                             BioFileUrl = d.Field<string>("bioFileUrl")
                                         }).ToList();

                    }
                }
            }

            return students;
        }

        public Student SaveStudent(Student student)
        {
            string connectionString = config["ConnectionStrings:StudentDB"];
            Student studentResult = null;

         

            using (SqlConnection sqlConn = new SqlConnection(connectionString))
            {
                sqlConn.Open();
                using (SqlTransaction transaction = sqlConn.BeginTransaction("SaveStudent"))
                {
                    try
                    {
                        var directoryPath = Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData);
                        var filePath = Path.Combine(directoryPath, Guid.NewGuid().ToString()) + Path.GetExtension(student.BioFile.FileName);

                        //Save student 
                        using (SqlCommand sqlCmd = sqlConn.CreateCommand())
                        {
                            sqlCmd.Transaction = transaction;
                            sqlCmd.CommandText = "Proc_Student_Insert";
                            sqlCmd.CommandType = CommandType.StoredProcedure;
                            sqlCmd.Parameters.AddWithValue("name", student.Name);
                            sqlCmd.Parameters.AddWithValue("lastName", student.LastName);
                            sqlCmd.Parameters.AddWithValue("age", student.Age);
                            sqlCmd.Parameters.AddWithValue("bioFileUrl", filePath);
                            sqlCmd.Parameters.AddWithValue("registerUser", "Logged user");

                            using (SqlDataAdapter sqlAdapter = new SqlDataAdapter(sqlCmd))
                            {
                                DataTable dt = new DataTable();
                                sqlAdapter.Fill(dt);

                                studentResult = (from d in dt.AsEnumerable()
                                                 select new Student
                                                 {
                                                     Id = d.Field<int>("id"),
                                                     Name = d.Field<string>("name"),
                                                     LastName = d.Field<string>("lastName"),
                                                     Age = d.Field<byte>("age"),
                                                     BioFileUrl = d.Field<string>("bioFileUrl")
                                                 }).FirstOrDefault();

                            }
                        }

                        //Save file
                        using (FileStream stream = File.Create(filePath))
                        {
                            student.BioFile.CopyTo(stream);
                        }
                        

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
                    }
                   
                }
               
            }
            return studentResult;

        }

        public List<Student> SaveMultipleStudents(List<StudentDTO> students)
        {
            string connectionString = config["ConnectionStrings:StudentDB"];

            List<Student> studentsResult = null;

            using (SqlConnection sqlConn = new SqlConnection(connectionString))
            {
                using (SqlCommand sqlCmd = new SqlCommand("Proc_Student_Multiple_Insert", sqlConn))
                {
                    sqlConn.Open();
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName= "Students",
                        Value = getStudentDataTable(students),
                        SqlDbType = SqlDbType.Structured,
                        TypeName = "dbo.StudentType"
                    };
                    sqlCmd.Parameters.Add(parameter);

                    using (SqlDataAdapter sqlAdapter = new SqlDataAdapter(sqlCmd))
                    {
                        DataTable dt = new DataTable();
                        sqlAdapter.Fill(dt);

                        studentsResult = (from d in dt.AsEnumerable()
                                    select new Student
                                    {
                                        Id = d.Field<int>("id"),
                                        Name = d.Field<string>("name"),
                                        LastName = d.Field<string>("lastName"),
                                        Age = d.Field<byte>("age"),
                                        BioFileUrl = d.Field<string>("bioFileUrl")
                                    }).ToList();

                    }
                }
            }

            return studentsResult;
        }

        //public IFormFile GetBioStudentFile(string bioFileUrl)
        //{
        //    using(FileStream fl = new FileStream(bioFileUrl, FileMode.Open, FileAccess.Read))
        //    {
        //        IFormFile file = fl.Write()
        //}

        private DataTable getStudentDataTable(IList<StudentDTO> item)
        {
            Type type = typeof(StudentDTO);
            var properties = type.GetProperties();

            DataTable dataTable = new DataTable();
            foreach (PropertyInfo info in properties)
            {
                dataTable.Columns.Add(new DataColumn(info.Name, Nullable.GetUnderlyingType(info.PropertyType) ?? info.PropertyType));
            }

            foreach (StudentDTO entity in item)
            {
                object[] values = new object[properties.Length];
                for (int i = 0; i < properties.Length; i++)
                {
                    values[i] = properties[i].GetValue(entity);
                }

                dataTable.Rows.Add(values);
            }
            return dataTable;
        }
    }


}
