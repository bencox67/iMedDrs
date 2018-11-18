using Foundation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using UIKit;
using BigTed;

namespace iMedDrs.iOS
{
    public partial class UserViewController : UIViewController
    {
        public string baseurl { get; set; }
        public string userid { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string gender { get; set; }
        public string birthdate { get; set; }
        public string language { get; set; }
        public string email { get; set; }
        public bool updated { get; set; }
        public List<string> languages { get; set; }
        public UILabel selectedLbl;
        private string newpassword;
        private string emailaddr;
        private string[] message;
        private string[] result;
        private UIAlertView alertView;
        private UIAlertView progressView;
        private UIDatePicker datePicker;
        private UIPickerView pickerView;
        private MServer ms;

        public UserViewController (IntPtr handle) : base (handle)
        {
            alertView = new UIAlertView();
            alertView.AddButton("Ok");
            progressView = new UIAlertView();
            progressView.Title = "Processing... Please Wait...";
            selectedLbl = new UILabel();
            languages = new List<string>();
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, OnKeyboardNotification);
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, OnKeyboardNotification);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            if (UIScreen.MainScreen.Bounds.Width == 320)
            {
                SetWidth(line5Txt, 320.0f);
                SetWidth(useridTxt, 210.0f);
                SetWidth(nameTxt, 210.0f);
                SetWidth(languageTxt, 210.0f);
                SetWidth(emailTxt, 210.0f);
                SetWidth(password1Txt, 210.0f);
                SetWidth(password2Txt, 210.0f);
                SetWidth(updateBtn, 300.0f);
                SetWidth(returnBtn, 300.0f);
            }
            datePicker = new UIDatePicker();
            datePicker.BackgroundColor = UIColor.White;
            datePicker.Mode = UIDatePickerMode.Date;
            UIToolbar toolbar1 = new UIToolbar(new RectangleF(0.0f, 0.0f, (float)View.Frame.Size.Width, 44.0f));
            toolbar1.Items = new UIBarButtonItem[]{
                new UIBarButtonItem(UIBarButtonSystemItem.Cancel, delegate { birthdateTxt.ResignFirstResponder(); }),
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem(UIBarButtonSystemItem.Done, BirthDate)
            };
            birthdateTxt.InputView = datePicker;
            birthdateTxt.InputAccessoryView = toolbar1;
            nameTxt.ShouldReturn += TextFieldShouldReturn;
            birthdateTxt.ShouldReturn += TextFieldShouldReturn;
            emailTxt.ShouldReturn += TextFieldShouldReturn;
            password1Txt.ShouldReturn += TextFieldShouldReturn;
            password2Txt.ShouldReturn += TextFieldShouldReturn;
            var tap = new UITapGestureRecognizer { CancelsTouchesInView = false };
            tap.AddTarget(() => View.EndEditing(true));
            View.AddGestureRecognizer(tap);
            ms = new MServer(baseurl);
            useridTxt.Text = userid;
            nameTxt.Text = username;
            birthdateTxt.Text = birthdate;
            datePicker.SetDate((NSDate)DateTime.SpecifyKind(Convert.ToDateTime(birthdate), DateTimeKind.Local), false);
            for (int i = 0; i < genderSmc.NumberOfSegments; i++)
            {
                if (genderSmc.TitleAt(i) == gender)
                    genderSmc.SelectedSegment = i;
            }
            selectedLbl.Text = language;
            languageTxt.Text = language;
            pickerView = new UIPickerView();
            pickerView.BackgroundColor = UIColor.White;
            pickerView.Model = new PickerModel(languages, selectedLbl);
            UIToolbar toolbar2 = new UIToolbar(new RectangleF(0.0f, 0.0f, (float)View.Frame.Size.Width, 44.0f));
            toolbar2.Items = new UIBarButtonItem[]{
                    new UIBarButtonItem(UIBarButtonSystemItem.Cancel, delegate { languageTxt.ResignFirstResponder(); }),
                    new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                    new UIBarButtonItem(UIBarButtonSystemItem.Done, Language)
                };
            languageTxt.InputView = pickerView;
            languageTxt.InputAccessoryView = toolbar2;
            emailTxt.Text = email;
            updated = false;
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
        }

