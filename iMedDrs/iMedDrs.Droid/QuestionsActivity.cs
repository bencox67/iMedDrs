using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Speech;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Newtonsoft.Json;

namespace iMedDrs.Droid
{
    [Activity(Name = "com.imeddrs.imeddrs.QuestionsActivity", Label = "iMedDrs - Questionnaire", Icon = "@drawable/Icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@android:style/Theme.Holo.Light")]
    public class QuestionsActivity : Activity, IRecognitionListener
    {
        string baseurl;
        string userid;
        string username;
        string questionnaireid;
        string questionnaire;
        string number;
        string name;
        string text;
        string last;
        string answer;
        string type;
        string datapath;
        string language;
        string extension;
        string instruction;
        string role;
        string[] responses;
        string[] response;
        string[] eresponse;
        string[] message;
        string[] result;
        bool init;
        bool required;
        bool play;
        bool handsfree;
        bool playing;
        TextView instructions;
        TextView qname;
        TextView question;
        EditText response1;
        EditText response2;
        RadioGroup response3;
        Spinner response4;
        DatePicker response5;
        RadioButton[] rb;
        ImageButton speak;
        ImageButton listen;
        Button next;
        Button previous;
        Button returns;
        AlertDialog stop;
        AlertDialog save;
        AlertDialog alert;
        AlertDialog progress;
        MServer ms;
        PServer ps;
        MediaPlayer player;
        SpeechRecognizer speech;
        Intent voice;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Get intent variables
            baseurl = Intent.GetStringExtra("baseurl");
            userid = Intent.GetStringExtra("userid");
            username = Intent.GetStringExtra("username");
            questionnaire = Intent.GetStringExtra("questionnaire");
            questionnaireid = Intent.GetStringExtra("questionnaireid");
            number = Intent.GetStringExtra("number");
            name = Intent.GetStringExtra("name");
            text = Intent.GetStringExtra("text");
            response = Intent.GetStringArrayExtra("response");
            eresponse = Intent.GetStringArrayExtra("eresponse");
            responses = Intent.GetStringArrayExtra("responses");
            last = Intent.GetStringExtra("last");
            required = Intent.GetBooleanExtra("required", false);
            type = Intent.GetStringExtra("type");
            answer = Intent.GetStringExtra("answer");
            role = Intent.GetStringExtra("role");
            datapath = Intent.GetStringExtra("datapath");
            language = Intent.GetStringExtra("language");
            extension = Intent.GetStringExtra("extension");
            instruction = Intent.GetStringExtra("instruction");
            handsfree = Convert.ToBoolean(Intent.GetStringExtra("handsfree"));

            // Set title
            this.ActionBar.Subtitle = username + " - " + questionnaire;

            // Set our view from the "process" layout resource
            SetContentView(Resource.Layout.Questions);

            // Initialize variables
            instructions = FindViewById<TextView>(Resource.Id.instructions);
            qname = FindViewById<TextView>(Resource.Id.qname);
            question = FindViewById<TextView>(Resource.Id.question);
            response1 = FindViewById<EditText>(Resource.Id.response1);
            response2 = FindViewById<EditText>(Resource.Id.response2);
            response3 = FindViewById<RadioGroup>(Resource.Id.response3);
            response4 = FindViewById<Spinner>(Resource.Id.response4);
            response5 = FindViewById<DatePicker>(Resource.Id.response5);
            speak = FindViewById<ImageButton>(Resource.Id.speak);
            listen = FindViewById<ImageButton>(Resource.Id.listen);
            next = FindViewById<Button>(Resource.Id.next);
            previous = FindViewById<Button>(Resource.Id.previous);
            returns = FindViewById<Button>(Resource.Id.returns);
            player = new MediaPlayer();
            var attribs = new AudioAttributes.Builder();
            player.SetAudioAttributes(attribs.SetFlags(AudioFlags.None).SetLegacyStreamType(Android.Media.Stream.Music).Build());
            attribs.Dispose();
            //player.SetAudioStreamType(Android.Media.Stream.Music);
            player.Prepared += Player_Prepared;
            player.Completion += Player_Completion;
            init = true;
            playing = false;
            speak.Visibility = ViewStates.Invisible;
            if (handsfree)
            {
                listen.Visibility = ViewStates.Invisible;
            }

