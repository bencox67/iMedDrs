using Foundation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UIKit;
using BigTed;
using Newtonsoft.Json;
using System.Linq;

namespace iMedDrs.iOS
{
    public partial class MainViewController : UIViewController
    {
        private string baseurl;
        private string datapath;
        private UserModel user;
        private List<string> questionnaires;
        private string[] message;
        private string[] result;
        private readonly UIAlertView alertView;
        private MServer ms;

        [Action("UnwindToMainViewController:")]
        public void UnwindToMainViewController(UIStoryboardSegue segue)
        {
            _ = segue;
        }

        public MainViewController(IntPtr handle) : base(handle)
        {
            alertView = new UIAlertView();
            alertView.AddButton("Ok");
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        public override void ViewWillAppear(bool animated)
        {
            versionLbl.Text = "Version " + NSBundle.MainBundle.InfoDictionary["CFBundleShortVersionString"].ToString();
            baseurl = "https://imeddrs.com/api";
            datapath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            ms = new MServer(baseurl);
            base.ViewWillAppear(animated);
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            base.PrepareForSegue(segue, sender);
            GetUserData();
            if (user != null)
            {
                if (segue.DestinationViewController.Class.Name == "ProcessViewController")
                {
                    var viewController = (ProcessViewController)segue.DestinationViewController;
                    viewController.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
                    viewController.Baseurl = baseurl;
                    viewController.Userid = user.Id.Value.ToString();
                    viewController.Username = user.Name;
                    viewController.Questionnaires = questionnaires;
                    viewController.Gender = user.Gender;
                    viewController.Birthdate = user.Birthdate.ToShortDateString();
                    viewController.Language = user.Language;
                    viewController.Languages = user.LanguageList.ToList();
                    viewController.Email = user.Email;
                    viewController.Datapath = datapath;
                    viewController.Role = user.Role;
                }
                if (segue.DestinationViewController.Class.Name == "LoginViewController")
                {
                    var viewController = (LoginViewController)segue.DestinationViewController;
                    viewController.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
                    viewController.Baseurl = baseurl;
                    viewController.Datapath = datapath;
                    viewController.Languages = user.LanguageList.ToList();
                }
            }
        }

        partial void StartBtn_TouchUpInside(UIButton sender)
        {
            _ = sender;
            PerformSegue("StartSegue", this);
        }

        private void GetUserData()
        {
            BTProgressHUD.Show("Processing...Please wait...");
            message = new string[] { "users", "demo", "1234" };
            Task.Run(() => result = ms.ProcessMessage(message, "GET", "")).Wait();
            BTProgressHUD.Dismiss();
            if (result[0] == "ack")
            {
                user = JsonConvert.DeserializeObject<UserModel>(result[1]);
                questionnaires = new List<string>();
                foreach (var item in user.QuestionnaireList)
                {
                    questionnaires.Add(item.Name);
                }
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