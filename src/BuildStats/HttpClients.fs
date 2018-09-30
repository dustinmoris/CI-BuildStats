module BuildStats.HttpClients

open System
open System.Net
open System.Net.Http
open System.Threading.Tasks
open Microsoft.Extensions.Logging
open FSharp.Control.Tasks.V2.ContextInsensitive
open System.Net.Http.Headers

exception BrokenCircuitException

type IResilientHttpClient =
    abstract member SendAsync : HttpRequestMessage -> Task<HttpResponseMessage>

type BaseHttpClient (clientFactory : IHttpClientFactory) =
    interface IResilientHttpClient with
        member __.SendAsync (request : HttpRequestMessage) =
            let client = clientFactory.CreateClient()
            client.SendAsync request

type CircuitBreakerHttpClient (httpClient        : IResilientHttpClient,
                               logger            : ILogger<CircuitBreakerHttpClient>,
                               minBreakDuration  : TimeSpan) =

    let mutable isBrokenCircuit = false
    let mutable brokenSince     = DateTime.MinValue

    let getBreakDuration (response : HttpResponseMessage) =
        match response.Headers.RetryAfter.Delta.HasValue with
        | true  -> max minBreakDuration response.Headers.RetryAfter.Delta.Value
        | false -> minBreakDuration

    let isClientError (status : HttpStatusCode) =
        let clientErrorCodes =
            [ 400; 401; 402; 403; 404; 405; 406; 407; 409; 410;
              411; 412; 413; 414; 415; 416; 417; 418; 421; 422;
              423; 424; 426; 428; 431; 444; 451; 499 ]
        clientErrorCodes |> List.contains ((int)status)

    interface IResilientHttpClient with
        member __.SendAsync (request : HttpRequestMessage) =
            task {
                match isBrokenCircuit with
                | true  ->
                    let brokenDuration = DateTime.Now - brokenSince
                    isBrokenCircuit <- brokenDuration <= minBreakDuration
                    return raise BrokenCircuitException
                | false ->
                    let! response = httpClient.SendAsync request
                    match response.IsSuccessStatusCode || isClientError response.StatusCode with
                    | true  -> return response
                    | false ->
                        let breakDuration = getBreakDuration response
                        logger.LogWarning("Request to '{url}' has failed. Breaking circuit for: {seconds}sec.", request.RequestUri, breakDuration.TotalSeconds)
                        brokenSince     <- DateTime.Now
                        isBrokenCircuit <- true
                        return response
            }

type RetryHttpClient (httpClient        : IResilientHttpClient,
                      logger            : ILogger<RetryHttpClient>,
                      maxRetries        : int) =

    let getWaitDuration (retryCount : int) =
        TimeSpan.FromSeconds(Math.Pow(2.0, float retryCount))

    let isWorthRetrying (status : HttpStatusCode) =
        status = HttpStatusCode.RequestTimeout
        || status = HttpStatusCode.BadGateway
        || status = HttpStatusCode.ServiceUnavailable
        || status = HttpStatusCode.GatewayTimeout

    let rec sendAsync (request : HttpRequestMessage) (retryCount : int) : Task<HttpResponseMessage> =
        task {
            let! response = httpClient.SendAsync request

            match response.IsSuccessStatusCode with
            | true  -> return response
            | false ->
                match retryCount > 0 && isWorthRetrying response.StatusCode with
                | false -> return response
                | true  ->
                    let waitDuration = getWaitDuration retryCount
                    logger.LogWarning(
                        "Request to '{url}' has failed. The response status code was: {statusCode}. Max retries left: {retryCount}. Next wait duration: {seconds}sec.",
                        request.RequestUri, response.StatusCode, retryCount, waitDuration.TotalSeconds)
                    do! Task.Delay waitDuration
                    return! sendAsync request (retryCount - 1)
        }

    interface IResilientHttpClient with
        member __.SendAsync (request : HttpRequestMessage) =
            sendAsync request maxRetries

type FallbackHttpClient (httpClient : IResilientHttpClient,
                         logger     : ILogger<FallbackHttpClient>) =

    let isClientError (status : HttpStatusCode) =
        let clientErrorCodes =
            [ 400; 401; 402; 403; 404; 405; 406; 407; 409; 410;
              411; 412; 413; 414; 415; 416; 417; 418; 421; 422;
              423; 424; 426; 428; 431; 444; 451; 499 ]
        clientErrorCodes |> List.contains ((int)status)

    member __.SendAsync (request : HttpRequestMessage) =
        task {
            try
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"))
                let! response = httpClient.SendAsync request
                match response.StatusCode with
                | HttpStatusCode.OK -> return! response.Content.ReadAsStringAsync()
                | _                 ->
                    if isClientError response.StatusCode then
                        logger.LogWarning("Request to '{url}' has failed due to a HTTP client error: {statusCode}.", request.RequestUri, response.StatusCode)
                    return ""
            with
                | :? BrokenCircuitException -> return ""
                | ex ->
                    logger.LogWarning(
                        "Exception thrown when sending a HTTP request to '{url}': {message}. Stack Trace: {stackTrace}.", request.RequestUri, ex.Message, ex.StackTrace)
                    return ""
        }