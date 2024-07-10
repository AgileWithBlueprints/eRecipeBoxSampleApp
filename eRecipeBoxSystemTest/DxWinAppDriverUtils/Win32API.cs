/*
* MIT License
* 
* Copyright (C) 2024 SoftArc, LLC
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/
using System.Runtime.InteropServices; 
namespace DxWinAppDriverUtils 
{ 
    //push key programmatically thru win32api
    //https://stackoverflow.com/questions/2527624/is-it-possible-to-programmatically-set-the-state-of-the-shift-and-control-keys
    public class Win32API
    {        
        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        private const byte SHIFT = 0x10;
        private const byte CTRL = 0x11;
        private const byte ALT= 0x12;

        public static void LockControlKey(bool enable)
        {
            if (enable)
                keybd_event(CTRL, 0, 0, 0);
            else  
                keybd_event(CTRL, 0, 2, 0);
        }
        public static void LockShiftKey(bool enable)
        {
            if (enable) 
                //shift down 
                keybd_event(SHIFT, 0, 0, 0);
            else //shift up
                keybd_event(SHIFT, 0, 2, 0);
        }
        public static void LockAltKey(bool enable)
        {
            if (enable)
                keybd_event(ALT, 0, 0, 0);
            else  
                keybd_event(ALT, 0, 2, 0);
        }
    }
}