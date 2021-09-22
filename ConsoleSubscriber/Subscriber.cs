using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Client.Subscribing;
using MQTTnet.Protocol;
using System;
using System.Text;
using System.Threading;

namespace ConsoleSubscriber
{
    public class Subscriber
    {
        private static IMqttClient _client;
        private static MqttClientOptionsBuilder _options;

        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder().AddJsonFile($"appsettings.json", true, true);
            var config = builder.Build();
            int port = Convert.ToInt32(config["Port"]);
            Subscriber program = new Subscriber();
            program.ConnectSubscriber(port);
        }

        public void ConnectSubscriber(int port)
        {
            try
            {
                Console.WriteLine("----Starting Subsriber----");
                //Create a new MQTT Client
                _client = CreateClient();
                //configure options
                _options = BuildClientOptions(port);

                //handlers
                _client = ConnectHandler();
                
                //actually connect
                _client.ConnectAsync(_options.Build()).Wait();
                Console.WriteLine("----Subsriber is connected----");
                Console.WriteLine("Press key to publish message.");
                string val =Console.ReadLine();
                SimulatePublish(_client, val);
                //Console.WriteLine("Press key to exit");
                Console.WriteLine("Simulation ended! press any key to exit.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public IMqttClient CreateClient()
        {
            var factory = new MqttFactory();
            return factory.CreateMqttClient();
        }

        public MqttClientOptionsBuilder BuildClientOptions(int port)
        {
            return new MqttClientOptionsBuilder()
                                .WithClientId("Subsciber")
                                .WithTcpServer("localhost", port: port)
                                .WithCredentials(username: "Rahul", password: "Wipro123")
                                .WithCleanSession();
        }

        public IMqttClient ConnectHandler(IMqttClient mqttClient= null)
        {
            string message = string.Empty;
            _client = _client == null ? mqttClient : _client;
            _client.UseConnectedHandler(e =>
            {
                Console.WriteLine("Connected successfully with MQTT Brokers.");

                //Subscribe to topic
                _client.SubscribeAsync(new TopicFilterBuilder().WithTopic("test").Build()).Wait();
            });
            _client.UseDisconnectedHandler(e =>
            {
                Console.WriteLine("Disconnected from MQTT Brokers.");
            });
            _client.UseApplicationMessageReceivedHandler(e =>
            {
                message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                Console.WriteLine();
            });
            return _client;
        }
        public MqttClientSubscribeOptions BuildSubscribeOptions(string topic)
        {
            return new MqttClientSubscribeOptionsBuilder()
                            .WithTopicFilter(topic, MqttQualityOfServiceLevel.ExactlyOnce)
                            .Build();
        }

        public MqttApplicationMessage SimulatePublish(IMqttClient mqttClient = null, string val = null)
        {
            _client = _client == null ? mqttClient : _client;
            var testMessage = new MqttApplicationMessageBuilder()
                .WithTopic("test")
                //.WithPayload($"Payload: {counter}  Hello Rahul " + val)
                //.WithPayload($"Payload: {val}")
                .WithPayload(val)
                .WithExactlyOnceQoS()
                .WithRetainFlag()
                .WithUserProperty(name: "Rahul123", value: "Wipro")
                //.WithContentType(contentType: "Json")
                .Build();

            if (_client.IsConnected)
            {
                //Console.WriteLine($"publishing at {DateTime.UtcNow}");
                _client.PublishAsync(testMessage);
            }
            Thread.Sleep(millisecondsTimeout: 2000);
            return testMessage;
        }
    }
}
