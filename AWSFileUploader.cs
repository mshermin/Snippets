using Amazon.S3;
using SampleCore.Services.ConfigProvider;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;


namespace SampleCore.Services
{
    public class AWSFileUploader : IFileUploader
    {
        
        private IConfigProvider _Config { get; set; }
        private AmazonS3Client _S3Client = null;

        public AWSFileUploader(AmazonS3Client client, IConfigProvider config)
        {
            _Config = config;
            _S3Client = client;
        }

        public string UploadFileAndGetURL(IFormFile fileToUpload)
        {
            if (fileToUpload.Length <= 0) {
                throw new IOException("File is empty");      
            }

            var url = string.Empty;
            var dir = Path.GetTempPath();
            try
            {
                if (fileToUpload != null)
                {
                    var filename = fileToUpload.FileName;
                    var path = Path.Combine(dir, filename);
                   
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        fileToUpload.CopyTo(stream);
                        stream.Flush();
                    }

                    url = _UploadToS3(path);

                }

               
            }
            catch (IOException ex) {
                throw ex;
            }

            return url;
        }

        private string _UploadToS3(string path)
        {
            Amazon.S3.Model.PutObjectRequest s3PutRequest = new Amazon.S3.Model.PutObjectRequest();
            s3PutRequest = new Amazon.S3.Model.PutObjectRequest
            {
                BucketName = _Config.AWSS3Bucket,
                CannedACL = S3CannedACL.PublicRead,
                Key = Guid.NewGuid().ToString(),
                FilePath = path
            };
            _S3Client.PutObjectAsync(s3PutRequest);
            return $"https://s3.amazonaws.com/{s3PutRequest.BucketName}/{s3PutRequest.Key}"; 
        }
        
         private Amazon.S3.AmazonS3Client GetAwsS3Client(IConfigProvider configProvider)
        {
            var accessKey = configProvider.AWSAccessKey;
            var secretAccessKey = configProvider.AWSSecretAccessKey;
            var s3Bucket = configProvider.AWSS3Bucket;
            var serviceUrl = configProvider.AWSS3ServiceUrl;

            Amazon.S3.AmazonS3Config s3Config = new Amazon.S3.AmazonS3Config();
            s3Config.ServiceURL = serviceUrl;
            
            return new Amazon.S3.AmazonS3Client(accessKey, secretAccessKey, s3Config);
        }
    }
}
