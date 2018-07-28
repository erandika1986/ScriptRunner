using ScriptRunner.Data;
using ScriptRunner.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;

namespace ScriptRunner.Business
{
    public class DbServerManager
    {
        public DbServerManager()
        {

        }

        public string GetSettingName()
        {
            try
            {
                using (var db = new ScriptRunnerDbEntities())
                {
                    var name = "Setting - " + (db.SettingHeaders.Count() + 1).ToString();

                    return name;
                }
            }
            catch(Exception ex)
            {
                return "";
            }

        }

        public string CreateServerConnectionString(string serverName,bool integratedSecurity,string username,string password)
        {
            string connectionstring = string.Empty;
            if(integratedSecurity)
            {
                connectionstring = string.Format("Data Source ={0}; Integrated Security = True;",serverName);
            }
            else
            {
                connectionstring = string.Format("Data Source ={0};User Id={1};Password = {2}; ", serverName,username,password);
            }

            return connectionstring;
        }

        public string CreateDbConnectionString(string serverName, bool integratedSecurity, string username, string password, string dbName)
        {
            string connectionstring = string.Empty;

            if (integratedSecurity)
            {
                connectionstring = string.Format("Data Source={0};Initial Catalog={1};Integrated Security=SSPI;Persist Security Info=False;", serverName, dbName);
            }
            else
            {
                connectionstring = string.Format("Data Source={0};Initial Catalog={1};Integrated Security=SSPI;User ID = {2}; Password = {3}; ", serverName, dbName, username, password);
            }

            return connectionstring;
        }

