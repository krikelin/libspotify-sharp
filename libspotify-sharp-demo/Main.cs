/*
Copyright (c) 2009 Jonas Larsson, jonas@hallerud.se

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/



/* NOTE
 * This demo does not show all features of this library.
 * What it does is:
 *  - Sets up a Spotify session
 *  - Executes a search
 *  - Plays the first result in the list
 *  - Requests cover, saves it to disk, display it (depending on your system)
 *  - Logs out
 * 
 * 
 * You must have libspotify installed in monos search path. Without
 * that everything will fail. make install as root on libspotify should
 * work. (sudo make install for some). For playback to work you also need alsa
 * (libasound). Most distros already have that available.
 * 
 * See here for mono search path
 * http://www.mono-project.com/Interop_with_Native_Libraries#Linux_Shared_Library_Search_Path
 * 
 * If you plan to use libspotify-sharp, I really recommend that you
 * study the source for a while.
 * 
*/
using System;
using System.Threading;

using Spotify;

namespace libspotifysharpdemo
{
	class MainClass
	{
		private static AutoResetEvent playbackDone = new AutoResetEvent(false);
		private static AutoResetEvent loggedOut = new AutoResetEvent(false);
		private static Track currentTrack = null;
		private static AlsaPlayer player = null;
		
		public static void Main(string[] args)
		{
			// If running in MonoDevelop, set these in code directly
			// and start w.o. parameters.
			string username = string.Empty;
			string password = string.Empty;
			
			if(args.Length == 2)
			{
				username = args[0];
				password = args[1];
			}
			
			if(string.IsNullOrEmpty(password) || string.IsNullOrEmpty(username))
			{
				Console.WriteLine("Usage: demo.exe username password");
				return;
			}
			
			#region key
			
			// This key is not mine. It was found on a public website. Please don't misuse.
						
			byte[] key = new Byte[]
			{
				0x01, 0x56, 0xFC, 0x14, 0x35, 0x86, 0x20, 0xF1, 0x69, 0xC6, 0x9B, 0x7C, 0xD3, 0x11, 0xAB, 0x56,
				0x3E, 0x1F, 0xF3, 0xB1, 0x58, 0xD4, 0x07, 0xF3, 0x51, 0xCF, 0xC1, 0x1D, 0xF8, 0xCF, 0x49, 0x73,
				0x9F, 0xFC, 0x66, 0x02, 0xA1, 0xCE, 0x82, 0x08, 0xBE, 0xF3, 0x89, 0xAA, 0xBD, 0x75, 0x42, 0x19,
				0x60, 0x45, 0xBF, 0x39, 0x70, 0x8C, 0x6E, 0xA9, 0x37, 0xE1, 0x5B, 0x54, 0xD9, 0x29, 0x1D, 0xEE,
				0xBF, 0x2B, 0x11, 0xD2, 0xF0, 0x28, 0xF3, 0xD4, 0x1D, 0x26, 0x99, 0xA6, 0x8A, 0xC8, 0xA8, 0xAE,
				0xC1, 0x98, 0x87, 0x4B, 0x4A, 0xB9, 0xD6, 0x6A, 0x90, 0x51, 0xA0, 0x4D, 0x4D, 0xA5, 0xCB, 0x66,
				0xC8, 0x5D, 0x3F, 0xE8, 0x1B, 0x6E, 0x22, 0xFF, 0x4F, 0xA5, 0x5C, 0x06, 0x14, 0x25, 0xD0, 0x74,
				0xBD, 0x81, 0x48, 0xDE, 0x47, 0x69, 0x4D, 0xF4, 0xE5, 0x6E, 0xB8, 0x26, 0x3B, 0x06, 0xFE, 0x0D,
				0x84, 0x55, 0x3F, 0x37, 0x67, 0x11, 0x14, 0xF3, 0x4A, 0x17, 0xC0, 0x50, 0x9D, 0x48, 0x9D, 0x95,
				0x93, 0xB4, 0x27, 0xB6, 0x27, 0x51, 0x99, 0xCA, 0xA7, 0xB3, 0xE9, 0x1C, 0x3B, 0x89, 0x2A, 0xE7,
				0x18, 0xFF, 0xF6, 0xB6, 0xAE, 0xB2, 0x17, 0x5A, 0x33, 0x61, 0x08, 0x9D, 0xE3, 0x03, 0xFD, 0x7D,
				0x12, 0x68, 0x24, 0x6D, 0xCF, 0x6F, 0xA8, 0x87, 0x06, 0x27, 0xED, 0x4A, 0xB7, 0x13, 0x23, 0xAA,
				0x62, 0xA2, 0x21, 0xC0, 0x0E, 0x2F, 0xF3, 0x47, 0x1D, 0xFD, 0x3D, 0x06, 0x10, 0x7D, 0xA2, 0xFB,
				0x63, 0xF9, 0x04, 0x20, 0x20, 0xE7, 0x28, 0x6B, 0x6F, 0xD6, 0x7A, 0x61, 0x33, 0x76, 0x2A, 0xA4,
				0x3E, 0xEE, 0x40, 0xE8, 0x07, 0x99, 0xDA, 0xEA, 0x63, 0x65, 0x21, 0x22, 0x30, 0x0A, 0xF1, 0xD5,
				0x46, 0xAA, 0x8C, 0x06, 0x57, 0xB7, 0xB4, 0x8A, 0xDE, 0xFE, 0xA9, 0xB8, 0xA3, 0x03, 0xF0, 0xDB,
				0x4C, 0x38, 0xC0, 0x57, 0xC1, 0x47, 0xBD, 0xC7, 0x24, 0x7E, 0xBB, 0x37, 0xD2, 0xFA, 0x4D, 0x5F,
				0x03, 0x23, 0xC6, 0x53, 0xD9, 0x43, 0xCA, 0xDF, 0x84, 0x72, 0x1A, 0x06, 0xF1, 0x93, 0xAB, 0x2A,
				0x52, 0xAB, 0xEB, 0x79, 0x9F, 0x74, 0xBF, 0xE7, 0xAC, 0x95, 0xCB, 0x63, 0xCE, 0x18, 0x08, 0x99,
				0x19, 0x17, 0x36, 0x9D, 0x9C, 0x7E, 0x82, 0xDC, 0x83, 0xDC, 0xA8, 0x8D, 0x30, 0x2D, 0xF4, 0xC7,
				0xD6
			};
			
			#endregion
			
			AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
			
			Session s = Session.CreateInstance(key, "/tmp/libspotify", "/tmp/libspotify", "libspotify-sharp-test");
			
			s.OnConnectionError += HandleOnConnectionError;
			s.OnLoggedOut += HandleOnLoggedOut;
			s.OnLoginComplete += HandleOnLoginComplete;
			s.OnLogMessage += HandleOnLogMessage;
			s.OnMessageToUser += HandleOnMessageToUser;			
			s.OnPlayTokenLost += HandleOnPlayTokenLost;			
            s.OnSearchComplete += HandleOnSearchComplete;			
			s.OnMusicDelivery += HandleOnMusicDelivery;
			s.OnEndOfTrack += HandleOnEndOfTrack;
			s.OnImageLoaded += HandleOnImageLoaded;

			Console.WriteLine("Logging in...");
			s.LogIn(username, password);
			
			playbackDone.WaitOne();
			Console.WriteLine("Logging out..");
			s.LogOut();
			loggedOut.WaitOne(5000, false);
			Console.WriteLine("Logged out");
			// FIXME
			// This is really ugly. However, mono doesn't exit even if all our threads are
			// terminated. libspotify internal threads are are still active and prevents mono
			// from exiting. Should be done with some other signal than SIGKILL.
			System.Diagnostics.Process.GetCurrentProcess().Kill();
		}	
		
