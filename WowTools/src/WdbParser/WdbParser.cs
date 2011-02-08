using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using WowTools.Core;

namespace WdbParser
{
    class WdbParser
    {
        XmlDocument m_definitions2 = new XmlDocument();
        const string quote = "\"";

        public WdbParser(string folderName)
        {
            LoadDefinitions();

            var xmlNodes = m_definitions2.GetElementsByTagName("wdbId");
            foreach (XmlElement el in xmlNodes)
            {
                var fileName = folderName + "\\" + el.Attributes["name"].Value + ".wdb";
                if (!File.Exists(fileName))
                {
                    Console.WriteLine("File {0} not found!", fileName);
                    continue;
                }
                ParseWdbFile(fileName, el);
            }

            Console.WriteLine("Done!");
            Console.ReadKey();
        }

        void LoadDefinitions()
        {
            Console.WriteLine("Loading XML configuration file...");

            var defFileName = "definitions.xml";

            if (!File.Exists(defFileName))
            {
                Console.WriteLine("Configuration file {0} not found!", Directory.GetCurrentDirectory() + "\\" + defFileName);
                return;
            }

            m_definitions2.Load("definitions.xml");

            Console.WriteLine("Done.");
        }

        void ParseWdbFile(string fileName, XmlElement el)
        {
            var reader = new BinaryReader(new FileStream(fileName, FileMode.Open, FileAccess.Read));
            var writer = new StreamWriter(fileName + ".sql");
            writer.AutoFlush = true;
            var sig = reader.ReadBytes(4);
            var build = reader.ReadUInt32();
            var locale = reader.ReadBytes(4);
            var unk1 = reader.ReadUInt32();
            var unk2 = reader.ReadUInt32();
            var ver = reader.ReadUInt32();
            var end = false;

            Console.WriteLine("Signature: {0}, build {1}, locale {2}", Encoding.ASCII.GetString(sig.Reverse().ToArray()), build, Encoding.ASCII.GetString(locale.Reverse().ToArray()));
            Console.WriteLine("unk1 {0}, unk2 {1}, ver {2}", unk1, unk2, ver);

            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                var InsertQuery = String.Format("INSERT INTO {0} VALUES (", el.Attributes["name"].Value);

                var i = 0;
                var xmlNodes = el.GetElementsByTagName("wdbElement");
                foreach (XmlElement elem in xmlNodes)
                {
                    var valtype = elem.Attributes["type"].Value;
                    string valname = null;
                    if (elem.Attributes["name"] != null)
                        valname = elem.Attributes["name"].Value;

                    switch (valtype.ToUpper())
                    {
                        case "INTEGER":
                            {
                                var valval = reader.ReadInt32();
                                if (valname != null)
                                    InsertQuery += valval;
                                break;
                            }
                        case "UINTEGER":
                            {
                                var valval = reader.ReadUInt32();
                                if (valname != null)
                                    InsertQuery += valval;
                                break;
                            }
                        case "SINGLE":
                            {
                                var valval = reader.ReadSingle().ToString("F", CultureInfo.InvariantCulture);
                                if (valname != null)
                                    InsertQuery += valval;
                                break;
                            }
                        case "VARCHAR":
                            {
                                var valval = Regex.Replace(reader.ReadCString(), @"'", @"\'");
                                valval = Regex.Replace(valval, "\"", "\\\"");
                                if (valname != null)
                                    InsertQuery += (quote + valval + quote);
                                break;
                            }
                        case "SMALLINT":
                            {
                                var valval = reader.ReadInt16();
                                if (valname != null)
                                    InsertQuery += valval;
                                break;
                            }
                        case "TINYINT":
                            {
                                var valval = reader.ReadSByte();
                                if (valname != null)
                                    InsertQuery += valval;
                                break;
                            }
                        case "BYTETOINT":
                            {
                                var valval = reader.ReadInt32();
                                if (valname != null)
                                    InsertQuery += valval;
                                break;
                            }
                        case "STRUCT":
                            {
                                var valval = reader.ReadInt32();
                                if (valname != null)
                                    InsertQuery += valval;
                                InsertQuery += ",";

                                var maxcount = Convert.ToUInt32(elem.Attributes["maxcount"].Value);

                                XmlNodeList structNodes = elem.GetElementsByTagName("structElement");

                                if (valval > 0)
                                {
                                    for (var k = 0; k < valval; k++)
                                    {
                                        foreach (XmlElement structElem in structNodes)
                                        {
                                            // something wrong here...
                                            var last = (valval == maxcount);
                                            ReadAndDumpByType(ref reader, structElem.Attributes["type"].Value.ToUpper(), valname, ref InsertQuery, last);
                                        }
                                    }
                                }

                                if (maxcount > 0)
                                {
                                    var lim = (int)maxcount - valval;
                                    for (var p = 0; p < lim; p++)
                                        foreach (XmlElement structElem in structNodes)
                                            InsertQuery += "0,";
                                    // remove last ","
                                    InsertQuery = InsertQuery.Remove(InsertQuery.Length - 1);
                                }

                                break;
                            }
                        case "SIZE":
                            {
                                var valval = reader.ReadUInt32();
                                if (valval == 0)
                                    end = true;
                                break;
                            }
                        default:
                            {
                                Console.WriteLine("Unknown type {0}", valtype.ToUpper());
                                break;
                            }
                    }

                    if (i != xmlNodes.Count - 1)
                    {
                        if (valname != null)
                            InsertQuery += ",";
                    }
                    else // query end
                    {
                        InsertQuery += ");";
                        writer.WriteLine(InsertQuery);
                    }

                    if (end)
                        break;

                    i++;
                }
            }

            reader.Close();
            writer.Flush();
            writer.Close();
        }

        void ReadAndDumpByType(ref BinaryReader reader, string valtype, string valname, ref string InsertQuery, bool last)
        {
            switch (valtype.ToUpper())
            {
                case "INTEGER":
                    {
                        var valval = reader.ReadInt32();
                        if (valname != null)
                            InsertQuery += valval;
                        break;
                    }
                case "UINTEGER":
                    {
                        var valval = reader.ReadUInt32();
                        if (valname != null)
                            InsertQuery += valval;
                        break;
                    }
                case "SINGLE":
                    {
                        var valval = reader.ReadSingle().ToString("F", CultureInfo.InvariantCulture);
                        if (valname != null)
                            InsertQuery += valval;
                        break;
                    }
                case "VARCHAR":
                    {
                        var valval = Regex.Replace(reader.ReadCString(), @"'", @"\'");
                        valval = Regex.Replace(valval, "\"", "\\\"");
                        if (valname != null)
                            InsertQuery += (quote + valval + quote);
                        break;
                    }
                case "SMALLINT":
                    {
                        var valval = reader.ReadInt16();
                        if (valname != null)
                            InsertQuery += valval;
                        break;
                    }
                case "TINYINT":
                    {
                        var valval = reader.ReadSByte();
                        if (valname != null)
                            InsertQuery += valval;
                        break;
                    }
                case "BYTETOINT":
                    {
                        var valval = reader.ReadInt32();
                        if (valname != null)
                            InsertQuery += valval;
                        break;
                    }
                default:
                    {
                        Console.WriteLine("Unknown type {0}", valtype.ToUpper());
                        break;
                    }
            }

            if (!last)
                InsertQuery += ",";
        }
    }
}
