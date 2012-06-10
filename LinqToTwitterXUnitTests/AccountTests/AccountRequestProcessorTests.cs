﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using LinqToTwitter;
using LinqToTwitterTests.Common;
using Xunit;

namespace LinqToTwitterXUnitTests.AccountTests
{
    public class AccountRequestProcessorTests
    {
        public AccountRequestProcessorTests()
        {
            TestCulture.SetCulture();
        }

        [Fact]
        public void ProcessResults_Handles_VerifyCredentials()
        {
            var target = new AccountRequestProcessor<Account>();

            List<Account> actual = target.ProcessResults(TestVerifyCredentialsQueryResponse);

            var acct = actual.Cast<Account>().ToList().FirstOrDefault();
            Assert.NotNull(acct);
            Assert.Equal("Joe Mayo", acct.User.Name);
        }

        [Fact]
        public void HandleRateLimitStatus_Converts_RateLimit_To_Account()
        {
            var acctReqProc = new AccountRequestProcessor<Account> { Type = AccountType.RateLimitStatus };
            DateTime expectedDateTime = new DateTimeOffset(2011, 9, 19, 2, 6, 36, 0, new TimeSpan(0, 0, 0)).DateTime;

            Account acct = acctReqProc.HandleRateLimitResponse(TestRateLimitStatusQueryResponse);

            Assert.NotNull(acct);
            Assert.Equal(343, acct.RateLimitStatus.RemainingHits);
            Assert.Equal(350, acct.RateLimitStatus.HourlyLimit);
            Assert.Equal(1316397996, acct.RateLimitStatus.ResetTimeInSeconds);
            Assert.Equal(expectedDateTime, acct.RateLimitStatus.ResetTime);
        }

        [Fact]
        public void HandleTotalsResponse_Converts_Totals_To_Account()
        {
            var acctReqProc = new AccountRequestProcessor<Account> { Type = AccountType.Totals };

            Account acct = acctReqProc.HandleTotalsResponse(TestTotalsResponse);

            Assert.NotNull(acct);
            Assert.Equal(1624, acct.Totals.Updates);
            Assert.Equal(161, acct.Totals.Friends);
            Assert.Equal(65, acct.Totals.Favorites);
            Assert.Equal(875, acct.Totals.Followers);
        }

        [Fact]
        public void HandleSettingsResponse_Converts_Settings_To_Account()
        {
            var acctReqProc = new AccountRequestProcessor<Account>();

            Account acct = acctReqProc.HandleSettingsResponse(TestSettingsResponse);

            Assert.NotNull(acct);
            Assert.Equal("23424977", acct.Settings.TrendLocation.WoeID);
            Assert.Equal("United States", acct.Settings.TrendLocation.Name);
            Assert.Equal(12, acct.Settings.TrendLocation.PlaceTypeNameCode);
            Assert.Equal("Country", acct.Settings.TrendLocation.PlaceTypeName);
            Assert.Equal("US", acct.Settings.TrendLocation.CountryCode);
            Assert.Equal("United States", acct.Settings.TrendLocation.Country);
            Assert.Equal("http://where.yahooapis.com/v1/place/23424977", acct.Settings.TrendLocation.Url);
            Assert.Equal(true, acct.Settings.GeoEnabled);
            Assert.Equal(false, acct.Settings.SleepTime.Enabled);
            Assert.Equal(0, acct.Settings.SleepTime.StartHour);
            Assert.Equal(0, acct.Settings.SleepTime.EndHour);
        }

        [Fact]
        public void ProcessActionResult_Handles_EndSession()
        {
            var acctReqProc = new AccountRequestProcessor<Account>();

            var acct = acctReqProc.ProcessActionResult(TestEndSessionResponse, AccountAction.EndSession);

            Assert.Equal("Logged out.", acct.EndSessionStatus.Error);
        }

        [Fact]
        public void HandleEndSessionResponse_Converts_EndSession_To_Account()
        {
            var acctReqProc = new AccountRequestProcessor<Account>();

            var acct = acctReqProc.HandleEndSessionResponse(TestEndSessionResponse);

            Assert.Equal("Logged out.", acct.EndSessionStatus.Error);
            Assert.Equal("/1/account/end_session.json", acct.EndSessionStatus.Request);
        }

