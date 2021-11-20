﻿using System.Net.Http;
using System.Threading.Tasks;
using NFluent;
using Suzianna.Rest.Screenplay.Abilities;
using Suzianna.Rest.Screenplay.Interactions;
using Suzianna.Rest.Tests.Unit.TestConstants;
using Suzianna.Rest.Tests.Unit.TestDoubles;
using Suzianna.Rest.Tests.Unit.TestUtils;
using Xunit;

namespace Suzianna.Rest.Tests.Unit.Tests.Screenplay
{
    public class CallAnApiTests
    {
        private readonly FakeHttpRequestSender _sender;
        private readonly HttpRequestMessage _request;
        private const string TokenValue = "VALUE";
        public CallAnApiTests()
        {
            this._sender = new FakeHttpRequestSender();
            this._request = HttpRequestFactory.CreateRequest();
        }
        
        [Fact]
        public void should_set_base_url()
        {
            var callAnApi = CallAnApi.At(Urls.Google);

            Check.That(callAnApi.BaseUrl).IsEqualTo(Urls.Google);
        }

        [Fact]
        public async Task should_send_http_request_using_sender()
        {
            var callAnApi = CallAnApi.At(Urls.Google).With(_sender);

            await callAnApi.SendRequest(_request);

            Check.That(_sender.GetLastSentMessage()).IsEqualTo(_request);
        }

        [Fact]
        public async Task should_intercept_requests_with_interceptors()
        {
            var interceptor = FakeHttpInterceptor.SetupToAddHeader(HttpHeaders.Authorization, TokenValue);
            var callAnApi = CallAnApi.At(Urls.Google).With(_sender).WhichRequestsInterceptedBy(interceptor);

            await callAnApi.SendRequest(_request);

            Check.That(_sender.GetLastSentMessage().FirstValueOfHeader(HttpHeaders.Authorization)).IsEqualTo(TokenValue);
        }

        [Fact]
        public async Task should_intercept_requests_with_multiple_interceptors()
        {
            var tokenInterceptor = FakeHttpInterceptor.SetupToAddHeader(HttpHeaders.Authorization, TokenValue);
            var acceptInterceptor = FakeHttpInterceptor.SetupToAddHeader(HttpHeaders.Accept, MediaTypes.ApplicationJson);
            var callAnApi = CallAnApi.At(Urls.Google).With(_sender)
                .WhichRequestsInterceptedBy(tokenInterceptor)
                .WhichRequestsInterceptedBy(acceptInterceptor);

            await callAnApi.SendRequest(_request);

            Check.That(_sender.GetLastSentMessage().FirstValueOfHeader(HttpHeaders.Authorization)).IsEqualTo(TokenValue);
            Check.That(_sender.GetLastSentMessage().FirstValueOfHeader(HttpHeaders.Accept)).IsEqualTo(MediaTypes.ApplicationJson);
        }
        
        [Fact]
        public async Task should_intercept_requests_in_order_of_registration()
        {
            const string sandbox = "Sandbox";
            var firstInterceptor = FakeHttpInterceptor.SetupToAddHeader(sandbox, "test");
            var secondInterceptor = FakeHttpInterceptor.SetupToAddHeader(sandbox, "test test");
            var callAnApi = CallAnApi.At(Urls.Google).With(_sender)
                .WhichRequestsInterceptedBy(firstInterceptor)
                .WhichRequestsInterceptedBy(secondInterceptor);

            await callAnApi.SendRequest(_request);

            Check.That(_sender.GetLastSentMessage().FirstValueOfHeader(sandbox)).IsEqualTo("test");
            Check.That(_sender.GetLastSentMessage().SecondValueOfHeader(sandbox)).IsEqualTo("test test");
        }
    }
}
