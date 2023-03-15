using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ApiVendas.Db;
using ApiVendas.Models;
using Oracle.ManagedDataAccess.Client;

namespace ApiVendas.DAO
{
	internal class PedidoDAO : ConnectionOracle
	{
		OracleCommand? cmd;
		
		public async Task<List<Pedido>> PedidosPorFornecedor(int cod_fornecedor)
		{
			 if (con.State == ConnectionState.Closed)
			{
				con.Open();
			}
			tran = con.BeginTransaction();
			
			try
			{
				cmd = new OracleCommand();
				cmd.Connection = con;
				cmd.Transaction = tran;
				
				cmd.CommandText = "SELECT DISTINCT P.NROPEDIDOSUPRIM AS PEDIDO, \n"+
					" P.DTAEMISSAO AS EMISSAO, \n"+
					" CASE  \n"+
					" 	WHEN I.QTDTOTRECEBIDA IS NULL THEN 'ABERTO' \n"+
					" 	ELSE 'RECEBIDO' \n"+
					" 	END AS SITUAÇÃO, \n"+
					" E.NOMEREDUZIDO AS LOJA, \n"+
					" TO_CHAR(PR.SEQPRODUTO) AS COD_PRODUTO, \n"+
					" TO_CHAR(CASE \n"+ 
					" WHEN (SELECT H.CODACESSO \n"+ 
					" FROM CONSINCO.MAP_PRODCODIGO H \n"+
					" WHERE H.SEQPRODUTO = PR.SEQPRODUTO \n"+
					" 		AND H.TIPCODIGO = 'F' \n"+
					" FETCH FIRST 1 ROWS ONLY) IS NULL THEN ' ' \n"+
					" ELSE (SELECT H.CODACESSO \n"+ 
					" FROM CONSINCO.MAP_PRODCODIGO H \n"+
					" WHERE H.SEQPRODUTO = PR.SEQPRODUTO \n"+
					" 		AND H.TIPCODIGO = 'F' \n"+
					" FETCH FIRST 1 ROWS ONLY) END \n"+
					" ) AS COD_FORNECEDOR, \n"+
					" TO_CHAR((SELECT G.CODACESSO  \n"+
					" 		FROM CONSINCO.MAP_PRODCODIGO G \n"+
					" 		WHERE PR.SEQPRODUTO = G.SEQPRODUTO \n"+
					" 				AND G.CODACESSO LIKE '789%' \n"+
					" 		ORDER BY G.CODACESSO \n"+
					" 		FETCH FIRST 1 ROWS ONLY \n"+
					" 		) || ',' || (SELECT G.CODACESSO  \n"+
					" 		FROM CONSINCO.MAP_PRODCODIGO G \n"+
					" 		WHERE PR.SEQPRODUTO = G.SEQPRODUTO \n"+
					" 				AND G.CODACESSO LIKE '789%' \n"+
					" 		ORDER BY G.CODACESSO DESC \n"+
					" 		FETCH FIRST 1 ROWS ONLY)) AS EANS, \n"+
					" PR.DESCCOMPLETA AS DESCRICAO, \n"+
					" CASE I.QTDEMBALAGEM  \n"+
					" 	WHEN 1 THEN 'UN 1' \n"+
					" 	ELSE 'CX '|| I.QTDEMBALAGEM \n"+
					" END AS EMBALAGEM, \n"+                        
					" TO_CHAR((I.QTDSOLICITADA / I.QTDEMBALAGEM)) AS QUANTIDADE, \n"+
					" TO_CHAR(I.VLREMBITEM,'99990D99') AS VALOR_UNITARIO, \n"+
					" TO_CHAR((I.QTDSOLICITADA / I.QTDEMBALAGEM) * I.VLREMBITEM, '99990D99') AS VALOR_ITEM, \n"+
					" TO_CHAR(I.VLREMBICMSST,'99990D99') AS ICMS_EMBITEM, \n"+
					" TO_CHAR(I.VLREMBIPI, '99990D99') AS IBI_ITEM, \n"+
					" TO_CHAR(((I.QTDSOLICITADA / I.QTDEMBALAGEM) * I.VLREMBICMSST) +  \n"+
					" 					((I.QTDSOLICITADA / I.QTDEMBALAGEM) * I.VLREMBIPI) +  \n"+
					" 					((I.QTDSOLICITADA / I.QTDEMBALAGEM) * I.VLREMBITEM), '99990D99') AS CUSTO_BRUTO \n"+
					" FROM CONSINCO.MSU_PEDIDOSUPRIM P, \n"+
					" 	CONSINCO.MSU_PSITEMRECEBER I, \n"+
					" 	CONSINCO.GE_PESSOA F, \n"+
					" 	CONSINCO.MAX_EMPRESA E, \n"+
					" 	CONSINCO.MAP_PRODUTO PR \n"+
					" WHERE P.NROPEDIDOSUPRIM = I.NROPEDIDOSUPRIM \n"+
					" 		AND P.NROEMPRESA = E.NROEMPRESA \n"+
					" 		AND F.SEQPESSOA = P.SEQFORNECEDOR \n"+
					" 		AND I.SEQPRODUTO = PR.SEQPRODUTO \n"+
					" 		AND P.DTAEMISSAO BETWEEN TRUNC(SYSDATE)-15 AND TRUNC(SYSDATE)  \n"+
					" 		AND I.STATUSITEM = 'A' \n"+
					" 		AND P.SEQFORNECEDOR = "+cod_fornecedor+" \n"+
					" ORDER BY P.NROPEDIDOSUPRIM, E.NOMEREDUZIDO";
				
				List<Pedido> pedidos = new List<Pedido>();
				OracleDataReader od = (OracleDataReader)await cmd.ExecuteReaderAsync();
				
				while(od.Read())
				{
					Pedido pedido = new Pedido()
					{
						NumPedido = od.GetInt32(0),
						Emissao = od.GetString(1),
						Situacao = od.GetString(2),
						Loja = od.GetString(3),
						Cod_Produto = od.GetString(4),
						Cod_Fornecedor = od.GetString(5),
						Eans = od.GetString(6),
						Descricao = od.GetString(7),
						Embalagem = od.GetString(8),
						Quantidade = od.GetString(9),
						Valor_Unitario = double.Parse(od.GetString(10)),
						Valor_Item = double.Parse(od.GetString(11)),
						Icms_EmbItem = double.Parse(od.GetString(12)),
						Ipi_EmbItem = double.Parse(od.GetString(13)),
						Custo_Bruto = double.Parse(od.GetString(14))
					};
					
					pedidos.Add(pedido);
				}
				
				return pedidos;
			}
			catch (OracleException e)
			{
				tran.Rollback();
				Console.WriteLine(e.ToString());
				return null;
			}
			finally
			{
				con.Close();
			}

		}	
		
