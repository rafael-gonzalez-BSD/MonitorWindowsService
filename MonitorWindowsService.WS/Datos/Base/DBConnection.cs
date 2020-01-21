using Dapper;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace MonitorWindowsService.WS.Datos.Base
{
    public class DBConnection : Disposable
    {
        private readonly string _conexion;

        public DBConnection(string conexion)
        {
            _conexion = ConfigurationManager.ConnectionStrings[conexion].ToString();
        }

        public IEnumerable<T> Query<T>(Dictionary<string, dynamic> P, string SP)
        {
            using (IDbConnection conn = new SqlConnection(_conexion))
            {
                DynamicParameters DP = new DynamicParameters();
                foreach (KeyValuePair<string, dynamic> item in P)
                {
                    DP.Add(item.Key, item.Value);
                }

                return conn.Query<T>(SP, param: DP, commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(Dictionary<string, dynamic> P, string SP)
        {
            using (IDbConnection conn = new SqlConnection(_conexion))
            {
                DynamicParameters DP = new DynamicParameters();
                foreach (KeyValuePair<string, dynamic> item in P)
                {
                    DP.Add(item.Key, item.Value);
                }

                return await conn.QueryAsync<T>(SP, param: DP, commandType: CommandType.StoredProcedure);
            }
        }
    }
}