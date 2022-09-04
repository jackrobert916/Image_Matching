using System;
using System.Net.Http;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Text.Json;
using RestSharp;
using Newtonsoft.Json.Linq;
using System.Drawing.Imaging;

namespace WinFormsApp1
{

    public partial class Form1 : Form
    {
        private string encoded_image;
        private string path;
        private PictureBox pb1;
        private Label lb;
        public Form1()
        {
            InitializeComponent();
            this.encoded_image = "";
            string folder_name = Application.StartupPath + "/image_matching";
            System.IO.Directory.CreateDirectory(folder_name);

            this.pb1 = new PictureBox();
            pb1.Location = new Point(50, 50);
            pb1.Size = new System.Drawing.Size(500, 500);
            pb1.SizeMode = PictureBoxSizeMode.StretchImage;

            this.lb = new Label();
            lb.Location = new Point(450, 20);
            lb.Size = new System.Drawing.Size(200, 20);
        }

        private void button2_Click(object sender, EventArgs e)
        {

            string image = "";
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp";
            DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                image = System.IO.Path.GetFullPath(openFileDialog1.FileName);
                string path = openFileDialog1.FileName.Split(".")[0];


            byte[] imageArray = System.IO.File.ReadAllBytes(image);
            this.encoded_image = Convert.ToBase64String(imageArray);

                this.pb1.Image = Image.FromFile(image);
                this.Controls.Add(this.pb1);

                string folder_name = Application.StartupPath;
                string path_string = System.IO.Path.Combine(folder_name, "image_matching");
                string filepath = System.IO.Path.Combine(path_string, path);             
                System.IO.Directory.CreateDirectory(filepath);
            }


        }

        private async void button1_Click(object sender, EventArgs e)
        {
            this.lb.Text = "Matching...";
            this.Controls.Add(this.lb);

            var client = new RestClient("https://vision.googleapis.com/");
            var body = @"{
" + "\n" +
@"  ""requests"": [
" + "\n" +
@"    {
" + "\n" +
@"      ""image"": {
" + "\n" +
@"        ""content"": """ + this.encoded_image + "\"" +
 "\n" +
@"      },
" + "\n" +
@"      ""features"": [
" + "\n" +
@"        {
" + "\n" +
@"          ""maxResults"": 10," + "\n" +
@"          ""type"": ""WEB_DETECTION""
" + "\n" +
@"        },
" + "\n" +
@"      ]
" + "\n" +
@"    }
" + "\n" +
@"  ]
" + "\n" +
@"}";
        
            var request = new RestRequest("v1/images:annotate?key=AIzaSyBXPx8pIIWLMQ6L1yIhbBoz3cW0-Um3lIQ").AddJsonBody(body);
            var response = await client.ExecutePostAsync(request);
            var check = response.IsSuccessful;

            JObject res_body = JObject.Parse(response.Content);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                try
                {
                    var partial_urls = res_body["responses"].Value<JArray>()[0]["webDetection"].Value<JObject>()["visuallySimilarImages"].Value<JArray>();
                    int num = 0;
                    foreach (JObject ele in partial_urls)
                    {
                        var url = ele["url"].Value<string>();
                        string file_name = url.Split("/")[url.Split("/").Length - 1].Split("?")[0];
                            num++;
                        if (num == 1)
                        {
                            this.lb.Text = "Matched " + num.ToString() + " image.";
                            this.Controls.Add(this.lb);
                        }
                        else
                        {
                            this.lb.Text = "Matched " + num.ToString() + " images.";
                            this.Controls.Add(this.lb);

                        }
                        try
                        {
                            WebClient webClient = new WebClient();
                            webClient.DownloadFile(url, "image_matching/" + this.path + "/" + file_name);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    this.lb.Text = "Matching finished.";
                    this.Controls.Add(this.lb);
                }
                catch
                {
                    this.lb.Text = "Nothing matched";
                    this.Controls.Add(this.lb);
                }
                
                /*Label instead_console = new Label();

                instead_console.Text = partial_urls.ToString();
                instead_console.Location = new Point(0, 0);
                instead_console.AutoSize = true;
                instead_console.Font = new Font("Calibri", 18);
                instead_console.BorderStyle = BorderStyle.Fixed3D;
                instead_console.ForeColor = Color.Green;
                instead_console.Padding = new Padding(6);
                this.Controls.Add(instead_console);*/
            }

        }

    }
}
