using Foundation;
using System;
using System.Threading.Tasks;
using UIKit;
using BigTed;

namespace iMedDrs.iOS
{
    public partial class SendViewController : UIViewController
    {
        public string Baseurl { get; set; }
        public string Userid { get; set; }
        public string Username { get; set; }
        public string Questionnaire { get; set; }
        public string Data { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Language { get; set; }
        private string[] message;
        private string[] result;
        private readonly UIAlertView alertView;
        private MServer ms;

        public SendViewController (IntPtr handle) : base (handle)
        {
            alertView = new UIAlertView();
            alertView.AddButton("Ok");
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
            ms = new MServer(Baseurl);
            if (Email == "")
                myemailBtn.Hidden = true;
            else
            {
                if (locationsTxt.Text != "" && !locationsTxt.Text.EndsWith("\n"))
                    locationsTxt.Text += "\n";
                locationsTxt.Text += Email + "\n";
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
            namequestLbl.Text = Username + " - " + Questionnaire;
        }

        partial void MyemailBtn_TouchUpInside(UIButton sender)
        {
            _ = sender;
            if (!locationsTxt.Text.Contains(Email))
            {
                if (locationsTxt.Text != "" && !locationsTxt.Text.EndsWith("\n"))
                    locationsTxt.Text += "\n";
                locationsTxt.Text += Email + "\n";
            }
        }

        partial void SendBtn_TouchUpInside(UIButton sender)
        {
            _ = sender;
             Send();
        }

        private async void Send()
        {
            string location = "~";
            if (locationsTxt.Text != "")
            {
                location = locationsTxt.Text.Replace("\n", "|");
                location = location.Replace("||", "|");
                if (location.EndsWith("|"))
                    location = location[0..^1];
            }
            BTProgressHUD.Show("Processing...Please wait...");
            if (Role == "demo")
                message = new string[] { "reports", Userid, Questionnaire, location, Data.Replace("/", "~").Replace(",", "|"), Language };
            else
                message = new string[] { "reports", Userid, Questionnaire, location, "*", Language };
            await Task.Run(() => result = ms.ProcessMessage(message, "GET", ""));
            BTProgressHUD.Dismiss();
            if (result[1] != "" && result[1] != "~")
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