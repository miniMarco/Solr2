﻿using CommonServiceLocator;
using Solr.Models;
using SolrNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Solr
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            Startup.Init<Artigo>("http://localhost:8983/solr/core");
            Startup.Init<Artigo>("http://localhost:8983/solr/core2");
            Startup.Init<Artigo>("http://localhost:8983/solr/core3");

            /*** Comentar as linhas para não subir mais artigos para o solr ******/
            //List<Artigo> artigos = processarArquivo();
            //recuperarSumarios(artigos);
            //subirArtigosNoSolr(artigos);
        }

        private List<Artigo> processarArquivo()
        {
            try
            {
                StreamReader arquivoOriginal = new StreamReader("c:/Arquivo/normal.txt", Encoding.Default);

                string nomeDoCluster = string.Empty;
                List<Artigo> artigos = new List<Artigo>();

                string linha = string.Empty;

                while ((linha = arquivoOriginal.ReadLine()) != null)
                {
                    if (linha.Contains("cluster") && !linha.Contains("Log"))
                    {
                        nomeDoCluster = linha;
                        continue;
                    }

                    if (string.IsNullOrEmpty(linha))
                        continue;

                    if (linha.Contains("Categorias"))
                        break;

                    Artigo artigo = new Artigo { Cluster = nomeDoCluster, NomeSumario = linha.Substring(1) };
                    if (!string.IsNullOrEmpty(artigo.Cluster))
                        artigos.Add(artigo);
                }

                return artigos;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void recuperarSumarios(List<Artigo> artigos)
        {
            
            try
            {
                StreamReader arquivoOriginal = new StreamReader("c:/Arquivo/normal.txt", Encoding.Default);

                bool preencherTexto = false;
                string linha, texto = string.Empty;
                string nome = string.Empty;
                
                while ((linha = arquivoOriginal.ReadLine()) != null)
                {
                    if (linha.Contains("[Original de:"))
                    {
                        preencherTexto = true;
                        preencherSumarioDeArtigo(nome, texto, artigos);

                        nome = formatarNomeArtigo(linha);
                        texto = string.Empty;
                        continue;
                    }

                    if (string.IsNullOrEmpty(linha))
                        continue;

                    if(preencherTexto)
                        texto += linha + " ";

                }
                arquivoOriginal.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void preencherSumarioDeArtigo(string nome, string texto, List<Artigo> artigos)
        {
            if (string.IsNullOrEmpty(texto) || string.IsNullOrEmpty(nome))
                return;

            Artigo artigo = artigos.Where(item => item.NomeSumario == nome).First();
            artigo.Sumario.Add(texto);
        }

        private string formatarNomeArtigo(string linha)
        {
            string[] substrings = linha.Split('"');
            return substrings[1];
        }

        private void subirArtigosNoSolr(List<Artigo> artigos)
        {
            try
            {
                Operacoes op = new Operacoes();
                op.deletarTodosOsDados();                
                op.adicionarArtigo(artigos);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
