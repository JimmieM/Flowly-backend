using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text;
namespace anonyFlow_backend.Controllers
{

    public class Chat
    {
        public int chat_id { get; set; }
        public int chat_from_id { get; set; }
        public int chat_to_id { get; set; }
        public string chat_message { get; set; }
        public string chat_date { get; set; }

        public Chat(int chat_id, int chat_from_id, int chat_to_id, string chat_message, string chat_date) {
            this.chat_id = chat_id;
            this.chat_from_id = chat_from_id;
            this.chat_to_id = chat_to_id;
            this.chat_message = chat_message;
            this.chat_date = chat_date;
            return;
        }
    }

    [Route("api/chat/[controller]")]
    public class GetChatController : Controller
    {
        [HttpPost]
        public Response Post([FromBody]dynamic obj)
        {
            Modules modules = new Modules();

            var chats = new List<Chat>();

            int your_id = obj.user_id;
            int contact_user_id = obj.contact_user_id;

            try
            {
                // include connection..
                SqlConnectionStringBuilder builder = WebApiConfig.Connection();

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("SELECT TOP (50) [chat_id],[chat_from_user_id],[chat_to_user_id], [chat_message], [chat_date] FROM [dbo].[chats] " +
                              "WHERE ([chat_to_user_id] = " + your_id + " AND [chat_from_user_id] = " + contact_user_id + ")" +
                              " OR " +
                              "([chat_from_user_id] = " + your_id + " AND [chat_to_user_id] = " + contact_user_id + ") ORDER BY chat_date ASC");
                   
                    String sql = sb.ToString();

                    Console.WriteLine(sql);

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader == null)
                            {
                                return new Response(false, "Query found 0 results");
                            }
                            else
                            {
                                while (reader.Read())
                                {
                                    string date = modules.betweenDates(reader.GetString(4).ToString());

                                    chats.Add(new Chat(
                                        reader.GetInt32(0),
                                        reader.GetInt32(1),
                                        reader.GetInt32(2),
                                        reader.GetString(3),
                                        date));
                                }

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

            return new Response(true, "", chats);
        }

    }
}