        partial void UpdateBtn_TouchUpInside(UIButton sender)
        {
            if (ValidateData())
                Update();
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

        public void BirthDate(object sender, EventArgs e)
        {
            birthdateTxt.Text = Convert.ToDateTime(datePicker.Date.ToString()).ToString("MM/dd/yyyy");
            birthdateTxt.ResignFirstResponder();
            languageTxt.BecomeFirstResponder();
        }

        public void Language(object sender, EventArgs e)
        {
            languageTxt.Text = selectedLbl.Text;
            languageTxt.ResignFirstResponder();
            emailTxt.BecomeFirstResponder();
        }

        private bool ValidateData()
        {
            bool result = true;
            string error = "";
            if (result && nameTxt.Text.Split(' ').Length < 2)
            {
                error = "Check Name";
                result = false;
            }
            try { DateTime dt = Convert.ToDateTime(birthdateTxt.Text); }
            catch
            {
                if (result)
                {
                    error = "Check Birth Date";
                    result = false;
                }
            }
            newpassword = "~";
            if (result && password1Txt.Text != "" && (password1Txt.Text != password2Txt.Text || password1Txt.Text.Length < 4))
            {
                error = "Check Password";
                result = false;
            }
            if (emailTxt.Text == "")
                emailaddr = "~";
            else
                emailaddr = emailTxt.Text;
            if (password1Txt.Text == "")
                newpassword = "~";
            else
                newpassword = password1Txt.Text;
            AlertMessage(error);
            return result;
        }

        private async void Update()
        {
            BTProgressHUD.Show("Processing...Please wait...");
            message = new string[] { "user", "change", useridTxt.Text, nameTxt.Text, genderSmc.TitleAt(genderSmc.SelectedSegment), birthdateTxt.Text.Replace("/", "|"), selectedLbl.Text, emailaddr, password, newpassword };
            await Task.Run(() => result = ms.ProcessMessage(message, "GET"));
            BTProgressHUD.Dismiss();
            if (result[1] == "ack")
            {
                password1Txt.Text = "";
                password2Txt.Text = "";
                AlertMessage("Update Complete");
                var viewController = (ProcessViewController)this.PresentingViewController;
                viewController.username = nameTxt.Text;
                viewController.gender = genderSmc.TitleAt(genderSmc.SelectedSegment);
                viewController.birthdate = birthdateTxt.Text;
                viewController.language = selectedLbl.Text;
                viewController.email = emailTxt.Text;
                if (newpassword != "~")
                    viewController.password = newpassword;
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

        private class PickerModel : UIPickerViewModel
        {
            List<string> list;
            UILabel label;

            public PickerModel(List<string> list, UILabel label)
            {
                this.list = list;
                this.label = label;
            }

            public override nint GetComponentCount(UIPickerView pickerView)
            {
                return 1;
            }

            public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
            {
                return list.Count;
            }

            public override string GetTitle(UIPickerView pickerView, nint row, nint component)
            {
                return list[(int)row];
            }

            public override nfloat GetRowHeight(UIPickerView pickerView, nint component)
            {
                return (nfloat)20;
            }

            public override UIView GetView(UIPickerView pickerView, nint row, nint component, UIView view)
            {
                UILabel lbl = new UILabel(new RectangleF(0, 0, 130f, 40f));
                lbl.TextColor = UIColor.Black;
                lbl.Font = UIFont.SystemFontOfSize(17f);
                lbl.TextAlignment = UITextAlignment.Center;
                lbl.Text = list[(int)row];
                return lbl;
            }

            public override void Selected(UIPickerView pickerView, nint row, nint component)
            {
                label.Text = list[(int)row];
            }
        }

        private void OnKeyboardNotification(NSNotification notification)
        {
            if (!IsViewLoaded) return;
            var visible = notification.Name == UIKeyboard.WillShowNotification;
            UIView.BeginAnimations("AnimateForKeyboard");
            UIView.SetAnimationBeginsFromCurrentState(true);
            UIView.SetAnimationDuration(UIKeyboard.AnimationDurationFromNotification(notification));
            UIView.SetAnimationCurve((UIViewAnimationCurve)UIKeyboard.AnimationCurveFromNotification(notification));
            bool landscape = InterfaceOrientation == UIInterfaceOrientation.LandscapeLeft || InterfaceOrientation == UIInterfaceOrientation.LandscapeRight;
            var keyboardFrame = visible ? UIKeyboard.FrameEndFromNotification(notification) : UIKeyboard.FrameBeginFromNotification(notification);
            OnKeyboardChanged(visible, landscape ? keyboardFrame.Width : keyboardFrame.Height);
            UIView.CommitAnimations();
        }

        protected virtual void OnKeyboardChanged(bool visible, nfloat keyboardHeight)
        {
            bool restore = false;
            UIView firstResponder = null;
            if (emailTxt.IsFirstResponder || password1Txt.IsFirstResponder || password2Txt.IsFirstResponder)
                firstResponder = password1Txt;
            else
                restore = true;
            var activeView = scrollView ?? firstResponder;
            if (activeView == null)
                return;
            if (!visible || restore)
                RestoreScrollPosition(scrollView);
            else
                CenterViewInScroll(activeView, scrollView, (float)keyboardHeight);
        }

        protected virtual void CenterViewInScroll(UIView viewToCenter, UIScrollView scrollView, float keyboardHeight)
        {
            var contentInsets = new UIEdgeInsets(0.0f, 0.0f, keyboardHeight, 0.0f);
            scrollView.ContentInset = contentInsets;
            scrollView.ScrollIndicatorInsets = contentInsets;

            // Position of the active field relative isnside the scroll view
            RectangleF relativeFrame = (RectangleF)viewToCenter.Superview.ConvertRectToView(viewToCenter.Frame, scrollView);

            bool landscape = InterfaceOrientation == UIInterfaceOrientation.LandscapeLeft || InterfaceOrientation == UIInterfaceOrientation.LandscapeRight;
            var spaceAboveKeyboard = (landscape ? scrollView.Frame.Width : scrollView.Frame.Height) - keyboardHeight;

            // Move the active field to the center of the available space
            var offset = relativeFrame.Y - (spaceAboveKeyboard - viewToCenter.Frame.Height) / 2;
            scrollView.ContentOffset = new PointF(0, (float)offset);
        }

        protected virtual void RestoreScrollPosition(UIScrollView scrollView)
        {
            scrollView.ContentInset = UIEdgeInsets.Zero;
            scrollView.ScrollIndicatorInsets = UIEdgeInsets.Zero;
        }
    }
}