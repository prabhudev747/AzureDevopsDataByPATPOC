using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;

namespace AzureDevopsDataByPAT
{

    class Program
    {
        private static readonly HttpClient client = new HttpClient();
      
        public int Id { get; set; }
        public string Name { get; set; }
        static async Task Main(string[] args)
        {
            string apiUrl = $"https://vsaex.dev.azure.com/organization/_apis/userentitlements?api-version=6.0-preview.3";
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json")); 
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", GetAuthenticationHeaderWithToken("PATtoken"));


            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string jsonResult = await response.Content.ReadAsStringAsync();
                var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
               // var users = JsonConvert.DeserializeObject<UserModel>(messageContent, settings);
                var Mylist1 = JsonConvert.DeserializeObject<UserModel>(jsonResult, settings);
                List<Members> Mylist = Mylist1.members.Where(m => m.user.originId != Guid.Empty).ToList();
                //Console.ReadKey();

                try
                {
                    using (var sqlConnection = new SqlConnection(""))
                    {
                        sqlConnection.Open();
                       
                        foreach (var user in Mylist)
                        {
                            DateTime currentDate = DateTime.Now;
                            bool a = true;

                            var command = new SqlCommand("INSERT INTO UserDetails(DevopsObjectId,AzureObjectId,DisplayName,UserPrincipalName,ModifiedDate,IsActive) VALUES (@dvUserId,@azUserId, @DisplayName,@UserPrincipalName,@date,@bool)", sqlConnection);
                            command.Parameters.AddWithValue("@dvUserId", user.id);
                            command.Parameters.AddWithValue("@azUserId", user.user.originId);
                            command.Parameters.AddWithValue("@DisplayName", user.user.displayName);
                            command.Parameters.AddWithValue("@UserPrincipalName", user.user.principalName);
                            command.Parameters.AddWithValue("@date",currentDate);
                            command.Parameters.AddWithValue("@bool",a);

                            command.ExecuteNonQuery();
                        }
                        sqlConnection.Close();
                    }
                    Console.WriteLine($"Found {Mylist.Count()} users in the tenant");
                }
                catch (Exception e)
                {
                    Console.WriteLine("We could not retrieve the user's list: " + $"{e}");
                }
            }
            else
            {
                Console.WriteLine("Failed to retrieve user data: " + response.ReasonPhrase);
            }

            Console.ReadLine();
        }

        public static string GetAuthenticationHeaderWithToken(string token)
        {
            return Convert.ToBase64String(
                    System.Text.ASCIIEncoding.ASCII.GetBytes(
                        string.Format("{0}:{1}", "", token)));
        }
    }
    public class Members
    {
        public Guid id { get; set; }       
        public User user { get; set; }

    }
    public class User
    {
        public string displayName { get; set; }
        public string principalName { get; set; }
        public Guid originId { get; set; }
    }
    public class UserModel
    {
        public List<Members> members { get; set; }
    }
}