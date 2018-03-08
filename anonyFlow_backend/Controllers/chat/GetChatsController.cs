using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text;

/*
 * get chats by Contacts table
 * 
 * 
 */
namespace anonyFlow_backend.Controllers
{
    public class LatestMessage : Response {

        public string latest_message { get; set; }
        public string latest_message_date { get; set; }

        // contructor for returning an error.
        public LatestMessage(Response response)
            : base(response.success, response.error, response.message)
        {
            this.latest_message = latest_message;
            this.latest_message_date = latest_message_date;
            return;
        }

        // contructor for returning the values. Re-use when sending to client?
        public LatestMessage(Response response, string latest_message, string latest_message_date) 
            : base (response.success, response.error, response.message) 
        {
            this.latest_message = latest_message;
            this.latest_message_date = latest_message_date;
            return;
        }
    }

    public class Contact {
        
        public string username { get; set; }
        public int user_id { get; set; }
        public int accepted { get; set; }
        public LatestMessage latest_message_object { get; set; }
        public string latest_message_text { get; set; }

        public int met_by_post { get; set; }

        // used when returning a user with data about the latestmessage. Otherwise its just a text.
        // used to sort chats in profile interface by latest message recieved/sent.
        public Contact(string username, int user_id, int accepted, int met_by_post, LatestMessage latest_message_object) {
            this.username = username;
            this.user_id = user_id;
            this.accepted = accepted;
            this.met_by_post = met_by_post;
            this.latest_message_object = latest_message_object;

            return;
        }

        // only text. Such as "User would like to add you"
        public Contact(string username, int user_id, int accepted, int met_by_post, string latest_message_text)
        {
            this.username = username;
            this.user_id = user_id;
            this.accepted = accepted;
            this.met_by_post = met_by_post;
            this.latest_message_text = latest_message_text;

            return;
        }


        // used for outgoing requets, where you're the requestee.
        public Contact(int met_by_post) {
            this.met_by_post = met_by_post;
        }
    }

    [Route("api/chat/[controller]")]
    public class GetChatsController : Controller
    {
        [HttpPost]
        public Response Post([FromBody]dynamic obj)
        {
            Modules modules = new Modules();

            List<Contact> contacts = new List<Contact>(); // accepted contacts
            List<Contact> awaiting = new List<Contact>(); // pending incoming
            List<Contact> requests = new List<Contact>(); // pending outgoing

            int user_id = obj.user_id; // your id

            Console.WriteLine("id: " + user_id);

            try
            {
                // include connection..
                SqlConnectionStringBuilder builder = WebApiConfig.Connection();

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();

                    sb.Append("SELECT [contact_user_id], [contact_with_user_id], [contact_accepted], [contact_met_by_post_id] FROM [dbo].[contacts] WHERE contact_user_id = " + user_id + " OR contact_with_user_id = " + user_id);

                    String sql = sb.ToString();

                    Console.WriteLine(sql);

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows) {
                                return new Response(false, "Query found 0 results");
                            } else {
                                while (reader.Read()) {
                                    int contact_user_id = reader.GetInt32(0);
                                    int contact_with_user_id = reader.GetInt32(1);
                                    int accepted = reader.GetInt32(2);

                                    Response usernameCallback;
                                    LatestMessage latestMessage;
                                    if(contact_user_id == user_id) {
                                        usernameCallback = getUsernameById(contact_with_user_id);    
                                        latestMessage = getLatestMessage(user_id, contact_with_user_id);
                                    } else if(contact_with_user_id == user_id) {
                                        usernameCallback = getUsernameById(contact_user_id);
                                        latestMessage = getLatestMessage(user_id, contact_user_id);
                                    } else {
                                        return new Response(false, "Internal Error");
                                    }

                                    if(usernameCallback.didSucceed()) {
                                        string username = usernameCallback.message;

                                        string message = "";
                                        if(latestMessage.didSucceed()) {
                                            message = latestMessage.message;
                                        } 

                                        int contacts_id;

                                        if(contact_with_user_id == user_id) {
                                            // if you're the requesteés contact
                                            contacts_id = contact_user_id;

                                        } else if(contact_user_id == user_id) {
                                            contacts_id = contact_with_user_id;
                                        } else {
                                            return new Response(false, "Internal error");
                                        }

                                        if(contact_user_id == user_id && accepted == 0) {
                                            // add only met_by_post_id
                                            requests.Add(new Contact(
                                                reader.GetInt32(3)
                                            ));
                                        } else {
                                            // if you have accepted
                                            if (accepted == 1) {
                                                contacts.Add(new Contact(
                                                  username,
                                                    contacts_id,
                                                  reader.GetInt32(2),
                                                  reader.GetInt32(3),
                                                    latestMessage));
                                            } else {
                                                awaiting.Add(new Contact(
                                                  username,
                                                    contacts_id,
                                                  reader.GetInt32(2),
                                                  reader.GetInt32(3),
                                                   "I'd like to add you as a contact!"));
                                            }
                                        }
                                    }
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

            return new Response(true, "", contacts, awaiting, requests);
        }

        public LatestMessage getLatestMessage(int your_id, int contacts_id) {
            Response resp;
            try
            {
                // include connection..
                SqlConnectionStringBuilder builder = WebApiConfig.Connection();

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();

                    sb.Append("SELECT TOP (1) [chat_message], [chat_date] FROM [dbo].[chats] " +
                              "WHERE ([chat_to_user_id] = " + your_id + " AND [chat_from_user_id] = " + contacts_id + ")" +
                              "OR " +
                              "([chat_from_user_id] = " + your_id + " AND [chat_to_user_id] = " + contacts_id + ")");
                    sb.Append(" ORDER BY [chat_date] DESC");
                       
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                resp = new Response(true, false, "");
                                return new LatestMessage(resp);
                            }
                            else
                            {
                                while (reader.Read())
                                {
                                    resp = new Response(true, false, "");
                                    return new LatestMessage(resp, reader.GetString(0).ToString(), reader.GetString(1).ToString());
                                }

                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
                resp = new Response(true, false, e.ToString());
                return new LatestMessage(resp);
            }

            // Redundant code....
            resp = new Response(true, false, "");
            return new LatestMessage(resp);
        }

        public Response getUsernameById(int id) {
            try
            {
                // include connection..
                SqlConnectionStringBuilder builder = WebApiConfig.Connection();

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();

                    sb.Append("SELECT [user_name] FROM [dbo].[users] WHERE [user_id] = " + id);

                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                return new Response(false, "Query found 0 results");
                            }
                            else
                            {
                                while (reader.Read())
                                {
                                    return new Response(true, reader.GetString(0).ToString());
                                }

                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
                return new Response(false, "Query found 0 results");
            }
            return new Response(false, "Query found 0 results");
        }
     }
}
