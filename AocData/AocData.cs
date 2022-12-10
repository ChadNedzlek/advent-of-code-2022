// See https://aka.ms/new-console-template for more information

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Cookie = System.Net.Cookie;

namespace ChadNedzlek.AdventOfCode.DataModule
{
    public class AocData
    {
        private static readonly string RootFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ChadNedzlek",
            "AocData");
        
        public async Task<StreamReader> GetDataAsync(int year, int day)
        {
            string dir = Path.Combine(RootFolder, year.ToString());
            Directory.CreateDirectory(dir);
            string targetPath = Path.Combine(dir, $"day-{day:D2}-input.txt");
            if (!File.Exists(targetPath))
            {
                string token = await GetAccessTokenAsync(year);
                var cookies = new CookieContainer();
                using var http = new HttpClient(new HttpClientHandler { CookieContainer = cookies });
                if (token != null)
                {
                    cookies.Add(new Cookie("session", token, null, "adventofcode.com"){Secure = true});
                }

                await using var httpStream =
                    await http.GetStreamAsync($"https://adventofcode.com/{year}/day/{day}/input");
                await using var cacheStream = File.Create(targetPath);
                await httpStream.CopyToAsync(cacheStream);
            }

            return File.OpenText(targetPath);
        }

        private async Task<string> GetAccessTokenAsync(int year)
        {
            var metadataPath = Path.Combine(RootFolder, year.ToString(), "metadata.json");
            Metadata metadata = new Metadata();
            if (File.Exists(metadataPath))
            {
                try
                {
                    await using var stream = File.OpenRead(metadataPath);
                    metadata = await JsonSerializer.DeserializeAsync<Metadata>(stream);
                }
                catch
                {
                    // Whatever, we'll make new data
                }
            }

            string returnValue = null;
            if (!string.IsNullOrEmpty(metadata.AccessToken))
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    returnValue = Encoding.UTF8.GetString(ProtectedData.Unprotect(
                        Convert.FromBase64String(metadata.AccessToken), null, DataProtectionScope.CurrentUser));
                }
                else
                {
                    returnValue = metadata.AccessToken;
                }
            }
            else
            {
                async Task<string> TryGetCookie(IPage p)
                {
                    var all = await p.Context.CookiesAsync();
                    foreach (var cookie in all)
                    {
                        if (cookie.Name == "session")
                        {
                            return cookie.Value;
                        }
                    }

                    return null;
                }

                Program.Main(new[] { "install" });
                var pl = await Playwright.CreateAsync();
                var ch = await pl.Chromium.LaunchAsync(new BrowserTypeLaunchOptions{Headless = false});
                var page = await ch.NewPageAsync();
                TaskCompletionSource<bool> closed = new TaskCompletionSource<bool>();
                var cTask = closed.Task;
                page.Close += (_, _) => closed.TrySetResult(true);
                await page.GotoAsync("https://adventofcode.com/2022/auth/login");
                string cookie = await TryGetCookie(page);
                while (cookie == null)
                {
                    var completed = await Task.WhenAny(cTask, page.WaitForNavigationAsync(new PageWaitForNavigationOptions{Timeout = 0}));
                    if (completed == cTask)
                        return null;
                    
                    if (page.Url.StartsWith("https://adventofcode.com/"))
                    {
                        cookie = await TryGetCookie(page);
                    }
                }

                returnValue = cookie;
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    metadata.AccessToken = Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(cookie),
                        null, DataProtectionScope.CurrentUser));
                }
                else
                {
                    metadata.AccessToken = cookie;
                }
                
                try
                {
                    await using var stream = File.Create(metadataPath);
                    await JsonSerializer.SerializeAsync(stream, metadata);
                }
                catch
                {
                    // Whatever, we'll make new data
                }
            }

            return returnValue;
        }

        private class Metadata
        {
            public string AccessToken { get; set; }
        }
    }
}