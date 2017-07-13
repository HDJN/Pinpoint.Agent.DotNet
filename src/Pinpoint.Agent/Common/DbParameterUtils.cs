namespace Pinpoint.Agent.Common
{
    using System.Data.Common;
    using System.Text;
    using System.Text.RegularExpressions;

    public class DbParameterUtils
    {
        public static string CollectionToString(DbParameterCollection collection)
        {
            var strBuilder = new StringBuilder();
            foreach (DbParameter param in collection)
            {
                strBuilder.AppendFormat("{0}, ",
                    param.Value != null ? param.Value.ToString() : "NULL");
            }

            if (collection.Count > 0)
            {
                strBuilder.Remove(strBuilder.Length - 2, 2);
            }

            return strBuilder.ToString();
        }

        public static string PretreatmentSql(string sql)
        {
            var regex = new Regex("@\\w*");
            return regex.Replace(sql, "?");
        }
    }
}
