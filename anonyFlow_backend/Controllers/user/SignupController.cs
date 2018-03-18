using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace anonyFlow_backend.Controllers.user
{
    [Route("api/user/[controller]")]
    public class SignupController : Controller
    {

        [HttpPost]
        public Response Post([FromBody]dynamic obj)
        {
            var response = new List<Response>();

            string username = obj.username;
            string email = obj.email;
            string password = obj.password;

            string passwordEncrypt = new Modules().sha256(password);

            if(userExists(username)) {
                return new Response(false, "Username is already taken!");
            }
            SqlConnectionStringBuilder builder = WebApiConfig.Connection();

            try
            {
                string sql = "INSERT INTO users (user_name, user_password, user_email, user_settings_show_in_flow) VALUES (@username, @password, @email, 0);";

                using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", passwordEncrypt);
                        cmd.Parameters.AddWithValue("@email", email);

                        int i = cmd.ExecuteNonQuery();
                        if(i > 0) {
                            GetUserIdByUsernameClass userId = new GetUserIdByUsernameClass(username);
                            UserId resp = userId.getId();

                            if (resp.didSucceed())
                            {
                                return new Response(true, resp.userId.ToString());
                            }

                            return new Response(false, resp.message);
                        }

                        return new Response(false, "Could not create account!");

                    }
                }
            }
            catch (SqlException ex)
            {
                string msg = "Insert Error:";
                msg += ex.Message;
                return new Response(false, msg);
            }
        }

        /*
         * @param string username
         * 
         * @returns bool if user exist.
         */
        public bool userExists(string username) {
            try
            {
                SqlConnectionStringBuilder builder = WebApiConfig.Connection();
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();

                    sb.Append("SELECT " +
                     "[user_name] " +
                      "FROM [dbo].[users] " +
                              "WHERE [user_name] = '" + username + "'"
                     );

                    String sql = sb.ToString();

                    Console.WriteLine(sql);

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // return true/false if username exists
                            Console.WriteLine("HAS rows: " + reader.HasRows);
                            return reader.HasRows; 
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
                return true;
            }
        }
    }
}
