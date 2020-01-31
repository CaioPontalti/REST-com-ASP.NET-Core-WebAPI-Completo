using AutoMapper;
using DevIO.Api.ViewModels;
using DevIO.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevIO.Api.Configuration
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
                           //de => para                
            CreateMap<Fornecedor, FornecedorViewModel>().ReverseMap(); //ReverseMap() => Faz o mapeamento inverso 
            CreateMap<Endereco, EnderecoViewModel>().ReverseMap();

            //Add o .ForMember para setar o nome do Fornecedor na propriedade NomeFornecedor da ProdutoViewModel
            CreateMap<Produto, ProdutoViewModel>()
                .ForMember(destino => destino.NomeFornecedor, opt => opt.MapFrom(src => src.Fornecedor.Nome));

            CreateMap<ProdutoViewModel, Produto>()
                .ForMember(destino => destino.Id, opt => opt.MapFrom(origem => origem.Id))
                .ForMember(destino => destino.FornecedorId, opt => opt.MapFrom(origem => origem.FornecedorId))
                .ForMember(destino => destino.Nome, opt => opt.MapFrom(origem => origem.Nome))
                .ForMember(destino => destino.Descricao, opt => opt.MapFrom(origem => origem.Descricao))
                .ForMember(destino => destino.Imagem, opt => opt.MapFrom(origem => origem.Imagem))
                .ForMember(destino => destino.Valor, opt => opt.MapFrom(origem => origem.Valor))
                .ForMember(destino => destino.DataCadastro, opt => opt.MapFrom(origem => origem.DataCadastro))
                .ForMember(destino => destino.Ativo, opt => opt.MapFrom(origem => origem.Ativo));


            CreateMap<ProdutoViewModelAlternativo, Produto>().ReverseMap();
        }
    }
}
