using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json.Linq;


namespace MotionverseSDK
{
    public class JsonDownloader
    {
        private const string TAG = nameof(JsonDownloader);
        public int Timeout { get; set; }

        public async Task<string> Execute(string url, CancellationToken token)
        {
            try
            {
                var webRequestDispatcher = new WebRequestDispatcher();
                string text = await webRequestDispatcher.Dispatch(url, token);
                return text;
            }
            catch (CustomException exception)
            {
                SDKLogger.Log(TAG, exception + "жиЪд");
                await Task.Delay(1000);
                return await Execute(url, token);
            }
        }

        public async Task<byte[]> ExecuteBuffer(string url, CancellationToken token)
        {
            try
            {
                var webRequestDispatcher = new WebRequestDispatcher();
                byte[] data = await webRequestDispatcher.DownloadBuffer(url, token);
                return data;
            }
            catch (CustomException exception)
            {
                SDKLogger.Log(TAG, exception + "жиЪд");
                await Task.Delay(1000);
                return await ExecuteBuffer(url, token);
            }
        }
        
    }
}