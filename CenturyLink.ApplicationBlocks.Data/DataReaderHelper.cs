using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace CenturyLink.ApplicationBlocks.Data
{
    public class DataReaderHelper
    {
        public static DateTime GetNonNullDateValue(IDataReader reader, string column)
        {
            string date = GetNonNullValue(reader, column, false);

            if (date.Length > 0)
                return DateTime.Parse(date);
            else
                return DateTime.MinValue;
        }

        public static string GetNonNullValue(IDataReader reader, string column)
        {
            return GetNonNullValue(reader, column, false);
        }

        public static string GetNonNullValue(IDataReader reader, string column, bool isNumericValue)
        {
            int ordinal = -1;

            // trying to prevent invalid index exception being thrown
            try
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (reader.GetName(i).Equals(column, StringComparison.OrdinalIgnoreCase))
                    {
                        ordinal = reader.GetOrdinal(column);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
            }

            if (ordinal >= 0 && !reader.IsDBNull(ordinal))
                return reader[column].ToString();
            else if (isNumericValue)
                return "0";
            else
                return "";
        }
    }
}
