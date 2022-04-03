using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;
using NAudio.Midi;
using NAudio.Wave;
using NAudio.WaveFormRenderer;
using System.Speech.Synthesis;

namespace AudioEditor
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MidiOut midiOut;
        public WasapiLoopbackCapture capture;
        public WaveFileWriter writer;
        public bool isRecordStart = false;
        public DispatcherTimer timer;
        public int volume = 0;
        public SpeechSynthesizer debugger;
        public string saveDataFilePath = "";
        
        public MainWindow()
        {
            InitializeComponent();

            Initialize();

        }

        public void Initialize()
        {
            debugger = new SpeechSynthesizer();
            InitializeCache();
            this.KeyUp += GlovalWindowKeyEventHandler;
            // 1. Configure Providers
            MaxPeakProvider maxPeakProvider = new MaxPeakProvider();
            RmsPeakProvider rmsPeakProvider = new RmsPeakProvider(200); // e.g. 200
            SamplingPeakProvider samplingPeakProvider = new SamplingPeakProvider(200); // e.g. 200
            AveragePeakProvider averagePeakProvider = new AveragePeakProvider(4); // e.g. 4

            // 2. Configure the style of the audio wave image
            StandardWaveFormRendererSettings myRendererSettings = new StandardWaveFormRendererSettings();
            myRendererSettings.Width = 1080;
            myRendererSettings.TopHeight = 64;
            myRendererSettings.BottomHeight = 64;

            // 3. Define the audio file from which the audio wave will be created and define the providers and settings
            WaveFormRenderer renderer = new WaveFormRenderer();
            string audioFilePath = saveDataFilePath;
            WaveStream audioFileStream = null;
            try
            {
                audioFileStream = new NAudio.Wave.WaveFileReader(audioFilePath);
                System.Drawing.Image image = renderer.Render(audioFileStream, averagePeakProvider, myRendererSettings);
                image.Save(@"C:\Gleb\audio_editor\chart.png");
                Uri source = new Uri(@"C:\Gleb\audio_editor\chart.png");
                waveForm.BeginInit();
                waveForm.Source = new BitmapImage(source);
                waveForm.EndInit();
            }
            catch (Exception)
            {

            }
            if (audioFileStream != null) {
                audioFileStream.Dispose();
                audioFileStream.Close();
            }

            InitializeMidiTimeLineRecord();

        }

        private void MIDIMessageReceived(object sender, MidiInMessageEventArgs e)
        {
            if (e.MidiEvent is NoteEvent)
            {
                debugger.Speak("Отлавливаю ноты");
            }
        }

        public void InitializeCache()
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\AudioEditor\record.wav";
            string cachePath = localApplicationDataFolderPath + @"\OfficeWare\AudioEditor";
            bool isCacheFolderExists = Directory.Exists(cachePath);
            bool isCacheFolderNotExists = !isCacheFolderExists;
            if (isCacheFolderNotExists)
            {
                Directory.CreateDirectory(cachePath);
            }
        }

        public void GlovalWindowKeyEventHandler(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Key currentKey = e.Key;
            Key qKey = Key.Q;
            Key wKey = Key.W;
            Key eKey = Key.E;
            Key rKey = Key.R;
            Key tKey = Key.T;
            Key yKey = Key.Y;
            Key uKey = Key.U;
            bool isQKey = currentKey == qKey;
            bool isWKey = currentKey == wKey;
            bool isEKey = currentKey == eKey;
            bool isRKey = currentKey == rKey;
            bool isTKey = currentKey == tKey;
            bool isYKey = currentKey == yKey;
            bool isUKey = currentKey == uKey;

            if (isQKey)
            {
                PlayNote(60);
            }
            else if (isWKey)
            {
                PlayNote(70);
            }
            else if (isEKey)
            {
                PlayNote(80);
            }
            else if (isRKey)
            {
                PlayNote(90);
            }
            else if (isTKey)
            {
                PlayNote(100);
            }
            else if (isYKey)
            {
                PlayNote(110);
            }
            else if (isUKey)
            {
                PlayNote(120);
            }
        }

        public async void DataAvailableHandler(object sender, WaveInEventArgs e)
        {
            try
            {
                await writer.WriteAsync(e.Buffer, 0, e.BytesRecorded);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public void TimeCodeUpdateHandler(object sender, EventArgs e)
        {
            String oneCharPrefix = "0";
            char timePartsSeparator = ':';
            int countSecondsInMinute = 60;
            int initialSeconds = 0;
            int countMinutesInHour = 60;
            int initialMinutes = 0;
            String titleText = timeCodeLabel.Text;
            String[] timeParts = titleText.Split(timePartsSeparator);
            String rawHours = timeParts[0];
            String rawMinutes = timeParts[1];
            String rawSeconds = timeParts[2];
            int hours = Int32.Parse(rawHours);
            int minutes = Int32.Parse(rawMinutes);
            int seconds = Int32.Parse(rawSeconds);
            seconds++;
            bool isToggleSecond = seconds == countSecondsInMinute;
            if (isToggleSecond)
            {
                seconds = initialSeconds;
                minutes++;
                bool isToggleMinute = minutes == countMinutesInHour;
                if (isToggleMinute)
                {
                    minutes = initialMinutes;
                    hours++;
                }
            }
            String updatedHoursText = hours.ToString();
            int countHoursChars = updatedHoursText.Length;
            bool isAddHoursPrefix = countHoursChars == 1;
            if (isAddHoursPrefix)
            {
                updatedHoursText = oneCharPrefix + updatedHoursText;
            }
            String updatedMinutesText = minutes.ToString();
            int countMinutesChars = updatedMinutesText.Length;
            bool isAddMinutesPrefix = countMinutesChars == 1;
            if (isAddMinutesPrefix)
            {
                updatedMinutesText = oneCharPrefix + updatedMinutesText;
            }
            String updatedSecondsText = seconds.ToString();
            int countSecondsChars = updatedSecondsText.Length;
            bool isAddSecondsPrefix = countSecondsChars == 1;
            if (isAddSecondsPrefix)
            {
                updatedSecondsText = oneCharPrefix + updatedSecondsText;
            }
            String currentTime = updatedHoursText + ":" + updatedMinutesText + ":" + updatedSecondsText;
            timeCodeLabel.Text = currentTime;
            timeLineCursor.Value += 1;
        }

        public async void PlayNote(int note)
        {
            midiOut = new MidiOut(0);
            // volume может быть от 0 до 127
            MidiMessage msg = MidiMessage.StartNote(note, volume, 1);
            int msgData = msg.RawData;
            // 3 параметр StartNote или StopNote от 1 до 16
            midiOut.Send(msgData);
            Thread.Sleep(1000);
            midiOut.Send(MidiMessage.StopNote(note, 50, 1).RawData);
            Thread.Sleep(1000);
            midiOut.Close();
            midiOut.Dispose();
            PackIcon keyFrame = new PackIcon();
            keyFrame.Kind = PackIconKind.MusicNote;
            Canvas.SetLeft(keyFrame, keyFrames.ActualWidth / 100 * timeLineCursor.Value);
            keyFrames.Children.Add(keyFrame);
        }

        private void VolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            volume = ((int)(e.NewValue));
        }

        private void SaveHandler(object sender, RoutedEventArgs e)
        {
            Save();
        }

        public async void Save()
        {
            capture.StopRecording();
            capture.Dispose();
            writer.Close();
            writer.Dispose();
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
            sfd.Title = "Выберите папку назначения и имя нового файла";
            sfd.FileName = "project.wav";
            sfd.DefaultExt = ".wav";
            sfd.Filter = "Wav documents (.wav)|*.wav";
            bool? res = sfd.ShowDialog();
            if (res != false)
            {
                string fullPath = sfd.FileName;
                try
                {
                    File.Copy(saveDataFilePath, fullPath);
                }
                catch (Exception)
                {

                }
            }
        }

        private void ToggleRecordHandler(object sender, RoutedEventArgs e)
        {
            ToggleRecord();
        }

        public void InitializeMidiTimeLineRecord()
        {
            capture = new WasapiLoopbackCapture();
            writer = new WaveFileWriter(saveDataFilePath, capture.WaveFormat);
            capture.DataAvailable += DataAvailableHandler;
            capture.StartRecording();
        }

        public void ToggleRecord()
        {
            timeLineCursor.Value = 0;
            if (isRecordStart)
            {
                capture.StopRecording();
                capture.Dispose();
                writer.Close();
                writer.Dispose();
                isRecordStart = false;
                recordBtnIcon.Kind = PackIconKind.Record;
                recordBtnIcon.Foreground = System.Windows.Media.Brushes.Black;
                timer.Stop();
                timeCodeLabel.Text = "00:00:00";
            
                InitializeMidiTimeLineRecord();

            }
            else
            {
                capture.StopRecording();
                writer.Close();
                capture = new WasapiLoopbackCapture();
                writer = new WaveFileWriter(saveDataFilePath, capture.WaveFormat);
                capture.DataAvailable += DataAvailableHandler;
                capture.StartRecording();
                isRecordStart = true;
                recordBtnIcon.Kind = PackIconKind.Stop;
                recordBtnIcon.Foreground = System.Windows.Media.Brushes.Red;
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += TimeCodeUpdateHandler;
                timer.Start();

            }
        }

        private void PlayAudioRecordHandler(object sender, RoutedEventArgs e)
        {
            PlayAudioRecord(saveDataFilePath);
        }

        public void PlayAudioRecord(string source)
        {
            capture.StopRecording();
            capture.Dispose();
            writer.Close();
            writer.Dispose();
            WaveFileReader audio = new WaveFileReader(source);
            IWavePlayer player = new WaveOut(WaveCallbackInfo.FunctionCallback());
            player.Volume = 1.0f;
            player.Init(audio);
            player.Play();
            while (true)
            {
                if (player.PlaybackState == NAudio.Wave.PlaybackState.Stopped)
                {
                    player.Dispose();
                    audio.Close();
                    audio.Dispose();

                    InitializeMidiTimeLineRecord();

                    break;
                }
            };
        }

        private void PlayAudioRecordFromFileHandler(object sender, RoutedEventArgs e)
        {
            PlayAudioRecordFromFile();
        }

        public void PlayAudioRecordFromFile()
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            bool? res = ofd.ShowDialog();
            if (res != false)
            {
                Stream myStream;
                if ((myStream = ofd.OpenFile()) != null)
                {
                    string file_name = ofd.FileName;
                    PlayAudioRecord(file_name);
                }
            }
        }

    }
}
