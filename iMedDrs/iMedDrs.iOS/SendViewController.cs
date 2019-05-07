using Foundation;
using System;
using System.Threading.Tasks;
using UIKit;
using BigTed;

namespace iMedDrs.iOS
{
    public partial class SendViewController : UIViewController
    {
        public string baseurl { get; set; }
        public string userid { get; set; }
        public string username { get; set; }
        public string questionnaire { get; set; }
        public string data { get; set; }
        public string email { get; set; }
        public int level { get; set; }
        private string[] message;
        private string[] result;
        private UIAlertView alertView;
        private UIAlertView progressView;
        private MServer ms;

        public SendViewController (IntPtr handle) : base (handle)
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
            locationsTxt.Layer.BorderColor = UIColor.LightGray.CGColor;
            locationsTxt.Layer.BorderWidth = 0.5f;
            locationsTxt.Layer.CornerRadius = 5.0f;
            var tap = new UITapGestureRecognizer { CancelsTouchesInView = false };
            tap.AddTarget(() => View.EndEditing(true));
            View.AddGestureRecognizer(tap);
            ms = new MServer(baseurl);
            if (email == "")
                myemailBtn.Hidden = true;
            else
            {
                if (locationsTxt.Text != "" && !locationsTxt.Text.EndsWith("\n"))
                    locationsTxt.Text += "\n";
                locationsTxt.Text += email + "\n";
            }
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
        }

        partial void MyemailBtn_TouchUpInside(UIButton sender)
        {
            if (!locationsTxt.Text.Contains(email))
            {
                if (locationsTxt.Text != "" && !locationsTxt.Text.EndsWith("\n"))
                    locationsTxt.Text += "\n";
                locationsTxt.Text += email + "\n";
            }
        }

        partial void SendBtn_TouchUpInside(UIButton sender)
        {
            if (locationsTxt.Text != "")
                Send();
        }

        private async void Send()
        {
            string location = locationsTxt.Text.Replace("\n", "|");
            location = location.Replace("||", "|");
            if (location.EndsWith("|"))
                location = location.Substring(0, location.Length - 1);
            BTProgressHUD.Show("Processing...Please wait...");
            if (level < 2)
                message = new string[] { "report", "send2", userid, questionnaire, location, data };
            else
                message = new string[] { "report", "send", userid, questionnaire, location };
            await Task.Run(() => result = ms.ProcessMessage(message, "GET"));
            BTProgressHUD.Dismiss();
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