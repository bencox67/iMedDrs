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
        private UIAlertView alertView;
        private UIAlertView progressView;
        private MServer ms;

        [Action("UnwindToReportViewController:")]
        public void UnwindToReportViewController(UIStoryboardSegue segue)
        {
        }

        public ReportViewController (IntPtr handle) : base (handle)
        {
            alertView = new UIAlertView();
            alertView.AddButton("Ok");
            progressView = new UIAlertView
            {
                Title = "Processing... Please Wait..."
            };
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
            number++;
            if (number == last)
                number = 0;
            ShowReport();
        }

        partial void PreviousBtn_TouchUpInside(UIButton sender)
        {
            number--;
            if (number < 0)
                number = last - 1;
            ShowReport();
        }

        partial void SendBtn_TouchUpInside(UIButton sender)
        {
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

        private void SetFrame(UIView view, float height, float width, float posx, float posy)
        {
            CoreGraphics.CGRect frame = view.Frame;
            if (height > 0) frame.Height = height;
            if (width > 0) frame.Width = width;
            if (height > 0) frame.Height = height;
            if (posx > 0) frame.X = posx;
            if (posy > 0) frame.Y = posy;
            view.Frame = frame;
        }
    }
}