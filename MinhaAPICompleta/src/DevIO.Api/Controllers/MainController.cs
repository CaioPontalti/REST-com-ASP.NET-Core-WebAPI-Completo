using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevIO.Business.Intefaces;
using DevIO.Business.Notificacoes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DevIO.Api.Controllers
{
    [ApiController]
    public abstract class MainController : ControllerBase
    {
        private readonly INotificador _notificador;

        public MainController(INotificador notificador)
        {
            _notificador = notificador;
        }

        protected ActionResult CustomResponse(ModelStateDictionary modelState)
        {
            if (!modelState.IsValid)
                NotificarErrosModelInvalida(modelState);

            return CustomResponse();
        }

        protected ActionResult CustomResponse(object result = null)
        {
            if (OperacaoValida())
            {
                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            else
            {
                return BadRequest(new
                {
                    success = false,
                    errors = _notificador.ObterNotificacoes().Select(n => n.Mensagem)
                }); ;
            }
        }

        protected bool OperacaoValida() 
        {
            return !_notificador.TemNotificacao();
        }

        protected void NotificarErrosModelInvalida(ModelStateDictionary modelState)
        {
            var erros = modelState.SelectMany(e => e.Value.Errors);
            foreach (var erro in erros)
            {
                var error = erro.Exception == null ? erro.ErrorMessage : erro.Exception.Message;
                NotificarErro(error);
            }
        }

        protected void NotificarErro(string mensagem)
        {
            _notificador.Handle(new Notificacao(mensagem));
        }
    }
}
