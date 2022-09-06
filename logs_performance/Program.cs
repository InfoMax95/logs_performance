using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace logs_performance
{
    internal class Program
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof(Program));
        static void Main(string[] args)
        {
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

            //log4net.Config.XmlConfigurator.Configure();
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
            bool write = true;
            bool read = true;
            while (true)
            {
                Parallel.Invoke(
                  () =>
                  {
                      if (write)
                      {
                          InsertLogs($"document", $"message", DateTime.Now);
                      }
                  },
                  () =>
                  {
                      if (read)
                      {
                          List<GetLogs> lists = ReadLogs();
                          foreach (GetLogs list in lists)
                          {
                              Console.WriteLine(list);
                          }
                      }
                  });

                System.Threading.Thread.Sleep(3000);
            }
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
                Console.WriteLine(ex.Message);
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
                        cmd.Parameters.Add("@id ", SqlDbType.NVarChar, 250);
                        cmd.Parameters.Add("@message ", SqlDbType.NVarChar, 400);
                        cmd.Parameters.Add("@t ", SqlDbType.DateTime);
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
            catch (Exception ex)
            {
                Console.WriteLine("Errore intercettato: {0}", ex.Message);
                return logs;
            }
        }


    }
}
