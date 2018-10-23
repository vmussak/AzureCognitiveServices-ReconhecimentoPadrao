using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ReconhecimentoPadroesDemo.Models;

namespace ReconhecimentoPadroesDemo
{
    public class ReconhecedorDeVerdade
    {
        //
        private string _key;
        private string uri = "https://southcentralus.api.cognitive.microsoft.com/customvision/v1.1/Prediction/";
        private HttpClient _client;

        public ReconhecedorDeVerdade(string key)
        {
            _key = key;
            _client = new HttpClient
            {
                BaseAddress = new Uri(uri)
            };

            _client.DefaultRequestHeaders.Add("Prediction-key", _key);
        }

        public ResultadoDoReconhecimento ReconhecerImagem(string projectId, string iterationId, FileStream imageStream)
        {
            using (var content = new ByteArrayContent(ToByteArray(imageStream)))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                var response = _client.PostAsync($"{projectId}/image?iterationId={iterationId}", content).Result;

                var result = response.Content.ReadAsStringAsync().Result;

                var tudoReconhecido = JsonConvert.DeserializeObject<ResultadoDoReconhecimento>(result);

                return tudoReconhecido;
            }
        }

        private byte[] ToByteArray(FileStream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}
