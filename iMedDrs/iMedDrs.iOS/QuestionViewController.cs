using Foundation;
using System;
using System.Collections.Generic;
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
        public string Baseurl { get; set; }
        public string Userid { get; set; }
        public string Username { get; set; }
        public string Questionnaire { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public string[] Response { get; set; }
        public string Last { get; set; }
        public string[] Responses { get; set; }
        public string Branches { get; set; }
        public bool Required { get; set; }
        public string Type { get; set; }
        public string Answer { get; set; }
        public string[] Eresponse { get; set; }
        public string Datapath { get; set; }
        public string Language { get; set; }
        public string Extension { get; set; }
        public string Instructions { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public bool Handsfree { get; set; }
        public List<string> Presponses { get; set; }
        public UILabel SelectedLbl;
        private bool recook = false;
        private bool report = false;
        private readonly string path;
        private string[] message;
        private string[] result;
        private readonly UIAlertView alertView;
        private readonly UIAlertView stopView;
        private AVPlayer player;
        private SFSpeechAudioBufferRecognitionRequest recognitionRequest;
        private SFSpeechRecognitionTask recognitionTask;
        private readonly AVAudioEngine audioEngine;
        private AVAudioSession audioSession;
        private readonly SFSpeechRecognizer speechRecognizer;
        private MServer ms;
        private readonly PServer ps;

        public QuestionViewController (IntPtr handle) : base (handle)
        {
            ps = new PServer();
            Presponses = new List<string>();
            path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/";
            SelectedLbl = new UILabel();
            alertView = new UIAlertView();
            alertView.AddButton("Ok");
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
            responseTxt.ShouldReturn += TextFieldShouldReturn;
            var tap = new UITapGestureRecognizer { CancelsTouchesInView = false };
            tap.AddTarget(() => View.EndEditing(true));
            View.AddGestureRecognizer(tap);
            questionBtn.TitleLabel.LineBreakMode = UILineBreakMode.WordWrap;
            voiceBtn.Hidden = Handsfree;
            speakBtn.Hidden = Handsfree;
            if (SFSpeechRecognizer.AuthorizationStatus == SFSpeechRecognizerAuthorizationStatus.NotDetermined)
            {
                SFSpeechRecognizer.RequestAuthorization((SFSpeechRecognizerAuthorizationStatus status) =>
                {
                    if (status == SFSpeechRecognizerAuthorizationStatus.Authorized)
                    {
                        var node = audioEngine.InputNode;
                        var recordingFormat = node.GetBusOutputFormat(0);
                        node.InstallTapOnBus(0, 1024, recordingFormat, (AVAudioPcmBuffer buffer, AVAudioTime when) =>
                        {
                            recognitionRequest.Append(buffer);
                        });
                        recook = true;
                    }
                    else
                        speakBtn.Hidden = true;
                });
            }
            else
            {
                if (SFSpeechRecognizer.AuthorizationStatus == SFSpeechRecognizerAuthorizationStatus.Authorized)
                {
                    var node = audioEngine.InputNode;
                    var recordingFormat = node.GetBusOutputFormat(0);
                    node.InstallTapOnBus(0, 1024, recordingFormat, (AVAudioPcmBuffer buffer, AVAudioTime when) =>
                    {
                        recognitionRequest.Append(buffer);
                    });
                    recook = true;
                }
                else
                    speakBtn.Hidden = true;
            }
            ms = new MServer(Baseurl);
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            namequestLbl.Text = Username + " - " + Questionnaire;
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
            _ = sender;
            Play();
        }

        partial void SpeakBtn_TouchUpInside(UIButton sender)
        {
            _ = sender;
            if (recook)
            {
                nameLbl.Text = "Speak now...";
                Recognize();
            }
        }

        partial void NextBtn_TouchUpInside(UIButton sender)
        {
            _ = sender;
            StopVoiceReco();
            Next();
        }

        partial void PreviousBtn_TouchUpInside(UIButton sender)
        {
            _ = sender;
            StopVoiceReco();
            Previous();
        }

        partial void ReturnBtn_TouchUpInside(UIButton sender)
        {
            _ = sender;
            stopView.Show();
            stopView.Clicked += (object senders, UIButtonEventArgs e) => 
            {
                if (e.ButtonIndex == 1)
                {
                    StopVoiceReco();
                    Handsfree = false;
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
            var url = NSUrl.FromString("https://imeddrsapi.azurewebsites.net/data/voice/" + Language + "/" + Name + Extension);
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
                if (Handsfree)
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
                                    nameLbl.Text = "Speak now...";
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
            if (Handsfree)
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
                    Handsfree = false;
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
                        if (!recresp.ToLower().Contains("previous") && !recresp.ToLower().Contains("next") && recresp.ToLower() != "stop")
                        {
                            if (recresp.ToLower().Contains("mail") && Name == "Gender")
                                recresp = "male";
                            if (Response.Length == 1)
                            {
                                if (Type == "number")
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
                                for (int i = 0; i < Response.Length; i++)
                                {
                                    if (recresp.ToLower().Contains(Response[i].ToLower().Trim()))
                                    {
                                        if (Response.Length < 4)
                                            responseSmc.SelectedSegment = i;
                                        else
                                            responsePkr.Select(i, 0, false);
                                        recresp = "next";
                                        break;
                                    }
                                }
                            }
                        }
                        if (Handsfree)
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
                    if (Handsfree)
                        recoLbl.Text = "error";
                }
                audioEngine.Stop();
                recognitionRequest.EndAudio();
                nameLbl.Text = Name;
            });
        }

        private void StopVoiceReco()
        {
            nameLbl.Text = Name;
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
            if (Convert.ToInt32(Number) < Convert.ToInt32(Last))
            {
                string ans = GetRepsonse();
                if (Required && ans == "~")
                    AlertMessage("A response is required");
                else
                {
                    BTProgressHUD.Show("Processing...Please wait...");
                    message = new string[] { "questionnaire", "next", Userid, Questionnaire, Number, ans, String.Join(',', Responses).Replace("/", "~"), Branches };
                    await Task.Run(() => result = ms.ProcessMessage(message, "GET", ""));
                    BTProgressHUD.Dismiss();
                    if (result[1] == "ack")
                    {
                        Number = result[2];
                        Name = result[3];
                        Text = result[4];
                        Response = result[5].Split(',');
                        Last = result[6];
                        //Responses = result[7];
                        Branches = result[8];
                        Required = Convert.ToBoolean(result[9]);
                        Type = result[10];
                        Answer = result[11];
                        Eresponse = result[12].Split(',');
                        Extension = result[13];
                        Instructions = result[14];
                        SetQuestion();
                        SetResponses();
                    }
                    else
                        AlertMessage(result[2]);
                }
            }
            else
            {
                if (Role != "demo")
                {
                    BTProgressHUD.Show("Processing...Please wait...");
                    message = new string[] { "questionnaire", "save", Userid, Questionnaire, String.Join(',', Responses).Replace("/", "~") };
                    await Task.Run(() => result = ms.ProcessMessage(message, "GET", ""));
                    BTProgressHUD.Dismiss();
                    if (!Handsfree)
                        AlertMessage(result[2]);
                }
                else
                {
                    string file = path + Questionnaire.Replace(" ", "_") + ".txt";
                    if (ps.WriteToFile(file, String.Join(',', Responses), true))
                    {
                        if (!Handsfree)
                            AlertMessage("Questionnaire responses saved");
                    }
                    else
                    {
                        if (!Handsfree)
                            AlertMessage("Not able to save resposnes");
                    }
                }
                DismissModalViewController(false);
            }
        }

        private async void Previous()
        {
            BTProgressHUD.Show("Processing...Please wait...");
            message = new string[] { "questionnaire", "previous", Userid, Questionnaire, Number, String.Join(',', Responses).Replace("/", "~"), Branches };
            await Task.Run(() => result = ms.ProcessMessage(message, "GET", ""));
            BTProgressHUD.Dismiss();
            if (result[1] == "ack")
            {
                Number = result[2];
                Name = result[3];
                Text = result[4];
                Response = result[5].Split(',');
                Last = result[6];
                //Responses = result[7];
                Branches = result[8];
                Required = Convert.ToBoolean(result[9]);
                Type = result[10];
                Answer = result[11];
                Eresponse = result[12].Split(',');
                Extension = result[13];
                Instructions = result[14];
                SetQuestion();
                SetResponses();
            }
            else
                AlertMessage(result[2]);
        }

        private void SetQuestion()
        {
            instructionsLbl.Text = Instructions.Replace("~", "\r\n");
            nameLbl.Text = Name;
            questionBtn.SetTitle(Text, UIControlState.Normal);
            if (Name == "End")
                report = true;
        }

        private void SetResponses()
        {
            responseTxt.Hidden = true;
            responseSmc.Hidden = true;
            responsePkr.Hidden = true;
            responseDpr.Hidden = true;
            if (Name != "End")
            {
                if (Response.Length == 1)
                {
                    if (Type == "date")
                    {
                        if (Answer != "")
                        {
                            try { responseDpr.SetDate((NSDate)Convert.ToDateTime(Answer), false); }
                            catch { responseDpr.SetDate((NSDate)DateTime.Now, false); }
                        }
                        else
                            responseDpr.SetDate((NSDate)DateTime.Now, false);
                        responseDpr.Hidden = false;
                    }
                    else
                    {
                        responseTxt.Text = Answer;
                        if (Name == "Age")
                        {
                            if (responseTxt.Text.StartsWith("0."))
                                responseTxt.Text = responseTxt.Text[1..];
                            if (responseTxt.Text.EndsWith(".00"))
                                responseTxt.Text = responseTxt.Text[0..^3];
                        }
                        responseTxt.Hidden = false;
                        if (Type == "number" || Type == "decimal")
                            responseTxt.KeyboardType = UIKeyboardType.NumbersAndPunctuation;
                        else
                            responseTxt.KeyboardType = UIKeyboardType.Default;
                    }
                }
                else
                {
                    if (Response.Length < 4)
                    {
                        responseSmc.RemoveAllSegments();
                        responseSmc.Frame = new CoreGraphics.CGRect(responseSmc.Frame.X, responseSmc.Frame.Y, 150f * Response.Length, responseSmc.Frame.Height);
                        for (int i = 0; i < Response.Length; i++)
                        {
                            responseSmc.InsertSegment(Response[i], i, true);
                            responseSmc.SetEnabled(true, i);
                            responseSmc.SetWidth(100f, i);
                            if (Eresponse[i] == Answer)
                                responseSmc.SelectedSegment = i;
                        }
                        responseSmc.Hidden = false;
                    }
                    else
                    {
                        int row = 0;
                        Presponses.Clear();
                        for (int i = 0; i < Response.Length; i++)
                        {
                            Presponses.Add(Response[i]);
                            if (Eresponse[i] == Answer)
                                row = i;
                        }
                        SelectedLbl.Text = Presponses[row];
                        responsePkr.Model = new PickerModel(Presponses, SelectedLbl);
                        responsePkr.Select(row, 0, false);
                        responsePkr.Hidden = false;
                    }
                }
            }
            if (Handsfree)
                Play();
        }

        private string GetRepsonse()
        {
            string result = "";
            if (Name != "End")
            {
                if (Response.Length == 1)
                {
                    switch (Type)
                    {
                        case "date":
                            try
                            {
                                DateTime dt = (DateTime)responseDpr.Date;
                                result = dt.ToString("MMddyyyy");
                            }
                            catch { }
                            break;
                        case "decimal":
                            try { decimal d = Convert.ToDecimal(responseTxt.Text); }
                            catch { responseTxt.Text = ""; }
                            result = responseTxt.Text;
                            break;
                        case "number":
                            try { int i = Convert.ToInt32(responseTxt.Text); }
                            catch { responseTxt.Text = ""; }
                            result = responseTxt.Text;
                            break;
                        default:
                            result = responseTxt.Text;
                            break;
                    }
                }
                else
                {
                    if (Response.Length < 4)
                    {
                        if (responseSmc.SelectedSegment > -1)
                            result = Eresponse[responseSmc.SelectedSegment];
                    }
                    else
                    {
                        result = SelectedLbl.Text;
                    }
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
                if (audioSession.SetMode(AVAudioSession.ModeVideoChat, out _))
                {
                    if (audioSession.OverrideOutputAudioPort(AVAudioSessionPortOverride.Speaker, out _))
                        audioSession.SetActive(true);
                }
            }
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

        public class PickerModel : UIPickerViewModel
        {
            readonly List<string> list;
            readonly UILabel label;

            public PickerModel(List<string> list, UILabel label)
            {
                this.list = list;
                this.label = label;
            }

            public override nint GetComponentCount(UIPickerView pickerView)
            {
                return 1;
            }

            public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
            {
                return list.Count;
            }

            public override string GetTitle(UIPickerView pickerView, nint row, nint component)
            {
                return list[(int)row];
            }

            public override nfloat GetRowHeight(UIPickerView pickerView, nint component)
            {
                return (nfloat)20;
            }

            public override UIView GetView(UIPickerView pickerView, nint row, nint component, UIView view)
            {
                UILabel lbl = new UILabel(new RectangleF(0, 0, 300f, 40f))
                {
                    TextColor = UIColor.Black,
                    Font = UIFont.SystemFontOfSize(17f),
                    TextAlignment = UITextAlignment.Center,
                    Text = list[(int)row]
                };
                return lbl;
            }

            public override void Selected(UIPickerView pickerView, nint row, nint component)
            {
                label.Text = list[(int)row];
            }
        }
    }
}