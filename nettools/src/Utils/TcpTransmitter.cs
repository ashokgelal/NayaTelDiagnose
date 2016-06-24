using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace nettools.Utils
{

    internal sealed class TcpTransmitter
    {

        private TcpClient client;
        private Stream baseStream;
        private StreamWriter streamWriter;
        private StreamReader streamReader;

        public TcpClient Client
        {
            get
            {
                return this.client;
            }
        }

        public Stream BaseStream
        {
            get
            {
                return this.baseStream;
            }
        }

        public StreamWriter Writer
        {
            get
            {
                return this.streamWriter;
            }
        }

        public StreamReader Reader
        {
            get
            {
                return this.streamReader;
            }
        }

        public TcpTransmitter(string host, short port)
        {
            this.StartPreConnectionWorking();
            client.Connect(host, port);
            this.StartPostConnectionWorking();
        }

        public TcpTransmitter(IPEndPoint endPoint)
        {
            this.StartPreConnectionWorking();
            client.Connect(endPoint);
            this.StartPostConnectionWorking();
        }
        
        private void StartPreConnectionWorking()
        {
            client = new TcpClient();
        }

        private void StartPostConnectionWorking()
        {
            baseStream = client.GetStream();
            streamWriter = new StreamWriter(baseStream);
            streamReader = new StreamReader(baseStream);
        }

        public void Write(string text)
        {
            this.Writer.Write(text);
            this.Writer.Flush();
        }

        public void WriteLine(string text, string newLine = "\r\n")
        {
            this.Writer.Write(text + newLine);
            this.Writer.Flush();
        }

        public void WaitForAvailbility()
        {
            while (this.client.Available == 0);
        }

        public string ReadToEnd()
        {
            string read = "";

            while (!this.streamReader.EndOfStream)
                read += (char)this.streamReader.Read();

            return read;
        }

    }

}
