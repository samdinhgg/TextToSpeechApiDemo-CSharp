# TextToSpeechApiDemo-CSharp

- A simple program to demo TTS APIs with GCP in CSharp.

## How to start this Demo with Cloud Shell

This demo is designed to be easily run and tested within Google Cloud Shell, which provides a pre-configured environment with the necessary tools and authentication. Here's how to get started:

1.  **Open Google Cloud Shell:**
    * Go to [shell.cloud.google.com](https://shell.cloud.google.com/) in your web browser.
    * Sign in with your Google Cloud account.
    * Ensure to select your project from the Cloud Shell terminal tab.

2.  **Clone the Repository:**
    * Once Cloud Shell is open, use the `git clone` command to clone this repository:
        ```bash
        git clone https://github.com/samdinhgg/TextToSpeechApiDemo-CSharp.git)
        ```

3.  **Navigate to the Project Directory:**
    * Use the `cd` command to navigate into the project directory:
        ```bash
        cd TextToSpeechApiDemo-CSharp
        ```

4.  **Restore Dependencies:**
    * Run the following command to restore the project's dependencies:
        ```bash
        dotnet restore
        ```

5.  **Build the Project:**
    * Build the project using the following command:
        ```bash
        dotnet build
        ```

6.  **Run the Demo:**
    * Execute the demo application:
        ```bash
        dotnet run
        ```
    * If you have multiple projects, you might need to specify the project name:
        ```bash
        dotnet run --project TextToSpeechApiDemo
        ```

7. **Interact with the application:**
    * The application will run and you can interact with it. The application will use the default credential of the Cloud Shell.

**Why Cloud Shell and Important Notes**

* **Authentication:** Cloud Shell automatically handles authentication with your Google Cloud account, so you don't need to set up any credentials manually.
* **GCP Project:** Ensure that you have selected the correct GCP project in Cloud Shell that has the Cloud Text-to-Speech API enabled.
* **API Enablement:** Make sure the Cloud Text-to-Speech API is enabled for your project. You can enable it in the Google Cloud Console.
* **SSML:** You can modify the SSML text in the `texts` folder to test different scenarios.
* **Cost:** Using Cloud Shell is free, but using the Text-to-Speech API might incur costs depending on your usage. Please refer to the GCP pricing page.

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
