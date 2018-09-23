using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using Wox.Plugin;

namespace WOX.Nasa.APOD
{
    public class Main : IPlugin
    {
        public readonly string ApiKey = "DEMO_KEY";
        public readonly string ApodUrl = "https://api.nasa.gov/planetary/apod";
        public bool ValidDate { get; set; }
        public WebClient Client { get; set; }

        public void Init(PluginInitContext context)
        {
            Client = new WebClient();
        }

        public List<Result> Query(Query query)
        {
            var dateQuery = query.Search;
            ValidDate = false;

            if (dateQuery.Length >= 8 && dateQuery.Length <= 10)
            {
                var date = ParseToDate(dateQuery);

                if (ValidDate)
                {
                    var formattedDate = date.ToString("yyyy-MM-dd");
                    var apiRequestUrl = $"{ApodUrl}?date={formattedDate}&api_key={ApiKey}";

                    try
                    {
                        var rawResult = Client.DownloadString(apiRequestUrl);
                        return new List<Result> { CrateSuccessResult(rawResult) };
                    }
                    catch (Exception e)
                    {
                        return new List<Result> { CreateFailedResult(e) };
                    }                   
                }
            }

            return null;
        }

        private DateTime ParseToDate(string query)
        {
            DateTime result;
            string[] formats = new[] { "yyyy-MM-dd", "yyyyMMdd" };


            ValidDate = DateTime.TryParseExact(query, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);

            return result;
        }

        private Result CreateFailedResult(Exception exception)
        {
            var result = new Result
            {
                Title = "Failed to load result from NASA",
                IcoPath = "Images\\nasa-logo.png",
                SubTitle = exception.Message,
                Action = null
            };

            return result;
        }

        private Result CrateSuccessResult(string json)
        {
            try
            {
                NasaApodResult nasaObj = JsonConvert.DeserializeObject<NasaApodResult>(json);

                var result = new Result
                {
                    Title = nasaObj.title,
                    IcoPath = "Images\\nasa-logo.png",
                    SubTitle = nasaObj.explanation,
                    Action = (Func<ActionContext, bool>)(c =>
                    {
                        Process.Start(nasaObj.hdurl);
                        return true;
                    })
                };

                return result;

            }
            catch (Exception e)
            {
                return CreateFailedResult(e);
            }
        }
    }
}
