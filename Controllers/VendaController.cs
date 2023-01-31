using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tech_test_payment_api.Context;
using tech_test_payment_api.Models;
using Microsoft.AspNetCore.Mvc;

namespace tech_test_payment_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VendaController : ControllerBase
    {
        private readonly PaymentContext _context;

        public VendaController(PaymentContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public IActionResult ObterPorId(int id)
        {
            var venda = _context.Vendas.Find(id);

            if (venda == null)
                return NotFound();

            var vendedor = _context.Vendedores.Find(Venda.VendedorId);
            if (vendedor == null)
                return NotFound();

            var itens = _context.Items.Where(i => i.VendaID == venda.Id).ToList();

            Facade.Venda.Response.Venda fVenda = new Facade.Venda.Response.Venda()
            {
                VendaID = venda.Id,
                Data = venda.Data,
                Status = venda.Status,
                Vendedor = new Facade.Venda.Vendedor()
                {
                    Nome = vendedor.Nome,
                    CPF = vendedor.CPF,
                    Email = vendedor.Email,
                    Fone = vendedor.Fone,
                }
            };

            foreach (Item item in itens)
            {
                fVenda.Items.Add(new Facade.Venda.Item()
                {
                    Nome = item.Nome,
                    Quantidade = item.Quantidade,
                    Valor = item.Valor
                });
            }

            return Ok(fVenda);
        }

        [HttpPost]
        public IActionResult Criar(Facade.Venda.Request.CriarVenda venda)
        {
            List<string> error = new List<string>();
            if (venda != null)
            {
                if (venda.Vendedor != null && venda.Vendedor.Nome.Length > 0 && venda.Vendedor.CPF.Length > 0 && venda.Vendedor.Fone.Length > 0)
                {
                    if (venda.Items.Count == 0)
                    {
                        error.Add("Informe pelo meno um item vendido!");
                    }
                }
                else
                {
                    error.Add("Informe os dados do vendedor!!");
                }
            }
            else
            {
                error.Add("Parâmetros não informados!!");
            }

            if (error.Count > 0)
            {
                return BadRequest(new { Erro = string.Join(", ", error) });
            }
            else
            {
                Venda vendaBanco = new Venda()
                {
                    Data = venda.Data,
                    Status = "Finalizado",

                    Vendedor = new Vendedor()
                    {
                        Nome = venda.Vendedor.Nome,
                        CPF = venda.Vendedor.CPF.Replace(" ", "").Replace(".", "").Replace("-", ""),
                        Email = venda.Vendedor.Email,
                        Fone = venda.Vendedor.Fone
                    }
                };

                _context.Vendas.Add(vendaBanco);
                _context.SaveChanges();

                foreach (Facade.Venda.Item item in venda.Items)
                {
                    _context.ItensVendidos.Add(new ItemVendido()
                    {
                        Nome = Ven,
                        Quantidade = ItemVendido.Quantidade,
                        Valor = ItemVendido.Valor,
                        VendaId = vendaBanco.Id
                    });
                }

                _context.SaveChanges();

                return Ok(vendaBanco.Id);
            }
        }

        [HttpPost("AlterarStatus")]
        public IActionResult AlterarStatus(Facade.Venda.Request.AlterarStatus req)
        {
            var Venda = _context.Vendas.Find(req.VendaID);
            if (Venda == null)
                return NotFound("Id não encontrado");

            switch (req.Status)
            {
                case "Pagamento Aprovado":
                    if (!Venda.StatusDaVenda.Equals("Aguardando pagamento"))
                    {
                        return BadRequest(new { Erro = "Sua venda precisa estar no status [Aguardando pagamento] para poder ser alterada para este novo status" });
                    }
                    else
                    {
                        Venda.StatusDaVenda = req.Status;
                    }
                    break;

                case "Enviando para transportadora":
                    if (!Venda.StatusDaVenda.Equals("Pagamento aprovado"))
                    {
                        return BadRequest(new { Erro = "Sua venda precisa estar no status [Enviado para a transportadora] para poder ser alterada para este novo status" });
                    }
                    else
                    {
                        Venda.StatusDaVenda = req.Status;
                    }
                    break;

                case "Entregue":
                    if (!Venda.StatusDaVenda.Equals("Enviado para a transportadora"))
                    {
                        return BadRequest(new { Erro = "Sua venda precisa estar no status [Enviado para a transportadora] para poder ser alterada para este novo status" });
                    }
                    else
                    {
                        Venda.StatusDaVenda = req.Status;
                    }
                    break;

                case "Cancelada":
                    if (!Venda.StatusDaVenda.Equals("Enviado para a transportadora"))
                    {
                        return BadRequest(new { Erro = "Como esta venda já está na transportadora, ela não pode ser cancelada" });
                    }
                    else
                    {
                        Venda.StatusDaVenda = req.Status;
                    }
                    break;

                default:
                    return BadRequest(new { Erro = "Informe um estatus válido: [Pagamento aprovado, Enviado para a transportadora, Entregue ou Cancelada]" });
            }

            _context.Vendas.Update(Venda);
            _context.SaveChanges();

            return Ok(Venda);
        }

    }
}