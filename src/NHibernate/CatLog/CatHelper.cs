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

namespace NHibernate.CatLog
{
    public static class CatHelper
    {
        private static List<KeyValuePair<long, String>> durationList = new List<KeyValuePair<long, String>> 
        {
            new KeyValuePair<long, String>(10,"10ms"),
            new KeyValuePair<long, String>(20,"10~20ms"),
            new KeyValuePair<long, String>(50,"20~50ms"),
            new KeyValuePair<long, String>(100,"50~100ms"),
            new KeyValuePair<long, String>(200,"100~200ms"),
            new KeyValuePair<long, String>(500,"200~500ms"),
            new KeyValuePair<long, String>(1000,"500~1000ms"),
            new KeyValuePair<long, String>(2000,"1~2s"),
            new KeyValuePair<long, String>(5000,"2~5s"),
            new KeyValuePair<long, String>(10000,"5~10s"),
            new KeyValuePair<long, String>(30000,"10~30s"),
        };

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
                return new NullTransaction(); 
            }
            return cat;
        }

        public static void DurationEvent(Stopwatch watch, IDbCommand cmd)
        {
            try
            {
                watch.Stop();
                foreach (var rang in durationList)
                {
                    if (rang.Key > watch.ElapsedMilliseconds)
                    {
                        Cat.LogEvent(rang.Value, cmd.CommandText);
                        return;
                    }
                }
                Cat.LogEvent(">30s", cmd.CommandText);
                return;
            }
            catch (Exception) { }
        }
    }
}
