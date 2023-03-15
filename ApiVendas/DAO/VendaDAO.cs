using ApiVendas.Db;
using ApiVendas.Models;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace ApiVendas.DAO
{
	internal class VendaDAO : ConnectionOracle
	{
		OracleCommand? cmd;

		public async Task<List<Venda>> vendas(int cod_fornecedor)
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

				cmd.CommandText = "select            E.NROCGC,  \n"+
						"E.DIGCGC,  \n"+
						"TO_CHAR(V.DTAVDA, 'dd/MM/YYYY') AS MÊS,  \n"+
						"TO_CHAR((SELECT G.CODACESSO  \n"+  
						"FROM CONSINCO.MAP_PRODCODIGO G  \n"+ 
						"WHERE A.SEQPRODUTO = G.SEQPRODUTO  \n"+ 
						"	AND G.TIPCODIGO = 'E'  \n"+
						"ORDER BY G.CODACESSO  \n"+ 
						"FETCH FIRST 1 ROWS ONLY  \n"+ 
						") || ',' || (SELECT G.CODACESSO  \n"+  
						"FROM CONSINCO.MAP_PRODCODIGO G  \n"+ 
						"WHERE A.SEQPRODUTO = G.SEQPRODUTO  \n"+
						"	AND G.TIPCODIGO = 'E'  \n"+  
						"ORDER BY G.CODACESSO DESC  \n"+ 
						"FETCH FIRST 1 ROWS ONLY)) AS EANS,  \n"+
						"CAST( CONSINCO.FCODACESSOPRODEMB(A.SEQPRODUTO, 'B', 0, K.QTDEMBALAGEM) AS VARCHAR(30) ) as CODINTERNO,  \n"+
						"A.DESCCOMPLETA,  \n"+
						"sum(round((V.QTDITEM - V.QTDDEVOLITEM) / K.QTDEMBALAGEM, 6)) as QUANTIDADE,  \n"+
						"sum(((case  \n"+
						"when 'N' in ('S', 'V') then   \n"+
						"V.VLRITEMSEMDESC  \n"+
						"else  \n"+
						"V.VLRITEM  \n"+
						"end) - (V.VLRDEVOLITEM))) as VLRVENDA,  \n"+
						"E.NOMEREDUZIDO   \n"+
						"from CONSINCO.MRL_CUSTODIA Y,  \n"+
						"CONSINCO.MAXV_ABCDISTRIBBASE V,  \n"+
						"CONSINCO.MAP_PRODUTO         A,  \n"+
						"CONSINCO.MAP_PRODUTO         PB,  \n"+
						"CONSINCO.MAP_FAMDIVISAO      D,  \n"+
						"CONSINCO.MAP_FAMEMBALAGEM    K,  \n"+
						"CONSINCO.MAX_EMPRESA         E,  \n"+
						"CONSINCO.MAD_FAMSEGMENTO     H  \n"+
						"where D.SEQFAMILIA = A.SEQFAMILIA  \n"+ 
						"and D.NRODIVISAO = '2'  \n"+ 
						"and V.SEQPRODUTO = A.SEQPRODUTO  \n"+    
						"and V.SEQPRODUTOCUSTO = PB.SEQPRODUTO  \n"+    
						"and V.NROEMPRESA in ( SELECT E.NROEMPRESA  \n"+ 
						"FROM CONSINCO.MAX_EMPRESA E  )  \n"+    
						"and V.NROSEGMENTO IN (2,3)  \n"+    
						"and E.NROEMPRESA = V.NROEMPRESA  \n"+    
						"and V.DTAVDA = TRUNC(SYSDATE)-1  \n"+    
						"and Y.NROEMPRESA = nvl(E.NROEMPCUSTOABC, E.NROEMPRESA)  \n"+    
						"and Y.DTAENTRADASAIDA = V.DTAVDA  \n"+    
						"and K.SEQFAMILIA = A.SEQFAMILIA  \n"+    
						"and K.QTDEMBALAGEM = H.PADRAOEMBVENDA  \n"+    
						"and Y.SEQPRODUTO = PB.SEQPRODUTO  \n"+    
						"and H.SEQFAMILIA = A.SEQFAMILIA  \n"+    
						"and H.NROSEGMENTO = V.NROSEGMENTO  \n"+    
						"and V.CODGERALOPER IN (307,308)  \n"+    
						"and A.SEQPRODUTO IN ( SELECT P.SEQPRODUTO  \n"+ 
						"FROM CONSINCO.MAP_PRODUTO P  )  \n"+    
						"and exists (select 1   \n"+          
						"from CONSINCO.MAP_FAMFORNEC   \n"+         
						"where SEQFORNECEDOR IN ( "+cod_fornecedor+"   \n"+ 
						")   \n"+           
						"and PRINCIPAL = 'S'   \n"+           
						"and MAP_FAMFORNEC.SEQFAMILIA = A.SEQFAMILIA)  \n"+ 
						"group by E.NROCGC,  \n"+           
						"E.NOMEREDUZIDO,  \n"+           
						"E.DIGCGC,    \n"+            
						"CONSINCO.FCODACESSOPRODEMB(A.SEQPRODUTO, 'B', 0, K.QTDEMBALAGEM),    \n"+         
						"V.DTAVDA,   \n"+          
						"A.DESCCOMPLETA,  \n"+
						"A.SEQPRODUTO  \n"+
						"order by E.NROCGC, E.DIGCGC, A.DESCCOMPLETA";

				List<Venda> vendas = new List<Venda>();
				OracleDataReader od = (OracleDataReader)await cmd.ExecuteReaderAsync();

				while (od.Read())
				{
					Venda venda = new Venda()
					{
						Loja = od.GetString(8),
						Cnpj = int.Parse(od.GetString(1)) < 10 ? od.GetString(0) + "0" + od.GetString(1) : od.GetString(0) + od.GetString(1),
						Data_Venda = od.GetString(2),
						Cod_Produto = od.GetString(3),
						Descricao_Produto = od.GetString(5),
						Quantidade_Vendida = od.GetDouble(6),
						Valor_Vendido = od.GetDouble(7),

					};

					vendas.Add(venda);
				}

				return vendas;
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
		
		public async Task<List<Venda>> vendasPorLoja(int cod_fornecedor, int loja)
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

				cmd.CommandText = "select            E.NROCGC,  \n"+
						"E.DIGCGC,  \n"+
						"TO_CHAR(V.DTAVDA, 'dd/MM/YYYY') AS MÊS,  \n"+
						"TO_CHAR((SELECT G.CODACESSO  \n"+  
						"FROM CONSINCO.MAP_PRODCODIGO G  \n"+ 
						"WHERE A.SEQPRODUTO = G.SEQPRODUTO  \n"+ 
						"	AND G.TIPCODIGO = 'E'  \n"+
						"ORDER BY G.CODACESSO  \n"+ 
						"FETCH FIRST 1 ROWS ONLY  \n"+ 
						") || ',' || (SELECT G.CODACESSO  \n"+  
						"FROM CONSINCO.MAP_PRODCODIGO G  \n"+ 
						"WHERE A.SEQPRODUTO = G.SEQPRODUTO  \n"+
						"	AND G.TIPCODIGO = 'E'  \n"+  
						"ORDER BY G.CODACESSO DESC  \n"+ 
						"FETCH FIRST 1 ROWS ONLY)) AS EANS,  \n"+
						"CAST( CONSINCO.FCODACESSOPRODEMB(A.SEQPRODUTO, 'B', 0, K.QTDEMBALAGEM) AS VARCHAR(30) ) as CODINTERNO,  \n"+
						"A.DESCCOMPLETA,  \n"+
						"sum(round((V.QTDITEM - V.QTDDEVOLITEM) / K.QTDEMBALAGEM, 6)) as QUANTIDADE,  \n"+
						"sum(((case  \n"+
						"when 'N' in ('S', 'V') then   \n"+
						"V.VLRITEMSEMDESC  \n"+
						"else  \n"+
						"V.VLRITEM  \n"+
						"end) - (V.VLRDEVOLITEM))) as VLRVENDA,  \n"+
						"E.NOMEREDUZIDO   \n"+
						"from CONSINCO.MRL_CUSTODIA Y,  \n"+
						"CONSINCO.MAXV_ABCDISTRIBBASE V,  \n"+
						"CONSINCO.MAP_PRODUTO         A,  \n"+
						"CONSINCO.MAP_PRODUTO         PB,  \n"+
						"CONSINCO.MAP_FAMDIVISAO      D,  \n"+
						"CONSINCO.MAP_FAMEMBALAGEM    K,  \n"+
						"CONSINCO.MAX_EMPRESA         E,  \n"+
						"CONSINCO.MAD_FAMSEGMENTO     H  \n"+
						"where D.SEQFAMILIA = A.SEQFAMILIA  \n"+ 
						"and D.NRODIVISAO = '2'  \n"+ 
						"and V.SEQPRODUTO = A.SEQPRODUTO  \n"+    
						"and V.SEQPRODUTOCUSTO = PB.SEQPRODUTO  \n"+    
						"and V.NROEMPRESA = "+loja+"  \n"+    
						"and V.NROSEGMENTO IN (2,3)  \n"+    
						"and E.NROEMPRESA = V.NROEMPRESA  \n"+    
						"and V.DTAVDA = TRUNC(SYSDATE)-1  \n"+    
						"and Y.NROEMPRESA = nvl(E.NROEMPCUSTOABC, E.NROEMPRESA)  \n"+    
						"and Y.DTAENTRADASAIDA = V.DTAVDA  \n"+    
						"and K.SEQFAMILIA = A.SEQFAMILIA  \n"+    
						"and K.QTDEMBALAGEM = H.PADRAOEMBVENDA  \n"+    
						"and Y.SEQPRODUTO = PB.SEQPRODUTO  \n"+    
						"and H.SEQFAMILIA = A.SEQFAMILIA  \n"+    
						"and H.NROSEGMENTO = V.NROSEGMENTO  \n"+    
						"and V.CODGERALOPER IN (307,308)  \n"+    
						"and A.SEQPRODUTO IN ( SELECT P.SEQPRODUTO  \n"+ 
						"FROM CONSINCO.MAP_PRODUTO P  )  \n"+    
						"and exists (select 1   \n"+          
						"from CONSINCO.MAP_FAMFORNEC   \n"+         
						"where SEQFORNECEDOR IN ( "+cod_fornecedor+"   \n"+ 
						")   \n"+           
						"and PRINCIPAL = 'S'   \n"+           
						"and MAP_FAMFORNEC.SEQFAMILIA = A.SEQFAMILIA)  \n"+ 
						"group by E.NROCGC,  \n"+           
						"E.NOMEREDUZIDO,  \n"+           
						"E.DIGCGC,    \n"+            
						"CONSINCO.FCODACESSOPRODEMB(A.SEQPRODUTO, 'B', 0, K.QTDEMBALAGEM),    \n"+         
						"V.DTAVDA,   \n"+          
						"A.DESCCOMPLETA,  \n"+
						"A.SEQPRODUTO  \n"+
						"order by E.NROCGC, E.DIGCGC, A.DESCCOMPLETA";

				List<Venda> vendas = new List<Venda>();
				OracleDataReader od = (OracleDataReader)await cmd.ExecuteReaderAsync();

				while (od.Read())
				{
					Venda venda = new Venda()
					{
						Loja = od.GetString(8),
						Cnpj = int.Parse(od.GetString(1)) < 10 ? od.GetString(0) + "0" + od.GetString(1) : od.GetString(0) + od.GetString(1),
						Data_Venda = od.GetString(2),
						Cod_Produto = od.GetString(3),
						Descricao_Produto = od.GetString(5),
						Quantidade_Vendida = od.GetDouble(6),
						Valor_Vendido = od.GetDouble(7),

					};

					vendas.Add(venda);
				}

				return vendas;
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
