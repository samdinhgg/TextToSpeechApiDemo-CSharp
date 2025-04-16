using Google.Cloud.TextToSpeech.V1;

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

            // Create the "audio" directory if it doesn't exist.
            Directory.CreateDirectory(audioDirectory);

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
                try
                {
                    // Read the SSML content from the XML file.
                    string ssmlText = File.ReadAllText(xmlFile);

                    // Extract the file name without the extension.
                    string baseFileName = Path.GetFileNameWithoutExtension(xmlFile);

                    // Create the output MP3 file name.
                    string outputFileName = $"{baseFileName}.mp3";

                    // Create the full output path.
                    string outputFilePath = Path.Combine(audioDirectory, outputFileName);

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
            Console.WriteLine("Finished processing all files.");
        }
    }
}
