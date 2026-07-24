using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SquareRodIntegration
{
    public sealed class SquareRodApiClient : IDisposable
    {
        private readonly HttpClient _httpClient;

        public SquareRodApiClient(string baseAddress = "http://127.0.0.1:8780/")
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseAddress),
                Timeout = TimeSpan.FromSeconds(5)
            };
        }

        public async Task EnsureServiceRunningAsync(
            string serviceExecutable,
            string serviceConfig,
            TimeSpan startupTimeout,
            CancellationToken cancellationToken)
        {
            if (await WaitUntilReadyAsync(TimeSpan.FromSeconds(2), cancellationToken))
            {
                return;
            }

            string executable = Path.GetFullPath(serviceExecutable);
            string config = Path.GetFullPath(serviceConfig);
            if (!File.Exists(executable))
            {
                throw new FileNotFoundException("算法服务程序不存在", executable);
            }
            if (!File.Exists(config))
            {
                throw new FileNotFoundException("算法服务配置不存在", config);
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = executable,
                Arguments = "--config \"" + config + "\"",
                WorkingDirectory = Path.GetDirectoryName(executable),
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            try
            {
                Process.Start(startInfo);
            }
            catch (Exception startError)
            {
                throw new InvalidOperationException(
                    "算法服务未运行，且自动启动失败", startError);
            }

            if (!await WaitUntilReadyAsync(startupTimeout, cancellationToken))
            {
                throw new TimeoutException(
                    "算法服务已启动，但未在规定时间内进入就绪状态");
            }
        }

        public async Task<bool> WaitUntilReadyAsync(
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            DateTime deadline = DateTime.UtcNow + timeout;
            while (DateTime.UtcNow < deadline)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    using (HttpResponseMessage response =
                        await _httpClient.GetAsync("api/v1/ready", cancellationToken))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            JObject payload = JObject.Parse(
                                await response.Content.ReadAsStringAsync());
                            if (payload.Value<bool>("ready"))
                            {
                                return true;
                            }
                        }
                    }
                }
                catch (HttpRequestException)
                {
                    // The Windows service may still be starting.
                }
                await Task.Delay(500, cancellationToken);
            }
            return false;
        }

        public async Task<string> SubmitTiffSetAsync(
            string requestId,
            string captureId,
            string specimen,
            string leftPath,
            string rightPath,
            string topPath,
            string downPath,
            CancellationToken cancellationToken)
        {
            var request = new
            {
                request_id = requestId,
                capture_id = captureId,
                specimen,
                input = new
                {
                    type = "tiff_set",
                    cameras = new
                    {
                        Left = leftPath,
                        Right = rightPath,
                        Top = topPath,
                        Down = downPath
                    }
                }
            };
            string json = JsonConvert.SerializeObject(request);
            using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
            using (HttpResponseMessage response = await _httpClient.PostAsync(
                "api/v1/measurements", content, cancellationToken))
            {
                string responseJson = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == (HttpStatusCode)429)
                {
                    throw new InvalidOperationException("算法队列已满：" + responseJson);
                }
                response.EnsureSuccessStatusCode();
                return JObject.Parse(responseJson).Value<string>("job_id");
            }
        }

        public async Task<JObject> WaitForResultAsync(
            string jobId,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            DateTime deadline = DateTime.UtcNow + timeout;
            while (DateTime.UtcNow < deadline)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using (HttpResponseMessage response = await _httpClient.GetAsync(
                    "api/v1/measurements/" + jobId, cancellationToken))
                {
                    response.EnsureSuccessStatusCode();
                    JObject job = JObject.Parse(
                        await response.Content.ReadAsStringAsync());
                    string status = job.Value<string>("job_status");
                    if (status == "completed" || status == "failed")
                    {
                        return job;
                    }
                }
                await Task.Delay(250, cancellationToken);
            }
            throw new TimeoutException("等待算法结果超时，任务ID：" + jobId);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
