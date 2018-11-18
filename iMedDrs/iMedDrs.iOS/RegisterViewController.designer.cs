// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace iMedDrs.iOS
{
    [Register ("RegisterViewController")]
    partial class RegisterViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField birthdateTxt { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField emailTxt { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UISegmentedControl genderSmc { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField languageTxt { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextView line3Txt { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField nameTxt { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField password1Txt { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField password2Txt { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton registerBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton returnBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIScrollView scrollView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField useridTxt { get; set; }

        [Action ("RegisterBtn_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void RegisterBtn_TouchUpInside (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (birthdateTxt != null) {
                birthdateTxt.Dispose ();
                birthdateTxt = null;
            }

            if (emailTxt != null) {
                emailTxt.Dispose ();
                emailTxt = null;
            }

            if (genderSmc != null) {
                genderSmc.Dispose ();
                genderSmc = null;
            }

            if (languageTxt != null) {
                languageTxt.Dispose ();
                languageTxt = null;
            }

            if (line3Txt != null) {
                line3Txt.Dispose ();
                line3Txt = null;
            }

            if (nameTxt != null) {
                nameTxt.Dispose ();
                nameTxt = null;
            }

            if (password1Txt != null) {
                password1Txt.Dispose ();
                password1Txt = null;
            }

            if (password2Txt != null) {
                password2Txt.Dispose ();
                password2Txt = null;
            }

            if (registerBtn != null) {
                registerBtn.Dispose ();
                registerBtn = null;
            }

            if (returnBtn != null) {
                returnBtn.Dispose ();
                returnBtn = null;
            }

            if (scrollView != null) {
                scrollView.Dispose ();
                scrollView = null;
            }

            if (useridTxt != null) {
                useridTxt.Dispose ();
                useridTxt = null;
            }
        }
    }
}