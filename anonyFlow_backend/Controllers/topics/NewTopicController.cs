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


    [Route("api/topics/[controller]")]
    public class NewTopicController : Controller
    {
        public Response hasDuplicate(string topic_name) {
            SqlConnectionStringBuilder builder = WebApiConfig.Connection();
            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();

                    sb.Append("SELECT [topic_name] FROM topics");

                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows) {
                                string index;
                                while(reader.Read()) {
                                    index = reader.GetString(0);

                                    // if found.
                                    // e.g, BMX == bmx. Bmx == bmx. BMX == Bmx etc.
                                    if(index.ToLower() == topic_name.ToLower()) {
                                        return new Response(true, "");
                                    }
                                }
                                return new Response(false, "");
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

        [HttpPost]
        public Response Post([FromBody]dynamic obj)
        {
            int user_id = obj.user_id;
            string topic_name = obj.topic_name;

            Response findDuplicate = this.hasDuplicate(topic_name);
            // if a duplicate is found.

            if(findDuplicate.success) {
                return new Response(false, "Topic already exist");
            } else if(!findDuplicate.success && findDuplicate.error) {
                return new Response(false, findDuplicate.message);
            }

            try
            {
                SqlConnectionStringBuilder builder = WebApiConfig.Connection();
                string sql = "INSERT INTO topics (topic_name, topic_added_by_user_id) VALUES (@topic_name, @user_id)";

                using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@topic_name", topic_name);
                        cmd.Parameters.AddWithValue("@user_id", user_id);

                        cmd.ExecuteNonQuery();

                        return new Response(true, "");
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

    }
}
