using Foundation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using AVFoundation;
using UIKit;
using BigTed;

namespace iMedDrs.iOS
{
    public partial class MaintainViewController : UIViewController
    {
        public string Baseurl { get; set; }
        public string Userid { get; set; }
        public List<string> Questionnaires { get; set; }
        public List<string> Languages { get; set; }
        public string MessageTitle { get; set; }
        public string Datapath { get; set; }
        public int Level { get; set; }
        private int scriptnum;
        private string[][] scriptdata;
        private string[] message;
        private string[] result;
        private readonly List<string> scripts;
        private readonly UILabel scriptLbl;
        private UILabel questionnaireLbl;
        private UILabel languageLbl;
        private readonly UIAlertView alertView;
        private UIPickerView questionnaireView;
        private UIPickerView languageView;
        private AVAudioSession audioSession;
        private AVAudioPlayer player;
        private AVAudioRecorder recorder;
        private readonly AudioSettings settings;
        private MServer ms;

        public MaintainViewController (IntPtr handle) : base (handle)
        {
            alertView = new UIAlertView();
            alertView.AddButton("Ok");
            scriptLbl = new UILabel();
            scripts = new List<string>();
            settings = new AudioSettings
            {
                SampleRate = 44100.0f,
                Format = AudioToolbox.AudioFormatType.MPEG4AAC,
                NumberChannels = 1,
                AudioQuality = AVAudioQuality.High,
            };
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            questionnaireLbl = new UILabel
            {
                Text = "0"
            };
            questionnaireView = new UIPickerView
            {
                BackgroundColor = UIColor.White,
                Model = new PickerModel(Questionnaires, questionnaireLbl)
            };
            UIToolbar toolbar1 = new UIToolbar(new RectangleF(0.0f, 0.0f, (float)View.Frame.Size.Width, 44.0f))
            {
                Items = new UIBarButtonItem[]{
                    new UIBarButtonItem(UIBarButtonSystemItem.Cancel, delegate { questionnaireTxt.ResignFirstResponder(); }),
                    new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                    new UIBarButtonItem(UIBarButtonSystemItem.Done, Questionnaire)
                }
            };
            questionnaireTxt.InputView = questionnaireView;
            questionnaireTxt.InputAccessoryView = toolbar1;
            languageLbl = new UILabel
            {
                Text = "0"
            };
            languageView = new UIPickerView
            {
                BackgroundColor = UIColor.White,
                Model = new PickerModel(Languages, languageLbl)
            };
            UIToolbar toolbar2 = new UIToolbar(new RectangleF(0.0f, 0.0f, (float)View.Frame.Size.Width, 44.0f))
            {
                Items = new UIBarButtonItem[]{
                    new UIBarButtonItem(UIBarButtonSystemItem.Cancel, delegate { languageTxt.ResignFirstResponder(); }),
                    new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                    new UIBarButtonItem(UIBarButtonSystemItem.Done, Language)
                }
            };
            languageTxt.InputView = languageView;
            languageTxt.InputAccessoryView = toolbar2;
            questionnaireTxt.Text = Questionnaires[0];
            languageTxt.Text = Languages[0];
            var tap = new UITapGestureRecognizer { CancelsTouchesInView = false };
            tap.AddTarget(() => View.EndEditing(true));
            View.AddGestureRecognizer(tap);
            ms = new MServer(Baseurl);
            Load();
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            SetAudioSettings();
        }

        public void Questionnaire(object sender, EventArgs e)
        {
            questionnaireTxt.ResignFirstResponder();
            if (questionnaireTxt.Text != Questionnaires[Convert.ToInt32(questionnaireLbl.Text)])
            {
                questionnaireTxt.Text = Questionnaires[Convert.ToInt32(questionnaireLbl.Text)];
                Load();
            }
        }

        public void Language(object sender, EventArgs e)
        {
            languageTxt.ResignFirstResponder();
            if (languageTxt.Text != Languages[Convert.ToInt32(languageLbl.Text)])
            {
                languageTxt.Text = Languages[Convert.ToInt32(languageLbl.Text)];
                Load();
            }
        }

        partial void PreviousBtn_TouchUpInside(UIButton sender)
        {
            _ = sender;
            if (recordBtn.TitleLabel.Text == "Record")
            {
                int position = 0;
                if (scriptnum > 0)
                    position = scriptnum - 1;
                if (nameTxt.Text != "")
                    position--;
                if (position > -1)
                    ScriptData(position);
            }
        }

        partial void NextBtn_TouchUpInside(UIButton sender)
        {
            _ = sender;
            if (recordBtn.TitleLabel.Text == "Record")
            {
                int position = 0;
                if (scriptnum > 0)
                    position = scriptnum - 1;
                if (nameTxt.Text != "")
                    position++;
                if (position < scripts.Count)
                    ScriptData(position);
            }
        }

        partial void SelectBtn_TouchUpInside(UIButton sender)
        {
            _ = sender;
            ScriptData(Convert.ToInt32(scriptLbl.Text));
        }

