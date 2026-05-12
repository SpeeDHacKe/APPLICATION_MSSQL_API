using System.Globalization;

namespace APPLICATION_MSSQL_API.Common.Extensions
{
    public static class DateTimeExtension
    {
        private static bool isBuddhistFormat = $"th".Equals(CultureInfo.CurrentCulture.ToString().Substring(0, 2));

        /// <summary>
        /// Check if date is a valid format
        /// </summary>
        /// <param name="date"></param>
        /// <returns>Return True If valid format</returns>
        public static Boolean IsDate(string date)
        {
            DateTime dateTime;
            return DateTime.TryParse(date, out dateTime);
        }

        public static string ToBuddhistDateString(this DateTime? dateTime, string format = "dd/MM/yyyy HH:mm:ss")
        {
            if (dateTime == null)
                return null;

            if (!dateTime.HasValue)
                return string.Empty;

            if (isBuddhistFormat)
                return dateTime.Value.ToString(format);


            Thread.CurrentThread.CurrentCulture = new CultureInfo("th-TH");
            string result = dateTime.Value.ToString(format);
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            return result;
        }

        public static string ToUniversalDateString(this DateTime? dateTime, string format = "dd/MM/yyyy HH:mm:ss")
        {
            if (dateTime == null)
                return null;

            if (!dateTime.HasValue)
                return string.Empty;

            if (!isBuddhistFormat)
                return dateTime.Value.ToString(format);


            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            string result = dateTime.Value.ToString(format);
            Thread.CurrentThread.CurrentCulture = new CultureInfo("th-TH");
            return result;
        }
        static public string ToThDateStrToEnDateStr(this string buddhistDate)
        {
            try
            {
                var dd1 = buddhistDate.Substring(0, 2);
                var mm1 = buddhistDate.Substring(3, 2);
                var y1 = Convert.ToInt16(buddhistDate.Substring(6, 4));
                var year1 = (y1 - 543).ToString();
                var dateStr = year1 + "-" + mm1 + "-" + dd1;
                DateTime conDate = Convert.ToDateTime(dateStr);
                var resp = conDate.ToString("yyyy-MM-dd", new CultureInfo("en-US"));
                return resp;
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot convert string to date {ex} {buddhistDate}");
            }
        }
    }
}