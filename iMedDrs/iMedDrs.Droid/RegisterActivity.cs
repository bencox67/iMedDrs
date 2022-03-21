using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Widget;
using Newtonsoft.Json;

namespace iMedDrs.Droid
{
    [Activity(Name = "com.imeddrs.imeddrs.RegisterActivity", Label = "iMedDrs - Register", Icon = "@drawable/Icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/Theme.AppCompat.Light.NoActionBar")]
    public class RegisterActivity : AppCompatActivity
    {
        string baseurl;
        string gender;
        string languages;
        string[] message;
        string[] result;
        EditText email;
        EditText name;
        RadioButton male;
        RadioButton female;
        EditText birthdate;
        Spinner language;
        EditText password1;
        EditText password2;
        ImageButton calendar;
        Button register;
        Button returns;
        Android.App.AlertDialog retrn;
        Android.App.AlertDialog alert;
        Android.App.AlertDialog progress;
        MServer ms;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Get intent variables
            baseurl = Intent.GetStringExtra("baseurl");
            languages = Intent.GetStringExtra("languages");

            // Set our view from the "login" layout resource
            SetContentView(Resource.Layout.Register);

            // Initialize variables
            email = FindViewById<EditText>(Resource.Id.email);
            name = FindViewById<EditText>(Resource.Id.name);
            male = FindViewById<RadioButton>(Resource.Id.male);
            female = FindViewById<RadioButton>(Resource.Id.female);
            birthdate = FindViewById<EditText>(Resource.Id.birthdate);
            language = FindViewById<Spinner>(Resource.Id.language);
            language.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, languages.Split('|'));
            password1 = FindViewById<EditText>(Resource.Id.password1);
            password2 = FindViewById<EditText>(Resource.Id.password2);
            calendar = FindViewById<ImageButton>(Resource.Id.calendar);
            register = FindViewById<Button>(Resource.Id.register);
            returns = FindViewById<Button>(Resource.Id.returns);

            // Initialize events
            calendar.Click += Calendar_Click;
            register.Click += Register_Click;
            returns.Click += Returns_Click;

            // Alert dialog for messages
            Android.App.AlertDialog.Builder alertbuilder1 = new Android.App.AlertDialog.Builder(this);
            alertbuilder1.SetPositiveButton("Ok", (EventHandler<DialogClickEventArgs>)null);
            alert = alertbuilder1.Create();

            // Alert dialog to return to login
            Android.App.AlertDialog.Builder alertbuilder2 = new Android.App.AlertDialog.Builder(this);
            alertbuilder2.SetPositiveButton("Ok", OkAction);
            retrn = alertbuilder2.Create();

            // Progress dialog for messaging
            Android.App.AlertDialog.Builder alertbuilder3 = new Android.App.AlertDialog.Builder(this);
            alertbuilder3.SetView(LayoutInflater.Inflate(Resource.Layout.Progress, null));
            progress = alertbuilder3.Create();
            progress.SetCancelable(false);

            // Initialize messaging
            ms = new MServer(baseurl);
        }

        protected override void OnResume()
        {
            base.OnResume();
        }

        public override void OnBackPressed()
        {
            return;
        }

        private void OkAction(object sender, DialogClickEventArgs e)
        {
            Finish();
        }

        private void Calendar_Click(object sender, EventArgs e)
        {
            DatePickerFragment frag = DatePickerFragment.NewInstance(birthdate.Text, delegate (DateTime time)
            {
                birthdate.Text = time.ToString("MM/dd/yyyy");
            });
            frag.Show(SupportFragmentManager, DatePickerFragment.TAG);
        }

        private async void Register_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                progress.Show();
                var names = name.Text.Split(' ');
                message = new string[] { "users" };
                UserModel user = new UserModel()
                {
                    Email = email.Text,
                    FirstName = names[0],
                    LastName = names[1],
                    Gender = gender,
                    Birthdate = Convert.ToDateTime(birthdate.Text),
                    Language = language.SelectedItem.ToString(),
                    Role = "user",
                    Password1 = password1.Text,
                    Password2 = password2.Text,
                };
                string json = JsonConvert.SerializeObject(user);
                await Task.Run(() => result = ms.ProcessMessage(message, "POST", json));
                progress.Dismiss();
                if (result[0] == "ack")
                {
                    retrn.SetMessage("Registeration Complete");
                    retrn.Show();
                }
                else
                    AlertMessage(result[1]);
            }
        }

        private void Returns_Click(object sender, EventArgs e)
        {
            Finish();
        }

        private bool ValidateData()
        {
            bool result = true;
            string error = "";
            email.Text = email.Text.Trim();
            if (email.Text == "")
            {
                error = "Check email address";
                result = false;
            }
            if (result && name.Text.Split(' ').Length < 2)
            {
                error = "Check Name";
                result = false;
            }
            gender = "";
            if (male.Checked)
                gender = "Male";
            if (female.Checked)
                gender = "Female";
            if (result && gender == "")
            {
                error = "Check Gender";
                result = false;
            }
            try { DateTime dt = Convert.ToDateTime(birthdate.Text); }
            catch
            {
                if (result)
                {
                    error = "Check Birth Date";
                    result = false;
                }
            }
            if (result && (password1.Text == "" || password2.Text == "" || password1.Text != password2.Text || password1.Text.Length < 4))
            {
                error = "Check Password";
                result = false;
            }
            AlertMessage(error);
            return result;
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

    public class DatePickerFragment : Android.Support.V4.App.DialogFragment, DatePickerDialog.IOnDateSetListener
    {
        // TAG can be any string of your choice.
        public static readonly string TAG = "X:" + typeof(DatePickerFragment).Name.ToUpper();

        // Initialize this value to prevent NullReferenceExceptions.
        Action<DateTime> _dateSelectedHandler = delegate { };
        string sBirthDate;

        public static DatePickerFragment NewInstance(string sDate, Action<DateTime> onDateSelected)
        {
            DatePickerFragment frag = new DatePickerFragment
            {
                _dateSelectedHandler = onDateSelected,
                sBirthDate = sDate
            };
            return frag;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            DateTime currently;
            try { currently = Convert.ToDateTime(sBirthDate); }
            catch { currently = DateTime.Now; }
            DatePickerDialog dialog = new DatePickerDialog(Activity, this, currently.Year, currently.Month - 1, currently.Day);
            return dialog;
        }

        public void OnDateSet(DatePicker view, int year, int monthOfYear, int dayOfMonth)
        {
            // Note: monthOfYear is a value between 0 and 11, not 1 and 12!
            DateTime selectedDate = new DateTime(year, monthOfYear + 1, dayOfMonth);
            _dateSelectedHandler(selectedDate);
        }
    }
}