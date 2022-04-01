using Foundation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UIKit;
using BigTed;
using Newtonsoft.Json;

namespace iMedDrs.iOS
{
    public partial class LoginViewController : UIViewController
    {
        public string Baseurl { get; set; }
        public string Datapath { get; set; }
        public string Role { get; set; }
        public List<string> Languages { get; set; }
        private UserModel user;
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
            if (useridTxt.Text != "" && passwordTxt.Text != "")
                Login();
        }

        partial void ResetpwdBtn_TouchUpInside(UIButton sender)
        {
            _ = sender;
            if (useridTxt.Text != "")
                Reset();
            else
                AlertMessage("Enter your email address first!");
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
                viewController.Userid = user.Id.Value.ToString();
                viewController.Password = passwordTxt.Text;
                viewController.Username = user.Name;
                viewController.Questionnaires = questionnaires;
                viewController.Gender = user.Gender;
                viewController.Birthdate = user.Birthdate.ToShortDateString();
                viewController.Language = user.Language;
                viewController.Email = user.Email;
                viewController.Datapath = Datapath;
                viewController.Role = user.Role;
                viewController.Languages = Languages;
            }
            if (segue.DestinationViewController.Class.Name == "RegisterViewController")
            {
                var viewController = (RegisterViewController)segue.DestinationViewController;
                viewController.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
                viewController.Baseurl = Baseurl;
                viewController.Languages = Languages.ToArray();
            }
        }

        private async void Login()
        {
            BTProgressHUD.Show("Processing...Please wait...");
            message = new string[] { "users", useridTxt.Text, passwordTxt.Text };
            await Task.Run(() => result = ms.ProcessMessage(message, "GET", ""));
            BTProgressHUD.Dismiss();
            if (result[0] == "ack")
            {
                user = JsonConvert.DeserializeObject<UserModel>(result[1]);
                if (user.Id != null)
                {
                    ps.RememberMe(Datapath, user.Email, rememberSwh.On);
                    questionnaires = new List<string>();
                    foreach (var item in user.QuestionnaireList)
                    {
                        questionnaires.Add(item.Name);
                    }
                    PerformSegue("ProcessSegue", this);
                }
                else
                    AlertMessage("Log in failed");
            }
            else
                AlertMessage(result[1]);
        }

        private async void Reset()
        {
            BTProgressHUD.Show("Processing...Please wait...");
            message = new string[] { "users", "reset", useridTxt.Text };
            await Task.Run(() => result = ms.ProcessMessage(message, "GET", ""));
            BTProgressHUD.Dismiss();
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