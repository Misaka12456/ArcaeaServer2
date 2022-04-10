using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using Team123it.Arcaea.MarveCube.Controllers;
using Team123it.Arcaea.MarveCube.Core;
using static Team123it.Arcaea.MarveCube.GlobalProperties;


namespace Team123it.Arcaea.MarveCube.Bots
{
    public static class Background
    {
        /// <summary>
        /// 检查Apikey的有效性。
        /// </summary>
        /// <exception cref="BotAPIException" />
        public static void CheckApiKey(string apikey)
        {
            var conn = new MySqlConnection(DatabaseConnectURL);
            try
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*),is_banned FROM bots WHERE apikey=?apikey;";
                cmd.Parameters.Add(new MySqlParameter("?apikey", apikey));
                var rd = cmd.ExecuteReader();
                rd.Read();
                if (rd.GetInt32(0) == 1)
                {
                    if (rd.GetBoolean(1))
                    {
                        throw new BotAPIException(BotAPIException.APIExceptionType.BotIsBlocked,null);
                    }
                }
                else
                {
                    throw new BotAPIException(BotAPIException.APIExceptionType.InvalidApiKey,null);
                }
            }
            catch (BotAPIException)
            {
                throw;
            }
            finally
            {
                conn.Close();
            }
        }

        public static bool IsSafeArgument(this HttpRequest req)
        {
            foreach (var queryStr in req.Query)
            {
                if (!BotController.IsSafeArgument(queryStr.Key)) return false;
                if (!BotController.IsSafeArgument(queryStr.Value)) return false;
                continue;
            }
            return true;
        }
    }
}