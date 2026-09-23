using System;
using System.Net;
using System.Text;
using System.Threading;

namespace PlayniteAnywhere
{
    public class WebServer
    {
        private readonly HttpListener listener;
        private Thread serverThread;

        public WebServer(int port)
        {
            listener = new HttpListener();
            listener.Prefixes.Add($"http://localhost:{port}/");
        }

        public void Start()
        {
            listener.Start();

            serverThread = new Thread(ListenLoop)
            {
                IsBackground = true
            };

            serverThread.Start();
        }

        public void Stop()
        {
            if (!listener.IsListening)
            {
                return;
            }

            listener.Stop();
            listener.Close();

            if (serverThread != null && serverThread.IsAlive)
            {
                serverThread.Join(1000);
            }
        }

        private void ListenLoop()
        {
            while (listener.IsListening)
            {
                try
                {
                    var context = listener.GetContext();
                    HandleRequest(context);
                }
                catch (HttpListenerException)
                {
                    // Expected when the listener is stopped.
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private void HandleRequest(HttpListenerContext context)
        {
            const string responseText = "Playnite Anywhere fonctionne !";

            byte[] buffer = Encoding.UTF8.GetBytes(responseText);

            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/plain; charset=utf-8";
            context.Response.ContentLength64 = buffer.Length;

            using (var output = context.Response.OutputStream)
            {
                output.Write(buffer, 0, buffer.Length);
            }
        }
    }
}