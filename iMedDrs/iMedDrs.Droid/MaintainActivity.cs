using System;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Util;
using Android.Widget;

namespace iMedDrs.Droid
{
    [Activity(Name = "com.imeddrs.imeddrs.MaintainActivity", Label = "iMedDrs - Maintain Questionnaires", Icon = "@drawable/Icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@android:style/Theme.Holo.Light")]
    public class MaintainActivity : Activity
    {
        string baseurl;
        string userid;
        string datapath;
        int level;
        int scriptnum;
        string[][] scriptdata;
        string[] questionnaires;
        string[] languages;
        string[] scripts;
        string[] message;
        string[] result;
        Spinner questionnaire;
        Spinner language;
        TextView name;
        ListView script;
        TextView question;
        Button play;
        Button record;
        Button next;
        Button previous;
        Button updatevoice;
        Button returns;
        MediaRecorder recorder;
        MediaPlayer player;
        AlertDialog alert;
        AlertDialog progress;
        MServer ms;
        PServer ps;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Get intent variables
            baseurl = Intent.GetStringExtra("baseurl");
            userid = Intent.GetStringExtra("userid");
            questionnaires = Intent.GetStringExtra("questionnaires").Split(',');
            level = Intent.GetIntExtra("level", 0);
            languages = Intent.GetStringExtra("languages").Split(',');
            datapath = Intent.GetStringExtra("datapath");

            // Set our view from the "process" layout resource
            SetContentView(Resource.Layout.Maintain);

            // Initialize variables
            questionnaire = FindViewById<Spinner>(Resource.Id.questionnaire);
            questionnaire.Adapter = new ArrayAdapter(this, Resource.Layout.ListItem, questionnaires);
            language = FindViewById<Spinner>(Resource.Id.language);
            language.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, languages);
            for (int i = 0; i < languages.Length; i++)
            {
                if (languages[i] == "English")
                    language.SetSelection(i);
            }
            name = FindViewById<TextView>(Resource.Id.name);
            script = FindViewById<ListView>(Resource.Id.script);
            question = FindViewById<TextView>(Resource.Id.question);
            record = FindViewById<Button>(Resource.Id.record);
            play = FindViewById<Button>(Resource.Id.play);
            next = FindViewById<Button>(Resource.Id.next);
            previous = FindViewById<Button>(Resource.Id.previous);
            updatevoice = FindViewById<Button>(Resource.Id.updatevoice);
            returns = FindViewById<Button>(Resource.Id.returns);
            recorder = new MediaRecorder();
            player = new MediaPlayer();
            scriptnum = -1;

            // Initialize events
            questionnaire.ItemSelected += Questionnaire_ItemSelected;
            language.ItemSelected += Language_ItemSelected;
            record.Click += Record_Click;
            play.Click += Play_Click;
            next.Click += Next_Click;
            previous.Click += Previous_Click;
            updatevoice.Click += UpdateVoice_Click;
            returns.Click += Returns_Click;
            script.ItemClick += Script_ItemClick;
            script.ChoiceMode = ChoiceMode.Single;

