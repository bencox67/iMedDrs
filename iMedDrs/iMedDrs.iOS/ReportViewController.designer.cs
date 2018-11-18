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
    [Register ("ReportViewController")]
    partial class ReportViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextView line7Txt { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel namequestLbl { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton nextBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton previousBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIWebView reportWv { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton returnBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton sendBtn { get; set; }

        [Action ("NextBtn_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void NextBtn_TouchUpInside (UIKit.UIButton sender);

        [Action ("PreviousBtn_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void PreviousBtn_TouchUpInside (UIKit.UIButton sender);

        [Action ("SendBtn_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void SendBtn_TouchUpInside (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (line7Txt != null) {
                line7Txt.Dispose ();
                line7Txt = null;
            }

            if (namequestLbl != null) {
                namequestLbl.Dispose ();
                namequestLbl = null;
            }

            if (nextBtn != null) {
                nextBtn.Dispose ();
                nextBtn = null;
            }

            if (previousBtn != null) {
                previousBtn.Dispose ();
                previousBtn = null;
            }

            if (reportWv != null) {
                reportWv.Dispose ();
                reportWv = null;
            }

            if (returnBtn != null) {
                returnBtn.Dispose ();
                returnBtn = null;
            }

            if (sendBtn != null) {
                sendBtn.Dispose ();
                sendBtn = null;
            }
        }
    }
}