using RabbitMQ.Client;
using System;

namespace RabbitMqCleaner
{
    class Program
    {
        static void Main(string[] args)
        {
            ConnectionFactory factory = new ConnectionFactory();

            factory.HostName = "localhost";
            factory.UserName = "guest";
            factory.Password = "guest";

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    process.StartInfo = new System.Diagnostics.ProcessStartInfo()
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                        FileName = "cmd.exe",
                        Arguments = "/C rabbitmqctl list_queues",
                        RedirectStandardError = true,
                        RedirectStandardOutput = true
                    };
                    process.Start();
                    // Now read the value
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();
                    var counter = 0;
                    if (string.IsNullOrEmpty(output))
                    {
                        Console.WriteLine(error);
                    } else
                    {
                        string[] result = output.Split('\n');
                        string[] lines = new string[result.Length - 4];
                        Array.Copy(result, 3, lines, 0, result.Length - 4);
                        foreach (var line in lines)
                        {
                            var queue = line.Split('\t')[0];
                            try
                            {
                                channel.QueuePurge(queue);
                                Console.WriteLine($"Purging queue: {queue}");
                                counter++;
                            } catch (Exception ex)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Error while purging queue: {queue}, MESSAGE: {ex.Message}");
                                Console.ForegroundColor = ConsoleColor.White;
                            }                            
                        }                        
                    }
                    Console.WriteLine($"{counter} queues have been purged. Press ENTER to exit...");
                    Console.ReadLine();
                }
            }
        }
    }
}
