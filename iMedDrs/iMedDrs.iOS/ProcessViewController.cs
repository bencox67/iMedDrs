using Foundation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using UIKit;
using BigTed;

namespace iMedDrs.iOS
{
    public partial class ProcessViewController : UIViewController
    {
        public string baseurl { get; set; }
        public string userid { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string gender { get; set; }
        public string birthdate { get; set; }
        public string language { get; set; }
        public string email { get; set; }
        public string datapath { get; set; }
        public int level { get; set; }
        public List<string> questionnaires { get; set; }
        public UILabel selectedLbl;
        private bool report;
        private string number;
        private string name;
        private string text;
        private string response;
        private string last;
        private string responses;
        private string branches;
        private string required;
        private string type;
        private string answer;
        private string data;
        private string eresponse;
        private string extension;
        private string instructions;
        private readonly List<string> languages;
        private string[] message;
        private string[] result;
        private readonly UIAlertView alertView;
        private MServer ms;
        private readonly PServer ps;

        [Action("UnwindToProcessViewController:")]
        public void UnwindToProcessViewController(UIStoryboardSegue segue)
        {
        }

        public ProcessViewController (IntPtr handle) : base (handle)
        {
            alertView = new UIAlertView();
            alertView.AddButton("Ok");
            selectedLbl = new UILabel();
            languages = new List<string>();
            ps = new PServer();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            report = false;
            usernameLbl.Text = username;
            selectedLbl.Text = questionnaires[0];
            PickerModel model = new PickerModel(questionnaires, selectedLbl);
            questionairePkr.Model = model;
            ms = new MServer(baseurl);
            if (level < 2)
            {
                updateBtn.Hidden = true;
                logoutBtn.Hidden = true;
            }
            else
                returnBtn.Hidden = true;
            if (level < 8)
                maintainBtn.Hidden = true;
            GetLanguages();
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        public override void ViewWillAppear(bool animated)
        {
            if (report)
                Report();
            base.ViewWillAppear(animated);
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            base.PrepareForSegue(segue, sender);
            if (segue.DestinationViewController.Class.Name == "QuestionViewController")
            {
                if (handsfreeSwh.On)
                    report = true;
                var viewController = (QuestionViewController)segue.DestinationViewController;
                viewController.baseurl = baseurl;
                viewController.userid = userid;
                viewController.username = username;
                viewController.questionnaire = selectedLbl.Text;
                viewController.number = number;
                viewController.name = name;
                viewController.text = text;
                viewController.response = response.Split(',');
                viewController.last = last;
                viewController.responses = responses;
                viewController.branches = branches;
                viewController.required = Convert.ToBoolean(required);
                viewController.type = type;
                viewController.answer = answer;
                viewController.eresponse = eresponse.Split(',');
                viewController.datapath = datapath;
                viewController.language = language;
                viewController.extension = extension;
                viewController.instructions = instructions;
                viewController.level = level;
                viewController.email = email;
                viewController.handsfree = handsfreeSwh.On;
            }
            if (segue.DestinationViewController.Class.Name == "ReportViewController")
            {
                var viewController = (ReportViewController)segue.DestinationViewController;
                viewController.baseurl = baseurl;
                viewController.userid = userid;
                viewController.username = username;
                viewController.questionnaire = selectedLbl.Text;
                viewController.last = Convert.ToInt32(last);
                viewController.number = Convert.ToInt32(number);
                viewController.text = text;
                viewController.data = data;
                viewController.email = email;
                viewController.level = level;
            }
            if (segue.DestinationViewController.Class.Name == "UserViewController")
            {
                var viewController = (UserViewController)segue.DestinationViewController;
                viewController.baseurl = baseurl;
                viewController.userid = userid;
                viewController.password = password;
                viewController.username = username;
                viewController.language = language;
                viewController.gender = gender;
                viewController.birthdate = birthdate;
                viewController.languages = languages;
                viewController.email = email;
            }
            if (segue.DestinationViewController.Class.Name == "MaintainViewController")
            {
                var viewController = (MaintainViewController)segue.DestinationViewController;
                viewController.baseurl = baseurl;
                viewController.userid = userid;
                viewController.questionnaires = questionnaires;
                viewController.languages = languages;
                viewController.datapath = datapath;
                viewController.level = level;
            }
        }

        partial void StartBtn_TouchUpInside(UIButton sender)
        {
            Start();
        }

        partial void ViewBtn_TouchUpInside(UIButton sender)
        {
            Report();
        }

        private async void GetLanguages()
        {
            BTProgressHUD.Show("Processing...Please wait...");
            message = new string[] { "user", "languages" };
            await Task.Run(() => result = ms.ProcessMessage(message, "GET"));
            BTProgressHUD.Dismiss();
            if (result[1] == "ack")
            {
                for (int i = 2; i < result.Length; i++)
                    languages.Add(result[i]);
            }
            else
                AlertMessage(result[2]);
        }

        private async void Start()
        {
            BTProgressHUD.Show("Processing...Please wait...");
            message = new string[] { "questionnaire", "start", userid, selectedLbl.Text };
            await Task.Run(() => result = ms.ProcessMessage(message, "GET"));
            BTProgressHUD.Dismiss();
            if (result[1] == "ack")
            {
                number = result[2];
                name = result[3];
                text = result[4];
                response = result[5];
                last = result[6];
                responses = result[7];
                branches = result[8];
                required = result[9];
                type = result[10];
                answer = result[11];
                eresponse = result[12];
                extension = result[13];
                instructions = result[14];
                PerformSegue("QuestSegue", this);
            }
            else
                AlertMessage(result[2]);
        }

        private async void Report()
        {
            report = false;
            message = null;
            if (level < 2)
            {
                data = ps.ReadFromFile(datapath + "/" + selectedLbl.Text.Replace(" ", "_") + ".txt");
                if (data != "")
                    message = new string[] { "report", "load2", userid, selectedLbl.Text, "0", data };
                else
                    AlertMessage("No responses saved");
            }
            else
                message = new string[] { "report", "load", userid, selectedLbl.Text, "0" };
            if (message != null)
            {
                BTProgressHUD.Show("Processing...Please wait...");
                await Task.Run(() => result = ms.ProcessMessage(message, "GET"));
                BTProgressHUD.Dismiss();
                if (result[1] == "ack")
                {
                    last = result[2];
                    number = result[3];
                    text = result[4];
                    PerformSegue("ReportSegue", this);
                }
                else
                    AlertMessage(result[2]);
            }
        }

        private void AlertMessage(string title)
        {
            if (title != "")
            {
                alertView.Title = title;
                alertView.Show();
            }
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