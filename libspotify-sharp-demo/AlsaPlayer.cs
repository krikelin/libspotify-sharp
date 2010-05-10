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

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace libspotifysharpdemo
{
	public class AlsaPlayer : Player
	{
		private bool run = false;
		private AutoResetEvent waitHandle = new AutoResetEvent(false);		
		private Queue<AudioData> buffer = new Queue<AudioData>();
		
		private int maxBufferedFrames = 0;
		private int bufferedFrames = 0;
		
		private Thread playerThread;
		
		private IntPtr pcmPtr = IntPtr.Zero;
		
		public AlsaPlayer(int buffersize)
		{
			run = true;
			this.maxBufferedFrames = buffersize;
			
			playerThread = new Thread(new ThreadStart(PlayerThread));
			playerThread.Priority = ThreadPriority.Highest;
			playerThread.IsBackground = true;
			playerThread.Start();
		}
		
		
		public override void Stop()
		{
			lock(buffer)
			{
				run = false;
				buffer.Clear();
				waitHandle.Set();
			}
		}

        public override int EnqueueSamples(int channels, int rate, byte[] samples, int frames) 
		{
			int consumedFrames = 0;

            if (samples != null && samples.Length > 0 && frames > 0)
			{		
				lock(buffer)
				{			
					if(run && (bufferedFrames <=  maxBufferedFrames || buffer.Count == 0))
					{
                        consumedFrames = frames;
						bufferedFrames += consumedFrames;
						buffer.Enqueue(new AudioData(channels, rate, samples, frames));
						waitHandle.Set();					
					}
				}
			}
			
			return consumedFrames;
		}
		
		
		private void PlayerThread()
		{
			int c;
			
			int written = 0;
			int lastChannels = 0;
			int lastRate = 0;
			
			while(run)
			{
				AudioData ad = null;
				do
				{
					bool isEmpty = true;
					lock(buffer)
						isEmpty = buffer.Count == 0;
					
					if(isEmpty)
						waitHandle.WaitOne();
					
					lock(buffer)
						if(buffer.Count > 0)
						{
							ad = buffer.Dequeue();							
							bufferedFrames -= ad.Frames;
						}
				}
				while(ad == null && run);
				
				if(!run)
					break;
				
				if (pcmPtr == IntPtr.Zero || lastRate != ad.Rate || lastChannels != ad.Channels)
				{
					if (pcmPtr != IntPtr.Zero)
					{
						c = snd_pcm_close(pcmPtr);						
						if(c < 0)
						{
							Console.Error.WriteLine("ALSA close error code {0}.", c);							
						}
						
						pcmPtr = IntPtr.Zero;
					}
	
					lastRate = ad.Rate;
					lastChannels = ad.Channels;	
					
					pcmPtr = OpenDefaultDevice(lastRate, lastChannels, ad.Frames, ad.Frames);					
					if (pcmPtr == IntPtr.Zero)
					{
						Console.Error.WriteLine("Unable to open default ALSA device ({0} channels, {1} Hz). AlsaPlayer bailing out.", lastChannels, lastRate);
						run = false;
						break;
					}
				}
				
				
				c = snd_pcm_wait(pcmPtr, 100);
				if (c >= 0)
					c = snd_pcm_avail_update(pcmPtr);
				
				if(c < 0)
				{	
					if (c == -32)
					{
						Console.Error.WriteLine("Buffer underrun, calling snd_pcm_prepare");
						c = snd_pcm_prepare(pcmPtr);
						if(c < 0)
						{
							Console.Error.WriteLine("ALSA prepare error code {0}. AlsaPlayer bailing out.", c);
							run = false;
							break;
						}
						
						c = snd_pcm_avail_update(pcmPtr);
						
						if(c < 0)
						{
							Console.Error.WriteLine("ALSA avail update after prepare error code {0}. AlsaPlayer bailing out.", c);
							run = false;
							break;
						}					
					}
					else
					{
						Console.Error.WriteLine("ALSA error code {0}. AlsaPlayer bailing out.", c);
						run = false;
						break;
					}
				}
				
				if(c > 0)
				{
					IntPtr samplePtr = IntPtr.Zero;
					try
					{
						samplePtr = Marshal.AllocHGlobal(ad.Samples.Length);
						Marshal.Copy(ad.Samples, 0, samplePtr, ad.Samples.Length);						
						written = snd_pcm_writei(pcmPtr, samplePtr, ad.Frames);						
						
						if(written < 0)
						{
							Console.Error.WriteLine("ALSA write error code {0}. AlsaPlayer bailing out.", c);
							run = false;
							break;
						}
						else if (written != ad.Frames)
						{
							Console.Error.WriteLine("ALSA wrote {0} samples of expected {1}. AlsaPlayer bailing out.", written, ad.Frames);
							run = false;
							break;
						}
					}
					finally
					{
						if(samplePtr != IntPtr.Zero)
							Marshal.FreeHGlobal(samplePtr);
					}
				}			
			}
			
			if(pcmPtr != IntPtr.Zero)
			{
				c = snd_pcm_close(pcmPtr);
						
				if(c < 0)
				{
					Console.Error.WriteLine("ALSA close error code {0}.", c);							
				}
						
				pcmPtr = IntPtr.Zero;
			}
			
			lock(buffer)
			{
				buffer.Clear();
			}
		}
		
		private static IntPtr OpenDefaultDevice(int rate, int channels, int period_size, int buffer_size)
		{
			IntPtr dev = IntPtr.Zero;
			
			IntPtr hwp = IntPtr.Zero;
			IntPtr swp = IntPtr.Zero;
			
	
			int r;
			int dir;
			
			int period_size_min;
			int period_size_max;
			
			int buffer_size_min;
			int buffer_size_max;			
			
					
			r = snd_pcm_open(out dev, "default", 0, 0);
			
			if(r < 0)
			{
				Console.Error.WriteLine("ALSA open device error code {0}.", r);
				return IntPtr.Zero;
			}

			try
			{
				r = snd_pcm_hw_params_sizeof();
				hwp = Marshal.AllocHGlobal(r);
				for(int i = 0; i < r; i++)
					Marshal.WriteByte(hwp, i, 0);
				
				snd_pcm_hw_params_any(dev, hwp);
				snd_pcm_hw_params_set_access(dev, hwp, 3); //SND_PCM_ACCESS_RW_INTERLEAVED)
				snd_pcm_hw_params_set_format(dev, hwp, 2); //SND_PCM_FORMAT_S16_LE)
				snd_pcm_hw_params_set_rate(dev, hwp, rate, 0);
				snd_pcm_hw_params_set_channels(dev, hwp, channels);
	
				dir = 0;
				snd_pcm_hw_params_get_period_size_min(hwp, out period_size_min, ref dir);
				dir = 0;
				snd_pcm_hw_params_get_period_size_max(hwp, out period_size_max, ref dir);
			
				if(period_size < period_size_min)
					period_size = period_size_min;
				if(period_size > period_size_max)
					period_size = period_size_max;
	
				dir = 0;
				r = snd_pcm_hw_params_set_period_size_near(dev, hwp, ref period_size, ref dir);
				
				if(r < 0)
				{
					Console.Error.WriteLine("ALSA set period {0} error code {1}.", period_size, r);
					snd_pcm_close(dev);
					
					return IntPtr.Zero;
				}
				
				dir = 0;
				r = snd_pcm_hw_params_get_period_size(hwp, out period_size, ref dir);
	
				if(r < 0)
				{
					Console.Error.WriteLine("ALSA get period error code {0}.", r);
					snd_pcm_close(dev);
					
					return IntPtr.Zero;
				}
	
				snd_pcm_hw_params_get_buffer_size_min(hwp, out buffer_size_min);
				snd_pcm_hw_params_get_buffer_size_max(hwp, out buffer_size_max);
				
				
				if(buffer_size < buffer_size_min)
					buffer_size = buffer_size_min;
				if(buffer_size > buffer_size_max)
					buffer_size = buffer_size_max;
	
			
				dir = 0;
				r = snd_pcm_hw_params_set_buffer_size_near(dev, hwp, ref buffer_size);
		
				if(r < 0)
				{
					Console.Error.WriteLine("ALSA set hw buffer size {0} error code {1}.", buffer_size, r);
					snd_pcm_close(dev);
					
					return IntPtr.Zero;
				}
	
				r = snd_pcm_hw_params_get_buffer_size(hwp, out buffer_size);
	
				if(r < 0)
				{
					Console.Error.WriteLine("ALSA get hw buffer size error code {0}.", r);
					snd_pcm_close(dev);
					
					return IntPtr.Zero;
				}
				
				r = snd_pcm_hw_params(dev, hwp);
	
				if(r < 0)
				{
					Console.Error.WriteLine("ALSA set hw parameters error code {0}.", r);
					snd_pcm_close(dev);
					
					return IntPtr.Zero;
				}
				
				r = snd_pcm_sw_params_sizeof();
				swp = Marshal.AllocHGlobal(r);
				for(int i = 0; i < r; i++)
					Marshal.WriteByte(swp, i, 0);
				
				snd_pcm_sw_params_current(dev, swp);
				
				r = snd_pcm_sw_params_set_avail_min(dev, swp, period_size);
	
				if(r < 0)
				{
					Console.Error.WriteLine("ALSA set sw avail_min size {0} error code {1}.", period_size, r);
					snd_pcm_close(dev);
					
					return IntPtr.Zero;
				}
	
				snd_pcm_sw_params_set_start_threshold(dev, swp, period_size);
	
				if(r < 0)
				{
					Console.Error.WriteLine("ALSA set sw start_threshold {0} error code {1}.", period_size, r);
					snd_pcm_close(dev);
					
					return IntPtr.Zero;
				}
	
				r = snd_pcm_sw_params(dev, swp);
	
				if(r < 0)
				{
					Console.Error.WriteLine("ALSA set sw parameters error code {0}.", r);
					snd_pcm_close(dev);
					
					return IntPtr.Zero;
				}
	
				r = snd_pcm_prepare(dev);
				if(r < 0)
				{
					Console.Error.WriteLine("ALSA prepare error code {0}.", r);					
					snd_pcm_close(dev);
					
					return IntPtr.Zero;
				}
			}
			finally
			{
				if(hwp != IntPtr.Zero)
					Marshal.FreeHGlobal(hwp);
				
				if(swp != IntPtr.Zero)
					Marshal.FreeHGlobal(swp);
			}
			
			return dev;
		}		
		
		[DllImport("libasound")]
		private static extern int snd_pcm_close(IntPtr pcmPtr);
		
		[DllImport("libasound")]
		private static extern int snd_pcm_wait(IntPtr pcmPtr, int timeout);
		
		[DllImport("libasound")]
		private static extern int snd_pcm_avail_update(IntPtr pcmPtr);		
		[DllImport("libasound")]
		private static extern int snd_pcm_prepare(IntPtr pcmPtr);			
		
		[DllImport("libasound")]
		private static extern int snd_pcm_writei(IntPtr pcmPtr, IntPtr samplePtr, int nSamples);
		
		[DllImport("libasound")]
		private static extern int snd_pcm_open(out IntPtr pcmPtr, string name, int stream, int mode);
		
		[DllImport("libasound")]
		private static extern int snd_pcm_hw_params_sizeof();
		
		[DllImport("libasound")]
		private static extern int snd_pcm_hw_params_any(IntPtr pcmPtr, IntPtr hwpPtr);
		
		[DllImport("libasound")]
		private static extern int snd_pcm_hw_params_set_access(IntPtr pcmPtr, IntPtr hwpPtr, int access);
		
		[DllImport("libasound")]
		private static extern int snd_pcm_hw_params_set_format(IntPtr pcmPtr, IntPtr hwpPtr, int format);
		
		[DllImport("libasound")]
		private static extern int snd_pcm_hw_params_set_rate(IntPtr pcmPtr, IntPtr hwpPtr, int rate, int dir);
		
		[DllImport("libasound")]
		private static extern int snd_pcm_hw_params_set_channels(IntPtr pcmPtr, IntPtr hwpPtr, int channels);
		
		[DllImport("libasound")]
		private static extern int snd_pcm_hw_params_get_period_size_min(IntPtr hwpPtr, out int frames, ref int dir);
		
		[DllImport("libasound")]
		private static extern int snd_pcm_hw_params_get_period_size_max(IntPtr hwpPtr, out int frames, ref int dir);
		
		[DllImport("libasound")]
		private static extern int snd_pcm_hw_params_set_period_size_near(IntPtr pcmPtr, IntPtr hwpPtr, ref int frames, ref int dir);
		
		[DllImport("libasound")]
		private static extern int snd_pcm_hw_params_get_period_size(IntPtr hwpPtr, out int frames, ref int dir);		
		
		[DllImport("libasound")]
		private static extern int snd_pcm_hw_params_get_buffer_size_min(IntPtr hwpPtr, out int frames);
		
		[DllImport("libasound")]
		private static extern int snd_pcm_hw_params_get_buffer_size_max(IntPtr hwpPtr, out int frames);
		
		[DllImport("libasound")]
		private static extern int snd_pcm_hw_params_set_buffer_size_near(IntPtr pcmPtr, IntPtr hwpPtr, ref int frames);
		
		[DllImport("libasound")]
		private static extern int snd_pcm_hw_params_get_buffer_size(IntPtr hwpPtr, out int frames);
		
		[DllImport("libasound")]
		private static extern int snd_pcm_hw_params(IntPtr pcmPtr, IntPtr hwpPtr);
		
		[DllImport("libasound")]
		private static extern int snd_pcm_sw_params_sizeof();
		
		[DllImport("libasound")]
		private static extern int snd_pcm_sw_params_current(IntPtr pcmPtr, IntPtr swpPtr);
		
		[DllImport("libasound")]
		private static extern int snd_pcm_sw_params_set_avail_min(IntPtr pcmPtr, IntPtr swpPtr, int frames);
		
		[DllImport("libasound")]
		private static extern int snd_pcm_sw_params_set_start_threshold(IntPtr pcmPtr, IntPtr swpPtr, int frames);
		
		[DllImport("libasound")]
		private static extern int snd_pcm_sw_params(IntPtr pcmPtr, IntPtr swpPtr);
	}
}