            // Detect screen size
            var metrics = Resources.DisplayMetrics;
            var dp = (int)((metrics.HeightPixels) / Resources.DisplayMetrics.Density);
            if (dp > 900)
                script.LayoutParameters.Height = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 320, Resources.DisplayMetrics);

            // Alert dialog for messages
            AlertDialog.Builder alertbuilder1 = new AlertDialog.Builder(this);
            alertbuilder1.SetPositiveButton("Ok", (EventHandler<DialogClickEventArgs>)null);
            alert = alertbuilder1.Create();

            // Progress dialog for messaging
            AlertDialog.Builder alertbuilder2 = new AlertDialog.Builder(this);
            alertbuilder2.SetView(LayoutInflater.Inflate(Resource.Layout.Progress, null));
            progress = alertbuilder2.Create();
            progress.SetCancelable(false);

            // Initialize messaging
            ms = new MServer(baseurl);
            ps = new PServer();
        }

        private void Returns_Click(object sender, EventArgs e)
        {
            if (record.Text == "Stop")
            {
                record.Text = "Record";
                recorder.Stop();
                recorder.Reset();
            }
            recorder.Release();
            player.Release();
            Finish();
        }

        public override void OnBackPressed()
        {
            return;
        }

        private void Questionnaire_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (scriptnum != -1)
                Load();
        }

        private void Language_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Load();
        }

        private void Script_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            ScriptData(e.Position);
        }

        private void Record_Click(object sender, EventArgs e)
        {
            if (name.Text != "")
            {
                if (record.Text == "Record")
                {
                    record.Text = "Stop";
                    recorder.Reset();
                    recorder.SetAudioSource(AudioSource.Mic);
                    recorder.SetOutputFormat(OutputFormat.Mpeg4);
                    recorder.SetAudioEncoder(AudioEncoder.Aac);
                    recorder.SetOutputFile(Path.Combine(datapath, name.Text + ".mp4"));
                    recorder.Prepare();
                    recorder.Start();
                }
                else
                {
                    record.Text = "Record";
                    recorder.Stop();
                    recorder.Reset();
                }
            }
        }

        private void Play_Click(object sender, EventArgs e)
        {
            if (name.Text != "")
            {
                if (record.Text == "Stop")
                {
                    record.Text = "Record";
                    recorder.Stop();
                    recorder.Reset();
                }
                if (File.Exists(Path.Combine(datapath, name.Text + ".mp4")))
                {
                    player.Reset();
                    player.SetDataSource(Path.Combine(datapath, name.Text + ".mp4"));
                    player.Prepare();
                    player.Start();
                }
            }
        }

        private void Previous_Click(object sender, EventArgs e)
        {
            if (record.Text == "Record")
            {
                int position = 0;
                if (scriptnum > 0)
                    position = scriptnum - 1;
                if (name.Text != "")
                    position--;
                if (position > -1)
                    ScriptData(position);
            }
        }

        private void Next_Click(object sender, EventArgs e)
        {
            if (record.Text == "Record")
            {
                int position = 0;
                if (scriptnum > 0)
                    position = scriptnum - 1;
                if (name.Text != "")
                    position++;
                if (position < scripts.Length)
                    ScriptData(position);
            }
        }

        private async void UpdateVoice_Click(object sender, EventArgs e)
        {
            if (record.Text == "Record")
            {
                if (File.Exists(Path.Combine(datapath, name.Text + ".mp4")))
                {
                    string message = "";
                    progress.Show();
                    await Task.Run(() => message = ms.PostFile(language.SelectedItem.ToString(), datapath, name.Text + ".mp4"));
                    progress.Dismiss();
                    if (message == "Recording updated")
                    {
                        try { File.Delete(Path.Combine(datapath, name.Text + ".mp4")); }
                        catch { }
                    }
                    AlertMessage(message);
                }
            }
        }

        private async void Load()
        {
            ClearForm();
            message = new string[] { "script", "data", questionnaire.SelectedItem.ToString(), level.ToString(), language.SelectedItem.ToString() };
            progress.Show();
            await Task.Run(() => result = ms.ProcessMessage(message, "GET"));
            progress.Dismiss();
            if (result[1] == "ack")
            {
                int len = result.Length - 2;
                scriptdata = new string[len][];
                scripts = new string[len];
                for (int i = 0; i < len; i++)
                {
                    scriptdata[i] = result[i + 2].Split('|');
                    scripts[i] = scriptdata[i][0];
                }
                script.Adapter = new ArrayAdapter(this, Resource.Layout.ListItem, scripts);
                script.SetItemChecked(0, true);
                script.SetSelection(0);
            }
            else
                AlertMessage(result[2]);
        }

        private void ScriptData(int number)
        {
            scriptnum = number + 1;
            name.Text = scriptdata[number][0];
            if (scriptdata[number][8] == "")
                question.Text = scriptdata[number][7];
            else
                question.Text = scriptdata[number][8];
        }

        private void ClearForm()
        {
            script.ClearChoices();
            name.Text = "";
            question.Text = "";
            scriptnum = 0;
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