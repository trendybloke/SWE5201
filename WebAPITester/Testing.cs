using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WebAPITester.Models;
using Xunit;

namespace WebAPITester
{
    public class Testing
    {
        static HttpClient client = new HttpClient();
        string baseAdr = "https://localhost:7269";

        public async Task initClient()
        {
            client.BaseAddress = new Uri(baseAdr);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );
        }

        [Fact]
        public async Task<bool> GetHostedEventOne()
        {
            await initClient();

            var hostedEventOne = await client.GetAsync($"{baseAdr}/api/HostedEvents/1");

            return hostedEventOne.IsSuccessStatusCode;
        }

        [Fact]
        public async Task<bool> GetEventTwo()
        {
            await initClient();

            var eventTwo = await client.GetAsync($"{baseAdr}/api/Events/2");

            return eventTwo.IsSuccessStatusCode;
        }

        [Fact]
        public async Task<bool> GetAllUsers()
        {
            await initClient();

            var allUsers = await client.GetAsync($"{baseAdr}/api/Account/List");

            return allUsers.IsSuccessStatusCode;
        }

        [Fact]
        public async Task<bool> GetAllBookingsForUser()
        {
            await initClient();

            var allBookingsForUser = await client.GetAsync($"{baseAdr}/api/ByUser/ddb3be84-b512-4c0b-91d3-3146ddc5ab76");

            return allBookingsForUser.IsSuccessStatusCode;
        }
    }
}
