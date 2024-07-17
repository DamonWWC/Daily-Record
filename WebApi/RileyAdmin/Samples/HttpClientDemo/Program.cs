using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.Net.Http;
using System.Net.Http.Json;

namespace HttpClientDemo
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
                  
            var host = new HostBuilder()
               .ConfigureServices(services =>
               {
                   ///方法1
                   /// services.AddHttpClient();
                   /// 
                   ///方法2
                   //services.AddHttpClient("GitHub",httpClient=>
                   //{
                   //    httpClient.BaseAddress = new Uri("https://api.github.com/");

                   //    httpClient.DefaultRequestHeaders.Add(
                   //        HeaderNames.Accept, "application/vnd.github.v3+json");
                   //    httpClient.DefaultRequestHeaders.Add(
                   //        HeaderNames.UserAgent, "HttpRequestsSample");
                   //});
                   ///
                   /// services.AddTransient<GitHubService>();
                   ///方法3
                   services.AddHttpClient<GitHubService1>();                  
                   ///

                   
               })
               .Build();
          
            try
            {
                var gitHubService = host.Services.GetRequiredService<GitHubService1>();
                var gitHubBranches = await gitHubService.GetAspNetCoreDocsBranchesAsync();

                Console.WriteLine($"{gitHubBranches?.Count() ?? 0} GitHub Branches");

                if (gitHubBranches is not null)
                {
                    foreach (var gitHubBranch in gitHubBranches)
                    {
                        Console.WriteLine($"- {gitHubBranch.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                host.Services.GetRequiredService<ILogger<Program>>().LogError(ex, "Unable to load branches from GitHub.");
            }


            Console.WriteLine("Hello, World!");
        }

       
    }

    public class GitHubService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GitHubService(IHttpClientFactory httpClientFactory) =>
            _httpClientFactory = httpClientFactory;

        public async Task<IEnumerable<GitHubBranch>?> GetAspNetCoreDocsBranchesAsync()
        {

            //方法1基本方法
            //var httpRequestMessage = new HttpRequestMessage(
            //    HttpMethod.Get,
            //    "https://api.github.com/repos/dotnet/AspNetCore.Docs/branches")
            //{
            //    Headers =
            //{
            //    { "Accept", "application/vnd.github.v3+json" },
            //    { "User-Agent", "HttpRequestsConsoleSample" }
            //}
            //};

            //var httpClient = _httpClientFactory.CreateClient();


            //var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

            //httpResponseMessage.EnsureSuccessStatusCode();

            //using var contentStream =
            //    await httpResponseMessage.Content.ReadAsStreamAsync();

            //return await JsonSerializer.DeserializeAsync
            //    <IEnumerable<GitHubBranch>>(contentStream);
            ///
            //方法2命名客户端
            var httpClient = _httpClientFactory.CreateClient("GitHub");
            var httpResponseMessage = await httpClient.GetAsync("repos/dotnet/AspNetCore.Docs/branches");

            if(httpResponseMessage.IsSuccessStatusCode)
            {
                using var contentStream=await httpResponseMessage.Content.ReadAsStreamAsync();

                return await JsonSerializer.DeserializeAsync<IEnumerable<GitHubBranch>>(contentStream);
            }
            return null;
            ///
                  
        }
    }


    /// <summary>
    /// 方法3 
    /// </summary>
    /// <param name="Name"></param>
    /// 
    public class GitHubService1
    {
        private readonly HttpClient _httpClient;
        public GitHubService1(HttpClient httpClient)
        {
            _httpClient = httpClient;

            _httpClient.BaseAddress = new Uri("https://api.github.com/");

            _httpClient.DefaultRequestHeaders.Add(
          HeaderNames.Accept, "application/vnd.github.v3+json");
            _httpClient.DefaultRequestHeaders.Add(
                HeaderNames.UserAgent, "HttpRequestsSample");
        }

        public async Task<IEnumerable<GitHubBranch>?> GetAspNetCoreDocsBranchesAsync()=>
             await _httpClient.GetFromJsonAsync<IEnumerable<GitHubBranch>>(
            "repos/dotnet/AspNetCore.Docs/branches");


    }



    public record GitHubBranch(
        [property: JsonPropertyName("name")] string Name);
}