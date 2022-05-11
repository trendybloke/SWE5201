using JWT;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using App.Converters;

namespace App.Data
{
    public class DecodedAccessToken
    {
        // Same secret as the API
        private static readonly string secretKey = "C28D4153698F080311DB7C324BE65E783250C22043758F21B2C11B20AF8A0F74";

        private string accessToken;
        public string AccessToken
        {
            get { return accessToken; }
            set
            {
                if (string.IsNullOrEmpty(accessToken))
                {
                    accessToken = value;
                }
                else
                    throw new Exception("Cannot set AccessTokenData again once set.");
            }
        }

        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("given_name")]
        public string GivenName { get; set; }
        [JsonProperty("family_name")]
        public string FamilyName { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("jti")]
        public string Jti { get; set; }

        // Converts N number of roles to an array of N indexes with each role
        [JsonConverter(typeof(SingleOrArrayJsonConverter<string>))]
        [JsonProperty("role")]
        public IList<string> Role { get; set; }
        [JsonProperty("exp")]
        public int Exp { get; set; }
        [JsonProperty("iss")]
        public string Iss { get; set; }
        [JsonProperty("aud")]
        public string Aud { get; set; }
        protected bool HasRole(string roleToCheck)
        {
            if (Role != null)
            {
                return Role.Any(r => r == roleToCheck);
            }
            return false;
        }
        public static T Decode<T>(Token token)
        where T : DecodedAccessToken
        {
            string jsonPayload = JsonWebToken.Decode(token.Data, secretKey);
            T dat = JsonConvert.DeserializeObject<T>(jsonPayload);
            dat.AccessToken = token.Data;
            return dat;
        }

    }
}
