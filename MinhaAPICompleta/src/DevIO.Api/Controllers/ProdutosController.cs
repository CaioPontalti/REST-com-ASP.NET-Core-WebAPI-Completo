using AutoMapper;
using DevIO.Api.ViewModels;
using DevIO.Business.Intefaces;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DevIO.Api.Controllers
{
    [Route("api/produtos")]
    public class ProdutosController : MainController
    {
        private readonly IProdutoRepository _produtoRepository;
        private readonly IProdutoService _produtoService;
        private readonly IMapper _mapper;

        public ProdutosController(IProdutoRepository produtoRepository, IMapper mapper, INotificador notificador) : base(notificador)
        {
            _produtoRepository = produtoRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IEnumerable<ProdutoViewModel>> ObterTodos()
        {
            return _mapper.Map<IEnumerable<ProdutoViewModel>>(await _produtoRepository.ObterTodos());
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProdutoViewModel>> ObterPorId(Guid id)
        {
            var produtoViewModel = _mapper.Map<ProdutoViewModel>(await _produtoRepository.ObterProdutoFornecedor(id));

            if (produtoViewModel == null) return NotFound();

            return produtoViewModel;
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ProdutoViewModel>> ExcluirProduto(Guid id)
        {
            var produtoViewModel = await _produtoRepository.ObterPorId(id);

            if (produtoViewModel == null) return NotFound();

            await _produtoRepository.Remover(id);

            return CustomResponse(new { produtoViewModel.Id, produtoViewModel.Nome }); //retorna o Id e o nome do produto excluído.
        }

        [DisableRequestSizeLimit] //Permite fazer o upload de arquivo com qualquer tamanho
        [HttpPost]
        public async Task<ActionResult<ProdutoViewModel>> Adicionar(ProdutoViewModel produtoViewModel)
        {
            if (!ModelState.IsValid) return BadRequest();

            if (!UploadArquivo(produtoViewModel.ImagemUpload, produtoViewModel.Imagem))
                return CustomResponse(produtoViewModel);

            var produto = _mapper.Map<Produto>(produtoViewModel);
            await _produtoRepository.Adicionar(produto);

            return Created("api/produto", produtoViewModel);
        }

        [HttpPost("adicionar-alternativo")]
        public async Task<ActionResult<ProdutoViewModel>> AdicionarAlternativo(ProdutoViewModelAlternativo produtoViewModel)
        {
            /*
               
                recebe via form-data
                Key : Value
                Propriedade ImagemUpload : o arquivo (imagem.jpg) por exemplo
                Recebe as outras propriedades do produtoViewModel via json. Key: produto.

            */


            if (!ModelState.IsValid) return BadRequest();

            if (! await UploadArquivoAlternativo(produtoViewModel.ImagemUpload, produtoViewModel.Imagem))
                return CustomResponse(produtoViewModel);

            var produto = _mapper.Map<Produto>(produtoViewModel);
            await _produtoRepository.Adicionar(produto);

            return Created("api/produto", produtoViewModel);
        }


        [RequestSizeLimit(40000000)] //Limita o request a 40 megas
        [HttpPost("imagem")]
        public async Task<ActionResult> AdicionarImagem(IFormFile file, [FromForm] string nome)//recebe via form-data
        {
            return Ok( await Task.FromResult(file) );
        }

        private bool UploadArquivo(string arquivo, string imgNome)
        {
            if (string.IsNullOrEmpty(arquivo))
            {
                //ModelState.AddModelError(string.Empty, "Imagem não informada");
                NotificarErro("Imagem não informada");
                return false;
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\imagens", imgNome);
            if (System.IO.File.Exists(filePath))
            {
                //ModelState.AddModelError(string.Empty, "Já existe um arquivo com esse nome");
                NotificarErro("Já existe um arquivo com esse nome");
                return false;
            }

            var imageDataByteArray = Convert.FromBase64String(arquivo);

            System.IO.File.WriteAllBytes(filePath, imageDataByteArray);

            return true;
        }

        private async Task<bool> UploadArquivoAlternativo(IFormFile arquivo, string imgNome)
        {
            if (arquivo == null || arquivo.Length <= 0)
            {
                //ModelState.AddModelError(string.Empty, "Imagem não informada");
                NotificarErro("Imagem não informada");
                return false;
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\imagens", imgNome);
            if (System.IO.File.Exists(filePath))
            {
                //ModelState.AddModelError(string.Empty, "Já existe um arquivo com esse nome");
                NotificarErro("Já existe um arquivo com esse nome");
                return false;
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await arquivo.CopyToAsync(stream);
            }

            return true;
        }
    }
}
