using Microsoft.Extensions.Configuration;
using StudentAPI.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace StudentAPI.Repository
{
    public class StudentRepository
    {
        readonly IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        public IEnumerable<Student> GetStudents(int pageIndex, int pageSize)
        {
            string connectionString = config["ConnectionStrings:StudentDB"];

            IEnumerable<Student> students = null;

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
                                             BiographyFileName = d.Field<string>("biographyFileName")
                                         }).ToList();

                    }
                }
            }

            return students;
        }
    }
}
