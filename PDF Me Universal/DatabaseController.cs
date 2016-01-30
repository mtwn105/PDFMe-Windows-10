using SQLitePCL;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace PDF_Me_Universal
{
    class DatabaseController
    {
        /*
        public static void ResetTable()
        {
            using (var connection = new SQLiteConnection("Storage.db"))
            {
                using (var statement = connection.Dispose
                {

                }
            }
        }
        */
        public static void CreateTable()
        {
            string path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "User.db");

            using (var connection = new SQLiteConnection(path))
            {
                using (var statement = connection.Prepare(@"CREATE TABLE DownloadList (
                                       
                                        FILENAME NVARCHAR(10),
                                        PATH NVARCHAR(1000),
                                        DATE NVARCHAR(100),
                                        SIZE NVARCHAR(100));"))
                {
                    statement.Step();
                }
            }

        }
        public static void AddDownload(string filename,string path, string date, string size)
        {
            string path1 = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "User.db");

            try
            {
                using (var connection = new SQLiteConnection(path1))
                {
                    using (var statement = connection.Prepare(@"INSERT INTO DownloadList (FILENAME,PATH,DATE,SIZE)
                                    VALUES(?,?,?,?);"))
                    {
                       
                        statement.Bind(1, filename);
                        statement.Bind(2, path);
                        statement.Bind(3, date);
                        statement.Bind(4, size);
                    
                        // Inserts data.
                        statement.Step();
                       
                        statement.Reset();
                        statement.ClearBindings();
                        Debug.WriteLine("Download Added");
                    }
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception\n" + ex.ToString());
            }
        }
        public static ObservableCollection<Downloads> getDownloads()
        {
            string path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "User.db");

            ObservableCollection<Downloads> list = new ObservableCollection<Downloads>();

            using (var connection = new SQLiteConnection(path))
            {
                using (var statement = connection.Prepare(@"SELECT * FROM DownloadList;"))
                {

                    while (statement.Step() == SQLiteResult.ROW)
                    {

                        list.Add(new Downloads()
                        {
                            FileName = (string)statement[0],
                            Path = (string)statement[1],
                            Date = (string)statement[2],
                            Size = (string)statement[3]

                          
                        });

                        Debug.WriteLine(statement[0] + " ---" + statement[1] + " ---" + statement[2]);
                    }
                }
            }
            return list;
        }
      /* public static void DeleteDownload(string date)
        {
           
           
                using (var connection = new SQLiteConnection("Storage.db"))
                {

                    using (var statement = connection.Prepare(@"DELETE FROM DownloadList WHERE ID=?"))
                    {
                        
                        statement.Bind(1, "1");
                        statement.Step();
                    }
                    //  getValues();
                   

                }
                /*
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            */
        
    
    }
}
