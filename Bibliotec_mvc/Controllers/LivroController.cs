using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Bibliotec.Contexts;
using Bibliotec.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace Bibliotec_mvc.Controllers
{
    [Route("[controller]")]
    public class LivroController : Controller
    {
        private readonly ILogger<LivroController> _logger;

        public LivroController(ILogger<LivroController> logger)
        {
            _logger = logger;
        }

        Context context = new Context();

        public IActionResult Index()
        {
            ViewBag.Admin = HttpContext.Session.GetString("Admin")!;

            //Criar uma lista de livros
            List<Livro> listaLivros = context.Livro.ToList();

            //Verificar se o livro tem reserva ou não
            //ToDictionay(chave, valor)
            var livrosReservados = context.LivroReserva.ToDictionary(livro => livro.LivroID, livror => livror.DtReserva);

            ViewBag.Livros = listaLivros;
            ViewBag.LivrosComReserva = livrosReservados;

            return View();
        }

        [Route("Cadastro")]
        //Método que retorna a tela de cadastro:
        public IActionResult Cadastro()
        {
            ViewBag.Admin = HttpContext.Session.GetString("Admin")!;

            ViewBag.Categorias = context.Categoria.ToList();
            //Retorna a View de cadastro:
            return View();
        }

        // Metodo de cadastrar um Livro
        [Route("Cadastrar")]
        public IActionResult Cadastrar(IFormCollection form){
            Livro novoLivro = new Livro();

            //O que meu usuario escrever no formulario sera atribuido a cada campo do livro ao novo livro
            novoLivro.Nome = form["Nome"].ToString();
            novoLivro.Descricao = form["Descricao"].ToString();
            novoLivro.Editora = form["Editora"].ToString();
            novoLivro.Escritor = form["Escritor"].ToString();
            novoLivro.Idioma = form["Idioma"].ToString();

            //img
            if(form.Files.Count > 0){
                var arquivo = form.Files[0];

                var pasta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/imagens/Livros");
                if(Directory.Exists(pasta)){
                    Directory.CreateDirectory(pasta);
                }
                var caminho = Path.Combine(pasta, arquivo.FileName);

                using (var stream = new FileStream(caminho, FileMode.Create)) {
                    //Copiou o arquivo para o meu diretorio
                    arquivo.CopyTo(stream);
                }
                
                novoLivro.Imagem = arquivo.FileName;
            }else{
                novoLivro.Imagem = "padrao.png";
            }

            context.Livro.Add(novoLivro);
            context.SaveChanges();

            //Segunda parte : e adicioaar dentro de LivroCategoria a categorai que pertence ao novoLivro
            List<LivroCategoria> livroCategorias = new List<LivroCategoria>();//lista as categoriascl

            //Array que possui as categorias selecionadas pelo usuario
            string [] categoriaSelecionadas = form["Categorias"].ToString().Split(',');

            foreach(string categoria in categoriaSelecionadas){ 
                LivroCategoria livroCategoria = new LivroCategoria();
                livroCategoria.CategoriaID = int.Parse(categoria);
                livroCategoria.LivroID = novoLivro.LivroID;
                //Adicionamos o obj livroCategoria dentro da lista ListaLivroCategoria
                livroCategorias.Add(livroCategoria);
            }
            //Peguei a colecao de livroCategorias e coloquei a tabela LivroCategoria
            context.LivroCategoria.AddRange(livroCategorias);

            context.SaveChanges();

            return LocalRedirect("/Cadastro");
        }


        [Route("Editar/{id}")]
        public IActionResult Editar(int id){

            ViewBag.Admin = HttpContext.Session.GetString("Admin")!;
            //LivroID == 3
            
            //Buscar quem e o tal do id numero 3:
            Livro livroEncontrado = context.Livro.FirstOrDefault(livro => livro.LivroID == id)!;

            //Buscar as categorias do livroEncontrado possui
            var categoriasDoLivroEncontrado = context.LivroCategoria.Where(identificadorLivro => identificadorLivro.LivroID == id)
            .Select(livro => livro.Categoria).ToList();

            //Quero pegar as informacoes e mandar para a minha View
            ViewBag.Livro = livroEncontrado;
            ViewBag.Categorias = categoriasDoLivroEncontrado;

            return View();
        }
        // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        // public IActionResult Error()
        // {
        //     return View("Error!");
        // }
    }
}