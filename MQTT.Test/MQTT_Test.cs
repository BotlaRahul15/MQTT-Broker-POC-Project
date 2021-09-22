using ConsolePublisher;
using ConsoleSubscriber;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using SampleMQTTBroker;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MQTT.Test
{
    public class MQTT_Test
    {

        private readonly MQTTBroker broker;
        private readonly Publisher publisher;
        private readonly Subscriber subscriber;
        int port = 1883;

        public MQTT_Test()
        {
            broker = new MQTTBroker();
            publisher = new Publisher();
            subscriber = new Subscriber();
        }

        [Fact]
        public async Task TestMQTTConnectionWithPublisher()
        {
            var brokerOptions = broker.BuildServerOptions(port);
            var brokerserver = broker.CreateServer();
            await brokerserver.StartAsync(brokerOptions.Build());
            var pubOptions = publisher.BuildClientOptions(port);
            var pubClient = publisher.CreateClient();
            await pubClient.ConnectAsync(pubOptions.Build());
            Assert.True(pubClient.IsConnected);
        }

        [Fact]
        public async Task TestMQTTConnectionWithSubscriber()
        {
            var brokerOptions = broker.BuildServerOptions(port);
            var brokerserver = broker.CreateServer();
            await brokerserver.StartAsync(brokerOptions.Build());
            var subOptions = subscriber.BuildClientOptions(port);
            var subClient = subscriber.CreateClient();
            await subClient.ConnectAsync(subOptions.Build());
            Assert.True(subClient.IsConnected);
        }

        [Fact]
        public async Task ValidateMessageDetails()
        {
            var msgCount = 0;
            MqttApplicationMessage receivedMessage = null;
            var brokerOptions = broker.BuildServerOptions(port);
            var brokerserver = broker.CreateServer();
            var pubOptions = publisher.BuildClientOptions(port);
            var pubClient = publisher.CreateClient();
            var subOptions = subscriber.BuildClientOptions(port);
            var subClient = subscriber.CreateClient();
            subClient = subscriber.ConnectHandler(subClient);
            subClient.UseApplicationMessageReceivedHandler(e => { receivedMessage = e.ApplicationMessage; msgCount++; });

            //Creating connections
            await brokerserver.StartAsync(brokerOptions.Build());
            await subClient.ConnectAsync(subOptions.Build());
            await pubClient.ConnectAsync(pubOptions.Build());
            //Publishing messages
            var message =  publisher.SimulatePublish(pubClient, "Welcome to wipro");
            publisher.SimulatePublish(pubClient, "Welcome to wipro");
            await Task.Delay(10);
            await brokerserver.StopAsync();

            //Assert
            Assert.NotNull(receivedMessage);
            Assert.Equal("test", receivedMessage.Topic);
            Assert.Equal("Welcome to wipro", Encoding.UTF8.GetString(receivedMessage.Payload));
            Assert.Equal(2, msgCount);
        }
    }
}
