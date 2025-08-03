
using System.IO.Compression;

namespace FabPackager
{
	internal class Program
	{
		// path to plugin root. By default will use first commandline argument.
		// however you can also hardcode this string, and pass no argument
		//static string PLUGIN_ROOT = "D:\\git\\UEToolboxFabBuild_NoBinaries\\SOURCE_DISTRIB\\GradientspaceUEToolbox\\";
		static string PLUGIN_ROOT = "";

		// base name used for plugin zip files - eg GradientspaceUEToolbox_5p3.zip
		static string PluginZipFileBaseName = "GradientspaceUEToolbox";

		// copyright string to replace existing copyright string with  (to conform to Fab requirements)
		static string NewCopyrightString = "// Copyright 2024-2025 Gradientspace Corp. All Rights Reserved.";
		// set to false to disable copyright rewriting
		static bool bConvertCopyrights = true;

		// list of versions to create zip files for - will result in (eg) UEToolbox_5p3.zip, UEToolbox_5p4.zip, etc
		static List<string> EngineVersions = new() { "5.3", "5.4", "5.5", "5.6" };

		// list of modules to delete before packaging - can be empty
		static List<string> RemoveModules = new List<string>() { "GradientspaceUEPluginManager" };




		static void Main(string[] args)
		{
			if (args[0].Length > 0)
				PLUGIN_ROOT = args[0];

			if (!Directory.Exists(PLUGIN_ROOT)) {
				System.Console.WriteLine("Usage: FabPackager <PluginRootPath>");
				return;
			}

			// search for .uplugin file in the target directory
			UPLUGIN_PATH = FindUPlugin(PLUGIN_ROOT);
			if (File.Exists(UPLUGIN_PATH) == false) {
				System.Console.WriteLine($"No UPlugin file exists at path ${UPLUGIN_PATH}");
				return;
			}

			// remove any PDBs (only needed for builds that include binaries)
			RemovePDBs(PLUGIN_ROOT);

			// remove non-public modules if there are any
			foreach (string moduleName in RemoveModules)
				RemoveModule(PLUGIN_ROOT, moduleName);

			// remove any empty files, will be rejected by Fab 
			RemoveEmptyFiles(PLUGIN_ROOT);

			// convert copyright string to Fab-compatible, if desired
			if (bConvertCopyrights)
				ConvertCopyrights(PLUGIN_ROOT);

			// make a zip file for each engine version, with suitable EngineVersion string in .uplugin
			foreach (string EngineVersionString in EngineVersions)
				MakeEngineVersionZipFile(EngineVersionString);
		}

		// will be found automatically
		static string UPLUGIN_PATH = "";


		static void MakeEngineVersionZipFile(string EngineVersionString)
		{
			SetEngineVersion(EngineVersionString);
			string ParentPath = Path.GetFullPath(Path.Combine(PLUGIN_ROOT, ".."));
			string NameString = EngineVersionString.Replace('.', 'p');
			string ArchiveName = Path.Combine(ParentPath, PluginZipFileBaseName + "_" + NameString + ".zip");
			if (File.Exists(ArchiveName))
				File.Delete(ArchiveName);
			ZipFile.CreateFromDirectory(PLUGIN_ROOT, ArchiveName, CompressionLevel.SmallestSize, true);
		}


		static void SetEngineVersion(string EngineVersion)
		{
			string[] Lines = File.ReadAllLines(UPLUGIN_PATH);
			for ( int i = 0;  i < Lines.Length; i++ ) {
				string line = Lines[i];
				if (line.Contains("\"EngineVersion\"")) {
					int quote2 = line.LastIndexOf('\"');
					int quote1 = line.LastIndexOf('\"', quote2-1);
					if (quote2 != quote1+1)
						line = line.Remove(quote1+1, quote2-(quote1+1));
					line = line.Insert(quote1+1, EngineVersion);
					Lines[i] = line;
					break;
				}
			}
			File.WriteAllLines(UPLUGIN_PATH, Lines);
		}


