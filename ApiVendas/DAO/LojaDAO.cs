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
	internal class LojaDAO : ConnectionOracle
	{
		OracleCommand? cmd;
		
		public async Task<List<Loja>> Lojas()
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
				
				cmd.CommandText = "SELECT E.NROEMPRESA,\r\n       " +
									"E.NOMEREDUZIDO,\r\n       " +
									"E.NROCGC,\r\n       " +
									"E.DIGCGC,\r\n       " +
									"E.CEP,\r\n       " +
									"G.NROLOGRADOURO,\r\n       " +
									"G.BAIRRO,\r\n       " +
									"G.LOGRADOURO\r\n       " +
									"FROM CONSINCO.GE_PESSOA G,\r\n            " +
									"CONSINCO.MAX_EMPRESA E\r\n            " +
									"WHERE E.CEP = G.CEP\r\n                  " +
									"AND G.NOMERAZAO LIKE '%COMETA%'\r\n       " +
									"ORDER BY E.NROEMPRESA";
				
				List<Loja> lojas = new List<Loja>();
				OracleDataReader od = (OracleDataReader)await cmd.ExecuteReaderAsync();
				
				while(od.Read())
				{
					Loja loja = new Loja()
					{
						Cod_Loja = od.GetInt32(0).ToString(),
						Nome = od.GetString(1),
						Cnpj = int.Parse(od.GetString(3)) < 10 ? od.GetString(2) + "0" + od.GetString(3) : od.GetString(2) + od.GetString(3),
						Cep = od.GetString(4),
						Numero= od.GetString(5),
						Bairro= od.GetString(6),	
						Rua = od.GetString(7)
                    };
					
					lojas.Add(loja);
				}
				
				return lojas;
				
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


        public async Task<Loja> LojaPorNumero(int cod_loja)
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

                cmd.CommandText = "SELECT E.NROEMPRESA,\r\n       " +
                                    "E.NOMEREDUZIDO,\r\n       " +
                                    "E.NROCGC,\r\n       " +
                                    "E.DIGCGC,\r\n       " +
                                    "E.CEP,\r\n       " +
                                    "G.NROLOGRADOURO,\r\n       " +
                                    "G.BAIRRO,\r\n       " +
                                    "G.LOGRADOURO\r\n       " +
                                    "FROM CONSINCO.GE_PESSOA G,\r\n            " +
                                    "CONSINCO.MAX_EMPRESA E\r\n            " +
                                    "WHERE E.CEP = G.CEP\r\n                  " +
                                    "AND G.NOMERAZAO LIKE '%COMETA%'\r\n       " +
									"AND E.NROEMPRESA = "+ cod_loja + " " +
                                    "ORDER BY E.NROEMPRESA";

                Loja loja = new Loja();
                OracleDataReader od = (OracleDataReader)await cmd.ExecuteReaderAsync();

                while (od.Read())
                {

					loja.Cod_Loja = od.GetInt32(0).ToString();
					loja.Nome = od.GetString(1);
					loja.Cnpj = int.Parse(od.GetString(3)) < 10 ? od.GetString(2) + "0" + od.GetString(3) : od.GetString(2) + od.GetString(3);
					loja.Cep = od.GetString(4);
					loja.Numero = od.GetString(5);
					loja.Bairro = od.GetString(6);
					loja.Rua = od.GetString(7);

                }

                return loja;

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