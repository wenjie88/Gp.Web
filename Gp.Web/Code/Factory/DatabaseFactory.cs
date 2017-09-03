using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;

namespace Gp.Web.Code.Factory
{
    public class DatabaseFactory
    {

        public static Database CreateDataBase(string database)
        {
            return new Database(database);
        }

    }


    public class Database
    {
        private string connStr { get; set; }



        public Database(string databaseName)
        {
            this.connStr = ConfigurationManager.ConnectionStrings[databaseName].ConnectionString;
        }




        /// <summary>
        /// 返回数据模型List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param">参数</param>
        /// <returns></returns>
        public List<T> Query<T>(string sql, object param = null)
        {
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                return conn.Query<T>(sql, param).ToList();
            }
        }
        /// <summary>
        /// 联合查询
        /// </summary>
        /// <typeparam name="TFirst">第一个表的模型</typeparam>
        /// <typeparam name="TSecond">第二个表模型</typeparam>
        /// <typeparam name="TReturn">返回的模型</typeparam>
        /// <param name="sql"></param>
        /// <param name="map"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="buffered"></param>
        /// <param name="splitOn">查询出来的结果，从那个字段开始是属于第二个表的</param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public List<TReturn> Query<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
        {
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                return conn.Query<TFirst, TSecond, TReturn>(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType).ToList();
            }
        }

        /// <summary>
        /// 执行数据库语句 可以批量操作
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param">参数</param>
        /// <param name="transaction">是否开启事务</param>
        /// <returns></returns>
        public int Execute(string sql, object param = null, bool transaction = false)
        {
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {

                IDbTransaction tran = null;
                //是否开启事务
                if (transaction)
                {
                    conn.Open();//如果开启事务，要加open  否则会报错
                    tran = conn.BeginTransaction();
                }
                int flag = conn.Execute(sql, param, tran);
                if (tran != null) //如果开启事务
                {
                    try
                    {
                        tran.Commit();
                    }
                    catch (Exception error)
                    {
                        tran.Rollback();
                        throw (error);
                    }
                    finally
                    {
                        conn.Close();
                        conn.Dispose();
                    }
                }
                return flag;
            }
        }


        public T ExectueScalar<T>(string sql, object param = null)
        {
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                return conn.ExecuteScalar<T>(sql, param);
            }
        }

        public Exception ExecuteTran(Action<MySqlConnection, IDbTransaction> execute)
        {
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            IDbTransaction tran = conn.BeginTransaction();
            Exception error = null;
            try
            {
                execute(conn, tran);
                tran.Commit();
            }
            catch (Exception ex)
            {
                tran.Rollback();
                //throw (ex);
                error = ex;
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
            return error;
        }
    }
}