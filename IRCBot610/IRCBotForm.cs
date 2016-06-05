using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;
using System.IO;

using System.Threading;
using System.Speech.Synthesis;

using MovablePython; // Global hotkey thing I borrowed from somewhere. You can tell it's not mine because it's well documented.

using System.Runtime.InteropServices;

namespace IRCBot610
{
    public partial class IRCBotForm : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(IntPtr hWnd, Int32 Msg, IntPtr wParam, IntPtr lParam);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetScrollPos(IntPtr hWnd, System.Windows.Forms.Orientation nBar);

        [DllImport("user32.dll")]
        static extern bool GetScrollRange(IntPtr hWnd, int nBar, out int lpMinPos, out int lpMaxPos);

        String[] args;
        
        bool gotmotd = false;

        TcpClient client = null;
        NetworkStream ns = null;
        StreamReader sr = null;
        StreamWriter sw = null;

        Thread listenThread, speakThread;
        int msgId = 0;

        SpeechSynthesizer voice;

        public String nick = "", password = "", server = "", channel = "";
        int port = 6667, volume = 50, rate = 0, wait = 0;
        int pingTimer = 0;
        public int delay = 250;
        int fontSize = 16;

        String configfile = "config.txt";

        List<Prompt2> prompts;
        List<String> muted;
        List<String> whitelist;
        List<String> commands, responses;

        public String mainText = "", command = "", consoleText="";
        public bool kill = false;
        bool connected = false;

        bool on = true, UIUpdated = false;

        bool listening = true, speaking = true;

        int lengthLimit = 0;

        int mode= 1;

        PasswordForm pwform;

        String[] filter = new String[0];
        Dictionary<String, String> filter2 = new Dictionary<String, String>();

        Dictionary<String, String> users;
        bool usersUpdated = false;

        // More global hotkey stuff
        public List<Hotkey> hotkeys = new List<Hotkey>();
        public Hotkey hk = new Hotkey(), interruptKey = new Hotkey(Keys.Oemtilde, false, false, false, false);
        public Hotkey2 hotkey2;
        public Thread hotkeyThread;
        public Hotkey hotkeyDown;

        String version = "0.17";

        int talkcooldown = 0;

        WebRequest request;
        WebResponse response;

        String recentusers = "";
        int ticks;

        public IRCBotForm(String[] args)
        {
            InitializeComponent();
            InputBox.MouseWheel += InputBox_MouseWheel;
            this.args = args;
        }

        private void IRCBotForm_Load(object sender, EventArgs e)
        {
            if (args.Length > 0)
            {
                configfile = args[0];
            }
            
            Text = "IRCBot610 v" + version;
            prompts = new List<Prompt2>();
            muted = new List<String>();
            whitelist = new List<String>();
            commands = new List<String>();
            responses = new List<String>();

            voice = new SpeechSynthesizer();
            voice.Volume = 50;
            voice.Rate = 0;

            readConfig();
            loadFilter();
            loadStuff();

            volumeBox.Text = volume+"";
            volumeSlider.Value = volume;

            rateBox.Text = rate + "";
            rateSlider.Value = rate;

            waitBox.Text = wait + "";
            waitSlider.Value = wait;
            
            speakThread = new Thread(speak);
            speakThread.Start();

            listenThread = new Thread(listen);

            if (server == "")
            {
                server = "irc.twitch.tv";
            }

            if (!channel.Contains("#")) channel = "#" + channel;

            pwform = new PasswordForm(this);

            if (nick != "" && password != "")
            {
                connect();
            }

            hotkeyDown = new Hotkey();

            hotkey2 = new Hotkey2(this);
            hotkeyThread = new Thread(hotkey2.monitor);
            hotkeyThread.Start();

            hotkeys.Add(new Hotkey(Keys.Oemtilde));

            users = new Dictionary<String, String>();

            setMode(1);
        }

        public Hotkey parseHotkey(String key)
        {
            key = key.ToLower();
            try
            {
                Hotkey hk = new Hotkey();
                if (key.IndexOf("alt") != -1) hk.Alt = true;
                if (key.IndexOf("control") != -1 || key.IndexOf("ctrl") != -1) hk.Control = true;
                if (key.IndexOf("shift") != -1) hk.Shift = true;

                String[] temp = key.Split("+".ToCharArray());
                String tempKey = temp[temp.Length - 1].Trim();
                tempKey = ("" + tempKey[0]).ToUpper() + tempKey.Substring(1).Trim();
                hk.KeyCode = (Keys)(new KeysConverter().ConvertFromString(tempKey));

                return hk;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hotkey problem: " + ex.ToString());
                return null;
            }
        }

        public void keyDown(Keys e, bool alt, bool control, bool shift)
        {
            hotkeyDown = new Hotkey();
            
            for (int i = 0; i < hotkeys.Count; i++)
            {
                if (e == hotkeys[i].KeyCode)
                {
                    if (alt == hotkeys[i].Alt && control == hotkeys[i].Control && shift == hotkeys[i].Shift)
                    {
                        hotkeyDown = new Hotkey(e, alt, control, shift, false);
                        break;
                    }
                }
            }

            if (null != hotkeyDown)
            {

                if (e == interruptKey.KeyCode && alt == interruptKey.Alt && control == interruptKey.Control && shift == interruptKey.Shift)
                {
                    cancelSpeech();
                }
            }

        }

