using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.ComponentModel;
using App.Data;


namespace App.Services
{
    public class DataService
    {
        private HttpClient Client { get; set; }
        private Dictionary<string, string> ModelEndpoints { get; set; }
        private string BaseUrlAddress { get; set; }
        private Action LoginHandler { get; set; }
        public bool IsStartLoginOnUnauthorizedEnabled { get; private set; }
        public bool ExceptionOnHttpClientError { get; private set; }

        public HttpResponseMessage LastResponse { get; private set; }

        static LoggedInUserAccount loggedInUserAccount;
        public LoggedInUserAccount LoggedInUserAccount
        {
            get { return loggedInUserAccount; }
            set
            {
                loggedInUserAccount = value;
                PropertyChanged?.Invoke(
                    this, 
                    new PropertyChangedEventArgs(nameof(LoggedInUserAccount))
                );
            }
        }

        public DataService(string baseAddress)
        {
            this.Client = new HttpClient();
            #if DEBUG
            // Insecure SSL certificate
            HttpClientHandler insecureHandler = new HttpClientHandler();
            insecureHandler.ServerCertificateCustomValidationCallback
                = (message, cert, chain, errors) =>
                {
                    if (cert.Issuer.Equals("CN=localhost"))
                        return true;
                    return errors == System.Net.Security.SslPolicyErrors.None;
                };
            this.Client = new HttpClient(insecureHandler);
            #endif
            this.BaseUrlAddress = baseAddress;
            this.ModelEndpoints = new Dictionary<string, string>();
            this.EnableThrowExceptionOnHttpClientError();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public DataService AddEntityModelEndpoint<TEntity>(string endpoint)
        {
            this.ModelEndpoints.Add(typeof(TEntity).FullName, endpoint);
            return this;
        }

        public DataService AddBearerToken()
        {
            return this.AddBearerToken(this.LoggedInUserAccount?.AccessToken);
        }

        public DataService AddBearerToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                this.RemoveBearerToken();
            }
            else
            {
                this.Client.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", token);
            }
            return this;
        }
        public DataService RemoveBearerToken()
        {
            this.Client.DefaultRequestHeaders.Authorization = null;
            return this;
        }
        public DataService SetLoginFlowAction(Action handler)
        {
            this.LoginHandler = handler;
            return this;
        }

        public DataService RemoveLoginFlowAction()
        {
            this.LoginHandler = null;
            return this;
        }
        public DataService EnableLoginOnUnauthorized()
        {
            // set the flag to enable but only if the LoginHandler delegate action has been set
            this.IsStartLoginOnUnauthorizedEnabled = this.LoginHandler != null;
            return this;
        }
        public DataService DisableLoginOnUnauthorized()
        {
            this.IsStartLoginOnUnauthorizedEnabled = false;
            return this;
        }
        public DataService EnableThrowExceptionOnHttpClientError()
        {
            this.ExceptionOnHttpClientError = true;
            return this;
        }
        public DataService DisableThrowExceptionOnHttpClientError()
        {
            this.ExceptionOnHttpClientError = false;
            return this;
        }
        private string GetEntityEndpoint<TEntity>(string nonDefaultEndpoint = null)
        {
            // private helper method to get the endpoint,
            // factored out into separate method for code reuse purposes
            StringBuilder endpoint = 
                new StringBuilder(this.ModelEndpoints[typeof(TEntity).FullName]);

            if (!string.IsNullOrEmpty(nonDefaultEndpoint))
            {
                endpoint.Append($"/{nonDefaultEndpoint}");
            }
            return endpoint.ToString();
        }

        private void HandleUnsuccesfulResponse()
        {
            if(LastResponse.StatusCode == HttpStatusCode.Unauthorized
                && this.IsStartLoginOnUnauthorizedEnabled)
            {
                this.RemoveBearerToken();
                this.LoggedInUserAccount = null;
                this.LoginHandler();
            }
            else
            {
                if(LastResponse.StatusCode == HttpStatusCode.Unauthorized
                    && !this.IsStartLoginOnUnauthorizedEnabled)
                {
                    this.RemoveBearerToken();
                }

                if (this.ExceptionOnHttpClientError)
                {
                    if(LastResponse.Content is object)
                    {
                        string problemDetailsStr = LastResponse.Content.ReadAsStringAsync().Result;
                        ValidationProblemDetails problemDetails 
                            = JsonConvert.DeserializeObject<ValidationProblemDetails>
                                (problemDetailsStr);

                        if(problemDetails != null)
                        {
                            var validationErrors = problemDetails.Errors
                                                    .SelectMany(x => x
                                                    .Value.Select(y => y));

                            var flattenedErrors = string.Join(Environment.NewLine, validationErrors);

                            throw new Exception($"{problemDetails.Status} {LastResponse.ReasonPhrase} {Environment.NewLine}{problemDetails.Title}" +
                                $"{Environment.NewLine}{problemDetails.Detail} {Environment.NewLine}{flattenedErrors}");
                        }
                        else
                        {
                            throw new Exception($"{LastResponse.StatusCode} {LastResponse.ReasonPhrase}");
                        }
                    }
                    else
                    {
                        throw new Exception($"{LastResponse.StatusCode} {LastResponse.ReasonPhrase}");
                    }
                }
                else
                {
                    throw new Exception($"{LastResponse.StatusCode} {LastResponse.ReasonPhrase}");
                }
            }
        }

