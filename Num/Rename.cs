// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Rename.cs" company="Software Inc.">
//   A.Robson
// </copyright>
// <summary>
//   Rename music and training files to a consistant format.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Num
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.PerformanceData;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    /// <summary>
    /// The rename.
    /// </summary>
    public class Rename
    {
        /// <summary>
        /// Gets or sets the item type.
        /// </summary>
        public static string ItemType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether continuous file numbering.
        /// </summary>
        public static bool Continuous { get; set; }

        public static int Main(string[] args)
        {
            ItemType = "m";
            Continuous = false;

            var argList = GetArguments(args);

            var fileDirectory = Environment.CurrentDirectory + @"\";

            var fileList = GetFileList(fileDirectory, argList.SubFolder);

            // remove all files that don't start with [0-9] or BS
            fileList = CleanFileList(fileList);

            var items = CreateItems(fileList, argList);

            if (argList.ChangeFileName)
            {
                ChangeFileNames(items);
            }

            WriteReport(items);

            Console.WriteLine("Finished...");


            return 0;
        }

        /// <summary>
        /// Change filenames.
        /// </summary>
        /// <param name="items">The file items.</param>
        private static void ChangeFileNames(IEnumerable<Item> items)
        {
            foreach (var item in items)
            {
                if (item.Changed)
                {
                    try
                    {
                        File.Move(item.Name, item.ChangeName);
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine(ex + "\n{0}", item.ChangeName);
                    }
                }
            }
        }

        /// <summary>
        /// Clean file list - remove files that don't start with [0-9].
        /// </summary>
        /// <param name="fileList">The full file list.</param>
        /// <returns>The <see cref="IEnumerable"/>clean file list.</returns>
        private static IEnumerable<string> CleanFileList(IEnumerable<string> fileList)
        {
            var newList = new List<string>();
            var singleDigit = "^[0-9]";

            foreach (var name in fileList)
            {
                if (Regex.IsMatch(Path.GetFileName(name), singleDigit) || Path.GetFileName(name).ToLowerInvariant().StartsWith("bs"))
                {
                    // Work out type of file
                    if (Path.GetExtension(name) == ".mp4" && ItemType != "t")
                    {
                        ItemType = "t";
                    }

                    newList.Add(name);
                }
            }

            return newList;
        }

        /// <summary>
        /// Create the items objects.
        /// </summary>
        /// <param name="fileList">The file list.</param>
        /// <param name="argList">The argument list.</param>
        /// <returns>The <see cref="List"/> items.</returns>
        private static List<Item> CreateItems(IEnumerable<string> fileList, ArgList argList)
        {
            var count = 0;
            var fileCount = 0;
            var itemList = new List<Item>();
            var folder = string.Empty;

            foreach (var name in fileList)
            {
                if (string.IsNullOrEmpty(folder) || Path.GetDirectoryName(name) != folder)
                {
                    folder = Path.GetDirectoryName(name); // new folder
                    fileCount = 0;
                }

                fileCount += 1;

                var item = new Item();

                item.ItemId = ++count;
                item.Name = name;

                item.ChangeName = RenameFile(name, fileCount, argList);

                item.Changed = item.Name != item.ChangeName;

                itemList.Add(item);
            }

            return itemList;
        }

        /// <summary>
        /// Rename music file.
        /// </summary>
        /// <param name="name">The file name.</param>
        /// <param name="count">file count number</param>
        /// <returns>The <see cref="string"/>clean music file name.</returns>
        private static string RenameFile(string name, int count, ArgList arglist)
        {
            var removeBracketsPattern = " \\(.*\\)";
            var directoryName = Path.GetDirectoryName(name);
            var extension = Path.GetExtension(name);
            var fileName = Path.GetFileNameWithoutExtension(name);

            if (fileName.ToLowerInvariant().StartsWith("bs"))
            {
                fileName = RemoveBossPrefix(fileName);
            }

            if (Regex.IsMatch(Path.GetFileName(fileName), removeBracketsPattern) && arglist.RemoveBrackets)
            {
                fileName = Regex.Replace(fileName, removeBracketsPattern, string.Empty);
            }

            fileName = RemoveModuleNumber(fileName);

            var iposn = FindAlpha(fileName);


            var prefix = fileName.Substring(0, iposn).TrimEnd();
            var shortName = fileName.Substring(iposn).TrimEnd();
            prefix = RemoveChars(prefix);
            prefix = RemoveLeadingZeros(prefix);

            if (count == 1)
            {
                // We are in a new folder so the file prefix should be 1.
                var number = 0;
                int.TryParse(prefix, out number);
                if (number > 1)
                {
                    Continuous = true;
                }
            }

            if (Continuous)
            {
                // All directory files are numbered 1 to nn.
                iposn = prefix.IndexOf('.', 0);

                if (iposn > 1)
                {
                    // grab the decimal part.
                    var decimalNumber = prefix.Substring(iposn + 1);
                    prefix = (count - 1) + "." + decimalNumber;
                }
                else
                {
                    prefix = count.ToString(CultureInfo.InvariantCulture);
                }
            }

            // work out how many digits at start of prefix
            iposn = prefix.IndexOf('.', 0);
            if (iposn > 0)
            {
                var newNumber = prefix.Substring(0, iposn);

                if (newNumber.Length == 1)
                {
                    prefix = 0 + prefix;
                }
            }
            else
            {
                if (prefix.Length == 1)
                {
                    prefix = 0 + prefix;
                }
            }

            if (ItemType == "t")
            {
                if (shortName.EndsWith(".zip"))
                {
                    shortName = shortName.Replace(".zip", string.Empty);
                }

                name = directoryName + "\\" + prefix + "-" + shortName + extension;
            }
            else
            {
                name = directoryName + "\\" + prefix + " - " + shortName + extension;
            }

            // Console.WriteLine(name);

            return name;
        }

        /// <summary>
        /// Write report on what needs to be changed.
        /// </summary>
        /// <param name="items">The items.</param>
        private static void WriteReport(List<Item> items)
        {
            var outFile = Environment.CurrentDirectory + "\\alan.log";
            var outStream = File.Create(outFile);
            var sw = new StreamWriter(outStream);

            // TODO: delete the log file if it exists
            foreach (var item in items)
            {
                if (item.Changed)
                {
                    sw.WriteLine("{0}\nto\n{1}\n\n", item.Name, item.ChangeName);
                }
            }

            // flush and close
            sw.Flush();
            sw.Close();
        }

        /// <summary>
        /// Remove leading zeros from the prefix.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <returns>The <see cref="string"/>cleaned prefix.</returns>
        private static string RemoveLeadingZeros(string prefix)
        {
            prefix = prefix.TrimStart(new char[] { '0' });

            return prefix;
        }

        /// <summary>
        /// Remove characters from the prefix string.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <returns>The <see cref="string"/>cleaned prefix.</returns>
        private static string RemoveChars(string prefix)
        {
            prefix = Regex.Replace(prefix, @"(\s+|-|_|)", string.Empty);

            if (prefix[prefix.Length - 1] == '.')
            {
                prefix = prefix.Substring(0, prefix.Length - 1);
            }

            return prefix;
        }

        /// <summary>
        /// Get a list of all files in a folder structure.
        /// </summary>
        /// <param name="folder">The folder name.</param>
        /// <param name="subFolders">The sub Folders.</param>
        /// <returns>A list of text files.</returns>
        private static IEnumerable<string> GetFileList(string folder, bool subFolders)
        {
            var dir = new DirectoryInfo(folder);
            var fileList = new List<string>();

            if (subFolders)
            {
                GetFiles(dir, fileList);
            }
            else
            {
                GetFiles(fileList);
            }

            return fileList;
        }

        /// <summary>
        /// Recursive list of files.
        /// </summary>
        /// <param name="d">Directory name.</param>
        /// <param name="fileList">The file List.</param>
        private static void GetFiles(DirectoryInfo d, ICollection<string> fileList)
        {
            var files = d.GetFiles("*.*");

            foreach (var fileName in files.Select(file => file.FullName))
            {
                fileList.Add(fileName);
            }

            // get sub-folders for the current directory
            var dirs = d.GetDirectories("*.*");

            // recurse
            foreach (var dir in dirs)
            {
                // Console.WriteLine(dir.FullName);
                GetFiles(dir, fileList);
            }
        }

        /// <summary>
        /// Get list of files.
        /// </summary>
        /// <param name="fileList">The image List.</param>
        private static void GetFiles(ICollection<string> fileList)
        {
            var imageDirectory = Environment.CurrentDirectory + @"\";
            var d = new DirectoryInfo(imageDirectory);

            var files = d.GetFiles("*.*");

            foreach (var fileName in files.Select(file => file.FullName))
            {
                if (Path.GetExtension(fileName.ToLowerInvariant()) != ".exe" && Path.GetExtension(fileName.ToLowerInvariant()) != ".bak")
                {
                    fileList.Add(fileName);
                }
            }
        }

        /// <summary>
        /// Get command line arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>The <see cref="bool"/>subfolder status.</returns>
        private static ArgList GetArguments(IList<string> args)
        {
            var subFolders = false;
            var changeFileNames = false;
            var removeBrackets = false;

            if (args.Count == 1)
            {
                if (args[0].ToLowerInvariant().Contains("s"))
                {
                    subFolders = true;
                }

                if (args[0].ToLowerInvariant().Contains("w"))
                {
                    changeFileNames = true;
                }

                if (args[0].ToLowerInvariant().Contains("b"))
                {
                    removeBrackets = true;
                }
            }

            if (args.Count == 1)
            {
                var argList = new ArgList(subFolders, changeFileNames, removeBrackets);

                return argList;
            }
            else
            {
                var arglist = new ArgList(false, false, false);

                return arglist;
            }
        }

        /// <summary>
        /// Remove the module number if there is one (leave the item number).
        /// </summary>
        /// <param name="name">The file name.</param>
        /// <returns>The <see cref="string"/>corrected file name.</returns>
        private static string RemoveModuleNumber(string name)
        {
            // I can have a 0101. or a 01 01. prefix
            var modulePattern = @"\d{4}";
            var moduleSpacePattern = @"\d{2}[ |_\-\.]\d{2}";

            if (Regex.IsMatch(Path.GetFileName(name), modulePattern))
            {
                var iposn = GetMatchPosition(name, modulePattern);

                if (iposn == 0)
                {
                    name = name.Substring(2);
                }

            }
            else if (Regex.IsMatch(Path.GetFileName(name), moduleSpacePattern))
            {
                // I'm only interested if the match is found at the start of the string.
                var iposn = GetMatchPosition(name, moduleSpacePattern);

                if (iposn == 0)
                {
                    name = name.Substring(3);
                }
            }

            return name;
        }

        /// <summary>
        /// Get match position.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="pattern">The regex pattern.</param>
        /// <returns>The <see cref="int"/>.
        /// </returns>
        private static int GetMatchPosition(string name, string pattern)
        {
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            var match = regex.Match(Path.GetFileName(name), 0);

            return match.Index;
        }

        /// <summary>
        /// Find the first alpha position.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <returns>The <see cref="int"/>position of the first alpha character.</returns>
        private static int FindAlpha(string fileName)
        {
            var iposn = 0;

            // loop through each character until you find the first Alpha
            for (var i = 0; i < fileName.Length; i++)
            {
                if (char.IsLetter(fileName[i]))
                {
                    iposn = i;
                    break;
                }
            }

            return iposn;
        }

        /// <summary>
        /// Remove the boss prefix.
        /// </summary>
        /// <param name="name">The file name.</param>
        /// <returns>The <see cref="string"/>cleaned file name.</returns>
        private static string RemoveBossPrefix(string name)
        {
            // remove boss prefix here - bs160131d1 08 Out in the Street.flac
            return name.Substring(11);
        }

        /// <summary>
        /// ReplaceEX: a case insensitive replace method.
        /// </summary>
        /// <param name="original">original string</param>
        /// <param name="pattern">pattern to replace</param>
        /// <param name="replacement">replacement text</param>
        /// <returns>the modified string</returns>
        private static string ReplaceEx(string original, string pattern, string replacement)
        {
            int position0, position1;
            var count = position0 = position1 = 0;
            var upperString = original.ToUpper();
            var upperPattern = pattern.ToUpper();
            var inc = (original.Length / pattern.Length) * (replacement.Length - pattern.Length);
            var chars = new char[original.Length + Math.Max(0, inc)];
            while ((position1 = upperString.IndexOf(upperPattern, position0, StringComparison.Ordinal)) != -1)
            {
                for (var i = position0; i < position1; ++i)
                {
                    chars[count++] = original[i];
                }

                for (var i = 0; i < replacement.Length; ++i)
                {
                    chars[count++] = replacement[i];
                }

                position0 = position1 + pattern.Length;
            }

            if (position0 == 0)
            {
                return original;
            }

            for (var i = position0; i < original.Length; ++i)
            {
                chars[count++] = original[i];
            }

            return new string(chars, 0, count);
        }
    }
}