            // Speech Recoginition
            speech = SpeechRecognizer.CreateSpeechRecognizer(this);
            speech.SetRecognitionListener(this);
            voice = new Intent(RecognizerIntent.ActionRecognizeSpeech);
            voice.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
            voice.PutExtra(RecognizerIntent.ExtraCallingPackage, PackageName);
            voice.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 3000);
            voice.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 3000);
            voice.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 3000);
            voice.PutExtra(RecognizerIntent.ExtraMaxResults, 1);

            // Clear response objects
            response1.Visibility = ViewStates.Gone;
            response2.Visibility = ViewStates.Gone;
            response3.Visibility = ViewStates.Gone;
            response4.Visibility = ViewStates.Gone;

            // Initialize events
            speak.Click += Speak_Click;
            listen.Click += Listen_Click;
            response1.FocusChange += Response1_FocusChange;
            response2.FocusChange += Response2_FocusChange;
            next.Click += Next_Click;
            previous.Click += Previous_Click;
            returns.Click += Returns_Click;

            // Alert dialog for stopping questionnaire
            AlertDialog.Builder alertbuilder1 = new AlertDialog.Builder(this);
            alertbuilder1.SetPositiveButton("Yes", OkAction);
            alertbuilder1.SetNegativeButton("No", (EventHandler<DialogClickEventArgs>)null);
            stop = alertbuilder1.Create();
            alertbuilder1.Dispose();

            // Alert dialog for saved questionnaire
            AlertDialog.Builder alertbuilder2 = new AlertDialog.Builder(this);
            alertbuilder2.SetPositiveButton("Ok", OkAction);
            save = alertbuilder2.Create();
            alertbuilder2.Dispose();

            // Alert dialog for messages
            AlertDialog.Builder alertbuilder3 = new AlertDialog.Builder(this);
            alertbuilder3.SetPositiveButton("Ok", (EventHandler<DialogClickEventArgs>)null);
            alert = alertbuilder3.Create();
            alertbuilder3.Dispose();

            // Progress dialog for messaging
            AlertDialog.Builder alertbuilder4 = new AlertDialog.Builder(this);
            alertbuilder4.SetView(LayoutInflater.Inflate(Resource.Layout.Progress, null));
            progress = alertbuilder4.Create();
            progress.SetCancelable(false);
            alertbuilder4.Dispose();

            // Initialize messaging
            ms = new MServer(baseurl);
            ps = new PServer();
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (init)
            {
                SetQuestion();
                SetResponses();
                init = false;
            }
        }

        public override void OnBackPressed()
        {
            return;
        }

        private void Player_Prepared(object sender, EventArgs e)
        {
            play = true;
            if (handsfree)
                PlayQuestion();
        }

        private void Player_Completion(object sender, EventArgs e)
        {
            if (playing)
            {
                playing = false;
                if (handsfree)
                {
                    //if (!report && !init)
                    //    GetVoice();
                    //else
                    //    NextQuestion();
                }
            }
        }

        private void Listen_Click(object sender, EventArgs e)
        {
            PlayQuestion();
        }

        private void Speak_Click(object sender, EventArgs e)
        {
            GetVoice();
        }

        private void OkAction(object sender, DialogClickEventArgs e)
        {
            StopVoiceReco();
            handsfree = false;
            Finish();
        }

        private void Response1_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (e.HasFocus)
                response1.Text = "";
        }

        private void Response2_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (e.HasFocus)
                response2.Text = "";
        }

        private void Next_Click(object sender, EventArgs e)
        {
            StopVoiceReco();
            NextQuestion();
        }

        private void Previous_Click(object sender, EventArgs e)
        {
            StopVoiceReco();
            PreviousQuestion();
        }

        private void Returns_Click(object sender, EventArgs e)
        {
            stop.SetMessage("Stop the questionnaire?");
            stop.Show();
        }

        private async void NextQuestion()
        {
            if (Convert.ToInt32(number) < Convert.ToInt32(last))
            {
                string ans = GetRepsonse();
                if (required && ans == "~")
                    AlertMessage("A response is required");
                else
                {
                    responses[Convert.ToInt32(number) - 1] = ans;
                    progress.Show();
                    string _responses = JsonConvert.SerializeObject(responses);
                    message = new string[] { "questionnaires", questionnaireid, number, userid, "Next", language };
                    await Task.Run(() => result = ms.ProcessMessage(message, "POST", _responses));
                    progress.Dismiss();
                    if (result[0] == "ack")
                    {
                        QuestionnaireModel model = JsonConvert.DeserializeObject<QuestionnaireModel>(result[1]);
                        number = model.Sequence.ToString();
                        name = model.QuestionName;
                        text = model.Question;
                        response = model.ActResponses.ToArray();
                        responses = model.Responses;
                        required = model.Required;
                        type = model.Type;
                        answer = responses[Convert.ToInt32(number) - 1];
                        eresponse = model.EngResponses.ToArray();
                        instruction = model.Instructions;
                        SetQuestion();
                        SetResponses();
                    }
                    else
                        AlertMessage(result[1]);
                }
            }
            else
            {
                if (role != "demo")
                {
                    progress.Show();
                    string _responses = JsonConvert.SerializeObject(responses);
                    message = new string[] { "questionnaires", "save", userid, questionnaireid };
                    await Task.Run(() => result = ms.ProcessMessage(message, "POST", _responses));
                    progress.Dismiss();
                    if (!handsfree)
                        save.SetMessage(result[1]);
                }
                else
                {
                    if (ps.WriteToFile(Path.Combine(datapath, questionnaire.Replace(" ", "_") + ".txt"), String.Join(',', responses), true))
                    {
                        if (!handsfree)
                            save.SetMessage("Questionnaire responses saved");
                    }
                    else
                    {
                        if (!handsfree)
                            save.SetMessage("Not able to save resposnes");
                    }
                }
                if (!handsfree)
                    save.Show();
                else
                    Finish();
            }
        }

        private async void PreviousQuestion()
        {
            progress.Show();
            string _responses = JsonConvert.SerializeObject(responses);
            message = new string[] { "questionnaires", questionnaireid, number, userid, "Previous", language };
            await Task.Run(() => result = ms.ProcessMessage(message, "POST", _responses));
            progress.Dismiss();
            if (result[0] == "ack")
            {
                QuestionnaireModel model = JsonConvert.DeserializeObject<QuestionnaireModel>(result[1]);
                number = model.Sequence.ToString();
                name = model.QuestionName;
                text = model.Question;
                response = model.ActResponses.ToArray();
                responses = model.Responses;
                required = model.Required;
                type = model.Type;
                answer = responses[Convert.ToInt32(number) - 1];
                eresponse = model.EngResponses.ToArray();
                instruction = model.Instructions;
                SetQuestion();
                SetResponses();
            }
            else
                AlertMessage(result[1]);
        }

        private void SetQuestion()
        {
            SetVoice();
            instructions.Text = instruction.Replace("~", "\r\n");
            qname.Text = name;
            question.Text = text;
        }

        private void SetVoice()
        {
            play = false;
            player.Stop();
            player.Reset();
            try
            {
                player.SetDataSource("https://imeddrs.com/data/voice/" + language + "/" + name + extension);
                player.PrepareAsync();
            }
            catch { }
        }

        private void PlayQuestion()
        {
            if (play)
            {
                try
                {
                    player.Start();
                    playing = true;
                }
                catch { }
            }
        }

        private void GetVoice()
        {
            speech.StartListening(voice);
        }

        private void SetResponses()
        {
            response1.Visibility = ViewStates.Gone;
            response2.Visibility = ViewStates.Gone;
            response3.Visibility = ViewStates.Gone;
            response4.Visibility = ViewStates.Gone;
            response5.Visibility = ViewStates.Gone;
            answer ??= "";
            if (name != "End")
            {
                if (response.Length == 0)
                {
                    switch (type)
                    {
                        case "date":
                            if (answer != "")
                                response5.DateTime = Convert.ToDateTime(answer);
                            else
                                response5.DateTime = DateTime.Now;
                            response5.Visibility = ViewStates.Visible;
                            break;
                        case "decimal":
                        case "number":
                            response1.Text = answer;
                            if (name == "Age")
                            {
                                if (response1.Text.StartsWith("0."))
                                    response1.Text = response1.Text[1..];
                                if (response1.Text.EndsWith(".00"))
                                    response1.Text = response1.Text[0..^3];
                            }
                            response1.Visibility = ViewStates.Visible;
                            break;
                        default:
                            response2.Text = answer;
                            response2.Visibility = ViewStates.Visible;
                            break;
                    }
                }
                else
                {
                    if (response.Length < 4)
                    {
                        response3.RemoveAllViews();
                        rb = new RadioButton[response.Length];
                        for (int i = 0; i < response.Length; i++)
                        {
                            rb[i] = new RadioButton(this)
                            { Text = response[i].PadRight(10, ' ') };
                            response3.AddView(rb[i]);
                            if (eresponse[i].ToLower() == answer.ToLower())
                                rb[i].Checked = true;
                        }
                        response3.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        response4.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, response);
                        for (int i = 0; i < response.Length; i++)
                        {
                            if (eresponse[i].ToLower() == answer.ToLower())
                            {
                                response4.SetSelection(i);
                                break;
                            }
                        }
                        response4.Visibility = ViewStates.Visible;
                    }
                }
            }
        }

        private string GetRepsonse()
        {
            string result = "";
            if (name != "End")
            {
                if (response.Length == 0)
                {
                    switch (type)
                    {
                        case "date":
                            try { result = response5.DateTime.ToString("MMddyyyy"); }
                            catch { }
                            break;
                        case "decimal":
                            try { decimal d = Convert.ToDecimal(response1.Text); }
                            catch { response1.Text = ""; }
                            result = response1.Text;
                            break;
                        case "number":
                            try { int i = Convert.ToInt32(response1.Text); }
                            catch { response1.Text = ""; }
                            result = response1.Text;
                            break;
                        default:
                            result = response2.Text;
                            break;
                    }
                    HideSoftKeyboard(this);
                }
                else
                {
                    if (response.Length < 4)
                    {
                        for (int i = 0; i < rb.Length; i++)
                        {
                            if (rb[i].Checked)
                                result = eresponse[i].ToLower().Trim();
                        }
                    }
                    else
                        result = eresponse[response4.SelectedItemPosition].ToLower().Trim();
                }
            }
            if (result == "")
                result = "~";
            return result;
        }

        private void StopVoiceReco()
        {
            qname.Text = name;
            player.Stop();
            speech.StopListening();
            speech.Cancel();
        }

        public void OnBeginningOfSpeech()
        {
        }

        public void OnBufferReceived(byte[] buffer)
        {
        }

        public void OnEndOfSpeech()
        {
            qname.Text = name;
            speech.StopListening();
            speech.Cancel();
        }

        public void OnError([GeneratedEnum] SpeechRecognizerError error)
        {
            qname.Text = name;
            speech.StopListening();
            speech.Cancel();
        }

        public void OnEvent(int eventType, Bundle @params)
        {
        }

        public void OnPartialResults(Bundle partialResults)
        {
            var matches = partialResults.GetStringArrayList(SpeechRecognizer.ResultsRecognition);
            if (matches != null)
            {
                if (matches.Count != 0)
                {
                    qname.Text = name;
                    SpeechResults(matches[0]);
                    speech.StopListening();
                    speech.Cancel();
                }
            }
        }

        public void OnReadyForSpeech(Bundle @params)
        {
            qname.Text = "Speak now...";
        }

        public void OnResults(Bundle results)
        {
            var matches = results.GetStringArrayList(SpeechRecognizer.ResultsRecognition);
            if (matches != null)
            {
                if (matches.Count != 0)
                {
                    qname.Text = name;
                    SpeechResults(matches[0]);
                    speech.StopListening();
                    speech.Cancel();
                }
            }
        }

        public void OnRmsChanged(float rmsdB)
        {
        }

        public void SpeechResults(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                string resp = result.Trim();
                if (resp.ToLower().Contains("mail") && name == "Gender")
                    resp = "male";
                if (!resp.ToLower().Contains("previous") && !resp.ToLower().Contains("next") && !resp.ToLower().StartsWith("stop"))
                {
                    if (response.Length == 0)
                    {
                        switch (type)
                        {
                            case "date":
                                try
                                {
                                    response5.DateTime = Convert.ToDateTime(resp);
                                    resp = "next";
                                }
                                catch { }
                                break;
                            case "decimal":
                                try
                                {
                                    response1.Text = Convert.ToDecimal(resp).ToString();
                                    resp = "next";
                                }
                                catch { response1.Text = ""; }
                                break;
                            case "number":
                                try
                                {
                                    response1.Text = Convert.ToInt32(resp).ToString();
                                    resp = "next";
                                }
                                catch { response1.Text = ""; }
                                break;
                            default:
                                response2.Text = resp;
                                resp = "next";
                                break;
                        }
                    }
                    else
                    {
                        if (response.Length < 4)
                        {
                            for (int i = 0; i < response.Length; i++)
                            {
                                if (resp.ToLower() == response[i].ToLower().Trim())
                                {
                                    rb[i].Checked = true;
                                    resp = "next";
                                    break;
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < response.Length; i++)
                            {
                                if (resp.ToLower() == response[i].ToLower().Trim())
                                {
                                    response4.SetSelection(i);
                                    resp = "next";
                                    break;
                                }
                            }
                        }
                    }
                }
                if (handsfree)
                {
                    switch (resp)
                    {
                        case "previous":
                            PreviousQuestion();
                            break;
                        case "next":
                            NextQuestion();
                            break;
                        case "stop":
                            Finish();
                            break;
                        default:
                            SetVoice();
                            break;
                    }
                }
            }
        }


        public void ShowSoftKeyboard(Activity activity, View view)
        {
            InputMethodManager inputMethodManager = (InputMethodManager)activity.GetSystemService(Context.InputMethodService);
            view.RequestFocus();
            inputMethodManager.ShowSoftInput(view, 0);
        }

        public void HideSoftKeyboard(Activity activity)
        {
            var currentFocus = activity.CurrentFocus;
            if (currentFocus != null)
            {
                InputMethodManager inputMethodManager = (InputMethodManager)activity.GetSystemService(Context.InputMethodService);
                inputMethodManager.HideSoftInputFromWindow(currentFocus.WindowToken, HideSoftInputFlags.None);
            }
        }

        private void AlertMessage(string messagetext)
        {
            if (messagetext != "")
            {
                alert.SetMessage(messagetext);
                alert.Show();
            }
        }
    }
}