		static bool RemoveModule(string BasePath, string ModuleName)
		{
			bool bRemoveDirOK = false;
			string ModulePath = Path.Combine(BasePath, "Source", ModuleName);

			if (Directory.Exists(ModulePath)) {
				Directory.Delete(ModulePath, true);
				bRemoveDirOK = true;
			}

			bool bRemoveUPlugin = false;
			string UPluginText = File.ReadAllText(UPLUGIN_PATH);

			int Modules_Start = UPluginText.IndexOf("\"Modules\"");
			int Plugins_Start = UPluginText.IndexOf("\"Plugins\"");

			int ModuleIndex = UPluginText.IndexOf(ModuleName);
			if (ModuleIndex != -1) {
				int NextBracketIndex = UPluginText.IndexOf('}', ModuleIndex);
				int PrevBracketIndex = find_prev(UPluginText, '{', ModuleIndex, Modules_Start, '[');

				int Comma_Prev = (PrevBracketIndex != -1) ?
					find_prev(UPluginText, ',', PrevBracketIndex, Modules_Start, '}') : -1;
				int Comma_Next = (NextBracketIndex != -1) ?
					find_next(UPluginText, ',', NextBracketIndex, Plugins_Start, '{', ']') : -1;

				if (Comma_Prev != -1) {
					UPluginText = UPluginText.Remove(Comma_Prev, (NextBracketIndex - Comma_Prev + 1));
					//File.WriteAllText(UPLUGIN_PATH + ".txt", UPluginText);
					File.WriteAllText(UPLUGIN_PATH, UPluginText);
				} else
					throw new Exception("[RemoveModule] unhandled case in RemoveModule...");

				bRemoveUPlugin = true;
			}

			return (bRemoveDirOK && bRemoveUPlugin);
		}


		static void RemovePDBs(string BasePath)
		{
			string[] PDBFiles = Directory.GetFiles(BasePath, "*.pdb", SearchOption.AllDirectories);
			foreach (string PDBFile in PDBFiles) {
				File.Delete(PDBFile);
			}
		}




		static string FindUPlugin(string BasePath)
		{
			string[] PluginFiles = Directory.GetFiles(BasePath, "*.uplugin");
			if (PluginFiles.Length > 0)
				return PluginFiles[0];
			return "";
		}


		static int find_prev(string Str, char c, int start_at, int stop_idx, char stop_char1)
		{
			int cur = start_at;
			while (--cur > 0) {
				if (Str[cur] == c)
					return cur;
				if (Str[cur] == stop_char1)
					return -1;
				if (cur == stop_idx)
					return -1;
			}
			return -1;
		}

		static int find_next(string Str, char c, int start_at, int stop_idx, char stop_char1, char stop_char2)
		{
			int cur = start_at;
			while (++cur < Str.Length) {
				if (Str[cur] == c)
					return cur;
				if (Str[cur] == stop_char1 || Str[cur] == stop_char2)
					return -1;
				if (cur == stop_idx)
					return -1;
			}
			return -1;
		}




		static void RemoveEmptyFiles(string PathRoot)
		{
			string[] CppCodeFiles = Directory.GetFiles(PathRoot, "*.cpp", SearchOption.AllDirectories);
			string[] CppHeaderFiles = Directory.GetFiles(PathRoot, "*.h", SearchOption.AllDirectories);
			List<string[]> FileSetsToProcess = new List<string[]>();
			FileSetsToProcess.Add(CppCodeFiles);
			FileSetsToProcess.Add(CppHeaderFiles);

			foreach (string[] FileSet in FileSetsToProcess) {
				foreach (string FilePath in FileSet) {
					string[] Lines = File.ReadAllLines(FilePath);
					if (Lines.Length <= 2)
						File.Delete(FilePath);
				}
			}
		}


		static void ConvertCopyrights(string PathRoot)
		{
			string CopyrightHeader = "// Copyright";

			string[] CsCodeFiles = Directory.GetFiles(PathRoot, "*.cs", SearchOption.AllDirectories);
			string[] CppCodeFiles = Directory.GetFiles(PathRoot, "*.cpp", SearchOption.AllDirectories);
			string[] CppHeaderFiles = Directory.GetFiles(PathRoot, "*.h", SearchOption.AllDirectories);

			List<string[]> FileSetsToProcess = new List<string[]>();
			FileSetsToProcess.Add(CppCodeFiles);
			FileSetsToProcess.Add(CppHeaderFiles);
			FileSetsToProcess.Add(CsCodeFiles);

			foreach (string[] FileSet in FileSetsToProcess) {
				foreach (string FilePath in FileSet) {
					string[] Lines = File.ReadAllLines(FilePath);
					if (Lines.Length < 1)
						continue;

					if (Lines[0].StartsWith(CopyrightHeader) == false) {
						List<string> NewLines = new List<string>();
						NewLines.Add(NewCopyrightString);
						NewLines.AddRange(Lines);
						File.WriteAllLines(FilePath, NewLines);
					} else {
						Lines[0] = NewCopyrightString;
						File.WriteAllLines(FilePath, Lines);
					}
				}
			}
		}

	}
}
