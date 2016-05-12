using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Net;
using System.ComponentModel;
using System.IO;

namespace MyPresenter
{
    /// <summary>
    /// Interaction logic for RemoteControl.xaml
    /// </summary>
    public class RemoteControl
    {
        public event EventHandler<RemoteControlArgs> RemoteControlEvent;

        public class RemoteControlArgs : EventArgs
        {
            public Dictionary<string, string> PageParams { get; set; }

            public RemoteControlArgs(Dictionary<string, string> pageParams)
            {
                this.PageParams = pageParams;
            }
        }

        public string LocalAddress { get; set; }

        private BackgroundWorker worker;
        private bool cancelling = false;
        private bool error = false;
        private HttpListener listener;
        private int portNumber = 4343;  //if you change this, Android phone doesn't work

        //The Dictionary is used to pass parameters from the background thread to the UI thread.  Using a static works in this case as long as 
        //rate of input is slow enough that UI events are consumed faster than button-presses are received from the remote device.
        //Otherwise the dictionary could be overwritten before it is used. A queue or some sort of semaphone might be needed if this proves to be a problem
        static Dictionary<string, string> pageParams = new Dictionary<string, string>();

        public RemoteControl()
        {
            worker = new BackgroundWorker();
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;
        }

        public string status { get; set; }

        public void StartServer()
        {
            worker.RunWorkerAsync();

            //Display some useful info
            status = "";

            //display the local machine name /  IP address for use with remote handler
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

            foreach (IPAddress ip in localIPs)
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    LocalAddress = ip.ToString() + ":" + portNumber.ToString();

            status += ("http://" + LocalAddress);
        }

        public void StopServer()
        {
            worker.CancelAsync();

            cancelling = true; //this prevents the error message display from the exception in the next line

            listener.Close(); //this causes the the listener to throw an exception
        }

        private void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                // Create a listener.
                listener = new HttpListener();

                // Add the prefixes.
                listener.Prefixes.Add("http://*:4343/");

                listener.Start();

                while (!worker.CancellationPending)
                {
                    // intitialize response buffer
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(string.Empty);;

                    // Note: The GetContext method blocks while waiting for a request. 
                    HttpListenerContext context = listener.GetContext();

                    if (context.Request.QueryString.Keys.Count == 0)
                    {
                        //this is a POST, handle the input parameters
                        var body = new StreamReader(context.Request.InputStream).ReadToEnd();

                        ParsePostParameters(body);

                        // raise event
                        worker.ReportProgress(100);

                        System.Threading.Thread.Sleep(100);

                        string pageString = App.Current.Properties["html"].ToString(); // tr.ReadToEnd();

                        // Set up the response object to serve the page
                        buffer = System.Text.Encoding.UTF8.GetBytes(pageString);

                        context.Response.ContentLength64 = buffer.Length;
                        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                        context.Response.Close();
                    }
                    else if (context.Request.QueryString.HasKeys())
                    {
                        foreach (string s in context.Request.QueryString.AllKeys)
                        {
                            if (context.Request.QueryString[s] == "time")
                                buffer = System.Text.Encoding.UTF8.GetBytes(DateTime.Now.ToString("HH:mm:ss"));

                            if (context.Request.QueryString[s] == "next")
                            {
                                pageParams.Clear();
                                pageParams.Add("update", "next");

                                // raise event
                                worker.ReportProgress(100);

                                System.Threading.Thread.Sleep(100);

                                buffer = System.Text.Encoding.UTF8.GetBytes(App.Current.Properties["card"].ToString());
                            }
                        }

                        context.Response.ContentLength64 = buffer.Length;
                        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                        context.Response.Close();
                    }
                }

                listener.Stop();
            }
            catch (Exception e1)
            {
                if (!cancelling)
                {
                    MessageBox.Show("Remote Listener failed. " + e1.Message);

                    listener.Stop();

                    error = true;
                }
            }
        }

        private static void ParsePostParameters(string body)
        {
            string[] stringParams = body.Split('&');

            pageParams.Clear();

            foreach (string s in stringParams)
            {
                int index = s.IndexOf('=');

                if (index > -1)
                {
                    string key = s.Substring(0, index);
                    string value = s.Substring(index + 1);

                    value = System.Uri.UnescapeDataString(value); //removes all the secret special-character encoding

                    pageParams.Add(key, value);
                }
            }
        }

        private void worker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            cancelling = false;

            status = "";

            // restart if error
            if (error)
            {
                worker.RunWorkerAsync();

                error = false;
            }
        }

        private void worker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            RemoteControlEvent(this, new RemoteControlArgs(pageParams));
        }
    }
}
