using CommunityToolkit.Mvvm.DependencyInjection;
using DialogHostAvalonia;
using EPP.Models;
using EPP.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EPP.Helpers
{
    public static class EventSavingHelper
    {
        public async static Task SaveEvent(EventFile eventFile)
        {
            var fileService = Ioc.Default.GetService<IFileService>()!;
            var configService = Ioc.Default.GetService<IConfigService>()!;

            var file = await fileService.OpenFileAsync(configService.ConfigData.EventPath);

            if (file == null)
            {
                throw new Exception("File not found");
            }

            string lineEnding = "\n";
            List<string> fileContent = new();
            await using (var stream = await file.OpenReadAsync())
            {
                using var streamReader = new StreamReader(stream);
                while (!streamReader.EndOfStream)
                {
                    var line = streamReader.ReadLine();
                    if (line != null)
                    {
                        fileContent.Add(line);
                    }
                }

                // Reset stream position to start
                streamReader.DiscardBufferedData();
                streamReader.BaseStream.Seek(0, SeekOrigin.Begin);

                // Store line ending to keep what file used
                if (streamReader.ReadToEnd().EndsWith("\r\n"))
                {
                    lineEnding = "\r\n";
                }
            }

            var balance = GetBracketBalance(fileContent);
            foreach (var modEvent in eventFile.Events)
            {
                if (modEvent.IsChanged)
                {
                    int eventStart = GetEventStart(modEvent, fileContent, balance);
                    if (eventStart == -1)
                    {
                        await DialogHost.Show(new InfoDialogData($"Error occured during saving pictures, could not find matching event for {modEvent.Id}, check if file was changed by other applications"), "MainDialogHost");
                        return;
                    }

                    if (modEvent.IsChanged)
                    {
                        bool wasSafeSuccess = await ReplacePictures(modEvent, fileContent, eventStart);
                        if (!wasSafeSuccess)
                        {
                            return;
                        }
                    }
                }
            }

            await using (var stream = await file.OpenWriteAsync())
            {
                using var streamWriter = new StreamWriter(stream);
                streamWriter.NewLine = lineEnding;
                foreach (var line in fileContent)
                {
                    await streamWriter.WriteLineAsync(line);
                }
            }

            foreach (var modEvent in eventFile.Events)
            {
                foreach (var picture in modEvent.Pictures)
                {
                    picture.Original = picture.Current;
                }
                modEvent.RefreshIsChanged();
            }
        }

        private static List<int> GetBracketBalance(List<string> text)
        {
            List<int> balance = new List<int>(text.Count);
            int currentBalance = 0;

            for (int i = 0; i < text.Count; i++)
            {
                string line = text[i];
                int commentIndex = line.IndexOf('#');
                if (commentIndex != -1)
                {
                    line = line.Substring(0, commentIndex);
                }

                currentBalance += line.Count(c => c == '{');
                currentBalance -= line.Count(c => c == '}');
                balance.Add(currentBalance);
            }

            return balance;
        }

        private static int GetEventStart(ModEvent modEvent, List<string> text, List<int> balance)
        {
            int idPosition = -1;

            for (int i = 1; i < text.Count; i++)
            {
                string line = text[i];
                if (balance[i - 1] == 1 && line.Contains("id"))
                {
                    // Clear comments and make sure full words will be compared instead of regular contains check (e.g. event with id 501 vs id of 50)
                    line = line.Split("#")[0].Trim().Replace("=", "").Replace("  ", " ");
                    if (line.Split()[1] == modEvent.Id)
                    {
                        idPosition = i;
                        break;
                    }
                }
            }

            // Go back and find event start (in case picture got defined above id)
            for (int i = idPosition; i > 0; i--)
            {
                if (balance[i] == 0)
                {
                    return i;
                }
            }

            return -1;
        }

        private static async Task<bool> ReplacePictures(ModEvent modEvent, List<string> text, int eventStart)
        {
            int pictureIndex = 0;
            EventPicture currentPicture = modEvent.Pictures[pictureIndex];
            bool isInsidePicture = false;

            for (int i = eventStart + 1; i < text.Count; i++)
            {
                string line = text[i];

                // Should only happen for those without any picture
                if (string.IsNullOrEmpty(currentPicture.Original))
                {
                    if (line.Contains("id"))
                    {
                        text.Insert(i + 1, "\tpicture = " + currentPicture.Current);
                        currentPicture.Original = currentPicture.Current;
                        return true;
                    }
                }
                else if (line.Contains("picture"))
                {
                    if (isInsidePicture || currentPicture.IsSimplePicture)
                    {
                        if (!line.Contains(currentPicture.Original))
                        {
                            await DialogHost.Show(new InfoDialogData($"Error occured during saving pictures, pictures structure for event {modEvent.Id} did not match loaded one"), "MainDialogHost");
                            return false;
                        }

                        text[i] = line.Replace(currentPicture.Original, currentPicture.Current);
                        if (pictureIndex == modEvent.Pictures.Count - 1)
                        {
                            return true;
                        }

                        isInsidePicture = false;
                        currentPicture = modEvent.Pictures[++pictureIndex];
                    }
                    else
                    {
                        if (!line.Contains('{'))
                        {
                            await DialogHost.Show(new InfoDialogData($"Error occured during saving pictures, pictures structure for event {modEvent.Id} did not match loaded one"), "MainDialogHost");
                            return false;
                        }

                        isInsidePicture = true;
                    }
                }
            }

            await DialogHost.Show(new InfoDialogData($"Error occured during saving pictures, pictures structure for event {modEvent.Id} did not match loaded one"), "MainDialogHost");
            return false;
        }
    }
}
