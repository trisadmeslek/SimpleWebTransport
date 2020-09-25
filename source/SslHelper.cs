#define SIMPLE_WEB_INFO_LOG
using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

namespace Mirror.SimpleWeb
{
    [Serializable]
    public struct SslConfig
    {
        public bool enabled;
        public string certPath;
        //public string certPassword;
        public bool ClientCertificateRequired;
        public SslProtocols EnabledSslProtocols;
        public bool CheckCertificateRevocation;
    }
    internal class SslHelper
    {
        readonly SslConfig sslConfig;
        readonly X509Certificate2 certificate;

        public SslHelper(SslConfig sslConfig)
        {
            this.sslConfig = sslConfig;
            certificate = new X509Certificate2(sslConfig.certPath, string.Empty);
        }

        internal bool TryCreateStream(Connection conn, SslConfig sslConfig)
        {
            try
            {
                NetworkStream stream = conn.client.GetStream();
                if (sslConfig.enabled)
                {
                    conn.stream = CreateStream(stream);
                    return true;
                }
                else
                {
                    conn.stream = stream;
                    return true;
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
                return false;
            }
        }

        Stream CreateStream(NetworkStream stream)
        {
            // dont need RemoteCertificateValidationCallback for server stream
            SslStream sslStream = new SslStream(stream, true, acceptClient);
            sslStream.AuthenticateAsServer(certificate, false, sslConfig.EnabledSslProtocols, false);

            return sslStream;
        }

        bool acceptClient(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // always accept client
            return true;
        }

        bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Debug.LogErrorFormat("Certificate error: {0}", sslPolicyErrors);
            return false;
        }
    }
}
