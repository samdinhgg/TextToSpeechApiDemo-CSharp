using Google.Cloud.TextToSpeech.V1;
using System;
using System.IO;

namespace TextToSpeechApiDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            // The SSML text.
            var ssmlText = @"
<speak>
  <prosody rate=""medium"">
    Chào bạn, <break time=""300ms""/> đến với <emphasis level=""strong"">Trường Tiểu Học Nghĩa Đô</emphasis>.
  </prosody>
  <break time=""500ms""/>
  <prosody rate=""medium"">
    Vui lòng cho biết <emphasis>thông tin cá nhân</emphasis> của bạn, bao gồm:
    <break time=""200ms""/>
    Họ, <break time=""150ms""/>
    Tên, <break time=""150ms""/>
    Số điện thoại, <break time=""150ms""/>
    và <emphasis>lý do</emphasis> vào cổng trường.
  </prosody>
  <break time=""400ms""/>
  <prosody rate=""medium"">
    <emphasis>Xin cảm ơn</emphasis>.
  </prosody>
</speak>";

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
                SsmlGender = SsmlVoiceGender.Female
            };

            // Specify the type of audio file.
            var audioConfig = new AudioConfig
            {
                AudioEncoding = AudioEncoding.Mp3
            };

            // Perform the text-to-speech request.
            var response = client.SynthesizeSpeech(input, voiceSelection, audioConfig);

            // Generate the dynamic file name.
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            string outputFileName = $"{timestamp}.mp3";

            // Write the response to the output file with the dynamic name.
            using (var output = File.Create(outputFileName))
            {
                response.AudioContent.WriteTo(output);
            }
            Console.WriteLine("Audio content written to file \"output.mp3\"");
        }
    }
}
