using Google.Cloud.TextToSpeech.V1;
using System;

namespace TextToSpeechApiDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = TextToSpeechClient.Create();
            var response = client.ListVoices("vi");
            foreach (var voice in response.Voices)
            {
                Console.WriteLine($"{voice.Name} ({voice.SsmlGender}); Language codes: {string.Join(", ", voice.LanguageCodes)}");
            }
        }
    }
}
