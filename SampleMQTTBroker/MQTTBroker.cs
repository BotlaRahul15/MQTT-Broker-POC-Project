using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Server;
using System;
using System.IO;

namespace SampleMQTTBroker
{
    public class MQTTBroker
    {
        
        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder().AddJsonFile($"appsettings.json", true, true);
            var config = builder.Build();
            int port = Convert.ToInt32(config["Port"]);
            MQTTBroker program = new MQTTBroker();
            program.ConnectMQTTBroker(port);
        }

        public string ConnectMQTTBroker(int port)
         {
            Console.WriteLine("----Connecting Broker----");
            //configure options
            var optionsBuilder = BuildServerOptions(port);

            // creates a new mqtt server     
            IMqttServer mqttServer = CreateServer();
            mqttServer.StartAsync(optionsBuilder.Build()).Wait();
            Console.WriteLine($"Broker is Connected: Host: {mqttServer.Options.DefaultEndpointOptions.BoundInterNetworkAddress} Port: {mqttServer.Options.DefaultEndpointOptions.Port}");
            
            Console.ReadLine();
            return "success";
        }
        public IMqttServer CreateServer()
        {
            return new MqttFactory().CreateMqttServer();
        }

        public MqttServerOptionsBuilder BuildServerOptions(int port)
        {
            return new MqttServerOptionsBuilder()
                                        // set endpoint to localhost
                                        .WithDefaultEndpoint()
                                        // port used will be 1883
                                        .WithDefaultEndpointPort(port);
        }
    }
}
