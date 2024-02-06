using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace StreamGlass.Audio
{
    public class SoundManager
    {
        private static readonly List<Tuple<string, int>> ms_Outputs = [];
        private static readonly List<Tuple<string, int>> ms_Inputs = [];

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
                            ms_Outputs.Add(new(device.FriendlyName, waveOutDevice));
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

        public static string[] GetOutputsNames()
        {
            List<string> names = [];
            foreach (Tuple<string, int> device in ms_Outputs)
                names.Add(device.Item1);
            return [..names];
        }

        public static string[] GetInputsNames()
        {
            List<string> names = [];
            foreach (Tuple<string, int> device in ms_Inputs)
                names.Add(device.Item1);
            return [.. names];
        }

        public static void PlaySound(string filePath)
        {
            AudioFileReader fileReader = new(filePath);
            //Using WaveOutEvent instead of WaveOut to avoid audio lag in the UI lag a bit
            WaveOutEvent waveOut = new();
            waveOut.Init(fileReader);
            waveOut.Play();
        }

        public static void PlaySound(string filePath, string output)
        {
            AudioFileReader fileReader = new(filePath);
            //Using WaveOutEvent instead of WaveOut to avoid audio lag in the UI lag a bit
            WaveOutEvent waveOut = new();
            foreach (Tuple<string, int> device in ms_Outputs)
            {
                if (device.Item1 == output)
                {
                    waveOut.DeviceNumber = device.Item2;
                    break;
                }
            }
            waveOut.Init(fileReader);
            waveOut.Play();
        }
    }
}
