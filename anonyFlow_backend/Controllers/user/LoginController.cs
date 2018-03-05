using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace anonyFlow_backend.Controllers.user
{
    [Route("api/user/[controller]")]
    public class LoginController : Controller
    {
        [HttpPost]
        public Response Post([FromBody]dynamic obj)
        {
            var flow = new List<flow>();

            string username = obj.username;
            string password = obj.password;

            string passwordEncrypt = new Modules().sha256(password);

            Console.WriteLine(passwordEncrypt);

            try
            {
                // include connection..
                SqlConnectionStringBuilder builder = WebApiConfig.Connection();

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    string sql = "SELECT" +
                     " [user_id]" +
                     " FROM [dbo].[users]" +
                        " WHERE [user_name] = '" + username + "' AND [user_password] = '" + passwordEncrypt + "'";

                    Console.WriteLine(sql);

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@username", username.ToString());
                        command.Parameters.AddWithValue("@password", passwordEncrypt.ToString());

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            int rowCount = 0;
                            int id = new Int32();
                            if(reader.HasRows) {
                                while (reader.Read())
                                {
                                    ++rowCount;
                                    if(rowCount > 1) {
                                        return new Response(false, "Too many rows found! DANGER!");
                                    }
                                    id = reader.GetInt32(0);
                                }
                            } else {
                                return new Response(false, "");
                            }
                            return new Response(true, id.ToString());

                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
                return new Response(false, e.ToString());
            }
        }
    }
}
