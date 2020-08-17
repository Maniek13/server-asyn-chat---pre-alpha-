﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace serwer
{
  

    public partial class Serwer : Form
    {
        public Serwer()
        {
            InitializeComponent();

        }

        public void Start_Click(object sender, EventArgs e)
        {
            Thread wątek = new Thread(new ThreadStart(AsynchronousSocketListener.StartListening));
            wątek.IsBackground = true;
            wątek.Start();
        
        }



        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void Stop_Click(object sender, EventArgs e)
        {
            Obliczenia obliczenia = new Obliczenia();
            obliczenia.ZapisKont();
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
      
        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }
    }

    public class Userspasword
    {
        public string RegisteredUser { get; set; }
        public string Pasword { get; set; }
        public Userspasword(string log, string pass)
        {
            RegisteredUser = log;
            Pasword = pass;
        }
    }


    public class Obliczenia
    {
        private static List<Userspasword> Users = new List<Userspasword>();

        public string Start(string wiadomość)
        {
            Wczytano();

            string z = "";

            z = Wiadomość(wiadomość);
            if(z == "")
            {
                z = Wiadomości(wiadomość);
                if (z == "")
                {
                    z = Logowanie(wiadomość);
                    if (z == "")
                    {
                        z = Rejestracja(wiadomość);
                    }
                }
            }

            return z;

        }
        public void WczytanieKont()
        {
            string[] konta = File.ReadAllLines("F:\\pendrive\\komunikator\\serwerasyn\\dane\\users.txt"); //nazwa + $ + hasło

            foreach (string line in konta)
            {
                int z = line.IndexOf("$");
                string login = line.Substring(0, z);
                string password = line.Substring(z + 1);
                Users.Add(new Userspasword(login, password));

            }

        }
        public void Wczytano()
        {

            if (Users.Count() == 0)
            {
                WczytanieKont();
            }

        }
        public string Logowanie(string msg)
        {
            //"LOG"+ Login.Text + "$" + hasło.Text;
            if (msg.StartsWith("LOG"))
            {
                string temp = msg.Substring(3);
                int t = temp.IndexOf("$");
                string password = temp.Substring(t + 1);
                string login = temp.Substring(0, t);

                if (Users.Exists(x => x.RegisteredUser == login && x.Pasword == password))
                {
                    return "Ok";

                }
                else
                {
                    return "Nie istnieje";

                }

            }
            return "";
        }
        public string Rejestracja(string msg)
        {
            //"REJ"Login.Text + "$" + hasło.Text;
            if (msg.StartsWith("REJ"))
            {
                string temp = msg.Substring(3);
                int t = temp.IndexOf("$");
                string password = temp.Substring(t + 1);
                string login = temp.Substring(0, t);



                if (Users.Exists(x => x.RegisteredUser == login && x.Pasword == password))
                {
                    return "Istnieje";
                }
                else
                {
                    Users.Add(new Userspasword(login, password));

                    return "Ok";
                }

            }
            else
            {
                return "";
            }
        }
        public string Wiadomość(string msg)
        {
            //"Wiadomosc od:" + login + "#" + wiadomość + "%" + osoba;

            if (msg.StartsWith("Wiadomosc od:"))
            {
                
                string temp = msg.Substring(13);

                int z = temp.IndexOf("#");

                string login = temp.Substring(0, z);

                temp = temp.Substring(z + 1);
                z = temp.LastIndexOf("%");
                string wiadomość = temp.Substring(0, z);
                string adresat = temp.Substring(z + 1);


                FileStream plik = new FileStream("F:\\pendrive\\komunikator\\serwerasyn\\dane\\wiadomości_" + adresat + ".txt", FileMode.OpenOrCreate);
                StreamWriter f = new StreamWriter(plik);

                f.WriteLine(login + "$" + wiadomość);
                f.Close();

                return login + " do " + adresat + ": " + wiadomość;

            }
            else
            {
                return "";
            }

            
          

        }
        public string Wiadomości(string msg)
        {

            if (msg.StartsWith("Wyswietl wiadomosci"))
            {

                String data = DateTime.Now.ToString();
                return "xxx: " + data;

            }
            else
            {
                return "";
            }
        }
        public void ZapisKont()
        {
            File.Delete(@"F:\\pendrive\\komunikator\\serwerasyn\\dane\\users.txt");

            FileStream plik = new FileStream(@"F:\\pendrive\\komunikator\\serwerasyn\\dane\\users.txt", FileMode.CreateNew);
            StreamWriter f = new StreamWriter(plik);

            while (Users.Count != 0)
            {
                f.WriteLine(Users.First().RegisteredUser + "$" + Users.First().Pasword);
                Users.RemoveAt(0);
            }

            f.Close();



        }
    }

    public class StateObject
    {
        public Socket workSocket = null;
        public const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
        
    }


    public class AsynchronousSocketListener
    {
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public AsynchronousSocketListener()
        {
        }

        public static void StartListening()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            Serwer msg = new Serwer();

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    allDone.Reset();
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);
                    allDone.WaitOne();
                }

            }
            catch (Exception p)
            {
                Console.WriteLine("Error..... " + p.StackTrace);
            }
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            allDone.Set();
            
             Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                content = state.sb.ToString();

                Obliczenia obliczenia = new Obliczenia();
                content = obliczenia.Start(content);

                Send(handler, content);

            }
        }

        private static void Send(Socket handler, String data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;
                int bytesSent = handler.EndSend(ar);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception p)
            {
                Console.WriteLine("Error..... " + p.StackTrace);
            }
        }

    }

}





