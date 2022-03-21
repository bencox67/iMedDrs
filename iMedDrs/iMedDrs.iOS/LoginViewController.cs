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
        public string Baseurl { get; set; }
        public string Datapath { get; set; }
        public int Level { get; set; }
        private string username;
        private string gender;
        private string birthdate;
        private string language;
        private string email;
        private List<string> questionnaires;
        private string[] message;
        private string[] result;
        private readonly UIAlertView alertView;
        private MServer ms;
        private PServer ps;

        [Action("UnwindToLoginViewController:")]
        public void UnwindToLoginViewController(UIStoryboardSegue segue)
        {
            _ = segue;
            passwordTxt.Text = "";
        }

        public LoginViewController (IntPtr handle) : base (handle)
        {
            alertView = new UIAlertView();
            alertView.AddButton("Ok");
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            useridTxt.ShouldReturn += TextFieldShouldReturn;
            passwordTxt.ShouldReturn += TextFieldShouldReturn;
            var tap = new UITapGestureRecognizer { CancelsTouchesInView = false };
            tap.AddTarget(() => View.EndEditing(true));
            View.AddGestureRecognizer(tap);
            ms = new MServer(Baseurl);
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
            useridTxt.Text = ps.RememberMe(Datapath, "", true);
            if (useridTxt.Text != "")
                rememberSwh.On = true;
        }

        public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            base.TraitCollectionDidChange(previousTraitCollection);
            if (this.TraitCollection.UserInterfaceStyle != previousTraitCollection.UserInterfaceStyle)
            {
            }
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            View.EndEditing(true);
        }

        partial void LoginBtn_TouchUpInside(UIButton sender)
        {
            _ = sender;
            if (useridTxt.Text != "" && passwordTxt.Text != "" && useridTxt.Text.ToLower() != "demo")
                Login();
        }

        partial void ResetpwdBtn_TouchUpInside(UIButton sender)
        {
            _ = sender;
            if (useridTxt.Text != "")
                Reset();
            else
                AlertMessage("Enter your User ID first!");
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
                viewController.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
                viewController.Baseurl = Baseurl;
                viewController.Userid = useridTxt.Text;
                viewController.Password = passwordTxt.Text;
                viewController.Username = username;
                viewController.Questionnaires = questionnaires;
                viewController.Gender = gender;
                viewController.Birthdate = birthdate;
                viewController.Language = language;
                viewController.Email = email;
                viewController.Datapath = Datapath;
                viewController.Level = Level;
            }
            if (segue.DestinationViewController.Class.Name == "RegisterViewController")
            {
                var viewController = (RegisterViewController)segue.DestinationViewController;
                viewController.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
                viewController.Baseurl = Baseurl;
            }
        }

        private async void Login()
        {
            BTProgressHUD.Show("Processing...Please wait...");
            message = new string[] { "user", "in", useridTxt.Text, passwordTxt.Text };
            await Task.Run(() => result = ms.ProcessMessage(message, "GET", ""));
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
                Level = Convert.ToInt32(result[8]);
                ps.RememberMe(Datapath, up, rememberSwh.On);
                PerformSegue("ProcessSegue", this);
            }
            else
                AlertMessage(result[2]);
        }

        private async void Reset()
        {
            BTProgressHUD.Show("Processing...Please wait...");
            message = new string[] { "user", "reset", useridTxt.Text };
            await Task.Run(() => result = ms.ProcessMessage(message, "GET", ""));
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
    }
}