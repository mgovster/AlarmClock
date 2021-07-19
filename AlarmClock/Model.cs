using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// added for brushes
using System.Windows.Media;
// added for shapes
using System.Windows.Shapes;
// for observable collections
using System.Collections.ObjectModel;

// debug output
using System.Diagnostics;
// timer, sleep
using System.Threading;
using HighPrecisionTimer;
using TimeDataDLL;
// Sockets
using System.Net.Sockets;
using System.Net;

// byte data serialization
using System.Runtime.Serialization.Formatters.Binary;

// memory streams
using System.IO;

using System.Windows.Threading;

// INotifyPropertyChanged
using System.ComponentModel;

//several classes dealing with shapes
using System.Windows;

// Final Project
// Nathan Fenske and Matthew Govia

namespace AlarmClock
{
    class Model : INotifyPropertyChanged
    {
        Stopwatch stopWatch;
        

        // provide an observable collections for shapes
        public ObservableCollection<MyShape> H1Collection;
        public ObservableCollection<MyShape> H0Collection;
        public ObservableCollection<MyShape> M1Collection;
        public ObservableCollection<MyShape> M0Collection;
        public ObservableCollection<MyShape> S1Collection;
        public ObservableCollection<MyShape> S0Collection;

        private int[,] digitConfig = { { 10, 24, 10, 80 }, { 80, 14, 20, 10 }, { 80, 104, 20, 10 }, { 10, 24, 100, 80 }, { 80, 14, 110, 10 }, { 80, 104, 110, 10 }, { 10, 24, 190, 80 } };
        private bool[] zeroConfig = new bool[] { true, true, true, false, true, true, true };
        private bool[] oneConfig = new bool[] { false, false, true, false, false, true, false };
        private bool[] twoConfig = new bool[] { true, false, true, true, true, false, true };
        private bool[] threeConfig = new bool[] { true, false, true, true, false, true, true };
        private bool[] fourConfig = new bool[] { false, true, true, true, false, true, false };
        private bool[] fiveConfig = new bool[] { true, true, false, true, false, true, true };
        private bool[] sixConfig = new bool[] { true, true, false, true, true, true, true };
        private bool[] sevenConfig = new bool[] { true, false, true, false, false, true, false };
        private bool[] eightConfig = new bool[] { true, true, true, true, true, true, true };
        private bool[] nineConfig = new bool[] { true, true, true, true, false, true, false };

        private int totalSec = 0;
        private int hrs = Convert.ToInt32(DateTime.Now.ToString("HH:mm:ss tt").Substring(0, 2));
        private int min = Convert.ToInt32(DateTime.Now.ToString("HH:mm:ss tt").Substring(3, 2));
        private int sec = Convert.ToInt32(DateTime.Now.ToString("HH:mm:ss tt").Substring(6, 2));
        private int[] alarm = new int[] { 0, 0, 10 };
        int alarmShowing = 0;
        private bool militaryTime = true;

        // some data that keeps track of ports and addresses
        private static int _localPort = 5000;
        private static string _localIPAddress = "192.168.1.194";
        // this is the thread that will run in the background
        // waiting for incomming data
        private Thread _receiveDataThread;

        // this is the UDP socket that will be used to communicate
        // over the network
        UdpClient _dataSocket;

