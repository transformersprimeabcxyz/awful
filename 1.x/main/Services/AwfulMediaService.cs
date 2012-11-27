using System;
using System.IO.IsolatedStorage;
using System.ComponentModel;
using System.Net;
using System.IO;
using System.Threading;
using System.Windows.Media.Imaging;
using Microsoft.Xna.Framework.Media;
using ImageTools.IO.Gif;
using ImageTools;
using System.Windows.Media;
using System.Windows;

namespace Awful.Services
{
    public class AwfulMediaService
    {
        private readonly BackgroundWorker worker = new BackgroundWorker();
        private readonly AutoResetEvent signal = new AutoResetEvent(false);

        public AwfulMediaService()
        {
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = false;
            worker.DoWork += new DoWorkEventHandler(DoWork);
        }

        public void CancelAsync()
        {
            if (worker.IsBusy)
                worker.CancelAsync();
        }

        public void SaveMediaToLibrary(string uri, Action<Awful.Core.Models.ActionResult> result)
        {
            RunWorkerCompletedEventHandler completed = null;
            completed = (obj, args) =>
                {
                    worker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(completed);
                    if (args.Cancelled)
                    {
                        result(Awful.Core.Models.ActionResult.Cancelled);
                    }
                    else
                    {
                        var action = (Awful.Core.Models.ActionResult)args.Result;
                        result(action);
                    }
                };

            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(completed);
            worker.RunWorkerAsync(uri);
        }

        private void DoWork(object sender, DoWorkEventArgs args)
        {
            string url = args.Argument.ToString();
            string name = string.Format("image-{0}.jpg", DateTime.Now.Ticks);

            if (url.Contains("/") && !url.Contains("attachment.php"))
            {
                var tokens = url.Split('/');
                name = tokens[tokens.Length - 1];
            }

            HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
            Stream data = null;

            request.BeginGetResponse(state =>
                {
                    try
                    {
                        var response = request.EndGetResponse(state);
                        data = response.GetResponseStream();
                    }
                    catch (Exception ex) { Console.WriteLine(ex.Message); }
                    finally { signal.Set(); }

                }, request);

            signal.WaitOne(App.Settings.ThreadTimeout);

            if (worker.CancellationPending) { args.Cancel = true; return; }

            if (data == null)
            {
                args.Result = Awful.Core.Models.ActionResult.Failure;
                return;
            }

             if (worker.CancellationPending) { args.Cancel = true; return; }
             IsolatedStorageFileStream fileStream = null;

            try
            {
                MediaLibrary lib = new MediaLibrary();
                
                string tempFile = name;
                var dispatch = Deployment.Current.Dispatcher;
                var dispatchSignal = new AutoResetEvent(false);
                var store = IsolatedStorageFile.GetUserStoreForApplication();

                dispatch.BeginInvoke(() =>
                    {
                        try
                        {
                            if (store.FileExists(tempFile))
                                store.DeleteFile(tempFile);

                            fileStream = store.CreateFile(tempFile);
                            WriteableBitmap bitmap;

                            try
                            {
                                var decoder = new GifDecoder();
                                var image = new ExtendedImage();
                                decoder.Decode(image, data);
                                bitmap = image.ToBitmap();
                            }
                            catch (Exception ex)
                            {
                                var image = new BitmapImage();
                                image.SetSource(data);
                                bitmap = new WriteableBitmap(image);
                            }

                            bitmap.SaveJpeg(fileStream, bitmap.PixelWidth, bitmap.PixelHeight, 0, 100);
                            fileStream.Close();
                            dispatchSignal.Set();
                        }
                        catch (Exception ex) { dispatchSignal.Set(); }
                    });

                dispatchSignal.WaitOne();
                fileStream = store.OpenFile(tempFile, FileMode.Open, FileAccess.Read);
                lib.SavePicture(name, fileStream);
                fileStream.Close();
                store.DeleteFile(tempFile);

                args.Result = Awful.Core.Models.ActionResult.Success;
            }
            catch (Exception ex)
            {
                if (fileStream != null)
                    fileStream.Close();

                Console.WriteLine(ex.Message);
                args.Result = Awful.Core.Models.ActionResult.Failure;
            }
        }
    }
}
