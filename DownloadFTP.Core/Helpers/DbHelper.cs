using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace DownloadFTP.Core.Helpers
{


    public class DbHelper : IDisposable
    {
        #region Private Fields

        private bool _disposed;
        private SqlConnection _conn;
        private SqlTransaction _trans;

        #endregion

        #region Public Constructors

        public DbHelper() { this.Initialize(); }
        public DbHelper(string conn) { this.Initialize(conn); }

        #endregion

        #region Destructors

        ~DbHelper()
        {
            this.Dispose(false);
        }

        #endregion

        #region Public Methods

        public DataTable ExecuteReader(string sql, CommandType commandType)
        {
            return (DataTable)this.ExecuteCommand(ReturnType.DataTable, sql, commandType, null);
        }

        public DataTable ExecuteReader(string sql, CommandType commandType, Dictionary<string, object> parameters)
        {
            return (DataTable)this.ExecuteCommand(ReturnType.DataTable, sql, commandType, parameters);
        }

        public int ExecuteNonQuery(string sql, CommandType commandType)
        {
            return (int)this.ExecuteCommand(ReturnType.NonQuery, sql, commandType, null);
        }

        public int ExecuteNonQuery(string sql, CommandType commandType, Dictionary<string, object> parameters)
        {
            return (int)this.ExecuteCommand(ReturnType.NonQuery, sql, commandType, parameters);
        }

        public object ExecuteScalar(string sql, CommandType commandType)
        {
            return this.ExecuteCommand(ReturnType.Scalar, sql, commandType, null);
        }

        public object ExecuteScalar(string sql, CommandType commandType, Dictionary<string, object> parameters)
        {
            return this.ExecuteCommand(ReturnType.Scalar, sql, commandType, parameters);
        }

        public void BeginTransaction()
        {
            if (this._trans == null)
                this._trans = this._conn.BeginTransaction();
        }

        public void EndTransaction()
        {
            if (this._trans != null)
            {
                this._trans.Commit();
                this._trans.Dispose();
            }

            this._trans = null; ;
        }

        public void RollbackTransaction()
        {
            if (this._trans != null)
            {
                this._trans.Rollback();
                this._trans.Dispose();
            }

            this._trans = null;
        }

        #endregion

        #region Private Methods

        private void Initialize()
        {
            try
            {
                this._conn = new SqlConnection();
                this._conn.ConnectionString = ConfigurationManager.ConnectionStrings["Allconnections"].ConnectionString;
                this._conn.Open();
            }
            catch (Exception ex) { throw ex; }
        }

        private void Initialize(string conn)
        {
            try
            {
                this._conn = new SqlConnection();
                this._conn.ConnectionString = conn;
                this._conn.Open();
            }
            catch (Exception ex) { throw ex; }
        }
        private object ExecuteCommand(ReturnType returnType, string sql, CommandType type, Dictionary<string, object> parameters)
        {
            object result = null;

            using (SqlCommand cmd = this._conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandType = type;

                if (parameters != null)
                {
                    foreach (KeyValuePair<string, object> kvp in parameters)
                        cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
                }

                if (this._trans != null)
                    cmd.Transaction = this._trans;

                switch (returnType)
                {
                    case ReturnType.NonQuery: result = cmd.ExecuteNonQuery(); break;
                    case ReturnType.Scalar: result = cmd.ExecuteScalar(); break;
                    case ReturnType.DataTable:
                        DataTable dataTable = new DataTable();
                        dataTable.Load(cmd.ExecuteReader());
                        result = dataTable.Copy();
                        break;
                }
            }

            return result;
        }

        #endregion

        #region Protected Virtual Methods

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    this.RollbackTransaction();

                    // Free managed code.
                    if (this._conn != null)
                    {
                        if (this._conn.State != ConnectionState.Closed)
                        {
                            this._conn.Close();
                            this._conn.Dispose();
                            this._conn = null;
                        }
                    }
                }

                this._disposed = true;
            }

            // Free unmanaged code and set null.
            this._conn = null;
        }

        #endregion

        #region IDisposable Members

        public void GetProjects()
        {

        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Inner Types

        [Flags]
        private enum ReturnType : int
        {
            NonQuery = 0,
            Scalar = 1,
            DataTable = 2
        }

        #endregion
    }


}
