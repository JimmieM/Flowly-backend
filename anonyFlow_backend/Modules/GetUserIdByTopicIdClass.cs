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
    public class GetUserIdByTopicIdClass
    {
        public int topic_id;
        public GetUserIdByTopicIdClass(int topic_id)
        {
            this.topic_id = topic_id; 
        }

        public Response returnUserId() {
            try
            {
                // include connection..
                SqlConnectionStringBuilder builder = WebApiConfig.Connection();

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();

                    sb.Append("SELECT [topic_added_by_user_id] FROM [topics] WHERE [topic_id] = " + topic_id);

                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if(reader.HasRows) {
                                int user_id = 0;
                                while(reader.Read()) {
                                    user_id = reader.GetInt32(0);
                                }
                                return new Response(true, user_id);
                            } else {
                                return new Response(false, "");
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
                return new Response(false, true, e.ToString());
            }
        }
    }
}
