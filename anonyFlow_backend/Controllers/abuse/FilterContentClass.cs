
using System;

/*
 *  class to filter out a content. such as a post or comment.
 * 
 *  if a post or comment has been flagged/reported x amount of times, then the content
 *  has to be filtered when returning back to client app.
 * 
 */
namespace anonyFlow_backend.Controllers.abuse
{
    public class FilterContentClass
    {
        public string table;
        public int table_id;

        private int maximum_reports = 5;

        /*
         * When calling this class you only want a boolean back for if the content should be hidden before tapping to show it.
         */
        public FilterContentClass(string table, int table_id)
        {
            this.table = table;
            this.table_id = table_id;
        }

        public bool shouldBeFiltered() {

            // use CollectReportsClass to decide if it should be filtered.
            CollectReportsClass collection = new CollectReportsClass(this.table, this.table_id);

            // get the report List back
            CollectedReportsObject returnable = collection.returnReports();

            // 
            return returnable.reports_received >= this.maximum_reports;
        }
    }
}
