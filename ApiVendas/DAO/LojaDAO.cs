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
				
				cmd.CommandText = "SELECT NOMEREDUZIDO FROM CONSINCO.MAX_EMPRESA WHERE NROEMPRESA NOT IN (7,31)";
				
				List<Loja> lojas = new List<Loja>();
				OracleDataReader od = (OracleDataReader)await cmd.ExecuteReaderAsync();
				
				while(od.Read())
				{
					Loja loja = new Loja()
					{
						Nome = od.GetString(0)
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
		
	}
}