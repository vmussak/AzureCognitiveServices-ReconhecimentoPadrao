using ReconhecimentoPadroesDemo.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReconhecimentoPadroesDemo
{
    class Program
    {
        //Chaves que vem do portal do Azure
        const string trainingApiKey = "7c0e4f203da244f89067f4d6dc5aca18";
        const string predictionApiKey = "cd3d1c14247a4138af9f2b1e9695e0a3";

        static void Main(string[] args)
        {
            Console.WriteLine("Instanciando o reconhecedor de padrões...");
            var reconhecedorDePadroes = new ReconhecedorDePadroes(trainingApiKey);

            Console.WriteLine("foi :)");

            Console.WriteLine("Buscando tipos de domínios...");
            IList<Domain> domains = reconhecedorDePadroes.BuscarDominios();

            foreach(var item in domains)
            {
                Console.WriteLine($"{item.Exportable} | {item.Id} | {item.Name}");
            }
            Console.ReadKey();

            Domain generalDomain = domains.First(d => d.Name == "General" && d.Exportable == false);

            Console.WriteLine("Criando um novo projeto...");
            Project project = reconhecedorDePadroes.CriarProjeto($"Mussak_DEMO_{Guid.NewGuid()}",
                                                        "Demo de reconhecimento de padrões :D",
                                                        generalDomain.Id);
            var meuProjeto = project;

            Console.WriteLine("Criando as tags...");
            Tag tagRgFrente = reconhecedorDePadroes.CriarTag(project.Id, "RG_Frente", "Frente do RG");
            Tag tagRgCostas = reconhecedorDePadroes.CriarTag(project.Id, "RG_Costas", "Costas do RG");

            void UploadDasImagens(Tag tag)
            {
                var tagIds = new List<string> { tag.Id.ToString() };
                foreach (var imagePath in Directory.GetFiles($@"C:\Users\Vinicius Mussak\Desktop\cognitive-demos\Imagens\{tag.Name}"))
                {
                    Console.WriteLine($"Preparando upload '{imagePath}'... ");

                    using (var imageStream = new FileStream(imagePath, FileMode.Open))
                        reconhecedorDePadroes.CriarImagem(project.Id, imageStream, tagIds);
                }
            }
            Console.WriteLine("Uploadeando Imagens... ");
            UploadDasImagens(tagRgFrente);
            UploadDasImagens(tagRgCostas);
            Console.WriteLine("Imagens uploadeadas");

            Console.WriteLine("Treinando a porra toda... ");
            Iteration iteration = reconhecedorDePadroes.TreinarProjeto(project.Id);

            while (iteration.Status == "Training")
            {
                Console.Write(".");
                iteration = reconhecedorDePadroes.BuscarIteracao(project.Id, iteration.Id);
            }

            Console.WriteLine("Reconhecendo Padrões... ");
            var reconhecedor = new ReconhecedorDeVerdade(predictionApiKey);

            foreach (var imagePath in Directory.GetFiles($@"C:\Users\Vinicius Mussak\Desktop\cognitive-demos\Imagens\Teste"))
            {
                Console.WriteLine($"\r\n{imagePath}:");

                ResultadoDoReconhecimento resultado;
                using (var imageStream = new FileStream(imagePath, FileMode.Open))
                    resultado = reconhecedor.ReconhecerImagem(project.Id, iteration.Id, imageStream);

                foreach (var tag in resultado.Predictions)
                    Console.WriteLine($"\t{tag.Tag}: {tag.Probability:P2}");
            }

            Console.ReadKey();
        }
    }
}
