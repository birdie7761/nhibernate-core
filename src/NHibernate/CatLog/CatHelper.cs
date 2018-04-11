using Org.Unidal.Cat;
using Org.Unidal.Cat.Message;
using Org.Unidal.Cat.Message.Internals;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data.Common;
using System.Configuration;

namespace NHibernate.CatLog
{
    public static class CatHelper
    {
        public static Boolean CatEnable;
        static CatHelper()
        {
            var enable = ConfigurationManager.AppSettings["CatTrasactionEnable"];
            bool blnEnable;
            if(!String.IsNullOrWhiteSpace(enable) && Boolean.TryParse(enable,out blnEnable))
            {
                CatEnable = blnEnable;
            }
        }
        
        public static Org.Unidal.Cat.Message.ITransaction NewSqlLog(IDbCommand cmd)
        {
            var cat = Cat.NewTransaction("SQL", cmd.CommandText);
            try
            {                
                if (0 < cmd.Parameters.Count)
                {
                    var param = new StringBuilder(cmd.Parameters.Count * 30);
                    param.Append("{");
                    var enumtor = cmd.Parameters.GetEnumerator();
                    while (enumtor.MoveNext())
                    {
                        var dbparam = (DbParameter)enumtor.Current;
                        if (dbparam.Direction == ParameterDirection.Input || dbparam.Direction == ParameterDirection.InputOutput)
                        {
                            param.Append(String.Concat(dbparam.ParameterName, "=", dbparam.Value.ToString(),"&"));
                        }                        
                    }
                    param.Append("}");
                    cat.AddData("param", param.ToString());
                }
                cat.Status = CatConstants.SUCCESS;
            }
            catch(Exception e)
            {
                Cat.LogError(e);
                return new NullTransaction(); 
            }
            return cat;
        }

        public static void DurationEvent(Stopwatch watch, IDbCommand cmd)
        {
            var sqlThreshold = ConfigurationManager.AppSettings["CatSQLTimeThreshold"];
            long threshold;
            if(String.IsNullOrWhiteSpace(sqlThreshold) || !long.TryParse(sqlThreshold,out threshold))
            {
                threshold = 100;
            }

            watch.Stop();
            if (threshold < watch.ElapsedMilliseconds)
            {
                Cat.LogEventDuration(watch, cmd.CommandText);
            }            
        }
    }
}
