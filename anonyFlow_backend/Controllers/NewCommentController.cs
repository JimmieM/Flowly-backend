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
    public class NewCommentController : Controller
    {
         // POST api/newcomment
        [HttpPost]
        public Response Post([FromBody]dynamic post)
        {
            int comment_post_id = post.comment_post_id;
            string comment_content = post.comment_content;
            int comment_user_id = post.comment_user_id; // your id

            SqlConnectionStringBuilder builder = WebApiConfig.Connection();

            try
            {
                string sql = "INSERT INTO comments " +
                    "(comment_post_id," +
                    "comment_content," +
                    "comment_date," +
                    "comment_user_id," +
                    "comment_dislikes," +
                    "comment_likes)" +
                    " VALUES " +
                    "(@comment_post_id," +
                    "@comment_content," +
                    "@comment_date," +
                    "@comment_user_id," +
                    "0," +
                    "0)".ToString();
                
                using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
                {
                    
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@comment_post_id", comment_post_id);
                        cmd.Parameters.AddWithValue("@comment_content", comment_content.ToString());
                        cmd.Parameters.AddWithValue("@comment_user_id", comment_user_id);
                        cmd.Parameters.AddWithValue("@comment_date", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));

                        cmd.ExecuteNonQuery();

                        PushNotificationObject push_object = new PushNotificationObject("Someone commented on your post");
                        NewNotificationClass notification = new NewNotificationClass(comment_post_id, push_object, "");

                        bool origin_user_id = notification.GetUserId(); // if the original poster's user id can be retrieved.

                        Console.WriteLine(origin_user_id);

                        // if the getUserId was successfull
                        // and the commenters user_id is not the same as yours. (?)
                        if(origin_user_id && notification.user_id != comment_user_id) {

                            try {
                                Response newNotification = notification.createNotification();

                                Response push = notification.createPush();
                            } catch (Exception e) {
                                Console.WriteLine(e);
                            }

                            return new Response(true, "Did Send push!");
                        }

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
