
using Google.Apis.Auth.OAuth2;
using Google.Apis.FirebaseCloudMessaging.v1;
using Google.Apis.FirebaseCloudMessaging.v1.Data;
using Google.Apis.Services;

namespace ProductManagement.API.Helpers;
public class FirebaseHelper
{
    public async static Task<bool> SendFcmNotificationAsync(string token, string? title, string? body, string? url, string? imageUrl = null)
    {
        var credentialsJson = @"
        {
            ""type"": ""service_account"",
            ""project_id"": ""ProductManagementapp-69a1f"",
            ""private_key_id"": ""4f0f0a4077a8d2234225dc6b68a7479be8b4dff9"",
            ""private_key"": ""-----BEGIN PRIVATE KEY-----\nMIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQDcKlT5HjjhyXHM\nCZ+lfGzBGX5euTpDqbQ/BVoMimNfFU1HJz2VF+hRSt5H7Jwt2lVNA/WU0kAyMnZ1\n8nZkQiwPKXSMgNsePAZTM0N46k37/9oLdLVoYaNcOdd8m7KHgjC4Tf2h2fcK5rA9\nuzX4jGnfnAoHhfwi0s3PFAEcy54mvYRJ7fVbhJzEFQ1bqqj4d2vpVZUceqWvFSyM\nK1uhND963CZaisF1IPCStGcRyX9OF92T9lqMSaO5evuACBE9f5XJLY4NeX0XI7XC\npqIM53JwC1Q447kM5MB/0eGzrhYnycRhrjjQIkKbqlWDHTgrl86zJCNt0bFAiDWP\ngPVR1pw9AgMBAAECggEACiHIl05Uv89NdV5cAnmfZ6OMtT9A9ywwJ1ZPtpQHzrBk\nRgoAQ4Q+JL+VSyoQZevOUSUFgDvMyIKaAQOPfz2sWWZG/tBPRSzt/ZY2XLIrtJUp\nq61LS+pyYiQnToy7wpCzPUgZjAFD5TSIl86txSpxFOFRdNctq0yYPBdnlbOpK48n\nUvmCJ0zDhDvE59F6u3XTuq8iwWxSZYFdyDHg0xUEbigEljkmAOBOF7NU3R7afrau\n5642LV/l2Jmot7/Ebk6VPxTH8a0VPnz3NkvfvMGS8VCt57PHBRFYMpmez/2LFdNU\nNr8Q+maAj2Wo20mpxL6QXJAqmRwlOQlwGjZlZ0YX4QKBgQD0ltFcaZE1N86qZZzP\nQgDA6YibfcIlgNMH778DdyJeRkP7gcHeJpl3yx0T4OrKXCpYq1xfGPByxxhnrj35\nTd3v4opC3F3I50idHw2hwTyopYlgKLefxxLY/2zP2dQuZvRGRBtzffDyqi8guks7\nN2leaSuQGSmCga7eBYc+v2rU9wKBgQDmb9DJxIvzWLpoFskM5qDWairmjy5UkRRe\nMoVh7dFpbJPcOhSVC3KCSSr4KRW9+qfHmAo03XFdCt9o641Nvnv/u6+ZabinqTek\nPtZIOIrmENIlmeAndgBPvVrDn8M8WRMc1f7DKNCi9xCTF2AojEJgZ83d6tUkHrVB\nf+rOiUvvawKBgQCR+BAqXrZSVGP6PXpAFtvsRXNJmNvtFdzbt7D/mHHiYXzg9KeG\ntPKSE53e5zQ4YJjFIj+WYYPefAOMnOhPiLETINfy3yD5bU5gts+j4XcaDvDVendb\n26rb5lRCF3YR20+dP7hCTL8nk3lcxtFvHbq8pXdiICItyMdbqWD1ibdwMQKBgChk\nsVSs/1tI0mQsmlGIm+j580s3DggE5m0crgTcizjPr5uLEtCNkSUFbmLR0N3cckyq\neh+KMbT9kuvxXnOiUFojrERYI+Y/42EHIsjGIJLzn+K3dnN0W9med6qS9sY9VvxX\nBrFvFiN664zdLB3veDMt/whHgdUzDNLOeXp61rTBAoGBAM8zjzEO2aWusk4E6k7F\nWJJ+9ODKkxeu5FXmuO/tft4ZlHGdWAFTtEkxQ/ie427UoQFsHtbtu2UFfNZsVSPF\n4l2l/MJApqlSmfN/kobceUt+vQmpXBOMTyCsYQorQ9kGMHVYkqQXk/AcRbj5ZqUP\nTijDgWgWvJxmx9/t84VfVVnX\n-----END PRIVATE KEY-----\n"",
            ""client_email"": ""firebase-adminsdk-omre6@ProductManagementapp-69a1f.iam.gserviceaccount.com"",
            ""client_id"": ""116098080831925242433"",
            ""auth_uri"": ""https://accounts.google.com/o/oauth2/auth"",
            ""token_uri"": ""https://oauth2.googleapis.com/token"",
            ""auth_provider_x509_cert_url"": ""https://www.googleapis.com/oauth2/v1/certs"",
            ""client_x509_cert_url"": ""https://www.googleapis.com/robot/v1/metadata/x509/firebase-adminsdk-omre6%40ProductManagementapp-69a1f.iam.gserviceaccount.com"",
            ""universe_domain"": ""googleapis.com""
        }
    ";

        var credential = GoogleCredential.FromJson(credentialsJson);
        FirebaseCloudMessagingService service = new(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "ProductManagement"
        });

        var message = new Message()
        {
            Token = token,
            Notification = new Notification()
            {
                Title = title,
                Body = body,
                Image = imageUrl,
            },
            Webpush = new WebpushConfig()
            {
                FcmOptions = new WebpushFcmOptions()
                {
                    Link = url
                }
            }
        };

        var sendMessageRequest = new SendMessageRequest
        {
            Message = message,
        };

        try
        {
            var response = await service.Projects.Messages.Send(sendMessageRequest, "projects/ProductManagementapp-69a1f").ExecuteAsync();
            if (response.Name != null)
                return true;
            else
                return false;
        }
        catch
        {
            return false;
        }
    }
}
