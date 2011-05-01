using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using WoWReader;
using System.CodeDom;
using System.CodeDom.Compiler;

namespace SimpleEnumExtractor {
	class Program {
		static GenericReader gr;
		static TextReader tr;

		static string streamString = String.Empty;

		static void Main(string[] args) {
		    // Ищем файл wow.exe сначала в текущей папке,
			// затем лезем в реестр и смотрим папку там.
			// TODO: взять путь к wow.exe из аргументов ком. строки
			var wowPath = "WoW.exe";
			if(!File.Exists(wowPath)) {
				wowPath = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Blizzard Entertainment\World of Warcraft",
					"InstallPath", null);
				wowPath = Path.Combine(wowPath ?? string.Empty, "WoW.exe");
			}

			var wowExe = new FileInfo(wowPath);
			if(!wowExe.Exists) {
				Console.WriteLine("File {0} not found!", wowExe.Name);
			}
			else {
				Console.WriteLine("File {0} found in {1}", wowExe.Name, wowExe.DirectoryName);
				Console.WriteLine("Extracting...");
			    using (var stream = wowExe.Open(FileMode.Open, FileAccess.Read))
			    {
			        gr = new GenericReader(stream, Encoding.ASCII);
			        tr = new StreamReader(stream, Encoding.ASCII);

			        streamString = tr.ReadToEnd();

                    ExtractEnum("ChatMsg", "CHAT_MSG_GUILD_ACHIEVEMENT", "CHAT_MSG_ADDON", s => ToCamelCase(s.Substring("CHAT_MSG_".Length)), -1);
			        ExtractEnum("ResponseCodes", "CHAR_NAME_DECLENSION_DOESNT_MATCH_BASE_NAME",
			                    "RESPONSE_SUCCESS");
			        ExtractEnum("ChatNotify", "VOICE_OFF", "YOU_CHANGED", a => string.Format("CHAT_{0}_NOTICE", a), 0);
			        ExtractEnum("OpCodes", "NUM_MSG_TYPES", "MSG_NULL_ACTION");
			        ExtractEnum("SpellFailedReason", "SPELL_FAILED_UNKNOWN",
			                    "SPELL_FAILED_SUCCESS");
			        ExtractEnum("EnchantConditions", "ENCHANT_CONDITION_REQUIRES",
			                    "ENCHANT_CONDITION_EQUAL_VALUE");
			        ExtractEnum("ItemModType", "ITEM_MOD_SPELL_POWER", "ITEM_MOD_MANA");

			        tr.Close();
			        gr.Close();
			    }
			    Console.WriteLine("Done");
			}
			Console.Write("Press any key to exit...");
			Console.ReadKey();
		}

        static string ToCamelCase(string value)
        {
            return string.Concat(value
                                     .Split('_')
                                     .Select(str => char.ToUpperInvariant(str[0]) + str.Substring(1).ToLowerInvariant())
                                     .ToArray());
        }

	    static void ExtractEnum(string name, string start, string end, Func<string, string> convertor, int index) {
			try {
				Console.Write("  Extracting {0,-61}", name);
			    var values = ReadValues(start, end)
                    .Select(convertor)
                    .ToList();
			    DumpToFile(name, values, index);
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("done");
			}
			catch {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("error");
			}
			finally {
				Console.ResetColor();
			}
		}

		static void ExtractEnum(string name, string start, string end) {
			ExtractEnum(name, start, end, a => a, 0);
		}

		static IEnumerable<string> ReadValues(string start, string end) {
			gr.BaseStream.Position = FindStartPos(start);
			var names = new List<string>();
			string value;
			do {
				value = gr.ReadStringNull();
			    names.Add(value);
				FindNextPos(gr);
			} while(value != end);
			names.Reverse();
			return names.ToArray();
		}

	    static void DumpToFile(string enumName, IEnumerable<string> names, int first) {
			// TODO: Необходимо задавать провайдер из параметров командной строки
			// по умолчанию брать Cpp провайдер
			var @enum = new CodeTypeDeclaration(enumName) {
				IsEnum = true,
			};

			foreach(var name in names) {
				var field = new CodeMemberField(string.Empty, name) {
					InitExpression = new CodePrimitiveExpression(first),
				};
				@enum.Members.Add(field);
				first++;
			}

			var codeProvider = CodeDomProvider.CreateProvider("CSharp");
			var options = new CodeGeneratorOptions {
				BlankLinesBetweenMembers = false,
                BracingStyle = "C",
			};

			using(var writer = new StreamWriter(enumName + "." + codeProvider.FileExtension)) {
				codeProvider.GenerateCodeFromType(@enum, writer, options);
			}
		}

	    static int FindStartPos(string name) {
			return streamString.IndexOf(name + '\0');
		}

		static void FindNextPos(BinaryReader reader) {
			while(reader.PeekChar() == 0x00) {
				reader.BaseStream.Position++;
			}
		}
	}
}