		// Event callbacks
		
		static void HandleOnLoginComplete(Session sender, SessionEventArgs e)
		{
			Console.WriteLine("Login result: " + e.Status);			
			Console.WriteLine("Searching for 'green day idiot'... ");
			
			
			// RequestId can be whatever. It's never used internally.
			// If several searches are active at the same time, it can be used
			// too tell them apart.
			sender.Search("green day idiot", 0, 500, 0, 500, 0, 500, null);
		}
		
		static void HandleOnSearchComplete(Session sender, SearchEventArgs e)
        {
            Console.WriteLine("Search returned:{0}{1}", Environment.NewLine, e.Result);
			
			if(e.Result.Tracks.Length > 0)
			{
				currentTrack = e.Result.Tracks[0];
				Console.WriteLine("Will play track:");
				Console.WriteLine(currentTrack.ToString());
				
				Console.WriteLine("Album:");
				Console.WriteLine(currentTrack.Album.ToString());
				
				Console.WriteLine("Load: " + sender.PlayerLoad(currentTrack));
				Console.WriteLine("Play: " + sender.PlayerPlay(true));
				if(sender.LoadImage(currentTrack.Album.CoverId, 12345))
					Console.WriteLine("Cover requested");
				else
					Console.WriteLine("Cover request failed");
				
			}
		}
		
		static void HandleOnMusicDelivery(Session sender, MusicDeliveryEventArgs e)
		{
			if(e.Samples.Length > 0)
			{
				if(player == null)
				{
					player = new AlsaPlayer(e.Rate / 2); // Buffer 500ms of audio
					Console.WriteLine("Player created with buffer size {0} frames", e.Rate / 2);
				}
				
				// Don't forget to set how many frames we consumed
				e.ConsumedFrames = player.EnqueueSamples(new AudioData(e.Channels, e.Rate, e.Samples, e.Frames));
			}
			else
			{
				e.ConsumedFrames = 0;
			}
		}
		
		static void HandleOnEndOfTrack(Session sender, SessionEventArgs e)
		{
			Console.WriteLine("End of music delivery. Flushing player buffer...");
			Thread.Sleep(510); // Samples left in player buffer. Player lags 500 ms
			player.Stop();
			player = null;
			Console.WriteLine("Playback complete");
			playbackDone.Set();
		}

		static void HandleOnImageLoaded(Session sender, ImageEventArgs e)
		{
			Console.WriteLine("Image with id {0} loaded. {1}",
				e.Id, string.IsNullOrEmpty(e.Error) ? string.Empty : "Error: " + e.Error);
			
			if(e.Image != null)
			{
				try
				{
					e.Image.Save("cover.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
					Console.WriteLine("Cover saved to cover.jpg");
					System.Diagnostics.Process.Start("cover.jpg");
				}
				catch
				{
				}
			}
		}
		
		static void HandleOnPlayTokenLost(Session sender, SessionEventArgs e)
		{
			Console.Out.WriteLine("Play token lost");
			playbackDone.Set();
		}
		
		static void HandleOnMessageToUser(Session sender, SessionEventArgs e)
		{			
			Console.WriteLine("Message: " + e.Message);
		}

		static void HandleOnLogMessage(Session sender, SessionEventArgs e)
		{
			Console.WriteLine("Log: " + e.Message);
		}		

		static void HandleOnLoggedOut(Session sender, SessionEventArgs e)
		{	
			playbackDone.Set();
			loggedOut.Set();
		}

		static void HandleOnConnectionError(Session sender, SessionEventArgs e)
		{
			Console.WriteLine("Connection error: " + e.Status);
		}

		static void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Console.WriteLine(e.ExceptionObject.ToString());
		}
	}
}