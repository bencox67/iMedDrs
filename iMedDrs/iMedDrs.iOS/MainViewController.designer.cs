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
    [Register ("MainViewController")]
    partial class MainViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextView line1Txt { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton loginBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel main1Lbl { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel main2Lbl { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel mainLbl { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton startBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel versionLbl { get; set; }

        [Action ("StartBtn_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void StartBtn_TouchUpInside (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (line1Txt != null) {
                line1Txt.Dispose ();
                line1Txt = null;
            }

            if (loginBtn != null) {
                loginBtn.Dispose ();
                loginBtn = null;
            }

            if (main1Lbl != null) {
                main1Lbl.Dispose ();
                main1Lbl = null;
            }

            if (main2Lbl != null) {
                main2Lbl.Dispose ();
                main2Lbl = null;
            }

            if (mainLbl != null) {
                mainLbl.Dispose ();
                mainLbl = null;
            }

            if (startBtn != null) {
                startBtn.Dispose ();
                startBtn = null;
            }

            if (versionLbl != null) {
                versionLbl.Dispose ();
                versionLbl = null;
            }
        }
    }
}