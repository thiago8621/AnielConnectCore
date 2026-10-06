using Google.Apis.Auth.OAuth2;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aniel.Connect.Core.Service
{
    public class CloudStorageService
    {
        private string credentialJson { get; set; }
        private string projectId { get; set; }
        private bool isJsonPath { get; set; }

        public CloudStorageService(string credentialJson, string projectId, bool isJsonPath = false)
        {           
            this.credentialJson = credentialJson;          
            this.projectId = projectId;
            this.isJsonPath = isJsonPath;
        }

        public async Task<string> UploadArquivoEmNuvem(string nomeArquivo, string mimeType, MemoryStream arquivoStream, string bucketName)
        {
            string urlArquivo = String.Empty;
           
            GoogleCredential credential = this.isJsonPath ? AuthenticateByJsonPath(this.credentialJson) : AuthenticateByJson(this.credentialJson);
            using (var client = StorageClient.Create(credential))
            {
                var bucket = await this.GetBucketAsync(client, this.projectId, bucketName);                 
                var retorno = await client.UploadObjectAsync(bucket.Name, nomeArquivo, mimeType, arquivoStream);

                urlArquivo = getFilePublicUrl(bucketName, nomeArquivo);                
            }
            
            return urlArquivo;
        }

        public async Task<MemoryStream> DownloadArquivoEmNuvem(string nomeArquivo, string bucketName)
        {           
            var retorno = new MemoryStream();  
            
            GoogleCredential credential = this.isJsonPath ? AuthenticateByJsonPath(this.credentialJson) : AuthenticateByJson(this.credentialJson);
            using (var client = StorageClient.Create(credential))
            {
                var bucket = await this.GetBucketAsync(client, this.projectId, bucketName);
                await client.DownloadObjectAsync(bucket.Name, nomeArquivo, retorno);
            }
                    
            return retorno;
        }

        private GoogleCredential AuthenticateByJson(string json)
        {     
            //Gera credencial a partir de um json
            return GoogleCredential.FromJson(json);                
        }

        private GoogleCredential AuthenticateByJsonPath(string jsonPath)
        {
            //Gera credencial a partir de um arquivo .json em disco
            return GoogleCredential.FromFile(jsonPath);
        }

        private async Task<Bucket> GetBucketAsync(StorageClient client, string projectId, string bucketName)
        {
            Bucket bucket = null;
            string bucketNameCustomizado = $"{bucketName}-{projectId}";
            try
            {
                // Busca o Bucket na conta do Cloud Storage
                bucket = await client.GetBucketAsync(bucketNameCustomizado);
            }            
            catch(Exception ex)
            {
                if(ex is Google.GoogleApiException && (ex as Google.GoogleApiException).HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    //Se não existe Bucket, cria um
                    if (bucket == null)
                    {
                        var options = new CreateBucketOptions { PredefinedDefaultObjectAcl = PredefinedObjectAcl.PublicRead };
                        bucket = await client.CreateBucketAsync(projectId, bucketNameCustomizado, options);
                    }
                }
                else
                {
                    throw;
                }
            }                                           
            return bucket;
        }      

        public string getFilePublicUrl(string bucketName, string nomeArquivo)
        {
            //A API não fornece nenhum método que permita obter a url public do arquivo
            //Por isso é necessário montar a url manualmente no seguinte padrão:
            //http(s)://storage.googleapis.com/[bucket]/[nome_do_arquivo_com_extensao]
            string bucketNameCustomizado = $"{bucketName}-{this.projectId}";
            return $"https://storage.googleapis.com/{bucketNameCustomizado}/{nomeArquivo}";
        }

        public static string getJsonDebug()
        {
            return "{\"type\":\"service_account\",\"project_id\":\"anielconnect-356113\",\"private_key_id\":\"5aff4e8b189a5a6458e5f8de1cde0542c8037f9b\",\"private_key\":\"-----BEGINPRIVATEKEY-----\\nMIIEvAIBADANBgkqhkiG9w0BAQEFAASCBKYwggSiAgEAAoIBAQCOR/2MXv42+Mvp\\n3+LlHzLEQ9ZRwFRk0R0jZolaumFAKYQCaPs+rqMF12GJTwvs4j8hycfK9HUAsYA0\\nTGvJJ5gLW7VIoF5ItGtyjdpgVEpVlhKPARLIxZU97Uoek2Nfg3DvGSJtQdn5EXdz\\nJyrkP18xQrB9PotJFeiM1/lby+EMYTEJs1FI2fwFqTjvdc1A9p7PV8jQj7ypFAhJ\\nEmzXqk8pLliuxVvvNbdjwVZoaImynf2ML6p/vTkqrYuuiq4M9tm+KsjZEGvtqKYB\\nraL5at//5GutB4NbrGzzQaCFkzBNGBaEt3u4MYyjSSLoUyhOZyEzqujncIgwJa82\\nwYyMyQ29AgMBAAECggEABvCK9NhDRPIgnAEgA3Xgl4MJhoY/d4Vdivb61xKFIMiF\\nmEp/iqCg8e194+EXn0+vrZm2ZXdD56tAqgxaK38DYvZvoqGzx48JOJajopQp5f/C\\nWvg97s34SAN1XIVq0ONL1FWhALw4AfZwoYy1Aln27iOnEQzGZWOsqoX0Mx6AFQbr\\n6xXnQJaSGbYZZJE+ybgdWLhxjppkPt4wZPiDHXRDalFINzK6CpUBKCtFcnWpeXuA\\nJH8FevojgzPpJ4GDQaq5EX8c+gitTOjNeA5cDkQaFQJ9MvuMyENj3mh7E+gWkUh3\\nhPSaOUStdgCQ3ss2xdWDfmV3wmkxqnharAhZfS2V7wKBgQDCQpKHq8Gc2U85sKEf\\nLz2eqUeAGDgsSLOFR3uExgz6h3t9LEIVjCBKI5soSbNTEXJrr08fZczQCXHmgzY+\\nnx3OCZtW1KNB8OCSBQMjgpd3xWKsGG/INXzj9CTrhLlBbgBkU1PiqAPsG727KFOb\\n909QhPVpKBFfp+H/U4sQu7CtrwKBgQC7gE4Mzs3BTEpfIxHCIk7/1280p4cbqwYc\\npDb+p/0gNTRGv6oO5tL5lxcIx9gYyQ5JqgIkWVvjmLCVo5FmpSZkOX2/h8H9c6nQ\\nkIV9DS16UcQ+j8R6ks+KIkhbHTBjxQVzsNa1n/GAwqWk6HnL7Qi7gamscjYG9mIb\\n5Q2Y4uiiUwKBgA86CqRJyT5TJkueVbLNECZsnGck/toxxKiQxtTMdV352qsgGfn4\\nZtuI2POae0zqlC5uqmQKpbg01VG9PTZBYKOyyxNOTZGwoNmxPOK+rMK9nRNiQvjN\\ngIGZtH8lgyPmzG8QvQWKJ+yM7B1MdulFxFjKYzYf5uS5kxRhZwgjviCPAoGATNlo\\npA06/cf0D4xDIguHz4qQFMe5M5jkN2cC91sI/2NvGrRB0ju/KvdHRJ0MMBRv27dn\\ni1hoth8F+T8CQEzeMGPz60WMOAJpDoe9fqShCCptQasRP5/emmI0v8t14t6Evocz\\naVu10y10UOPLz85GLvwU/A4PoMjpKz+1U/1klesCgYBd0gnVqjEaG+1rXKI67ezv\\nniLSEqNSoejTQsqe9WJoxHYyBCEhLwCt4OdDzlydtgGPOAodfdIuQWzlE1ezBmq6\\nanfBXbH5Ui8pdqG4qyBy+pzRNfQoGdaM8YbeFpEmc/6XnsGhAVhxFIeSA/NB9ev5\\nhGqByqS99fBAsnj7amWfYQ==\\n-----ENDPRIVATEKEY-----\\n\",\"client_email\":\"aniel-cloud-storage-service@anielconnect-356113.iam.gserviceaccount.com\",\"client_id\":\"114807768191269054464\",\"auth_uri\":\"https://accounts.google.com/o/oauth2/auth\",\"token_uri\":\"https://oauth2.googleapis.com/token\",\"auth_provider_x509_cert_url\":\"https://www.googleapis.com/oauth2/v1/certs\",\"client_x509_cert_url\":\"https://www.googleapis.com/robot/v1/metadata/x509/aniel-cloud-storage-service%40anielconnect-356113.iam.gserviceaccount.com\"}";
        }

        public static string getProjectIdDebug()
        {
            return "anielconnect-356113";
        }      
    }
}
