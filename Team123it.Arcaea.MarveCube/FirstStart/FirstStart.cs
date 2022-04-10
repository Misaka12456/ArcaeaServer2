using static Microsoft.VisualBasic.Information;
using static Team123it.Arcaea.MarveCube.GlobalProperties;
using System;
using System.Collections.Generic;
using c = System.Console;
using MySql.Data.MySqlClient;
using System.Net.NetworkInformation;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Reflection;
using Newtonsoft.Json;

namespace Team123it.Arcaea.MarveCube.FirstStart
{
	public sealed class FirstStart
	{
		public static void FastInitialize()
		{
			var main = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "data"));
			if (!main.Exists) main.Create();
			File.WriteAllBytes(Path.Combine(AppContext.BaseDirectory, "data", "config.json"), ReadEmbeddedResources("Team123it.Arcaea.MarveCube.FirstStartData.ConfigExample.json"));
			c.WriteLine("请打开{程序根目录}\\data\\config.json, 阅读完毕注释后按照注释填写配置信息, 并在保存时删除所有注释");
			c.WriteLine("(如果您使用的是Linux等系统, 请重新打开一个新的ssh连接编辑对应的文件)");
			c.WriteLine("以上均完成后单击任意键继续");
DoInitialize:
			c.ReadKey(true);
			try
			{
				string initSQLCodes = Encoding.UTF8.GetString(ReadEmbeddedResources("Team123it.Arcaea.MarveCube.FirstStartData.Initialization.sql"));
				var conn = new MySqlConnection(GetDatabaseConnectNoDBNameURL(out string dbName));
				conn.Open();
				var cmd = conn.CreateCommand();
				cmd.CommandText = $"DROP DATABASE IF EXISTS {dbName};";
				cmd.ExecuteNonQuery();
				conn.Close();
				conn = new MySqlConnection(DatabaseConnectURL);
				conn.Open();
				cmd = conn.CreateCommand();
				cmd.CommandText = initSQLCodes;
				cmd.ExecuteNonQuery();
				conn.Close();
				c.WriteLine("数据库初始化成功完成");
			}
			catch (MySqlException)
			{
				c.WriteLine("无法连接到数据库, 请检查配置信息是否填写有误后单击任意键继续");
				goto DoInitialize;
			}
			catch (JsonException)
			{
				c.WriteLine("配置信息填写有误, 请重新填写, 注意一定要删除所有注释");
				c.WriteLine("完成后单击任意键继续");
				goto DoInitialize;
			}
			var worldMap = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "data", "static", "WorldMap"));
			var songs = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "data", "static", "Songs"));
			if (!worldMap.Exists) worldMap.Create();
			if (!songs.Exists) songs.Create();
			c.WriteLine("所有初始化进程成功完成, 等待主程序加载");
			return;
		}

		[Obsolete("StartWizard() 已被弃用。 请改用 FastInitalize()。")]
		public static void StartWizard()
		{
			try
			{
				Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "data"));
				string dbIP, dbUser, dbPassword, dbName, httpsCerPass;
				uint dbPort, listenPort;
				c.Clear();
				c.WriteLine("Welcome to 123 Marvelous Cube Configuration Wizard.");
				c.WriteLine("This wizard will lead you to finish the remaining steps of first-start configuration.");
				c.WriteLine("You needs to prepare the following things:");
				c.WriteLine("(1)MySQL [Must be 8.0+]");
				c.WriteLine("(2)The running port that you want to bind for Internet visits");
				c.WriteLine("(3)HTTPS Certificate for your Internet domain where 123 Marvelous Cube runs above");
				c.WriteLine();
				c.WriteLine("If you finished preparing things above, press any key to start wizard.");
				c.ReadKey(true);
				c.WriteLine("1) 123 Marvelous Cube(Called 'System' Below) is the API server for Extended Arcaea Fanmade Project - \"Arcaea\".");
				c.WriteLine("To save the data of players and maps and so on, system uses MySQL as the data store.");
				c.WriteLine("System needs the MySQL version aboving or equaling 8.0.");
				while (true)
				{
					c.WriteLine("Please input the MySQL Connect IP for system using(do not contain protocol prefix, like 'http' or 'https'):");
					string i = c.ReadLine();
					if (string.IsNullOrWhiteSpace(i))
					{
						continue;
					}
					else if (i.Trim().ToLower().StartsWith("https://") || i.Trim().ToLower().StartsWith("http://"))
					{
						c.WriteLine("Please do not contain protocol prefix in the Connect IP.");
						continue;
					}
					else
					{
						dbIP = i.Trim();
						break;
					}
				}
				while (true)
				{
					c.WriteLine("Please input the MySQL Connect Port for system using(Default:3306):");
					string i = c.ReadLine();
					if (string.IsNullOrWhiteSpace(i))
					{
						dbPort = 3306;
						break;
					}
					else if (IsNumeric(i))
					{
						c.WriteLine("Please input a valid port number.");
						continue;
					}
					else if (uint.TryParse(i, out uint uint_i) && uint_i > 0 && uint_i <= 65535)
					{
						dbPort = uint_i;
						break;
					}
					else
					{
						c.WriteLine("Please input a valid port number.");
						continue;
					}
				}
				while (true)
				{
					c.WriteLine("Please input the MySQL user name for system using:");
					string i = c.ReadLine();
					if (string.IsNullOrWhiteSpace(i))
					{
						continue;
					}
					else if (i.Trim().ToLower() == "root")
					{
						c.WriteLine("Warning: Using the super administrator user 'root' may cause security problems after the whole api starts.");
						c.WriteLine("Are you sure to use 'root' as the user for system using?(Y/N,Default:N)");
						if (c.ReadLine().Trim().ToLower() == "y")
						{
							dbUser = "root";
							break;
						}
						else
						{
							continue;
						}
					}
					else
					{
						dbUser = i.Trim();
						break;
					}
				}
				while (true)
				{
					c.WriteLine("Please input the MySQL password for system using(System will store as Base64 encrypted,so please don't worry security problems):");
					string i = c.ReadLine();
					if (string.IsNullOrEmpty(i))
					{
						continue;
					}
					else
					{
						dbPassword = i;
						break;
					}
				}
				while (true)
				{
					c.WriteLine("Please input the API Server Data Database Name(Default:arcaea):");
					string i = c.ReadLine();
					if (string.IsNullOrWhiteSpace(i))
					{
						dbName = "arcaea";
						break;
					}
					else
					{
						dbName = i.Trim().ToLower();
						break;
					}
				}
				c.WriteLine("OK, we have got enough information for MySQL connection.");
				c.WriteLine("Please check the details:");
				c.WriteLine("MySQL Connect IP:{0}", dbIP);
				c.WriteLine("MySQL Connect Port:{0}", dbPort);
				c.WriteLine("MySQL User:{0}", dbUser);
				c.WriteLine("MySQL User Password:{0}", dbPassword);
				c.WriteLine("MySQL Database Name:{0}", dbName);
				c.WriteLine("Generated Connection String: 'server={0};port={1};user={2};password={3};database={4};'", dbIP, dbPort, dbUser, dbPassword, dbName);
				c.WriteLine("Press any key to try connecting to server. If connection succeeded, wizard will automatically create the database instantly.");
				c.WriteLine("If the connection failed, wizard cannot continue. So please be sure that the MySQL Service is available before pressing any key.");
				c.ReadKey(true);
				while (true)
				{
					try
					{
						var conn = new MySqlConnection(string.Format("server={0};port={1};user={2};password={3};", dbIP, dbPort, dbUser, dbPassword));
						conn.Open();
						c.WriteLine("MySQL Service connected successfully.");
						var cmd = conn.CreateCommand();
						cmd.CommandText = $"SELECT COUNT(*) FROM information_schema.SCHEMATA WHERE SCHEMA_NAME='{dbName}';";
						if ((int)cmd.ExecuteScalar() != 1) //数据库不存在
						{
							c.WriteLine($"Cannot find database '{dbName}', try creating it...");
							cmd.CommandText = $"CREATE DATABASE `{dbName}`;";
							int result = cmd.ExecuteNonQuery();
							conn.Close();
							if (result == 1)
							{
								c.WriteLine($"Database '{dbName}' created successfully.");
								break;
							}
							else
							{
								c.WriteLine($"Database '{dbName}' created failed.");
							}
						}
						else
						{
							conn.Close();
							c.WriteLine($"Found existing database '{dbName}'");
							break;
						}
						c.WriteLine("System cannot finish test action. Press any key to try again.");
						c.ReadKey(true);
						continue;
					}
					catch (MySqlException ex)
					{
						c.WriteLine($"System met an exception thrown by MySQL processor: {ex.Message} (code={ex.Code})");
						c.WriteLine("Press any key to try again.");
						c.ReadKey(true);
						continue;
					}
				}
				c.WriteLine("System successfully finished the test to connect MySQL database.");
				c.WriteLine("Press any key to continue wizard.");
				c.ReadKey(true);
				c.WriteLine("2) As we all know, the whole api should be run on a Internet domain with a certain port so that the client can visit this api server.");
				c.WriteLine("Please make sure that you have binded your Internet domain to current computer, or please bind one domain.");
				c.WriteLine("If you have binded the domain, press any key to continue wizard.");
				c.ReadKey(true);
				while (true)
				{
					c.WriteLine("Please input the port that you want to run the api server(Every client will connect the api by visiting your domain and this port)(Default:443):");
					string i = c.ReadLine();
					if (string.IsNullOrWhiteSpace(i))
					{
						listenPort = 443;
						break;
					}
					else if (IsNumeric(i))
					{
						c.WriteLine("Please input a valid port number.");
						continue;
					}
					else if (uint.TryParse(i, out uint uint_i) && uint_i > 0 && uint_i <= 65535)
					{
						if (isPortInUse(uint_i))
						{
							c.WriteLine($"The port you inputed(uint_i) are used by another program now.");
							c.WriteLine("Sure to set this port as the api server listen port(running port)?(Y/N,Default:N)");
							if (c.ReadLine().Trim().ToLower() == "y")
							{
								listenPort = uint_i;
								break;
							}
							else
							{
								continue;
							}
						}
						listenPort = uint_i;
						break;
					}
					else
					{
						c.WriteLine("Please input a valid port number.");
						continue;
					}
				}
				c.WriteLine($"Successfully set the listen port(running port) of the api server to {listenPort}.");
				c.WriteLine("Press any key to continue wizard.");
				c.ReadKey(true);
				c.WriteLine("3) For the security, the api will run above SSL protocol(https).");
				c.WriteLine("Though, it is necessary to use an SSL certificate for system in order to run above https normally.");
				c.WriteLine("System supported certificate type: X.509 PKCS#12 Certificate(*.pfx).");
				c.WriteLine("If you do not have a SSL certificate of your domain, please buy it on Internet.");
				c.WriteLine("If you have a SSL certificate of your domain, press any key to continue wizard.");
				c.ReadKey(true);
				while (true)
				{
					c.WriteLine("Please input the SSL certificate file absolute path:");
					string i = c.ReadLine();
					if (string.IsNullOrWhiteSpace(i))
					{
						continue;
					}
					else if (!File.Exists(i))
					{
						c.WriteLine("Cannot find the file by given path, please re-input again.");
						continue;
					}
					else
					{
						File.Copy(i,Path.Combine(AppContext.BaseDirectory, "data", "https.pfx"));
						break;
					}
				}
				while (true)
				{
					c.WriteLine("Please input the password of the SSL certificate(System will store as Base64 encrypted,so please don't worry security problems):");
					string i = c.ReadLine();
					if (string.IsNullOrEmpty(i))
					{
						continue;
					}
					else
					{
						try
						{
							var cer2 = new X509Certificate2(Path.Combine(AppContext.BaseDirectory,"data","https.pfx"), i);
							httpsCerPass = i;
							break;
						}
						catch
						{
							c.WriteLine("You inputed the wrong password, please try again.");
							continue;
						}
					}
				}
				c.WriteLine("OK we have got all the information for the api server.");
				c.WriteLine("Press any key to save configuration, initialize the database and start the api server.");
				SaveAndInitialize(dbIP, dbPort, dbUser, dbPassword, dbName, listenPort, httpsCerPass);
				c.WriteLine("Wizard finished");
			}
			catch(Exception ex)
			{
				c.WriteLine("Sorry but wizard met an fatal exception. Wizard will exit.");
				c.WriteLine("Please send the following exception details to us for the fastest repair(s).");
				c.WriteLine("-----Technical Information-----");
				c.WriteLine($"Exception: {ex.GetType().ToString()}");
				c.WriteLine($"Message: {ex.Message}");
				c.WriteLine($"StackTrace: {ex.StackTrace}");
				c.WriteLine("-----End of Technical Information-----");
				c.WriteLine("Press any key to exit wizard.");
				c.ReadKey(true);
				Environment.Exit(0);
			}
		}

		[Obsolete]
		private static bool isPortInUse(uint port)
		{
			bool inUse = false;
			var ipProperties = IPGlobalProperties.GetIPGlobalProperties();
			var iPEndPoints = ipProperties.GetActiveTcpListeners();
			foreach (var endPoint in iPEndPoints)
			{
				if (endPoint.Port == port)
				{
					inUse = true;
					break;
				}
			}
			return inUse;
		}

		[Obsolete]
		private static void SaveAndInitialize(string dbIP,uint dbPort,string dbUser,string dbPassword,string dbName,uint listenPort,string httpsCerPass)
		{
			var save = new JObject() 
			{
				{"settings",new JObject()
					{
						{"isMaintaining",false }
					} 
				},
				{"config",new JObject()
					{
						{ "dbIP", dbIP },
						{ "dbPort", dbPort },
						{ "dbUser", dbUser },
						{ "dbPass", Convert.ToBase64String(Encoding.UTF8.GetBytes(dbPassword)) },
						{ "dbName", dbName },
						{ "listenPort",listenPort },
						{ "httpsCerPass", Convert.ToBase64String(Encoding.UTF8.GetBytes(httpsCerPass)) }
					} 
				}
			};
			File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "data", "config.json"), save.ToString(), Encoding.UTF8);
			c.WriteLine("Configuration Saved");
			var dbSQLCommands = new List<string>();
			string p = "Team123it.Arcaea.MarveCube.FirstStartData.db_"; //p=prefix
			dbSQLCommands.Add(Encoding.UTF8.GetString(ReadEmbeddedResources($"{p}bests.sql")));
			c.WriteLine("Read table structure: bests");
			dbSQLCommands.Add(Encoding.UTF8.GetString(ReadEmbeddedResources($"{p}friends.sql")));
			c.WriteLine("Read table structure: friends");
			dbSQLCommands.Add(Encoding.UTF8.GetString(ReadEmbeddedResources($"{p}logins.sql")));
			c.WriteLine("Read table structure: logins");
			dbSQLCommands.Add(Encoding.UTF8.GetString(ReadEmbeddedResources($"{p}users.sql")));
			c.WriteLine("Read table structure: users");
			dbSQLCommands.Add(Encoding.UTF8.GetString(ReadEmbeddedResources($"{p}user_chars.sql")));
			c.WriteLine("Read table structure: user_chars");
			dbSQLCommands.Add(Encoding.UTF8.GetString(ReadEmbeddedResources($"{p}user_world.sql")));
			c.WriteLine("Read table structure: user_world");
			dbSQLCommands.Add(Encoding.UTF8.GetString(ReadEmbeddedResources($"{p}world_songplay.sql")));
			c.WriteLine("Read table structure: world_songplay");
			dbSQLCommands.Add(Encoding.UTF8.GetString(ReadEmbeddedResources($"{p}fixed_characters.sql")));
			c.WriteLine("Read table structure: fixed_characters");
			dbSQLCommands.Add(Encoding.UTF8.GetString(ReadEmbeddedResources($"{p}fixed_properties.sql")));
			c.WriteLine("Read table structure: fixed_properties");
			dbSQLCommands.Add(Encoding.UTF8.GetString(ReadEmbeddedResources($"{p}fixed_songs.sql")));
			c.WriteLine("Read table structure: fixed_songs (with song data)");
			var conn = new MySqlConnection(string.Format("server={0};port={1};user={2};password={3};database={4};", dbIP, dbPort, dbUser, dbPassword, dbName));
			conn.Open();
			c.WriteLine($"Database connected to: {dbIP}:{dbPort}");
			var cmd = conn.CreateCommand();
			foreach(string dbSQLCmd in dbSQLCommands)
			{
				cmd.CommandText = dbSQLCmd;
				c.WriteLine($"Creating table\r\nCommand Details: {dbSQLCmd}");
				cmd.ExecuteNonQuery();
			}
			conn.Close();
			c.WriteLine("Database structure all built");
			return;
		}

		public static byte[] ReadEmbeddedResources(string resPath)
		{
			var assembly = Assembly.GetExecutingAssembly();
			var stream = new BufferedStream(assembly.GetManifestResourceStream(resPath));
			byte[] data = new byte[stream.Length];
			stream.Read(data, 0, (int)stream.Length);
			stream.Close();
			return data;
		}
	}
}
