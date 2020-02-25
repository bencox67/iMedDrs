using Foundation;
using System;
using System.Threading.Tasks;
using UIKit;
using BigTed;

namespace iMedDrs.iOS
{
    public partial class ReportViewController : UIViewController
    {
        public string baseurl { get; set; }
        public string userid { get; set; }
        public string username { get; set; }
        public string questionnaire { get; set; }
        public string text { get; set; }
        public string data { get; set; }
        public string email { get; set; }
        public int last { get; set; }
        public int number { get; set; }
        public int level { get; set; }
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
            reportWv.LoadHtmlString(text, null);
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            base.PrepareForSegue(segue, sender);
            if (segue.DestinationViewController.Class.Name == "SendViewController")
            {
                var viewController = (SendViewController)segue.DestinationViewController;
                viewController.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
                viewController.baseurl = baseurl;
                viewController.userid = userid;
                viewController.username = username;
                viewController.questionnaire = questionnaire;
                viewController.data = data;
                viewController.email = email;
                viewController.level = level;
            }
        }

        partial void NextBtn_TouchUpInside(UIButton sender)
        {
            _ = sender;
            number++;
            if (number == last)
                number = 0;
            ShowReport();
        }

        partial void PreviousBtn_TouchUpInside(UIButton sender)
        {
            _ = sender;
            number--;
            if (number < 0)
                number = last - 1;
            ShowReport();
        }

        partial void SendBtn_TouchUpInside(UIButton sender)
        {
            _ = sender;
        }

        private async void ShowReport()
        {
            BTProgressHUD.Show("Processing...Please wait...");
            if (level < 2)
                message = new string[] { "report", "load2", userid, questionnaire, number.ToString(), data };
            else
                message = new string[] { "report", "load", userid, questionnaire, number.ToString() };
            await Task.Run(() => result = ms.ProcessMessage(message, "GET"));
            BTProgressHUD.Dismiss();
            if (result[1] == "ack")
            {
                last = Convert.ToInt32(result[2]);
                number = Convert.ToInt32(result[3]);
                reportWv.LoadHtmlString(result[4], null);
            }
            else
                AlertMessage(result[2]);
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