        public Model()
        {
            _dataSocket = new UdpClient(_localPort);

            // start the thread to listen for data from other UDP peer
            ThreadStart threadFunction = new ThreadStart(ReceiveThreadFunction);
            _receiveDataThread = new Thread(threadFunction);
            _receiveDataThread.Start();
        }
        public void CleanUp()
        {
            // if we don't close the socket and abort the thread, 
            // the applicatoin will not close properly
            if (_dataSocket != null) _dataSocket.Close();
            if (_receiveDataThread != null) _receiveDataThread.Abort();
        }
        // this is the thread that waits for incoming messages
        private void ReceiveThreadFunction()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                try
                {
                    // wait for data
                    Byte[] receiveData = _dataSocket.Receive(ref endPoint);

                    // check to see if this is synchronization data 
                    // ignore it. we should not recieve any sychronization
                    // data here, because synchronization data should have 
                    // been consumed by the SynchWithOtherPlayer thread. but, 
                    // it is possible to get 1 last synchronization byte, which we
                    // want to ignore
                    if (receiveData.Length < 2)
                        continue;


                    // process and display data


                    TimeDataDLL.TimeData.StructTimeData timeData;
                    BinaryFormatter formatter = new BinaryFormatter();
                    MemoryStream stream = new MemoryStream();

                    // deserialize data back into our GameData structure
                    stream = new System.IO.MemoryStream(receiveData);
                    timeData = (TimeDataDLL.TimeData.StructTimeData)formatter.Deserialize(stream);

                    // update view data through our bound properties
                    Console.WriteLine(timeData.hour.ToString()) ;
                    Console.WriteLine(timeData.minute.ToString());
                    Console.WriteLine(timeData.second.ToString());
                    Console.WriteLine(timeData.is24HourTime.ToString());
                    Console.WriteLine(timeData.isAlarmTime.ToString());
                    militaryTime = timeData.is24HourTime;
                    if(timeData.isAlarmTime == true)
                    {
                        if (!timeData.is24HourTime && timeData.hour > 12)
                        {
                            alarm[0] = timeData.hour - 12;
                        } else
                        {
                            alarm[0] = timeData.hour;
                        }
                        
                        alarm[1] = timeData.minute;
                        alarm[2] = timeData.second;
                    } else
                    {
                        if (!militaryTime && timeData.hour > 12)
                        {
                            TimeTM = "PM";
                            hrs = timeData.hour - 12;
                        } else if (!militaryTime)
                        {
                            TimeTM = "AM";
                            hrs = timeData.hour;
                        } else
                        {
                            TimeTM = "";
                            hrs = timeData.hour;
                        }
                        min = timeData.minute;
                        sec = timeData.second;
                        NETDispatchTimerTotalTime = timeData.second * 1000;
                        setTime(hrs, min, sec);
                    }


                }
                catch (SocketException ex)
                {
                    // got here because either the Receive failed, or more
                    // or more likely the socket was destroyed by 
                    // exiting from the JoystickPositionWindow form
                    Console.WriteLine(ex.ToString());
                    return;
                }
                catch (Exception ex)
                {
                    Console.Write(ex.ToString());
                }

            }
        }
        public void InitModel()
        {
            if (militaryTime)
            {
                TimeTM = "";
            }
            stopWatch = new Stopwatch();
            stopWatch.Start();

            // create observable collections
            H1Collection = new ObservableCollection<MyShape>();
            H0Collection = new ObservableCollection<MyShape>();
            M1Collection = new ObservableCollection<MyShape>();
            M0Collection = new ObservableCollection<MyShape>();
            S1Collection = new ObservableCollection<MyShape>();
            S0Collection = new ObservableCollection<MyShape>();

            createDigits(H1Collection);
            createDigits(H0Collection);
            createDigits(M1Collection);
            createDigits(M0Collection);
            createDigits(S1Collection);
            createDigits(S0Collection);
        }




        private void createDigits(ObservableCollection<MyShape> collection)
        {
            
            for (int i = 0; i < 7; i++)
            {
                MyShape s = new MyShape();
                Console.WriteLine(digitConfig[i,0]);
                s.CanvasTop = digitConfig[i, 2];
                s.CanvasLeft = digitConfig[i, 1];
                s.Height = digitConfig[i, 0];
                s.Width = digitConfig[i, 3];
                s.Fill = Brushes.Red;
                collection.Add(s);
            }
            Console.WriteLine(collection.Count);
        }

        private void setTime(int h, int m, int s)
        {
            int h1 = h / 10;
            int h0 = h % 10;
            int m1 = m / 10;
            int m0 = m % 10;
            int s1 = s / 10;
            int s0 = s % 10;
            configDigit(H1Collection, h1);
            configDigit(H0Collection, h0);
            configDigit(M1Collection, m1);
            configDigit(M0Collection, m0);
            configDigit(S1Collection, s1);
            configDigit(S0Collection, s0);
        }
        private void configDigit(ObservableCollection<MyShape> collection, int n)
        {
            bool[] config = zeroConfig;
            switch (n)
            {
                case 1:
                    config = oneConfig;
                    break;
                case 2:
                    config = twoConfig;
                    break;
                case 3:
                    config = threeConfig;
                    break;
                case 4:
                    config = fourConfig;
                    break;
                case 5:
                    config = fiveConfig;
                    break;
                case 6:
                    config = sixConfig;
                    break;
                case 7:
                    config = sevenConfig;
                    break;
                case 8:
                    config = eightConfig;
                    break;
                case 9:
                    config = nineConfig;
                    break;
                default:
                    config = zeroConfig;
                    break;
            }
            for(int i = 0; i < 7; i++)
            {
                if(config[i] == true)
                {
                    collection[i].Fill = Brushes.Red;
                } else
                {
                    collection[i].Fill = Brushes.Black;
                }
            }
                    
        }

        public void StopTimer()
        {
            NETDispatchTimerStart(false);
        }

        bool _netDispatchTimerRunning = false;

        uint NETDispatchTimerTicks = 0;
        long NETDispatchTimerTotalTime = 0;

        long NETDispatchTimerPreviousTime;

        DispatcherTimer dotNetDispatchTimer;

        public bool NETDispatchTimerStart(bool startStop)
        {
            if (startStop == true)
            {
                // reset counters for timing
                NETDispatchTimerTicks = 0;
                NETDispatchTimerTotalTime = sec * 1000;
                NETDispatchTimerPreviousTime = stopWatch.ElapsedMilliseconds;

                // set timer interval from GUI and start timer
                dotNetDispatchTimer = new DispatcherTimer();
                dotNetDispatchTimer.Tick += new EventHandler(NETDispatchTimerOnTick);
                dotNetDispatchTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000);
                dotNetDispatchTimer.Start();

                _netDispatchTimerRunning = true;
            }
            else if (_netDispatchTimerRunning == true)
            {
                // stop timer
                dotNetDispatchTimer.Stop();
            }

            return true;
        }

        public delegate void UpdateTextCallback(string message);

        private void UpdateText(string message)
        {
            Console.WriteLine(message);
        }
        public void NETDispatchTimerOnTick(object obj, EventArgs ea)
        {
            // add time elapsed since previous callback to our total time
            NETDispatchTimerTotalTime += stopWatch.ElapsedMilliseconds - NETDispatchTimerPreviousTime;


            
            // resent previous time to current time
            NETDispatchTimerPreviousTime = stopWatch.ElapsedMilliseconds;

            // increment the number of times the callback was called over the time period
            NETDispatchTimerTicks++;


            try
            {
                //Console.WriteLine(NETDispatchTimerTotalTime.ToString());
                dotNetDispatchTimer.Dispatcher.Invoke(() =>
                {
                    if (NETDispatchTimerTotalTime < 1000)
                    {
                        
                    }
                    else
                    {
                        if(alarmShowing > 0)
                        {
                            alarmShowing -= 1;
                            if(alarmShowing == 0)
                            {
                                AlarmText = "";
                            }
                        }
                        string temp = NETDispatchTimerTotalTime.ToString();
                        Console.WriteLine(temp.Substring(0, temp.Length - 3));
                        totalSec = Convert.ToInt32(temp.Substring(0, temp.Length - 3));
                        if(totalSec >= 60)
                        {
                            totalSec -= 60;
                            totalSec = 0;
                            NETDispatchTimerTotalTime = 0;
                            min += 1;
                            if (min >= 60)
                            {
                                min = 0;
                                hrs += 1;
                                if((!militaryTime && hrs > 12) || (militaryTime && hrs > 24))
                                {
                                    hrs = 0;
                                    if (!militaryTime)
                                    {
                                        if (TimeTM == "AM")
                                        {
                                            TimeTM = "PM";
                                        }
                                        else
                                        {
                                            TimeTM = "AM";
                                        }
                                    }
                                }
                            }
                        }
                        sec = totalSec;
                        if(hrs == alarm[0] && min == alarm[1] && sec == alarm[2])
                        {
                            AlarmText = "!ALARM!";
                            alarmShowing = 10;
                        }
                        setTime(hrs, min, sec);
                            
                    }

                });

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private string _timeTM = "";
        public string TimeTM
        {
            get { return _timeTM; }
            set
            {
                _timeTM = value;
                OnPropertyChanged("TimeTM");
            }
        }

        private string _alarmText = "";
        public string AlarmText
        {
            get { return _alarmText; }
            set
            {
                _alarmText = value;
                OnPropertyChanged("AlarmText");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
