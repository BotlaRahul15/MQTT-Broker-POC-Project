using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Client.Subscribing;
using MQTTnet.Server;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsolePublisher
{
    public class Publisher
    {
        private static IMqttClient _client;
        private static MqttClientOptionsBuilder _options;
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder().AddJsonFile($"appsettings.json", true, true);
            var config = builder.Build();
            int port = Convert.ToInt32(config["Port"]);
            Publisher program = new Publisher();
            program.ConnectPublisher(port);
        }

        public void ConnectPublisher(int port,string message = null)
        {
            
            Console.WriteLine("Starting Publisher....");
            try
            {
                //Create a new MQTT Client
                _client = CreateClient();
                //configure options
                _options = BuildClientOptions(port);


                //handlers
                _client = ConnectHandler();

                //connect
                _client.ConnectAsync(_options.Build()).Wait();
                Console.WriteLine("-----Publisher is Connected----");
                Console.WriteLine("Please enter topic");
                string topic = Console.ReadLine();
                Console.WriteLine("Press key to publish message.");
                string val = string.Empty;
                if (message == null)
                {
                    val = Console.ReadLine();
                }
                else
                {
                    val = message;
                }
                //simulating publish
                var message1 = SimulatePublish(null,val, topic);
                //SimulatePublish(val);
                Console.WriteLine("Do you want to send another message please enter or else enter 0 to stop the stimulation");
                val = Console.ReadLine();
                if(val == "0")
                {
                    Console.WriteLine("Simulation ended! press any key to exit.");
                }
                else
                {
                    SimulatePublish(null, val);
                }
                
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
                                .WithClientId("Publisher")
                                .WithTcpServer("localhost", port: port)
                                .WithCredentials(username: "Rahul123", password: "Wipro")
                                .WithCleanSession();
        }
        public IMqttClient ConnectHandler()
        {
            _client.UseConnectedHandler(async e =>
            {
                Console.WriteLine("### CONNECTED WITH SERVER ###");

                // Subscribe to a topic
                //await _client.SubscribeAsync(new MqttClientSubscribeOptionsBuilder().WithTopicFilter("my/topic").Build());
                await _client.SubscribeAsync(new MqttClientSubscribeOptionsBuilder().WithTopicFilter("test").Build());

                Console.WriteLine("### SUBSCRIBED ###");
            });
            _client.UseDisconnectedHandler(async e =>
            {
                Console.WriteLine("### DISCONNECTED FROM SERVER ###");
                await Task.Delay(TimeSpan.FromSeconds(5));

                try
                {
                    await _client.ConnectAsync(_options.Build(), CancellationToken.None);
                }
                catch
                {
                    Console.WriteLine("### RECONNECTING FAILED ###");
                }
            });
            _client.UseApplicationMessageReceivedHandler(e =>
            {
                Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                Console.WriteLine();

                //Task.Run(() => _client.PublishAsync("hello/world")); //for qos>0
            });
            return _client;
        }

        //This method sends messages to topic "test"
        public MqttApplicationMessage SimulatePublish(IMqttClient mqttClient = null, string val = null, string topic = null)
        {
            _client = _client == null ? mqttClient : _client;
            var testMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                //.WithPayload($"Payload: {counter}  Hello Rahul " + val)
                //.WithPayload($"Payload: {val}")
                .WithPayload(val)
                .WithExactlyOnceQoS()
                .WithRetainFlag()
                .WithUserProperty(name: "Rahul123",value: "Wipro")
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
