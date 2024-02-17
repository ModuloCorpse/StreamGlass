using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace StreamGlass.Core.Audio
{
    public class SoundManager
    {
        class AudioOutput : IDisposable
        {
            //Using WaveOutEvent instead of WaveOut to avoid audio lag in the UI lag a bit
            private readonly WaveOutEvent m_WaveOut;
            private readonly MixingSampleProvider m_Mixer;
            private readonly string m_Name;

            public string Name => m_Name;

            public AudioOutput(string name, int device)
            {
                m_Name = name;
                m_WaveOut = new() { DeviceNumber = device };
                m_Mixer = new(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2)) { ReadFully = true };
                m_WaveOut.Init(m_Mixer);
                m_WaveOut.Play();
            }

            public void PlaySound(CachedSound sound)
            {
                CachedSoundSampleProvider provider = new(sound);
                m_Mixer.AddMixerInput((provider.WaveFormat.Channels == 1) ? new MonoToStereoSampleProvider(provider) : provider);
            }

            public void Dispose() => m_WaveOut.Dispose();
        }

        private class CachedSound
        {
            private readonly WaveFormat m_WaveFormat;
            private readonly float[] m_AudioData;
            private volatile uint m_References = 1;

            public WaveFormat WaveFormat => m_WaveFormat;
            public float[] AudioData => m_AudioData;
            public uint References => m_References;

            public CachedSound(string audioFileName)
            {
                using var audioFileReader = new AudioFileReader(audioFileName);
                m_WaveFormat = audioFileReader.WaveFormat;
                List<float> wholeFile = new((int)(audioFileReader.Length / 4));
                var readBuffer = new float[audioFileReader.WaveFormat.SampleRate * audioFileReader.WaveFormat.Channels];
                int samplesRead;
                while ((samplesRead = audioFileReader.Read(readBuffer, 0, readBuffer.Length)) > 0)
                    wholeFile.AddRange(readBuffer.Take(samplesRead));
                m_AudioData = [.. wholeFile];
            }

            public void AddReference() => m_References++;
            public void RemoveReference() => m_References--;
        }

        private class CachedSoundSampleProvider(CachedSound cachedSound) : ISampleProvider
        {
            private readonly CachedSound m_CachedSound = cachedSound;
            private long m_Position;
            public WaveFormat WaveFormat => m_CachedSound.WaveFormat;
            public int Read(float[] buffer, int offset, int count)
            {
                var availableSamples = m_CachedSound.AudioData.Length - m_Position;
                var samplesToCopy = Math.Min(availableSamples, count);
                Array.Copy(m_CachedSound.AudioData, m_Position, buffer, offset, samplesToCopy);
                m_Position += samplesToCopy;
                return (int)samplesToCopy;
            }
        }

        private static readonly AudioOutput ms_DefaultOutput = new("Default", -1);
        private static readonly List<Tuple<string, int>> ms_Inputs = [];
        private static readonly Dictionary<string, AudioOutput> ms_Outputs = [];
        private static readonly Dictionary<string, CachedSound> ms_CachedSounds = [];

        static SoundManager()
        {
            MMDeviceEnumerator enumerator = new();
            int waveOutDevices = WaveOut.DeviceCount;
            if (waveOutDevices > 0)
            {
                MMDeviceCollection renderDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.All);
                for (int waveOutDevice = 0; waveOutDevice < waveOutDevices; waveOutDevice++)
                {
                    WaveOutCapabilities deviceInfo = WaveOut.GetCapabilities(waveOutDevice);
                    foreach (MMDevice device in renderDevices)
                    {
                        if (device.FriendlyName.StartsWith(deviceInfo.ProductName))
                        {
                            AudioOutput audioOutput = new(device.FriendlyName, waveOutDevice);
                            ms_Outputs[audioOutput.Name] = audioOutput;
                            break;
                        }
                    }
                }
            }

            int waveInDevices = WaveIn.DeviceCount;
            if (waveInDevices > 0)
            {
                MMDeviceCollection captureDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.All);
                for (int waveInDevice = 0; waveInDevice < waveInDevices; waveInDevice++)
                {
                    WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(waveInDevice);
                    foreach (MMDevice device in captureDevices)
                    {
                        if (device.FriendlyName.StartsWith(deviceInfo.ProductName))
                        {
                            ms_Inputs.Add(new(device.FriendlyName, waveInDevice));
                            break;
                        }
                    }
                }
            }
        }

        public static string[] GetOutputsNames() => [.. ms_Outputs.Keys];

        internal static void LoadSound(string file)
        {
            if (!ms_CachedSounds.TryGetValue(file, out CachedSound? sound))
                ms_CachedSounds[file] = new(file);
            else
                sound!.AddReference();
        }

        internal static void UnloadSound(string file)
        {
            if (ms_CachedSounds.TryGetValue(file, out CachedSound? sound))
            {
                sound!.RemoveReference();
                if (sound.References == 0)
                    ms_CachedSounds.Remove(file);
            }
        }

        public static void PlaySound(Sound sound)
        {
            if (!string.IsNullOrEmpty(sound.File))
            {
                if (!ms_CachedSounds.TryGetValue(sound.File, out CachedSound? cachedSound))
                {
                    cachedSound = new(sound.File);
                    ms_CachedSounds[sound.File] = cachedSound;
                }
                if (!ms_Outputs.TryGetValue(sound.Output, out AudioOutput? output))
                    output = ms_DefaultOutput;
                output.PlaySound(cachedSound);
            }
        }
    }
}
