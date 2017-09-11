using System;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using UIKit;
using Akavache;
using Splat;
using Akavache.Sqlite3;
using System.Threading;

namespace EncryptedProvider.iOS
{
    public partial class ViewController : UIViewController
    {
        private readonly SomeData kStoreMe = new SomeData() { SomeName = "store me" };
        private PasswordProtectedEncryptionProvider _encryptionProvider;
        private IBlobCache _encryptedBlobCache;
        
        public ViewController(IntPtr handle) : base(handle)
        {

        }


        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _encryptionProvider = new PasswordProtectedEncryptionProvider(TaskPoolScheduler.Default);
            _encryptedBlobCache =
                new SQLiteEncryptedBlobCache("secure.db", _encryptionProvider, TaskPoolScheduler.Default);
        }

        partial void BtnStoreData_TouchUpInside(UIButton sender)
        {
            _encryptionProvider.SetPassword(txtPassword.Text);
            
            _encryptedBlobCache
                .InsertObject("key", kStoreMe)
                .Do(_=> InvokeOnMainThread(() =>  lblResult.Text = "Data Stored"))
                .Subscribe();

        }

        partial void BtnRestoreData_TouchUpInside(UIButton sender)
        {
            _encryptionProvider.SetPassword(txtPassword.Text);
            _encryptedBlobCache
                .GetObject<SomeData>("key")
                .Do(data => InvokeOnMainThread(() =>  lblResult.Text = $"Data Restored: {data.SomeName}"))
                .Catch((Exception exc) =>
                {
                    InvokeOnMainThread(() => lblResult.Text = $"{exc}");
                    return Observable.Empty<SomeData>();
                })
                .Subscribe();
        }
    }


    public class SomeData
    {
        public string SomeName { get; set; }
    }
}