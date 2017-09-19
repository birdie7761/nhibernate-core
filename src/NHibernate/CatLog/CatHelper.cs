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
            new KeyValuePair<long, String>(10,"0~10ms"),
            new KeyValuePair<long, String>(50,"10~50ms"),
            new KeyValuePair<long, String>(100,"50~100ms"),
            new KeyValuePair<long, String>(200,"100~200ms"),
            new KeyValuePair<long, String>(500,"200~500ms"),
            new KeyValuePair<long, String>(1000,"500ms~1s"),
            new KeyValuePair<long, String>(5000,"1~5s"),
            new KeyValuePair<long, String>(10000,"5~10s"),
            new KeyValuePair<long, String>(20000,"10~20s"),
            new KeyValuePair<long, String>(30000,"20~30s"),
            new KeyValuePair<long, String>(50000,"30~50s"),
            new KeyValuePair<long, String>(120000,"50~120s"),
            new KeyValuePair<long, String>(300000,"2~5m"),
            new KeyValuePair<long, String>(600000,"5~10m"),
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
                Cat.LogError(e);
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
                Cat.LogEvent(">10m", cmd.CommandText);
                return;
            }
            catch (Exception) { }
        }
    }
}
