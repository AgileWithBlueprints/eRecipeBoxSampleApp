/*
* MIT License
* 
* Copyright (C) 2024 SoftArc, LLC
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/
using Microsoft.CognitiveServices.Speech;
using System;
using System.Threading.Tasks;
namespace SimpleCRUDFramework
{
    public delegate void SpeechRecognitionErrorHandler(string errorMessage);

    //Note:
    //system.speech is terrible.  dont use it
    //https://stackoverflow.com/questions/67188111/speechrecogntion-quality-is-extremely-poor-especially-compared-to-word

    //eRecipeBox used this instead
    //https://www.nuget.org/packages/Microsoft.CognitiveServices.Speech

    //azure setup to get your key
    //https://learn.microsoft.com/en-us/azure/ai-services/speech-service/quickstarts/setup-platform?tabs=windows%2Cubuntu%2Cdotnetcli%2Cdotnet%2Cjre%2Cmaven%2Cbrowser%2Cmac%2Cpypi&pivots=programming-language-csharp
    //https://learn.microsoft.com/en-us/azure/ai-services/speech-service/get-started-speech-to-text?tabs=windows%2Cterminal&pivots=programming-language-csharp

    public class MemoEditWithSpeech : DevExpress.XtraEditors.MemoEdit
    {

        private string azureSubscriptionKey = null;
        private string azureSubscriptionRegion = null;
        public void RegisterAzureSubscriptionKey(string azureSubscriptionKey, string azureSubscriptionRegion)
        {
            if (string.IsNullOrEmpty(azureSubscriptionKey) || string.IsNullOrEmpty(azureSubscriptionRegion))
                throw new Exception("Logic error: azureSubscriptionKey and azureSubscriptionRegion are both required");
            this.azureSubscriptionKey = azureSubscriptionKey;
            this.azureSubscriptionRegion = azureSubscriptionRegion;
        }


        public event SpeechRecognitionErrorHandler SpeechRecognitionError;

        private void RaiseErrorEvent(string errorMessage)
        {
            SpeechRecognitionError?.Invoke(errorMessage);
        }

        private bool speechRecognitionIsRunning = false;
        public bool SpeechRecognitionIsRunning
        {
            get { return speechRecognitionIsRunning; }
            private set { speechRecognitionIsRunning = value; }
        }

        public string SpeechRecognitionErrorMessage { get; private set; }

        private SpeechRecognizer recognizer;
        public MemoEditWithSpeech() : base()
        {
            Leave += MemoEditWithSpeech_Leave;
            Enter += MemoEditWithSpeech_Enter;
        }

        private async Task StopSpeechRecognizer()
        {
            speechRecognitionIsRunning = false;
            if (recognizer != null)
            {
                await recognizer.StopContinuousRecognitionAsync();
                recognizer.Recognized -= Recognizer_Recognized;
                recognizer.Canceled -= Recognizer_Canceled;
                recognizer = null;
            }
        }

        private void MemoEditWithSpeech_Enter(object sender, EventArgs e)
        {
            if (azureSubscriptionKey != null)
                InitializeSpeechRecognizer(azureSubscriptionKey, azureSubscriptionRegion);
        }

        private async void MemoEditWithSpeech_Leave(object sender, EventArgs e)
        {
            await StopSpeechRecognizer();
        }


        private async void InitializeSpeechRecognizer(string azureSubscriptionKey, string azureSubscriptionRegion)
        {
            SpeechRecognitionErrorMessage = null;

            try
            {
                SpeechConfig config = SpeechConfig.FromSubscription(azureSubscriptionKey, azureSubscriptionRegion);
                recognizer = new SpeechRecognizer(config);

                recognizer.Recognized += Recognizer_Recognized;
                recognizer.Canceled += Recognizer_Canceled;

                await recognizer.StartContinuousRecognitionAsync();
                SpeechRecognitionIsRunning = true;
            }
            catch (Exception ex)
            {
                SpeechRecognitionErrorMessage = ex.Message;
                SpeechRecognitionIsRunning = false;
                RaiseErrorEvent(ex.Message);
            }
        }

        private void Recognizer_Recognized(object sender, SpeechRecognitionEventArgs e)
        {
            //#WORKAROUND Success with no text
            if (e.Result.Reason == ResultReason.RecognizedSpeech && string.IsNullOrEmpty(e.Result.Text))
                return;
            try
            {
                this.Invoke(new Action(() =>
                {
                    Text += e.Result.Text + "\r\n";
                    //place cursor at end
                    SelectionStart = Text.Length;
                }));
                SpeechRecognitionErrorMessage = null;
                SpeechRecognitionIsRunning = true;
            }
            catch (Exception)
            {
                //#WORKAROUND Even though i see we stop and tear down the recognizer, this event gets fired
                //with a disposed memoEdit. But checking IsDisposed doesnt event work
                //So just eat the Exception
            }
        }

        private void Recognizer_Canceled(object sender, SpeechRecognitionCanceledEventArgs e)
        {
            if (e.ErrorCode != CancellationErrorCode.NoError)
            {
                SpeechRecognitionErrorMessage = $"Reason: {e.Reason}\nErrorDetails: {e.ErrorDetails}";
                RaiseErrorEvent(SpeechRecognitionErrorMessage);
                SpeechRecognitionIsRunning = false;
            }
        }
    }
}
