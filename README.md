# TextToSpeechApiDemo-CSharp

- A simple program to demo TTS APIs with GCP in CSharp.

## A sample SSML text

- Please refer to the files within `texts` folder for more examples.

```xml
<speak>
  <prosody rate="medium">
    Chào bạn đến với <emphasis level="strong">Trường Tiểu Học Nghĩa Đô</emphasis>.
  </prosody>
  <break time="500ms"/>
  <prosody rate="medium">
    Vui lòng cho biết <emphasis>thông tin cá nhân</emphasis> của bạn, bao gồm:
    <break time="200ms"/>
    Họ, <break time="150ms"/>
    Tên, <break time="150ms"/>
    Số điện thoại, <break time="150ms"/>
    và <emphasis>lý do</emphasis> vào cổng trường.
  </prosody>
  <break time="400ms"/>
  <prosody rate="medium">
    <emphasis>Xin cảm ơn</emphasis>.
  </prosody>
</speak>

```