        public List<DatabaseViewModel> GetDbList(string connectionString,int settingHeaderId=0)
        {
            var dbList = new List<DatabaseViewModel>();
            var settings = new List<Setting>();
            try
            {

                if (settingHeaderId != 0)
                {
                    using (var db = new ScriptRunnerDbEntities())
                    {
                        settings = db.Settings.Where(t => t.HeaderId == settingHeaderId).ToList();
                    }
                }

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT name from sys.databases", con))
                    {
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                string dbName = dr[0].ToString();
                                if (dbName != "master" && dbName != "tempdb" && dbName != "model" && dbName != "msdb")
                                {

                                    var dbVm = new DatabaseViewModel()
                                    {
                                        IsSelected = false,
                                        Name = dr[0].ToString()
                                    };

                                    if(settingHeaderId!=0)
                                    {
                                        var matchedDb = settings.FirstOrDefault(t => t.DbName == dbName);
                                        if(matchedDb!=null)
                                        {
                                            dbVm.IsSelected = true;
                                            dbVm.Id = matchedDb.Id;
                                        }
                                    }

                                    dbList.Add(dbVm);
                                }


                            }
                        }
                    }
                }

            }
            catch(Exception ex)
            {
                throw ex;
            }

            return dbList;
        }

        public bool SaveDbSettings(int settingId,string serverName, bool integratedSecurity, string username, string password,string settingName, List<DatabaseViewModel> selectedDbs)
        {
            using (var db = new ScriptRunnerDbEntities())
            {
                try
                {
                    if(settingId==0)
                    {
                        var settingHeading = new SettingHeader()
                        {
                            IntegratedSecurity = integratedSecurity,
                            Name = settingName,
                            Password = password,
                            Username = username,
                            Server = serverName,
                        };

                        db.SettingHeaders.Add(settingHeading);
                        db.SaveChanges();

                        selectedDbs.ForEach(d =>
                        {
                            var setting = new Setting()
                            {
                                DbName = d.Name,
                                HeaderId = settingHeading.Id
                            };

                            db.Settings.Add(setting);
                            db.SaveChanges();

                        });
                    }
                    else
                    {
                        var settingHeader = db.SettingHeaders.FirstOrDefault(t => t.Id == settingId);
                        settingHeader.IntegratedSecurity = integratedSecurity;
                        settingHeader.Name = settingName;
                        settingHeader.Password = password;
                        settingHeader.Username = username;
                        settingHeader.Server = serverName;

                        db.SaveChanges();


                        var savedSettings = db.Settings.Where(t => t.HeaderId == settingId).ToList();

                        //newly added Settings
                        var newItems = selectedDbs.Where(t => t.Id == 0).ToList();
                        newItems.ForEach(d =>
                        {
                            var setting = new Setting()
                            {
                                DbName = d.Name,
                                HeaderId = settingHeader.Id
                            };

                            db.Settings.Add(setting);
                            db.SaveChanges();

                        });

                        //DeletedDbs
                        var deletedDbs = (from e in savedSettings where !selectedDbs.Any(t => t.Id == e.Id) select e).ToList();
                        foreach (var item in deletedDbs)
                        {
                            db.Settings.Remove(item);
                            db.SaveChanges();
                        }
                    }


                    return true;
                }
                catch(Exception ex)
                {
                    return false;
                }
            }
        }

        public List<SettingsHeaderViewModel> GetAllSettings()
        {
            var settingHeaders = new List<SettingsHeaderViewModel>();

            using (var db = new ScriptRunnerDbEntities())
            {

                var settings = db.SettingHeaders.ToList();

                foreach(var item in settings)
                {
                    settingHeaders.Add(new SettingsHeaderViewModel()
                    {
                        Id=item.Id,
                        IntegratedSecurity=item.IntegratedSecurity,
                        Password=item.Password,
                        SettingName=item.Name,
                        UserName=item.Username,
                        ServerName=item.Server
                    });
                }
            }

            return settingHeaders;
        }

        public bool DeleteSetting(int id)
        {
            try
            {
                using (var db = new ScriptRunnerDbEntities())
                {
                    var item = db.SettingHeaders.FirstOrDefault(t => t.Id==id);
                    var settings = db.Settings.Where(t => t.HeaderId == item.Id).ToList();
                    foreach(var s in settings)
                    {
                        db.Settings.Remove(s);
                        db.SaveChanges();
                    }

                    db.SettingHeaders.Remove(item);
                    db.SaveChanges();

                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<ScriptRunResult> RunScript(string script,string scriptName,int settingId)
        {
            var executeResults = new List<ScriptRunResult>();

            using (var db = new ScriptRunnerDbEntities())
            {
                var dbHeader = db.SettingHeaders.FirstOrDefault(t => t.Id == settingId);

                var settings = db.Settings.Where(t => t.HeaderId == dbHeader.Id).ToList();

                foreach (var d in settings)
                {
                    string connectionString = CreateDbConnectionString(dbHeader.Server, dbHeader.IntegratedSecurity, dbHeader.Username, dbHeader.Password, d.DbName);

                    var result = new ScriptRunResult()
                    {
                        ConnectionString = connectionString,
                        DatabaseName = d.DbName,
                        FileName = scriptName,
                        BackGroundCode= "#009900"
                    };

                    try
                    {
                        SqlConnection conn = new SqlConnection(connectionString);

                        Server server = new Server(new ServerConnection(conn));

                        server.ConnectionContext.ExecuteNonQuery(script);

                        result.Status = true;
                        result.DisplayMessage = string.Format( "SQL Script has been executed successfully for  database {0}. \nScript Name : {1}.\nConnection String : {2}",d.DbName,scriptName,connectionString);
                        
                    }
                    catch(Exception ex)
                    {
                        result.Status = false;
                        //result.Message = string.Format("SQL Script execution has been failed for {0}. Error Message : {1}",d.DbName,ex.ToString());
                        result.DisplayMessage= string.Format("SQL Script execution has been failed for database {0}.\nConnection String : {1}.\nError Message : {2}", d.DbName,connectionString, ex.ToString());
                        result.BackGroundCode = "#FF6600";
                    }
                    finally
                    {

                    }

                    executeResults.Add(result);

                }
            }

            return executeResults;
        }
    }

    public class ScriptRunResult
    {
        public string FileName { get; set; }
        public string DatabaseName { get; set; }
        public string ConnectionString { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
        public string DisplayMessage { get; set; }
        public string BackGroundCode { get; set; }
    }
}
