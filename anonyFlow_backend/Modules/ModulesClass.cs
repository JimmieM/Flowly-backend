using System;
using System.Text;
using System.Data.SqlClient;

namespace anonyFlow_backend.Controllers
{
    public class Modules
    {
        public string betweenDates(string dateInput)
        {
            try {
                String now = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                DateTime n = DateTime.Parse(now);
                DateTime p = DateTime.Parse(dateInput);

                TimeSpan span = n.Subtract(p);

                String date = span.Hours.ToString() + " hrs ago";

                if(span.Days > 0) {
                    date = span.Days.ToString() + " days ago";
                } else {
                    if (span.Hours < 1)
                    {
                        if (span.Minutes <= 1)
                        {
                            date = "right now";
                        }
                        else
                        {
                            date = span.Minutes.ToString() + " minutes ago";
                        }
                    }
                }

                return date;

            } catch(DataMisalignedException e) {
                Console.WriteLine(e);
                return dateInput;
            }
        }

        public static byte[] ToByteArray(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }

        public string sha256(string password)
        {
            System.Security.Cryptography.SHA256Managed crypt = new System.Security.Cryptography.SHA256Managed();
            System.Text.StringBuilder hash = new System.Text.StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password), 0, Encoding.UTF8.GetByteCount(password));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }

        public bool empty(string value)
        {
            return string.IsNullOrEmpty(value);
        }

        private String GetStringValue(SqlDataReader reader, String columnName)
        {
            return (reader[columnName] != null && reader[columnName] != DBNull.Value) ? Convert.ToString(reader[columnName]) : null;
        }
    }
}
