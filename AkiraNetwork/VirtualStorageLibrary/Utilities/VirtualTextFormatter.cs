// This file is part of VirtualStorageLibrary.
//
// Copyright (C) 2024 Akira Shimodate
//
// VirtualStorageLibrary is free software, and it is distributed under the terms of 
// the GNU General Public License (version 3, or at your option, any later version). 
// This license is published by the Free Software Foundation.
//
// VirtualStorageLibrary is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY, without even the implied warranty of MERCHANTABILITY or 
// FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along 
// with VirtualStorageLibrary. If not, see https://www.gnu.org/licenses/.

using System.Reflection;

namespace AkiraNetwork.VirtualStorageLibrary.Utilities
{
    /// <summary>
    /// Utility class for generating text-based representations of the virtual storage's tree
    /// structure. Provides static methods for generating various debug texts.
    /// </summary>
    /// <remarks>
    /// This class distinguishes between full-width and half-width characters based on specific
    /// character code ranges. The following ranges are considered full-width characters:
    /// - 0x1100 to 0x115F: Korean Hangul Jamo characters
    /// - 0x2E80 to 0xA4CF (excluding 0x303F): CJK Unified Ideographs and compatibility characters
    /// - 0xAC00 to 0xD7A3: Korean Hangul syllables
    /// - 0xF900 to 0xFAFF: CJK compatibility ideographs
    /// - 0xFE10 to 0xFE19: Vertical forms punctuation
    /// - 0xFE30 to 0xFE6F: CJK compatibility forms
    /// - 0xFF00 to 0xFF60: Full-width ASCII and symbols
    /// - 0xFFE0 to 0xFFE6: Full-width special symbols
    /// 
    /// The distinction between full-width and half-width characters is important for ensuring proper
    /// alignment and formatting in text-based representations, especially when working with mixed-language
    /// data or presenting data in a tabular format. This classification helps in correctly calculating
    /// string widths and aligning elements in debug text outputs, such as file paths or tabular data,
    /// where accurate spacing is crucial.
    /// </remarks>
    public static class VirtualTextFormatter
    {
        /// <summary>
        /// Generates and returns a text-based representation of the virtual storage tree structure.
        /// </summary>
        /// <typeparam name="T">The type of data in the storage.</typeparam>
        /// <param name="vs">The instance of the virtual storage.</param>
        /// <param name="basePath">The base path from which to start.</param>
        /// <param name="recursive">Whether to list subdirectories recursively.</param>
        /// <param name="followLinks">Whether to follow symbolic links.</param>
        /// <returns>The generated debug text of the tree structure.</returns>
        /// <remarks>
        /// The generated text represents the tree structure starting from the specified base path.
        /// Each node is represented by the result of the ToString method overridden in the specific 
        /// derived class of the node.
        ///
        /// <para>
        /// When `recursive` is true, subdirectories are listed recursively. If false, 
        /// subdirectories are not included in the output.
        /// </para>
        ///
        /// <para>
        /// When `followLinks` is true, symbolic links are replaced with their target paths 
        /// in the output. If false, the symbolic links themselves are displayed.
        /// </para>
        ///
        /// The output format includes indents and special characters to denote directory 
        /// hierarchies and symbolic links. Example:
        /// <code>
        /// /
        /// ├dir1/
        /// │├subdir1/
        /// ││└item3
        /// │└item2
        /// ├link-to-dir -> /dir1
        /// ├item1
        /// └link-to-item -> /item1
        /// </code>
        /// </remarks>
        public static string GenerateTreeDebugText<T>(this VirtualStorage<T> vs, VirtualPath basePath, bool recursive = true, bool followLinks = false)
        {
            const char FullWidthSpaceChar = '\u3000';
            StringBuilder tree = new();

            basePath = vs.ConvertToAbsolutePath(basePath).NormalizePath();
            IEnumerable<VirtualNodeContext> nodeContexts = vs.WalkPathTree(basePath, VirtualNodeTypeFilter.All, recursive, followLinks);
            StringBuilder line = new();
            string prevLine = string.Empty;

            VirtualNodeContext nodeFirstContext = nodeContexts.First();
            VirtualPath baseAbsolutePath = basePath + nodeFirstContext.TraversalPath;

            if (basePath.IsRoot)
            {
                tree.AppendLine("/");
            }
            else
            {
                tree.AppendLine(baseAbsolutePath);
            }

            foreach (var nodeInfo in nodeContexts.Skip(1))
            {
                VirtualNode node = nodeInfo.Node!;
                string nodeName = nodeInfo.TraversalPath.NodeName;
                int depth = nodeInfo.Depth;
                int count = nodeInfo.ParentDirectory!.Count;
                int index = nodeInfo.Index;

                line.Clear();

                if (depth > 0)
                {
                    for (int i = 0; i < depth - 1; i++)
                    {
                        switch (prevLine[i])
                        {
                            case FullWidthSpaceChar:
                                line.Append(FullWidthSpaceChar);
                                break;
                            case '│':
                                line.Append('│');
                                break;
                            case '├':
                                line.Append('│');
                                break;
                            default:
                                line.Append(FullWidthSpaceChar);
                                break;
                        }
                    }

                    if (index == count - 1)
                    {
                        line.Append('└');
                    }
                    else
                    {
                        line.Append('├');
                    }
                }

                line.Append(node.ToString());

                prevLine = line.ToString();
                tree.AppendLine(line.ToString());
            }

            return tree.ToString();
        }