        [Fact]
        public void ProcessResults_Returns_Empty_Collection_When_Empty_Results()
        {
            var reqProc = new AccountRequestProcessor<Account> { BaseUrl = "http://api.twitter.com/1/" };

            var accts = reqProc.ProcessResults(string.Empty);

            Assert.Equal(0, accts.Count);
        }

        [Fact]
        public void GetParametersTest()
        {
            var target = new AccountRequestProcessor<Account>();
            Expression<Func<Account, bool>> expression = acct => acct.Type == AccountType.RateLimitStatus;
            var lambdaExpression = expression as LambdaExpression;

            var queryParams = target.GetParameters(lambdaExpression);

            Assert.True(
                queryParams.Contains(
                    new KeyValuePair<string, string>("Type", ((int)AccountType.RateLimitStatus).ToString(CultureInfo.InvariantCulture))));
        }

        [Fact]
        public void BuildUrl_Constructs_VerifyCredentials_Url()
        {
            const string ExpectedUrl = "https://api.twitter.com/1/account/verify_credentials.json";
            var acctReqProc = new AccountRequestProcessor<Account> { BaseUrl = "https://api.twitter.com/1/" };
            var parameters =
                new Dictionary<string, string>
                {
                        { "Type", ((int)AccountType.VerifyCredentials).ToString(CultureInfo.InvariantCulture) }
                };

            Request req = acctReqProc.BuildUrl(parameters);

            Assert.Equal(ExpectedUrl, req.FullUrl);
        }

        [Fact]
        public void BuildUrl_Constructs_RateLimitStatus_Url()
        {
            const string ExpectedUrl = "https://api.twitter.com/1/account/rate_limit_status.json";
            var acctReqProc = new AccountRequestProcessor<Account> { BaseUrl = "https://api.twitter.com/1/" };
            var parameters =
                new Dictionary<string, string>
                {
                        { "Type", ((int)AccountType.RateLimitStatus).ToString(CultureInfo.InvariantCulture) }
                };

            Request req = acctReqProc.BuildUrl(parameters);

            Assert.Equal(ExpectedUrl, req.FullUrl);
        }

        [Fact]
        public void BuildUrl_Returns_Totals_Url()
        {
            const string ExpectedUrl = "https://api.twitter.com/1/account/totals.json";
            var acctReqProc = new AccountRequestProcessor<Account> { BaseUrl = "https://api.twitter.com/1/" };
            var parameters = new Dictionary<string, string>
                {
                        { "Type", ((int)AccountType.Totals).ToString(CultureInfo.InvariantCulture) }
                };

            Request req = acctReqProc.BuildUrl(parameters);

            Assert.Equal(ExpectedUrl, req.FullUrl);
        }

        [Fact]
        public void BuildUrl_Returns_Settings_Url()
        {
            const string ExpectedUrl = "https://api.twitter.com/1/account/settings.json";
            var acctReqProc = new AccountRequestProcessor<Account> { BaseUrl = "https://api.twitter.com/1/" };
            var parameters = new Dictionary<string, string>
                {
                        { "Type", ((int)AccountType.Settings).ToString(CultureInfo.InvariantCulture) }
                };

            Request req = acctReqProc.BuildUrl(parameters);

            Assert.Equal(ExpectedUrl, req.FullUrl);
        }

        [Fact]
        public void BuildUrl_Throws_When_Type_Not_Provided()
        {
            var acctReqProc = new AccountRequestProcessor<Account> { BaseUrl = "https://api.twitter.com/1/" };
            var parameters = new Dictionary<string, string>();

            var ex = Assert.Throws<ArgumentException>(() => acctReqProc.BuildUrl(parameters));

            Assert.Equal("Type", ex.ParamName);
        }

        [Fact]
        public void BuildUrl_Throws_With_Null_Parameters()
        {
            var acctReqProc = new AccountRequestProcessor<Account> { BaseUrl = "https://api.twitter.com/1/" };

            var ex = Assert.Throws<ArgumentException>(() => acctReqProc.BuildUrl(null));

            Assert.Equal("Type", ex.ParamName);
        }

