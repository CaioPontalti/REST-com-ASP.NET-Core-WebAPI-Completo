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

            CreateMap<ProdutoViewModelAlternativo, Produto>().ReverseMap();

            //Add o .ForMember para setar o nome do Fornecedor na propriedade NomeFornecedor da ProdutoViewModel
            CreateMap<Produto, ProdutoViewModel>()
                .ForMember(destino => destino.NomeFornecedor, opt => opt.MapFrom(src => src.Fornecedor.Nome));

            CreateMap<ProdutoViewModel, Produto>();
        }
    }
}
