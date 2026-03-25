using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

// Load certificate
var certPath = Path.Combine(AppContext.BaseDirectory, "client-certificate.p12");
var certificate = new X509Certificate2(
    certPath,
    "insert_password_here"
);

int numberOfRequests = 4;

for (int i = 0; i < numberOfRequests; i++)
{
    //Wait a few seconds so Wireshark shows a clear separation between Socket connections
    System.Threading.Thread.Sleep(5000);

    using var handler = new SocketsHttpHandler
    {
        SslOptions = new SslClientAuthenticationOptions
        {
            ClientCertificates = new X509CertificateCollection { certificate },
            EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12,

            // For testing only: disables server certificate checks
            RemoteCertificateValidationCallback = (sender, serverCert, chain, sslPolicyErrors) => true
        },
        
        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(10),
        PooledConnectionLifetime = TimeSpan.FromMinutes(10),
        MaxConnectionsPerServer = 1
    };

    using var client = new HttpClient(handler)
    {
        Timeout = TimeSpan.FromSeconds(3)
    };

    var request = new HttpRequestMessage(HttpMethod.Get, "https://here.goes.the.api.url");

    request.Headers.Add("apikey", "api_key_goes_here");
    request.Headers.Add("Accept", "application/hal+json");
    request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
    request.Headers.Add("x-origin-oin", "origin_oin_number_goes_here");

    try
    {
        Console.WriteLine($"Request {i}");

        var response = client.Send(request);
        response.EnsureSuccessStatusCode();

        Console.WriteLine("Success");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"FAILED: {ex.Message}");
    }
    Console.WriteLine("");
}
