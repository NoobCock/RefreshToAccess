using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;

namespace RefreshToAccess
{
    internal class IGNRename
    {

        public static MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

        public static void Rename(string newName)
        {
            request(newName);
        }
        
        public static async void request(string name)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    string url = "https://api.minecraftservices.com/minecraft/profile/name/"+name;
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", MainWindow.accessToken); 
                    StringContent content = new StringContent("{}", Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await httpClient.PutAsync(url, content);
                    string body = await response.Content.ReadAsStringAsync();
                    int code = (int)response.StatusCode;
                    if (code == 401)
                    {
                        throw new Exception("Invalid token");
                    }
                    if (code == 429)
                    {
                        throw new Exception("You are changing the name too often, wait a moment and try again");
                    }
                    else
                    {
                        if (body.Contains("FORBIDDEN"))
                        {
                            throw new Exception("You'll need to wait for 30 days before changing your name again");
                        }
                        if (body.Contains("DUPLICATE"))
                        {
                            throw new Exception("This name is already used by someone else.");
                        }
                        if (body.Contains("NOT_ALLOWED"))
                        {
                            throw new Exception("This name is not allowed, try something else");
                        }
                        if (code == 400)
                        {
                            throw new Exception("Invalid name format");
                        }
                        if (code == 200)
                        {
                            MainWindow.profileName=name;
                            mainWindow.IGNBox.Text=name;
                            MessageBox.Show("Sucessfully renamed profile name to: "+name);
                        }
                        else
                        {
                            throw new Exception("Error");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message=="Error")
                {
                    MessageBox.Show("Something went wrong...");
                    return;
                }
                MessageBox.Show(e.Message);
            }
        }
    }
}
