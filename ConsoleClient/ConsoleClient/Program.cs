using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using NLog;
namespace ConsoleClient
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        //Задаем значения порта/адреса по дефолту
        static int port = 0;
        static string address = "0";
        private static string[] words;
        private static TcpClient client = null;
        private static NetworkStream stream = null;
        private static string logLevel = "all";
        static void Main(string[] args)
        {
            Console.Write("Введите свое имя:");
            string userName = Console.ReadLine();
            LogLevel("Клиент ввёл свой ник: " + userName, "info");
            Console.WriteLine("Введите /help для того, чтобы просмотреть список возможных команд");
            //Рефакторинг этого дела обязателен!!!

            try
            {
                //Первый ввод, в случае отсутствия подключения к серверу
                while (true)
                {
                    Console.Write(userName + ": ");
                    string messageT = Console.ReadLine();
                    if (messageT.Equals("/help")) { HelpMessage(); LogLevel("Клиент ввёл команду /help", "debug"); }//В случае, если клиент не подключился к серверу, а просмотреть сводку решил
                    else
                    {
                        CheckSplit(messageT);
                        if (!words[0].Equals("connect")) { Console.WriteLine("Вы не подключены к серверу!"); LogLevel("Попытка ввода без подключения к серверу!", "warn"); }
                        else
                        {
                            client = new TcpClient(address, port);
                            stream = client.GetStream();
                            LogLevel("Подключение к серверу!", "info");
                            while (true)
                            {
                                Console.Write(userName + ": ");
                                // ввод сообщения
                                string message = Console.ReadLine();
                                CheckSplit(message);
                                if (message.Equals("disconnect")) { 
                                    Disconnect(); 
                                    Console.WriteLine("Подключение прервано!"); 
                                    LogLevel("Отключение от сервера!", "info");
                                    LogLevel("Клиент ввёл команду disconnect", "debug"); 
                                    break; 
                                }
                                else if (words[0].Equals("send"))
                                {
                                    LogLevel("Клиент ввёл команду send", "debug");
                                    string[] msg = message.Split(' ', 2);//разбиваем строку всего на 2 части, когда как код в методе CheckSplit не позволит этого сделать
                                    message = String.Format("{0}: {1}", userName, msg[1]);
                                    // преобразуем сообщение в массив байтов
                                    byte[] data = Encoding.Unicode.GetBytes(message);
                                    // отправка сообщения
                                    stream.Write(data, 0, data.Length);
                                    LogLevel("Клиент отправил сообщение: " + msg[1], "info");
                                    // получаем ответ
                                    data = new byte[64]; // буфер для получаемых данных
                                    StringBuilder builder = new StringBuilder();
                                    int bytes = 0;
                                    do
                                    {
                                        bytes = stream.Read(data, 0, data.Length);
                                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                                    }
                                    while (stream.DataAvailable);

                                    message = builder.ToString();
                                    Console.WriteLine("EchoServer>> {0}", message);
                                    LogLevel("Клиент получил сообщение от сервера сообщение: " + message, "info");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                LogLevel("Ошибка в подключении", "error");
            }
            finally
            {
                Disconnect();
            }
        }

        static void Disconnect()
        {
            if (stream != null)
                stream.Close();//отключение потока
            if (client != null)
                client.Close();//отключение клиента
        }


        //Метод для проверки введенной команды и дальнейшая обработка данных
        private static void CheckSplit(string word)
        {
            words = word.Split(' ');
            switch (words[0].ToLower())
            {
                case "connect":
                    {
                        address = words[1]; 
                        port = Convert.ToInt32(words[2]);
                        Console.WriteLine("Вы подключились! Данные: {0}, {1}", address, port);
                        LogLevel("Клиент ввёл команду connect", "debug");
                        break;
                    }
                case "/help": { HelpMessage(); ; break; }
                case "quit": { LogLevel("Клиент ввёл команду quit", "debug"); LogLevel("Клиент вышел из приложения!", "info"); Environment.Exit(0); break; }
                case "logLevel": { LogLevel("Клиент ввёл команду quit", "logLevel"); logLevel = words[1]; break; }
            }
        }

        //Метод с отправкой сообщения-помощника
        private static void HelpMessage()
        {
            Console.WriteLine(
                "\nДоступные команды:\n" +
                "connect <<address>> <<port>> | подключение к серверу с заданными адресом/портом;\n" +
                "disconnect                   | отключиться от сервера;\n" +
                "send <<message>>             | отправляет ваше сообщение на сервер при условии, что вы подключены;\n" +
                "logLevel <<level>>           | устанавливает уровень логирования;\n" +
                "quit                         | закрыть окно консоли;\n" +
                "/help                        | показать список возможных команд.\n" +
                "logLevel <<level>>           | устанавливает определенный уровень логирования ALL|DEBUG|INFO|WARN|ERROR|FATAL\n"
                );
        }

        private static void LogLevel(string message, string type)
        {
            switch(logLevel.ToLower())
            {
                case "all":
                    {
                        switch(type.ToLower())
                        {
                            case "info": logger.Info(message); break;
                            case "debug": logger.Debug(message); break;
                            case "warn": logger.Warn(message); break;
                            case "error": logger.Error(message); break;
                            case "fatal": logger.Fatal(message); break;
                        }
                        break;
                    }
                case "info":
                    {
                        if (type.ToLower().Equals("info")) logger.Info(message);
                        break;
                    }
                case "debug":
                    {
                        if (type.ToLower().Equals("debug")) logger.Info(message);
                        break;
                    }
                case "warn":
                    {
                        if (type.ToLower().Equals("warn")) logger.Info(message);
                        break;
                    }
                case "error":
                    {
                        if (type.ToLower().Equals("error")) logger.Info(message);
                        break;
                    }
                case "fatal":
                    {
                        if (type.ToLower().Equals("fatal")) logger.Info(message);
                        break;
                    }
            }
        }

    }
}