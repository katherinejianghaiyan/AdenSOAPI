﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

namespace WxOrderTransfer
{
    public class testhelper
    {
        //public static string baseConn = ConfigurationManager.ConnectionStrings["default"].ToString();
        //public static string customerConn = ConfigurationManager.ConnectionStrings["custom"].ToString();
        //public static string customerAllConn = ConfigurationManager.ConnectionStrings["customAll"].ToString();
        //private static string salesorderConn = "";//ConfigurationManager.ConnectionStrings["salesorder"].ToString();
        //public static string gladisConn = ConfigurationManager.ConnectionStrings["gladis"].ToString();
        //public static string purchasepriceconn = ConfigurationManager.ConnectionStrings["purchaseprice"].ToString();
        //public static string purchaseorderconn = ConfigurationManager.ConnectionStrings["purchaseorder"].ToString();
        //public static string sfeed = ConfigurationManager.ConnectionStrings["sfeed"].ToString();
        //public static string sfeedPicUrl = ConfigurationManager.ConnectionStrings["sfeedPicUrl"].ToString();

        public static string SalesorderConn()
        {
                return ConfigurationManager.ConnectionStrings["salesorder"].ToString();
        }
        public static List<Aden.Model.SOMastData.Company> GetEntityList<T>(string v)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取数据表
        /// </summary>
        /// <param name="conn">连接字符串</param>
        /// <param name="sql">单一执行的Sql查询语句</param>
        /// <param name="tableName">表名</param>
        /// <returns>数据表</returns>
        public static DataTable GetDataTable(string conn, string sql, string tableName = "DataTable")
        {
            return GetDataTable(conn, sql, CommandType.Text, tableName, null);
        }

        /// <summary>
        /// 获取数据表
        /// </summary>
        /// <param name="conn">连接字符串</param>
        /// <param name="sql">单一执行的Sql查询语句</param>
        /// <param name="parameters">参数数组</param>
        /// <param name="tableName">表名</param>
        /// <returns>数据表</returns>
        public static DataTable GetDataTable(string conn, string sql, SqlParameter[] parameters, string tableName = "DataTable")
        {
            return GetDataTable(conn, sql, CommandType.Text, tableName, parameters);
        }

