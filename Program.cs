﻿using Google.Cloud.TextToSpeech.V1;
using System.Security.Cryptography;
using System.Text.Json;

namespace TextToSpeechApiDemo
{
    class Program
    {
        // Cache the JsonSerializerOptions instance
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

        /// <summary>
        /// The main entry point of the program.
        /// </summary>
        /// <param name="args">Command-line arguments (not used).</param>
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome.\nThe program will read all the files under `texts/` and \nconvert to audio files and place under `audio/`.\n");

            // Directory for the SSML text files.
            string textsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "texts");

            // Directory for the output audio files.
            string audioDirectory = Path.Combine(Directory.GetCurrentDirectory(), "audio");

            // Directory for snapshots
            string snapshotDirectory = Path.Combine(Directory.GetCurrentDirectory(), "snapshots");

            // Directory for configs
            string configDirectory = Path.Combine(Directory.GetCurrentDirectory(), "configs");

            // Create the "audio", "snapshots" and "configs" directories if they don't exist.
            Directory.CreateDirectory(audioDirectory);
            Directory.CreateDirectory(snapshotDirectory);
            Directory.CreateDirectory(configDirectory);

            // Check if the texts directory exists.
            if (!Directory.Exists(textsDirectory))
            {
                Console.WriteLine($"Error: Texts directory not found at '{textsDirectory}'.");
                return;
            }

            // Load the TextToSpeech configuration from config.json
            TextToSpeechConfig config = LoadTextToSpeechConfig(configDirectory);

            // Get all .xml files in the texts directory.
            string[] xmlFiles = Directory.GetFiles(textsDirectory, "*.xml");

