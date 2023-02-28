using Oracle.ManagedDataAccess.Client;

namespace ApiVendas.Db
{
    public class ConnectionOracle
    {
        protected OracleConnection con;
        protected OracleTransaction? tran;

        public ConnectionOracle()
        {
            con = new OracleConnection("User Id=PERF;Password=per2013_;Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST =192.168.7.33)(PORT=1521)))(CONNECT_DATA=(SERVICE_NAME=COMETA))));Pooling = true;Connection Lifetime=600;Connection Timeout=600");
        }
    }
}