        /// <summary>
        /// 获取数据表
        /// </summary>
        /// <param name="conn">连接字符串</param>
        /// <param name="sql">单一执行的Sql查询语句</param>
        /// <param name="commandType">查询类型,文本,存储过程...</param>
        /// <param name="parameters">查询参数</param>
        /// <returns>数据表</returns>
        private static DataTable GetDataTable(string conn, string sql, CommandType commandType, string tableName, SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(conn))
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = commandType;
                    command.CommandTimeout = 600;
                    //如果传入参数@parameter, 则添加参数
                    if (parameters != null && parameters.Length > 0)
                    {
                        foreach (SqlParameter parameter in parameters)
                            command.Parameters.Add(parameter);
                    }
                    connection.Open();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable data = new DataTable(tableName);
                        adapter.Fill(data);
                        return data;
                    }
                }
            }
        }

               

        
        private static List<T> GetList<T>(string conn, string sql, CommandType commandType, SqlParameter[] parameters,
            Func<SqlDataReader, List<T>> func)
        {
            using (SqlConnection connection = new SqlConnection(conn))
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = commandType;
                    command.CommandTimeout = 600;
                    //如果传入参数@parameter, 则添加参数
                    if (parameters != null && parameters.Length > 0)
                    {
                        foreach (SqlParameter parameter in parameters)
                            command.Parameters.Add(parameter);
                    }
                    Thread t = new Thread(() =>
                    {
                        connection.Open();
                    });
                    t.Start();
                    t.Join(2000);
                    if (connection.State != ConnectionState.Open)
                    {
                        t.Abort();
                        throw new Exception("Can't connect database");
                    }
                    List<T> result = func(command.ExecuteReader(CommandBehavior.CloseConnection));
                    command.Parameters.Clear();
                    return result;
                }
            }
        }

        public static int Execute(string salesorderConn, StringBuilder sqladdwindow)
        {
            throw new NotImplementedException();
        }

        
        



        /// <summary>
        /// 获取第一行第一列数据
        /// </summary>
        /// <param name="conn">连接字符串</param>
        /// <param name="sql">单一执行的Sql查询语句</param>
        /// <returns>单一数据</returns>
        public static object GetDataScalar(string conn, string sql)
        {
            return GetDataScalar(conn, sql, CommandType.Text, null);
        }

        /// <summary>
        /// 获取第一行第一列数据
        /// </summary>
        /// <param name="conn">连接字符串</param>
        /// <param name="sql">单一执行的Sql查询语句</param>
        /// <param name="commandType">查询类型,文本,存储过程</param>
        /// <param name="parameters">查询参数</param>
        /// <returns>单一数据</returns>
        public static object GetDataScalar(string conn, string sql, CommandType commandType, SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(conn))
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = commandType;
                    command.CommandTimeout = 600;
                    //如果传入参数@parameter, 则添加参数
                    if (parameters != null && parameters.Length > 0)
                    {
                        foreach (SqlParameter parameter in parameters)
                            command.Parameters.Add(parameter);
                    }
                    connection.Open();
                    return command.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// 增,删,改数据库操作
        /// </summary>
        /// <param name="conn">连接字符串</param>
        /// <param name="sql">操作语句</param>
        /// <returns>执行成功记录数</returns>
        public static int Execute(string conn, string sql)
        {
            return Execute(conn, sql, CommandType.Text, null);
        }

        public int Execute1(string conn, string sql)
        {
            return Execute(conn, sql, CommandType.Text, null);
        }

        /// <summary>
        /// 增,删,改数据库操作
        /// </summary>
        /// <param name="conn">连接字符串</param>
        /// <param name="sql">操作语句</param>
        /// <param name="commandType">操作类型,文本,存储过程</param>
        /// <param name="parameters">操作参数</param>
        /// <returns>执行成功记录数</returns>
        public static int Execute(string conn, string sql, CommandType commandType, SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(conn))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();
                using (SqlCommand command = new SqlCommand(sql, connection, transaction))
                {
                    command.CommandType = commandType;
                    command.CommandTimeout = 600;
                    //如果传入参数@parameter, 则添加参数
                    if (parameters != null && parameters.Length > 0)
                    {
                        foreach (SqlParameter parameter in parameters)
                            command.Parameters.Add(parameter);
                    }
                    try
                    {
                        int r = command.ExecuteNonQuery();
                        transaction.Commit();
                        return r;
                    }
                    catch //(Exception ee)
                    {
                        transaction.Rollback();
                        return 0;
                    }
                }
            }
        }
        /// <summary>
        /// Angel
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="execSQL"></param>
        /// <param name="selectSQL"></param>
        /// <returns></returns>
        public static DataTable ExecAndGetDataTable(string conn, string execSQL, string selectSQL)
        {
            if (string.IsNullOrWhiteSpace(execSQL) || string.IsNullOrWhiteSpace(selectSQL)) return null;
            DataSet data = new DataSet();
            using (SqlConnection connection = new SqlConnection(conn))
            {
                connection.Open();
                try
                {
                    SqlTransaction sqlTrans = connection.BeginTransaction();
                    try
                    {
                        //执行SQL
                        using (SqlCommand sqlCmd = new SqlCommand("", connection, sqlTrans))
                        {
                            sqlCmd.CommandType = CommandType.Text;
                            sqlCmd.CommandTimeout = 200;

                            //执行
                            sqlCmd.CommandText = execSQL;
                            sqlCmd.ExecuteNonQuery();

                            //查询
                            sqlCmd.CommandText = selectSQL;
                            SqlDataAdapter sqlAdpt = new SqlDataAdapter(sqlCmd);
                            //sqlAdpt.SelectCommand = sqlCmd;
                            sqlAdpt.Fill(data, "Table0");

                            sqlTrans.Commit();

                            if (data != null && data.Tables.Count > 0 && data.Tables[0].Rows.Count > 0)
                                return data.Tables[0];

                            return null;
                        }
                    }
                    catch (Exception e) { throw e; }
                    finally
                    {
                        try { sqlTrans.Rollback(); }
                        catch { }
                    }
                }
                catch (Exception ex) { throw ex; }
                finally { connection.Close(); }
            }
        }
    }
}
