using System;
using System.Net.Sockets;
using System.Text;

namespace EchoServer
{
    public class ClientObject
    {
        public TcpClient client;
        public ClientObject(TcpClient tcpClient)
        {
            client = tcpClient;
        }

        public void Process()
        {
            NetworkStream stream = null;
            try
            {
                stream = client.GetStream();
                byte[] data = new byte[64]; // буфер для получаемых данных
                    while (true)
                    {
                        try
                        {
                        // получаем сообщение
                        StringBuilder builder = new StringBuilder();
                        int bytes = 0;

                        do
                        {
                            bytes = stream.Read(data, 0, data.Length);
                            builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                        } while (stream.DataAvailable);
                        if (bytes != 0)
                        {
                            string message = builder.ToString();
                            Console.WriteLine(message);
                            // отправляем обратно сообщение
                            message = message.Substring(message.IndexOf(':') + 1).Trim();
                            data = Encoding.Unicode.GetBytes(message);
                            stream.Write(data, 0, data.Length);
                        }
                        else
                        {
                            break;
                        }
                        }
                        catch
                        {
                            break;
                        }
                    }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.WriteLine("Клиент отключился");
                if (stream != null)
                    stream.Close();
                if (client != null)
                    client.Close();
            }
        }
    }
}