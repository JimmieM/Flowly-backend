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
    public class Notification {

        public int notification_id { get; set; }
        public int notification_user_id { get; set; }
        public int notification_post_id { get; set; }
        public string notification_content { get; set; }
        public string notification_date { get; set; }
        public int notification_seen { get; set; }

        public Notification
        (int notification_id,
        int notification_user_id,
        int notification_post_id,
        string notification_content,
        string notification_date,
        int notification_seen) {

            this.notification_id = notification_id;
            this.notification_user_id = notification_user_id;
            this.notification_post_id = notification_post_id;
            this.notification_content = notification_content;
            this.notification_date = notification_date;
            this.notification_seen = notification_seen;
            return;

        }    
    }


    [Route("api/user/notifications/[controller]")]
    public class GetNotificationsController : Controller
    {
        SqlConnectionStringBuilder builder = WebApiConfig.Connection();

        public Response updateNotificationSeen(int notification_id) 
        {
            try
            {
                string sql = "UPDATE notifications SET notification_seen = 1 WHERE notification_id = @id";

                using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", notification_id);

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

        [HttpPost]
        public Response Post([FromBody]dynamic obj)
        {
            Modules modules = new Modules();
            int user_id = obj.user_id;
            List<Notification> notifications = new List<Notification>();

            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();

                    sb.Append("SELECT TOP(50)" +
                              "[notification_id]," +
                              "[notification_user_id]," +
                              "[notification_post_id], " +
                              "[notification_content], " +
                              "[notification_date], " +
                              "[notification_seen] " +
                              "FROM " +
                              "[dbo].[notifications]" +
                              "WHERE [notification_user_id] = " + user_id +
                              " ORDER BY notification_date DESC");

                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if(reader.HasRows) {
                                while(reader.Read()) {
                                    
                                    int id = reader.GetInt32(0);
                                    int seen = reader.GetInt32(5);

                                    if(seen == 0) {
                                        // update to 1, but don't update local variable to send to client.
                                        updateNotificationSeen(id);
                                    }

                                    string date = modules.betweenDates(reader.GetString(4).ToString());

                                    notifications.Add(new Notification(
                                        id,
                                        reader.GetInt32(1),
                                        reader.GetInt32(2),
                                        reader.GetString(3).ToString(),
                                        date,
                                        seen));
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
                return new Response(false, e.ToString());
            }

            return new Response(true, "", notifications);
        }

    }
}
