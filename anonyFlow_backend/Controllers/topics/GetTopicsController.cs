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

    public class Topic {
        public int topic_id { get; set; }
        public int topic_sub_topic_of_id { get; set; }
        public string topic_name { get; set; }
        public int topic_likes { get; set; }
        public int topic_dislikes { get; set; }
        public int topic_deleted { get; set; }

        public Topic(int topic_id,
                     int topic_likes,
                     int topic_dislikes,
                     int topic_deleted,
                     string topic_name,
                     int topic_sub_topic_of_id) {
            
            this.topic_id = topic_id;
            this.topic_sub_topic_of_id = topic_sub_topic_of_id;
            this.topic_name = topic_name;
            this.topic_likes = topic_likes;
            this.topic_dislikes = topic_dislikes;
            this.topic_deleted = topic_deleted;
            return;
        }
    }

    [Route("api/topics/[controller]")]
    public class GetTopicsController : Controller
    {
        [HttpPost]
        public Response Post([FromBody]dynamic obj)
        {
            Response topics = this.returnTopics();

            return topics;
        }

        public Response returnTopics() {
            List<Topic> topics = new List<Topic>();

            try
            {
                // include connection..
                SqlConnectionStringBuilder builder = WebApiConfig.Connection();

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();

                    sb.Append("SELECT * FROM topics WHERE topic_deleted = 0");

                    int id = 0;
                    if(id > 0) {
                        sb.Append(" AND topic_id = " + id);
                    }

                    sb.Append(" ORDER BY topic_likes DESC");
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if(reader.HasRows) {
                                while(reader.Read()) {
                                    topics.Add(new Topic(
                                        reader.GetInt32(0),
                                        reader.GetInt32(1),
                                        reader.GetInt32(2),
                                        reader.GetInt32(3),
                                        reader.GetString(4),
                                        reader.GetInt32(5)
                                     ));
                                }

                                return new Response(true, "", topics);
                            } else {
                                return new Response(false, "Found 0 results");
                            }
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