        public void connect()
        {
            try
            {
                sw.Close();
                sr.Close();
                ns.Close();
                client.Close();
            }
            catch (Exception ex) {
                
            }

            try
            {
                client = new TcpClient(server, port);
                ns = client.GetStream();
                sr = new StreamReader(ns);
                sw = new StreamWriter(ns);
            }
            catch (Exception ex) {
                writeLine("Connection failed " + ex.ToString());
            }

            listenThread.Start();

            if (password != "") send("PASS " + password);
            send("USER " + nick + " 0 * :" + nick);
            send("NICK " + nick);
            if (server.ToLower().Contains("twitch"))
            {
                send("CAP REQ :twitch.tv/membership"); // This makes the users list work
            }
            
            send("JOIN " + channel);
        }

        public void send(String msg)
        {
            try
            {
                sw.WriteLine(msg);
                sw.Flush();
            }
            catch (Exception ex)
            {
                writeLine("Failed to send message to server");
            }
        }

        public void listen()
        {
            char[] split = { ' ' };
            bool thinghappened;
            while (!kill)
            {
                thinghappened = false;
                if (listening)
                {
                    try
                    {
                        bool shh = false;
                        bool read = true;

                        String line = sr.ReadLine();
                        if (line != null)
                        {
                            thinghappened = true;

                            //writeLine(line); // Debug!

                            String[] stuff = line.Split(split, 4);

                            if (stuff[0].Equals("PING",StringComparison.InvariantCultureIgnoreCase))
                            {
                                try
                                {
                                    //send("PONG");
                                    send("PONG" + " " + stuff[1]);
                                }
                                catch (Exception ex)
                                {
                                    writeLine("Failed to send ping reply! " + ex.ToString());
                                    prompts.Add(new Prompt2("Ping reply failed. I'm probably gonna time out."));
                                    //send("PONG");
                                }
                                shh = true;
                            }
                            else
                            {
                                if (stuff[1].Equals("PRIVMSG",StringComparison.InvariantCultureIgnoreCase))
                                {
                                    try
                                    {
                                        if (stuff[2].Contains("#"))
                                        {
                                            char split2 = ' ';
                                            String sFull = stuff[3].Substring(1).Trim();
                                            String name = getName(stuff[0]);
                                            
                                            addUser(name, true);

                                            String text = "\\b " + name + "\\b0 : " + sFull.Replace("\\", "\\\\"); ;

                                            if (!canSpeak(name))
                                            {
                                                text = "\\i " + text + "\\i0 ";
                                            }

                                            writeLine(text);

                                            String[] s = sFull.Split(split2);

                                            if (s[0][0] == '!')
                                            {
                                                read = false;
                                            }

                                            bool done = false;

                                            try
                                            {
                                                if (users[name].Contains("@") || users[name].Contains('~'))
                                                {
                                                    if (s[0].Equals("!stfu", StringComparison.InvariantCultureIgnoreCase))
                                                    {
                                                        done = true;
                                                        try
                                                        {
                                                            if (prompts.Count == 1)
                                                            {
                                                                cancelSpeech();
                                                            }
                                                            else
                                                            {
                                                                prompts.RemoveAt(prompts.Count - 1);
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            writeLine("!stfu error: " + ex.ToString());
                                                        }
                                                    }
                                                    else if (s[0].Equals("!nope", StringComparison.InvariantCultureIgnoreCase))
                                                    {
                                                        done = true;
                                                        try
                                                        {
                                                            if (prompts.Count > 0)
                                                            {
                                                                cancelAllSpeech();
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            writeLine("Error with !nope: " + ex.ToString());
                                                        }
                                                    }
                                                    else if (s[0].Equals("!on", StringComparison.InvariantCultureIgnoreCase))
                                                    {
                                                        done = true;
                                                        prompts.Add(new Prompt2("Speech enabled"));
                                                        on = true;
                                                        UIUpdated = true;
                                                    }
                                                    else if (s[0].Equals("!off", StringComparison.InvariantCultureIgnoreCase))
                                                    {
                                                        done = true;

                                                        cancelAllSpeech();

                                                        on = false;
                                                        UIUpdated = true;
                                                    }
                                                    else if (s[0].Equals("!volume", StringComparison.InvariantCultureIgnoreCase))
                                                    {
                                                        int n = volume;
                                                        try
                                                        {
                                                            n = int.Parse(s[1]);
                                                            setVolume(n);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            write("Volume change failed: " + ex.ToString());
                                                        }
                                                    }
                                                    else if (s[0].Equals("!speed", StringComparison.InvariantCultureIgnoreCase))
                                                    {
                                                        int n = rate;
                                                        try
                                                        {
                                                            n = int.Parse(s[1]);
                                                            setRate(n);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            write("Speed change failed: " + ex.ToString());
                                                        }
                                                    }
                                                    else if (s[0].Equals("!wait", StringComparison.InvariantCultureIgnoreCase))
                                                    {
                                                        int n = wait;
                                                        try
                                                        {
                                                            n = int.Parse(s[1]);
                                                            setDelay(n);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            write("Delay change failed: " + ex.ToString());
                                                        }
                                                    }
                                                    else if (s[0].Equals("!flip", StringComparison.InvariantCultureIgnoreCase))
                                                    {
                                                        done = true;
                                                        say("(╯°□°）╯︵ ┻━┻");
                                                    }
                                                    else if (s[0].Equals("!mute", StringComparison.InvariantCultureIgnoreCase))
                                                    {
                                                        done = true;

                                                        String temp = s[1].ToLower();

                                                        try
                                                        {
                                                            if (!muted.Contains(temp))
                                                            {
                                                                muted.Add(temp);
                                                                writeLine(temp + " added to mute list.");
                                                            }
                                                            else
                                                            {
                                                                writeLine(temp + " is already muted.");
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            writeLine("Muting error: " + ex.ToString());
                                                        }
                                                    }
                                                    else if (s[0].Equals("!unmute", StringComparison.InvariantCultureIgnoreCase))
                                                    {
                                                        done = true;

                                                        String temp = s[1].ToLower();

                                                        try
                                                        {
                                                            if (muted.Contains(temp))
                                                            {
                                                                muted.Remove(temp);
                                                                writeLine(temp + " removed from mute list.");
                                                            }
                                                            else
                                                            {
                                                                writeLine(temp + " wasn't in the mute list.");
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            writeLine("Unmuting error: " + ex.ToString());
                                                        }
                                                    }
                                                    else if (s[0].Equals("!whitelist", StringComparison.InvariantCultureIgnoreCase))
                                                    {
                                                        done = true;

                                                        String temp = s[1].ToLower();

                                                        try
                                                        {
                                                            if (!whitelist.Contains(temp))
                                                            {
                                                                whitelist.Add(temp);
                                                                writeLine(temp + " added to whitelist.");
                                                            }
                                                            else
                                                            {
                                                                writeLine(temp + " is already whitelisted.");
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            writeLine("Whitelist error: " + ex.ToString());
                                                        }
                                                    }
                                                    else if (s[0].Equals("!unwhitelist", StringComparison.InvariantCultureIgnoreCase))
                                                    {
                                                        done = true;

                                                        String temp = s[1].ToLower();

                                                        try
                                                        {
                                                            if (whitelist.Contains(temp))
                                                            {
                                                                whitelist.Remove(temp);
                                                                writeLine(temp + " removed from whitelist.");
                                                            }
                                                            else
                                                            {
                                                                writeLine(temp + " wasn't whitelisted.");
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            writeLine("Unwhitelist error: " + ex.ToString());
                                                        }
                                                    }
                                                    else if (s[0].Equals("!mode", StringComparison.InvariantCultureIgnoreCase))
                                                    {
                                                        try
                                                        {
                                                            int n = int.Parse(s[1]);
                                                            setMode(n);
                                                            done = true;
                                                        }
                                                        catch (Exception ex)
                                                        {

                                                        }
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                if (users.Keys.Contains(name))
                                                {
                                                    writeLine("Generic error #0! " + ex.ToString());
                                                }
                                            }

                                            if (!done)
                                            {
                                                bool allow = true;

                                                if (muted.Contains(name))
                                                {
                                                    allow = false;
                                                }
                                                
                                                if (allow)
                                                {
                                                    bool speak = true;

                                                    // mode 1 is assumed, since it's default.
                                                    if (mode == 2)
                                                    {
                                                        speak = false;
                                                    }

                                                    if (s[0].Equals("!silent", StringComparison.InvariantCultureIgnoreCase))
                                                    {
                                                        // Do nothin'!
                                                    }
                                                    else if ((s[0].Equals("!download", StringComparison.InvariantCultureIgnoreCase) || s[0].Equals("!help", StringComparison.InvariantCultureIgnoreCase)) && talkcooldown == 0)
                                                    {
                                                        //done = true;

                                                        say("(´･ω･`)ﾉ Here you go! http://www.project610.com/anna");
                                                        talkcooldown = 100;
                                                    }
                                                    else if (s[0].Equals("!time", StringComparison.InvariantCultureIgnoreCase))
                                                    {
                                                        String timechannel = channel;
                                                        if (s.Length >= 2)
                                                        {
                                                            timechannel = s[1];
                                                        }
                                                        timechannel = timechannel.Substring(timechannel.IndexOf('#') + 1);

                                                        //WebRequest request = HttpWebRequest.Create("http://www.project610.com/twitch.php?channel="+timechannel+"&time");
                                                        request = HttpWebRequest.Create("http://www.project610.com/twitch.php?channel=" + timechannel + "&time");
                                                        response = request.GetResponse();

                                                        Stream dataStream = response.GetResponseStream();
                                                        StreamReader reader = new StreamReader(dataStream);

                                                        String responseLine = "";

                                                        if ((responseLine = reader.ReadLine()) != null)
                                                        {
                                                            say("The stream (" + timechannel + ") has been running for: " + responseLine);
                                                        }

                                                        talkcooldown = 100;
                                                    }
                                                    else if (s[0].Equals("!title", StringComparison.InvariantCultureIgnoreCase))
                                                    {
                                                        String titlechannel = channel;
                                                        if (s.Length >= 2)
                                                        {
                                                            titlechannel = s[1];
                                                        }
                                                        titlechannel = titlechannel.Substring(titlechannel.IndexOf('#') + 1);

                                                        request = HttpWebRequest.Create("http://www.project610.com/twitch.php?channel=" + titlechannel + "&title");
                                                        response = request.GetResponse();

                                                        Stream dataStream = response.GetResponseStream();
                                                        StreamReader reader = new StreamReader(dataStream);

                                                        String responseLine = "";

                                                        if ((responseLine = reader.ReadLine()) != null)
                                                        {
                                                            say("!- Title: " + responseLine);
                                                        }

                                                        talkcooldown = 100;
                                                    }
                                                    else
                                                    {
                                                        for (int i = 0; i < commands.Count; i++)
                                                        {
                                                            if (s[0].Equals(commands[i], StringComparison.InvariantCultureIgnoreCase))
                                                            {
                                                                say(responses[i]);
                                                                break;
                                                            }
                                                        }

                                                        if (on)
                                                        {
                                                            if (lengthLimit == 0 || sFull.Length < lengthLimit || name.ToLower().Contains("yoshilover")) // If the YoshiLover exception is frowned upon by too many, I'll probably have to make this optional
                                                            {
                                                                if (sayNamesBox.Checked)
                                                                {
                                                                    sFull = filterMessage(name) + ", " + sFull;
                                                                }

                                                                // No if mode 1, since speak would be true.
                                                                if (mode == 2)
                                                                {
                                                                    speak = false;
                                                                    if (canSpeak(name))
                                                                    {
                                                                        speak = true;
                                                                    }
                                                                }

                                                                if (speak && read)
                                                                {
                                                                    sFull = butcher(sFull).Trim();
                                                                    sFull = filterMessage(sFull);
                                                                    sFull = butcher(sFull).Trim();

                                                                    prompts.Add(new Prompt2(sFull.ToLower()));
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                            }
                                            shh = true;
                                        }
                                        else
                                        {
                                            shh = true;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        writeLine("PRIVMSG error: " + ex.ToString());
                                    }
                                }
                                else if (stuff[1].Equals("JOIN",StringComparison.InvariantCultureIgnoreCase))
                                {
                                    try
                                    {
                                        String name = getName(stuff[0]).Trim();

                                        addUser(name, true);
                                    }
                                    catch (Exception ex)
                                    {
                                        writeLine("Someone joined, but there was an error! " + ex.ToString());
                                    }
                                    shh = true;
                                }
                                else if (stuff[1].Equals("PART", StringComparison.InvariantCultureIgnoreCase) || stuff[1].Equals("QUIT", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    try
                                    {
                                        String name = getName(stuff[0]);
                                        //writeLine(name + " left " + channel);
                                        try
                                        {
                                            users.Remove(name);
                                            usersUpdated = true;
                                        }
                                        catch (Exception ex)
                                        {
                                            writeLine("Someone left, but there was an error removing them from the user list! " + ex.ToString());
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        writeLine("Someone left, but there was an error saying so! " + ex.ToString());
                                    }
                                    shh = true;
                                }
                                else if (stuff[1].Equals("MODE",StringComparison.InvariantCultureIgnoreCase))
                                {
                                    String mode = stuff[3].Split(split)[0];
                                    String name = "";

                                    try
                                    {
                                        name = stuff[3].Split(split)[1];
                                    }
                                    catch (Exception ex)
                                    {
                                    }

                                    if (mode.Equals("+o",StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        try
                                        {
                                            if (users.ContainsKey(name))
                                            {
                                                if (!users[name].Contains("@"))
                                                {
                                                    users[name] += "@";
                                                    usersUpdated = true;
                                                }
                                            }
                                            if (!usersUpdated)
                                            {
                                                try
                                                {
                                                    users.Add(name, "@");
                                                    usersUpdated = true;
                                                }
                                                catch (Exception ex) { }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            writeLine("There was an error setting mode +o to " + name + "! " + ex.ToString());
                                        }
                                    }
                                    else if (mode.Equals("-o",StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        try
                                        {
                                            if (users.ContainsKey(name))
                                                users[name].Replace("@", "");
                                            usersUpdated = true;
                                        }
                                        catch (Exception ex)
                                        {
                                            writeLine("There was an error setting mode -o to " + name + "! " + ex.ToString());
                                        }
                                    }
                                    shh = true;
                                }
                                else
                                {
                                    int n = -1;
                                    try
                                    {
                                        n = int.Parse(stuff[1]);
                                        if (n < 400 && !debugBox.Checked)
                                        {
                                            shh = true;
                                        }
                                        if (n == 376)
                                        {
                                            gotmotd = true;

                                            prompts.Add(new Prompt2("TTS is working and stuff"));
                                            send("JOIN " + channel);
                                            send("PRIVMSG nickserv IDENTIFY " + password);

                                            request = HttpWebRequest.Create("http://www.project610.com/twitch.php");
                                            response = request.GetResponse();

                                        }
                                        else if (n == 353)
                                        {
                                            String[] wholist = stuff[3].Substring(stuff[3].IndexOf(":")+1).Split(' ');
                                            foreach (String name in wholist)
                                            {
                                                if (name.Trim() != "") addUser(name, false);
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                            }

                            if (line.Trim() != "")
                            {
                                if (!shh) writeLine(msgId.ToString("0000") + "> " + line);
                                msgId++;
                            }

                        }
                        else
                        {
                            
                        }
                    }
                    catch (Exception ex)
                    {
                        if (!kill)
                        {
                            writeLine("Something broke in the listen thread. It's been stopped.\n" + ex.ToString());
                            voice.SpeakAsync("Listen thread broke down, gonna want to restart that IRC bot.");
                        }
                        listening = false;
                    }
                }
                if (!thinghappened) Thread.Sleep(delay);
            }
        }

        public void addUser(String name, bool verbose)
        {
            if (!users.ContainsKey(name))
            {
                //if (verbose) writeLine(name + " joined " + channel);

                users.Add(name, "");
                recentusers += "," + name;

                usersUpdated = true;
            }
        }

        public void removeUser(String s)
        {

        }

        public void setMode (int num, bool verbose = false)
        {
            if (mode == num)
            {
                if (verbose) say("Mode is already " + num);
            }
            else
            {
                if (1 <= num && num <= 2)
                {
                    mode = num;
                    if (verbose) say("Mode set to " + num);

                    UIUpdated = true;
                }
            }
        }

        public void setVolume(int num)
        {
            if (0 <= num && num <= 100)
            {
                volume = num;
                UIUpdated = true;
            }
        }

        public void setRate(int num)
        {
            if (-10 <= num && num <= 10)
            {
                rate = num;
                UIUpdated = true;
            }
        }

        public bool canSpeak (String name)
        {
            if (muted.Contains(name)) {
                return false;
            }
            else if (mode == 1)
            {
                return true;
            }
            else if (mode == 2)
            {
                try
                {
                    if (whitelist.Contains(name) || users[name].Contains('@') || users[name].Contains('~') || users[name].Contains('+')) // ~ and + are for non-Twitch IRC things, and probably maybe don't work anyway
                    {
                        return true;
                    }
                }
                catch (Exception ex) { }
            }

            return false;
        }

        public void setDelay(int num)
        {
            if (0 <= num && num <= 60)
            {
                delay = num;
                UIUpdated = true;
            }
        }

        public void speak()
        {
            Prompt2 current = null;

            while (!kill)
            {
                if (speaking && on)
                {
                    try
                    {
                        if (prompts.Count > 0)
                        {
                            if (voice.State == SynthesizerState.Ready)
                            {
                                try
                                {
                                    if (current != null)
                                    {
                                        if (prompts[0].IsCompleted)
                                        {
                                            prompts.RemoveAt(0);
                                            current = null;
                                        }
                                    }
                                    else
                                    {
                                        Thread.Sleep(wait*1000);
                                        voice.SpeakAsync(prompts[0]);
                                        current = prompts[0];
                                    }
                                }
                                catch (Exception ex)
                                {
                                    writeLine("Clearing prompts, something broke. " + ex.ToString());
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        writeLine("SHIT SHIT SHIT: " + ex.ToString());
                        voice.SpeakAsync("Shit shit shit, the TTS queue broke down");
                        speaking = false;
                    }
                }
                Thread.Sleep(delay);
            }
        }

        public void cancelSpeech()
        {
            try
            {
                voice.SpeakAsyncCancel(prompts[0]);
                writeLine("Cancelled message: " + prompts[0].Text);
            }
            catch (Exception ex)
            {
                if (prompts.Count > 0)
                {
                   writeLine("Failed to cancel single prompt.");
                }
            }
        }

        public void say(String s) {
            send("PRIVMSG " + channel + " :" + s);
            writeLine("\\b " + nick + ":\\b0  " + s);
        }

        public String filterMessage(String s)
        {
            foreach (String key in filter2.Keys)
            {
                // This is the old method
                s = s.ReplaceIgnoreCase(key, filter2[key]);

                // New method
                /*String[] split = {key};
                String[] temp = s.Split(split, StringSplitOptions.None);*/
            }
            
            return s;
        }

        public String butcher(String s)
        {
            char[] split = { ' ' };
            String[] stuff = s.Split(split);
            s = "";
            for (int i = 0; i < stuff.Length; i++)
            {
                if (stuff[i].Contains("://") || stuff[i].ToLower().Contains("www.") || stuff[i].ToLower().Contains(".com"))
                {
                    stuff[i] = "(link)";
                }

                s += " " + stuff[i];
            }

            Char temp = ' ';
            int count = 0;
            for (int i = 0; i < s.Length; i++) {
                if (temp == s[i])
                {
                    count++;
                    if (count > 2)
                    {
                        s = s.Remove(i, 1);
                        i--;
                    }
                }
                else count = 0;
                temp = s[i];
            }

            return s;
        }

        public void readConfig()
        {
            try
            {
                String text = File.ReadAllText(configfile).ToLower();

                //char[] split = { ';' };
                String[] lines = File.ReadAllLines(configfile); //text.Split(split);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains("//"))
                    {
                        lines[i] = lines[i].Substring(0, lines[i].IndexOf("//"));
                    }

                    if (lines[i].Length > 0)
                    {
                        lines[i] = lines[i].Trim();
                        if (lines[i][lines[i].Length - 1] == ';')
                            lines[i] = lines[i].Substring(0, lines[i].Length - 1);

                        char[] split = { '=' };
                        String[] stuff = lines[i].Split(split, 2);

                        stuff[0] = stuff[0].ToLower().Trim();

                        if (stuff[0] == "username")
                        {
                            nick = stuff[1].ToLower();
                        }
                        else if (stuff[0] == "password")
                        {
                            password = stuff[1];
                        }
                        else if (stuff[0] == "channel")
                        {
                            channel = stuff[1].ToLower();
                        }
                        else if (stuff[0] == "volume")
                        {
                            try
                            {
                                volume = int.Parse(stuff[1]);
                                voice.Volume = volume;
                            }
                            catch (Exception ex) { }
                        }
                        else if (stuff[0] == "speed")
                        {
                            try
                            {
                                rate = int.Parse(stuff[1]);
                                voice.Rate = rate;
                            }
                            catch (Exception ex) { }
                        }
                        else if (stuff[0] == "speechdelay")
                        {
                            try
                            {
                                wait = int.Parse(stuff[1]);
                            }
                            catch (Exception ex) { }
                        }
                        else if (stuff[0] == "silencekey")
                        {
                            stuff[1] = stuff[1].ToLower();
                            try
                            {
                                interruptKey = parseHotkey(stuff[1]);
                            }
                            catch (Exception ex) { }
                        }
                        else if (stuff[0] == "server")
                        {
                            stuff[1] = stuff[1].ToLower();
                            server = stuff[1];
                        }
                        else if (stuff[0] == "blind")
                        {
                            stuff[1] = stuff[1].ToLower();
                            if (stuff[1] == "true" || stuff[1] == "1")
                            {
                                blindBox.Checked = true;
                                ConsoleBox.Font = new Font(ConsoleBox.Font.FontFamily, 11);
                                fontSize = 23;
                            }
                        }
                        else if (stuff[0] == "saynames")
                        {
                            stuff[1] = stuff[1].ToLower();
                            if (stuff[1] == "true" || stuff[1] == "1")
                            {
                                sayNamesBox.Checked = true;
                            }
                        }
                        else if (stuff[0] == "mode")
                        {
                            try
                            {
                                int n = int.Parse(stuff[1]);
                                if (1 <= n && n <= 2)
                                {
                                    mode = n;
                                }
                            }
                            catch (Exception ex) { }
                        }
                        else if (stuff[0] == "lengthlimit")
                        {
                            try
                            {
                                lengthLimit = int.Parse(stuff[1]);
                            }
                            catch (Exception ex) {
                                writeLine("Failed to set length limit.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Well that didn't work. Your config file is broken or something. " + ex.ToString());
            }

        }

        public void loadFilter()
        {
            try
            {
                filter = File.ReadAllLines("filter.txt");
                Char split = '=';
                foreach (String s in filter)
                {
                    if (s.IndexOf("//") == 0 || s.Trim() == "")
                    {

                    }
                    else
                    {
                        if (s.IndexOf(split) > 0)
                        {
                            filter2[s.Split(split)[0]] = s.Split(split)[1];
                        }
                        else
                        {
                            filter2[s] = "(censored)";
                        }
                    }
                }
                writeLine("Filter loaded! Filtered phrases count: " + filter2.Count);
            }
            catch (Exception ex) {
                filter = new String[0];
                writeLine("Failed to read filter.txt. Only the basic filter will be used. " + ex.ToString());
            }
        }

        public void loadStuff()
        {
            try
            {
                String[] temp = File.ReadAllLines("muted.txt");

                foreach (String s in temp)
                {
                    muted.Add(s);
                }
                writeLine("Mute list loaded! Silenced users count: " + muted.Count);
            }
            catch (Exception ex)
            {
                writeLine("Failed to read muted.txt. " + ex.ToString());
            }

            try
            {
                String[] temp = File.ReadAllLines("whitelist.txt");

                foreach (String s in temp)
                {
                    whitelist.Add(s);
                }
                writeLine("Whitelist loaded! Privileged users count: " + whitelist.Count);
            }
            catch (Exception ex)
            {
                writeLine("Failed to read whitelist.txt. " + ex.ToString());
            }

            try
            {
                String[] temp = File.ReadAllLines("responses.txt");

                foreach (String s in temp)
                {
                    String[] temp2 = s.Split("=".ToCharArray(), 2);
                    if (temp2[0].Trim() != "" && temp2[0].IndexOf("//") == -1 && temp2.Length == 2)
                    {
                        commands.Add(temp2[0]);
                        responses.Add(temp2[1]);
                    }
                }
                writeLine("Responses loaded! Commands count: " + commands.Count);
            }
            catch (Exception ex)
            {
                writeLine("Failed to read responses.txt. " + ex.ToString());
            }
        }

        public void saveStuff()
        {
            File.WriteAllLines("muted.txt", muted.ToArray());
            File.WriteAllLines("whitelist.txt", whitelist.ToArray());
            //File.WriteAllLines("responses.txt", SOMETHING); Need to add ability to add commands at runtime
        }

        public void writeLine(String s)
        {
            write(DateTime.Now.ToString("(MM/dd/yy) [HH:mm:ss] ") + s + "\\par"/*"\r\n"*/);

        }

        public void write(String s)
        {
            mainText += s;
        }

        public String getName(String s)
        {
            s = s.Substring(1, s.IndexOf("!") - 1);

            return s;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            ticks++;

            try
            {
                // Nothing??
            }
            catch (Exception ex) {
                MessageBox.Show("STREAM ERROR: " + ex.ToString());
            }

            if (talkcooldown > 0)
            {
                talkcooldown--;
            }

            if (usersUpdated)
            {
                try
                {
                    List<String> temp = new List<String>();
                    
                    foreach (String key in users.Keys)
                    {
                        temp.Add(users[key] + key);
                    }
                    temp.Sort();
                    
                    userBox.Items.Clear();

                    foreach (String temp2 in temp)
                    {
                        userBox.Items.Add(temp2);
                    }
                }
                catch (Exception ex)
                {
                    writeLine("Something went wrong updating the users list: " + ex.ToString());
                }
                usersUpdated = false;
            }
            if (UIUpdated)
            {
                if (on) voiceOnBox.Checked = true;
                else voiceOnBox.Checked = false;

                if (mode == 2) whitelistBox.Checked = true;
                else whitelistBox.Checked = false;

                try
                {
                    rateSlider.Value = rate;
                    rateBox.Text = rate + "";
                }
                catch (Exception ex) { }

                try
                {
                    volumeSlider.Value = volume;
                    volumeBox.Text = volume + "";
                }
                catch (Exception ex) { }


                try
                {
                    waitSlider.Value = delay;
                    waitBox.Text = delay + "";
                }
                catch (Exception ex) { }

                UIUpdated = false;
            }

            int FirstVisibleLineBefore = (int)SendMessage(ConsoleBox.Handle, 0x00CE, IntPtr.Zero, IntPtr.Zero);  

            int min = 0, max = 0;
            GetScrollRange(ConsoleBox.Handle, 1, out min, out max);
            
            if (mainText != "")
            {
                if (GetScrollPos(ConsoleBox.Handle, Orientation.Vertical) == ConsoleBox.Lines.Length)
                {

                }

                consoleText += mainText;

                String start = "{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang4105{\\fonttbl{\\f0\\fnil\\fcharset0 Microsoft Sans Serif;}}\\viewkind4\\uc1\\pard\\f0\\fs" + fontSize;
                String end = "\\par\\par}";
                ConsoleBox.Rtf = start + consoleText + end;
                
                ConsoleBox.SelectionStart = ConsoleBox.Text.Length;
                ConsoleBox.ScrollToCaret();

                mainText = "";
            }

            if (pwform != null && (password == "" && !pwform.Visible))
            {
                pwform.Show();
            }

            try
            {
                if (command.Length > 0)
                {
                    if (command[0] == '/')
                    {
                        char[] split = {' '};
                        String[] stuff = command.Split(split, 2);
                        if (stuff.Length == 1)
                        {
                            if (stuff[0] == "//") //HOTKEY
                            {
                                try
                                {
                                    voice.SpeakAsyncCancel(prompts[0]);
                                    writeLine("Cancelled message: " + prompts[0].Text);
                                }
                                catch (Exception ex)
                                {
                                    if (prompts.Count > 0)
                                    {
                                        writeLine("Failed to cancel single prompt. " + ex.ToString());
                                    }
                                }
                            }
                            else if (stuff[0] == "///")
                            {
                                cancelAllSpeech();
                            }
                            else if (stuff[0].ToLower() == "/flip")
                            {
                                send("PRIVMSG " + channel + " :(╯°□°）╯︵ ┻━┻");
                            }
                        }
                        else if (stuff.Length == 2)
                        {
                            if (stuff[0].Equals("/volume", StringComparison.InvariantCultureIgnoreCase))
                            {
                                try
                                {
                                    voice.Volume = int.Parse(stuff[1]);
                                }
                                catch (Exception ex)
                                {
                                    writeLine("Volume error.");
                                }
                                writeLine("TTS volume: " + voice.Volume + "%");
                            }
                            else if (stuff[0].Equals("/rate", StringComparison.InvariantCultureIgnoreCase))
                            {
                                try
                                {
                                    voice.Rate = int.Parse(stuff[1]);
                                    writeLine("Speech rate changed to: " + voice.Rate);
                                }
                                catch (Exception ex)
                                {
                                    writeLine("That rate change didn't work for some reason. Rate remains at: " + voice.Rate);
                                }
                            }
                            else if (stuff[0].Equals("/say", StringComparison.InvariantCultureIgnoreCase))
                            {
                                say(stuff[1]);
                            }
                            else if (stuff[0].ToLower() == "/join" || stuff[0].ToLower() == "/j")
                            {
                                String temp = "";
                                if (stuff[1][0] != '#') temp = "#";
                                send("JOIN " + temp + stuff[1]);
                                send("WHO " + temp + stuff[1]);
                            }
                            else if (stuff[0].ToLower() == "/part")
                            {
                                String temp = "";
                                if (stuff[1][0] != '#') temp = "#";
                                send("PART " + temp + stuff[1]);
                            }
                            else if (stuff[0].ToLower() == "/mode")
                            {
                                try
                                {
                                    int n = int.Parse(stuff[1]);
                                    setMode(n);
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                            else if (stuff[0].ToLower() == "/mute")
                            {
                                String temp = stuff[1].ToLower();

                                try
                                {
                                    if (!muted.Contains(temp))
                                    {
                                        muted.Add(temp);
                                        writeLine(temp + " added to mute list.");
                                    }
                                    else
                                    {
                                        writeLine(temp + " is already muted.");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    writeLine("Muting error: " + ex.ToString());
                                }
                            }
                            else if (stuff[0].ToLower() == "/unmute")
                            {
                                String temp = stuff[1].ToLower();

                                try
                                {
                                    if (muted.Contains(temp))
                                    {
                                        muted.Remove(temp);
                                        writeLine(temp + " removed from mute list.");
                                    }
                                    else
                                    {
                                        writeLine(temp + " wasn't in the mute list.");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    writeLine("Unmuting error: " + ex.ToString());
                                }
                            }
                            else if (stuff[0].ToLower() == "/whitelist")
                            {
                                String temp = stuff[1].ToLower();

                                try
                                {
                                    if (!muted.Contains(temp))
                                    {
                                        whitelist.Add(temp);
                                        writeLine(temp + " added to whitelist.");
                                    }
                                    else
                                    {
                                        writeLine(temp + " is already whitelisted.");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    writeLine("Whitelist error: " + ex.ToString());
                                }
                            }
                            else if (stuff[0].ToLower() == "/unwhitelist")
                            {
                                String temp = stuff[1].ToLower();

                                try
                                {
                                    if (whitelist.Contains(temp))
                                    {
                                        whitelist.Remove(temp);
                                        writeLine(temp + " removed from whitelist.");
                                    }
                                    else
                                    {
                                        writeLine(temp + " wasn't whitelisted.");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    writeLine("Unwhitelist error: " + ex.ToString());
                                }
                            }
                        }
                    }
                    else
                    {
                        say(command);
                    }
                    command = "";
                }
            }
            catch (Exception ex)
            {
                writeLine("Something fucked up: " + ex.ToString());
                voice.SpeakAsync("Something fucked up. Restart the IRC bot.");
            }

            // Ticks is in centiseconds

            if (ticks % 600 == 0)
            {
                try
                {
                    foreach (String username in userBox.Items)
                    {

                        recentusers += "," + username.Replace("@","");
                    }
                    request = HttpWebRequest.Create("http://www.project610.com/mariomaker/index.php?recentusers=" + recentusers);
                    recentusers = "";
                    response = request.GetResponse();
                    response.Close();
                }
                catch (Exception ex) { }
            }
        }

        public void cancelAllSpeech()
        {
            try
            {
                while (prompts.Count > 1)
                {
                    prompts.RemoveAt(1);
                }

                try
                {
                    voice.SpeakAsyncCancel(prompts[0]);
                }
                catch (Exception ex)
                {
                    writeLine("Failed to cancel the first of all the prompts");
                }

                writeLine("Cancelled all messages");
            }
            catch (Exception ex)
            {

            }
        }

        private void IRCBotForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                Hide();
                cancelAllSpeech();
                saveStuff();

                kill = true;

                sw.Close();
                sr.Close();
                ns.Close();
                client.Close();
            }
            catch (Exception ex) { }
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                command = InputBox.Text;
                InputBox.Text = "";
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                InputBox.Text = "";
                e.SuppressKeyPress = true;
            }
        }

        private void rateSlider_Scroll(object sender, EventArgs e)
        {
            voice.Rate = rateSlider.Value;
            rateBox.Text = rateSlider.Value + "";
        }

        private void rateBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                voice.Rate = int.Parse(rateBox.Text);
                rateSlider.Value = int.Parse(rateBox.Text);
            }
            catch (Exception ex) { }
        }

        private void volumeSlider_Scroll(object sender, EventArgs e)
        {
            voice.Volume = volumeSlider.Value;
            volumeBox.Text = volumeSlider.Value + "";
        }

        private void volumeBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                voice.Volume = int.Parse(volumeBox.Text);
                volumeSlider.Value = int.Parse(volumeBox.Text);
            }
            catch (Exception ex) { }
        }

        private void waitSlider_Scroll(object sender, EventArgs e)
        {
            wait = waitSlider.Value;
            waitBox.Text = waitSlider.Value + "";
        }

        private void waitBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                wait = int.Parse(waitBox.Text);
                waitSlider.Value = int.Parse(waitBox.Text);
            }
            catch (Exception ex) { }
        }

        private void voiceOnBox_CheckedChanged(object sender, EventArgs e)
        {
            if (voiceOnBox.Checked)
            {
                on = true;
            }
            else
            {
                on = false;
                cancelAllSpeech();
            }
        }

        private void ConsoleBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (ConsoleBox.SelectedText.Length > 0)
            {
                Clipboard.SetText(ConsoleBox.SelectedText);
            }
            InputBox.Focus();
        }

        private void blindBox_CheckedChanged(object sender, EventArgs e)
        {
            if (blindBox.Checked)
            {
                ConsoleBox.Font = new Font(ConsoleBox.Font.FontFamily, 11);
                fontSize = 23;
            }
            else
            {
                ConsoleBox.Font = new Font(ConsoleBox.Font.FontFamily, 8.25f);
                fontSize = 16;
            }
        }

        private void sayNamesBox_CheckedChanged(object sender, EventArgs e)
        {
            if (sayNamesBox.Checked)
            {
                // Wow, this is useful
            }
            else
            {

            }
        }

        void InputBox_MouseWheel(object sender, MouseEventArgs e)
        {
            //0x020A is mousewheel, or something.
            SendMessage(ConsoleBox.Handle, 0x00B6, (IntPtr)0, (IntPtr)(-1*(e.Delta / Math.Abs(e.Delta))));
        }

        private void ConsoleBox_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(e.LinkText);
            }
            catch (Exception ex)
            {
                writeLine("Failed to do hyperlink stuff: " + ex.ToString());
            }
            InputBox.Focus();
        }

        private void whitelistBox_CheckedChanged(object sender, EventArgs e)
        {
            if (whitelistBox.Checked)
            {
                setMode(2);
            }
            else
            {
                setMode(1);
            }
        }
    }

    public class User
    {
        public String name;
        public String flag;

        public User(String name, String flag = "")
        {
            this.name = name;
            this.flag = flag;
        }

        public override String ToString() {
            return flag + name;
        }
    }

    public static class Extensions
    {
        public static string ReplaceIgnoreCase(this string source, string oldString, string newString)
        {
            int index = -1;
            // Determine if we found a match

            while ((index = source.IndexOf(oldString, StringComparison.InvariantCultureIgnoreCase)) >= 0)
            {
                // Remove the old text
                source = source.Remove(index, oldString.Length);
                // Add the replacement text
                source = source.Insert(index, newString);
            }

            return source;
        }
    }
}
