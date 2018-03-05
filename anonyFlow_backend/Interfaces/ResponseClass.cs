using System;
using System.Collections.Generic;

namespace anonyFlow_backend.Controllers
{

    public class Response
    {
        public Boolean success { get; set; }
        public Boolean error { get; set; }
        public int integer { get; set; }
        public string message { get; set; }

        public List<flow> flow_response_dynamic { get; set; }

        public List<flow> flow_response_normal { get; set; }
        public List<flow> flow_response_popular { get; set; }
        public List<flow> flow_response_unpopular { get; set; }

        public List<Contact> contacts { get; set; }
        public List<Contact> contacts_awaiting { get; set; }
        public List<Contact> contacts_requests { get; set; }

        public Profile profile { get; set; }

        public List<Chat> chats { get; set; }

        public List<Notification> notification_response { get; set; }

        public List<Topic> topic_response { get; set; }

        public Boolean didSucceed() {
            return success;
        }

        public Response(Boolean success, int integer)
        {
            this.success = success;
            this.integer = integer;
            return;
        }

        public Response(Boolean success, string message)
        {
            this.success = success;
            this.message = message;
            return;
        }

        public Response(Boolean success, Boolean error, string message)
        {
            this.success = success;
            this.error = error;
            this.message = message;
            return;
        }

        public Response(Boolean success, string message, List<Notification> notifiction_response) {
            this.success = success;
            this.message = message;
            this.notification_response = notifiction_response;
            return;
        }

        public Response(Boolean success, string message, List<flow> flow_response_dynamic)
        {
            this.success = success;
            this.message = message;
            this.flow_response_dynamic = flow_response_dynamic;
            return;
        }

        public Response(Boolean success, 
                        string message, 
                        List<flow> flow_response_normal, 
                        List<flow> flow_response_popular, 
                        List<flow> flow_response_unpopular)
        {
            this.success = success;
            this.message = message;
            this.flow_response_normal = flow_response_normal;
            this.flow_response_popular = flow_response_popular;
            this.flow_response_unpopular = flow_response_unpopular;
            return;
        }

        public Response(Boolean success, string message, List<Contact> contacts, List<Contact> contacts_awaiting, List<Contact> contacts_requests) {
            this.success = success;
            this.message = message;
            this.contacts = contacts;
            this.contacts_awaiting = contacts_awaiting;
            this.contacts_requests = contacts_requests;
            return;
        }

        public Response(Boolean success, string message, List<Chat> chats)
        {
            this.success = success;
            this.message = message;
            this.chats = chats;
            return;
        }


        public Response(Boolean success, string message, List<Topic> topic_response)
        {
            this.success = success;
            this.message = message;
            this.topic_response = topic_response;
            return;
        }

        public Response(Boolean success, string message, Profile profile) {
            this.success = success;
            this.message = message;
            this.profile = profile;
            return;
        }
    }
}
