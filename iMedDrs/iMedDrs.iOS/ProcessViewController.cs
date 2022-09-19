using Foundation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using UIKit;
using BigTed;
using Newtonsoft.Json;

namespace iMedDrs.iOS
{
    public partial class ProcessViewController : UIViewController
    {
        public string Baseurl { get; set; }
        public string Userid { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Gender { get; set; }
        public string Birthdate { get; set; }
        public string Language { get; set; }
        public string Email { get; set; }
        public string Datapath { get; set; }
        public string Role { get; set; }
        public List<string> Questionnaires { get; set; }
        public List<string> Languages { get; set; }
        public UILabel SelectedLbl;
        private string data;
        private string[] message;
        private string[] result;
        private bool report;
        private QuestionnaireModel questionnaire;
        private ReportModel model;
        private readonly UIAlertView alertView;
        private MServer ms;
        private readonly PServer ps;

        [Action("UnwindToProcessViewController:")]
        public void UnwindToProcessViewController(UIStoryboardSegue segue)
        {
            QuestionViewController controller = (QuestionViewController)segue.SourceViewController;
            report = controller.Report;
        }

        public ProcessViewController (IntPtr handle) : base (handle)
        {
            alertView = new UIAlertView();
            alertView.AddButton("Ok");
            SelectedLbl = new UILabel();
            ps = new PServer();
            report = false;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            usernameLbl.Text = Username;
            SelectedLbl.Text = Questionnaires[0];
            PickerModel model = new PickerModel(Questionnaires, SelectedLbl);
            questionairePkr.Model = model;
            ms = new MServer(Baseurl);
            if (Role == "demo")
            {
                updateBtn.Hidden = true;
                logoutBtn.Hidden = true;
            }
            else
                returnBtn.Hidden = true;
            if (Role != "admin")
                maintainBtn.Hidden = true;
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
                var viewController = (QuestionViewController)segue.DestinationViewController;
                viewController.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
                viewController.Baseurl = Baseurl;
                viewController.Userid = Userid;
                viewController.Username = Username;
                viewController.Questionnaire = SelectedLbl.Text;
                viewController.Questionnaireid = questionnaire.Id.ToString();
                viewController.Number = questionnaire.Sequence.ToString();
                viewController.Name = questionnaire.QuestionName;
                viewController.Text = questionnaire.Question;
                viewController.Response = questionnaire.ActResponses.ToArray();
                viewController.Last = questionnaire.EndSequence.ToString();
                viewController.Responses = questionnaire.Responses;
                viewController.Required = questionnaire.Required;
                viewController.Type = questionnaire.Type;
                viewController.Answer = "";
                viewController.Eresponse = questionnaire.EngResponses.ToArray();
                viewController.Datapath = Datapath;
                viewController.Language = Language;
                viewController.Extension = ".mp3";
                viewController.Instructions = questionnaire.Instructions.Replace("<br/>", "\r\n");
                viewController.Role = Role;
                viewController.Email = Email;
                viewController.Handsfree = handsfreeSwh.On;
            }
            if (segue.DestinationViewController.Class.Name == "ReportViewController")
            {
                var viewController = (ReportViewController)segue.DestinationViewController;
                viewController.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
                viewController.Baseurl = Baseurl;
                viewController.Userid = Userid;
                viewController.Username = Username;
                viewController.Questionnaire = SelectedLbl.Text;
                viewController.Last = model.MaxId;
                viewController.Number = 0;
                viewController.Reports = model.Reports;
                viewController.Data = data;
                viewController.Email = Email;
                viewController.Role = Role;
                viewController.Language = "English";
            }
            if (segue.DestinationViewController.Class.Name == "UserViewController")
            {
                var viewController = (UserViewController)segue.DestinationViewController;
                viewController.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
                viewController.Baseurl = Baseurl;
                viewController.Userid = Userid;
                viewController.Password = Password;
                viewController.Username = Username;
                viewController.Language = Language;
                viewController.Gender = Gender;
                viewController.Birthdate = Birthdate;
                viewController.Languages = Languages;
                viewController.Email = Email;
                viewController.Role = Role;
            }
            if (segue.DestinationViewController.Class.Name == "MaintainViewController")
            {
                var viewController = (MaintainViewController)segue.DestinationViewController;
                viewController.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
                viewController.Baseurl = Baseurl;
                viewController.Userid = Userid;
                viewController.Questionnaires = Questionnaires;
                viewController.Languages = Languages;
                viewController.Datapath = Datapath;
            }
        }

        partial void StartBtn_TouchUpInside(UIButton sender)
        {
            _ = sender;
            Start();
        }

        partial void ViewBtn_TouchUpInside(UIButton sender)
        {
            _ = sender;
            Report();
        }

        private async void Start()
        {
            BTProgressHUD.Show("Processing...Please wait...");
            message = new string[] { "questionnaires", SelectedLbl.Text, Userid, Language };
            await Task.Run(() => result = ms.ProcessMessage(message, "GET", ""));
            BTProgressHUD.Dismiss();
            if (result[0] == "ack")
            {
                questionnaire = JsonConvert.DeserializeObject<QuestionnaireModel>(result[1]);
                PerformSegue("QuestSegue", this);
            }
            else
                AlertMessage(result[1]);
        }

        private async void Report()
        {
            message = null;
            string[] responses = new string[1];
            if (Role == "demo")
            {
                data = ps.ReadFromFile(Datapath + "/" + SelectedLbl.Text.Replace(" ", "_") + ".txt");
                if (data != "")
                {
                    responses = data.Split(',');
                    message = new string[] { "reports", Userid, SelectedLbl.Text.Replace(" ", "%20"), "English" };
                }
                else
                    AlertMessage("No responses saved");
            }
            else
                message = new string[] { "reports", Userid, SelectedLbl.Text.Replace(" ", "%20"), "English" };
            if (message != null)
            {
                BTProgressHUD.Show("Processing...Please wait...");
                if (Role == "demo")
                {
                    await Task.Run(() => result = ms.ProcessMessage(message, "POST", JsonConvert.SerializeObject(responses)));
                }
                else
                {
                    await Task.Run(() => result = ms.ProcessMessage(message, "GET", ""));
                }
                BTProgressHUD.Dismiss();

                if (result[0] == "ack")
                {
                    model = JsonConvert.DeserializeObject<ReportModel>(result[1]);
                    if (!string.IsNullOrWhiteSpace(model.Reports[0].Text))
                        PerformSegue("ReportSegue", this);
                    else
                        AlertMessage("No responses saved");
                }
                else
                    AlertMessage(result[1]);
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