        partial void RecordBtn_TouchUpInside(UIButton sender)
        {
            _ = sender;
            if (nameTxt.Text != "")
            {
                if (recordBtn.TitleLabel.Text == "Record")
                {
                    recorder = AVAudioRecorder.Create(new NSUrl(Path.Combine(Datapath, nameTxt.Text + ".mp4")), settings, out NSError error);
                    if (recorder != null)
                    {
                        recorder.PrepareToRecord();
                        recorder.Record();
                        recordBtn.SetTitle("Stop", UIControlState.Normal);
                    }
                    if (error != null)
                        AlertMessage(error.LocalizedDescription);
                }
                else
                {
                    recorder.Stop();
                    recorder.Dispose();
                    recordBtn.SetTitle("Record", UIControlState.Normal);
                }
            }
        }

        partial void PlayBtn_TouchUpInside(UIButton sender)
        {
            _ = sender;
            if (nameTxt.Text != "")
            {
                if (recordBtn.TitleLabel.Text == "Stop")
                {
                    recorder.Stop();
                    recorder.Dispose();
                    recordBtn.SetTitle("Record", UIControlState.Normal);
                }
                if (playBtn.TitleLabel.Text == "Play")
                {
                    if (File.Exists(Path.Combine(Path.Combine(Datapath, nameTxt.Text + ".mp4"))))
                    {
                        player = AVAudioPlayer.FromUrl(new NSUrl(Path.Combine(Datapath, nameTxt.Text + ".mp4")), out _);
                        if (player != null)
                        {
                            player.FinishedPlaying += Player_FinishedPlaying;
                            player.Play();
                            playBtn.SetTitle("Stop", UIControlState.Normal);
                        }
                    }
                }
                else
                {
                    if (player.Playing)
                    {
                        player.Stop();
                        player.Dispose();
                        playBtn.SetTitle("Play", UIControlState.Normal);
                    }
                }
            }
        }

        private void Player_FinishedPlaying(object sender, AVStatusEventArgs e)
        {
            player = null;
            playBtn.SetTitle("Play", UIControlState.Normal);
        }

        partial void UpdatevoiceBtn_TouchUpInside(UIButton sender)
        {
            _ = sender;
            if (recordBtn.TitleLabel.Text == "Record")
            {
                if (File.Exists(Path.Combine(Datapath, nameTxt.Text + ".mp4")))
                    UpdateVoice();
            }
        }

        private async void Load()
        {
            ClearForm();
            message = new string[] { "script", "data", questionnaireTxt.Text, Level.ToString(), languageTxt.Text };
            BTProgressHUD.Show("Processing...Please wait...");
            await Task.Run(() => result = ms.ProcessMessage(message, "GET", ""));
            BTProgressHUD.Dismiss();
            if (result[1] == "ack")
            {
                int len = result.Length - 2;
                scriptdata = new string[len][];
                for (int i = 0; i < len; i++)
                {
                    scriptdata[i] = result[i + 2].Split('|');
                    scripts.Add(scriptdata[i][0]);
                }
                scriptLbl.Text = "0";
                questionPkr.Model = new PickerModel(scripts, scriptLbl);
                questionPkr.Select(0, 0, false);
            }
            else
            {
                questionPkr.Model = new PickerModel(scripts, scriptLbl);
                AlertMessage(result[2]);
            }
        }

        private async void UpdateVoice()
        {
            string message = "";
            if (File.Exists(Path.Combine(Datapath, nameTxt.Text + ".mp4")))
            {
                BTProgressHUD.Show("Processing...Please wait...");
                await Task.Run(() => message = ms.PostFile(languageTxt.Text, Datapath, nameTxt.Text + ".mp4"));
                BTProgressHUD.Dismiss();
                if (message == "Recording updated")
                {
                    try { File.Delete(Path.Combine(Datapath, nameTxt.Text + ".mp4")); }
                    catch { }
                }
            }
            AlertMessage(message);
        }

        private void ScriptData(int number)
        {
            scriptnum = number + 1;
            nameTxt.Text = scriptdata[number][0];
            if (scriptdata[number][8] == "")
                questionTxt.Text = scriptdata[number][7];
            else
                questionTxt.Text = scriptdata[number][8];
        }

        private void ClearForm()
        {
            scriptnum = 0;
            scripts.Clear();
            scriptLbl.Text = "0";
            nameTxt.Text = "";
            questionTxt.Text = "";
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

        private class PickerModel : UIPickerViewModel
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
                UILabel lbl = new UILabel(new RectangleF(0, 0, 300f, 20f))
                {
                    TextColor = UIColor.Black,
                    Font = UIFont.SystemFontOfSize(14f),
                    TextAlignment = UITextAlignment.Center,
                    Text = list[(int)row]
                };
                return lbl;
            }

            public override void Selected(UIPickerView pickerView, nint row, nint component)
            {
                label.Text = row.ToString();
            }
        }
    }
}