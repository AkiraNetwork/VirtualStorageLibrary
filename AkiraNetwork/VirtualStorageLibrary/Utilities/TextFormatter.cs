using System.Reflection;

namespace AkiraNetwork.VirtualStorageLibrary.Utilities
{
    public static class TextFormatter
    {
        public static string FormatTable(IEnumerable<IEnumerable<string>> tableData)
        {
            if (!tableData.Any())
            {
                return string.Empty;
            }

            var tableDataList = tableData.Select(row => row.ToList()).ToList(); // Convert to List<List<string>>
            int[] columnWidths = GetColumnWidths(tableDataList);

            string horizontalLine = new('-', columnWidths.Sum() + columnWidths.Length * 3 + 1);
            StringBuilder formattedTable = new();
            formattedTable.AppendLine(horizontalLine);

            foreach (var row in tableDataList)
            {
                formattedTable.Append('|');
                for (int i = 0; i < row.Count; i++)
                {
                    formattedTable.Append(" " + row[i].PadRight(columnWidths[i] - GetStringWidth(row[i]) + row[i].Length) + " |");
                }
                formattedTable.AppendLine();
                formattedTable.AppendLine(horizontalLine);
            }

            return formattedTable.ToString();
        }

        private static int[] GetColumnWidths(IEnumerable<IEnumerable<string>> tableData)
        {
            var tableDataList = tableData.Select(row => row.ToList()).ToList();
            int columnCount = tableDataList[0].Count;
            int[] columnWidths = new int[columnCount];

            foreach (var row in tableDataList)
            {
                for (int i = 0; i < row.Count; i++)
                {
                    int cellWidth = GetStringWidth(row[i]);
                    if (cellWidth > columnWidths[i])
                    {
                        columnWidths[i] = cellWidth;
                    }
                }
            }

            return columnWidths;
        }

        private static int GetStringWidth(string str)
        {
            int width = 0;
            foreach (char c in str)
            {
                if (IsFullWidth(c))
                {
                    width += 2;
                }
                else
                {
                    width += 1;
                }
            }
            return width;
        }

        // Custom method to check if a character is full-width
        private static bool IsFullWidth(char c)
        {
            // Full-width character ranges
            return c >= 0x1100 && c <= 0x115F ||
                   c >= 0x2E80 && c <= 0xA4CF && c != 0x303F ||
                   c >= 0xAC00 && c <= 0xD7A3 ||
                   c >= 0xF900 && c <= 0xFAFF ||
                   c >= 0xFE10 && c <= 0xFE19 ||
                   c >= 0xFE30 && c <= 0xFE6F ||
                   c >= 0xFF00 && c <= 0xFF60 ||
                   c >= 0xFFE0 && c <= 0xFFE6;
        }

        public static string GenerateTextBasedTable<T>(IEnumerable<T> enumerableObject)
        {
            if (enumerableObject == null || !enumerableObject.Any())
            {
                return string.Empty;
            }

            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            List<IEnumerable<string>> tableData = [];
            tableData.Add(properties.Select(p => p.Name));

            foreach (var obj in enumerableObject)
            {
                List<string> row = [];
                foreach (var property in properties)
                {
                    object? value = property.GetValue(obj);
                    row.Add(value?.ToString() ?? "(null)");
                }
                tableData.Add(row);
            }

            return FormatTable(tableData);
        }

        public static string GenerateTextBasedTableBySingle<T>(T singleObject)
        {
            Type type = typeof(T);
            List<PropertyInfo> properties = [];

            // Get properties from base classes first
            Type? currentType = type;
            while (currentType != null)
            {
                properties.InsertRange(0, currentType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
                currentType = currentType.BaseType;
            }

            // Ensure properties are unique by name
            var uniqueProperties = properties.GroupBy(p => p.Name).Select(g => g.First()).ToList();

            List<string> headers = [];
            List<string> row = [];

            foreach (var property in uniqueProperties)
            {
                headers.Add(property.Name);

                try
                {
                    // Get the property value
                    object? value = property.GetValue(singleObject);

                    // Skip collections and dictionaries
                    if (value != null && IsToStringOverridden(value.GetType()))
                    {
                        row.Add(value?.ToString() ?? "(null)");
                    }
                    // Skip collections and dictionaries
                    else if (value is IEnumerable && value is not string)
                    {
                        row.Add("(no output)");
                    }
                    else
                    {
                        row.Add(value?.ToString() ?? "(null)");
                    }
                }
                catch
                {
                    // If any exception occurs, replace with "(no output)"
                    row.Add("(no output)");
                }
            }

            List<IEnumerable<string>> tableData =
            [
                headers,
                row
            ];

            return FormatTable(tableData);
        }

        private static bool IsToStringOverridden(Type type)
        {
            MethodInfo? toStringMethod = type.GetMethod("ToString", Type.EmptyTypes);
            return toStringMethod?.DeclaringType != typeof(object);
        }
    }
}
