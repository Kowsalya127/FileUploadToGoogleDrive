using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AssessmentFileUpload
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {

            OpenFileDialog objOpnFD = new OpenFileDialog();
            objOpnFD.Multiselect = true;
            objOpnFD.ShowDialog();
            objOpnFD.Filter = "PDF Files|*.pdf";

            txtFileUpload.Text = objOpnFD.FileName;

            //For pdf file validation
            string strFileExt = System.IO.Path.GetExtension(objOpnFD.FileName).ToLower();
            if (strFileExt != ".pdf")
            {
                MessageBox.Show("Please upload PDF files");
                txtFileUpload.Text = string.Empty;
                return;
            }
            //For ask user to authorize our app
            UserAuthorization();

        }

        private void UserAuthorization()
        {
            string[] strArrScopes = new string[] { DriveService.Scope.Drive, DriveService.Scope.DriveFile, };

            // Credentials get From https://console.developers.google.com
            var clientId = "400614179863-m0rmv33mrtop1d96ie862e0rjigdnaus.apps.googleusercontent.com";
            var clientSecret = "EDIXCRblWtDUZ62nyWkY5Y8A";

            // here is where we Request the user to give us access, or use the Refresh Token that was previously stored in %AppData%  
            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            }, strArrScopes,
            Environment.UserName, CancellationToken.None, new FileDataStore("MyAppsToken")).Result;
            //Once consent is recieved, your token will be stored locally on the AppData directory, so that next time you wont be prompted for consent.   

            DriveService objService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "AssessmentFileUpload",

            });
            objService.HttpClient.Timeout = TimeSpan.FromMinutes(180);
            //Long Operations like file uploads might timeout. 180 is just precautionary value, can be set to any reasonable value depending on what you use your service for  

            var strResponse = FileUploadtoGD(objService, txtFileUpload.Text, "");
            MessageBox.Show("File Uploaded to your Google Drive successfully!");
        }

        // Calling webservice Google.Apis.Drive.v3
        public Google.Apis.Drive.v3.Data.File FileUploadtoGD(DriveService objService, string strUploadFile, string strDescription = "Uploaded with .NET!")
        {
            if (System.IO.File.Exists(strUploadFile))
            {
                string strMIMEType = GetMIMEType(strUploadFile);

                Google.Apis.Drive.v3.Data.File objFileBody = new Google.Apis.Drive.v3.Data.File();
                objFileBody.Name = System.IO.Path.GetFileName(strUploadFile);
                objFileBody.Description = strDescription;
                objFileBody.MimeType = strMIMEType;
                byte[] byteArray = System.IO.File.ReadAllBytes(strUploadFile);
                System.IO.MemoryStream objStream = new System.IO.MemoryStream(byteArray);
                try
                {
                    FilesResource.CreateMediaUpload objFileUpload = objService.Files.Create(objFileBody, objStream, strMIMEType);
                    objFileUpload.SupportsTeamDrives = true;
                    objFileUpload.Upload();
                    return objFileUpload.ResponseBody;
                }
                catch (Exception e)
                {
                    {
                        MessageBox.Show(e.Message, "Error Occured");
                        return null;
                    }
                }
            }
            else
            {
                MessageBox.Show("The file does not exist.", "404");
                return null;
            }
        }

        private static string GetMIMEType(string strFileName)
        {
            string strMIMEType = "application/unknown";
            string strFileExt = System.IO.Path.GetExtension(strFileName).ToLower();
            Microsoft.Win32.RegistryKey objRegKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(strFileExt);
            if (objRegKey != null && objRegKey.GetValue("Content Type") != null)
            {
                strMIMEType = objRegKey.GetValue("Content Type").ToString();
            }
            System.Diagnostics.Debug.WriteLine(strMIMEType);
            return strMIMEType;
        }

    }
}
