using Google.Cloud.TextToSpeech.V1;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TextToSpeechApiDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome.\nThe program will read all the files under `texts/` and \nconvert to audio files and place under `audio/`.\n");

            // Directory for the SSML text files.
            string textsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "texts");

            // Directory for the output audio files.
            string audioDirectory = Path.Combine(Directory.GetCurrentDirectory(), "audio");

            // Directory for snapshots
            string snapshotDirectory = Path.Combine(Directory.GetCurrentDirectory(), "snapshots");

            // Create the "audio" and "snapshots" directories if they don't exist.
            Directory.CreateDirectory(audioDirectory);
            Directory.CreateDirectory(snapshotDirectory);

            // Check if the texts directory exists.
            if (!Directory.Exists(textsDirectory))
            {
                Console.WriteLine($"Error: Texts directory not found at '{textsDirectory}'.");
                return;
            }

            // Get all .xml files in the texts directory.
            string[] xmlFiles = Directory.GetFiles(textsDirectory, "*.xml");

            // Process each XML file.
            foreach (string xmlFile in xmlFiles)
            {
                ProcessXmlFile(xmlFile, audioDirectory, snapshotDirectory);
            }
            Console.WriteLine("Finished processing all files.");
        }

        /// <summary>
        /// Processes a single XML file, generating an audio file if necessary.
        /// </summary>
        /// <param name="xmlFile">The path to the XML file.</param>
        /// <param name="audioDirectory">The directory where audio files will be saved.</param>
        /// <param name="snapshotDirectory">The directory where snapshots will be saved.</param>
        static void ProcessXmlFile(string xmlFile, string audioDirectory, string snapshotDirectory)

        {
            try
            {
                string baseFileName = Path.GetFileNameWithoutExtension(xmlFile);
                string outputFileName = $"{baseFileName}.mp3";
                string outputFilePath = Path.Combine(audioDirectory, outputFileName);

                string? latestSnapshot = GetLatestSnapshot(xmlFile, snapshotDirectory);
                string currentHash = CalculateFileHash(xmlFile);

                bool needsAudioGeneration = true;

                if (latestSnapshot != null)
                {
                    string latestHash = CalculateFileHash(latestSnapshot);
                    if (currentHash == latestHash)
                    {
                        Console.WriteLine($"No changes detected in {xmlFile} since last snapshot.");

                        if (File.Exists(outputFilePath))
                        {
                            Console.WriteLine($"Audio file already exists for {xmlFile}. Skipping audio generation.");
                            needsAudioGeneration = false;
                        }
                        else
                        {
                            Console.WriteLine($"Audio file not found for {xmlFile}. Regenerating audio.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Changes detected in {xmlFile} since last snapshot.");
                        Console.WriteLine($"Creating new snapshot for {xmlFile}.");
                        CreateSnapshot(xmlFile, snapshotDirectory);
                    }
                }
                else
                {
                    Console.WriteLine($"No previous snapshots found for {xmlFile}.");
                    Console.WriteLine($"Creating new snapshot for {xmlFile}.");
                    CreateSnapshot(xmlFile, snapshotDirectory);
                }

                if (needsAudioGeneration)
                {
                    // Read the SSML content from the XML file.
                    string ssmlText = File.ReadAllText(xmlFile);

                    // Create the TextToSpeech client.
                    var client = TextToSpeechClient.Create();

                    // The input to be synthesized, now using the SSML text.
                    var input = new SynthesisInput
                    {
                        Ssml = ssmlText
                    };

                    // Build the voice request.
                    var voiceSelection = new VoiceSelectionParams
                    {
                        LanguageCode = "vi-VN",
                        Name = "vi-VN-Wavenet-A",
                        SsmlGender = SsmlVoiceGender.Female
                    };

                    // Specify the type of audio file.
                    var audioConfig = new AudioConfig
                    {
                        AudioEncoding = AudioEncoding.Mp3
                    };

                    // Perform the text-to-speech request.
                    var response = client.SynthesizeSpeech(input, voiceSelection, audioConfig);

                    // Write the response to the output file.
                    using (var output = File.Create(outputFilePath))
                    {
                        response.AudioContent.WriteTo(output);
                    }
                    Console.WriteLine($"Audio generated: {outputFilePath}");
                }

                // Remove the CWD from the paths for display.
                string relativeXmlPath = "texts/" + Path.GetFileName(xmlFile);
                string relativeOutputPath = "audio/" + outputFileName;

                Console.WriteLine($"Processed: '{relativeXmlPath}' -> '{relativeOutputPath}'");
            }
            catch (Exception ex)
            {
                // Remove the CWD from the path for display.
                string relativeXmlPath = "texts/" + Path.GetFileName(xmlFile);
                Console.WriteLine($"Error processing '{relativeXmlPath}': {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the path to the latest snapshot file for a given XML file.
        /// </summary>
        /// <param name="xmlFile">The path to the XML file.</param>
        /// <param name="snapshotDirectory">The directory where snapshots are stored.</param>
        /// <returns>The path to the latest snapshot file, or <c>null</c> if no snapshots exist.</returns>
        static string? GetLatestSnapshot(string xmlFile, string snapshotDirectory)
        {
            string xmlNameBase = Path.GetFileNameWithoutExtension(xmlFile);
            string? latestSnapshot = null;
            DateTime latestTimestamp = DateTime.MinValue;

            // Early exit if no files are found in the snapshot directory.
            if (!Directory.EnumerateFiles(snapshotDirectory).Any())
            {
                return null;
            }

            foreach (string filename in Directory.GetFiles(snapshotDirectory))
            {
                if (filename.StartsWith(Path.Combine(snapshotDirectory, xmlNameBase + "-")) && filename.EndsWith(".xml"))
                {
                    try
                    {
                        string timestampStr = Path.GetFileName(filename).Substring(xmlNameBase.Length + 1, 19);
                        DateTime timestamp = DateTime.ParseExact(timestampStr, "yyyy-MM-dd-HH-mm-ss", null);
                        if (timestamp > latestTimestamp)
                        {
                            latestTimestamp = timestamp;
                            latestSnapshot = filename;
                        }
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine($"Warning: Invalid snapshot filename format: {filename}");
                    }
                }
            }
            return latestSnapshot;
        }

        static string CalculateFileHash(string filePath)
        {
            using (var sha256 = SHA256.Create())
            {
                using var stream = File.OpenRead(filePath);
                byte[] hashBytes = sha256.ComputeHash(stream);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        static void CreateSnapshot(string xmlFile, string snapshotDirectory)
        {
            string xmlNameBase = Path.GetFileNameWithoutExtension(xmlFile);
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            string snapshotFilename = $"{xmlNameBase}-{timestamp}.xml";
            string snapshotPath = Path.Combine(snapshotDirectory, snapshotFilename);
            File.Copy(xmlFile, snapshotPath, true); //overwrite if exist
            Console.WriteLine($"Snapshot created: {snapshotPath}");
        }
    }
}
