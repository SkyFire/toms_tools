using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace dbc2sql
{
    class Program
    {
        static IWowClientDBReader m_reader;
        static XmlDocument m_definitions;
        static string DBCName;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please specify file name!");
                return;
            }

            string fileName = args[0];

            if (!File.Exists(fileName))
            {
                Console.WriteLine("File {0} doesn't exist", fileName);
                return;
            }

            LoadDefinitions();

            if (Path.GetExtension(fileName).ToLowerInvariant() == ".dbc")
                m_reader = new DBCReader(fileName);
            else
                m_reader = new DB2Reader(fileName);

            Console.WriteLine("Records: {0}, Fields: {1}, Row Size {2}, String Table Size: {3}", m_reader.RecordsCount, m_reader.FieldsCount, m_reader.RecordSize, m_reader.StringTableSize);

            DBCName = Path.GetFileNameWithoutExtension(fileName);

            XmlElement definition = m_definitions["DBFilesClient"][DBCName];

            if (definition == null)
            {
                Console.WriteLine("Definition for file {0} not found! File name is case sensitive!", fileName);
                return;
            }

            XmlNodeList fields = definition.GetElementsByTagName("field");
            XmlNodeList indexes = definition.GetElementsByTagName("index");

            StreamWriter sqlWriter = new StreamWriter(fileName + ".sql");

            WriteSqlStructure(sqlWriter, fileName, fields, indexes);

            for (int i = 0; i < m_reader.RecordsCount; ++i)
            {
                BinaryReader reader = m_reader[i];

                StringBuilder result = new StringBuilder();
                result.Append("INSERT INTO `dbc_" + DBCName + "` VALUES (");

                int flds = 0;

                foreach (XmlElement field in fields)
                {
                    switch (field.Attributes["type"].Value)
                    {
                        case "long":
                            result.Append(reader.ReadInt64());
                            break;
                        case "ulong":
                            result.Append(reader.ReadUInt64());
                            break;
                        case "int":
                            result.Append(reader.ReadInt32());
                            break;
                        case "uint":
                            result.Append(reader.ReadUInt32());
                            break;
                        case "short":
                            result.Append(reader.ReadInt16());
                            break;
                        case "ushort":
                            result.Append(reader.ReadUInt16());
                            break;
                        case "sbyte":
                            result.Append(reader.ReadSByte());
                            break;
                        case "byte":
                            result.Append(reader.ReadByte());
                            break;
                        case "float":
                            result.Append(reader.ReadSingle().ToString(CultureInfo.InvariantCulture));
                            break;
                        case "double":
                            result.Append(reader.ReadDouble().ToString(CultureInfo.InvariantCulture));
                            break;
                        case "string":
                            result.Append("\"" + StripBadCharacters(m_reader.StringTable[reader.ReadInt32()]) + "\"");
                            break;
                        default:
                            throw new Exception(String.Format("Unknown field type {0}!", field.Attributes["type"].Value));
                    }

                    if (flds != fields.Count - 1)
                        result.Append(", ");

                    flds++;
                }

                result.Append(");");
                sqlWriter.WriteLine(result);

                if (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    Console.WriteLine("Data under read!!!, diff {0}", reader.BaseStream.Length - reader.BaseStream.Position);
                }

                reader.Close();
            }

            sqlWriter.Flush();
            sqlWriter.Close();
        }

        static void LoadDefinitions()
        {
            Console.WriteLine("Loading XML configuration file...");
            m_definitions = new XmlDocument();
            m_definitions.Load("dbclayout.xml");
            Console.WriteLine("Done!");
        }

        static void WriteSqlStructure(StreamWriter sqlWriter, string fileName, XmlNodeList fields, XmlNodeList indexes)
        {
            sqlWriter.WriteLine("DROP TABLE IF EXISTS `dbc_{0}`;", DBCName);
            sqlWriter.WriteLine("CREATE TABLE `dbc_{0}` (", DBCName);

            foreach (XmlElement field in fields)
            {
                sqlWriter.Write("\t" + String.Format("`{0}`", field.Attributes["name"].Value));

                switch (field.Attributes["type"].Value)
                {
                    case "long":
                        sqlWriter.Write(" BIGINT NOT NULL DEFAULT '0'");
                        break;
                    case "ulong":
                        sqlWriter.Write(" BIGINT UNSIGNED NOT NULL DEFAULT '0'");
                        break;
                    case "int":
                        sqlWriter.Write(" INT NOT NULL DEFAULT '0'");
                        break;
                    case "uint":
                        sqlWriter.Write(" INT UNSIGNED NOT NULL DEFAULT '0'");
                        break;
                    case "short":
                        sqlWriter.Write(" SMALLINT NOT NULL DEFAULT '0'");
                        break;
                    case "ushort":
                        sqlWriter.Write(" SMALLINT UNSIGNED NOT NULL DEFAULT '0'");
                        break;
                    case "sbyte":
                        sqlWriter.Write(" TINYINT NOT NULL DEFAULT '0'");
                        break;
                    case "byte":
                        sqlWriter.Write(" TINYINT UNSIGNED NOT NULL DEFAULT '0'");
                        break;
                    case "float":
                        sqlWriter.Write(" FLOAT NOT NULL DEFAULT '0'");
                        break;
                    case "double":
                        sqlWriter.Write(" DOUBLE NOT NULL DEFAULT '0'");
                        break;
                    case "string":
                        sqlWriter.Write(" TEXT NOT NULL");
                        break;
                    default:
                        throw new Exception(String.Format("Unknown field type {0}!", field.Attributes["type"].Value));
                }

                sqlWriter.WriteLine(",");
            }

            foreach (XmlElement index in indexes)
            {
                sqlWriter.WriteLine("\tPRIMARY KEY (`{0}`)", index["primary"].InnerText);
            }

            sqlWriter.WriteLine(") ENGINE=MyISAM DEFAULT CHARSET=utf8 COMMENT='Export of {0}';", Path.GetFileName(fileName));
            sqlWriter.WriteLine();
        }

        static string StripBadCharacters(string input)
        {
            input = Regex.Replace(input, @"'", @"\'");
            input = Regex.Replace(input, @"\""", @"\""");
            return input;
        }
    }
}
