using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace Clip2Text
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            ClipboardNotification.ClipboardUpdate += ClipboardNotification_ClipboardUpdate;
        }

        private void ClipboardNotification_ClipboardUpdate(object sender, EventArgs e)
        {
            var clipboardImage = Clipboard.GetImage();
            if (clipboardImage == null) return;

            using (var stream = new MemoryStream())
            {
                clipboardImage.Save(stream, ImageFormat.Png);
                var bytes = stream.GetBuffer();

                var postParameters = new Dictionary<string, object>();
                postParameters.Add("i2ocr_languages", "gb,eng");
                postParameters.Add("i2ocr_options", "file");
                postParameters.Add("i2ocr_url", "http://");
                postParameters.Add("i2ocr_uploadedfile", new FormUpload.FileParameter(bytes, "temp.png", "image/png"));

                // Create request and receive response
                var postURL = "http://www.i2ocr.com/process_form";
                string userAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.110 Safari/537.36";
                using (var webResponse = FormUpload.MultipartFormDataPost(postURL, userAgent, postParameters))
                {
                    // Process response
                    StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
                    string fullResponse = responseReader.ReadToEnd();

                    var resultBegin = fullResponse.IndexOf("ocrTextBox") + 18;
                    var resultEnd = fullResponse.IndexOf("iImgLoader") - 20;
                    var result = fullResponse.Substring(resultBegin, resultEnd - resultBegin);
                    textBox1.Text = result.Replace("\\n\\n", "\\n").Replace("\\n", "\r\n").Replace("\\", "");
                }
            }
        }
    }
}
