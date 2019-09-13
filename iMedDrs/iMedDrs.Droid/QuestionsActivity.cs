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

namespace iMedDrs.Droid
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
    [Activity(Name = "com.imeddrs.imeddrs.QuestionsActivity", Label = "iMedDrs - Questionnaire", Icon = "@drawable/Icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@android:style/Theme.Holo.Light")]
    public class QuestionsActivity : Activity, IRecognitionListener
    {
        string baseurl;
        string userid;
        string username;
        string questionnaire;
        string number;
        string name;
        string text;
        string last;
        string responses;
        string branches;
        string answer;
        string type;
        string datapath;
        string language;
        string extension;
        string instruction;
        string email;
        string[] response;
        string[] eresponse;
        string[] message;
        string[] result;
        int level;
        bool init;
        bool required;
        bool play;
        bool handsfree;
        bool report;
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
            number = Intent.GetStringExtra("number");
            name = Intent.GetStringExtra("name");
            text = Intent.GetStringExtra("text");
            response = Intent.GetStringExtra("response").Split(',');
            eresponse = Intent.GetStringExtra("eresponse").Split(',');
            responses = Intent.GetStringExtra("responses");
            branches = Intent.GetStringExtra("branches");
            last = Intent.GetStringExtra("last");
            required = Convert.ToBoolean(Intent.GetStringExtra("required"));
            type = Intent.GetStringExtra("type");
            answer = Intent.GetStringExtra("answer");
            level = Intent.GetIntExtra("level", 0);
            datapath = Intent.GetStringExtra("datapath");
            language = Intent.GetStringExtra("language");
            extension = Intent.GetStringExtra("extension");
            instruction = Intent.GetStringExtra("instruction");
            email = Intent.GetStringExtra("email");
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
            var attribs = new AudioAttributes.Builder().SetFlags(AudioFlags.None).SetLegacyStreamType(Android.Media.Stream.Music).Build();
            player.SetAudioAttributes(attribs);
            //player.SetAudioStreamType(Android.Media.Stream.Music);
            player.Prepared += Player_Prepared;
            player.Completion += Player_Completion;
            init = true;
            report = false;
            playing = false;
            if (handsfree)
            {
                listen.Visibility = ViewStates.Invisible;
                speak.Visibility = ViewStates.Invisible;
            }

            // Speech Recoginition
            speech = SpeechRecognizer.CreateSpeechRecognizer(this);
            speech.SetRecognitionListener(this);
            voice = new Intent(RecognizerIntent.ActionRecognizeSpeech);
            voice.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelWebSearch);
            voice.PutExtra(RecognizerIntent.ExtraCallingPackage, PackageName);
            voice.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 3000);
            voice.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 3000);
            voice.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 15000);
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

            // Alert dialog for saved questionnaire
            AlertDialog.Builder alertbuilder2 = new AlertDialog.Builder(this);
            alertbuilder2.SetPositiveButton("Ok", OkAction);
            save = alertbuilder2.Create();

            // Alert dialog for messages
            AlertDialog.Builder alertbuilder3 = new AlertDialog.Builder(this);
            alertbuilder3.SetPositiveButton("Ok", (EventHandler<DialogClickEventArgs>)null);
            alert = alertbuilder3.Create();

            // Progress dialog for messaging
            AlertDialog.Builder alertbuilder4 = new AlertDialog.Builder(this);
            alertbuilder4.SetView(LayoutInflater.Inflate(Resource.Layout.Progress, null));
            progress = alertbuilder4.Create();
            progress.SetCancelable(false);

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
                    if (!report && !init)
                        GetVoice();
                    else
                        NextQuestion();
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
                    progress.Show();
                    message = new string[] { "questionnaire", "next", userid, questionnaire, number, ans, responses, branches };
                    await Task.Run(() => result = ms.ProcessMessage(message, "GET"));
                    progress.Dismiss();
                    if (result[1] == "ack")
                    {
                        number = result[2];
                        name = result[3];
                        text = result[4];
                        response = result[5].Split(',');
                        last = result[6];
                        responses = result[7];
                        branches = result[8];
                        required = Convert.ToBoolean(result[9]);
                        type = result[10];
                        answer = result[11];
                        eresponse = result[12].Split(',');
                        extension = result[13];
                        instruction = result[14];
                        SetQuestion();
                        SetResponses();
                    }
                    else
                        AlertMessage(result[2]);
                }
            }
            else
            {
                if (level > 1)
                {
                    progress.Show();
                    message = new string[] { "questionnaire", "save", userid, questionnaire, responses };
                    await Task.Run(() => result = ms.ProcessMessage(message, "GET"));
                    progress.Dismiss();
                    if (result[1] == "ack")
                    {
                        if (!handsfree)
                        {
                            save.SetMessage(result[2]);
                            save.Show();
                        }
                        else
                            GetReport();
                    }
                    else
                        AlertMessage(result[2]);
                }
                else
                {
                    if (ps.WriteToFile(Path.Combine(datapath, questionnaire.Replace(" ", "_") + ".txt"), responses, true))
                    {
                        if (!handsfree)
                        {
                            save.SetMessage("Questionnaire responses saved");
                            save.Show();
                        }
                        else
                            GetReport();
                    }
                    else
                        AlertMessage("Not able to save resposnes");
                }
            }
        }

        private async void PreviousQuestion()
        {
            progress.Show();
            message = new string[] { "questionnaire", "previous", userid, questionnaire, number, responses, branches };
            await Task.Run(() => result = ms.ProcessMessage(message, "GET"));
            progress.Dismiss();
            if (result[1] == "ack")
            {
                number = result[2];
                name = result[3];
                text = result[4];
                response = result[5].Split(',');
                last = result[6];
                responses = result[7];
                branches = result[8];
                required = Convert.ToBoolean(result[9]);
                type = result[10];
                answer = result[11];
                eresponse = result[12].Split(',');
                extension = result[13];
                instruction = result[14];
                SetQuestion();
                SetResponses();
            }
            else
                AlertMessage(result[2]);
        }

        private async void GetReport()
        {
            string data = "";
            message = null;
            if (level < 2)
            {
                data = ps.ReadFromFile(Path.Combine(datapath, questionnaire.Replace(" ", "_") + ".txt"));
                if (data != "")
                    message = new string[] { "report", "load2", userid, questionnaire, "0", data };
            }
            else
                message = new string[] { "report", "load", userid, questionnaire, "0" };
            if (message != null)
            {
                progress.Show();
                await Task.Run(() => result = ms.ProcessMessage(message, "GET"));
                progress.Dismiss();
                if (result[1] == "ack")
                {
                    Intent intent = new Intent(this.ApplicationContext, typeof(ReportActivity));
                    intent.PutExtra("baseurl", baseurl);
                    intent.PutExtra("userid", userid);
                    intent.PutExtra("username", username);
                    intent.PutExtra("questionnaire", questionnaire);
                    intent.PutExtra("last", Convert.ToInt32(result[2]));
                    intent.PutExtra("number", Convert.ToInt32(result[3]));
                    intent.PutExtra("text", result[4]);
                    intent.PutExtra("data", data);
                    intent.PutExtra("email", email);
                    intent.PutExtra("level", level);
                    StartActivity(intent);
                }
            }
            Finish();
        }
        private void SetQuestion()
        {
            SetVoice();
            if (name == "End")
                report = true;
            instructions.Text = instruction.Replace("~", "\r\n");
            qname.Text = name;
            question.Text = text;
            if (name == "End")
                report = true;
        }

        private void SetVoice()
        {
            play = false;
            player.Stop();
            player.Reset();
            try
            {
                player.SetDataSource("https://imeddrs.com/audio/" + language + "/" + name + extension);
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
            if (name != "End")
            {
                if (response.Length == 1)
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
                                    response1.Text = response1.Text.Substring(1);
                                if (response1.Text.EndsWith(".00"))
                                    response1.Text = response1.Text.Substring(0, response1.Text.Length - 3);
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
                            if (eresponse[i] == answer)
                                rb[i].Checked = true;
                        }
                        response3.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        response4.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, response);
                        for (int i = 0; i < response.Length; i++)
                        {
                            if (eresponse[i] == answer)
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
                if (response.Length == 1)
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
                }
                else
                {
                    if (response.Length < 4)
                    {
                        for (int i = 0; i < rb.Length; i++)
                        {
                            if (rb[i].Checked)
                                result = eresponse[i].Trim();
                        }
                    }
                    else
                        result = eresponse[response4.SelectedItemPosition].Trim();
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
        }

        public void OnError([GeneratedEnum] SpeechRecognizerError error)
        {
            qname.Text = name;
        }

        public void OnEvent(int eventType, Bundle @params)
        {
        }

        public void OnPartialResults(Bundle partialResults)
        {
            qname.Text = name;
        }

        public void OnReadyForSpeech(Bundle @params)
        {
            qname.Text = "Speak now...";
        }

        public void OnResults(Bundle results)
        {
            qname.Text = name;
            string resp = "";
            var matches = results.GetStringArrayList(SpeechRecognizer.ResultsRecognition);
            if (matches.Count != 0)
            {
                resp = matches[0];
                if (resp.ToLower() == "mail" && name == "Gender")
                    resp = "male";
                if (resp.ToLower() != "previous" && resp.ToLower() != "next" && !resp.ToLower().StartsWith("stop"))
                {
                    if (response.Length == 1)
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
                                if (response[i].ToLower() == resp.ToLower())
                                {
                                    rb[i].Checked = true;
                                    resp = "next";
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < response.Length; i++)
                            {
                                if (response[i].ToLower() == resp.ToLower())
                                {
                                    response4.SetSelection(i);
                                    resp = "next";
                                    break;
                                }
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

        public void OnRmsChanged(float rmsdB)
        {
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