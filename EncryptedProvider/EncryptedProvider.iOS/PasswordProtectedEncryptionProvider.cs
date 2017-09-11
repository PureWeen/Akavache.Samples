/*
Taken from http://kent-boogaart.com/blog/password-protected-encryption-provider-for-akavache

*/
using System;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Security.Cryptography;
using System.Text;
using Akavache;

namespace EncryptedProvider.iOS
{
    //# add a random, 16 character string here #
    public sealed class PasswordProtectedEncryptionProvider : IEncryptionProvider
    {

        private static readonly byte[] salt = Encoding.ASCII.GetBytes("1234567890123456");
        private readonly IScheduler scheduler;
        private readonly SymmetricAlgorithm symmetricAlgorithm;
        private ICryptoTransform decryptTransform;
        private ICryptoTransform encryptTransform;

        public PasswordProtectedEncryptionProvider(IScheduler scheduler)
        {
            _ = scheduler ?? throw new ArgumentNullException(nameof(scheduler));

            this.scheduler = scheduler;
            this.symmetricAlgorithm = new RijndaelManaged();
        }

        public void SetPassword(string password)
        {
            _ = password ?? throw new ArgumentNullException(nameof(password));

            var derived = new Rfc2898DeriveBytes(password, salt);
            var bytesForKey = this.symmetricAlgorithm.KeySize / 8;
            var bytesForIV = this.symmetricAlgorithm.BlockSize / 8;
            this.symmetricAlgorithm.Key = derived.GetBytes(bytesForKey);
            this.symmetricAlgorithm.IV = derived.GetBytes(bytesForIV);
            this.decryptTransform = this.symmetricAlgorithm.CreateDecryptor(this.symmetricAlgorithm.Key, this.symmetricAlgorithm.IV);
            this.encryptTransform = this.symmetricAlgorithm.CreateEncryptor(this.symmetricAlgorithm.Key, this.symmetricAlgorithm.IV);
        }

        public IObservable<byte[]> DecryptBlock(byte[] block)
        {
            _ = block ?? throw new ArgumentNullException(nameof(block));

            if (this.decryptTransform == null)
            {
                return Observable.Throw<byte[]>(new InvalidOperationException("You must call SetPassword first."));
            }

            return Observable
                .Start(
                    () => InMemoryTransform(block, this.decryptTransform),
                    this.scheduler);
        }

        public IObservable<byte[]> EncryptBlock(byte[] block)
        {
            _ = block ?? throw new ArgumentNullException(nameof(block));

            if (this.encryptTransform == null)
            {
                return Observable.Throw<byte[]>(new InvalidOperationException("You must call SetPassword first."));
            }

            return Observable
                .Start(
                    () => InMemoryTransform(block, this.encryptTransform),
                    this.scheduler);
        }

        private static byte[] InMemoryTransform(byte[] bytesToTransform, ICryptoTransform transform)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(bytesToTransform, 0, bytesToTransform.Length);
                }

                return memoryStream.ToArray();
            }
        }
    }
}