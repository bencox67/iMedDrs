using Foundation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UIKit;
using BigTed;

namespace iMedDrs.iOS
{
    public partial class LoginViewController : UIViewController
    {
        public string baseurl { get; set; }
        public string datapath { get; set; }
        public int level { get; set; }
        private string username;
        private string gender;
        private string birthdate;
        private string language;
        private string email;
        private List<string> questionnaires;
        private string[] message;
        private string[] result;
        private UIAlertView alertView;
        private UIAlertView progressView;
        private MServer ms;
        private PServer ps;

        [Action("UnwindToLoginViewController:")]
        public void UnwindToLoginViewController(UIStoryboardSegue segue)
        {
            passwordTxt.Text = "";
        }

        public LoginViewController (IntPtr handle) : base (handle)
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
            useridTxt.ShouldReturn += TextFieldShouldReturn;
            passwordTxt.ShouldReturn += TextFieldShouldReturn;
            var tap = new UITapGestureRecognizer { CancelsTouchesInView = false };
            tap.AddTarget(() => View.EndEditing(true));
            View.AddGestureRecognizer(tap);
            ms = new MServer(baseurl);
            ps = new PServer();
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            useridTxt.Text = ps.RememberMe(datapath, "", true);
            if (useridTxt.Text != "")
                rememberSwh.On = true;
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            View.EndEditing(true);
        }

        partial void LoginBtn_TouchUpInside(UIButton sender)
        {
            if (useridTxt.Text != "" && passwordTxt.Text != "" && useridTxt.Text.ToLower() != "demo")
                Login();
        }

        private bool TextFieldShouldReturn(UITextField textfield)
        {
            nint nextTag = textfield.Tag + 1;
            UIResponder nextResponder = this.View.ViewWithTag(nextTag);
            textfield.ResignFirstResponder();
            if (nextResponder != null)
                nextResponder.BecomeFirstResponder();
            return false;
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            base.PrepareForSegue(segue, sender);
            if (segue.DestinationViewController.Class.Name == "ProcessViewController")
            {
                var viewController = (ProcessViewController)segue.DestinationViewController;
                viewController.baseurl = baseurl;
                viewController.userid = useridTxt.Text;
                viewController.password = passwordTxt.Text;
                viewController.username = username;
                viewController.questionnaires = questionnaires;
                viewController.gender = gender;
                viewController.birthdate = birthdate;
                viewController.language = language;
                viewController.email = email;
                viewController.datapath = datapath;
                viewController.level = level;
            }
            if (segue.DestinationViewController.Class.Name == "RegisterViewController")
            {
                var viewController = (RegisterViewController)segue.DestinationViewController;
                viewController.baseurl = baseurl;
            }
        }

        private async void Login()
        {
            BTProgressHUD.Show("Processing...Please wait...");
            message = new string[] { "user", "in", useridTxt.Text, passwordTxt.Text };
            await Task.Run(() => result = ms.ProcessMessage(message, "GET"));
            BTProgressHUD.Dismiss();
            if (result[1] == "ack")
            {
                string up = useridTxt.Text;
                if (result[8] == "1")
                    up = up + "~" + passwordTxt.Text;
                username = result[2];
                Array a = result[3].Split(',');
                questionnaires = new List<string>();
                for (int i = 0; i < a.Length; i++)
                    questionnaires.Add(a.GetValue(i).ToString());
                gender = result[4];
                birthdate = result[5];
                language = result[6];
                email = result[7];
                level = Convert.ToInt32(result[8]);
                ps.RememberMe(datapath, up, rememberSwh.On);
                PerformSegue("ProcessSegue", this);
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

        private void SetWidth(UIView view, float width)
        {
            CoreGraphics.CGRect frame = view.Frame;
            frame.Width = width;
            view.Frame = frame;
        }
    }
}