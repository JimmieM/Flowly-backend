using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Net.Http;

namespace anonyFlow_backend.Controllers
{
    public class Username
    {
        public bool success { get; set; }
        public string message { get; set; }
        public string username { get; set; }

        public Username(bool success, string message, string username)
        {
            this.success = success;
            this.message = message;
            this.username = username;
            return;
        }

        public Username(bool success, string username)
        {
            this.success = success;
            this.username = username;
            return;
        }

        public bool didSucceed()
        {
            return success;
        }
    }


    public class GetUsernameByUserId
    {
        public int user_id;
        public GetUsernameByUserId(int user_id)
        {
            this.user_id = user_id;
            
        }

        public Username getUsername()
        {
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
                              "WHERE [user_id] = " + user_id
                     );

                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                string _username = "";
                                while (reader.Read())
                                {
                                    _username = reader.GetString(0);
                                }

                                if (_username == string.Empty)
                                {
                                    return new Username(false, "Internal error", "");
                                }

                                return new Username(true, _username);

                            }
                            else
                            {
                                return new Username(false, "", "");
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine("Select error: " + e.ToString());
                return new Username(false, e.ToString(), "");
            }
        }
    }
}
