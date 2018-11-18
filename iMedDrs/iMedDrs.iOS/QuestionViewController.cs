using Foundation;
using System;
using System.Drawing;
using System.Threading.Tasks;
using AVFoundation;
using Speech;
using UIKit;
using BigTed;

namespace iMedDrs.iOS
{
    public partial class QuestionViewController : UIViewController
    {
        public string baseurl { get; set; }
        public string userid { get; set; }
        public string username { get; set; }
        public string questionnaire { get; set; }
        public string number { get; set; }
        public string name { get; set; }
        public string text { get; set; }
        public string[] response { get; set; }
        public string last { get; set; }
        public string responses { get; set; }
        public string branches { get; set; }
        public bool required { get; set; }
        public string type { get; set; }
        public string answer { get; set; }
        public string[] eresponse { get; set; }
        public string datapath { get; set; }
        public string language { get; set; }
        public string extension { get; set; }
        public string instructions { get; set; }
        public int level { get; set; }
        public string email { get; set; }
        public bool handsfree { get; set; }
        private bool recook = false;
        private bool report = false;
        private string path;
        private string[] message;
        private string[] result;
        private UIAlertView alertView;
        private UIAlertView progressView;
        private UIAlertView stopView;
        private AVPlayer player;
        private SFSpeechAudioBufferRecognitionRequest recognitionRequest;
        private SFSpeechRecognitionTask recognitionTask;
        private AVAudioEngine audioEngine;
        private AVAudioSession audioSession;
        private SFSpeechRecognizer speechRecognizer;
        private MServer ms;
        private PServer ps;

