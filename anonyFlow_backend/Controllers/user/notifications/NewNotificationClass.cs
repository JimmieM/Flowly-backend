using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Data.SqlClient;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace anonyFlow_backend.Controllers
{

    // container to hold information about the push notification for the user.
    public class UserPushDetails
    {
        public bool success { get; set; }
        public string message { get; set; }

        public string username { get; set; }
        public string platform { get; set; }
        public string device_token { get; set; }

        public UserPushDetails(bool success, string username, string platform, string device_token)
        {
            this.success = success;
            this.username = username;
            this.platform = platform;
            this.device_token = device_token;
            return;
        }

        public UserPushDetails(bool success, string message)
        {
            this.success = success;
            this.message = message;
        }
    }

    // container to hold information abut the push contents, message etc.
    public class PushNotificationObject
    {
        public string message { get; set; }

        public string open_page { get; set; } // could be 'chat', 'post'
        public int dynamic_id { get; set; } // chat id, post id, etc.


        // open a page and load specific contents, such as a chat or a post..
        public PushNotificationObject(string message, string open_page, int dynamic_id)
        {
            this.message = message;
            this.open_page = open_page;
            this.dynamic_id = dynamic_id;
        }

        // only open a page..
        public PushNotificationObject(string message, string open_page)
        {
            this.message = message;
            this.open_page = open_page;
        }

        // only send a message..
        public PushNotificationObject(string message)
        {
            this.message = message;
        }
    }

    public class NewNotificationClass
    {
        private SqlConnectionStringBuilder builder = WebApiConfig.Connection();
        private static readonly HttpClient client = new HttpClient();

        public int user_id;
        private int post_id;
        private int comment_id;

        public PushNotificationObject push_object { get; set; }

        private NewNotificationClass() {}

        public NewNotificationClass(int user_id, PushNotificationObject push_object)
        {
            this.user_id = user_id;

            this.push_object = push_object;
        }

        public NewNotificationClass(int user_id, int post_id, PushNotificationObject push_object)
        {
            this.user_id = user_id;
            this.post_id = post_id;
            this.push_object = push_object;
        }

        public NewNotificationClass(int post_id, PushNotificationObject push_object, string x)
        {
            this.post_id = post_id;
            this.push_object = push_object;
        }

        // if getting user_id is nessecary.
        // will assign the retreieved userId as this.user_id.
        public Boolean GetUserId()
        {
            GetUserByPostIdClass userId = new GetUserByPostIdClass(this.post_id);

            User returnable = userId.getUser();

            Console.WriteLine(returnable.message);
            if (returnable.didSucceed())
                {
                    this.user_id = returnable.user_id;
                    return true;
                }

                return false;
        }

        // get and return device token, username, platform and store in object.
        public UserPushDetails getDeviceToken(int user_id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();

                    sb.Append("SELECT " +
                     "[user_name], " +
                     "[user_platform], " +
                     "[user_device_token] " +
                     "FROM [dbo].[users] " +
                      "WHERE [user_id] = " + user_id +
                      " AND [user_platform] IS NOT NULL" +
                      " AND [user_device_token] IS NOT NULL"
                     );

                    String sql = sb.ToString();

                    Console.WriteLine(sql);

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                string username = "";
                                string platform = "";
                                string device_token = "";
                                while (reader.Read())
                                {
                                    username = reader.GetString(0);
                                    platform = reader.GetString(1);
                                    device_token = reader.GetString(2);
                                }

                                if (device_token != String.Empty)
                                {
                                    return new UserPushDetails(true, username, platform, device_token);
                                }

                                return new UserPushDetails(false, "Failed to get device token");

                            }
                            else
                            {
                                return new UserPushDetails(false, "Not registered");
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
                return new UserPushDetails(false, e.ToString());
            }
        }

        private async void pushNotification(string device_token, PushNotificationObject obj)
        {
            Console.WriteLine(obj);
            var values = new Dictionary<string, string>
            {
                { "device_token", device_token },
                { "message", obj.message },
                { "open_page", obj.open_page },
                { "dynamic_id", obj.dynamic_id.ToString() }
            };

            Console.WriteLine(values.ToString());

            var messageForm = new FormUrlEncodedContent(values);

            var response = await client.PostAsync("https://bithatcher.com/flowly/api/push_notification.php", messageForm);

            var responseString = await response.Content.ReadAsStringAsync();
        }

        public Response createPush() {
             
            Console.WriteLine(this.push_object);

            // device token is always needed.
            UserPushDetails details = this.getDeviceToken(this.user_id);

            // if token could be retreived.
            if(details.success) {
                this.pushNotification(details.device_token, this.push_object);

                return new Response(true, "");
            }

            return new Response(false, true, details.message);
        }

        public Response createNotification()
        {
            try
            {
                string sql = "INSERT INTO notifications (notification_user_id, notification_post_id, notification_content, notification_date) VALUES (@user_id, @post_id, @content, @date)".ToString();

                using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
                {

                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@user_id", this.user_id);
                        cmd.Parameters.AddWithValue("@post_id", this.post_id);
                        cmd.Parameters.AddWithValue("@content", this.push_object.message);
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
