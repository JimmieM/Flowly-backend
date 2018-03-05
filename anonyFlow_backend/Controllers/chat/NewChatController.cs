using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;

namespace anonyFlow_backend.Controllers.chat
{
    public class NewChat {
        string username { get; set; }
        string to_username { get; set; }
        string message { get; set; }

        public NewChat(string username, string to_username, string message) {
            this.username = username;
            this.to_username = to_username;
            this.message = message;
            return;
        }
    }
    
    [Route("api/chat/[controller]")]
    public class NewChatController : Controller
    {
        [HttpPost]
        public Response Post([FromBody]dynamic obj)
        {
            Modules modules = new Modules();

            int user_id = obj.user_id;
            string username = obj.username;

            int to_user_id = obj.to_user_id;
            string to_username = obj.to_username;

            string message = obj.message;


            PushNotificationObject push_object = new PushNotificationObject("New message from " + username);

            NewNotificationClass newNotification = new NewNotificationClass(to_user_id, push_object);

            Response push = newNotification.createPush();

            if(!push.success) {
                Console.WriteLine("Push failed! Details: " + push.message);
            }

            SqlConnectionStringBuilder builder = WebApiConfig.Connection();

            try
            {
                string sql = "INSERT INTO chats " +
                    "(chat_from_user_id," +
                    "chat_to_user_id," +
                    "chat_message," +
                    "chat_date)" +
                    " VALUES " +
                    "(@from_user_id," +
                    "@to_user_id," +
                    "@message," +
                    "@date)".ToString();

                using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@from_user_id", user_id);
                        cmd.Parameters.AddWithValue("@to_user_id", to_user_id);
                        cmd.Parameters.AddWithValue("@message", message);
                        cmd.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));

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
