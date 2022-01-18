using NeoSmart.Utils;
using System.Drawing;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using WebSocketSharp;

namespace WaifuLabsGrabber
{
    class Program
    {
        public class waifus
        {
            public List<string> waifuSeeds = new List<string>();
            public string lastSeed;
        }

        public static waifus lastGeneration  = new waifus();
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Title = "Waifu Generator";
            Directory.CreateDirectory("Waifus");
            if (File.Exists("seed.txt") == false) { File.Create("seed.txt").Close(); File.WriteAllText("seed.txt", "QTEyOEdDTQ.V6J61z5uS_tMmfzlaO8BR42E6n2S96lO0I4vL2G8lVkYsAsJ_EVX1t6TEf0.w2ToewwpIloL1QBy.zoJdvYkJpZ6IoZ1O8KlFcaJfgUuAFyUit8wbeE6nUWuxAJVwcMrMqYfC87X4L3co6it71qA1ANaEPNGhiL8mRjgUIzLBWlUZzaA-DV8b0blonuCqNUc3NOsZE8PncCPtDdYy9DcQNy7cOhDi2L-8rWoCpLsWR7VjtSY0blmNOY8gyX9VCrXM-uQC2TSOzZn3TxUYmg.5SCTnxuYHxvIetCLDSWTqQ"); }

            while (true == true)
            {
                Console.WriteLine("Type a command:");
                typeCommand(Console.ReadLine());
            }
        }

