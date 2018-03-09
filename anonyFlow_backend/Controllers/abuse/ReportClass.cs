using System;
namespace anonyFlow_backend.Controllers.abuse
{
    // class to hold a specific report of a post or comment.
    public class ReportClass
    {
        public int user_id { get; set; } // the user that got reported.
        public string table { get; set; } // post or comment 'string'
        public int table_id { get; set; } // post or comment ID
        public int reports_received { get; set; } // how many times this post / comment has been reported.

        // with a user id
        // when searching for a post/comment and retreive how many times its been reported
        public ReportClass(int user_id, string table, int table_id, int reports_received)
        {
            this.user_id = user_id;
            this.table = table;
            this.table_id = table_id;
            this.reports_received = reports_received;
            return;
        }

        // without a user id
        // when searching for a post/comment and retreive how many times its been reported
        public ReportClass(string table, int table_id, int reports_received)
        {
            this.table = table;
            this.table_id = table_id;
            this.reports_received = reports_received;
            return;
        }

        // when getting all reports connected to a user
        public ReportClass(int user_id, string table, int table_id)
        {
            this.user_id = user_id;
            this.table = table;
            this.table_id = table_id;
            return;
        }

    }
}
