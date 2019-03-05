using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.ServiceModel;
using Aden.Model.Interface;

namespace Aden.Util.Database
{
    public class NetTcpHelper
    {
        private static string ipAddr = "202.106.70.195:2015/payservice";

        public static List<T> GetEntityList<T>(string ip, string db, string sql)
        {
            return GetDataTable(ip,db,sql).ToEntityList<T>();
        }

        public static T GetEntity<T>(string ip, string db, string sql)
        {
            return GetDataTable(ip, db, sql).ToEntityList<T>().FirstOrDefault();
        }

        private static T Process<T>(string ip, Func<IWcfAdenPay,T> func)
        {
            try
            {
                ipAddr = string.Format("net.tcp://{0}", ip);
                using (ChannelFactory<IWcfAdenPay> cf = new ChannelFactory<IWcfAdenPay>("AdenPayClientService"))
                {
                    //net.tcp://10.1.8.210:2015/payservice
                    cf.Endpoint.Address = new EndpointAddress(ipAddr);
                    IWcfAdenPay instance = cf.CreateChannel();
                    return func(instance);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private static  DataTable GetDataTable(string ip, string db, string sql)
        {
            try
            {
                Dictionary<string, string> dicSql = new Dictionary<string, string> { {"table",sql } };
                return Process<DataTable>(ip, (ins) =>
                   {
                       return ins.GetDataSetFromClient(db, dicSql).Tables[0];
                   });
                //ipAddr = string.Format("net.tcp://{0}", ip);
                //using (ChannelFactory<IWcfAdenPay> cf = new ChannelFactory<IWcfAdenPay>("AdenPayClientService"))
                //{
                //    //net.tcp://10.1.8.210:2015/payservice
                //    cf.Endpoint.Address = new EndpointAddress(ipAddr);
                //    IWcfAdenPay instance = cf.CreateChannel();
                //    return instance.GetDataSetFromClient(db, dicSql).Tables[0];
                //}
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public static int Execute (string ip, string db, string sql)
        {
            try
            {
                return Process<int>(ip, (ins) =>
                {
                    return ins.ExecSQLFromClient(db, sql);
                });

            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
