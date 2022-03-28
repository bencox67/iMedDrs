using Foundation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using UIKit;
using BigTed;
using Newtonsoft.Json;

namespace iMedDrs.iOS
{
    public partial class UserViewController : UIViewController
    {
        public string Baseurl { get; set; }
        public string Userid { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Gender { get; set; }
        public string Birthdate { get; set; }
        public string Language { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool Updated { get; set; }
        public List<string> Languages { get; set; }
        public UILabel SelectedLbl;
        private string[] message;
        private string[] result;
        private readonly UIAlertView alertView;
        private UIDatePicker datePicker;
        private UIPickerView pickerView;
        private MServer ms;

        public UserViewController (IntPtr handle) : base (handle)
        {
            alertView = new UIAlertView();
            alertView.AddButton("Ok");
            SelectedLbl = new UILabel();
            Languages = new List<string>();
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, OnKeyboardNotification);
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, OnKeyboardNotification);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            UITextAttributes uITextAttributes = new UITextAttributes
            {
                TextColor = UIColor.Black
            };
            genderSmc.SetTitleTextAttributes(uITextAttributes, UIControlState.Normal);
            datePicker = new UIDatePicker
            {
                BackgroundColor = UIColor.White,
                Mode = UIDatePickerMode.Date,
                Frame = new RectangleF(0.0f, 0.0f, (float)this.View.Frame.Size.Width, 100.0f),
                PreferredDatePickerStyle = UIDatePickerStyle.Wheels
            };
            UIToolbar toolbar1 = new UIToolbar(new RectangleF(0.0f, 0.0f, (float)View.Frame.Size.Width, 44.0f))
            {
                Items = new UIBarButtonItem[]{
                new UIBarButtonItem(UIBarButtonSystemItem.Cancel, delegate { birthdateTxt.ResignFirstResponder(); }),
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem(UIBarButtonSystemItem.Done, BirthDate)
            }
            };
            birthdateTxt.InputView = datePicker;
            birthdateTxt.InputAccessoryView = toolbar1;
            nameTxt.ShouldReturn += TextFieldShouldReturn;
            birthdateTxt.ShouldReturn += TextFieldShouldReturn;
            password1Txt.ShouldReturn += TextFieldShouldReturn;
            password2Txt.ShouldReturn += TextFieldShouldReturn;
            var tap = new UITapGestureRecognizer { CancelsTouchesInView = false };
            tap.AddTarget(() => View.EndEditing(true));
            View.AddGestureRecognizer(tap);
            ms = new MServer(Baseurl);
            useridTxt.Text = Email;
            useridTxt.Enabled = false;
            nameTxt.Text = Username;
            birthdateTxt.Text = Birthdate;
            datePicker.SetDate((NSDate)DateTime.SpecifyKind(Convert.ToDateTime(Birthdate), DateTimeKind.Local), false);
            for (int i = 0; i < genderSmc.NumberOfSegments; i++)
            {
                if (genderSmc.TitleAt(i) == Gender)
                    genderSmc.SelectedSegment = i;
            }
            SelectedLbl.Text = Language;
            languageTxt.Text = Language;
            pickerView = new UIPickerView
            {
                BackgroundColor = UIColor.White,
                Model = new PickerModel(Languages, SelectedLbl)
            };
            UIToolbar toolbar2 = new UIToolbar(new RectangleF(0.0f, 0.0f, (float)View.Frame.Size.Width, 44.0f))
            {
                Items = new UIBarButtonItem[]{
                    new UIBarButtonItem(UIBarButtonSystemItem.Cancel, delegate { languageTxt.ResignFirstResponder(); }),
                    new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                    new UIBarButtonItem(UIBarButtonSystemItem.Done, LanguageList)
                }
            };
            languageTxt.InputView = pickerView;
            languageTxt.InputAccessoryView = toolbar2;
            Updated = false;
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
            _ = sender;
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

        public void LanguageList(object sender, EventArgs e)
        {
            languageTxt.Text = SelectedLbl.Text;
            languageTxt.ResignFirstResponder();
            password1Txt.BecomeFirstResponder();
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
            if (result && password1Txt.Text != "" && (password1Txt.Text != password2Txt.Text || password1Txt.Text.Length < 4))
            {
                error = "Check Password";
                result = false;
            }
            AlertMessage(error);
            return result;
        }

        private async void Update()
        {
            var names = nameTxt.Text.Split(' ');
            message = new string[] { "users" };
            UserModel user = new UserModel()
            {
                Id = Convert.ToInt32(Userid),
                Email = useridTxt.Text,
                FirstName = names[0],
                LastName = names[1],
                Gender = genderSmc.TitleAt(genderSmc.SelectedSegment),
                Birthdate = Convert.ToDateTime(birthdateTxt.Text),
                Language = SelectedLbl.Text,
                Role = Role,
                Password1 = password1Txt.Text,
                Password2 = password2Txt.Text,
            };
            string json = JsonConvert.SerializeObject(user);
            await Task.Run(() => result = ms.ProcessMessage(message, "PUT", json));
            BTProgressHUD.Dismiss();
            if (result[0] == "ack")
            {
                password1Txt.Text = "";
                password2Txt.Text = "";
                AlertMessage("Update Complete");
                var viewController = (ProcessViewController)this.PresentingViewController;
                viewController.Username = nameTxt.Text;
                viewController.Gender = genderSmc.TitleAt(genderSmc.SelectedSegment);
                viewController.Birthdate = birthdateTxt.Text;
                viewController.Language = SelectedLbl.Text;
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

        private class PickerModel : UIPickerViewModel
        {
            readonly List<string> list;
            readonly UILabel label;

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
                return list != null ? list.Count : 0;
            }

            public override string GetTitle(UIPickerView pickerView, nint row, nint component)
            {
                return list != null ? list[(int)row] : "";
            }

            public override nfloat GetRowHeight(UIPickerView pickerView, nint component)
            {
                return (nfloat)20;
            }

            public override UIView GetView(UIPickerView pickerView, nint row, nint component, UIView view)
            {
                UILabel lbl = new UILabel(new RectangleF(0, 0, 130f, 40f))
                {
                    TextColor = UIColor.Black,
                    Font = UIFont.SystemFontOfSize(17f),
                    TextAlignment = UITextAlignment.Center,
                    Text = list != null ? list[(int)row] : ""
                };
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
            if (password1Txt.IsFirstResponder || password2Txt.IsFirstResponder)
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