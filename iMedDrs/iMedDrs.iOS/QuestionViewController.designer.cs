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
    [Register ("QuestionViewController")]
    partial class QuestionViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel instructionsLbl { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextView line6Txt { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel nameLbl { get; set; }

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
        UIKit.UIButton questionBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel recoLbl { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIPickerView responsePkr { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UISegmentedControl responseSmc { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField responseTxt { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton returnBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIScrollView scrollView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton speakBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton voiceBtn { get; set; }

        [Action ("NextBtn_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void NextBtn_TouchUpInside (UIKit.UIButton sender);

        [Action ("PreviousBtn_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void PreviousBtn_TouchUpInside (UIKit.UIButton sender);

        [Action ("ReturnBtn_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void ReturnBtn_TouchUpInside (UIKit.UIButton sender);

        [Action ("SpeakBtn_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void SpeakBtn_TouchUpInside (UIKit.UIButton sender);

        [Action ("VoiceBtn_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void VoiceBtn_TouchUpInside (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (instructionsLbl != null) {
                instructionsLbl.Dispose ();
                instructionsLbl = null;
            }

            if (line6Txt != null) {
                line6Txt.Dispose ();
                line6Txt = null;
            }

            if (nameLbl != null) {
                nameLbl.Dispose ();
                nameLbl = null;
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

            if (questionBtn != null) {
                questionBtn.Dispose ();
                questionBtn = null;
            }

            if (recoLbl != null) {
                recoLbl.Dispose ();
                recoLbl = null;
            }

            if (responsePkr != null) {
                responsePkr.Dispose ();
                responsePkr = null;
            }

            if (responseSmc != null) {
                responseSmc.Dispose ();
                responseSmc = null;
            }

            if (responseTxt != null) {
                responseTxt.Dispose ();
                responseTxt = null;
            }

            if (returnBtn != null) {
                returnBtn.Dispose ();
                returnBtn = null;
            }

            if (scrollView != null) {
                scrollView.Dispose ();
                scrollView = null;
            }

            if (speakBtn != null) {
                speakBtn.Dispose ();
                speakBtn = null;
            }

            if (voiceBtn != null) {
                voiceBtn.Dispose ();
                voiceBtn = null;
            }
        }
    }
}