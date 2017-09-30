using Akavache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using Akavache.Sqlite3;
using Xamarin.Forms;
using System.Reactive.Concurrency;
using Microsoft.Azure.Mobile;
using Microsoft.Azure.Mobile.Crashes;
using Microsoft.Azure.Mobile.Distribute;
using Microsoft.Azure.Mobile.Analytics;
using Microsoft.Azure.Mobile.Push;
using System.Threading.Tasks;

namespace MobileCenterApp
{
    public class SomeObject
    {
        public string Property1 { get; set; }
        public string Property2 { get; set; }
    }

    public partial class App : Application
    {
        const string LogTag = "MobileCenterXamarinDemo";

        const string uwpKey = "5bce20c8-f00b-49ca-8580-7a49d5705d4c";
        const string androidKey = "987b5941-4fac-4968-933e-98a7ff29237c";
        const string iosKey = "fe2bf05d-f4f9-48a6-83d9-ea8033fbb644";


        private IBlobCache _BlobCache;

        public App ()
        {

            InitializeComponent();

            SQLitePCL.Batteries_V2.Init();
            SQLitePCL.raw.FreezeProvider();
            _BlobCache = new SQLitePersistentBlobCache("afile.db");
            MainPage = new MobileCenterApp.MainPage();
        }

		protected override void OnStart ()
        {
            MobileCenter.LogLevel = LogLevel.Error;
            Crashes.ShouldProcessErrorReport = ShouldProcess;
            Crashes.ShouldAwaitUserConfirmation = ConfirmationHandler;
            Crashes.GetErrorAttachments = GetErrorAttachments;
            Distribute.ReleaseAvailable = OnReleaseAvailable;
            MobileCenter.Start($"uwp={uwpKey};android={androidKey};ios={iosKey}",
                               typeof(Analytics), typeof(Crashes), typeof(Distribute), typeof(Push));

            MobileCenter.GetInstallIdAsync().ContinueWith(installId =>
            {
                MobileCenterLog.Info(LogTag, "MobileCenter.InstallId=" + installId.Result);
            });
            Crashes.HasCrashedInLastSessionAsync().ContinueWith(hasCrashed =>
            {
                MobileCenterLog.Info(LogTag, "Crashes.HasCrashedInLastSession=" + hasCrashed.Result);
            });
            Crashes.GetLastSessionCrashReportAsync().ContinueWith(report =>
            {
                MobileCenterLog.Info(LogTag, "Crashes.LastSessionCrashReport.Exception=" + report.Result?.Exception);
            });




            MainPage.Appearing += (x, y) =>
            {
                _BlobCache.GetObject<SomeObject>("test")
                   .Catch((KeyNotFoundException ke) => Observable.Return<SomeObject>(null))
                   .SelectMany(someClass =>
                   {
                       Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                       {
                           Current.MainPage.DisplayActionSheet($"{someClass?.ToString() ?? "null"}", null, null, $"{someClass?.ToString() ?? "null"}", $"{someClass?.ToString() ?? "null"}", $"{someClass?.ToString() ?? "null"}").ContinueWith((arg) =>
                           {

                           });
                       });
                       return _BlobCache.InsertObject("test",
                           new SomeObject()
                           {
                               Property1 = "1234",
                               Property2 = "4321"
                           }
                           );
                   })
                       .SelectMany(_ => _BlobCache.Flush())
                       .SelectMany(_ => _BlobCache.GetObject<SomeObject>("test"))
                       .Subscribe(someclass =>
                       {
                       }
                       );

            };
            // Handle when your app starts
        }

        bool OnReleaseAvailable(ReleaseDetails releaseDetails)
        {
            MobileCenterLog.Info("MobileCenterDemo", "OnReleaseAvailable id=" + releaseDetails.Id
                                            + " version=" + releaseDetails.Version
                                            + " releaseNotesUrl=" + releaseDetails.ReleaseNotesUrl);
            var custom = releaseDetails.ReleaseNotes?.ToLowerInvariant().Contains("custom") ?? false;
            if (custom)
            {
                var title = "Version " + releaseDetails.ShortVersion + " available!";
                Task answer;
                if (releaseDetails.MandatoryUpdate)
                {
                    answer = Current.MainPage.DisplayAlert(title, releaseDetails.ReleaseNotes, "Update now!");
                }
                else
                {
                    answer = Current.MainPage.DisplayAlert(title, releaseDetails.ReleaseNotes, "Update now!", "Maybe tomorrow...");
                }
                answer.ContinueWith((task) =>
                {
                    if (releaseDetails.MandatoryUpdate || (task as Task<bool>).Result)
                    {
                        Distribute.NotifyUpdateAction(UpdateAction.Update);
                    }
                    else
                    {
                        Distribute.NotifyUpdateAction(UpdateAction.Postpone);
                    }
                });
            }
            return custom;
        }

        IEnumerable<ErrorAttachmentLog> GetErrorAttachments(ErrorReport report)
        {
            return new ErrorAttachmentLog[]
            {
                ErrorAttachmentLog.AttachmentWithText("Hello world!", "hello.txt"),
                ErrorAttachmentLog.AttachmentWithBinary(Encoding.UTF8.GetBytes("Fake image"), "fake_image.jpeg", "image/jpeg")
            };
        }

        bool ConfirmationHandler()
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
            {
                Current.MainPage.DisplayActionSheet("Crash detected. Send anonymous crash report?", null, null, "Send", "Always Send", "Don't Send").ContinueWith((arg) =>
                {
                    var answer = arg.Result;
                    UserConfirmation userConfirmationSelection;
                    if (answer == "Send")
                    {
                        userConfirmationSelection = UserConfirmation.Send;
                    }
                    else if (answer == "Always Send")
                    {
                        userConfirmationSelection = UserConfirmation.AlwaysSend;
                    }
                    else
                    {
                        userConfirmationSelection = UserConfirmation.DontSend;
                    }
                    MobileCenterLog.Debug(LogTag, "User selected confirmation option: \"" + answer + "\"");
                    Crashes.NotifyUserConfirmation(userConfirmationSelection);
                });
            });

            return true;
        }

        bool ShouldProcess(ErrorReport report)
        {
            MobileCenterLog.Info(LogTag, "Determining whether to process error report");
            return true;
        }

        protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}
