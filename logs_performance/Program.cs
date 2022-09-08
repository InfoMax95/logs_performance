using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Threading;
using log4net;

namespace logs_performance
{
    internal class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));    
        //private static readonly ILog log = LogManager.GetLogger(typeof(Program));
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            // invoco i due metodi parallelamente tramite il metodo della classe Parallel->Invoke()
            Operation op = new Operation();
            Parallel.Invoke(
            () =>
            {
                // creo file e inserisco prime informazioni nel db
                int i = 0;
                while (i < (i + 1))
                {
                    foreach (string type in op.documentType)
                    {
                        op.WriteCharacters(i, type);
                        DateTime getCreationTime = File.GetCreationTime($@"C:\Users\m.gasaro.ext\Documents\test_bench\MyTest_{type}_{i}.txt");
                        InsertLogs($"{type}_{i}", "Create", getCreationTime);
                    }
                    i++;
                    Thread.Sleep(1000);
                }
            },
            () =>
            {
                // leggo file dal db e scrivo dentro il file, poi ritrascrivo questa operazione nel db
                int i = 0;
                while(i < (i + 1))
                {
                    List<GetLogs> lists = ReadLogs();
                    foreach (GetLogs list in lists)
                    {
                        log.DebugFormat("PK: {0}\nIdentify: {1}\nMessage: {2}\nTimeStamp: {3}\nInsertDate: {4}", list.PK, list.Identify, list.Message, list.TimeStamp, list.InsertDate);
                    }
                    i++;
                    Thread.Sleep(2000);
                }
            },
            async () =>
            {
                // creo file e inserisco prime informazioni nel db
                await Task.Delay(5000);
                int i = 0;
                while (i < (i + 1))
                {
                    foreach (string type in op.documentType)
                    {
                        op.WriteInto(i, type);
                        Console.WriteLine("Update con successo!");
                        DateTime getUpdateTime = File.GetLastWriteTime($@"C:\Users\m.gasaro.ext\Documents\test_bench\MyTest_{type}_{i}.txt");
                        InsertLogs($"{type}_{i}", "Update", getUpdateTime);
                        Thread.Sleep(10000);
                    }
                    i++;
                    Thread.Sleep(10000);
                }
            });
            
            // tengo aperta la console
            Console.Read();

            



            // verificare connessione con db
            /*
               using(SqlConnection conn = new SqlConnection(connectionString))
               {
                   try
                   {
                       conn.Open();
                       // Do what you please here        
                   }
                   catch (Exception ex)
                   {
                       // Write error to file
                       File.Append(..., 
                           DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + " " + 
                           ex.Message);
                   }
                   finally 
                   { 
                       conn.Close();
                   } 
               } 
           */

            //[questo metodo non funziona]
            //Parallel.Invoke(InsertLogs($"document", $"message", DateTime.Now), ReadLogs());

            //[questo invece funziona ma non esprime in ms il tempo che ci mette per compiere l'operazione]
            //e inoltre bisogna integrare creazione di file
            //for(int i = 201; i < 300; i++)
            //{
            //    InsertLogs($"document_{i}", $"message{i}", DateTime.Now);
            //}
            //List<GetLogs> logs = ReadLogs();
            //foreach(GetLogs log in logs)
            //{
            //    Console.WriteLine(log);
            //}
            //Console.Read();
            //string[] identifyType = { "MATMAS", "LOIBOM", "LOIROI", "JISORD" };
            //foreach (string ident in identifyType)
            //{
            //    InsertLogs(ident, "message", DateTime.Now);
            //}

            /* invochiamo parallelamente due metodi 
             * all'interno di un ciclo continuo 
             * che non interferiscono l'uno con l'altro
             */
            /*   bool write = true;
               bool read = true;
               Parallel.Invoke(
                   () =>
                   {
                       while (true) if (write) InsertLogs($"document", $"message", DateTime.Now);
                   },
                   () =>
                   {
                       while (true)
                       {
                           if (read)
                           {
                               List<GetLogs> lists = ReadLogs();
                               foreach (GetLogs list in lists)
                               {
                                   Console.WriteLine(list);
                               }
                           }
                       }                   
                   });
            */


        }

        private void __OldMethod()
        {
            // definisco stringa di connessione db
            string connectionString = ConfigurationManager.ConnectionStrings["DBLog"].ConnectionString;

            //definisco stringa per la stored procedure
            string stored_proc = "SP_Log";

            DateTime startMethod = DateTime.Now;
            List<GetLogs> entryMod = new List<GetLogs>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(stored_proc, conn))
                    {
                        conn.Open();
                        DataTable dt = new DataTable();
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@id ", SqlDbType.NVarChar, 250);
                        cmd.Parameters.Add("@message ", SqlDbType.NVarChar, 400);
                        cmd.Parameters.Add("@t ", SqlDbType.DateTime).Value = DateTime.MinValue;

                        //using (SqlDataReader dr = cmd.ExecuteReader())
                        //{
                        //    while (dr.Read())
                        //    {
                        //        GetLogs l = new GetLogs();
                        //        l.Identify = (string)dr["Identify"];
                        //        l.Message = (string)dr["Message"];
                        //        DateTime tempEntry = DateTime.MinValue;
                        //        l.TimeStamp = tempEntry;
                        //        entryMod.Add(l);
                        //    }
                        //    dr.Close();
                        //    Console.WriteLine(entryMod);

                        //}
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine(string.Format("new - SqlException : {0}", sqlEx.Message));
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("new- Exception : {0}", ex.Message));
            }
        }

        private static void InsertLogs(string setIdentify, string setMessage, DateTime setTime)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DBLog"].ConnectionString;
            string stored_proc = "SP_Log";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(stored_proc, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@id ", SqlDbType.NVarChar, 250).Value = setIdentify.ToString();
                        cmd.Parameters.Add("@message ", SqlDbType.NVarChar, 400).Value = setMessage.ToString();
                        cmd.Parameters.Add("@t ", SqlDbType.DateTime).Value = setTime;
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("Data insert correctly!!");
                        conn.Close();
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore metodo insertlogs: {ex.Message}");
            }
        }

        private static List<GetLogs> ReadLogs()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DBLog"].ConnectionString;
            string stored_proc = "SP_Log_Read";
            List<GetLogs> logs = new List<GetLogs>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(stored_proc, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        //cmd.Parameters.Add("@id ", SqlDbType.NVarChar, 250);
                        //cmd.Parameters.Add("@message ", SqlDbType.NVarChar, 400);
                        //cmd.Parameters.Add("@t ", SqlDbType.DateTime);
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                GetLogs log = new GetLogs();
                                log.Identify = (string)reader["Identify"];
                                log.Message = (string)reader["Message"];
                                DateTime timeStamp = DateTime.MinValue;
                                log.TimeStamp = timeStamp;
                                DateTime insertDate = DateTime.Now;
                                log.InsertDate = insertDate;
                                logs.Add(log);
                            }
                        }
                        return logs;
                    }
                }
            }
            catch (InvalidCastException castInvalidEx)
            {
                Console.WriteLine("Errore InvalidCast intercettato: {0}", castInvalidEx.Message);
                return null;
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine("Errore SQL intercettato: {0}", sqlEx.Message);
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore intercettato: {0}", ex.Message);
                return null;
            }
        }
    }
}
