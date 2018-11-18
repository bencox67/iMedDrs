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
    [Register ("SendViewController")]
    partial class SendViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel instructionsLbl { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextView line8Txt { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextView locationsTxt { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton myemailBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel namequestLbl { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton returnBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton sendBtn { get; set; }

        [Action ("MyemailBtn_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void MyemailBtn_TouchUpInside (UIKit.UIButton sender);

        [Action ("SendBtn_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void SendBtn_TouchUpInside (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (instructionsLbl != null) {
                instructionsLbl.Dispose ();
                instructionsLbl = null;
            }

            if (line8Txt != null) {
                line8Txt.Dispose ();
                line8Txt = null;
            }

            if (locationsTxt != null) {
                locationsTxt.Dispose ();
                locationsTxt = null;
            }

            if (myemailBtn != null) {
                myemailBtn.Dispose ();
                myemailBtn = null;
            }

            if (namequestLbl != null) {
                namequestLbl.Dispose ();
                namequestLbl = null;
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