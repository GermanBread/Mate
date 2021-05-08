// System
using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

// Discord
using Discord;

// Mate
using Mate.Utility;
using Mate.Variables;

namespace Mate.Extra
{
    public sealed class Webserver
    {
        private CancellationTokenSource tokenSource;
        private CancellationToken cancelToken;
        private HttpListener webListener;
        private int port;
        private string[] domainNames;
        private string htmlBaseDir = GlobalVariables.HtmlPath;
        // Credit: https://gist.github.com/aksakalli/9191056
        private IDictionary<string, string> mimeTypeMappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {
        #region extension to MIME type list
        {".asf", "video/x-ms-asf"},
        {".asx", "video/x-ms-asf"},
        {".avi", "video/x-msvideo"},
        {".bin", "application/octet-stream"},
        {".cco", "application/x-cocoa"},
        {".crt", "application/x-x509-ca-cert"},
        {".css", "text/css"},
        {".deb", "application/octet-stream"},
        {".der", "application/x-x509-ca-cert"},
        {".dll", "application/octet-stream"},
        {".dmg", "application/octet-stream"},
        {".ear", "application/java-archive"},
        {".eot", "application/octet-stream"},
        {".exe", "application/octet-stream"},
        {".flv", "video/x-flv"},
        {".gif", "image/gif"},
        {".hqx", "application/mac-binhex40"},
        {".htc", "text/x-component"},
        {".htm", "text/html"},
        {".html", "text/html"},
        {".ico", "image/x-icon"},
        {".img", "application/octet-stream"},
        {".iso", "application/octet-stream"},
        {".jar", "application/java-archive"},
        {".jardiff", "application/x-java-archive-diff"},
        {".jng", "image/x-jng"},
        {".jnlp", "application/x-java-jnlp-file"},
        {".jpeg", "image/jpeg"},
        {".jpg", "image/jpeg"},
        {".js", "application/x-javascript"},
        {".mml", "text/mathml"},
        {".mng", "video/x-mng"},
        {".mov", "video/quicktime"},
        {".mp3", "audio/mpeg"},
        {".mpeg", "video/mpeg"},
        {".mpg", "video/mpeg"},
        {".msi", "application/octet-stream"},
        {".msm", "application/octet-stream"},
        {".msp", "application/octet-stream"},
        {".pdb", "application/x-pilot"},
        {".pdf", "application/pdf"},
        {".pem", "application/x-x509-ca-cert"},
        {".pl", "application/x-perl"},
        {".pm", "application/x-perl"},
        {".png", "image/png"},
        {".prc", "application/x-pilot"},
        {".ra", "audio/x-realaudio"},
        {".rar", "application/x-rar-compressed"},
        {".rpm", "application/x-redhat-package-manager"},
        {".rss", "text/xml"},
        {".run", "application/x-makeself"},
        {".sea", "application/x-sea"},
        {".shtml", "text/html"},
        {".sit", "application/x-stuffit"},
        {".swf", "application/x-shockwave-flash"},
        {".tcl", "application/x-tcl"},
        {".tk", "application/x-tcl"},
        {".txt", "text/plain"},
        {".war", "application/java-archive"},
        {".wbmp", "image/vnd.wap.wbmp"},
        {".wmv", "video/x-ms-wmv"},
        {".xml", "text/xml"},
        {".xpi", "application/x-xpinstall"},
        {".zip", "application/zip"},
        #endregion
        };
        /// <summary>
        /// Instantiates a new Webserver
        /// </summary>
        /// <param name="Domain">The domain name</param>
        /// <param name="Port">The port to use</param>
        /// <param name="WebFiles">The HTML files to use (will default to one value: "index.html"). The names will also be checked lowercase.</param>
        public Webserver(string[] Domains, int Port) {
            port = Port;
            domainNames = Domains;
            Directory.CreateDirectory(htmlBaseDir);
            webListener = new HttpListener();
            tokenSource = new CancellationTokenSource();
            cancelToken = tokenSource.Token;
        }
        public async Task StartAsync() {
            await Logger.Log(new LogMessage(LogSeverity.Info, "Webserver", "Starting"));
            webListener.Start();
            for (int i = 0; i < domainNames.Length; i++)
            {
                webListener.Prefixes.Add($"http://{domainNames[i]}:{port.ToString()}/");
                await Logger.Log(new LogMessage(LogSeverity.Info, "Webserver", $"Adding url \"http://{domainNames[i]}:{port.ToString()}/\""));
            }
            await Logger.Log(new LogMessage(LogSeverity.Info, "Webserver", "Ready, visit one of the urls listed to visit the dashboard"));
            while (!cancelToken.IsCancellationRequested) {
                IAsyncResult request = webListener.BeginGetContext(new AsyncCallback(HandleConnection), webListener);
                request.AsyncWaitHandle.WaitOne(100, cancelToken.IsCancellationRequested);
            }
        }
        public async Task StopAsync() {
            await Logger.Log(new LogMessage(LogSeverity.Info, "Webserver", "Stopping, this will most likely take a second"));
            webListener.Stop();
            webListener.Abort();
            tokenSource.Cancel();
        }
        private void HandleConnection(IAsyncResult result) {
            // Cast the variable into a HttpListener variable using black magic
            HttpListener listener = (HttpListener) result.AsyncState;
            
            // Get the context
            HttpListenerContext context = listener.EndGetContext(result);
            
            // Get the underlying request
            HttpListenerRequest request = context.Request;

            if (request.ContentType == "application/json") {
                Logger.SilentLog(new LogMessage(LogSeverity.Info, "Webserver", $"Inbound API connection to {request.Url.LocalPath}"));
                HandleAPIConnection(context);
            }
            else {
                Logger.SilentLog(new LogMessage(LogSeverity.Info, "Webserver", $"Inbound browser connection to {request.Url.LocalPath}"));
                HandleBrowserConnection(context);
            }
        }
        private void HandleBrowserConnection(HttpListenerContext context) {
            // Get the underlying request
            HttpListenerRequest request = context.Request;

            // Obtain a response object
            HttpListenerResponse response = context.Response;

            // Declare our buffer
            byte[] buffer = null;

            // Construct a response.
            string responseString = "";
            try {
                // Every extension in this list will be read as text
                List<string> webFiles = new List<string> {
                    ".css", ".html", ".js"
                };
                
                string htmlDirectory = Path.GetDirectoryName(request.Url.LocalPath);
                string htmlDocument = Path.GetFileName(request.Url.LocalPath);
                
                // Remove the preceeding directory character
                try {
                    if (htmlDirectory[0] == '/') htmlDirectory = htmlDirectory.Substring(1);
                } catch (NullReferenceException) {
                    htmlDirectory = "";
                }

                // Set the appropriate response type
                context.Response.ContentType = mimeTypeMappings.TryGetValue(Path.GetExtension(htmlDocument), out string mime) ? mime : "text/html";

                // If no file is defined. return to index.html
                if (string.IsNullOrEmpty(htmlDocument)) htmlDocument = "index.html";

                // We don't want the user to be able to acess this file (because it looks wierd)
                if (htmlDocument == "error.html") throw new FileNotFoundException("Accessing the error.html file is not allowed");

                string htmlPath = Path.Combine(Path.Combine(htmlBaseDir, htmlDirectory), htmlDocument);

                // Read the file (called html file, but it also refers to css and js)
                if (webFiles.Contains(Path.GetExtension(htmlDocument)))
                    responseString = File.ReadAllText(htmlPath)
                        .Replace("%BOTNAME", GlobalVariables.DiscordBot.Client.CurrentUser.Username)
                        .Replace("%PFPLINK", GlobalVariables.DiscordBot.Client.CurrentUser.GetAvatarUrl(ImageFormat.Png, 512));
                else
                    buffer = File.ReadAllBytes(htmlPath);
            } catch (Exception ex) {
                string errorDocument = Path.Combine(htmlBaseDir, "error.html");
                if (ex is FileNotFoundException || ex is DirectoryNotFoundException || ex is UnauthorizedAccessException || ex is IOException) {
                    if (!File.Exists(errorDocument)) {
                        responseString = "Well this is akward. I can't find the error template... Uh 404? 500?";
                        goto end;
                    }
                    // Just pretend it's a 404
                    response.StatusCode = 404;
                    string header = "404 not found";
                    string error = $"{request.Url.LocalPath} does not exist";
                    responseString = File.ReadAllText(errorDocument)
                        .Replace("%R", header)
                        .Replace("%E", error)
                        .Replace("/*%BANNERWIDTH*/", header.Length.ToString());
                }
                else {
                    // Something something exception thrown
                    response.StatusCode = 500;
                    string header = "I messed up (500 internal error)";
                    string error = ex.ToString();
                    responseString = File.ReadAllText(errorDocument)
                        .Replace("%R", header)
                        .Replace("%E", error)
                        .Replace("/*%BANNERWIDTH*/", header.Length.ToString());
                }
                end:;
            }
            if (buffer == null)
                buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            
            // Set the buffer size
            response.ContentLength64 = buffer.Length;
            
            // Create a stream and write the output
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            
            // You must close the output stream.
            output.Close();

            response.Close();
        }
        
        private void HandleAPIConnection(HttpListenerContext context) {
            // Get the underlying request
            HttpListenerRequest request = context.Request;

            // Obtain a response object
            HttpListenerResponse response = context.Response;

            // Construct a response.
            string responseString = "";
            try {
                responseString = context.HandleRequest();
                response.StatusCode = 200;
            } catch (Exception ex) {
                if (ex is InvalidOperationException) {
                    // Bad request, most likely not an api call
                    response.StatusCode = 400;
                    string header = "400 bad request";
                    string error = ex.Message;
                    responseString = JsonSerializer.Serialize(new string[] {
                        header,
                        error
                    });
                }
                else {
                    // Something something exception thrown
                    response.StatusCode = 500;
                    string header = "500 internal error";
                    string error = ex.ToString();
                    responseString = JsonSerializer.Serialize(new string[] {
                        header,
                        error
                    });
                    Logger.Log(new LogMessage(LogSeverity.Error, "Webserver", "Unhandled exception", ex));
                }
            }
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            
            // Set the buffer size
            response.ContentLength64 = buffer.Length;
            
            // Create a stream and write the output
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            
            // You must close the output stream.
            output.Close();
        }
    }
}