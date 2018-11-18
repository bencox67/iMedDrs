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
    [Register ("ProcessViewController")]
    partial class ProcessViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UISwitch handsfreeSwh { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextView line4Txt { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton logoutBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton maintainBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIPickerView questionairePkr { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton returnBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton startBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton updateBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel usernameLbl { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton viewBtn { get; set; }

        [Action ("StartBtn_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void StartBtn_TouchUpInside (UIKit.UIButton sender);

        [Action ("ViewBtn_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void ViewBtn_TouchUpInside (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (handsfreeSwh != null) {
                handsfreeSwh.Dispose ();
                handsfreeSwh = null;
            }

            if (line4Txt != null) {
                line4Txt.Dispose ();
                line4Txt = null;
            }

            if (logoutBtn != null) {
                logoutBtn.Dispose ();
                logoutBtn = null;
            }

            if (maintainBtn != null) {
                maintainBtn.Dispose ();
                maintainBtn = null;
            }

            if (questionairePkr != null) {
                questionairePkr.Dispose ();
                questionairePkr = null;
            }

            if (returnBtn != null) {
                returnBtn.Dispose ();
                returnBtn = null;
            }

            if (startBtn != null) {
                startBtn.Dispose ();
                startBtn = null;
            }

            if (updateBtn != null) {
                updateBtn.Dispose ();
                updateBtn = null;
            }

            if (usernameLbl != null) {
                usernameLbl.Dispose ();
                usernameLbl = null;
            }

            if (viewBtn != null) {
                viewBtn.Dispose ();
                viewBtn = null;
            }
        }
    }
}