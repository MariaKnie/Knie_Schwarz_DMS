namespace ASP_Rest_API.Services
{
    public interface IMessageQueueService
    {
        void SendToQueue(string message);
    }
}
