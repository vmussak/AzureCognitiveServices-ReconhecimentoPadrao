using ReconhecimentoPadroesDemo.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace ReconhecimentoPadroesDemo
{
    public class ReconhecedorDePadroes
    {
        private string _key;
        private string uri = "https://southcentralus.api.cognitive.microsoft.com/customvision/v1.1/Training/";
        private HttpClient _client;

        public ReconhecedorDePadroes(string key)
        {
            _key = key;
            _client = new HttpClient
            {
                BaseAddress = new Uri(uri)
            };

            _client.DefaultRequestHeaders.Add("Training-key", _key);
        }

        public IList<Domain> BuscarDominios()
        {
            var response = _client.GetAsync("domains").Result;

            var result = response.Content.ReadAsStringAsync().Result;

            var dominios = JsonConvert.DeserializeObject<IList<Domain>>(result);

            return dominios;
        }

        public Project CriarProjeto(string nome, string descricao, string domainId)
        {
            var response = _client.PostAsync($"projects?name={nome}&description={descricao}&domainId={domainId}", null).Result;

            var result = response.Content.ReadAsStringAsync().Result;

            var projeto = JsonConvert.DeserializeObject<Project>(result);

            return projeto;
        }

        public Tag CriarTag(string projectId, string nome, string descricao)
        {
            var response = _client.PostAsync($"projects/{projectId}/tags?name={nome}&description={descricao}", null).Result;

            var result = response.Content.ReadAsStringAsync().Result;

            var tag = JsonConvert.DeserializeObject<Tag>(result);

            return tag;
        }

        public void CriarImagem(string projectId, FileStream imageStream, List<string> tagIds)
        {
            using (var content = new ByteArrayContent(ToByteArray(imageStream)))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                var response = _client.PostAsync($"projects/{projectId}/images/image?tagIds={string.Join(",", tagIds)}", content).Result;

                var result = response.Content.ReadAsStringAsync().Result;
            }
        }

        public Iteration TreinarProjeto(string projectId)
        {
            var response = _client.PostAsync($"projects/{projectId}/train", null).Result;

            var result = response.Content.ReadAsStringAsync().Result;

            var iteration = JsonConvert.DeserializeObject<Iteration>(result);

            return iteration;
        }

        public Iteration BuscarIteracao(string projectId, string iterationId)
        {
            var response = _client.GetAsync($"projects/{projectId}/iterations/{iterationId}").Result;

            var result = response.Content.ReadAsStringAsync().Result;

            var iteration = JsonConvert.DeserializeObject<Iteration>(result);

            return iteration;
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