        const string TestVerifyCredentialsQueryResponse = @"{
   ""statuses_count"":1624,
   ""favourites_count"":65,
   ""protected"":false,
   ""profile_text_color"":""3C3940"",
   ""profile_image_url"":""http:\/\/a2.twimg.com\/profile_images\/520626655\/JoeTwitterBW_-_150_x_150_normal.jpg"",
   ""name"":""Joe Mayo"",
   ""profile_sidebar_fill_color"":""95E8EC"",
   ""default_profile"":false,
   ""listed_count"":101,
   ""following"":false,
   ""profile_background_tile"":false,
   ""utc_offset"":-25200,
   ""description"":""Independent .NET Consultant; author of 6 books; MVP; interested in C#, MVC, and Language Integrated Query (LINQ)"",
   ""location"":""Denver, CO"",
   ""contributors_enabled"":false,
   ""verified"":false,
   ""profile_link_color"":""0099B9"",
   ""followers_count"":875,
   ""url"":""http:\/\/linqtotwitter.codeplex.com\/"",
   ""status"":{
      ""in_reply_to_status_id_str"":null,
      ""truncated"":false,
      ""in_reply_to_user_id_str"":null,
      ""geo"":null,
      ""retweet_count"":2,
      ""contributors"":null,
      ""coordinates"":null,
      ""favorited"":false,
      ""in_reply_to_screen_name"":null,
      ""source"":""web"",
      ""id_str"":""115131137906589696"",
      ""in_reply_to_status_id"":null,
      ""id"":115131137906589696,
      ""created_at"":""Sat Sep 17 18:32:56 +0000 2011"",
      ""possibly_sensitive"":false,
      ""place"":null,
      ""retweeted"":false,
      ""in_reply_to_user_id"":null,
      ""text"":""Visual Studio 11 Developer Preview: http:\/\/t.co\/fISuavMD""
   },
   ""profile_sidebar_border_color"":""5ED4DC"",
   ""screen_name"":""JoeMayo"",
   ""default_profile_image"":false,
   ""notifications"":false,
   ""show_all_inline_media"":false,
   ""geo_enabled"":true,
   ""profile_use_background_image"":true,
   ""friends_count"":161,
   ""id_str"":""15411837"",
   ""is_translator"":false,
   ""lang"":""en"",
   ""time_zone"":""Mountain Time (US & Canada)"",
   ""created_at"":""Sun Jul 13 04:35:50 +0000 2008"",
   ""profile_background_color"":""0099B9"",
   ""id"":15411837,
   ""follow_request_sent"":false,
   ""profile_background_image_url_https"":""https:\/\/si0.twimg.com\/profile_background_images\/13330711\/200xColor_2.png"",
   ""profile_background_image_url"":""http:\/\/a1.twimg.com\/profile_background_images\/13330711\/200xColor_2.png"",
   ""profile_image_url_https"":""https:\/\/si0.twimg.com\/profile_images\/520626655\/JoeTwitterBW_-_150_x_150_normal.jpg""
}";

        const string TestRateLimitStatusQueryResponse = @"{
   ""remaining_hits"":343,
   ""hourly_limit"":350,
   ""reset_time_in_seconds"":1316397996,
   ""reset_time"":""Mon Sep 19 02:06:36 +0000 2011""
}";

        const string TestEndSessionResponse = @"{
  ""request"": ""/1/account/end_session.json"",
  ""error"": ""Logged out.""
}";

        const string TestTotalsResponse = @"{
   ""followers"":875,
   ""favorites"":65,
   ""friends"":161,
   ""updates"":1624
}";

        const string TestSettingsResponse = @"{
   ""protected"":false,
   ""trend_location"":[
      {
         ""name"":""United States"",
         ""countryCode"":""US"",
         ""placeType"":{
            ""name"":""Country"",
            ""code"":12
         },
         ""parentid"":1,
         ""country"":""United States"",
         ""url"":""http:\/\/where.yahooapis.com\/v1\/place\/23424977"",
         ""woeid"":23424977
      }
   ],
   ""language"":""en"",
   ""sleep_time"":{
      ""start_time"":null,
      ""end_time"":null,
      ""enabled"":false
   },
   ""show_all_inline_media"":false,
   ""discoverable_by_email"":true,
   ""time_zone"":{
      ""tzinfo_name"":""America\/Denver"",
      ""name"":""Mountain Time (US & Canada)"",
      ""utc_offset"":-25200
   },
   ""geo_enabled"":true,
   ""screen_name"":""JoeMayo"",
   ""always_use_https"":true
}";
    }
}