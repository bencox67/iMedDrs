using Foundation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UIKit;
using BigTed;
using Newtonsoft.Json;
using WebKit;

namespace iMedDrs.iOS
{
    public partial class ReportViewController : UIViewController
    {
        public string Baseurl { get; set; }
        public string Userid { get; set; }
        public string Username { get; set; }
        public string Questionnaire { get; set; }
        public List<Report> Reports { get; set; }
        public string Data { get; set; }
        public string Email { get; set; }
        public int Last { get; set; }
        public int Number { get; set; }
        public string Role { get; set; }
        public string Language { get; set; }

        [Action("UnwindToReportViewController:")]
        public void UnwindToReportViewController(UIStoryboardSegue segue)
        {
            _ = segue;
        }

        public ReportViewController (IntPtr handle) : base (handle)
        {
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
            namequestLbl.Text = Username + " - " + Questionnaire;
            reportWv.LoadHtmlString(Reports[Number].Text, null);
            base.ViewWillAppear(animated);
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
            reportWv.LoadHtmlString(Reports[Number].Text, null);
        }

        partial void PreviousBtn_TouchUpInside(UIButton sender)
        {
            _ = sender;
            Number--;
            if (Number < 0)
                Number = Last - 1;
            reportWv.LoadHtmlString(Reports[Number].Text, null);
        }

        partial void SendBtn_TouchUpInside(UIButton sender)
        {
            _ = sender;
        }
    }
}