            // Process each XML file.
            foreach (string xmlFile in xmlFiles)
            {
                ProcessXmlFile(xmlFile, audioDirectory, snapshotDirectory, config);
            }
            Console.WriteLine("Finished processing all files.");
        }

        /// <summary>
        /// Processes a single XML file, generating an audio file if necessary.
        /// </summary>
        /// <param name="xmlFile">The path to the XML file.</param>
        /// <param name="audioDirectory">The directory where audio files will be saved.</param>
        /// <param name="snapshotDirectory">The directory where snapshots will be saved.</param>
        /// <param name="config">The TextToSpeech configuration.</param>
        static void ProcessXmlFile(string xmlFile, string audioDirectory, string snapshotDirectory, TextToSpeechConfig config)

        {
            string fileExtension;
            Console.WriteLine($"Processing: {Path.GetFileName(xmlFile)}");

            // Use a switch statement to determine the file extension based on the AudioEncoding enum.
            switch (config.AudioEncoding)
            {
                case AudioEncoding.Mp3:
                    fileExtension = ".mp3";
                    break;
                case AudioEncoding.OggOpus:
                    fileExtension = ".ogg";
                    break;
                default:
                    Console.WriteLine($"Warning: Unsupported audio encoding '{config.AudioEncoding}'. Defaulting to .mp3");
                    fileExtension = ".mp3";
                    break;
            }

            try
            {
                string baseFileName = Path.GetFileNameWithoutExtension(xmlFile);
                string outputFileName = $"{baseFileName}{fileExtension}";
                string outputFilePath = Path.Combine(audioDirectory, outputFileName);

                string? latestSnapshot = GetLatestSnapshot(xmlFile, snapshotDirectory);
                string currentHash = CalculateFileHash(xmlFile);

                bool needsAudioGeneration = true;

                if (latestSnapshot != null)
                {
                    string latestHash = CalculateFileHash(latestSnapshot);
                    if (currentHash == latestHash)
                    {
                        Console.WriteLine($"No changes detected in {Path.GetFileName(xmlFile)} since last snapshot.");

                        if (File.Exists(outputFilePath))
                        {
                            Console.WriteLine($"Audio file already exists for {Path.GetFileName(xmlFile)}. Skipping audio generation.");
                            needsAudioGeneration = false;
                        }
                        else
                        {
                            Console.WriteLine($"Audio file not found for {Path.GetFileName(xmlFile)}. Regenerating audio.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Changes detected in {Path.GetFileName(xmlFile)} since last snapshot.");
                        Console.WriteLine($"Creating new snapshot for {Path.GetFileName(xmlFile)}.");
                        CreateSnapshot(xmlFile, snapshotDirectory);
                    }
                }
                else
                {
                    // Remove the CWD from the paths for display.
                    Console.WriteLine($"No previous snapshots found for {Path.GetFileName(xmlFile)}.");
                    Console.WriteLine($"Creating new snapshot for {Path.GetFileName(xmlFile)}.");
                    CreateSnapshot(xmlFile, snapshotDirectory);
                }

                if (needsAudioGeneration)
                {
                    GenerateAudio(xmlFile, outputFilePath, config);
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
        /// Generates an audio file from an SSML file.
        /// </summary>
        /// <param name="xmlFile">The path to the XML file containing SSML.</param>
        /// <param name="outputFilePath">The path to save the generated audio file.</param>
        /// <param name="config">The TextToSpeech configuration.</param>
        static void GenerateAudio(string xmlFile, string outputFilePath, TextToSpeechConfig config)
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
                LanguageCode = config.LanguageCode,
                Name = config.VoiceName,
                SsmlGender = config.SsmlGender
            };

            // Specify the type of audio file.
            var audioConfig = new AudioConfig
            {
                AudioEncoding = config.AudioEncoding
            };

            // Perform the text-to-speech request.
            var response = client.SynthesizeSpeech(input, voiceSelection, audioConfig);

            // Write the response to the output file.
            using (var output = File.Create(outputFilePath))
            {
                response.AudioContent.WriteTo(output);
            }
            Console.WriteLine($"Audio generated: {Path.GetFileName(outputFilePath)}");
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

        /// <summary>
        /// Calculates the SHA-256 hash of a file.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <returns>The SHA-256 hash of the file.</returns>
        static string CalculateFileHash(string filePath)
        {
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            byte[] hashBytes = sha256.ComputeHash(stream);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        /// <summary>
        /// Creates a new snapshot file for a given XML file.
        /// </summary>
        /// <param name="xmlFile">The path to the XML file.</param>
        static void CreateSnapshot(string xmlFile, string snapshotDirectory)
        {
            string xmlNameBase = Path.GetFileNameWithoutExtension(xmlFile);
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            string snapshotFilename = $"{xmlNameBase}-{timestamp}.xml";
            string snapshotPath = Path.Combine(snapshotDirectory, snapshotFilename);
            File.Copy(xmlFile, snapshotPath, true); //overwrite if exist
            Console.WriteLine($"Snapshot created: snapshots/{snapshotFilename}");
        }

        /// <summary>
        /// Loads the TextToSpeech configuration from config.json.
        /// </summary>
        /// <param name="configDirectory">The directory where config.json is located.</param>
        /// <returns>The TextToSpeech configuration.</returns>
        static TextToSpeechConfig LoadTextToSpeechConfig(string configDirectory)
        {
            string configFilePath = Path.Combine(configDirectory, "config.json");

            // Create default config file if not exist
            if (!File.Exists(configFilePath))
            {
                Console.WriteLine($"Config file not found at '{configFilePath}'. Creating a default one.");
                var defaultConfig = new TextToSpeechConfig();
                // Use the cached instance here
                string jsonString = JsonSerializer.Serialize(defaultConfig, _jsonSerializerOptions);
                File.WriteAllText(configFilePath, jsonString);
            }

            try
            {
                string jsonString = File.ReadAllText(configFilePath);
                return JsonSerializer.Deserialize<TextToSpeechConfig>(jsonString, _jsonSerializerOptions) ?? new TextToSpeechConfig();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading or parsing config file: {ex.Message}");
                Console.WriteLine("Using default configuration.");
                return new TextToSpeechConfig();
            }
        }

        /// <summary>
        /// The TextToSpeech configuration.
        /// </summary>
        public class TextToSpeechConfig
        {
            public string LanguageCode { get; set; } = "vi-VN";
            public string VoiceName { get; set; } = "vi-VN-Wavenet-A";
            public SsmlVoiceGender SsmlGender { get; set; } = SsmlVoiceGender.Female;
            // Use OggOpus for better quality with a smaller size even at the same bitrate to mp3.
            public AudioEncoding AudioEncoding { get; set; } = AudioEncoding.OggOpus;
        }

    }
}
