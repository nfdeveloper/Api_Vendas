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

                cmd.CommandText = "select E.NROCGC,\r\n" +
                                  "E.DIGCGC,\r\n" +
                                  "TO_CHAR(V.DTAVDA, 'dd/MM/YYYY') AS MÊS,\r\n" +
                                  "C.CODACESSO,\r\n" +
                                  "CAST( CONSINCO.FCODACESSOPRODEMB(A.SEQPRODUTO, 'B', 0, K.QTDEMBALAGEM) AS VARCHAR(30) ) as CODINTERNO,\r\n" +
                                  "A.DESCCOMPLETA,\r\n" +
                                  "sum(round((V.QTDITEM - V.QTDDEVOLITEM) / K.QTDEMBALAGEM, 6)) as QUANTIDADE,\r\n" +
                                  "sum(((case\r\n" +
                                  "when 'N' in ('S', 'V') then\r\n " +
                                  "V.VLRITEMSEMDESC\r\n" +
                                  "else\r\n" +
                                  "V.VLRITEM\r\n" +
                                  "end) - (V.VLRDEVOLITEM))) as VLRVENDA,\r\n" +
                                  "E.NOMEREDUZIDO\r\n " +
                                  "from CONSINCO.MRL_CUSTODIA Y,\r\n" +
                                  "CONSINCO.MAXV_ABCDISTRIBBASE V,\r\n" +
                                  "CONSINCO.MAP_PRODUTO         A,\r\n" +
                                  "CONSINCO.MAP_PRODUTO         PB,\r\n" +
                                  "CONSINCO.MAP_FAMDIVISAO      D,\r\n" +
                                  "CONSINCO.MAP_FAMEMBALAGEM    K,\r\n" +
                                  "CONSINCO.MAX_EMPRESA         E,\r\n" +
                                  "CONSINCO.MAD_FAMSEGMENTO     H,\r\n " +
                                  "CONSINCO.MAP_PRODCODIGO      C\r\n  " +
                                  "where D.SEQFAMILIA = A.SEQFAMILIA\r\n " +
                                  "and D.NRODIVISAO = '2'\r\n " +
                                  "and V.SEQPRODUTO = A.SEQPRODUTO\r\n    " +
                                  "and C.SEQPRODUTO = A.SEQPRODUTO\r\n    " +
                                  "and C.INDUTILVENDA = 'S'\r\n    " +
                                  "and V.SEQPRODUTOCUSTO = PB.SEQPRODUTO\r\n    " +
                                  "and V.NROEMPRESA in ( SELECT E.NROEMPRESA\r\n " +
                                  "FROM CONSINCO.MAX_EMPRESA E\r\n  )\r\n    " +
                                  "and V.NROSEGMENTO IN (2,3)\r\n    " +
                                  "and E.NROEMPRESA = V.NROEMPRESA\r\n    " +
                                  "--and V.DTAVDA BETWEEN '${inicio}' AND '${fim}'\r\n    " +
                                  "and V.DTAVDA = TRUNC(SYSDATE)-1\r\n" +
                                  "--and V.DTAVDA BETWEEN '01NOV2022' AND '30NOV2022'\r\n    " +
                                  "and Y.NROEMPRESA = nvl(E.NROEMPCUSTOABC, E.NROEMPRESA)\r\n    " +
                                  "and Y.DTAENTRADASAIDA = V.DTAVDA\r\n    " +
                                  "and K.SEQFAMILIA = A.SEQFAMILIA\r\n    " +
                                  "and K.QTDEMBALAGEM = H.PADRAOEMBVENDA\r\n    " +
                                  "and Y.SEQPRODUTO = PB.SEQPRODUTO\r\n    " +
                                  "and H.SEQFAMILIA = A.SEQFAMILIA\r\n    " +
                                  "and H.NROSEGMENTO = V.NROSEGMENTO\r\n    " +
                                  "and V.CODGERALOPER IN (307,308)\r\n    " +
                                  "and A.SEQPRODUTO IN ( SELECT P.SEQPRODUTO\r\n " +
                                  "FROM CONSINCO.MAP_PRODUTO P\r\n  )\r\n    " +
                                  "and exists (select 1\r\n           " +
                                  "from CONSINCO.MAP_FAMFORNEC\r\n          " +
                                  "where SEQFORNECEDOR IN ( "+cod_fornecedor+"\r\n  " +
                                  ")\r\n            " +
                                  "and PRINCIPAL = 'S'\r\n            " +
                                  "and MAP_FAMFORNEC.SEQFAMILIA = A.SEQFAMILIA)\r\n  " +
                                  "group by E.NROCGC,\r\n           " +
                                  "E.NOMEREDUZIDO,\r\n           " +
                                  "E.DIGCGC,\r\n           " +
                                  "C.CODACESSO,\r\n           " +
                                  "CONSINCO.FCODACESSOPRODEMB(A.SEQPRODUTO, 'B', 0, K.QTDEMBALAGEM),\r\n           " +
                                  "V.DTAVDA,\r\n           " +
                                  "A.DESCCOMPLETA\r\n  " +
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
