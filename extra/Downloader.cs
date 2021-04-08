// System
using System;
using System.IO;
using System.Net;

// Discord
using Discord;

// Mate
using Mate.Utility;

namespace Mate.Extra
{
    public class Downloader
    {
        /// <summary>
        /// Downloads a file
        /// </summary>
        /// <returns>TRUE if successful, otherwise FALSE</returns>
        public static bool Download(string URL, string destination) {
            int bufferSize = 1024;
            bufferSize *= 1000;
            long existLen = 0;
            
            HttpWebResponse httpRes;
            HttpWebRequest httpReq;
            Stream resStream;
            FileStream saveFileStream;
            
            // Figure out how much is already downloaded
            /*if (File.Exists(destination)) {
                FileInfo destinationFileInfo = new FileInfo(destination);
                existLen = destinationFileInfo.Length;
            }

            if (existLen > 0)
                saveFileStream = new FileStream(destination, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            else*/
                saveFileStream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            
            // Create a HTTP/GET request
            httpReq = (HttpWebRequest) HttpWebRequest.Create(URL);
            httpReq.AddRange((int) existLen);
            
            // Get the response
            try {
                httpRes = (HttpWebResponse) httpReq.GetResponse();
                resStream = httpRes.GetResponseStream();
            } catch (WebException wex) {
                Logger.Log(new LogMessage(LogSeverity.Warning, "Downloader", $"Failed to download {URL}, server responed with {wex.Status}", wex));
                return false;
            } catch (Exception ex) {
                Logger.Log(new LogMessage(LogSeverity.Warning, "Downloader", $"Failed to download {URL}", ex));
                return false;
            }

            // Then proceed to download the file
            int byteSize;
            byte[] downBuffer = new byte[bufferSize];
        
            while ((byteSize = resStream.Read(downBuffer, 0, downBuffer.Length)) > 0)
            {
                saveFileStream.Write(downBuffer, 0, byteSize);
            }
            saveFileStream.Close();
            saveFileStream.Dispose();

            return true;
        }
    }
}