        public QuestionViewController (IntPtr handle) : base (handle)
        {
            ps = new PServer();
            path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/";
            alertView = new UIAlertView();
            alertView.AddButton("Ok");
            progressView = new UIAlertView();
            progressView.Title = "Processing... Please Wait...";
            stopView = new UIAlertView();
            stopView.AddButton("No");
            stopView.AddButton("Yes");
            stopView.Title = "Stop the questionnaire?";
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, OnKeyboardNotification);
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, OnKeyboardNotification);
            audioEngine = new AVAudioEngine();
            speechRecognizer = new SFSpeechRecognizer(new NSLocale("en_US"));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            if (UIScreen.MainScreen.Bounds.Width == 320)
            {
                SetWidth(line6Txt, 320.0f, 0.0f);
                SetWidth(instructionsLbl, 300.0f, 0.0f);
                SetWidth(speakBtn, 46.0f, 270.0f);
                SetWidth(questionLbl, 320.0f, 0.0f);
                SetWidth(questionBtn, 300.0f, 0.0f);
                SetWidth(responseTxt, 300.0f, 0.0f);
                SetWidth(previousBtn, 145.0f, 0.0f);
                SetWidth(nextBtn, 145.0f, 165.0f);
                SetWidth(returnBtn, 300.0f, 0.0f);
            }
            responseTxt.ShouldReturn += TextFieldShouldReturn;
            var tap = new UITapGestureRecognizer { CancelsTouchesInView = false };
            tap.AddTarget(() => View.EndEditing(true));
            View.AddGestureRecognizer(tap);
            questionBtn.TitleLabel.LineBreakMode = UILineBreakMode.WordWrap;
            voiceBtn.Hidden = handsfree;
            speakBtn.Hidden = handsfree;
            SFSpeechRecognizer.RequestAuthorization((SFSpeechRecognizerAuthorizationStatus status) =>
            {
                if (status == SFSpeechRecognizerAuthorizationStatus.Authorized)
                {
                    var node = audioEngine.InputNode;
                    var recordingFormat = node.GetBusOutputFormat(0);
                    node.InstallTapOnBus(0, 1024, recordingFormat, (AVAudioPcmBuffer buffer, AVAudioTime when) => {
                        recognitionRequest.Append(buffer);
                    });
                    recook = true;
                }
                else
                    speakBtn.Hidden = true;
            });
            ms = new MServer(baseurl);
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            namequestLbl.Text = username + " - " + questionnaire;
            SetAudioSettings();
            SetQuestion();
            SetResponses();
        }

        private bool TextFieldShouldReturn(UITextField textfield)
        {
            textfield.ResignFirstResponder();
            return false;
        }

        partial void VoiceBtn_TouchUpInside(UIButton sender)
        {
            Play();
        }

        partial void SpeakBtn_TouchUpInside(UIButton sender)
        {
            if (recook)
            {
                namelbl.Text = "Speak now...";
                Recognize();
            }
        }

        partial void NextBtn_TouchUpInside(UIButton sender)
        {
            StopVoiceReco();
            Next();
        }

        partial void PreviousBtn_TouchUpInside(UIButton sender)
        {
            StopVoiceReco();
            Previous();
        }

        partial void ReturnBtn_TouchUpInside(UIButton sender)
        {
            stopView.Show();
            stopView.Clicked += (object senders, UIButtonEventArgs e) => 
            {
                if (e.ButtonIndex == 1)
                {
                    StopVoiceReco();
                    handsfree = false;
                    recook = false;
                    audioEngine.Dispose();
                    speechRecognizer.Dispose();
                    DismissModalViewController(false);
                }
            };
        }

        private async void Play()
        {
            if (player != null)
            {
                player.Dispose();
                player = null;
            }
            recoLbl.Text = "";
            var url = NSUrl.FromString("https://imeddrs.com/audio/" + language + "/" + name + extension);
            player = AVPlayer.FromUrl(url);
            if (player != null)
            {
                player.Volume = 1.0f;
                do { await Task.Delay(5); } while (player.Status != AVPlayerStatus.ReadyToPlay);
                player.Play();
                player.ActionAtItemEnd = AVPlayerActionAtItemEnd.None;
                NSNotificationCenter.DefaultCenter.AddObserver(AVPlayerItem.DidPlayToEndTimeNotification, (notify) =>
                {
                    InvokeOnMainThread(() =>
                    {
                        recoLbl.Text = "complete";
                    });
                });
                if (handsfree)
                { 
                    do { await Task.Delay(5); } while (recoLbl.Text == "");
                    if (recoLbl.Text != "")
                    {
                        recoLbl.Text = "";
                        if (!report)
                        {
                            if (recook)
                            {
                                InvokeOnMainThread(() =>
                                {
                                    namelbl.Text = "Speak now...";
                                });
                                Recognize();
                            }
                        }
                        else
                            Next();
                    }
                }
            }
        }

        private async void Recognize()
        {
            recoLbl.Text = "";
            GetRecognition();
            if (handsfree)
                do { await Task.Delay(5); } while (recoLbl.Text == "");
            switch (recoLbl.Text)
            {
                case "previous":
                    Previous();
                    break;
                case "next":
                    Next();
                    break;
                case "stop":
                    handsfree = false;
                    recook = false;
                    audioEngine.Stop();
                    recognitionTask.Cancel();
                    DismissModalViewController(false);
                    break;
                default:
                    SetResponses();
                    break;
            }
        }

        private void GetRecognition()
        {
            recognitionRequest = new SFSpeechAudioBufferRecognitionRequest();
            audioEngine.Prepare();
            audioEngine.StartAndReturnError(out NSError error);
            if (error != null)
                return;
            recognitionTask = speechRecognizer.GetRecognitionTask(recognitionRequest, (SFSpeechRecognitionResult result, NSError err) => {
                if (err == null)
                {
                    if (result.Final)
                    {
                        string recresp = result.BestTranscription.FormattedString;
                        if (recresp.ToLower() != "previous" && recresp.ToLower() != "next" && recresp.ToLower() != "stop")
                        {
                            if (recresp.ToLower() == "mail" && name == "Gender")
                                recresp = "male";
                            if (response.Length == 1)
                            {
                                if (type == "number")
                                {
                                    try
                                    {
                                        responseTxt.Text = Convert.ToInt32(recresp).ToString();
                                        recresp = "next";
                                    }
                                    catch { responseTxt.Text = ""; }
                                }
                                else
                                {
                                    responseTxt.Text = recresp;
                                    recresp = "next";
                                }
                            }
                            else
                            {
                                for (int i = 0; i < response.Length; i++)
                                {
                                    if (response[i].ToLower() == recresp.ToLower())
                                    {
                                        responseSmc.SelectedSegment = i;
                                        recresp = "next";
                                    }
                                }
                            }
                        }
                        if (handsfree)
                        {
                            if (recresp == "")
                                recoLbl.Text = "error";
                            else
                                recoLbl.Text = recresp;
                        }
                    }
                }
                else
                {
                    if (handsfree)
                        recoLbl.Text = "error";
                }
                audioEngine.Stop();
                recognitionRequest.EndAudio();
                namelbl.Text = name;
            });
        }

        private void StopVoiceReco()
        {
            namelbl.Text = name;
            if (player != null)
            {
                player.Dispose();
                player = null;
            }
            audioEngine.Stop();
            if (recognitionTask != null)
                recognitionTask.Cancel();
        }

        private async void Next()
        {
            if (Convert.ToInt32(number) < Convert.ToInt32(last))
            {
                string ans = GetRepsonse();
                if (required && ans == "~")
                    AlertMessage("A response is required");
                else
                {
                    BTProgressHUD.Show("Processing...Please wait...");
                    message = new string[] { "questionnaire", "next", userid, questionnaire, number, ans, responses, branches };
                    await Task.Run(() => result = ms.ProcessMessage(message, "GET"));
                    BTProgressHUD.Dismiss();
                    if (result[1] == "ack")
                    {
                        number = result[2];
                        name = result[3];
                        text = result[4];
                        response = result[5].Split(',');
                        last = result[6];
                        responses = result[7];
                        branches = result[8];
                        required = Convert.ToBoolean(result[9]);
                        type = result[10];
                        answer = result[11];
                        eresponse = result[12].Split(',');
                        extension = result[13];
                        instructions = result[14];
                        SetQuestion();
                        SetResponses();
                    }
                    else
                        AlertMessage(result[2]);
                }
            }
            else
            {
                if (level > 1)
                {
                    BTProgressHUD.Show("Processing...Please wait...");
                    message = new string[] { "questionnaire", "save", userid, questionnaire, responses };
                    await Task.Run(() => result = ms.ProcessMessage(message, "GET"));
                    BTProgressHUD.Dismiss();
                    if (!handsfree)
                        AlertMessage(result[2]);
                }
                else
                {
                    string file = path + questionnaire.Replace(" ", "_") + ".txt";
                    if (ps.WriteToFile(file, responses, true))
                    {
                        if (!handsfree)
                            AlertMessage("Questionnaire responses saved");
                    }
                    else
                    {
                        if (!handsfree)
                            AlertMessage("Not able to save resposnes");
                    }
                }
                DismissModalViewController(false);
            }
        }

        private async void Previous()
        {
            BTProgressHUD.Show("Processing...Please wait...");
            message = new string[] { "questionnaire", "previous", userid, questionnaire, number, responses, branches };
            await Task.Run(() => result = ms.ProcessMessage(message, "GET"));
            BTProgressHUD.Dismiss();
            if (result[1] == "ack")
            {
                number = result[2];
                name = result[3];
                text = result[4];
                response = result[5].Split(',');
                last = result[6];
                responses = result[7];
                branches = result[8];
                required = Convert.ToBoolean(result[9]);
                type = result[10];
                answer = result[11];
                eresponse = result[12].Split(',');
                extension = result[13];
                instructions = result[14];
                SetQuestion();
                SetResponses();
            }
            else
                AlertMessage(result[2]);
        }

        private void SetQuestion()
        {
            instructionsLbl.Text = instructions.Replace("~", "\r\n");
            namelbl.Text = name;
            questionBtn.SetTitle(text, UIControlState.Normal);
            if (name == "End")
                report = true;
        }

        private void SetResponses()
        {
            responseTxt.Hidden = true;
            responseSmc.Hidden = true;
            if (name != "End")
            {
                if (response.Length == 1)
                {
                    responseTxt.Text = answer;
                    responseTxt.Hidden = false;
                    if (type == "number")
                        responseTxt.KeyboardType = UIKeyboardType.NumbersAndPunctuation;
                    else
                        responseTxt.KeyboardType = UIKeyboardType.Default;
                }
                else
                {
                    responseSmc.RemoveAllSegments();
                    responseSmc.Frame = new CoreGraphics.CGRect(responseSmc.Frame.X, responseSmc.Frame.Y, 150f * response.Length, responseSmc.Frame.Height);
                    for (int i = 0; i < response.Length; i++)
                    {
                        responseSmc.InsertSegment(response[i], i, true);
                        responseSmc.SetEnabled(true, i);
                        responseSmc.SetWidth(100f, i);
                        if (eresponse[i] == answer)
                            responseSmc.SelectedSegment = i;
                    }
                    responseSmc.Hidden = false;
                }
            }
            if (handsfree)
                Play();
        }

        private string GetRepsonse()
        {
            string result = "";
            if (name != "End")
            {
                if (response.Length == 1)
                {
                    if (type == "number")
                    {
                        if (name == "age" && (Convert.ToInt32(responseTxt.Text) < 1 || Convert.ToInt32(responseTxt.Text) > 150))
                            responseTxt.Text = "";
                        result = responseTxt.Text;
                    }
                    else
                        result = responseTxt.Text;
                }
                else
                {
                    if (responseSmc.SelectedSegment > -1)
                        result = eresponse[responseSmc.SelectedSegment];
                }
            }
            if (result == "")
                result = "~";
            return result;
        }

        private void AlertMessage(string title)
        {
            if (title != "")
            {
                alertView.Title = title;
                alertView.Show();
            }
        }

        private void SetAudioSettings()
        {
            audioSession = AVAudioSession.SharedInstance();
            NSError error = audioSession.SetCategory(AVAudioSessionCategory.PlayAndRecord, AVAudioSessionCategoryOptions.DefaultToSpeaker);
            if (error == null)
            {
                if (audioSession.SetMode(AVAudioSession.ModeVideoChat, out error))
                {
                    if (audioSession.OverrideOutputAudioPort(AVAudioSessionPortOverride.Speaker, out error))
                        error = audioSession.SetActive(true);
                }
            }
        }

        private void SetWidth(UIView view, float width, float posx)
        {
            CoreGraphics.CGRect frame = view.Frame;
            frame.Width = width;
            if (posx > 0)
                frame.X = posx;
            view.Frame = frame;
        }

        private void OnKeyboardNotification(NSNotification notification)
        {
            if (!IsViewLoaded) return;
            var visible = notification.Name == UIKeyboard.WillShowNotification;
            UIView.BeginAnimations("AnimateForKeyboard");
            UIView.SetAnimationBeginsFromCurrentState(true);
            UIView.SetAnimationDuration(UIKeyboard.AnimationDurationFromNotification(notification));
            UIView.SetAnimationCurve((UIViewAnimationCurve)UIKeyboard.AnimationCurveFromNotification(notification));
            bool landscape = InterfaceOrientation == UIInterfaceOrientation.LandscapeLeft || InterfaceOrientation == UIInterfaceOrientation.LandscapeRight;
            var keyboardFrame = visible ? UIKeyboard.FrameEndFromNotification(notification) : UIKeyboard.FrameBeginFromNotification(notification);
            OnKeyboardChanged(visible, landscape ? keyboardFrame.Width : keyboardFrame.Height);
            UIView.CommitAnimations();
        }

        protected virtual void OnKeyboardChanged(bool visible, nfloat keyboardHeight)
        {
            bool restore = false;
            UIView firstResponder = null;
            if (responseTxt.IsFirstResponder)
                firstResponder = responseTxt;
            else
                restore = true;
            var activeView = scrollView ?? firstResponder;
            if (activeView == null)
                return;
            if (!visible || restore)
                RestoreScrollPosition(scrollView);
            else
                CenterViewInScroll(activeView, scrollView, (float)keyboardHeight);
        }

        protected virtual void CenterViewInScroll(UIView viewToCenter, UIScrollView scrollView, float keyboardHeight)
        {
            var contentInsets = new UIEdgeInsets(0.0f, 0.0f, keyboardHeight, 0.0f);
            scrollView.ContentInset = contentInsets;
            scrollView.ScrollIndicatorInsets = contentInsets;

            // Position of the active field relative isnside the scroll view
            RectangleF relativeFrame = (RectangleF)viewToCenter.Superview.ConvertRectToView(viewToCenter.Frame, scrollView);

            bool landscape = InterfaceOrientation == UIInterfaceOrientation.LandscapeLeft || InterfaceOrientation == UIInterfaceOrientation.LandscapeRight;
            var spaceAboveKeyboard = (landscape ? scrollView.Frame.Width : scrollView.Frame.Height) - keyboardHeight;

            // Move the active field to the center of the available space
            var offset = relativeFrame.Y - (spaceAboveKeyboard - viewToCenter.Frame.Height) / 2;
            scrollView.ContentOffset = new PointF(0, (float)offset);
        }

        protected virtual void RestoreScrollPosition(UIScrollView scrollView)
        {
            scrollView.ContentInset = UIEdgeInsets.Zero;
            scrollView.ScrollIndicatorInsets = UIEdgeInsets.Zero;
        }
    }
}