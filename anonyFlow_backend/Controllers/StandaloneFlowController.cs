using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace anonyFlow_backend.Controllers
{
    [Route("api/[controller]")]
    public class StandaloneFlowController : Controller
    {
        [HttpPost]
        public Response Post([FromBody]dynamic obj)
        {
            int user_id = obj.user_id;
            int post_id = obj.post_id;

            Modules modules = new Modules();
            FlowController flow = new FlowController();

            var post = new List<flow>();
            try
            {
                // include connection..
                SqlConnectionStringBuilder builder = WebApiConfig.Connection();

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();

                    sb.Append("SELECT" +
                     " [posts].[post_id]," +
                     " [posts].[post_user_id]," +
                      " [posts].[post_content]," +
                      " [posts].[post_date]," +
                      " [posts].[post_dislikes]," +
                      " [posts].[post_likes]," +
                      " [posts].[post_topic_id]," + 
                      " [posts].[post_locality]," + 
                      " [posts].[post_country]" + 
                      " FROM [dbo].[posts]" +
                      " WHERE post_id = " + post_id
                     );
                    
                    String sql = sb.ToString();

                    Console.WriteLine(sql);

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                return new Response(false, "This post might have been deleted.");
                            }
                            else
                            {
                                while (reader.Read())
                                {
                                    var date = modules.betweenDates(reader.GetString(3).ToString());
                                    List<comment> Comments = flow.GetComments(reader.GetInt32(0));
                                    post.Add(new flow(
                                     reader.GetInt32(0),
                                     reader.GetInt32(1),
                                     reader.GetString(2).ToString(),
                                     date,
                                     reader.GetInt32(4),
                                     reader.GetInt32(5),
                                     reader.GetInt32(6),
                                     reader.GetValue(7).ToString(),
                                     reader.GetValue(8).ToString(),
                                     Comments,
                                        false
                                 ));
                                }
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
                return new Response(false,true, e.ToString());
            }

            return new Response(true, "", post);
        }

    }
}
