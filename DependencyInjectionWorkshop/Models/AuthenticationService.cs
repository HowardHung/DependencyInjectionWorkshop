using SlackAPI;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using Dapper;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        public bool Verify(string accountId, string password, string otp)
        {
            var passwordFromDb = GetPassword(accountId);
            var hashedPassword = GetHash(password);
            var currentOtp = GetOtp(accountId);
            if (passwordFromDb==hashedPassword&&currentOtp == otp)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public string GetPassword(string accountId)
        {
            using (var connection = new SqlConnection("my connection string"))
            {
                var password = connection.Query<string>("spGetUserPassword", new { Id = accountId },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();

                return password;
            }
        }
        public string GetHash(string plainText)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(plainText));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }
        public string GetOtp(string accountId)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
            var response = httpClient.PostAsJsonAsync("api/otps", accountId).Result;
            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadAsAsync<string>().Result;
            }
            else
            {
                throw new Exception($"web api error, accountId:{accountId}");
            }

        }
    }
}