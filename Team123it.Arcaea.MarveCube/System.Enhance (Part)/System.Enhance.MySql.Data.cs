using MySql.Data.MySqlClient;
using System.Collections;
using System.IO;
using System.Text;

namespace System.Enhance.MySql.Data
{
    public class MysqlExecutor
    {
        public static bool ExecuteSqlFileData(string sqlConnString, string varData)
        {
            var stream = new MemoryStream();
            var ws = new StreamWriter(stream, Encoding.UTF8);
            ws.Write(varData);
            ws.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            var rs = new StreamReader(stream, Encoding.UTF8);
            var alSql = new ArrayList();
            string commandText = "";
            string varLine = "";
            while (rs.Peek() > -1)
            {
                varLine = rs.ReadLine();
                if (varLine == "")
                {
                    continue;
                }
                if (varLine != "GO")
                {
                    commandText += varLine;
                    commandText += "\r\n";
                }
                else
                {
                    commandText += "";
                }
            }
            alSql.Add(commandText);
            rs.Close();
            try
            {
                ExecuteCommand(sqlConnString, alSql);
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private static void ExecuteCommand(string sqlConnString, ArrayList varSqlList)
        {
			using var conn = new MySqlConnection(sqlConnString);
			conn.Open();
			var cmd = conn.CreateCommand();
			try
			{
				foreach (string varcommandText in varSqlList)
				{
					cmd.CommandText = varcommandText;
					cmd.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
                throw;
			}
			finally
			{
				conn.Close();
			}
		}
    }
}