        private static void typeCommand(string commandString)
        {
            commandString = commandString.ToLower();
            string[] parameters = commandString.Split((char)32);

            if (parameters[0] == "clear") { Console.Clear(); return; }

            if (commandString == "dir") { Process.Start("waifus"); }
            if (commandString == "log") { Process.Start("log.txt"); }
            if (commandString == "saved") { Process.Start("saved_waifus.txt"); }
            
            if (commandString == "seeds")
            {
                foreach (string seed in lastGeneration.waifuSeeds)
                {
                    Console.WriteLine(seed);
                }
            }

            int requestNo = 1;
            string waifuSeed = string.Empty;
            int waifuStep = 3;

            int i = 0;
            foreach (string parameter in parameters)
            {
                if (parameter == string.Empty) { continue; }
                if (i == 0) { i += 1; continue; }
                if (parameter == "w") { waifuSeed = lastGeneration.lastSeed; }
                if (parameter.Substring(0, 1) == "s" && parameter.Length > 1) { try { waifuStep = int.Parse(parameter.Substring(1, parameter.Length - 1)); } catch {} }
                if (parameter.Substring(0, 1) == "w" && parameter.Length > 1) { try { waifuSeed = lastGeneration.waifuSeeds[int.Parse(parameter.Substring(1, parameter.Length - 1)) - 1]; } catch {} }
                if (parameter == "l") { waifuSeed = lastGeneration.lastSeed; }
                if (int.TryParse(parameter, out int n) == true) { requestNo = int.Parse(parameter); }
                if (parameter.Contains("[")) { waifuSeed = parameter; }
                if (parameter == "saved") { waifuSeed = "saved"; }
                i += 1;
            }

            try
            {
                if (waifuSeed != "saved")
                {
                    if (waifuSeed.Contains(".") == false || waifuSeed.Length < 50) { waifuSeed = string.Empty; }
                    if (waifuSeed == string.Empty) { waifuSeed = File.ReadAllText("seed.txt").Trim(); waifuStep = 0; }
                }
                
            } catch
            {
                printWarning("Invalid or no seed provided so used the default.");
                waifuSeed = "QTEyOEdDTQ.vzZqvT1mcMLFkJqcZYQpdlGbJZOBS2YVLJoCRkJIFP4LTDus2x-PxQWIxs4.gI_iRdT-OHuev9dJ.Lr1FWZNBbs1orCRjBE5nz2US1552mJII3VWuxTnz4-uoBkoAuMJVSxF_FhUasomfTLhiNxCFU-VvPcQUrPjjWq7wZKa2PSsi9ueBRgXvDbqS1zSL15uGLzy3CfvTa_RCyX9Mfz1DAFvCbR1Oz1qh_DMWu_7i3UkRLq4q6aQgFfcMQehu1hQWCIovg61mhaO2bPW0Yw.XBcmAnb_VQfxKKfJ2vxFYQ";
            }

            if (waifuStep > 4) { waifuStep = 0; }


            if (parameters[0] == "gen" && waifuSeed == "saved") {
                int waifuNo = 1;
                foreach (string seed in File.ReadAllLines("saved_waifus.txt"))
                {
                    imagesRequest3(requestNo, seed, waifuStep, waifuNo);
                    waifuNo += 1;
                }
                imagesRequest3(requestNo, waifuSeed, waifuStep);
            
            }
            if (parameters[0] == "gen" && waifuStep != 4) { imagesRequest3(requestNo, waifuSeed, waifuStep); }
            if (parameters[0] == "gen" && waifuStep == 4) { imagesRequest3(requestNo, waifuSeed, waifuStep, 1); }
            if (parameters[0] == "save") {
                if (File.Exists("saved_waifus.txt") == false) { File.Create("saved_waifus.txt").Close(); }

                if (parameters.Length != 2) { return; }
                File.AppendAllText("saved_waifus.txt", waifuSeed + "\n");
                printSuccess("Saved seed successfully!");
            }
            Console.WriteLine();
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static void imagesRequest3(int requests = 1, string waifuSeed = "", int waifuStep = 0, int preview = 0)
        {
            WebSocket webS = new WebSocket("wss://waifulabs.com/creator/socket/websocket?token=SFMyNTY.g2gDZAACb2tuBgBRztpsfgFiAAFRgA.LzTTTBZrKZ2-hcuksApkvV2tW0b5fTFh9JAOgg3P5yc&vsn=2.0.0");

            webS.OnMessage += WebS_OnMessage;

            Console.WriteLine($"Generating 16 waifus\n" +
                $"Seed: {waifuSeed}\n" +
                $"Step: {waifuStep}");

            webS.Connect();
            webS.Send("[\"3\",\"3\",\"api\",\"phx_join\",{}]");
            webS.Send("[\"3\",\"5\",\"api\",\"phx_leave\",{}]");
            webS.Send("[null,\"6\",\"api\",\"generate\",{\"id\":1,\"params\":{\"step\":" + waifuStep + "}}]");
            webS.Send("[\"7\",\"7\",\"api\",\"phx_join\",{}]");
            //webS.Send("[\"7\",\"8\",\"api\",\"generate\",{\"id\":1,\"params\":{\"step\":" + waifuStep + "}}]");
            //["3","6","api","generate_big",{"id":1,"params":{"currentGirl":"QTEyOEdDTQ.V6J61z5uS_tMmfzlaO8BR42E6n2S96lO0I4vL2G8lVkYsAsJ_EVX1t6TEf0.w2ToewwpIloL1QBy.zoJdvYkJpZ6IoZ1O8KlFcaJfgUuAFyUit8wbeE6nUWuxAJVwcMrMqYfC87X4L3co6it71qA1ANaEPNGhiL8mRjgUIzLBWlUZzaA-DV8b0blonuCqNUc3NOsZE8PncCPtDdYy9DcQNy7cOhDi2L-8rWoCpLsWR7VjtSY0blmNOY8gyX9VCrXM-uQC2TSOzZn3TxUYmg.5SCTnxuYHxvIetCLDSWTqQ"}}]
            webS.Send("[\"3\",\"8\",\"api\",\"generate\",{\"id\":1,\"params\":{\"step\":" + waifuStep + ",\"currentGirl\":\"" + waifuSeed + "\"}}]");

            lastGeneration.lastSeed = waifuSeed;
        }

        private static void WebS_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Data.Contains("newGirls")) { processResponse(e.Data); }
            else if (e.Data.Contains("error")) { Console.WriteLine("Something went wrong"); }
        }
        private static void processResponse(string response)
        {
            var newGirls = JArray.Parse(response).Last.First.First.First.First.First.First;
            
            try
            {
                lastGeneration.waifuSeeds.Clear();
            }
            catch { }

            File.Create("log.txt").Close();
            string toLog = string.Empty;

            List<string> imageResponses = new List<string>();

            int imageNo = 1;
            foreach (JToken seed in newGirls)
            {
                var jsonDouble = seed;

                string imageString = Newtonsoft.Json.JsonConvert.SerializeObject(jsonDouble.First.First);
                string newWaifuSeed = Newtonsoft.Json.JsonConvert.SerializeObject(jsonDouble.Last.First);
                imageString = imageString.Substring(1, imageString.Length - 2);
                newWaifuSeed = newWaifuSeed.Substring(1, newWaifuSeed.Length - 2);

                imageResponses.Add(newWaifuSeed);
                toLog += $"{imageNo}:{newWaifuSeed}\n";
                lastGeneration.waifuSeeds.Add(newWaifuSeed);

                

                byte[] imageBytes;
                try
                {
                    imageBytes = Convert.FromBase64String(imageString);
                }
                catch (Exception ex)
                {
                    printError($"Failed to convert image {imageNo} to byte array.");
                    printError(ex.Message);
                    printError($"imageString = {imageString}");
                    printError($"newWaifuSeed.length = {newWaifuSeed.Length}");
                    return;
                }
                Bitmap bmp;
                using (var ms = new MemoryStream(imageBytes))
                {
                    bmp = new Bitmap(ms);
                    bmp.Save($"Waifus/Waifu{imageNo}.jpeg");
                }
                imageNo += 1;
            }
            File.WriteAllText("log.txt", toLog);
            Console.WriteLine("Done!");
            return;
        }

        
        private static void imagesRequest2(int requests = 1, string waifuSeed = "", int waifuStep = 3, int preview = 0)
        {
            try
            {
                lastGeneration.waifuSeeds.Clear();
            }
            catch { }
            lastGeneration.lastSeed = waifuSeed;

            Console.WriteLine($"Generating {requests * 16} waifus\n" +
                $"Seed: {waifuSeed}\n" +
                $"Step: {waifuStep}");
            Console.WriteLine();

            File.Create("log.txt").Close();
            string toLog = string.Empty;

            string apiUrl = preview == 0 ? "https://api.waifulabs.com/generate" : "https://api.waifulabs.com/generate_preview";
            string stringContent = preview == 0 ? "{\"step\":" + waifuStep + ",\"currentGirl\":" + waifuSeed + "}" : "{\"currentGirl\":" + waifuSeed + ",\"product\":\"POSTER\"}";



            var handler = new HttpClientHandler();
            handler.UseCookies = false;

            using (var httpClient = new HttpClient(handler))
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), "wss://waifulabs.com/creator/socket/websocket?token=SFMyNTY.g2gDZAACb2tuBgDFtKRpfgFiAAFRgA.SMO2YX946XI1OUf8kmmX1RlHrjuUakdGkVCUO7u4cjI&vsn=2.0.0"))
                {
                    request.Headers.TryAddWithoutValidation("Pragma", "no-cache");
                    request.Headers.TryAddWithoutValidation("Origin", "https://waifulabs.com");
                    request.Headers.TryAddWithoutValidation("Accept-Language", "ja,en-GB;q=0.9,en;q=0.8");
                    request.Headers.TryAddWithoutValidation("Sec-WebSocket-Key", "j+zz62pf4taC5Rs9ccQqDQ==");
                    request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.71 Safari/537.36");
                    request.Headers.TryAddWithoutValidation("Upgrade", "websocket");
                    request.Headers.TryAddWithoutValidation("Sec-WebSocket-Extensions", "permessage-deflate; client_max_window_bits");
                    request.Headers.TryAddWithoutValidation("Cache-Control", "no-cache");
                    request.Headers.TryAddWithoutValidation("Connection", "Upgrade");
                    request.Headers.TryAddWithoutValidation("Sec-WebSocket-Version", "13");
                    request.Headers.TryAddWithoutValidation("Cookie", "__stripe_mid=97d115f0-2aca-48ee-a69b-0f8ae3de8496a18cab; _waifulab_key=SFMyNTY.g3QAAAABbQAAAAtfY3NyZl90b2tlbm0AAAAYVzgxTV9Kc3lWSWs5SEQ5TkcyaTFjVWdD.4acoEPjSP1AXACWWyXc6ZZtyWjZtrSlNPPMYtZQk7iA; __stripe_sid=1c3084f4-65f0-491a-8738-3580ccab656c9c5354");

                    var response = httpClient.SendAsync(request);
                    var responseResult = response.Result;
                    if (responseResult.StatusCode != HttpStatusCode.OK) { printError($"{responseResult.ReasonPhrase}. The provided seed was likely incorrect."); return; }

                    string content = responseResult.Content.ReadAsStringAsync().Result;



                    Console.WriteLine("Finished");
                }
            }
        }

        private static void imagesRequest(int requests = 1, string waifuSeed = "", int waifuStep = 3, int preview = 0)
        {       
            try
            {
                lastGeneration.waifuSeeds.Clear();
            } catch {}
            lastGeneration.lastSeed = waifuSeed;

            Console.WriteLine($"Generating {requests * 16} waifus\n" +
                $"Seed: {waifuSeed}\n" +
                $"Step: {waifuStep}");
            Console.WriteLine();

            File.Create("log.txt").Close();
            string toLog = string.Empty;

            string apiUrl = preview == 0 ? "https://api.waifulabs.com/generate" : "https://api.waifulabs.com/generate_preview";
            string stringContent = preview == 0 ? "{\"step\":" + waifuStep + ",\"currentGirl\":" + waifuSeed + "}" : "{\"currentGirl\":" + waifuSeed + ",\"product\":\"POSTER\"}";





            if (waifuStep == 4 || preview != 0)
            {

                var handler = new HttpClientHandler();

                // If you are using .NET Core 3.0+ you can replace `~DecompressionMethods.None` to `DecompressionMethods.All`
                handler.AutomaticDecompression = ~DecompressionMethods.None;

                // In production code, don't destroy the HttpClient through using, but better reuse an existing instance
                // https://www.aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
                using (var httpClient = new HttpClient(handler))
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://api.waifulabs.com/generate_preview"))
                    {
                        request.Headers.TryAddWithoutValidation("authority", "api.waifulabs.com");
                        request.Headers.TryAddWithoutValidation("pragma", "no-cache");
                        request.Headers.TryAddWithoutValidation("cache-control", "no-cache");
                        request.Headers.TryAddWithoutValidation("sec-ch-ua", "\"Google Chrome\";v=\"95\", \"Chromium\";v=\"95\", \";Not A Brand\";v=\"99\"");
                        request.Headers.TryAddWithoutValidation("dnt", "1");
                        request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                        request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4638.69 Safari/537.36");
                        request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
                        request.Headers.TryAddWithoutValidation("accept", "*/*");
                        request.Headers.TryAddWithoutValidation("origin", "https://waifulabs.com");
                        request.Headers.TryAddWithoutValidation("sec-fetch-site", "same-site");
                        request.Headers.TryAddWithoutValidation("sec-fetch-mode", "cors");
                        request.Headers.TryAddWithoutValidation("sec-fetch-dest", "empty");
                        request.Headers.TryAddWithoutValidation("referer", "https://waifulabs.com/");
                        request.Headers.TryAddWithoutValidation("accept-language", "ja,en-GB;q=0.9,en;q=0.8");

                        request.Content = new StringContent(stringContent);
                        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                        var response = httpClient.SendAsync(request);
                        var responseResult = response.Result;
                        if (responseResult.StatusCode != HttpStatusCode.OK) { printError($"{responseResult.ReasonPhrase}. The provided seed was likely incorrect."); return; }

                        string content = responseResult.Content.ReadAsStringAsync().Result;

                        List<string> imageResponses = new List<string>();
                        JToken jsonResponse = JObject.Parse(content).First;

                        //var imageString = seed.Last.Last;
                        var swting = Newtonsoft.Json.JsonConvert.SerializeObject(JObject.Parse(content).First.First);
                        //var newWaifuSeed = Newtonsoft.Json.JsonConvert.SerializeObject(seed.First.First);
                        //toLog += $"1:{newWaifuSeed}\n";
                        //lastGeneration.waifuSeeds.Add(newWaifuSeed);

                        //swting = swting.Substring(1, swting.Length - 3) + swting;

                        byte[] imageBytes = Convert.FromBase64String(swting.Substring(1, swting.Length - 2));

                        Bitmap bmp;
                        using (var ms = new MemoryStream(imageBytes))
                        {
                            bmp = new Bitmap(ms);
                            bmp.Save($"Waifus/Waifu{preview.ToString().Replace("0", "")}.jpeg");
                        }
                        return;
                    }
                }

            }


            int imageNo = 1;
            for (int requestNo = 1; requestNo <= requests; requestNo += 1)
            {

                var handler = new HttpClientHandler();

                handler.AutomaticDecompression = ~DecompressionMethods.None;

                using (var httpClient = new HttpClient(handler))
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://api.waifulabs.com/generate"))
                    {
                        request.Headers.TryAddWithoutValidation("authority", "api.waifulabs.com");
                        request.Headers.TryAddWithoutValidation("pragma", "no-cache");
                        request.Headers.TryAddWithoutValidation("cache-control", "no-cache");
                        request.Headers.TryAddWithoutValidation("sec-ch-ua", "\"Google Chrome\";v=\"95\", \"Chromium\";v=\"95\", \";Not A Brand\";v=\"99\"");
                        request.Headers.TryAddWithoutValidation("dnt", "1");
                        request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                        request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4638.54 Safari/537.36");
                        request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
                        request.Headers.TryAddWithoutValidation("accept", "*/*");
                        request.Headers.TryAddWithoutValidation("origin", "https://waifulabs.com");
                        request.Headers.TryAddWithoutValidation("sec-fetch-site", "same-site");
                        request.Headers.TryAddWithoutValidation("sec-fetch-mode", "cors");
                        request.Headers.TryAddWithoutValidation("sec-fetch-mode", "cors");
                        request.Headers.TryAddWithoutValidation("sec-fetch-dest", "empty");
                        request.Headers.TryAddWithoutValidation("referer", "https://waifulabs.com/");
                        request.Headers.TryAddWithoutValidation("accept-language", "ja,en-GB;q=0.9,en;q=0.8");

                        request.Content = new StringContent(stringContent);
                        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");


                        var response = httpClient.SendAsync(request);
                        var responseResult = response.Result;
                        if (responseResult.StatusCode != HttpStatusCode.OK) { printError($"{responseResult.ReasonPhrase}. The provided seed was likely incorrect.");  return; }
                        string content = responseResult.Content.ReadAsStringAsync().Result;

                        List<string> imageResponses = new List<string>();
                        JToken jsonResponse = JObject.Parse(content).First.First;
                        
                        foreach (JToken seed in jsonResponse)
                        {
                            var imageString = Newtonsoft.Json.JsonConvert.SerializeObject(seed.Last.First);
                            var newWaifuSeed = Newtonsoft.Json.JsonConvert.SerializeObject(seed.First.First);
                            imageResponses.Add(newWaifuSeed);
                            toLog += $"{imageNo}:{newWaifuSeed}\n";
                            lastGeneration.waifuSeeds.Add(newWaifuSeed);

                            //swting = swting.Substring(1, swting.Length - 3) + swting;

                            byte[] imageBytes;
                            try
                            {   try
                                {
                                    imageBytes = Convert.FromBase64String(newWaifuSeed.Substring(1, newWaifuSeed.Length - 2));
                                }
                                catch
                                {
                                    string base64ImageString = imageString;
                                    base64ImageString = base64ImageString.Substring(1, base64ImageString.Length - 2);

                                    imageBytes = Convert.FromBase64String(base64ImageString);
                                }
                            }
                            catch (Exception ex)
                            {
                                printError($"Failed to convert image {imageNo} to byte array.");
                                printError(ex.Message);
                                printError($"imageString = {imageString}");
                                printError($"newWaifuSeed.length = {newWaifuSeed.Length}");
                                return;
                            }
                            Bitmap bmp;
                            using (var ms = new MemoryStream(imageBytes))
                            {
                                bmp = new Bitmap(ms);
                                bmp.Save($"Waifus/Waifu{imageNo}.jpeg");
                            }
                            imageNo += 1;
                        }


                    }
                }

                File.WriteAllText("log.txt", toLog);

                Console.WriteLine($"Done request {requestNo}");
            }
            Console.WriteLine($"Generated {requests * 16} waifus!");
        }





        private static void printError(string errorMessage)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(errorMessage);
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void printWarning(string warningMessage)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(warningMessage);
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void printSuccess(string successMessage)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(successMessage);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
