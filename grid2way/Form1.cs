using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace grid2way
{
    public partial class Form1 : Form
    {
        [DllImport("kernel32.dll")]
        private extern static IntPtr LoadLibrary(String DllName);

        [DllImport("kernel32.dll")]
        private extern static IntPtr GetProcAddress(IntPtr hModule, String ProcName);

        [DllImport("kernel32.dll")]
        private extern static bool FreeLibrary(IntPtr hModule);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool InitializeWinIoType();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void SetPortValType(UInt16 PortAddr, UInt32 PortVal, UInt16 Size);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool ShutdownWinIoType();

        IntPtr hMod;

        UInt16 address = 16376; //Port address of Lenovo T530 for PCI Parallel Port, check EEG Based BCI Experiments Manual for further info

        int trial = 100;        // total number of trials
        int keyLimit = 2;       // 2 sec wait after press
        int keyCounter = 0; 
        int showLimit = 2;      // 2 sec show selected response  - timer2
        int showCounter = 0;
        int keyEvent = 0;       // which direction
        int position = 0;       // current position check

        public Form1()
        {
            InitializeComponent();

            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(Form1_KeyDown); 

            var height = Screen.PrimaryScreen.Bounds.Height;
            var width = Screen.PrimaryScreen.Bounds.Width;
            
            pictureBox1.Height = height;
            pictureBox3.Height = height / 10;
            pictureBox2.Height = height / 10;
            pictureBox4.Height = height / 10;

            pictureBox5.Height = height / 10;
            pictureBox5.Width = width / 10;

            pictureBox1.Width = width;
            pictureBox3.Width = width / 10;
            pictureBox2.Width = width / 10;
            pictureBox4.Width = width / 10;

            pictureBox3.Location = new Point((width / 2)-80, (height / 2) - 50);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Check pointer size to determine dll type
            if (IntPtr.Size == 4)
            {
                hMod = LoadLibrary("WinIo32.dll");
            }
            else if (IntPtr.Size == 8)
            {
                hMod = LoadLibrary("WinIo64.dll");
            }
            //Error Message
            if (hMod == IntPtr.Zero)
            {
                MessageBox.Show("Can't find WinIo dll");
                this.Close();
            }
            IntPtr pFunc = GetProcAddress(hMod, "InitializeWinIo");
            if (pFunc != IntPtr.Zero)
            {
                InitializeWinIoType InitializeWinIo = (InitializeWinIoType)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(InitializeWinIoType));
                bool Result = InitializeWinIo();
                if (!Result)
                {
                    MessageBox.Show("Error returned from InitializeWinIo.\nMake sure you are running with administrative privileges and that the WinIo library files are located in the same directory as your executable file.");
                    FreeLibrary(hMod);
                    this.Close();
                }
            }

    //        label1.Text = "TRIAL: " + trial;

            IntPtr send = GetProcAddress(hMod, "SetPortVal");
            SetPortValType SetPortVal = (SetPortValType)Marshal.GetDelegateForFunctionPointer(send, typeof(SetPortValType));
            SetPortVal(address, 50, 1);
        }

        void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            var height = Screen.PrimaryScreen.Bounds.Height;
            var width = Screen.PrimaryScreen.Bounds.Width;
            
            var x = (width / 2) - 80;
            var y = (height / 2) - 50;
        
            if (e.KeyCode == Keys.Left)
            {
                IntPtr send = GetProcAddress(hMod, "SetPortVal");
                SetPortValType SetPortVal = (SetPortValType)Marshal.GetDelegateForFunctionPointer(send, typeof(SetPortValType));
                SetPortVal(address, 100, 1);

                keyEvent = 0; // left
                timer1.Start();

            }
            if (e.KeyCode == Keys.Right)
            {
                IntPtr send = GetProcAddress(hMod, "SetPortVal");
                SetPortValType SetPortVal = (SetPortValType)Marshal.GetDelegateForFunctionPointer(send, typeof(SetPortValType));
                SetPortVal(address, 150, 1);

                keyEvent = 1; // right 
                timer1.Start();
            }
  
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            keyCounter = keyCounter + 1;

            if (keyCounter >= keyLimit)
            {
                timer1.Stop();
                keyCounter = 0;

                Random rnd = new Random();
                int result = rnd.Next(10);
                trial--;

                if (keyEvent == 0) // left 
                {
                    if (result >= 2) // CORRECT DIRECTION 
                    {
                        position--;
                        pictureBox3.Left -= 180;
                  //      pictureBox4.Visible = true;
                  //      pictureBox4.Location = pictureBox3.Location;

                              pictureBox5.Visible = true;
                             pictureBox5.Location = pictureBox3.Location;

                             IntPtr send = GetProcAddress(hMod, "SetPortVal");
                             SetPortValType SetPortVal = (SetPortValType)Marshal.GetDelegateForFunctionPointer(send, typeof(SetPortValType));
                             SetPortVal(address, 120, 1);
                    }

                    else // WRONG DIRECTION 
                    {
                        position++;
                        pictureBox3.Left += 180;
                      //  pictureBox2.Visible = true;
                    //    pictureBox2.Location = pictureBox3.Location;

                        pictureBox5.Visible = true;
                        pictureBox5.Location = pictureBox3.Location;
                        IntPtr send = GetProcAddress(hMod, "SetPortVal");
                        SetPortValType SetPortVal = (SetPortValType)Marshal.GetDelegateForFunctionPointer(send, typeof(SetPortValType));
                        SetPortVal(address, 180, 1);
                    }
                }

                else if (keyEvent == 1) //right
                {
                    if (result >= 2) // CORRECT DIRECTION 
                    {
                        pictureBox3.Left += 180;
                        position++;
                   //     pictureBox4.Visible = true;
                   //     pictureBox4.Location = pictureBox3.Location;

                        pictureBox5.Visible = true;
                        pictureBox5.Location = pictureBox3.Location;

                        IntPtr send = GetProcAddress(hMod, "SetPortVal");
                        SetPortValType SetPortVal = (SetPortValType)Marshal.GetDelegateForFunctionPointer(send, typeof(SetPortValType));
                        SetPortVal(address, 120, 1);
                      
                    }
                    else // WRONG DIRECTION 
                    {
                        pictureBox3.Left -= 180;
                        position--;
                    //    pictureBox2.Visible = true;
                   //     pictureBox2.Location = pictureBox3.Location;

                        pictureBox5.Visible = true;
                        pictureBox5.Location = pictureBox3.Location;

                        IntPtr send = GetProcAddress(hMod, "SetPortVal");
                        SetPortValType SetPortVal = (SetPortValType)Marshal.GetDelegateForFunctionPointer(send, typeof(SetPortValType));
                        SetPortVal(address, 180, 1);
                    }
                }

                timer2.Start();
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            showCounter = showCounter + 1;
            var height = Screen.PrimaryScreen.Bounds.Height;
            var width = Screen.PrimaryScreen.Bounds.Width;

            if (showCounter >= showLimit)
            {
                timer2.Stop();
            //    pictureBox2.Visible = false;
             //   pictureBox4.Visible = false;
                pictureBox5.Visible = false;
       
                showCounter = 0;

                if (position <= -4 || position >= 4)
                {
                    pictureBox3.Location = new Point((width / 2) - 80, (height / 2) - 50);
                    position = 0;
                }

            //    label1.Text = "TRIAL: " + trial;

                IntPtr send = GetProcAddress(hMod, "SetPortVal");
                SetPortValType SetPortVal = (SetPortValType)Marshal.GetDelegateForFunctionPointer(send, typeof(SetPortValType));
                SetPortVal(address, 50, 1);

                if (trial <= 0)
                {
                    SetPortVal(address, 250, 1);
                    Application.Exit();
                }
            }
        }    
    }
}
