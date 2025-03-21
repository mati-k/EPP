using DialogHostAvalonia;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EPP.Models
{
    public class Localization
    {
        Dictionary<string, string> _entries = new();

        public async Task<bool> LoadFromFileAsync(string filePath)
        {
            try
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        while (!reader.EndOfStream)
                        {
                            string? line = reader.ReadLine();

                            if (String.IsNullOrWhiteSpace(line))
                                continue;

                            line = line.Trim();
                            // Ignore comment line
                            if (line.StartsWith("#"))
                            {
                                continue;
                            }

                            if (line.Contains("#"))
                            {
                                line = line.Substring(0, line.IndexOf("#")).Trim();
                            }

                            if (line.IndexOf(' ') == -1)
                            {
                                continue;
                            }

                            string[] split = line.Split(new char[] { ' ' }, 2);

                            if (split[0].Length == 0 || split[1].Length <= 2)
                            {
                                continue;
                            }

                            string[] key = split[0].Split(':');
                            string value = split[1].Substring(1, split[1].Length - 2);

                            if (string.IsNullOrEmpty(key[0]))
                            {
                                continue;
                            }

                            if (!_entries.ContainsKey(key[0]))
                            {
                                _entries.Add(key[0], value);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Error occured during reading localization file");
                await DialogHost.Show(new InfoDialogData("Error occured during reading event localization, see logs folder for more information"), "MainDialogHost");
                return false;
            }

            return true;
        }

        public string GetValueForKey(string key)
        {
            if (!string.IsNullOrEmpty(key) && _entries.ContainsKey(key))
            {
                return _entries[key];
            }

            return key;
        }
    }
}