        /// <summary>
        /// Generates and returns a table representation of symbolic link information.
        /// </summary>
        /// <typeparam name="T">The type of data in the storage.</typeparam>
        /// <param name="vs">The instance of the virtual storage.</param>
        /// <returns>The generated debug text of the link table.</returns>
        public static string GenerateLinkTableDebugText<T>(this VirtualStorage<T> vs)
        {
            if (vs.LinkDictionary.Count == 0)
            {
                return "(Link dictionary is empty.)";
            }

            List<IEnumerable<string>> tableData =
            [
                ["TargetPath", "LinkPath"]
            ];

            foreach (var entry in vs.LinkDictionary)
            {
                var targetPath = entry.Key;
                var linkPathsWithTypes = entry.Value
                    .Select(vp =>
                    {
                        var symbolicLinkNode = vs.TryGetSymbolicLink(vp);
                        var targetNodeType = symbolicLinkNode?.TargetNodeType.ToString() ?? "Unknown";
                        return $"{vp}({targetNodeType})";
                    })
                    .ToList();

                tableData.Add(
                [
                    targetPath.ToString(),
                    string.Join(", ", linkPathsWithTypes)
                ]);
            }

            return VirtualTextFormatter.FormatTable(tableData);
        }

        /// <summary>
        /// Generates and returns a table representation of the collection's contents.
        /// </summary>
        /// <typeparam name="T">The type of objects in the collection.</typeparam>
        /// <param name="enumerableObject">The collection to be represented.</param>
        /// <returns>The generated debug text of the table.</returns>
        public static string GenerateTableDebugText<T>(this IEnumerable<T> enumerableObject)
        {
            if (!enumerableObject.Any())
            {
                return "(Collection is empty.)";
            }

            List<IEnumerable<string>> tableData = [];

            Type type = typeof(T);
            bool isSimpleType = type == typeof(string) || Nullable.GetUnderlyingType(type) != null || type.IsPrimitive;

            if (isSimpleType)
            {
                // Handle simple types (string, nullable types, and primitives)
                tableData.Add(["Value"]);

                foreach (var obj in enumerableObject)
                {
                    tableData.Add([obj?.ToString() ?? "(null)"]);
                }
            }
            else
            {
                PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                if (properties.Length > 0)
                {
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
                }
                else
                {
                    tableData.Add(["Value"]);

                    foreach (var obj in enumerableObject)
                    {
                        tableData.Add([obj?.ToString() ?? "(null)"]);
                    }
                }
            }

            return FormatTable(tableData);
        }

        /// <summary>
        /// Generates and returns a table representation of a single object's properties.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="singleObject">The object to be represented.</param>
        /// <returns>The generated debug text of the table.</returns>
        public static string GenerateSingleTableDebugText<T>(this T singleObject)
        {
            Type type = typeof(T);
            List<PropertyInfo> properties = [];

            if (type == typeof(string))
            {
                List<IEnumerable<string>> stringTable =
                [
                    ["Value"],
                    [singleObject?.ToString() ?? "(null)"]
                ];
                return FormatTable(stringTable);
            }

            bool isSimpleType = Nullable.GetUnderlyingType(type) != null || type.IsPrimitive;

            if (isSimpleType)
            {
                List<IEnumerable<string>> simpleTable =
                [
                    ["Value"],
                    [singleObject?.ToString() ?? "(null)"]
                ];
                return FormatTable(simpleTable);
            }

            // Handle IEnumerable separately
            if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
            {
                List<IEnumerable<string>> enumerableTable =
                [
                    ["Value"],
                    ["(no output)"]
                ];
                return FormatTable(enumerableTable);
            }

            // Get properties from base classes first
            Type? currentType = type;
            while (currentType != null)
            {
                properties.InsertRange(0, currentType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
                currentType = currentType.BaseType;
            }

            var uniqueProperties = properties.GroupBy(p => p.Name).Select(g => g.First()).ToList();
            List<string> headers = uniqueProperties.Select(p => p.Name).ToList();
            List<string> row = [];

            foreach (var property in uniqueProperties)
            {
                try
                {
                    object? value = property.GetValue(singleObject);
                    if (value != null)
                    {
                        if (IsToStringOverridden(value.GetType()))
                        {
                            row.Add(value.ToString()!);
                        }
                        else if (value is IEnumerable && value is not string)
                        {
                            row.Add("(no output)");
                        }
                        else
                        {
                            row.Add(value.ToString()!);
                        }
                    }
                    else
                    {
                        row.Add("(null)");
                    }
                }
                catch
                {
                    row.Add("(exception)");
                }
            }

            if (headers.Count == 0)
            {
                headers.Add("Value");
                if (singleObject != null)
                {
                    row.Add(singleObject.ToString()!);
                }
                else
                {
                    row.Add("(null)");
                }
                //row.Add(singleObject?.ToString()!);
            }

            List<IEnumerable<string>> tableData =
            [
                headers,
            row
            ];

            return FormatTable(tableData);
        }

        /// <summary>
        /// Formats the table data and returns it as a text string.
        /// </summary>
        /// <param name="tableData">The table data.</param>
        /// <returns>The formatted table data as text.</returns>
        private static string FormatTable(IEnumerable<IEnumerable<string>> tableData)
        {
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

        /// <summary>
        /// Gets the column widths for the table data.
        /// </summary>
        /// <param name="tableData">The table data.</param>
        /// <returns>The width of each column.</returns>
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

        /// <summary>
        /// Gets the width of a string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>The width of the string.</returns>
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

        /// <summary>
        /// Determines if a character is full-width.
        /// </summary>
        /// <param name="c">The character to check.</param>
        /// <returns>True if the character is full-width; otherwise, false.</returns>
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

        /// <summary>
        /// Determines if the <c>ToString</c> method is overridden.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if <c>ToString</c> is overridden; otherwise, false.</returns>
        private static bool IsToStringOverridden(Type type)
        {
            MethodInfo toStringMethod = type.GetMethod("ToString", Type.EmptyTypes)!;
            return toStringMethod.DeclaringType != typeof(object);
        }
    }
}
