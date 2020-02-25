using Foundation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UIKit;
using BigTed;

namespace iMedDrs.iOS
{
    public partial class MainViewController : UIViewController
    {
        private string baseurl;
        private string userid;
        private string username;
        private string password;
        private string gender;
        private string birthdate;
        private string language;
        private string email;
        private string datapath;
        private int level;
        private List<string> questionnaires;
        private string[] message;
        private string[] result;
        private readonly UIAlertView alertView;
        private MServer ms;
        private PServer ps;

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
            versionLbl.Text = "Version " + NSBundle.MainBundle.InfoDictionary["CFBundleShortVersionString"].ToString();
            baseurl = "https://imeddrs.com/beacon/api";
            datapath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            ms = new MServer(baseurl);
            ps = new PServer();
            userid = ps.RememberMe(datapath, "", true);
            if (userid.Contains("~"))
            {
                string[] up = userid.Split('~');
                userid = up[0];
                password = up[1];
                main1Lbl.Hidden = true;
                main2Lbl.Hidden = true;
                loginBtn.Hidden = true;
            }
            else
            {
                userid = "demo";
                password = "1234";
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
            if (userid != "demo")
                Start();
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            base.PrepareForSegue(segue, sender);
            if (segue.DestinationViewController.Class.Name == "ProcessViewController")
            {
                var viewController = (ProcessViewController)segue.DestinationViewController;
                viewController.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
                viewController.baseurl = baseurl;
                viewController.userid = userid;
                viewController.username = username;
                viewController.password = password;
                viewController.questionnaires = questionnaires;
                viewController.gender = gender;
                viewController.birthdate = birthdate;
                viewController.language = language;
                viewController.email = email;
                viewController.datapath = datapath;
                viewController.level = level;
            }
            if (segue.DestinationViewController.Class.Name == "LoginViewController")
            {
                var viewController = (LoginViewController)segue.DestinationViewController;
                viewController.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
                viewController.baseurl = baseurl;
                viewController.datapath = datapath;
                viewController.level = level;
            }
        }

        partial void StartBtn_TouchUpInside(UIButton sender)
        {
            _ = sender;
            Start();
        }

        private async void Start()
        {
            message = new string[] { "user", "in", userid, password };
            BTProgressHUD.Show("Processing...Please wait...");
            await Task.Run(() => result = ms.ProcessMessage(message, "GET"));
            BTProgressHUD.Dismiss();
            if (result[1] == "ack")
            {
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
                PerformSegue("StartSegue", this);
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