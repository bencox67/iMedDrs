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
    [Register ("MaintainViewController")]
    partial class MaintainViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField languageTxt { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextView line9Txt { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField nameTxt { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton nextBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton playBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton previousBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField questionnaireTxt { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIPickerView questionPkr { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextView questionTxt { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton recordBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton returnBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIScrollView scrollView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton selectBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton updatevoiceBtn { get; set; }

        [Action ("NextBtn_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void NextBtn_TouchUpInside (UIKit.UIButton sender);

        [Action ("PlayBtn_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void PlayBtn_TouchUpInside (UIKit.UIButton sender);

        [Action ("PreviousBtn_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void PreviousBtn_TouchUpInside (UIKit.UIButton sender);

        [Action ("RecordBtn_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void RecordBtn_TouchUpInside (UIKit.UIButton sender);

        [Action ("SelectBtn_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void SelectBtn_TouchUpInside (UIKit.UIButton sender);

        [Action ("UpdatevoiceBtn_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void UpdatevoiceBtn_TouchUpInside (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (languageTxt != null) {
                languageTxt.Dispose ();
                languageTxt = null;
            }

            if (line9Txt != null) {
                line9Txt.Dispose ();
                line9Txt = null;
            }

            if (nameTxt != null) {
                nameTxt.Dispose ();
                nameTxt = null;
            }

            if (nextBtn != null) {
                nextBtn.Dispose ();
                nextBtn = null;
            }

            if (playBtn != null) {
                playBtn.Dispose ();
                playBtn = null;
            }

            if (previousBtn != null) {
                previousBtn.Dispose ();
                previousBtn = null;
            }

            if (questionnaireTxt != null) {
                questionnaireTxt.Dispose ();
                questionnaireTxt = null;
            }

            if (questionPkr != null) {
                questionPkr.Dispose ();
                questionPkr = null;
            }

            if (questionTxt != null) {
                questionTxt.Dispose ();
                questionTxt = null;
            }

            if (recordBtn != null) {
                recordBtn.Dispose ();
                recordBtn = null;
            }

            if (returnBtn != null) {
                returnBtn.Dispose ();
                returnBtn = null;
            }

            if (scrollView != null) {
                scrollView.Dispose ();
                scrollView = null;
            }

            if (selectBtn != null) {
                selectBtn.Dispose ();
                selectBtn = null;
            }

            if (updatevoiceBtn != null) {
                updatevoiceBtn.Dispose ();
                updatevoiceBtn = null;
            }
        }
    }
}