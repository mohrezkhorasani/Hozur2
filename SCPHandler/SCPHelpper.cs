using Application.Interface.IInfrastructure;
using Microsoft.AspNetCore.Hosting;
using System;
using WinSCP;

namespace SCPHandler
{
    public class ScpUploader : IScpUploader
    {
        private const string host = "2.179.167.151";
        private const string username = "webupload";
        private const string password = "@12a";
        private const string remoteFolder = "/var/www/bigbluebutton-default/assets/cert/";
        string sshPrivateKeyPath = @"F:\DOTNET\New folder\CoreMe\wwwroot\pvkey.ppk"; // مسیر کلید خصوصی
        private IWebHostEnvironment hosting;

        public ScpUploader(IWebHostEnvironment host)
        {
            this.hosting = host;
        }

        public void UploadFile(string localFilePath)
        {
            try
            {
                var baseDirectory = hosting.ContentRootPath;
                sshPrivateKeyPath = Path.Combine(baseDirectory, "pvkey.ppk");


                var sessionOptions = new SessionOptions
                {
                    Protocol = Protocol.Sftp,
                    HostName = "2.179.167.151",
                    UserName = "webupload",
                    SshPrivateKeyPath = sshPrivateKeyPath,
                    SshHostKeyFingerprint = "ssh-ed25519 255 NIpA/fnLdkoTfjvnLtxZkW038Jm5uweC4JJ3t7yin+I"
                };


                using (var session = new Session())
                {
                    session.Timeout = TimeSpan.FromMinutes(5);
                    session.ReconnectTime = TimeSpan.FromSeconds(20);
                    // اتصال به سرور
                    session.Open(sessionOptions);


                    // تنظیمات انتقال
                    var transferOptions = new TransferOptions
                    {
                        TransferMode = TransferMode.Binary,
                        FilePermissions = null,
                        PreserveTimestamp = false
                    };

                    // آپلود فایل
                    var remotePath = $"{remoteFolder}{System.IO.Path.GetFileName(localFilePath)}";
                    var transferResult = session.PutFiles(localFilePath, remotePath, false, transferOptions);

                    // بررسی نتایج انتقال
                    transferResult.Check();

                    Console.WriteLine($"✅ فایل '{localFilePath}' با موفقیت به '{remotePath}' آپلود شد.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ خطا در آپلود: {ex.Message}");
                throw new Exception(ex.Message);
            }
        }
    }
}
