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
    public class PostInteractionController : Controller
    {
        // POST api/values
        [HttpPost]
        public Response Post([FromBody]dynamic obj)
        {

            // if its an interaction for a post, not a comment.
            int origin_post_id = obj.origin_post_id; // the post id
            string table = obj.table; // either post or comment
            int id = obj.id; // either post or comment id

            int user_id = obj.user_id; // users ID
            int dislikes = obj.dislikes; // 1 or 0
            int likes = obj.likes; // 1 or 0

            SqlConnectionStringBuilder builder = WebApiConfig.Connection();

            try
            {
                string sql = "UPDATE " +
                   table + "s " +
                    "SET " +
                    table + "_likes = " + table + "_likes + @likes," +
                    table + "_dislikes = " + table + "_dislikes + @dislikes " +
                    "WHERE " +
                    table + "_id = @id";

                using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@likes", likes);
                        cmd.Parameters.AddWithValue("@dislikes", dislikes);
                        cmd.Parameters.AddWithValue("@id", id);

                        cmd.ExecuteNonQuery();


                        PushNotificationObject push_object = new PushNotificationObject("Someone interacted on your " + table);


                        NewNotificationClass notification = new NewNotificationClass(origin_post_id, push_object);

                        bool origin_user_id = notification.GetUserId(); // if the original poster's user id can be retrieved.

                        // if the getUserId was successfull
                        // and the commenters user_id is not the same as yours. (?)
                        if (origin_user_id && notification.user_id != user_id)
                        {
                            Response newNotification = notification.createNotification();
                            Console.WriteLine(newNotification.didSucceed());
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