        public Task<List<TEntity>> GetAllAsync<TEntity>(string nonDefaultEndpoint = null)
            where TEntity : new()
        {
            return GetAllAsync<TEntity>(nonDefaultEndpoint, null);
        }

        public Task<List<TEntity>> GetAllAsync<TEntity>(IList<string> searchParams)
            where TEntity : new()
        {
            return GetAllAsync<TEntity>(null, searchParams);
        }

        public async Task<List<TEntity>> GetAllAsync<TEntity>(string nonDefaultEndpoint, IList<string> searchParams)
            where TEntity : new()
        {
            // form the URI for the webservice GET request
            var endpoint = GetEntityEndpoint<TEntity>(nonDefaultEndpoint);
            StringBuilder searchParamsSB = new StringBuilder();
            if (searchParams != null)
            {
                foreach (var param in searchParams)
                {
                    searchParamsSB.Append($"/{param}");
                }
            }
            var url = $"{this.BaseUrlAddress}/{endpoint}{searchParamsSB}";
            var uri = new Uri(string.Format(url));
            // make the GET request to the URI

            //LastResponse = await Client.GetAsync(uri)
            //.ConfigureAwait(false);

            LastResponse = Client.GetAsync(uri).Result;

            if (LastResponse.IsSuccessStatusCode)
            {
                // read the content returned by the GET request
                var content = await LastResponse.Content
                                        .ReadFromJsonAsync<List<TEntity>>();

                // return deserialised objects back to caller as List<TEntity> collection
                return content;
            }
            else
            {
                this.HandleUnsuccesfulResponse();
                return null;
            }
        }

        public async Task<TEntity> GetAsync<TEntity, T>(T id, string nonDefaultEndpoint = null)
 where TEntity : new()
        {
            // form the URI for the webservice GET request
            var endpoint = GetEntityEndpoint<TEntity>(nonDefaultEndpoint);
            var uri = new Uri($"{this.BaseUrlAddress}/{endpoint}/{id}");
            // make the GET request to the URI
            LastResponse =
            await Client.GetAsync(uri)
            .ConfigureAwait(false);
            if (LastResponse.IsSuccessStatusCode)
            {
                // read the content returned by the GET request
                var content = await
                LastResponse.Content
                .ReadFromJsonAsync<TEntity>();
                // return deserialised object back to caller as TEntity type
                return content;
            }
            else
            {
                this.HandleUnsuccesfulResponse();
                return default(TEntity);
            }
        }
        public async Task<bool> UpdateAsync<TEntity, T>(TEntity entity, T id, string nonDefaultEndpoint = null)
        {
            // form the URI for the webservice PUT request
            var endpoint = GetEntityEndpoint<TEntity>(nonDefaultEndpoint);
            var url = $"{this.BaseUrlAddress}/{endpoint}/{id}";
            var uri = new Uri(url);
            // make the PUT request to the URI
            LastResponse = await Client.PutAsJsonAsync(uri, entity)
            .ConfigureAwait(false);
            if (!LastResponse.IsSuccessStatusCode)
            {
                this.HandleUnsuccesfulResponse();
            }
            // return success or failure boolean
            return LastResponse.IsSuccessStatusCode;
        }
        public async Task<TEntity> InsertAsync<TEntity>(TEntity entity, string nonDefaultEndpoint = null)
        {
            return await this.InsertAsync<TEntity, TEntity>(entity, nonDefaultEndpoint);
        }
        public async Task<TReturnEntity> InsertAsync<TEntity, TReturnEntity>(TEntity entity, string nonDefaultEndpoint = null)
        {
            // form the URI for the webservice POST request
            var endpoint = GetEntityEndpoint<TEntity>(nonDefaultEndpoint);
            var url = $"{this.BaseUrlAddress}/{endpoint}";
            var uri = new Uri(url);
            // make the POST request to the URI
            LastResponse = await Client.PostAsJsonAsync(uri, entity)
            .ConfigureAwait(false);
            if (LastResponse.IsSuccessStatusCode)
            {
                // read the response content returned by the POST request (the created object)
                var responseContent = await
                LastResponse.Content
                .ReadFromJsonAsync<TReturnEntity>()
                .ConfigureAwait(false);
                // return deserialised object back to caller as TReturnEntity type
                return responseContent;
            }
            else
            {
                this.HandleUnsuccesfulResponse();
                return default(TReturnEntity);
            }
        }
        public async Task<bool> DeleteAsync<TEntity, T>(TEntity entity, T id, string nonDefaultEndpoint = null)
        {
            // form the URI for the webservice DELETE request
            var endpoint = GetEntityEndpoint<TEntity>(nonDefaultEndpoint);
            var url = $"{this.BaseUrlAddress}/{endpoint}/{id}";
            var uri = new Uri(url);
            // make the DELETE request to the URI
            LastResponse = await Client.DeleteAsync(uri)
            .ConfigureAwait(false);
            if (!LastResponse.IsSuccessStatusCode)
            {
                this.HandleUnsuccesfulResponse();
            }
            // return success or failure boolean
            return LastResponse.IsSuccessStatusCode;
        }
    }
}
