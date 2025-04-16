using Google.Cloud.TextToSpeech.V1;
using System;
using System.IO;

namespace TextToSpeechApiDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = TextToSpeechClient.Create();

            // The input to be synthesized, can be provided as text or SSML.
            var input = new SynthesisInput
            {
                Text = "Xin chào. Vui lòng cung cấp thông tin cá nhân, bao gồm Họ, Tên và Số điện thoại trước khi vào cổng trường."
            };

            // Build the voice request.
            var voiceSelection = new VoiceSelectionParams
            {
                LanguageCode = "vi-VN",
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
            using (var output = File.Create("output.mp3"))
            {
                response.AudioContent.WriteTo(output);
            }
            Console.WriteLine("Audio content written to file \"output.mp3\"");
        }
    }
}
