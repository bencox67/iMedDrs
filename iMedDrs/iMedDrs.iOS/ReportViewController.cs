using Foundation;
using System;
using System.Threading.Tasks;
using UIKit;
using BigTed;
using Newtonsoft.Json;

namespace iMedDrs.iOS
{
    public partial class ReportViewController : UIViewController
    {
        public string Baseurl { get; set; }
        public string Userid { get; set; }
        public string Username { get; set; }
        public string Questionnaire { get; set; }
        public string Text { get; set; }
        public string Data { get; set; }
        public string Email { get; set; }
        public int Last { get; set; }
        public int Number { get; set; }
        public string Role { get; set; }
        public string Language { get; set; }
        private string[] message;
        private string[] result;
        private readonly UIAlertView alertView;
        private MServer ms;

        [Action("UnwindToReportViewController:")]
        public void UnwindToReportViewController(UIStoryboardSegue segue)
        {
            _ = segue;
        }

        public ReportViewController (IntPtr handle) : base (handle)
        {
            alertView = new UIAlertView();
            alertView.AddButton("Ok");
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
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
            reportWv.LoadHtmlString(Text, null);
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            base.PrepareForSegue(segue, sender);
            if (segue.DestinationViewController.Class.Name == "SendViewController")
            {
                var viewController = (SendViewController)segue.DestinationViewController;
                viewController.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
                viewController.Baseurl = Baseurl;
                viewController.Userid = Userid;
                viewController.Username = Username;
                viewController.Questionnaire = Questionnaire;
                viewController.Data = Data;
                viewController.Email = Email;
                viewController.Role = Role;
                viewController.Language = Language;
            }
        }

        partial void NextBtn_TouchUpInside(UIButton sender)
        {
            _ = sender;
            Number++;
            if (Number == Last)
                Number = 0;
            ShowReport();
        }

        partial void PreviousBtn_TouchUpInside(UIButton sender)
        {
            _ = sender;
            Number--;
            if (Number < 0)
                Number = Last - 1;
            ShowReport();
        }

        partial void SendBtn_TouchUpInside(UIButton sender)
        {
            _ = sender;
        }

        private async void ShowReport()
        {
            BTProgressHUD.Show("Processing...Please wait...");
            if (Role == "demo")
                message = new string[] { "reports", Userid, Questionnaire, Data.Replace("/", "~").Replace(",", "|"), Language };
            else
                message = new string[] { "reports", Userid, Questionnaire, "*", Language };
            await Task.Run(() => result = ms.ProcessMessage(message, "GET", ""));
            BTProgressHUD.Dismiss();
            if (result[0] == "ack")
            {
                ReportModel model = JsonConvert.DeserializeObject<ReportModel>(result[1]);
                reportWv.LoadHtmlString(model.Reports[Number].Text, null);
            }
            else
                AlertMessage(result[1]);
        }

        private void AlertMessage(string title)
        {
            if (title != "")
            {
                alertView.Title = title;
                alertView.Show();
            }
        }
    }
}