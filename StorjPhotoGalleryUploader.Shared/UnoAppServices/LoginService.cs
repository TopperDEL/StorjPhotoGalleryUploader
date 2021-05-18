using StorjPhotoGalleryUploader.Contracts.Interfaces;
using StorjPhotoGalleryUploader.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.Security.Credentials;

namespace StorjPhotoGalleryUploader.UnoAppServices
{
    public class LoginService : ILoginService
    {
        private const string STORJPHOTOGALLERY_RESOURCE = "STORJPHOTOGALLERY";
        private const string ACCESS_GRANT = "ACCESS_GRANT";
        private const string BUCKET_NAME = "BUCKET_NAME";

        private PasswordVault _vault = new PasswordVault();

        public bool GetIsLoggedIn()
        {
            try
            {
                var access = GetLogin();
                if (!string.IsNullOrEmpty(access.AccessGrant))
                    return true;
            }
            catch
            {
                //Whatever happened - consider the user as not logged in
            }
            return false;
        }

        public AppConfig GetLogin()
        {
            return new AppConfig(_vault.Retrieve(STORJPHOTOGALLERY_RESOURCE, ACCESS_GRANT)?.Password, _vault.Retrieve(STORJPHOTOGALLERY_RESOURCE, BUCKET_NAME)?.Password);
        }

        public bool Login(AppConfig appConfig)
        {
            //ToDo: Verify if access is valid

            //Save access grant to vault
            _vault.Add(new PasswordCredential(STORJPHOTOGALLERY_RESOURCE, ACCESS_GRANT, appConfig.AccessGrant));
            _vault.Add(new PasswordCredential(STORJPHOTOGALLERY_RESOURCE, BUCKET_NAME, appConfig.BucketName));

            return true;
        }

        public void Logout()
        {
            var list = _vault.FindAllByResource(STORJPHOTOGALLERY_RESOURCE);
            foreach (var entry in list)
                _vault.Remove(entry);
        }
    }
}
