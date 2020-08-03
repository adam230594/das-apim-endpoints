﻿using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.SharedOuterApi.Interfaces
{
    public interface IRestApiClient
    {
        Task<string> Get(string uri, CancellationToken cancellationToken = default);
        Task<string> Get(string uri, object queryData, CancellationToken cancellationToken = default);
        Task<T> Get<T>(string uri, CancellationToken cancellationToken = default) where T : class, new();
        Task<T> Get<T>(string uri, object queryData, CancellationToken cancellationToken = default) where T : class, new();

        Task<HttpStatusCode> GetHttpStatusCode(string uri, CancellationToken cancellationToken = default);
        Task<HttpStatusCode> GetHttpStatusCode(string uri, object queryData, CancellationToken cancellationToken = default);

        Task<string> Post(string uri, CancellationToken cancellationToken = default);
        Task<string> Post<TRequest>(string uri, TRequest request, CancellationToken cancellationToken = default) where TRequest : class;

        Task<TResponse> Post<TRequest, TResponse>(string uri, TRequest requestData, CancellationToken cancellationToken = default) where TRequest : class where TResponse : class, new();
    }
}