//
// This autonomous intelligent system software is the property of Cartheur Robotics, spol. s r.o. Copyright 2021, all rights reserved.
//
#define Windows
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Net;
using System.Text;
using System.Threading;
using System.Timers;
using System.Xml;
using NeSpeak;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Cartheur.Animals.Control;
using Cartheur.Animals.Core;
using Cartheur.Animals.FileLogic;
using Cartheur.Animals.Personality;
using Cartheur.Animals.Utilities;
using Cartheur.Speech;
using System.Net.NetworkInformation;
using ArduinoUploader;
using ArduinoUploader.Hardware;
#if Windows
using System.Speech.Recognition;
using System.Speech.Synthesis;
#endif

namespace Cartheur.Huggable.Console
{
    /// <summary>
    /// Application wrapper of the Aeon algorithm. A friendly companion you can take with you anywhere, always.
    /// </summary>
    /// <remarks>Multiplatform code: Windows and Linux. The latter is the ultimate target.</remarks>
    class Program
    {
        // Configuration of the application.
        public static LoaderPaths Configuration;
        public static bool StartUpTheme { get; set; }
        public static string StartUpThemeFile { get; set; }
        public static bool TerminalMode { get; set; }
        public static Process TerminalProcess { get; set; }
        // Aeon's personal algorithmic items.
        private static Aeon _thisAeon;
        private static User _thisUser;
        private static Request _thisRequest;
        private static Result _thisResult;
        private static DateTime _aeonChatStartedOn;
        private static TimeSpan _aeonChatDuration;
        private static Thread _aeonAloneThread;
        private static Thread _moodicThread;
        // Aeon's status.
        private static bool SettingsLoaded { get; set; }
        private static bool AeonLoaded { get; set; }
        public static string UserInput { get; set; }
        public static string AeonOutputDialogue { get; set; }
        public static string AeonOutputDebug { get; set; }
        public static int AloneMessageOutput { get; set; }
        public static int PreviousAloneMessageOutput { get; set; }
        public static int AloneMessageVariety { get; set; }
        public static string LastOutput { get; set; }
        public bool EncryptionUsed { get; set; }
        public int AeonSize { get; set; }
        public static string AeonType { get; set; }
        public static bool AeonIsAlone { get; set; }
        public static string AloneTextCurrent { get; set; }
        public static string XmsDirectoryPath { get; set; }
        public static string NucodeDirectoryPath { get; set; }
        public static string PythonLocation { get; set; }
        // Aeon's mood, interaction, and manifest personality.
        public static bool TestHardware { get; set; }
        private static int SwitchMood { get; set; }
        public static bool DetectUserEmotion { get; set; }
        private static string EmotiveEquation { get; set; }
        public static Mood AeonMood { get; set; }
        public static string AeonCurrentMood { get; set; }
        // Vocal introduction of the start-up experience.
        private static SoundPlayer AnimalActive { get; set; }
        // Use Arduino hardware to include behavior features.
        private static bool EmotionalDisplayUsed { get; set; }
        private static bool MoodicDisplayUsed{ get; set; }
        private static bool MoodicDurationUsed { get; set; }
        private static bool SketchUploaded { get; set; }
        private static Thread SketchThread { get; set; }
        private static string LastSketch { get; set; }
        private static double MoodicDuration { get; set; }
        // Speech recognition and synthesizer engines.
        public static bool SapiWindowsUsed { get; set; }
        public static bool PocketSphinxUsed { get; set; }
        // For RabbitMQ messaging on pocketsphinx output.
        public static ConnectionFactory Factory { get; set; }
        public static IModel Channel { get; private set; }
        public static EventingBasicConsumer Consumer { get; set; }
        public static Process SphinxProcess { get; set; }
        public static readonly string SphinxStartup = @"pocketsphinx_continuous -hmm /usr/share/pocketsphinx/model/hmm/en_US/en-us -dict /usr/share/pocketsphinx/model/lm/en_US/cmudict-en-us.dict -lm /usr/share/pocketsphinx/model/lm/en_US/en-us.lm.bin -inmic yes -backtrace yes -logfn /dev/null";
        // For remote puppeteering.
        static readonly WebClient HenryClient = new WebClient();
        public static bool UsePythonBottle { get; set; }
        // For voice synthetizer
        public static bool SpeechSynthesizerUsed { get; set; }
        static ESpeakLibrary PresenceSpeaker { get; set; }
        public static string PresenceSpeakerVoice { get; set; }
        // Speech recognizer and synthesizer for Windows.
#if Windows
        static readonly SpeechRecognitionEngine Recognizer = new SpeechRecognitionEngine();
        static readonly GrammarBuilder GrammarBuilder = new GrammarBuilder();
        static readonly SpeechSynthesizer SpeechSynth = new SpeechSynthesizer();
        static readonly PromptBuilder PromptBuilder = new PromptBuilder();
#endif
        // External hardware and libraries.
        public static bool BottleServerConnected { get; private set; }
        public static string BottleIpAddress { get; set; }
        public static Process RobotProcess { get; set; }
        public static string RobotProcessOutput { get; set; }
        public static int RobotProcessExitCode { get; set; }
        public static bool CorrectExecution { get; set; }
        // Facial movements in three-dimensions.
        public static int NumberOfMotors { get; set; }
        public static int EyesOpenGpio { get; set; }
        public static int EyesCloseGpio { get; set; }
        public static int NoseOpenGpio { get; set; }
        public static int NoseCloseGpio { get; set; }
        public static int MouthOpenGpio { get; set; }
        public static int MouthCloseGpio { get; set; }
        // Additive huggable feature-set.
        private static SoundPlayer EmotiveResponse { get; set; }
        public static string EmotiveA { get; set; }
        public static string EmotiveB { get; set; }
        public static string EmotiveC { get; set; }
        private static SoundPlayer TonalResponse { get; set; }
        public static string TonalRootPath { get; set; }
        public static string TonalA { get; set; }
        public static string TonalB { get; set; }
        public static string TonalC { get; set; }
        public static string TonalD { get; set; }
        public static string TonalE { get; set; }
        public static string TonalF { get; set; }
        public static string TonalFs { get; set; }
        public static string TonalG { get; set; }
        public static string TonalAp { get; set; }
        public static int TonalDelay { get; set; }
        public static bool SpeechToTonal { get; set; }
        public static bool TonalSpeechLimit { get; set; }
        public static int TonalSpeechLimitValue { get; set; }
        public static int Repetition { get; set; }
        /// <summary>
        /// Where the action takes place.
        /// </summary>
        static void Main()
        {
            Configuration = new LoaderPaths("Debug");
            Logging.ActiveConfiguration = Configuration.ActiveRuntime;
            // Create the aeon and load its basic parameters from a config file.
            _thisAeon = new Aeon("1+2i");
            _thisAeon.LoadSettings(Configuration.PathToSettings);
            SettingsLoaded = _thisAeon.LoadDictionaries(Configuration);
            _thisUser = new User(_thisAeon.GlobalSettings.GrabSetting("username"), _thisAeon);
            System.Console.WriteLine(_thisAeon.GlobalSettings.GrabSetting("product") + " - Version " + _thisAeon.GlobalSettings.GrabSetting("version") + ".");
            System.Console.WriteLine(_thisAeon.GlobalSettings.GrabSetting("ip") + ".");
            System.Console.WriteLine(_thisAeon.GlobalSettings.GrabSetting("claim") + ".");
            Thread.Sleep(700);
            System.Console.WriteLine(_thisAeon.GlobalSettings.GrabSetting("warning"));
            Thread.Sleep(700);
            System.Console.WriteLine("------ Begin help ------");
            System.Console.WriteLine("If running in terminal mode, type 'quit' to leave and resume an aeon.");
            Thread.Sleep(700);
            System.Console.WriteLine("While in terminal mode, type 'exit' to quit the application entirely.");
            Thread.Sleep(700);
            System.Console.WriteLine("If you want to dissolve your aeon while in non-terminal mode, type 'aeon quit'.");
            System.Console.WriteLine("------ End help------");
            Thread.Sleep(700);
            System.Console.WriteLine("Continuing to construct the personality...");
            // Check that the aeon launch is valid.
            UserInput = "";
            _thisAeon.Name = _thisAeon.GlobalSettings.GrabSetting("name");
            _thisAeon.EmotionUsed = Convert.ToBoolean(_thisAeon.GlobalSettings.GrabSetting("emotionused"));
            DetectUserEmotion = Convert.ToBoolean(_thisAeon.GlobalSettings.GrabSetting("emotiondetection"));
            StartUpTheme = Convert.ToBoolean(_thisAeon.GlobalSettings.GrabSetting("startuptheme"));
            StartUpThemeFile = _thisAeon.GlobalSettings.GrabSetting("startupthemefile");
            EmotiveEquation = _thisAeon.GlobalSettings.GrabSetting("emotiveequation");
            // Initialize the alone feature.
            _thisAeon.AeonAloneTimer = new System.Timers.Timer();
            _thisAeon.AeonAloneTimer.Elapsed += AloneEvent;
            _thisAeon.AeonAloneTimer.Interval = Convert.ToDouble(_thisAeon.GlobalSettings.GrabSetting("alonetimecheck"));
            _thisAeon.AeonAloneTimer.Enabled = false;
            _aeonAloneThread = new Thread(AeonAloneText);
#if Linux
                PresenceSpeaker = new ESpeakLibrary(NeSpeak.espeak_AUDIO_OUTPUT.AUDIO_OUTPUT_PLAYBACK);
#endif
            SharedFunctions.ThisAeon = _thisAeon;
            // Determine what external hardware is to be used.
            SapiWindowsUsed = Convert.ToBoolean(_thisAeon.GlobalSettings.GrabSetting("sapiwindows"));
            PocketSphinxUsed = Convert.ToBoolean(_thisAeon.GlobalSettings.GrabSetting("pocketsphinx"));
            SpeechSynthesizerUsed = Convert.ToBoolean(_thisAeon.GlobalSettings.GrabSetting("speechsynthesizer"));
            SpeechToTonal = Convert.ToBoolean(_thisAeon.GlobalSettings.GrabSetting("tonalspeech"));
            TonalSpeechLimit = Convert.ToBoolean(_thisAeon.GlobalSettings.GrabSetting("tonalspeechlimit"));
            TonalSpeechLimitValue = Convert.ToInt32(_thisAeon.GlobalSettings.GrabSetting("tonalspeechlimitvalue"));
            EmotionalDisplayUsed = Convert.ToBoolean(_thisAeon.GlobalSettings.GrabSetting("emotionaldisplayused"));
            MoodicDisplayUsed = Convert.ToBoolean(_thisAeon.GlobalSettings.GrabSetting("moodicdisplayused"));
            MoodicDurationUsed = Convert.ToBoolean(_thisAeon.GlobalSettings.GrabSetting("moodicdurationused"));
            MoodicDuration = Convert.ToDouble(_thisAeon.GlobalSettings.GrabSetting("moodicduration"));
            UsePythonBottle = Convert.ToBoolean(_thisAeon.GlobalSettings.GrabSetting("usepythonbottle"));
            // Initialize the moodic feature.
            if (MoodicDurationUsed)
            {
                _thisAeon.MoodicTimer = new System.Timers.Timer();
                _thisAeon.MoodicTimer.Elapsed += MoodicEvent;
                _thisAeon.MoodicTimer.Interval = Convert.ToDouble(_thisAeon.GlobalSettings.GrabSetting("moodicduration"));
                _thisAeon.MoodicTimer.Enabled = false;
                _moodicThread = new Thread(SetQuiet);
            }
            // Load any external interpreters for hardware control.
            PythonLocation = _thisAeon.GlobalSettings.GrabSetting("pythonlocation");
            // Physical toy where code will be running hardware controllers.
            TestHardware = Convert.ToBoolean(_thisAeon.GlobalSettings.GrabSetting("testhardware"));
            NumberOfMotors = Convert.ToInt32(_thisAeon.GlobalSettings.GrabSetting("numberofmotors"));
            EyesOpenGpio = Convert.ToInt32(_thisAeon.GlobalSettings.GrabSetting("eyesopengpio"));
            EyesCloseGpio = Convert.ToInt32(_thisAeon.GlobalSettings.GrabSetting("eyesclosegpio"));
            NoseOpenGpio = Convert.ToInt32(_thisAeon.GlobalSettings.GrabSetting("noseopengpio"));
            NoseCloseGpio = Convert.ToInt32(_thisAeon.GlobalSettings.GrabSetting("noseclosegpio"));
            MouthOpenGpio = Convert.ToInt32(_thisAeon.GlobalSettings.GrabSetting("mouthopengpio"));
            MouthCloseGpio = Convert.ToInt32(_thisAeon.GlobalSettings.GrabSetting("mouthclosegpio"));
            TerminalMode = Convert.ToBoolean(_thisAeon.GlobalSettings.GrabSetting("terminalmode"));
            // Set the path for the emotive audio files.
            EmotiveA = _thisAeon.GlobalSettings.GrabSetting("emotiveafile");
            EmotiveB = _thisAeon.GlobalSettings.GrabSetting("emotivebfile");
            EmotiveC = _thisAeon.GlobalSettings.GrabSetting("emotivecfile");
            TonalRootPath = Configuration.PathToTonalRoot;
            TonalA = _thisAeon.GlobalSettings.GrabSetting("tonalafile");
            TonalB = _thisAeon.GlobalSettings.GrabSetting("tonalbfile");
            TonalC = _thisAeon.GlobalSettings.GrabSetting("tonalcfile");
            TonalD = _thisAeon.GlobalSettings.GrabSetting("tonaldfile");
            TonalE = _thisAeon.GlobalSettings.GrabSetting("tonalefile");
            TonalF = _thisAeon.GlobalSettings.GrabSetting("tonalffile");
            TonalFs = _thisAeon.GlobalSettings.GrabSetting("tonalfsfile");
            TonalG = _thisAeon.GlobalSettings.GrabSetting("tonalgfile");
            TonalAp = _thisAeon.GlobalSettings.GrabSetting("tonalapfile");
            TonalDelay = Convert.ToInt32(_thisAeon.GlobalSettings.GrabSetting("tonaldelay"));
            Repetition = Convert.ToInt32(_thisAeon.GlobalSettings.GrabSetting("repetition"));
            // Initialize the mood feature and set the display to the current mood.
            if (_thisAeon.EmotionUsed)
            {
                // ToDo: Once a mood state is realized, how does it influence the conversation?
                AeonMood = new Mood(StaticRandom.Next(0, 20), _thisAeon, _thisUser, EmotiveEquation);
                AeonCurrentMood = AeonMood.GetCurrentMood();
                SwitchMood = 0;
                // What happens next when the animal is to be emotional?
                if (AeonCurrentMood == "Energized" | AeonCurrentMood == "Happy" | AeonCurrentMood == "Confident")
                {
                    EmotiveResponse = new SoundPlayer(Configuration.ActiveRuntime + @"\emotive\tones\" + EmotiveA);
                    //for (int i = 0; i < Repetition; i++) // TODO: Needs queing but thinking creating longer *.wav a better idea.
                    EmotiveResponse.Play();
                    Thread.Sleep(500);
                    // First draft of playing a tonal file sequence, based on the mood.
                    PlayTonalSequence("fun.txt");
                }
                if (AeonCurrentMood == "Helped" | AeonCurrentMood == "Insecure")
                {
                    EmotiveResponse = new SoundPlayer(Configuration.ActiveRuntime + @"\emotive\tones\" + EmotiveB);
                    EmotiveResponse.Play();
                    Thread.Sleep(250);
                    PlayTonalSequence("okay.txt");
                }
                if (AeonCurrentMood == "Hurt" | AeonCurrentMood == "Sad" | AeonCurrentMood == "Tired")
                {
                    EmotiveResponse = new SoundPlayer(Configuration.ActiveRuntime + @"\emotive\tones\" + EmotiveC);
                    EmotiveResponse.Play();
                    Thread.Sleep(250);
                    PlayTonalSequence("contemplate.txt");
                }
            }
            // Initialize the arduino hardware.
            if (EmotionalDisplayUsed)
            {
                // Only upload if a new emotion is present and the display is in use.
                if (!SketchUploaded)
                {
                    SetEmotion(Mood.CurrentMood);
                }
                if (SketchUploaded)
                {
                    if (LastSketch != Mood.CurrentMood)
                    {
                        SetEmotion(Mood.CurrentMood);
                    }
                }
                SwitchMood = 0;
            }
            if (MoodicDisplayUsed)
            {
                // Begin with the baseline moodic display.
                if (!SketchUploaded)
                {
                    SetMoodic(Mood.CurrentMood);
                }
                if (SketchUploaded)
                {
                    if (LastSketch != Mood.CurrentMood)
                    {
                        SetMoodic(Mood.CurrentMood);
                    }
                }
                SwitchMood = 0;
            }
            // Utilize the correct settings based on the aeon personality.
            if (_thisAeon.Name == "Rhodo" && SettingsLoaded)
                AeonLoaded = _thisAeon.LoadPersonality(Configuration);
            if (_thisAeon.Name == "Henry" && SettingsLoaded)
                AeonLoaded = _thisAeon.LoadPersonality(Configuration);
            if (_thisAeon.Name == "Blank" && SettingsLoaded)
                AeonLoaded = _thisAeon.LoadBlank(Configuration, null);
            if (_thisAeon.Name == "Samantha" && SettingsLoaded)
                AeonLoaded = _thisAeon.LoadPersonality(Configuration);
            if (_thisAeon.Name == "Fred" && SettingsLoaded)
                AeonLoaded = _thisAeon.LoadPersonality(Configuration);
            if (_thisAeon.Name == "Aeon" && SettingsLoaded)
                AeonLoaded = _thisAeon.LoadPersonality(Configuration);
            if (_thisAeon.Name == "Mitsuku" && SettingsLoaded)
                AeonLoaded = _thisAeon.LoadPersonality(Configuration);
            // Attach logging,  xms functionality, and spontaneous file generation.
            Logging.LogModelFile = _thisAeon.GlobalSettings.GrabSetting("logmodelfile");
            Logging.TranscriptModelFile = _thisAeon.GlobalSettings.GrabSetting("transcriptmodelfile");
            // Determine whether to load the Linux or Windows speech synthesizer.
            if (PocketSphinxUsed && AeonLoaded)
            {
                System.Console.WriteLine("Intializing the pocketsphinx interface with RabbitMQ...");
                InitializeMessageQueue();
            }
#if Windows
            if (SapiWindowsUsed && AeonLoaded)
                InitializeSapiWindows();
#endif
            // Set the aeon type by personality.
            switch (_thisAeon.Name)
            {
                case "Aeon":
                    AeonType = "Assistive";
                    break;
                case "Fred":
                    AeonType = "Toy";
                    break;
                case "Henry":
                    AeonType = "Toy";
                    break;
                case "Rhodo":
                    AeonType = "Default";
                    break;
                case "Samantha":
                    AeonType = "Friendly";
                    break;
                case "Blank":
                    AeonType = "Huggable";
                    break;
            }
            System.Console.WriteLine("The aeon type is " + AeonType.ToLower() + ".");
            System.Console.WriteLine("Personality construction completed.");
            System.Console.WriteLine("A presence named '" + _thisAeon.Name + "' has been initialized.");
            System.Console.WriteLine("It has " + _thisAeon.Size + " categories available in its mind.");
            // Set the runtime state.
            if (_thisAeon.Name.ToLower() == "aeon")
                System.Console.WriteLine("You have selected to load the aeon-assist variety.");
            // Set final parameters and play the welcome message.
            if (StartUpTheme)
            {
                try
                {
                    AnimalActive = new SoundPlayer(Configuration.ActiveRuntime + @"\sounds\" + StartUpThemeFile);
                    AnimalActive.Play();
                }
                catch (Exception ex)
                {
                    Logging.WriteLog(ex.Message, Logging.LogType.Error, Logging.LogCaller.AeonRuntime);
                }
            }
            System.Console.WriteLine("Your aeon is ready for an interaction with you.");
            System.Console.WriteLine("The aeon's mood is " + AeonCurrentMood + ".");
            System.Console.WriteLine("The mood polynomial is: " + AeonMood.ReturnMoodPolynomialProperties());
            System.Console.WriteLine("The polynomial properties are: Its roots " + AeonMood.PolynomialRoots[0] + ", " + AeonMood.PolynomialRoots[1] + ", " + AeonMood.PolynomialRoots[2] + " and derivative " + AeonMood.PolynomialDerivative + ".");
            System.Console.WriteLine("Your transcript follows. Enjoy!");
            System.Console.WriteLine("**********************");
            if (TerminalMode)
            {
                System.Console.WriteLine("You have selected terminal mode.");
                // Trigger the terminal for typing torch commands.
                while (true && UserInput != "quit")
                {
                    UserInput = "";
                    UserInput = System.Console.ReadLine();
                    if (UserInput != null)
                        ProcessTerminal();
                    if (UserInput == "quit")
                    {
                        System.Console.WriteLine("Leaving terminal mode and starting a conversational aeon.");
                        Thread.Sleep(2000);
                        break;
                    }
                    if (UserInput == "exit")
                    {
                        System.Console.WriteLine("Leaving terminal mode and exiting the application.");
                        Thread.Sleep(2000);
                        Environment.Exit(0);
                        break;
                    }
                }
            }
            if (_thisAeon.EmotionUsed)
                System.Console.WriteLine("The aeon's current mood is: " + Mood.CurrentMood + ".");

            if (TestHardware && AeonLoaded)
            {
                BlinkRoutine(7);
            }
            // Initialize the queue/pocketsphinx code.
            if (PocketSphinxUsed && AeonLoaded)
            {
                try
                {
                    ExecuteSphinxProcessToQueue();
                }
                catch (Exception ex)
                {
                    Logging.WriteLog(ex.Message, Logging.LogType.Error, Logging.LogCaller.AeonRuntime);
                    System.Console.WriteLine("Pocketsphinx interface error. " + ex.Message);
                }
            }
            while (true)
            {
                UserInput = System.Console.ReadLine();
                if (UserInput != null)
                    ProcessInput();
                if (UserInput == "aeon quit")
                    break;
            }

            System.Console.WriteLine(_thisAeon.GlobalSettings.GrabSetting("product") + " is closing in ten seconds.");
            Thread.Sleep(10000);
            Logging.WriteLog("Companion shut down at " + DateTime.Now, Logging.LogType.Information, Logging.LogCaller.AeonRuntime);
            Environment.Exit(0);
        }
        public static bool ProcessTerminal()
        {
            //System.Console.WriteLine("Processing terminal command: " + UserInput);
            // For now, pass to the input-processing engine.
            ProcessInput();
            return true;
        }
        /// <summary>
        /// The main input method to pass an enquiry to the system, yielding a reaction/response behavior to the user.
        /// </summary>
        /// <remarks>Once a mood state is realized, how does it influence the conversation?</remarks>
        public static bool ProcessInput(string returnFromProcess = "")
        {
            if (DetectUserEmotion)
            {
                //CorrectExecution = Gpio.RunPythonScript("detectemotion.py", "", Configuration);
                // Will need to return the emotion detected by the script.
            }
            Syntax.CommandReceived = false;
            if (_thisAeon.IsAcceptingUserInput)
            {
                _aeonChatStartedOn = DateTime.Now;
                Thread.Sleep(250);
                var rawInput = UserInput;
                if (rawInput.Contains("\n"))
                {
                    rawInput = rawInput.TrimEnd('\n');
                }
                System.Console.WriteLine(_thisUser.UserName + ": " + rawInput);
                _thisRequest = new Request(rawInput, _thisUser, _thisAeon);
                _thisResult = _thisAeon.Chat(_thisRequest);
                Thread.Sleep(200);
                System.Console.WriteLine(_thisAeon.Name + ": " + _thisResult.Output);
                Logging.RecordTranscript(_thisUser.UserName + ": " + rawInput);
                Logging.RecordTranscript(_thisAeon.Name + ": " + _thisResult.Output);
                // Record performance vectors for the result.
                _aeonChatDuration = DateTime.Now - _aeonChatStartedOn;
                Logging.WriteLog("Result search was conducted in: " + _aeonChatDuration.Seconds + @"." + _aeonChatDuration.Milliseconds + " seconds", Logging.LogType.Information, Logging.LogCaller.AeonRuntime);
                // Learning: Send the result to the learning algorithm.
                //AeonFive = new MeaningFive(_thisResult);
                if (!UsePythonBottle && SpeechSynthesizerUsed)
                    SpeakText(_thisResult.Output);
                if (SpeechToTonal)
                    TransposeTonalSpeech(_thisResult.Output);
                if (UsePythonBottle)
                {
                    var message = new NameValueCollection
                    {
                        ["speech"] = _thisResult.Output
                    };

                    try
                    {
                        HenryClient.UploadValues(_thisAeon.GlobalSettings.GrabSetting("bottleipaddress"), "POST", message);
                    }
                    catch (Exception ex)
                    {
                        Logging.WriteLog(ex.Message, Logging.LogType.Error, Logging.LogCaller.AeonRuntime);
                        System.Console.WriteLine("No response from the emotional toy.");
                    }
                }
                if (TestHardware && NumberOfMotors == 2)
                {
                    CorrectExecution = Gpio.RunPythonScript("emu1.py", "1", Configuration);
                }
                if (TestHardware && NumberOfMotors == 3)
                {
                    CorrectExecution = Gpio.RunPythonScript("emu2.py", "1", Configuration);
                }
                _thisAeon.AeonAloneTimer.Enabled = true;
                _thisAeon.AeonAloneStartedOn = DateTime.Now;
                AeonIsAlone = false;
                AeonOutputDebug = GenerateAeonOutputDebug();
                AeonOutputDialogue = _thisResult.Output;
                if (UserInput == "exit")
                {
                    System.Console.WriteLine("Detected 'exit'...quitting the application.");
                    Thread.Sleep(2000);
                    Environment.Exit(0);
                }
            }
            else
            {
                UserInput = string.Empty;
                System.Console.WriteLine("Aeon is not accepting user input." + Environment.NewLine);
            }
            return true;
        }

        #region Terminal process
        public static bool ExecuteTerminalProcess(string terminalCommand)
        {
            TerminalProcess = new Process();
            TerminalProcess.StartInfo.FileName = "th";
            TerminalProcess.StartInfo.Arguments = " " + terminalCommand;
            TerminalProcess.StartInfo.UseShellExecute = false;
            TerminalProcess.StartInfo.RedirectStandardOutput = true;

            try
            {
                TerminalProcess.Start();
                System.Console.WriteLine(TerminalProcess.StandardOutput.ReadToEnd());
                TerminalProcess.WaitForExit();
                return true;
            }
            catch (Exception ex)
            {
                Logging.WriteLog(ex.Message, Logging.LogType.Error, Logging.LogCaller.AeonRuntime);
                System.Console.WriteLine("An error ocurred in the terminal process.");
                return false;
            }

        }
        #endregion

        #region Pocketsphinx process
        /// <summary>
        /// Execute the pocketsphinx process. Connect to the engine.
        /// </summary>
        /// <returns>Standard output and standard error from the process.</returns>
        public static void ExecuteSphinxProcessToQueue()
        {
            SphinxProcess = new Process();
            SphinxProcess.StartInfo.FileName = "/bin/bash";
            SphinxProcess.StartInfo.Arguments = "-c \" " + SphinxStartup + " \"";
            SphinxProcess.StartInfo.UseShellExecute = false;
            SphinxProcess.StartInfo.RedirectStandardOutput = true;
            SphinxProcess.Start();

            while (!SphinxProcess.StandardOutput.EndOfStream)
            {
                // Send the output to the MQ.
                var body = Encoding.UTF8.GetBytes(SphinxProcess.StandardOutput.ReadLine());
                Channel.BasicPublish(exchange: "",
                                     routingKey: "speech",
                                     basicProperties: null,
                                     body: body);
            }
        }
        #endregion

        #region Learning by my own design

        // When learning, create a new *.aeon file.
        public void PositData(Characteristic local, string pattern, string template)
        {
            // Save current state data to the xms system (overwrites in filesystem but only <template/> value in aeon's memory).
            //pattern = "TRADING SESSION";// Area of the brain.
            //template = "";// Content which includes, srai, star, emotion, etc. as: <srai>I am thinking about <star/></srai>
            // Also, more interestingly as: They are really smart animals.<think><set name="they"><set name="like"><set name="topic">CATS</set></set></set></think>
            // And: I like stories about robots the best.  <think><set name="it"><set name="like"><set name="topic">science fiction</set></set></set></think>
            const string filename = "holiday.aeon"; // Currently human-readable naming, but could be another since the program will read all the files in a given folder, which, in the current iteration is coded as "fragments" in the config file.
            var path = Configuration.PathToNucode + @"\" + filename;
            FileTemplate.PatternText = pattern.ToUpper();
            FileTemplate.TemplateText = template;
            FileTemplate.CreateFileTemplate();
            FileTemplate.WriteNuFile(filename, "learned-factiod");
            var fi = new FileInfo(Configuration.PathToNucode + @"\" + filename);
            if (fi.Exists)
            {
                var doc = new XmlDocument();
                try
                {
                    doc.Load(path);
                    _thisAeon.LoadAeonFromXml(doc, path);
                    // Will add a category thereby increasing aeon's size by the number of entries. Perhaps deleting old values is possible? Or how necessary?
                    Logging.WriteLog("Categories added.  Size is " + _thisAeon.Size + @" categories.", Logging.LogType.Information, Logging.LogCaller.AeonRuntime);
                }
                catch
                {
                    Logging.WriteLog("Aeon failed to learn something new from the following: " + path, Logging.LogType.Error, Logging.LogCaller.Learn);
                }
            }
            // Do not delete since will use them when loading a fresh aeon with increased capacity.
        }

        #endregion

        #region Social features
        /// <summary>
        /// The method to speak the alone message.
        /// </summary>
        /// <param name="alone">if set to <c>true</c> [alone].</param>
        protected static void AloneMessage(bool alone)
        {
            if (alone)
            {
                if (!_aeonAloneThread.IsAlive)
                {
                    _aeonAloneThread = new Thread(AeonAloneText) { IsBackground = true };
                    _aeonAloneThread.Start();
                }
            }
        }
        /// <summary>
        /// Check if Aeon is in the state of being alone.
        /// </summary>
        public static void CheckIfAeonIsAlone()
        {
            if (_thisAeon.IsAlone())
            {
                AloneMessage(true);
                //SetMoodic("alone");
                AeonIsAlone = true;
                _thisAeon.AeonAloneStartedOn = DateTime.Now;
            }
        }
        private static void AloneEvent(object source, ElapsedEventArgs e)
        {
            CheckIfAeonIsAlone();
        }
        private static void AeonAloneText()
        {
            AloneMessageVariety = StaticRandom.Next(1, 1750);
            System.Console.WriteLine("Your relationship with your aeon is: " + Mood.RelationshipOutcome(_thisResult.Output) + ".");
            if (AloneMessageVariety.IsBetween(1, 250))
                AloneMessageOutput = 0;
            if (AloneMessageVariety.IsBetween(251, 500))
                AloneMessageOutput = 1;
            if (AloneMessageVariety.IsBetween(501, 750))
                AloneMessageOutput = 2;
            if (AloneMessageVariety.IsBetween(751, 1000))
                AloneMessageOutput = 3;
            if (AloneMessageVariety.IsBetween(1001, 1250))
                AloneMessageOutput = 4;
            if (AloneMessageVariety.IsBetween(1001, 1250))
                AloneMessageOutput = 5;
            if (AloneMessageVariety.IsBetween(1251, 1500))
                AloneMessageOutput = 6;
            if (AloneMessageVariety.IsBetween(1501, 1750))
                AloneMessageOutput = 7;

            PreviousAloneMessageOutput = AloneMessageOutput;
            SwitchMood = 1;
            SpeakText(_thisAeon.GlobalSettings.GrabSetting("alonesalutaion") + _thisUser.UserName + ", " + AloneTextCurrent);
            AloneTextCurrent = _thisAeon.GlobalSettings.GrabSetting("alonemessage" + AloneMessageOutput);
        }
        #endregion

        #region Speech recognizer as per the OS

        #region Linux (including messaging-MQ for pocketsphinx)
        /// <summary>
        /// Here is where the output from pocketsphinx is being publically recieved and sent to the aeon.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="BasicDeliverEventArgs"/> instance containing the event data.</param>
        private static void PocketSphinxSpeechRecognized(object sender, BasicDeliverEventArgs e)
        {
            UserInput = Encoding.UTF8.GetString(e.Body);
            ProcessInput();
            Logging.WriteLog(LastOutput, Logging.LogType.Information, Logging.LogCaller.AeonRuntime);
        }
        /// <summary>
        /// Initializes the messaging queue.
        /// </summary>
        /// <returns></returns>
        public static bool InitializeMessageQueue()
        {
            try
            {
                System.Console.WriteLine("Intializing the message queue...");
                Factory = new ConnectionFactory()
                {
                    HostName = _thisAeon.GlobalSettings.GrabSetting("messagingqueuehost")
                };
                var connection = Factory.CreateConnection();
                Channel = connection.CreateModel();
                Channel.QueueDeclare(queue: "speech",
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);
                Consumer = new EventingBasicConsumer(Channel);
                Consumer.Received += PocketSphinxSpeechRecognized;
                Channel.BasicConsume(queue: "speech",
                         autoAck: true,
                         consumer: Consumer);
                System.Console.WriteLine("Message queue initialized.");
                return true;
            }
            catch (Exception ex)
            {
                Logging.WriteLog(ex.Message, Logging.LogType.Error, Logging.LogCaller.AeonRuntime);
                return false;
            }

        }
        #endregion

        #region Windows
        #if Windows
        /// <summary>
        /// Here is where the input to the robot is being received, in the laptop and not the robot.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SpeechRecognizedEventArgs"/> instance containing the event data.</param>
        private static void SapiWindowsSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            // Here is where the logic of the hardware connects to the application. Removed user input textbox.
            UserInput = e.Result.Text;
            ProcessInput();
            Logging.WriteLog(LastOutput, Logging.LogType.Information, Logging.LogCaller.AeonRuntime);
        }
        /// <summary>
        /// Initializes the speech recognizer.
        /// </summary>
        /// <returns></returns>
        public static bool InitializeSapiWindows()
        {
            try
            {
                // Read in the list of phrases that the speech engine will recognise when it detects it being spoken.
                GrammarBuilder.Append(
                    new Choices(File.ReadAllLines(Path.Combine(Configuration.ActiveRuntime, Path.Combine("grammar", "valid-grammar.txt")))));
                Logging.WriteLog("Windows SAPI detected. Load the grammar file.", Logging.LogType.Information, Logging.LogCaller.AeonRuntime);
                System.Console.WriteLine("Windows SAPI detected. Load the grammar file found in the 'bin' folder.");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                return false;
            }
            var gr = new Grammar(GrammarBuilder);
            try
            {
                Recognizer.UnloadAllGrammars();
                Recognizer.RecognizeAsyncCancel();
                Recognizer.RequestRecognizerUpdate();
                Recognizer.LoadGrammar(gr);
                Recognizer.SpeechRecognized += SapiWindowsSpeechRecognized;
                Recognizer.SetInputToDefaultAudioDevice();
                Recognizer.RecognizeAsync(RecognizeMode.Multiple);
                Logging.WriteLog("Windows SAPI: Recognizer initialized.", Logging.LogType.Information, Logging.LogCaller.AeonRuntime);
                System.Console.WriteLine("Windows SAPI: Recognizer initialized.");
                Thread.Sleep(700);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("An error has occurred in the recognizer. " + ex.Message);
                Logging.WriteLog(ex.Message, Logging.LogType.Error, Logging.LogCaller.RobotDialogue);
            }
            System.Console.WriteLine("Windows SAPI: Loaded the correct grammar file.");
            Thread.Sleep(700);
            return true;
        }
        #endif
        #endregion

        #endregion

        #region Speech synthesizer as per the OS
        /// <summary>
        /// Speak the text using a native voice synthesizer.
        /// </summary>
        /// <param name="input">The input.</param>
        public static void SpeakText(string input)
        {
            if (UsePythonBottle)
            {
                var message = new NameValueCollection
                {
                    ["speech"] = input
                };
                try
                {
                    HenryClient.UploadValues(_thisAeon.GlobalSettings.GrabSetting("bottleipaddress"), "POST", message);
                }
                catch (Exception ex)
                {
                    Logging.WriteLog(ex.Message, Logging.LogType.Error, Logging.LogCaller.AeonRuntime);
                    System.Console.WriteLine("No response from the animal.");
                }
            }
            if (SpeechSynthesizerUsed && PocketSphinxUsed)
            {
                try
                {
                    PresenceSpeaker.Synthesize(input); // Linux
                }
                catch (Exception ex)
                {
                    Logging.WriteLog(ex.Message, Logging.LogType.Error, Logging.LogCaller.AeonRuntime);
                }
            }
            if (SpeechSynthesizerUsed)
            {
                try
                {
#if Windows
                    PromptBuilder.ClearContent();
                    PromptBuilder.AppendText(input);
                    SpeechSynth.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);
                    SpeechSynth.Speak(PromptBuilder);
#endif
                }
                catch (Exception ex)
                {
                    Logging.WriteLog(ex.Message, Logging.LogType.Error, Logging.LogCaller.AeonRuntime);
                }
            }
        }
        #endregion

        #region Tonal transposition of spoken output
        /// <summary>
        /// Transpose langauge to tonals. v.1.1 has eleven tones for twenty-six alphabet characters.
        /// </summary>
        /// <param name="input">The spoken input to be toned.</param>
        /// <remarks>Contains a code segment to keeps the aeon from toning too long. v1.1.0 feature.</remarks>
        public static void TransposeTonalSpeech(string input)
        {
            var words = input.Split(' ');
            if (TonalSpeechLimit)
            {
                if (words.Length > TonalSpeechLimitValue)
                {
                    try
                    {
                        Array.Resize(ref words, TonalSpeechLimitValue);
                    }
                    catch
                    {
                        System.Console.WriteLine("Something went wrong with the array resizing. If this persists, set the speech limit to false.");
                    }
                }
            }
            foreach (var word in words)
            {
                foreach (var letter in word)
                {
                    switch (letter.ToString().ToUpper())
                    {
                        case "A":
                            TonalResponse = new SoundPlayer(ReturnTonalPath() + TonalA);
                            TonalResponse.Play();
                            break;
                        case "B":
                            TonalResponse = new SoundPlayer(ReturnTonalPath() + TonalB);
                            TonalResponse.Play();
                            break;
                        case "C":
                            TonalResponse = new SoundPlayer(ReturnTonalPath() + TonalC);
                            TonalResponse.Play();
                            break;
                        case "D":
                            TonalResponse = new SoundPlayer(ReturnTonalPath() + TonalD);
                            TonalResponse.Play();
                            break;
                        case "E":
                            TonalResponse = new SoundPlayer(ReturnTonalPath() + TonalE);
                            TonalResponse.Play();
                            break;
                        case "F":
                            TonalResponse = new SoundPlayer(ReturnTonalPath() + TonalF);
                            TonalResponse.Play();
                            break;
                        case "G":
                            TonalResponse = new SoundPlayer(ReturnTonalPath() + TonalFs);
                            TonalResponse.Play();
                            break;
                        case "H":
                            TonalResponse = new SoundPlayer(ReturnTonalPath() + TonalG);
                            TonalResponse.Play();
                            break;
                        case "I":
                            TonalResponse = new SoundPlayer(ReturnTonalPath() + TonalAp);
                            TonalResponse.Play();
                            break;
                        case "J":
                            TonalResponse = new SoundPlayer(ReturnTonalPath() + TonalA);
                            TonalResponse.Play();
                            break;
                        case "K":
                            TonalResponse = new SoundPlayer(ReturnTonalPath() + TonalB);
                            TonalResponse.Play();
                            break;
                        case "L":
                            TonalResponse = new SoundPlayer(ReturnTonalPath() + TonalC);
                            TonalResponse.Play();
                            break;
                        case "M":
                            TonalResponse = new SoundPlayer(ReturnTonalPath() + TonalD);
                            TonalResponse.Play();
                            break;
                        case "N":
                            TonalResponse = new SoundPlayer(ReturnTonalPath() + TonalE);
                            TonalResponse.Play();
                            break;
                        case "O":
                            TonalResponse = new SoundPlayer(ReturnTonalPath() + TonalF);
                            TonalResponse.Play();
                            break;
                        case "P":
                            TonalResponse = new SoundPlayer(ReturnTonalPath() + TonalFs);
                            TonalResponse.Play();
                            break;
                        case "Q":
                            TonalResponse = new SoundPlayer(ReturnTonalPath() + TonalG);
                            TonalResponse.Play();
                            break;
                        case "R":
                            TonalResponse = new SoundPlayer(ReturnTonalPath() + TonalAp);
                            TonalResponse.Play();
                            break;
                        case "S":
                            TonalResponse = new SoundPlayer(ReturnTonalPath() + TonalA);
                            TonalResponse.Play();
                            break;
                        case "T":
                            TonalResponse = new SoundPlayer(ReturnTonalPath() + TonalB);
                            TonalResponse.Play();
                            break;
                        case "U":
                            TonalResponse = new SoundPlayer(ReturnTonalPath() + TonalC);
                            TonalResponse.Play();
                            break;
                        case "V":
                            TonalResponse = new SoundPlayer(ReturnTonalPath() + TonalD);
                            TonalResponse.Play();
                            break;
                        case "W":
                            TonalResponse = new SoundPlayer(ReturnTonalPath() + TonalE);
                            TonalResponse.Play();
                            break;
                        case "X":
                            TonalResponse = new SoundPlayer(ReturnTonalPath() + TonalF);
                            TonalResponse.Play();
                            break;
                        case "Y":
                            TonalResponse = new SoundPlayer(ReturnTonalPath() + TonalFs);
                            TonalResponse.Play();
                            break;
                        case "Z":
                            TonalResponse = new SoundPlayer(ReturnTonalPath() + TonalG);
                            TonalResponse.Play();
                            break;
                    }
                    Thread.Sleep(TonalDelay);
                }
            }
        }
        #endregion

        #region GPIO bindings
        /// <summary>
        /// Bind the GPIO ports directly to this application, sans scripts. Experimental.
        /// </summary>
        /// <returns></returns>
        public static bool BindGpio()
        {
            var gpio = new AeonGpio();
            for (var i = 0; i < 5; i++) //flash pin 17, 5 times on & off (1 second each)
            {
                // Open the eyes.
                gpio.OutputPin(AeonGpio.PinSets.P4, true);
                Thread.Sleep(1000);
                // Close they eyes.
                gpio.OutputPin(AeonGpio.PinSets.P6, false);
                Thread.Sleep(1000);
            }
            //Console.WriteLine( "Value of pin 18 is " + gpio.InputPin(FileGPIO.enumPIN.gpio18) ); //UNTESTED!
            gpio.CleanUpAllPins();
            return true;
        }
        #endregion

        #region Emotive processing
        /// <summary>
        /// Runs the blink routine.
        /// </summary>
        /// <param name="cycles">The number of cycles to blink.</param>
        public static void BlinkRoutine(int cycles)
        {
            CorrectExecution = Gpio.RunPythonScript("blink.py", cycles.ToString(), Configuration);
        }
        public static bool PlayTonalSequence(string filename)
        {
            try
            {
                var files = File.ReadAllText(TonalRootPath + "\\files\\" + filename, Encoding.UTF8);
                foreach(var value in files)
                {
                    switch (value.ToString())
                    {
                        case "A":
                            TonalResponse = new SoundPlayer(ReturnTonalPath() + TonalA);
                            TonalResponse.Play();
                            break;
                        case "B":
                            TonalResponse = new SoundPlayer(ReturnTonalPath() + TonalB);
                            TonalResponse.Play();
                            break;
                        case "C":
                            TonalResponse = new SoundPlayer(ReturnTonalPath() + TonalC);
                            TonalResponse.Play();
                            break;
                        case "D":
                            TonalResponse = new SoundPlayer(ReturnTonalPath() + TonalD);
                            TonalResponse.Play();
                            break;
                        case "E":
                            TonalResponse = new SoundPlayer(ReturnTonalPath() + TonalE);
                            TonalResponse.Play();
                            break;
                        case "F":
                            TonalResponse = new SoundPlayer(ReturnTonalPath() + TonalF);
                            TonalResponse.Play();
                            break;
                        case "Fs":
                            TonalResponse = new SoundPlayer(ReturnTonalPath() + TonalFs);
                            TonalResponse.Play();
                            break;
                        case "G":
                            TonalResponse = new SoundPlayer(ReturnTonalPath() + TonalG);
                            TonalResponse.Play();
                            break;
                        case "Ap":
                            TonalResponse = new SoundPlayer(ReturnTonalPath() + TonalAp);
                            TonalResponse.PlaySync();
                            break;
                            
                    }
                    Thread.Sleep(TonalDelay + 30);
                }
            }
            catch (Exception ex)
            {
                Logging.WriteLog(ex.Message, Logging.LogType.Error, Logging.LogCaller.AeonRuntime);
                return false;
            }
            return true;
        }
        public static void SetEmotion(string emotion)
        {
            string sketchEmotion;
            switch (emotion)
            {
                case "Happy":
                    sketchEmotion = "happy";
                    LastSketch = emotion;
                    break;
                case "Confident":
                    sketchEmotion = "content";
                    LastSketch = emotion;
                    break;
                case "Energized":
                    sketchEmotion = "laugh";
                    LastSketch = emotion;
                    break;
                case "Helped":
                    sketchEmotion = "content";
                    LastSketch = emotion;
                    break;
                case "Insecure":
                    sketchEmotion = "sad";
                    LastSketch = emotion;
                    break;
                case "Sad":
                    sketchEmotion = "sad";
                    LastSketch = emotion;
                    break;
                case "Hurt":
                    sketchEmotion = "upset";
                    LastSketch = emotion;
                    break;
                default:
                    sketchEmotion = "happy";
                    break;
            }
            var uploader = new ArduinoSketchUploader(new ArduinoSketchUploaderOptions
            {
                FileName = Environment.CurrentDirectory + @"\sketches\" + sketchEmotion + @".ino.standard.hex",
                PortName = _thisAeon.GlobalSettings.GrabSetting("arduinocomport"),
                ArduinoModel = ArduinoModel.UnoR3
            });
            if (SketchThread == null)
            {
                try
                {
                    // Send the upload task to a separate thread.
                    SketchThread = new Thread(uploader.UploadSketch);
                    SketchThread.Start();
                    SketchUploaded = true;
                }
                catch (Exception ex)
                {
                    Logging.WriteLog(ex.Message, Logging.LogType.Error, Logging.LogCaller.AeonRuntime);
                }
                finally
                {
                    if (SketchThread != null)
                        SketchThread.Join();
                }
            }
            if (SketchThread != null)
            {
                if (!SketchThread.IsAlive)
                {
                    try
                    {
                        // Send the upload task to a separate thread.
                        SketchThread = new Thread(uploader.UploadSketch);
                        SketchThread.Start();
                        SketchUploaded = true;
                    }
                    catch (Exception ex)
                    {
                        Logging.WriteLog(ex.Message, Logging.LogType.Error, Logging.LogCaller.AeonRuntime);
                    }
                    finally
                    {
                        SketchThread.Join();
                    }
                }
            }
        }
        public static void SetMoodic(string emotion)
        {
            string moodicEmotion;
            switch (emotion)
            {
                case "Happy":
                    moodicEmotion = "moodicRoutine01";
                    LastSketch = emotion;
                    break;
                case "Confident":
                    moodicEmotion = "moodicRoutine01";
                    LastSketch = emotion;
                    break;
                case "Energized":
                    moodicEmotion = "moodicRoutine01";
                    LastSketch = emotion;
                    break;
                case "Helped":
                    moodicEmotion = "moodicRoutine01";
                    LastSketch = emotion;
                    break;
                case "Insecure":
                    moodicEmotion = "moodicRoutine01";
                    LastSketch = emotion;
                    break;
                case "Sad":
                    moodicEmotion = "moodicRoutine01";
                    LastSketch = emotion;
                    break;
                case "Hurt":
                    moodicEmotion = "moodicRoutine01";
                    LastSketch = emotion;
                    break;
                case "alone":
                    moodicEmotion = "moodicRoutine01";
                    LastSketch = emotion;
                    break;
                default:
                    moodicEmotion = "moodicRoutine01";
                    break;
            }
            if (emotion == "")
            {
                moodicEmotion = "quiet";
            }
            var uploader = new ArduinoSketchUploader(new ArduinoSketchUploaderOptions
            {
                FileName = Environment.CurrentDirectory + @"\moodics\" + moodicEmotion + @".ino.standard.hex",
                PortName = _thisAeon.GlobalSettings.GrabSetting("arduinocomport"),
                ArduinoModel = ArduinoModel.UnoR3
            });
            if (SketchThread == null)
            {
                try
                {
                    // Send the upload task to a separate thread.
                    SketchThread = new Thread(uploader.UploadSketch);
                    SketchThread.Start();
                    SketchUploaded = true;
                    if (moodicEmotion != "quiet")
                    {
                        if (MoodicDurationUsed)
                        {
                            _thisAeon.MoodicTimer.Enabled = true;
                            _thisAeon.MoodicTimer.Start();
                        }
                    }    
                }
                catch (Exception ex)
                {
                    Logging.WriteLog(ex.Message, Logging.LogType.Error, Logging.LogCaller.AeonRuntime);
                }
                finally
                {
                    if (SketchThread != null)
                        SketchThread.Join();
                }
            }
        }
        public static void SetQuiet()
        {
            var uploader = new ArduinoSketchUploader(new ArduinoSketchUploaderOptions
            {
                FileName = Environment.CurrentDirectory + @"\moodics\" + @"quiet.ino.standard.hex",
                PortName = _thisAeon.GlobalSettings.GrabSetting("arduinocomport"),
                ArduinoModel = ArduinoModel.UnoR3
            });
            if (SketchThread == null)
            {
                try
                {
                    // Send the upload task to a separate thread.
                    SketchThread = new Thread(uploader.UploadSketch);
                    SketchThread.Start();
                    SketchUploaded = true;
                }
                catch (Exception ex)
                {
                    Logging.WriteLog(ex.Message, Logging.LogType.Error, Logging.LogCaller.AeonRuntime);
                }
                finally
                {
                    if (SketchThread != null)
                        SketchThread.Join();
                }
            }
            if (SketchThread != null)
            {
                if (!SketchThread.IsAlive)
                {
                    try
                    {
                        // Send the upload task to a separate thread.
                        SketchThread = new Thread(uploader.UploadSketch);
                        SketchThread.Start();
                        SketchUploaded = true;
                    }
                    catch (Exception ex)
                    {
                        Logging.WriteLog(ex.Message, Logging.LogType.Error, Logging.LogCaller.AeonRuntime);
                    }
                    finally
                    {
                        SketchThread.Join();
                    }
                }
            }
        }
        private static void MoodicEvent(object source, ElapsedEventArgs e)
        {
            if (!_moodicThread.IsAlive)
            {
                _moodicThread = new Thread(SetQuiet) { IsBackground = true };
                _moodicThread.Start();
            }
        }
        static string ReturnTonalPath()
        {
            return TonalRootPath + "\\tones\\";
        }
        #endregion

        #region Debugging
        protected static string GenerateAeonOutputDebug()
        {
            if (!Equals(null, _thisResult))
            {
                var result = new StringBuilder();

                foreach (var query in _thisResult.SubQueries)
                {
                    result.Append("Pattern: " + query.Trajectory + Environment.NewLine);
                    result.Append("Template: " + query.Template + Environment.NewLine);
                    result.Append("Emotion stars: ");
                    foreach (var emotion in query.EmotionStar)
                    {
                        result.Append(emotion + ", ");
                    }
                    result.Append(Environment.NewLine);
                    result.Append("Input stars: ");
                    foreach (var star in query.InputStar)
                    {
                        result.Append(star + ", ");
                    }
                    result.Append(Environment.NewLine);
                    result.Append("That stars: ");
                    foreach (var that in query.ThatStar)
                    {
                        result.Append(that + ", ");
                    }
                    result.Append(Environment.NewLine);
                    result.Append("Topic stars: ");
                    foreach (var topic in query.TopicStar)
                    {
                        result.Append(topic + ", ");
                    }
                }
                return result.ToString();
            }
            return "";
        }
        #endregion
    }
}