		public async Task<List<Pedido>> PedidosPorFornecedorPorLoja(int cod_fornecedor, int loja)
		{
			 if (con.State == ConnectionState.Closed)
			{
				con.Open();
			}
			tran = con.BeginTransaction();
			
			try
			{
				cmd = new OracleCommand();
				cmd.Connection = con;
				cmd.Transaction = tran;
				
				cmd.CommandText = "SELECT DISTINCT P.NROPEDIDOSUPRIM AS PEDIDO, \n"+
					" P.DTAEMISSAO AS EMISSAO, \n"+
					" CASE  \n"+
					" 	WHEN I.QTDTOTRECEBIDA IS NULL THEN 'ABERTO' \n"+
					" 	ELSE 'RECEBIDO' \n"+
					" 	END AS SITUAÇÃO, \n"+
					" E.NOMEREDUZIDO AS LOJA, \n"+
					" TO_CHAR(PR.SEQPRODUTO) AS COD_PRODUTO, \n"+
					" TO_CHAR(CASE \n"+ 
					" WHEN (SELECT H.CODACESSO \n"+ 
					" FROM CONSINCO.MAP_PRODCODIGO H \n"+
					" WHERE H.SEQPRODUTO = PR.SEQPRODUTO \n"+
					" 		AND H.TIPCODIGO = 'F' \n"+
					" FETCH FIRST 1 ROWS ONLY) IS NULL THEN ' ' \n"+
					" ELSE (SELECT H.CODACESSO \n"+ 
					" FROM CONSINCO.MAP_PRODCODIGO H \n"+
					" WHERE H.SEQPRODUTO = PR.SEQPRODUTO \n"+
					" 		AND H.TIPCODIGO = 'F' \n"+
					" FETCH FIRST 1 ROWS ONLY) END \n"+
					" ) AS COD_FORNECEDOR, \n"+
					" TO_CHAR((SELECT G.CODACESSO  \n"+
					" 		FROM CONSINCO.MAP_PRODCODIGO G \n"+
					" 		WHERE PR.SEQPRODUTO = G.SEQPRODUTO \n"+
					" 				AND G.CODACESSO LIKE '789%' \n"+
					" 		ORDER BY G.CODACESSO \n"+
					" 		FETCH FIRST 1 ROWS ONLY \n"+
					" 		) || ',' || (SELECT G.CODACESSO  \n"+
					" 		FROM CONSINCO.MAP_PRODCODIGO G \n"+
					" 		WHERE PR.SEQPRODUTO = G.SEQPRODUTO \n"+
					" 				AND G.CODACESSO LIKE '789%' \n"+
					" 		ORDER BY G.CODACESSO DESC \n"+
					" 		FETCH FIRST 1 ROWS ONLY)) AS EANS, \n"+
					" PR.DESCCOMPLETA AS DESCRICAO, \n"+
					" CASE I.QTDEMBALAGEM  \n"+
					" 	WHEN 1 THEN 'UN 1' \n"+
					" 	ELSE 'CX '|| I.QTDEMBALAGEM \n"+
					" END AS EMBALAGEM, \n"+                        
					" TO_CHAR((I.QTDSOLICITADA / I.QTDEMBALAGEM)) AS QUANTIDADE, \n"+
					" TO_CHAR(I.VLREMBITEM,'99990D99') AS VALOR_UNITARIO, \n"+
					" TO_CHAR((I.QTDSOLICITADA / I.QTDEMBALAGEM) * I.VLREMBITEM, '99990D99') AS VALOR_ITEM, \n"+
					" TO_CHAR(I.VLREMBICMSST,'99990D99') AS ICMS_EMBITEM, \n"+
					" TO_CHAR(I.VLREMBIPI, '99990D99') AS IBI_ITEM, \n"+
					" TO_CHAR(((I.QTDSOLICITADA / I.QTDEMBALAGEM) * I.VLREMBICMSST) +  \n"+
					" 					((I.QTDSOLICITADA / I.QTDEMBALAGEM) * I.VLREMBIPI) +  \n"+
					" 					((I.QTDSOLICITADA / I.QTDEMBALAGEM) * I.VLREMBITEM), '99990D99') AS CUSTO_BRUTO \n"+
					" FROM CONSINCO.MSU_PEDIDOSUPRIM P, \n"+
					" 	CONSINCO.MSU_PSITEMRECEBER I, \n"+
					" 	CONSINCO.GE_PESSOA F, \n"+
					" 	CONSINCO.MAX_EMPRESA E, \n"+
					" 	CONSINCO.MAP_PRODUTO PR \n"+
					" WHERE P.NROPEDIDOSUPRIM = I.NROPEDIDOSUPRIM \n"+
					" 		AND P.NROEMPRESA = E.NROEMPRESA \n"+
					"       AND P.NROEMPRESA = "+loja+"  \n"+
					" 		AND F.SEQPESSOA = P.SEQFORNECEDOR \n"+
					" 		AND I.SEQPRODUTO = PR.SEQPRODUTO \n"+
					" 		AND P.DTAEMISSAO BETWEEN TRUNC(SYSDATE)-15 AND TRUNC(SYSDATE)  \n"+
					" 		AND I.STATUSITEM = 'A' \n"+
					" 		AND P.SEQFORNECEDOR = "+cod_fornecedor+" \n"+
					" ORDER BY P.NROPEDIDOSUPRIM, E.NOMEREDUZIDO";
				
				List<Pedido> pedidos = new List<Pedido>();
				OracleDataReader od = (OracleDataReader)await cmd.ExecuteReaderAsync();
				
				while(od.Read())
				{
					Pedido pedido = new Pedido()
					{
						NumPedido = od.GetInt32(0),
						Emissao = od.GetString(1),
						Situacao = od.GetString(2),
						Loja = od.GetString(3),
						Cod_Produto = od.GetString(4),
						Cod_Fornecedor = od.GetString(5),
						Eans = od.GetString(6),
						Descricao = od.GetString(7),
						Embalagem = od.GetString(8),
						Quantidade = od.GetString(9),
						Valor_Unitario = double.Parse(od.GetString(10)),
						Valor_Item = double.Parse(od.GetString(11)),
						Icms_EmbItem = double.Parse(od.GetString(12)),
						Ipi_EmbItem = double.Parse(od.GetString(13)),
						Custo_Bruto = double.Parse(od.GetString(14))
					};
					
					pedidos.Add(pedido);
				}
				
				return pedidos;
			}
			catch (OracleException e)
			{
				tran.Rollback();
				Console.WriteLine(e.ToString());
				return null;
			}
			finally
			{
				con.Close();
			}

		}	
	}
}