using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace anonyFlow_backend.Controllers
{
    public class TopicDislikes {
        public int dislikes { get; set; }
        public bool success { get; set; }

        public TopicDislikes(bool success, int dislikes) {
            this.success = success;
            this.dislikes = dislikes;
            return;
        }
    }

    [Route("api/topics/[controller]")]
    public class VoteTopicController : Controller
    {
        /*
         * ban user if x amount of topics has been deleted due downvotes...
         */
        public Response banOriginTopicCreator(int topic_id) {

            GetUserIdByTopicIdClass findUser = new GetUserIdByTopicIdClass(topic_id);
            Response resp = findUser.returnUserId();

            if(!resp.success || resp.integer == 0) {
                return new Response(false, true, resp.message);
            }

            int user_id = resp.integer;
                                    
            try
            {
                // include connection..
                SqlConnectionStringBuilder builder = WebApiConfig.Connection();

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();

                    sb.Append("SELECT [topic_deleted] FROM [topics] WHERE [topic_added_by_user_id] = " + user_id);

                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if(reader.HasRows) {
                                int deleted;
                                int count = 0;
                                int maximum = 5; // maximum of deletions before ban.
                                while(reader.Read()) {
                                    deleted = reader.GetInt32(0);

                                    if(deleted == 1) {
                                        count++;
                                    }
                                }

                                if(count >= maximum) {
                                    BanUserClass ban = new BanUserClass(user_id);
                                    Response respBan = ban.BanUser();
                                    if(respBan.success) {
                                        return new Response(true, "");
                                    } else {
                                        return new Response(false,true, respBan.message);
                                    }
                                } else {
                                    return new Response(false, "Didnt ban");
                                }
                                    
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

        public Response removeTopic(int topic_id) {

            // ban
            Response banResp = this.banOriginTopicCreator(topic_id);
            
            SqlConnectionStringBuilder builder = WebApiConfig.Connection();

            try
            {
                string sql = "UPDATE topics SET topic_deleted = 1 WHERE topic_id = @topic_id";

                using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@topic_id", topic_id);

                        cmd.ExecuteNonQuery();

                        return new Response(true, "");
                    }
                }
            }
            catch (SqlException ex)
            {
                string msg = "Update error: ";
                msg += ex.Message;
                return new Response(false, msg);
            }
        }

        public TopicDislikes getTopicDislikes(int topic_id) {
            try
            {
                // include connection..
                SqlConnectionStringBuilder builder = WebApiConfig.Connection();

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();

                    sb.Append("SELECT topic_dislikes FROM topics WHERE topic_deleted = 0");
                    sb.Append(" AND topic_id = " + topic_id);

                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                int dislikes = 0;
                                while (reader.Read())
                                {
                                    dislikes = reader.GetInt32(0);
                                }

                                return new TopicDislikes(true, dislikes);

                            }
                            else
                            {
                                return new TopicDislikes(true, 0);
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
                return new TopicDislikes(false, 0);
            }
        }

        [HttpPost]
        public Response Post([FromBody]dynamic obj)
        {
            int dislikes = obj.dislikes;
            int likes = obj.likes;
            int topic_id = obj.topic_id;

            if(dislikes == 1) {
                TopicDislikes resp = this.getTopicDislikes(topic_id);

                // remove topic at 19 dislikes
                if(resp.success && resp.dislikes >= 19) {
                    Response didRemove = this.removeTopic(topic_id);

                    // return response, since there's no purpose to update topics as below.
                    if(didRemove.success) {
                        return new Response(true, "");
                    }
                }
            }

            SqlConnectionStringBuilder builder = WebApiConfig.Connection();

            try
            {
                string sql = "UPDATE topics SET topic_likes = topic_likes + @likes, topic_dislikes = topic_dislikes + @dislikes WHERE topic_id = @topic_id";

                using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@likes", likes);
                        cmd.Parameters.AddWithValue("@dislikes", dislikes);
                        cmd.Parameters.AddWithValue("@topic_id", topic_id);

                        cmd.ExecuteNonQuery();

                        return new Response(true, "");
                    }
                }
            }
            catch (SqlException ex)
            {
                string msg = "Update error: ";
                msg += ex.Message;
                return new Response(false, msg);
            }
        }
    }
}
