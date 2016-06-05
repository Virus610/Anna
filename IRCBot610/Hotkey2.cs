using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using MovablePython;

//using IRCBot610;

namespace IRCBot610
{
    public class Hotkey2
    {
        [DllImport("User32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);

        bool interrupt = false, keyDown = false, tempKeyDown = false;
        IRCBotForm mainForm;

        bool alt, control, shift;

        public Hotkey2(IRCBotForm mainForm)
        {
            this.mainForm = mainForm;
        }

        public void monitor()
        {
            Keys[] keys = { Keys.ShiftKey, Keys.ControlKey, Keys.Menu, mainForm.interruptKey.KeyCode };
            
            while (!mainForm.kill)
            {
                Thread.Sleep(25);

                try
                {
                    keyDown = false;
                    tempKeyDown = false;

                    if (null != mainForm.hotkeyDown)
                    {
                        if (mainForm.hotkeyDown.KeyCode != Keys.None)
                        {
                            if (GetAsyncKeyState(mainForm.hotkeyDown.KeyCode) < 0)
                            {
                                tempKeyDown = true;
                                keyDown = true;
                            }
                        }
                        else
                        {
                            for (int i = 0; i < keys.Length; i++)
                            {
                                if (keys[i].ToString() == "Menu")
                                {
                                    if (GetAsyncKeyState(keys[i]) < 0) alt = true;
                                    else alt = false;
                                }
                                else if (keys[i].ToString() == "ControlKey")
                                {
                                    if (GetAsyncKeyState(keys[i]) < 0) control = true;
                                    else control = false;
                                }
                                else if (keys[i].ToString() == "ShiftKey")
                                {
                                    if (GetAsyncKeyState(keys[i]) < 0) shift = true;
                                    else shift = false;
                                }
                                else if (GetAsyncKeyState(keys[i]) < 0)
                                {
                                    if (mainForm.hotkeyDown.KeyCode != Keys.None)
                                    {
                                        tempKeyDown = true;
                                        keyDown = true;
                                        break;
                                    }
                                    else
                                    {
                                        mainForm.keyDown(keys[i], alt, control, shift);
                                        tempKeyDown = true;
                                        keyDown = true;
                                        break;
                                    }
                                }
                            }
                        }

                        if (!tempKeyDown)
                        {
                            keyDown = false;
                            mainForm.hotkeyDown = new Hotkey();
                        }

                        if (!keyDown && mainForm.hotkeyDown.KeyCode != Keys.None)
                        {
                            // Do nothing for some reason
                        }
                    }
                    else
                    {
                        //mainForm.hotkeyDown = null;
                        //mainForm.mainText = "IF YOU SEE THIS, HOTKEYS BROKE SOMEHOW";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        public void stop()
        {
            interrupt = true;
        }
    }
}
