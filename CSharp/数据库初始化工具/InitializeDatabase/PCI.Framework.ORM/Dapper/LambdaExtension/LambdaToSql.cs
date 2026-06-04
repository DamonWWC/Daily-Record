//using Dapper;
using Dm;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using PCI.Framework.ORM.DapperExtensions.Sql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
//https://github.com/zqlovejyc/SQLBuilder
/*
 * lishuangquan测试BUG记录：
 * 1.mysql ：测试用例 GroupBy分组查询 用法1 生成的sql语句后面多了一个逗号（,）--已提BUG解决
 * 2.mysql：不支持full join--Mysql不使用fulljoin
 * 3.oracle:group by生成的语句：--ORACLE使用GroupBY需要注意
     SELECT * FROM AAA_TEST_A_COPY A WHERE A.Name = :Param0 GROUP BY A.Id,A.Age
     报错：ORA-00979: 不是 GROUP BY 表达式，原因为：select多少字段就要group多少字段，详情：https://www.cnblogs.com/MrZhaoyx/p/11772399.html
 * 4.oracle的insert语句，当传递对象时，生成的sql语句：--
     * INSERT INTO AAA_TEST_A_COPY (Name,Age,CreatedTime,Price,C_Number,C_Image,C_Char,C_Nvarchar2,C_TimeStamp) 
     * SELECT :Param0,:Param1,NULL,NULL,NULL,NULL,NULL,NULL,NULL FROM DUAL 
     * UNION ALL 
     * SELECT :Param2,:Param3,NULL,NULL,NULL,NULL,NULL,NULL,NULL FROM DUAL
     * 报错：ORA-00918: 未明确定义列
 */
namespace PCI.Framework.ORM.DapperExtensions.LambdaExtension
{
    #region Refrence Alias

    ////Table
    //using CusTableAttribute = LambdaToSql.TableAttribute;
    ////using SysTableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

    ////Column
    //using CusColumnAttribute = LambdaToSql.ColumnAttribute;
    ////using SysColumnAttribute = System.ComponentModel.DataAnnotations.Schema.ColumnAttribute;

    ////Key
    //using CusKeyAttribute = LambdaToSql.KeyAttribute;
    ////using SysKeyAttribute = System.ComponentModel.DataAnnotations.KeyAttribute;
    #endregion

    #region sqlPack
    /// <summary>
    /// SqlPack
    /// </summary>
    internal class SqlPack
    {
        #region Private Field
        /// <summary>
        /// tableAlias
        /// </summary>
        private static readonly List<string> tableAlias = new List<string> { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

        /// <summary>
        /// dicTableName
        /// </summary>
        private readonly Dictionary<string, string> dicTableName = new Dictionary<string, string>();

        /// <summary>
        /// tableAliasQueue
        /// </summary>
        private Queue<string> tableAliasQueue = new Queue<string>(tableAlias);
        #endregion

        #region fangwenhan add，使用委托,减少依赖
        /// <summary>
        /// 获取表名,参数：实体类型。
        /// </summary>
        public Func<Type, string> FuncGetTableName { get; set; }

        /// <summary>
        /// 获取列名，参数：实体类型，字段名
        /// </summary>
        public Func<Type, string, string> FuncGetColumnName { get; set; }
        #endregion

        #region Public Property

        /// <summary>
        /// 更新和新增时，是否对null值属性进行sql拼接操作
        /// </summary>
        public bool IsEnableNullValue { get; set; } = true;

        /// <summary>
        /// 默认T类型
        /// </summary>
        public Type DefaultType { get; set; }

        /// <summary>
        /// IsSingleTable
        /// </summary>
        public bool IsSingleTable { get; set; }

        /// <summary>
        /// SelectFields
        /// </summary>
        public List<string> SelectFields { get; set; }

        /// <summary>
        /// SelectFieldsStr
        /// </summary>
        public string SelectFieldsStr => string.Join(",", this.SelectFields);

        /// <summary>
        /// Length
        /// </summary>
        public int Length => this.Sql.Length;

        /// <summary>
        /// Sql
        /// </summary>
        public StringBuilder Sql { get; set; }

        /// <summary>
        /// DatabaseType
        /// </summary>
        public ConnectType DatabaseType { get; set; }

        /// <summary>
        /// DbParams
        /// </summary>
        public Dictionary<string, object> DbParams { get; set; }

        /// <summary>
        /// DbParamPrefix
        /// </summary>
        public string DbParamPrefix
        {
            get
            {
                switch (this.DatabaseType)
                {
                    case ConnectType.SQLite: return "@";
                    case ConnectType.SQLServer: return "@";
                    case ConnectType.MySql: return "?";
                    case ConnectType.Oracle: return ":";
                    case ConnectType.Dm: return ":";
                    case ConnectType.GBase: return ":";
                    case ConnectType.PostgreSQL: return ":";
                    case ConnectType.Db2: return "@";
                    default: return "";
                }
            }
        }

        /// <summary>
        /// 为数据库的列名加上前后缀
        /// </summary>
        public string FormatTempl
        {
            get
            {
                switch (this.DatabaseType)
                {
                    case ConnectType.SQLite: return "\"{0}\"";
                    case ConnectType.SQLServer: return "[{0}]";
                    case ConnectType.MySql: return "`{0}`";
                    case ConnectType.Oracle: return "\"{0}\"";
                    case ConnectType.Dm: return "\"{0}\"";
                    case ConnectType.GBase:return "\"{0}\"";
                    //lishuangquan modify
                    //case ConnectType.Oracle: return "{0}";
                    case ConnectType.PostgreSQL: return "\"{0}\"";
                    case ConnectType.Db2:return "\"{0}\"";
                    default: return "{0}";
                }
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// SqlPack
        /// </summary>
        public SqlPack()
        {
            this.DbParams = new Dictionary<string, object>();
            this.Sql = new StringBuilder();
            this.SelectFields = new List<string>();
        }
        #endregion

        #region Public Methods
        #region this[index]
        /// <summary>
        /// this[index]
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>char</returns>
        public char this[int index] => this.Sql[index];
        #endregion

        #region operator +
        /// <summary>
        /// operator +
        /// </summary>
        /// <param name="sqlPack">sql打包对象</param>
        /// <param name="sql">sql语句</param>
        /// <returns>SqlPack</returns>
        public static SqlPack operator +(SqlPack sqlPack, string sql)
        {
            sqlPack.Sql.Append(sql);
            return sqlPack;
        }
        #endregion

        #region Clear
        /// <summary>
        /// Clear
        /// </summary>
        public void Clear()
        {
            this.SelectFields.Clear();
            this.Sql.Clear();
            this.DbParams.Clear();
            this.dicTableName.Clear();
            this.tableAliasQueue = new Queue<string>(tableAlias);
        }
        #endregion

        #region AddDbParameter
        /// <summary>
        /// AddDbParameter
        /// </summary>
        /// <param name="parameterValue">参数值</param>
        /// <param name="parameterKey">参数名称</param>
        public void AddDbParameter(object parameterValue, string parameterKey = null)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
            {
                this.Sql.Append("NULL");
            }
            else if (string.IsNullOrEmpty(parameterKey))
            {
                var name = this.DbParamPrefix + "Param" + this.DbParams.Count;
                this.DbParams.Add(name, parameterValue);
                this.Sql.Append(name);
            }
            else
            {
                var name = this.DbParamPrefix + parameterKey;
                this.DbParams.Add(name, parameterValue);
                this.Sql.Append(name);
            }
        }
        #endregion

        #region SetTableAlias
        /// <summary>
        /// SetTableAlias
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <returns>bool</returns>
        public bool SetTableAlias(string tableName)
        {
            if (!this.dicTableName.Keys.Contains(tableName))
            {
                this.dicTableName.Add(tableName, this.tableAliasQueue.Dequeue());
                return true;
            }
            return false;
        }
        #endregion

        #region GetTableAlias
        /// <summary>
        /// GetTableAlias
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <returns>string</returns>
        public string GetTableAlias(string tableName)
        {
            if (!this.IsSingleTable && this.dicTableName.Keys.Contains(tableName))
            {
                return this.dicTableName[tableName];
            }
            return string.Empty;
        }
        #endregion

        #region GetFormatName
        /// <summary>
        /// GetFormatName
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetFormatName(string name)
        {
            if (
                name?.StartsWith("[") == false &&
                name?.StartsWith("`") == false &&
                name?.StartsWith("\"") == false)
            {
                name = string.Format(this.FormatTempl, name);
            }
            return name;
        }
        #endregion

        #region GetTableName
        /// <summary>
        /// GetTableName
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>string</returns>
        public string GetTableName(Type type)
        {
            /*----------------------------fangwenhan----------------------- */
            //var tableName = this.GetFormatName(type.Name);
            //if (type.GetCustomAttributes(typeof(CusTableAttribute), false).FirstOrDefault() is CusTableAttribute cta)
            //{
            //    if (!string.IsNullOrEmpty(cta.Name))
            //    {
            //        tableName = this.GetFormatName(cta.Name);
            //    }
            //    if (!string.IsNullOrEmpty(cta.Schema))
            //    {
            //        tableName = $"{this.GetFormatName(cta.Schema)}.{tableName}";
            //    }
            //}
            ////else if (type.GetCustomAttributes(typeof(SysTableAttribute), false).FirstOrDefault() is SysTableAttribute sta)
            ////{
            ////    if (!string.IsNullOrEmpty(sta.Name))
            ////    {
            ////        tableName = this.GetFormatName(sta.Name);
            ////    }
            ////    if (!string.IsNullOrEmpty(sta.Schema))
            ////    {
            ////        tableName = $"{this.GetFormatName(sta.Schema)}.{tableName}";
            ////    }
            ////}
            /*----------------------------fangwenhan add----------------------- */
            if (FuncGetTableName == null)
            {
                throw new ArgumentNullException("table is null");
            }
            return FuncGetTableName(type);
        }
        #endregion

        #region GetColumnName
        /// <summary>
        /// GetFormatColumnName
        /// </summary>
        /// <param name="columnName">列名</param>
        /// <returns></returns>
        public string GetColumnName(string columnName) => this.GetFormatName(columnName);
        #endregion

        #region GetColumnInfo
        /// <summary>
        /// GetColumnInfo
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="member">成员</param>
        /// <returns>Tuple</returns>
        public Tuple<string, bool, bool>/*(string columnName, bool isInsert, bool isUpdate)*/ GetColumnInfo(Type type, MemberInfo member)
        {
            string columnName = null;

            var isInsert = true;
            var isUpdate = true;
            /*----------------------------fangwenhan----------------------- */
            //var props = type.GetProperties();
            //var isHaveColumnAttribute = props.Any(x => x.ContainsAttribute<CusColumnAttribute>());
            ////if (!isHaveColumnAttribute)
            ////{
            ////    isHaveColumnAttribute = props.Any(x => x.ContainsAttribute<SysColumnAttribute>());
            ////}
            //if (isHaveColumnAttribute)
            //{
            //    if (member.GetFirstOrDefaultAttribute<CusColumnAttribute>() is CusColumnAttribute cca)
            //    {
            //        columnName = cca.Name;
            //        isInsert = cca.Insert;
            //        isUpdate = cca.Update;
            //    }
            //    //else if (member?.GetFirstOrDefaultAttribute<SysColumnAttribute>() is SysColumnAttribute sca)
            //    //{
            //    //    columnName = sca.Name;
            //    //}
            //    else
            //    {
            //        var p = props.Where(x => x.Name == member?.Name).FirstOrDefault();
            //        if (p?.GetFirstOrDefaultAttribute<CusColumnAttribute>() is CusColumnAttribute cus)
            //        {
            //            columnName = cus.Name;
            //            isInsert = cus.Insert;
            //            isUpdate = cus.Update;
            //        }
            //        //else if (p?.GetFirstOrDefaultAttribute<SysColumnAttribute>() is SysColumnAttribute sys)
            //        //{
            //        //    columnName = sys.Name;
            //        //}
            //    }
            //}
            //columnName = columnName ?? member?.Name;
            ////判断列是否是Key
            //if (member?.GetFirstOrDefaultAttribute<CusKeyAttribute>() is CusKeyAttribute cka)
            //{
            //    isUpdate = false;
            //    if (!cka.Name.IsNullOrEmpty() && cka.Name != columnName)
            //        columnName = cka.Name;
            //}
            ////else if (member?.GetFirstOrDefaultAttribute<SysKeyAttribute>() is SysKeyAttribute ska)
            ////{
            ////    isUpdate = false;
            ////}


            /*----------------------------fangwenhan add----------------------- */
            if (FuncGetColumnName == null)
            {
                throw new ArgumentNullException("column is null");
            }
            return new Tuple<string, bool, bool>(FuncGetColumnName(type, member.Name), isInsert, isUpdate);
        }
        #endregion

        #region GetPrimaryKey
        /// <summary>
        /// GetPrimaryKey
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="isFormat">是否格式化</param>
        /// <returns>Tuple</returns>
        public List<Tuple<string, string>/*(string key, string property)*/> GetPrimaryKey(Type type, bool isFormat = true)
        {
            var result = new List<Tuple<string, string>/*(string key, string property)*/>();
            var props = type.GetProperties();
            /*----------------------------fangwenhan----------------------- */
            //var isHaveColumnAttribute = props.Any(x => x.ContainsAttribute<CusKeyAttribute>());
            ////if (!isHaveColumnAttribute)
            ////{
            ////    isHaveColumnAttribute = props.Any(x => x.ContainsAttribute<SysKeyAttribute>());
            ////}
            //if (isHaveColumnAttribute)
            //{
            //    var properties = props.Where(x => x.ContainsAttribute<CusKeyAttribute>()).ToList();
            //    //if (properties.Count() == 0)
            //    //{
            //    //    properties = props.Where(x => x.ContainsAttribute<SysKeyAttribute>()).ToList();
            //    //}
            //    foreach (var property in properties)
            //    {
            //        var propertyName = property?.Name;
            //        string keyName = null;
            //        if (property?.GetFirstOrDefaultAttribute<CusKeyAttribute>() is CusKeyAttribute cka)
            //        {
            //            keyName = cka.Name ?? propertyName;
            //        }
            //        //else if (property?.GetFirstOrDefaultAttribute<SysKeyAttribute>() is SysKeyAttribute ska)
            //        //{
            //        //    keyName = propertyName;
            //        //}
            //        result.Add(new Tuple<string, string>(this.GetColumnName(keyName), propertyName));
            //    }
            //}
            return result;
        }
        #endregion

        #region ToString
        /// <summary>
        /// ToString
        /// </summary>
        /// <returns>string</returns>
        public override string ToString() => this.Sql.ToString();
        #endregion
        #endregion
    }
    #endregion

    #region BinaryExpressionResolve
    /// <summary>
    /// 表示具有二进制运算符的表达式
    /// </summary>
    internal class BinaryExpressionResolve : BaseSqlBuilder<BinaryExpression>
    {
        #region Private Methods
        /// <summary>
        /// OperatorParser
        /// </summary>
        /// <param name="expressionNodeType">表达式树节点类型</param>
        /// <param name="operatorIndex">操作符索引</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <param name="useIs">是否使用is</param>
        private void OperatorParser(ExpressionType expressionNodeType, int operatorIndex, SqlPack sqlPack, bool useIs = false)
        {
            switch (expressionNodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    sqlPack.Sql.Insert(operatorIndex, " AND ");
                    break;
                case ExpressionType.Equal:
                    if (useIs)
                    {
                        sqlPack.Sql.Insert(operatorIndex, " IS ");
                    }
                    else
                    {
                        sqlPack.Sql.Insert(operatorIndex, " = ");
                    }
                    break;
                case ExpressionType.GreaterThan:
                    sqlPack.Sql.Insert(operatorIndex, " > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    sqlPack.Sql.Insert(operatorIndex, " >= ");
                    break;
                case ExpressionType.NotEqual:
                    if (useIs)
                    {
                        sqlPack.Sql.Insert(operatorIndex, " IS NOT ");
                    }
                    else
                    {
                        sqlPack.Sql.Insert(operatorIndex, " <> ");
                    }
                    break;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    sqlPack.Sql.Insert(operatorIndex, " OR ");
                    break;
                case ExpressionType.LessThan:
                    sqlPack.Sql.Insert(operatorIndex, " < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    sqlPack.Sql.Insert(operatorIndex, " <= ");
                    break;
                case ExpressionType.Add:
                    sqlPack.Sql.Insert(operatorIndex, " + ");
                    break;
                case ExpressionType.Subtract:
                    sqlPack.Sql.Insert(operatorIndex, " - ");
                    break;
                case ExpressionType.Multiply:
                    sqlPack.Sql.Insert(operatorIndex, " * ");
                    break;
                case ExpressionType.Divide:
                    sqlPack.Sql.Insert(operatorIndex, " / ");
                    break;
                case ExpressionType.Modulo:
                    sqlPack.Sql.Insert(operatorIndex, " % ");
                    break;
                default:
                    throw new NotImplementedException("未实现的节点类型" + expressionNodeType);
            }
        }
        #endregion

        #region Override Base Class Methods
        /// <summary>
        /// Join
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Join(BinaryExpression expression, SqlPack sqlPack)
        {
            SqlBuilderProvider.Join(expression.Left, sqlPack);
            var operatorIndex = sqlPack.Sql.Length;
            //嵌套条件
            var flag = false;
            if (expression.Right is BinaryExpression binaryExpression && (binaryExpression.Right as BinaryExpression) != null)
            {
                flag = true;
                sqlPack += "(";
            }
            SqlBuilderProvider.Where(expression.Right, sqlPack);
            if (flag)
            {
                sqlPack += ")";
            }
            var sqlLength = sqlPack.Sql.Length;
            if (sqlLength - operatorIndex == 5 && sqlPack.ToString().ToUpper().EndsWith("NULL"))
            {
                OperatorParser(expression.NodeType, operatorIndex, sqlPack, true);
            }
            else
            {
                OperatorParser(expression.NodeType, operatorIndex, sqlPack);
            }
            return sqlPack;
        }

        #region 最初版本
        ///// <summary>
        ///// Where
        ///// </summary>
        ///// <param name="expression">表达式树</param>
        ///// <param name="sqlPack">sql打包对象</param>
        ///// <returns>SqlPack</returns>
        //public override SqlPack Where(BinaryExpression expression, SqlPack sqlPack)
        //      {
        //          var startIndex = sqlPack.Length;
        //          SqlBuilderProvider.Where(expression.Left, sqlPack);
        //          var signIndex = sqlPack.Length;
        //          //嵌套条件
        //          var flag = false;
        //          if (expression.Right is BinaryExpression binaryExpression && (binaryExpression.Right as BinaryExpression) != null)
        //          {
        //              flag = true;
        //              sqlPack += "(";
        //          }
        //          SqlBuilderProvider.Where(expression.Right, sqlPack);
        //          if (flag)
        //          {
        //              sqlPack += ")";
        //          }
        //          //表达式左侧为bool类型常量且为true时，不进行Sql拼接
        //          if (!(expression.Left.NodeType == ExpressionType.Constant && expression.Left.ToObject() is bool b && b))
        //          {
        //              //若表达式右侧为bool类型，且为false时，条件取非
        //              if ((expression.Right.NodeType == ExpressionType.Constant
        //                  || (expression.Right.NodeType == ExpressionType.Convert
        //                  && expression.Right is UnaryExpression unary
        //                  && unary.Operand.NodeType == ExpressionType.Constant))
        //                  && expression.Right.ToObject() is bool r)
        //              {
        //                  if (!r)
        //                  {
        //                      var subString = sqlPack.ToString().Substring(startIndex, sqlPack.ToString().Length - startIndex).ToUpper();

        //                      //IS NOT、IS                      
        //                      if (subString.Contains("IS NOT"))
        //                      {
        //                          var index = sqlPack.ToString().LastIndexOf("IS NOT");
        //                          if (index != -1) sqlPack.Sql.Replace("IS NOT", "IS", index, 6);
        //                      }
        //                      if (subString.Contains("IS") && subString.LastIndexOf("IS") != subString.LastIndexOf("IS NOT"))
        //                      {
        //                          var index = sqlPack.ToString().LastIndexOf("IS");
        //                          if (index != -1) sqlPack.Sql.Replace("IS", "IS NOT", index, 2);
        //                      }

        //                      //NOT LIKE、LIKE
        //                      if (subString.Contains("NOT LIKE"))
        //                      {
        //                          var index = sqlPack.ToString().LastIndexOf("NOT LIKE");
        //                          if (index != -1) sqlPack.Sql.Replace("NOT LIKE", "LIKE", index, 8);
        //                      }
        //                      if (subString.Contains("LIKE") && subString.LastIndexOf("LIKE") != (subString.LastIndexOf("NOT LIKE") + 4))
        //                      {
        //                          var index = sqlPack.ToString().LastIndexOf("LIKE");
        //                          if (index != -1) sqlPack.Sql.Replace("LIKE", "NOT LIKE", index, 4);
        //                      }

        //                      //NOT IN、IN
        //                      if (subString.Contains("NOT IN"))
        //                      {
        //                          var index = sqlPack.ToString().LastIndexOf("NOT IN");
        //                          if (index != -1) sqlPack.Sql.Replace("NOT IN", "IN", index, 6);
        //                      }
        //                      if (subString.Contains("IN") && subString.LastIndexOf("IN") != (subString.LastIndexOf("NOT IN") + 4))
        //                      {
        //                          var index = sqlPack.ToString().LastIndexOf("IN");
        //                          if (index != -1) sqlPack.Sql.Replace("IN", "NOT IN", index, 2);
        //                      }

        //                      //AND、OR
        //                      if (subString.Contains("AND"))
        //                      {
        //                          var index = sqlPack.ToString().LastIndexOf("AND");
        //                          if (index != -1) sqlPack.Sql.Replace("AND", "OR", index, 3);
        //                      }
        //                      if (subString.Contains("OR"))
        //                      {
        //                          var index = sqlPack.ToString().LastIndexOf("OR");
        //                          if (index != -1) sqlPack.Sql.Replace("OR", "AND", index, 2);
        //                      }

        //                      //=、<>
        //                      if (subString.Contains(" = "))
        //                      {
        //                          var index = sqlPack.ToString().LastIndexOf(" = ");
        //                          if (index != -1) sqlPack.Sql.Replace(" = ", " <> ", index, 3);
        //                      }
        //                      if (subString.Contains("<>"))
        //                      {
        //                          var index = sqlPack.ToString().LastIndexOf("<>");
        //                          if (index != -1) sqlPack.Sql.Replace("<>", "=", index, 2);
        //                      }

        //                      //>、<
        //                      if (subString.Contains(" > "))
        //                      {
        //                          var index = sqlPack.ToString().LastIndexOf(" > ");
        //                          if (index != -1) sqlPack.Sql.Replace(" > ", " <= ", index, 3);
        //                      }
        //                      if (subString.Contains(" < "))
        //                      {
        //                          var index = sqlPack.ToString().LastIndexOf(" < ");
        //                          if (index != -1) sqlPack.Sql.Replace(" < ", " >= ", index, 3);
        //                      }

        //                      //>=、<=
        //                      if (subString.Contains(" >= "))
        //                      {
        //                          var index = sqlPack.ToString().LastIndexOf(" >= ");
        //                          if (index != -1) sqlPack.Sql.Replace(" >= ", " < ", index, 4);
        //                      }
        //                      if (subString.Contains(" <= "))
        //                      {
        //                          var index = sqlPack.ToString().LastIndexOf(" <= ");
        //                          if (index != -1) sqlPack.Sql.Replace(" <= ", " > ", index, 4);
        //                      }
        //                  }
        //              }
        //              else
        //              {
        //                  if (sqlPack.ToString().ToUpper().EndsWith("NULL"))
        //                  {
        //                      OperatorParser(expression.NodeType, signIndex, sqlPack, true);
        //                  }
        //                  else
        //                  {
        //                      OperatorParser(expression.NodeType, signIndex, sqlPack);
        //                  }
        //              }
        //          }
        //          return sqlPack;
        //      }
        #endregion

        #region 自己修改的版本
        //public override SqlPack Where(BinaryExpression expression, SqlPack sqlPack)
        //{
        //    sqlPack += "(";
        //    var startIndex = sqlPack.Length;
        //    SqlBuilderProvider.Where(expression.Left, sqlPack);
        //    var signIndex = sqlPack.Length;

        //    SqlBuilderProvider.Where(expression.Right, sqlPack);

        //    sqlPack += ")";
        //    //表达式左侧为bool类型常量且为true时，不进行Sql拼接
        //    if (!(expression.Left.NodeType == ExpressionType.Constant && expression.Left.ToObject() is bool b && b))
        //    {
        //        //若表达式右侧为bool类型，且为false时，条件取非
        //        if ((expression.Right.NodeType == ExpressionType.Constant
        //            || (expression.Right.NodeType == ExpressionType.Convert
        //            && expression.Right is UnaryExpression unary
        //            && unary.Operand.NodeType == ExpressionType.Constant))
        //            && expression.Right.ToObject() is bool r)
        //        {
        //            if (!r)
        //            {
        //                var subString = sqlPack.ToString().Substring(startIndex, sqlPack.ToString().Length - startIndex).ToUpper();

        //                //IS NOT、IS                      
        //                if (subString.Contains("IS NOT"))
        //                {
        //                    var index = sqlPack.ToString().LastIndexOf("IS NOT");
        //                    if (index != -1) sqlPack.Sql.Replace("IS NOT", "IS", index, 6);
        //                }
        //                if (subString.Contains("IS") && subString.LastIndexOf("IS") != subString.LastIndexOf("IS NOT"))
        //                {
        //                    var index = sqlPack.ToString().LastIndexOf("IS");
        //                    if (index != -1) sqlPack.Sql.Replace("IS", "IS NOT", index, 2);
        //                }

        //                //NOT LIKE、LIKE
        //                if (subString.Contains("NOT LIKE"))
        //                {
        //                    var index = sqlPack.ToString().LastIndexOf("NOT LIKE");
        //                    if (index != -1) sqlPack.Sql.Replace("NOT LIKE", "LIKE", index, 8);
        //                }
        //                if (subString.Contains("LIKE") && subString.LastIndexOf("LIKE") != (subString.LastIndexOf("NOT LIKE") + 4))
        //                {
        //                    var index = sqlPack.ToString().LastIndexOf("LIKE");
        //                    if (index != -1) sqlPack.Sql.Replace("LIKE", "NOT LIKE", index, 4);
        //                }

        //                //NOT IN、IN
        //                if (subString.Contains("NOT IN"))
        //                {
        //                    var index = sqlPack.ToString().LastIndexOf("NOT IN");
        //                    if (index != -1) sqlPack.Sql.Replace("NOT IN", "IN", index, 6);
        //                }
        //                if (subString.Contains("IN") && subString.LastIndexOf("IN") != (subString.LastIndexOf("NOT IN") + 4))
        //                {
        //                    var index = sqlPack.ToString().LastIndexOf("IN");
        //                    if (index != -1) sqlPack.Sql.Replace("IN", "NOT IN", index, 2);
        //                }

        //                //AND、OR
        //                if (subString.Contains("AND"))
        //                {
        //                    var index = sqlPack.ToString().LastIndexOf("AND");
        //                    if (index != -1) sqlPack.Sql.Replace("AND", "OR", index, 3);
        //                }
        //                if (subString.Contains("OR"))
        //                {
        //                    var index = sqlPack.ToString().LastIndexOf("OR");
        //                    if (index != -1) sqlPack.Sql.Replace("OR", "AND", index, 2);
        //                }

        //                //=、<>
        //                if (subString.Contains(" = "))
        //                {
        //                    var index = sqlPack.ToString().LastIndexOf(" = ");
        //                    if (index != -1) sqlPack.Sql.Replace(" = ", " <> ", index, 3);
        //                }
        //                if (subString.Contains("<>"))
        //                {
        //                    var index = sqlPack.ToString().LastIndexOf("<>");
        //                    if (index != -1) sqlPack.Sql.Replace("<>", "=", index, 2);
        //                }

        //                //>、<
        //                if (subString.Contains(" > "))
        //                {
        //                    var index = sqlPack.ToString().LastIndexOf(" > ");
        //                    if (index != -1) sqlPack.Sql.Replace(" > ", " <= ", index, 3);
        //                }
        //                if (subString.Contains(" < "))
        //                {
        //                    var index = sqlPack.ToString().LastIndexOf(" < ");
        //                    if (index != -1) sqlPack.Sql.Replace(" < ", " >= ", index, 3);
        //                }

        //                //>=、<=
        //                if (subString.Contains(" >= "))
        //                {
        //                    var index = sqlPack.ToString().LastIndexOf(" >= ");
        //                    if (index != -1) sqlPack.Sql.Replace(" >= ", " < ", index, 4);
        //                }
        //                if (subString.Contains(" <= "))
        //                {
        //                    var index = sqlPack.ToString().LastIndexOf(" <= ");
        //                    if (index != -1) sqlPack.Sql.Replace(" <= ", " > ", index, 4);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (sqlPack.ToString().ToUpper().EndsWith("NULL"))
        //            {
        //                OperatorParser(expression.NodeType, signIndex, sqlPack, true);
        //            }
        //            else
        //            {
        //                OperatorParser(expression.NodeType, signIndex, sqlPack);
        //            }
        //        }
        //    }
        //    return sqlPack;
        //}
        #endregion

        #region V1.1.1.7版本
        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Where(BinaryExpression expression, SqlPack sqlPack)
        {
            var startIndex = sqlPack.Length;

            //左侧嵌套
            var leftBinary = expression.Left as BinaryExpression;
            var isBinaryLeft = leftBinary?.Left is BinaryExpression;
            var isBoolMethodCallLeft = (leftBinary?.Left as MethodCallExpression)?.Method.ReturnType == typeof(bool);
            var isBinaryRight = leftBinary?.Right is BinaryExpression;
            var isBoolMethodCallRight = (leftBinary?.Right as MethodCallExpression)?.Method.ReturnType == typeof(bool);
            var leftNested = (isBinaryLeft || isBoolMethodCallLeft) && (isBinaryRight || isBoolMethodCallRight);
            if (leftNested)
            {
                sqlPack += "(";
            }
            SqlBuilderProvider.Where(expression.Left, sqlPack);
            if (leftNested)
            {
                sqlPack += ")";
            }

            var signIndex = sqlPack.Length;

            //右侧嵌套
            var rightBinary = expression.Right as BinaryExpression;
            isBinaryLeft = rightBinary?.Left is BinaryExpression;
            isBoolMethodCallLeft = (rightBinary?.Left as MethodCallExpression)?.Method.ReturnType == typeof(bool);
            isBinaryRight = rightBinary?.Right is BinaryExpression;
            isBoolMethodCallRight = (rightBinary?.Right as MethodCallExpression)?.Method.ReturnType == typeof(bool);
            var rightNested = (isBinaryLeft || isBoolMethodCallLeft) && (isBinaryRight || isBoolMethodCallRight);
            if (rightNested)
            {
                sqlPack += "(";
            }
            SqlBuilderProvider.Where(expression.Right, sqlPack);
            if (rightNested)
            {
                sqlPack += ")";
            }

            //表达式左侧为bool类型常量且为true时，不进行Sql拼接
            if (!(expression.Left.NodeType == ExpressionType.Constant && expression.Left.ToObject() is bool b && b))
            {
                //若表达式右侧为bool类型，且为false时，条件取非
                if ((expression.Right.NodeType == ExpressionType.Constant
                    || (expression.Right.NodeType == ExpressionType.Convert
                    && expression.Right is UnaryExpression unary
                    && unary.Operand.NodeType == ExpressionType.Constant))
                    && expression.Right.ToObject() is bool r)
                {
                    if (!r)
                    {
                        var subString = sqlPack.ToString().Substring(startIndex, sqlPack.ToString().Length - startIndex).ToUpper();

                        //IS NOT、IS                      
                        if (subString.Contains("IS NOT"))
                        {
                            var index = sqlPack.ToString().LastIndexOf("IS NOT");
                            if (index != -1) sqlPack.Sql.Replace("IS NOT", "IS", index, 6);
                        }
                        if (subString.Contains("IS") && subString.LastIndexOf("IS") != subString.LastIndexOf("IS NOT"))
                        {
                            var index = sqlPack.ToString().LastIndexOf("IS");
                            if (index != -1) sqlPack.Sql.Replace("IS", "IS NOT", index, 2);
                        }

                        //NOT LIKE、LIKE
                        if (subString.Contains("NOT LIKE"))
                        {
                            var index = sqlPack.ToString().LastIndexOf("NOT LIKE");
                            if (index != -1) sqlPack.Sql.Replace("NOT LIKE", "LIKE", index, 8);
                        }
                        if (subString.Contains("LIKE") && subString.LastIndexOf("LIKE") != (subString.LastIndexOf("NOT LIKE") + 4))
                        {
                            var index = sqlPack.ToString().LastIndexOf("LIKE");
                            if (index != -1) sqlPack.Sql.Replace("LIKE", "NOT LIKE", index, 4);
                        }

                        //NOT IN、IN
                        if (subString.Contains("NOT IN"))
                        {
                            var index = sqlPack.ToString().LastIndexOf("NOT IN");
                            if (index != -1) sqlPack.Sql.Replace("NOT IN", "IN", index, 6);
                        }
                        if (subString.Contains("IN") && subString.LastIndexOf("IN") != (subString.LastIndexOf("NOT IN") + 4))
                        {
                            var index = sqlPack.ToString().LastIndexOf("IN");
                            if (index != -1) sqlPack.Sql.Replace("IN", "NOT IN", index, 2);
                        }

                        //AND、OR
                        if (subString.Contains("AND"))
                        {
                            var index = sqlPack.ToString().LastIndexOf("AND");
                            if (index != -1) sqlPack.Sql.Replace("AND", "OR", index, 3);
                        }
                        if (subString.Contains("OR"))
                        {
                            var index = sqlPack.ToString().LastIndexOf("OR");
                            if (index != -1) sqlPack.Sql.Replace("OR", "AND", index, 2);
                        }

                        //=、<>
                        if (subString.Contains(" = "))
                        {
                            var index = sqlPack.ToString().LastIndexOf(" = ");
                            if (index != -1) sqlPack.Sql.Replace(" = ", " <> ", index, 3);
                        }
                        if (subString.Contains("<>"))
                        {
                            var index = sqlPack.ToString().LastIndexOf("<>");
                            if (index != -1) sqlPack.Sql.Replace("<>", "=", index, 2);
                        }

                        //>、<
                        if (subString.Contains(" > "))
                        {
                            var index = sqlPack.ToString().LastIndexOf(" > ");
                            if (index != -1) sqlPack.Sql.Replace(" > ", " <= ", index, 3);
                        }
                        if (subString.Contains(" < "))
                        {
                            var index = sqlPack.ToString().LastIndexOf(" < ");
                            if (index != -1) sqlPack.Sql.Replace(" < ", " >= ", index, 3);
                        }

                        //>=、<=
                        if (subString.Contains(" >= "))
                        {
                            var index = sqlPack.ToString().LastIndexOf(" >= ");
                            if (index != -1) sqlPack.Sql.Replace(" >= ", " < ", index, 4);
                        }
                        if (subString.Contains(" <= "))
                        {
                            var index = sqlPack.ToString().LastIndexOf(" <= ");
                            if (index != -1) sqlPack.Sql.Replace(" <= ", " > ", index, 4);
                        }
                    }
                }
                else
                {
                    if (sqlPack.ToString().ToUpper().EndsWith("NULL"))
                        OperatorParser(expression.NodeType, signIndex, sqlPack, true);
                    else
                        OperatorParser(expression.NodeType, signIndex, sqlPack);
                }
            }
            return sqlPack;
        }
        #endregion
        #endregion
    }
    #endregion

    #region ConstantExpressionResolve
    /// <summary>
    /// 表示具有常数值的表达式
    /// </summary>
    internal class ConstantExpressionResolve : BaseSqlBuilder<ConstantExpression>
    {
        #region Override Base Class Methods
        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Select(ConstantExpression expression, SqlPack sqlPack)
        {
            if (expression.Value == null)
            {
                var tableName = sqlPack.GetTableName(sqlPack.DefaultType);
                string tableAlias = sqlPack.GetTableAlias(tableName);
                if (!tableAlias.IsNullOrEmpty())
                    tableAlias += ".";
                sqlPack.SelectFields.Add($"{tableAlias}*");
            }
            else
            {
                sqlPack.SelectFields.Add(expression.Value.ToString());
            }
            return sqlPack;
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Where(ConstantExpression expression, SqlPack sqlPack)
        {
            //表达式左侧为bool类型常量
            if (expression.NodeType == ExpressionType.Constant && expression.Value is bool b)
            {
                var sql = sqlPack.ToString().ToUpper().Trim();
                if (!b && (sql.EndsWith("WHERE") || sql.EndsWith("AND") || sql.EndsWith("OR")))
                {
                    sqlPack += " 1 = 0 ";
                }
            }
            else
            {
                sqlPack.AddDbParameter(expression.Value);
            }
            return sqlPack;
        }

        /// <summary>
        /// In
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack In(ConstantExpression expression, SqlPack sqlPack)
        {
            sqlPack.AddDbParameter(expression.Value);
            return sqlPack;
        }

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack GroupBy(ConstantExpression expression, SqlPack sqlPack)
        {
            var tableName = sqlPack.GetTableName(sqlPack.DefaultType);
            sqlPack.SetTableAlias(tableName);
            var tableAlias = sqlPack.GetTableAlias(tableName);
            if (!string.IsNullOrEmpty(tableAlias)) tableAlias += ".";
            sqlPack += tableAlias + sqlPack.GetColumnName(expression.Value.ToString()) + ",";
            return sqlPack;
        }

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlPack</returns>
        public override SqlPack OrderBy(ConstantExpression expression, SqlPack sqlPack, params OrderType[] orders)
        {
            var tableName = sqlPack.GetTableName(sqlPack.DefaultType);
            sqlPack.SetTableAlias(tableName);
            var tableAlias = sqlPack.GetTableAlias(tableName);
            if (!string.IsNullOrEmpty(tableAlias)) tableAlias += ".";
            var field = expression.Value.ToString();
            if (!field.ToUpper().Contains(" ASC") && !field.ToUpper().Contains(" DESC"))
                field = sqlPack.GetColumnName(field);
            sqlPack += tableAlias + field;
            if (orders?.Length > 0)
                sqlPack += $" { (orders[0] == OrderType.Descending ? "DESC" : "ASC")}";
            return sqlPack;
        }
        #endregion
    }
    #endregion

    #region InvocationExpressionResolve
    /// <summary>
    /// 表示将委托或lambda表达式应用于参数表达式列表的表达式
    /// </summary>
    internal class InvocationExpressionResolve : BaseSqlBuilder<InvocationExpression>
    {
        #region Override Base Class Methods
        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Where(InvocationExpression expression, SqlPack sqlPack)
        {
            SqlBuilderProvider.Where(expression.Expression, sqlPack);
            return sqlPack;
        }
        #endregion
    }

    #endregion

    #region LambdaExpressionResolve
    /// <summary>
    /// 描述一个lambda表达式
    /// </summary>
    internal class LambdaExpressionResolve : BaseSqlBuilder<LambdaExpression>
    {
        #region Override Base Class Methods
        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Where(LambdaExpression expression, SqlPack sqlPack)
        {

            SqlBuilderProvider.Where(expression.Body, sqlPack);
            return sqlPack;
        }
        #endregion
    }

    #endregion

    #region ListInitExpressionResolve
    /// <summary>
    /// 表示包含集合初始值设定项的构造函数调用
    /// </summary>
    internal class ListInitExpressionResolve : BaseSqlBuilder<ListInitExpression>
    {
        #region Override Base Class Methods
        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Insert(ListInitExpression expression, SqlPack sqlPack)
        {
            var fields = new List<string>();
            var array = expression.ToObject() as IEnumerable<object>;
            for (var i = 0; i < array.Count(); i++)
            {
                if (sqlPack.DatabaseType != ConnectType.Oracle&& sqlPack.DatabaseType != ConnectType.Dm && sqlPack.DatabaseType != ConnectType.GBase)
                    sqlPack.Sql.Append("(");
                if (i > 0 && (sqlPack.DatabaseType == ConnectType.Oracle|| sqlPack.DatabaseType == ConnectType.Dm || sqlPack.DatabaseType == ConnectType.GBase))
                    sqlPack.Sql.Append(" UNION ALL SELECT ");
                var properties = array.ElementAt(i)?.GetType().GetProperties();
                foreach (var p in properties)
                {
                    var type = p.DeclaringType.ToString().Contains("AnonymousType") ? sqlPack.DefaultType : p.DeclaringType;
                    //(string columnName, bool isInsert, bool isUpdate) = sqlPack.GetColumnInfo(type, p);
                    var tuple = sqlPack.GetColumnInfo(type, p);
                    string columnName = tuple.Item1;
                    bool isInsert = tuple.Item2;
                    bool isUpdate = tuple.Item3;
                    if (isInsert)
                    {
                        var value = p.GetValue(array.ElementAt(i), null);
                        if (value != null || (sqlPack.IsEnableNullValue && value == null))
                        {
                            sqlPack.AddDbParameter(value);
                            if (!fields.Contains(columnName)) fields.Add(columnName);
                            sqlPack += ",";
                        }
                    }
                }
                if (sqlPack[sqlPack.Length - 1] == ',')
                {
                    sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
                    if (sqlPack.DatabaseType != ConnectType.Oracle&& sqlPack.DatabaseType != ConnectType.Dm && sqlPack.DatabaseType != ConnectType.GBase)
                        sqlPack.Sql.Append("),");
                    else
                        sqlPack.Sql.Append(" FROM DUAL");
                }
            }
            if (sqlPack.Sql[sqlPack.Sql.Length - 1] == ',')
                sqlPack.Sql.Remove(sqlPack.Sql.Length - 1, 1);
            sqlPack.Sql = new StringBuilder(string.Format(sqlPack.ToString(), string.Join(",", fields).TrimEnd(',')));
            return sqlPack;
        }

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack GroupBy(ListInitExpression expression, SqlPack sqlPack)
        {
            var array = (expression.ToObject() as IEnumerable<object>)?.ToList();
            if (array != null)
            {
                for (var i = 0; i < array.Count; i++)
                {
                    SqlBuilderProvider.GroupBy(Expression.Constant(array[i], array[i].GetType()), sqlPack);
                }
                sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
            }
            return sqlPack;
        }

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlPack</returns>
        public override SqlPack OrderBy(ListInitExpression expression, SqlPack sqlPack, params OrderType[] orders)
        {
            var array = (expression.ToObject() as IEnumerable<object>)?.ToList();
            if (array != null)
            {
                for (var i = 0; i < array.Count; i++)
                {
                    SqlBuilderProvider.OrderBy(Expression.Constant(array[i], array[i].GetType()), sqlPack);
                    if (i <= orders.Length - 1)
                    {
                        sqlPack += $" { (orders[i] == OrderType.Descending ? "DESC" : "ASC")},";
                    }
                    else if (!array[i].ToString().ToUpper().Contains("ASC") && !array[i].ToString().ToUpper().Contains("DESC"))
                    {
                        sqlPack += " ASC,";
                    }
                    else
                    {
                        sqlPack += ",";
                    }
                }
                sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
            }
            return sqlPack;
        }
        #endregion
    }
    #endregion

    #region MemberExpressionResolve
    /// <summary>
    /// 表示访问字段或属性
    /// </summary>
    internal class MemberExpressionResolve : BaseSqlBuilder<MemberExpression>
    {
        #region Override Base Class Methods
        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Insert(MemberExpression expression, SqlPack sqlPack)
        {
            var objectArray = new List<object>();
            var fields = new List<string>();
            var obj = expression.ToObject();
            if (obj.GetType().IsArray)
                objectArray.AddRange(obj as object[]);
            else if (obj.GetType().Name == "List`1")
                objectArray.AddRange(obj as IEnumerable<object>);
            else
                objectArray.Add(obj);
            for (var i = 0; i < objectArray.Count; i++)
            {
                if (sqlPack.DatabaseType != ConnectType.Oracle&& sqlPack.DatabaseType != ConnectType.Dm && sqlPack.DatabaseType != ConnectType.GBase)
                    sqlPack.Sql.Append("(");
                if (i > 0 && (sqlPack.DatabaseType == ConnectType.Oracle|| sqlPack.DatabaseType == ConnectType.Dm || sqlPack.DatabaseType == ConnectType.GBase))
                    sqlPack.Sql.Append(" UNION ALL SELECT ");
                var properties = objectArray[i]?.GetType().GetProperties();
                foreach (var p in properties)
                {
                    var type = p.DeclaringType.ToString().Contains("AnonymousType") ? sqlPack.DefaultType : p.DeclaringType;
                    //(string columnName, bool isInsert, bool isUpdate) = sqlPack.GetColumnInfo(type, p);
                    var tuple = sqlPack.GetColumnInfo(type, p);
                    string columnName = tuple.Item1;
                    bool isInsert = tuple.Item2;
                    bool isUpdate = tuple.Item3;
                    if (isInsert)
                    {
                        var value = p.GetValue(objectArray[i], null);
                        if (value != null || (sqlPack.IsEnableNullValue && value == null))
                        {
                            sqlPack.AddDbParameter(value);
                            if (!fields.Contains(columnName)) fields.Add(columnName);
                            sqlPack += ",";
                        }
                    }
                }
                if (sqlPack[sqlPack.Length - 1] == ',')
                {
                    sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
                    if (sqlPack.DatabaseType != ConnectType.Oracle && sqlPack.DatabaseType != ConnectType.Dm && sqlPack.DatabaseType != ConnectType.GBase)
                        sqlPack.Sql.Append("),");
                    else
                        sqlPack.Sql.Append(" FROM DUAL");
                }
            }
            if (sqlPack[sqlPack.Length - 1] == ',')
                sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
            sqlPack.Sql = new StringBuilder(string.Format(sqlPack.ToString(), string.Join(",", fields).TrimEnd(',')));
            return sqlPack;
        }

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Update(MemberExpression expression, SqlPack sqlPack)
        {
            var obj = expression.ToObject();
            var properties = obj?.GetType().GetProperties();
            foreach (var item in properties)
            {
                var type = item.DeclaringType.ToString().Contains("AnonymousType") ? sqlPack.DefaultType : item.DeclaringType;
                //(string columnName, bool isInsert, bool isUpdate) = sqlPack.GetColumnInfo(type, item);
                var tuple = sqlPack.GetColumnInfo(type, item);
                string columnName = tuple.Item1;
                bool isInsert = tuple.Item2;
                bool isUpdate = tuple.Item3;
                if (isUpdate)
                {
                    var value = item.GetValue(obj, null);
                    if (value != null || (sqlPack.IsEnableNullValue && value == null))
                    {
                        sqlPack += columnName + " = ";
                        sqlPack.AddDbParameter(value);
                        sqlPack += ",";
                    }
                }
            }
            if (sqlPack[sqlPack.Length - 1] == ',')
            {
                sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
            }
            return sqlPack;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Select(MemberExpression expression, SqlPack sqlPack)
        {
            var type = expression.Expression.Type != expression.Member.DeclaringType ?
                       expression.Expression.Type :
                       expression.Member.DeclaringType;
            var tableName = sqlPack.GetTableName(type);
            sqlPack.SetTableAlias(tableName);
            string tableAlias = sqlPack.GetTableAlias(tableName);
            if (!string.IsNullOrEmpty(tableAlias)) tableAlias += ".";
            //sqlPack.SelectFields.Add(tableAlias + sqlPack.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName);
            string columnName = sqlPack.GetColumnInfo(expression.Member.DeclaringType, expression.Member).Item1;
            if (columnName == null)//20200602 li 被Ignore标记的属性
            {
                return sqlPack;
            }
            //lishuangquan 20200502 注意，Dapper-Extensions返回的列有时候包含前缀，有时候不包含前缀
            if (!string.Format(sqlPack.FormatTempl,expression.Member.Name).Equals(columnName, StringComparison.OrdinalIgnoreCase)&&
                !expression.Member.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase))//有别名
            {
                sqlPack.SelectFields.Add(string.Format("{0}{1} AS {2}", tableAlias,string.Format(sqlPack.FormatTempl,columnName), expression.Member.Name));
                
            }
            else
            {
                sqlPack.SelectFields.Add(string.Format("{0}{1}", tableAlias, string.Format(sqlPack.FormatTempl, columnName)));
            }
            return sqlPack;
        }

        /// <summary>
        /// Join
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Join(MemberExpression expression, SqlPack sqlPack)
        {
            var type = expression.Expression.Type != expression.Member.DeclaringType ?
                       expression.Expression.Type :
                       expression.Member.DeclaringType;
            var tableName = sqlPack.GetTableName(type);
            sqlPack.SetTableAlias(tableName);
            string tableAlias = sqlPack.GetTableAlias(tableName);
            if (!string.IsNullOrEmpty(tableAlias)) tableAlias += ".";
            // sqlPack += tableAlias + sqlPack.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName;
            sqlPack += tableAlias + sqlPack.GetColumnInfo(expression.Member.DeclaringType, expression.Member).Item1;
            return sqlPack;
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Where(MemberExpression expression, SqlPack sqlPack)
        {
            //此处判断expression的Member是否是可空值类型
            //if (expression.Expression.NodeType == ExpressionType.MemberAccess && expression.Member.DeclaringType.IsNullable())
            //{
            //    expression = expression.Expression as MemberExpression;
            //}
            //if (expression != null)
            //{
            //    if (expression.Expression.NodeType == ExpressionType.Parameter)
            //    {
            //        var type = expression.Expression.Type != expression.Member.DeclaringType ?
            //                   expression.Expression.Type :
            //                   expression.Member.DeclaringType;
            //        var tableName = sqlPack.GetTableName(type);
            //        sqlPack.SetTableAlias(tableName);
            //        var tableAlias = sqlPack.GetTableAlias(tableName);
            //        if (!string.IsNullOrEmpty(tableAlias)) tableAlias += ".";
            //        // sqlPack += tableAlias + sqlPack.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName;
            //        sqlPack += tableAlias + sqlPack.GetColumnInfo(expression.Member.DeclaringType, expression.Member).Item1;
            //        //字段是bool类型
            //        if (expression.NodeType == ExpressionType.MemberAccess && expression.Type.GetCoreType() == typeof(bool))
            //        {
            //            sqlPack += " = 1";
            //        }
            //    }
            //    else
            //    {
            //        sqlPack.AddDbParameter(expression.ToObject());
            //    }
            //}
            //return sqlPack;
            if (expression.Expression!=null&& expression.Expression.NodeType == ExpressionType.MemberAccess && expression.Member.DeclaringType.IsNullable())
            {
                //20200602 李双全 has value的处理
                if (expression.Member.Name == "HasValue")
                {
                    var memberExpression = expression.Expression as MemberExpression;
                    var type = memberExpression.Expression.Type != memberExpression.Member.DeclaringType ?
                               memberExpression.Expression.Type :
                               memberExpression.Member.DeclaringType;
                    var tableName = sqlPack.GetTableName(type);
                    sqlPack.SetTableAlias(tableName);
                    var tableAlias = sqlPack.GetTableAlias(tableName);
                    if (!string.IsNullOrEmpty(tableAlias)) tableAlias += ".";
                    var columnName = string.Format(sqlPack.FormatTempl, sqlPack.GetColumnInfo(memberExpression.Member.DeclaringType, memberExpression.Member).Item1);
                    sqlPack += tableAlias + columnName;
                    //HasValue就是不为空的意思
                    sqlPack += " IS NOT NULL ";
                    return sqlPack;
                }
                else //其他可空类型的情况
                {
                    expression = expression.Expression as MemberExpression;
                }
            }
            if (expression != null)
            {
                if (expression.Expression!=null&& expression.Expression.NodeType == ExpressionType.Parameter)
                {
                    var type = expression.Expression.Type != expression.Member.DeclaringType ?
                               expression.Expression.Type :
                               expression.Member.DeclaringType;
                    var tableName = sqlPack.GetTableName(type);
                    sqlPack.SetTableAlias(tableName);
                    var tableAlias = sqlPack.GetTableAlias(tableName);
                    if (!string.IsNullOrEmpty(tableAlias)) tableAlias += ".";
                    string columnName = string.Format(sqlPack.FormatTempl, sqlPack.GetColumnInfo(expression.Member.DeclaringType, expression.Member).Item1);
                    sqlPack += tableAlias + columnName;
                    //字段是bool类型
                    if (expression.NodeType == ExpressionType.MemberAccess && expression.Type.GetCoreType() == typeof(bool))
                    {
                        sqlPack += " = 1";
                    }
                }
                else
                {
                    sqlPack.AddDbParameter(expression.ToObject());
                }
            }
            return sqlPack;
        }

        /// <summary>
        /// In
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack In(MemberExpression expression, SqlPack sqlPack)
        {
            var obj = expression.ToObject();
            if (obj is IEnumerable array)
            {
                sqlPack += "(";
                foreach (var item in array)
                {
                    SqlBuilderProvider.In(Expression.Constant(item), sqlPack);
                    sqlPack += ",";
                }
                if (sqlPack.Sql[sqlPack.Sql.Length - 1] == ',')
                {
                    sqlPack.Sql.Remove(sqlPack.Sql.Length - 1, 1);
                }
                sqlPack += ")";
            }
            return sqlPack;
        }

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack GroupBy(MemberExpression expression, SqlPack sqlPack)
        {
            string tableName = string.Empty;
            if (expression.Expression.NodeType == ExpressionType.Parameter)
            {
                var type = expression.Expression.Type != expression.Member.DeclaringType ?
                           expression.Expression.Type :
                           expression.Member.DeclaringType;
                tableName = sqlPack.GetTableName(type);
            }
            if (expression.Expression.NodeType == ExpressionType.Constant)
            {
                tableName = sqlPack.GetTableName(sqlPack.DefaultType);
            }
            sqlPack.SetTableAlias(tableName);
            var tableAlias = sqlPack.GetTableAlias(tableName);
            if (!string.IsNullOrEmpty(tableAlias)) tableAlias += ".";
            if (expression.Expression.NodeType == ExpressionType.Parameter)
            {
                //sqlPack += tableAlias + sqlPack.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName + ",";

                sqlPack += tableAlias + sqlPack.GetColumnInfo(expression.Member.DeclaringType, expression.Member).Item1 ;
            }
            if (expression.Expression.NodeType == ExpressionType.Constant)
            {
                var obj = expression.ToObject();
                if (obj != null)
                {
                    var type = obj.GetType().Name;
                    if (type == "String[]" && obj is string[] array)
                    {
                        foreach (var item in array)
                        {
                            SqlBuilderProvider.GroupBy(Expression.Constant(item, item.GetType()), sqlPack);
                        }
                        sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
                    }
                    if (type == "List`1" && obj is List<string> list)
                    {
                        foreach (var item in list)
                        {
                            SqlBuilderProvider.GroupBy(Expression.Constant(item, item.GetType()), sqlPack);
                        }
                        sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
                    }
                    if (type == "String" && obj is string str)
                    {
                        SqlBuilderProvider.GroupBy(Expression.Constant(str, str.GetType()), sqlPack);
                        sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
                    }
                }
            }
            return sqlPack;
        }

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlPack</returns>
        public override SqlPack OrderBy(MemberExpression expression, SqlPack sqlPack, params OrderType[] orders)
        {
            string tableName = string.Empty;
            if (expression.Expression.NodeType == ExpressionType.Parameter)
            {
                var type = expression.Expression.Type != expression.Member.DeclaringType ?
                           expression.Expression.Type :
                           expression.Member.DeclaringType;
                tableName = sqlPack.GetTableName(type);
            }
            if (expression.Expression.NodeType == ExpressionType.Constant)
            {
                tableName = sqlPack.GetTableName(sqlPack.DefaultType);
            }
            sqlPack.SetTableAlias(tableName);
            var tableAlias = sqlPack.GetTableAlias(tableName);
            if (!string.IsNullOrEmpty(tableAlias)) tableAlias += ".";
            if (expression.Expression.NodeType == ExpressionType.Parameter)
            {
                //sqlPack += tableAlias + sqlPack.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName;
                sqlPack += tableAlias + sqlPack.GetColumnInfo(expression.Member.DeclaringType, expression.Member).Item1;
                if (orders?.Length > 0)
                    sqlPack += $" { (orders[0] == OrderType.Descending ? "DESC" : "ASC")}";
            }
            if (expression.Expression.NodeType == ExpressionType.Constant)
            {
                var obj = expression.ToObject();
                if (obj != null)
                {
                    var type = obj.GetType().Name;
                    if (type == "String[]" && obj is string[] array)
                    {
                        for (var i = 0; i < array.Length; i++)
                        {
                            SqlBuilderProvider.OrderBy(Expression.Constant(array[i], array[i].GetType()), sqlPack);
                            if (i <= orders.Length - 1)
                            {
                                sqlPack += $" { (orders[i] == OrderType.Descending ? "DESC" : "ASC")},";
                            }
                            else if (!array[i].ToUpper().Contains("ASC") && !array[i].ToUpper().Contains("DESC"))
                            {
                                sqlPack += " ASC,";
                            }
                            else
                            {
                                sqlPack += ",";
                            }
                        }
                        sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
                    }
                    if (type == "List`1" && obj is List<string> list)
                    {
                        for (var i = 0; i < list.Count; i++)
                        {
                            SqlBuilderProvider.OrderBy(Expression.Constant(list[i], list[i].GetType()), sqlPack);
                            if (i <= orders.Length - 1)
                            {
                                sqlPack += $" { (orders[i] == OrderType.Descending ? "DESC" : "ASC")},";
                            }
                            else if (!list[i].ToUpper().Contains("ASC") && !list[i].ToUpper().Contains("DESC"))
                            {
                                sqlPack += " ASC,";
                            }
                            else
                            {
                                sqlPack += ",";
                            }
                        }
                        sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
                    }
                    if (type == "String" && obj is string str)
                    {
                        SqlBuilderProvider.OrderBy(Expression.Constant(str, str.GetType()), sqlPack);
                        str = str.ToUpper();
                        if (!str.Contains("ASC") && !str.Contains("DESC"))
                        {
                            if (orders.Length >= 1)
                            {
                                sqlPack += $" { (orders[0] == OrderType.Descending ? "DESC" : "ASC")},";
                            }
                            else
                            {
                                sqlPack += " ASC,";
                            }
                            sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
                        }
                    }
                }
            }
            return sqlPack;
        }

        /// <summary>
        /// Max
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Max(MemberExpression expression, SqlPack sqlPack)
        {
            var type = expression.Expression.Type != expression.Member.DeclaringType ?
                       expression.Expression.Type :
                       expression.Member.DeclaringType;
            //sqlPack.Sql.Append($"SELECT MAX({sqlPack.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName}) FROM {sqlPack.GetTableName(type)}");
            sqlPack.Sql.Append($"SELECT MAX({sqlPack.GetColumnInfo(expression.Member.DeclaringType, expression.Member).Item1}) FROM {sqlPack.GetTableName(type)}");
            return sqlPack;
        }

        /// <summary>
        /// Min
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Min(MemberExpression expression, SqlPack sqlPack)
        {
            var type = expression.Expression.Type != expression.Member.DeclaringType ?
                       expression.Expression.Type :
                       expression.Member.DeclaringType;
            //sqlPack.Sql.Append($"SELECT MIN({sqlPack.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName}) FROM {sqlPack.GetTableName(type)}");
            sqlPack.Sql.Append($"SELECT MIN({sqlPack.GetColumnInfo(expression.Member.DeclaringType, expression.Member).Item1}) FROM {sqlPack.GetTableName(type)}");
            return sqlPack;
        }

        /// <summary>
        /// Avg
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Avg(MemberExpression expression, SqlPack sqlPack)
        {
            var type = expression.Expression.Type != expression.Member.DeclaringType ?
                       expression.Expression.Type :
                       expression.Member.DeclaringType;
            //sqlPack.Sql.Append($"SELECT AVG({sqlPack.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName}) FROM {sqlPack.GetTableName(type)}");
            sqlPack.Sql.Append($"SELECT AVG({sqlPack.GetColumnInfo(expression.Member.DeclaringType, expression.Member).Item1}) FROM {sqlPack.GetTableName(type)}");
            return sqlPack;
        }

        /// <summary>
        /// Count
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Count(MemberExpression expression, SqlPack sqlPack)
        {
            var type = expression.Expression.Type != expression.Member.DeclaringType ?
                       expression.Expression.Type :
                       expression.Member.DeclaringType;
            //sqlPack.Sql.Append($"SELECT COUNT({sqlPack.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName}) FROM {sqlPack.GetTableName(type)}");
            sqlPack.Sql.Append($"SELECT COUNT({sqlPack.GetColumnInfo(expression.Member.DeclaringType, expression.Member).Item1}) FROM {sqlPack.GetTableName(type)}");
            return sqlPack;
        }

        /// <summary>
        /// Sum
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Sum(MemberExpression expression, SqlPack sqlPack)
        {
            var type = expression.Expression.Type != expression.Member.DeclaringType ?
                       expression.Expression.Type :
                       expression.Member.DeclaringType;
            //sqlPack.Sql.Append($"SELECT SUM({sqlPack.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName}) FROM {sqlPack.GetTableName(type)}");
            sqlPack.Sql.Append($"SELECT SUM({sqlPack.GetColumnInfo(expression.Member.DeclaringType, expression.Member).Item1}) FROM {sqlPack.GetTableName(type)}");
            return sqlPack;
        }
        #endregion
    }
    #endregion

    #region MemberInitExpressionResolve
    /// <summary>
    /// 表示调用构造函数并初始化新对象的一个或多个成员
    /// </summary>
    internal class MemberInitExpressionResolve : BaseSqlBuilder<MemberInitExpression>
    {
        #region Override Base Class Methods
        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Insert(MemberInitExpression expression, SqlPack sqlPack)
        {
            if (sqlPack.DatabaseType != ConnectType.Oracle && sqlPack.DatabaseType != ConnectType.Dm && sqlPack.DatabaseType != ConnectType.GBase)
                sqlPack.Sql.Append("(");
            var fields = new List<string>();
            foreach (MemberAssignment m in expression.Bindings)
            {
                var type = m.Member.DeclaringType.ToString().Contains("AnonymousType") ? sqlPack.DefaultType : m.Member.DeclaringType;
                //(string columnName, bool isInsert, bool isUpdate) = sqlPack.GetColumnInfo(type, m.Member);
                var tuple = sqlPack.GetColumnInfo(type, m.Member);
                string columnName = tuple.Item1;
                bool isInsert = tuple.Item2;
                bool isUpdate = tuple.Item3;

                if (isInsert)
                {
                    var value = m.Expression.ToObject();
                    if (value != null || (sqlPack.IsEnableNullValue && value == null))
                    {
                        sqlPack.AddDbParameter(value);
                        if (!fields.Contains(columnName)) fields.Add(columnName);
                        sqlPack += ",";
                    }
                }
            }
            if (sqlPack[sqlPack.Length - 1] == ',')
            {
                sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
                if (sqlPack.DatabaseType != ConnectType.Oracle && sqlPack.DatabaseType != ConnectType.Dm && sqlPack.DatabaseType != ConnectType.GBase)
                    sqlPack.Sql.Append(")");
                else
                    sqlPack.Sql.Append(" FROM DUAL");
            }
            sqlPack.Sql = new StringBuilder(string.Format(sqlPack.ToString(), string.Join(",", fields).TrimEnd(',')));
            return sqlPack;
        }

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Update(MemberInitExpression expression, SqlPack sqlPack)
        {
            foreach (MemberAssignment m in expression.Bindings)
            {
                var type = m.Member.DeclaringType.ToString().Contains("AnonymousType") ? sqlPack.DefaultType : m.Member.DeclaringType;
                //(string columnName, bool isInsert, bool isUpdate) = sqlPack.GetColumnInfo(type, m.Member);
                var tuple = sqlPack.GetColumnInfo(type, m.Member);
                string columnName = tuple.Item1;
                bool isInsert = tuple.Item2;
                bool isUpdate = tuple.Item3;

                if (isUpdate)
                {
                    var value = m.Expression.ToObject();
                    if (value != null || (sqlPack.IsEnableNullValue && value == null))
                    {
                        sqlPack += columnName + " = ";
                        sqlPack.AddDbParameter(value);
                        sqlPack += ",";
                    }
                }
            }
            if (sqlPack[sqlPack.Length - 1] == ',')
                sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
            return sqlPack;
        }
        #endregion
    }
    #endregion

    #region MethodCallExpressionResolve
    /// <summary>
    /// 表示对静态方法或实例方法的调用
    /// </summary>
    internal class MethodCallExpressionResolve : BaseSqlBuilder<MethodCallExpression>
    {
        #region Private Static Methods
        /// <summary>
        /// methods
        /// </summary>
        private static readonly Dictionary<string, Action<MethodCallExpression, SqlPack>> methods = new Dictionary<string, Action<MethodCallExpression, SqlPack>>
        {
            ["Like"] = Like,
            ["LikeLeft"] = LikeLeft,
            ["LikeRight"] = LikeRight,
            ["NotLike"] = NotLike,
            ["In"] = IN,
            ["NotIn"] = NotIn,
            ["Contains"] = Contains,
            ["IsNullOrEmpty"] = IsNullOrEmpty,
            ["Equals"] = Equals,
            ["ToUpper"] = ToUpper,
            ["ToLower"] = ToLower,
            ["Trim"] = Trim,
            ["TrimStart"] = TrimStart,
            ["TrimEnd"] = TrimEnd
        };

        /// <summary>
        /// IN
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        private static void IN(MethodCallExpression expression, SqlPack sqlPack)
        {
            SqlBuilderProvider.Where(expression.Arguments[0], sqlPack);
            sqlPack += " IN ";
            SqlBuilderProvider.In(expression.Arguments[1], sqlPack);
        }

        /// <summary>
        /// Not In
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        private static void NotIn(MethodCallExpression expression, SqlPack sqlPack)
        {
            SqlBuilderProvider.Where(expression.Arguments[0], sqlPack);
            sqlPack += " NOT IN ";
            SqlBuilderProvider.In(expression.Arguments[1], sqlPack);
        }

        /// <summary>
        /// Like
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        private static void Like(MethodCallExpression expression, SqlPack sqlPack)
        {
            if (expression.Object != null)
            {
                SqlBuilderProvider.Where(expression.Object, sqlPack);
            }
            SqlBuilderProvider.Where(expression.Arguments[0], sqlPack);
            switch (sqlPack.DatabaseType)
            {
                case ConnectType.SQLServer:
                    sqlPack += " LIKE '%' + ";
                    break;
                case ConnectType.MySql:
                case ConnectType.PostgreSQL:
                    sqlPack += " LIKE CONCAT('%',";
                    break;
                case ConnectType.Oracle:
                case ConnectType.SQLite:
                case ConnectType.Db2:
                case ConnectType.GBase:
                case ConnectType.Dm:
                    sqlPack += " LIKE '%' || ";
                    break;
                default:
                    break;
            }
            SqlBuilderProvider.Where(expression.Arguments[1], sqlPack);
            switch (sqlPack.DatabaseType)
            {
                case ConnectType.SQLServer:
                    sqlPack += " + '%'";
                    break;
                case ConnectType.MySql:
                case ConnectType.PostgreSQL:
                    sqlPack += ",'%')";
                    break;
                case ConnectType.Oracle:
                case ConnectType.SQLite:
                case ConnectType.Db2:
                case ConnectType.GBase:
                case ConnectType.Dm:
                    sqlPack += " || '%'";
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// LikeLeft
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        private static void LikeLeft(MethodCallExpression expression, SqlPack sqlPack)
        {
            if (expression.Object != null)
            {
                SqlBuilderProvider.Where(expression.Object, sqlPack);
            }
            SqlBuilderProvider.Where(expression.Arguments[0], sqlPack);
            switch (sqlPack.DatabaseType)
            {
                case ConnectType.SQLServer:
                    sqlPack += " LIKE '%' + ";
                    break;
                case ConnectType.MySql:
                case ConnectType.PostgreSQL:
                    sqlPack += " LIKE CONCAT('%',";
                    break;
                case ConnectType.Oracle:
                case ConnectType.SQLite:
                case ConnectType.Db2:
                case ConnectType.Dm:
                case ConnectType.GBase:
                    sqlPack += " LIKE '%' || ";
                    break;
                default:
                    break;
            }
            SqlBuilderProvider.Where(expression.Arguments[1], sqlPack);
            switch (sqlPack.DatabaseType)
            {
                case ConnectType.MySql:
                case ConnectType.PostgreSQL:
                    sqlPack += ")";
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// LikeRight
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        private static void LikeRight(MethodCallExpression expression, SqlPack sqlPack)
        {
            if (expression.Object != null)
            {
                SqlBuilderProvider.Where(expression.Object, sqlPack);
            }
            SqlBuilderProvider.Where(expression.Arguments[0], sqlPack);
            switch (sqlPack.DatabaseType)
            {
                case ConnectType.SQLServer:
                case ConnectType.Oracle:
                case ConnectType.SQLite:
                case ConnectType.Db2:
                case ConnectType.GBase:
                case ConnectType.Dm:
                    sqlPack += " LIKE ";
                    break;
                case ConnectType.MySql:
                case ConnectType.PostgreSQL:
                    sqlPack += " LIKE CONCAT(";
                    break;
                default:
                    break;
            }
            SqlBuilderProvider.Where(expression.Arguments[1], sqlPack);
            switch (sqlPack.DatabaseType)
            {
                case ConnectType.SQLServer:
                    sqlPack += " + '%'";
                    break;
                case ConnectType.MySql:
                case ConnectType.PostgreSQL:
                    sqlPack += ",'%')";
                    break;
                case ConnectType.Oracle:
                case ConnectType.SQLite:
                case ConnectType.Db2:
                case ConnectType.GBase:
                case ConnectType.Dm:
                    sqlPack += " || '%'";
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// NotLike
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        private static void NotLike(MethodCallExpression expression, SqlPack sqlPack)
        {
            if (expression.Object != null)
            {
                SqlBuilderProvider.Where(expression.Object, sqlPack);
            }
            SqlBuilderProvider.Where(expression.Arguments[0], sqlPack);
            switch (sqlPack.DatabaseType)
            {
                case ConnectType.SQLServer:
                    sqlPack += " NOT LIKE '%' + ";
                    break;
                case ConnectType.MySql:
                case ConnectType.PostgreSQL:
                    sqlPack += " NOT LIKE CONCAT('%',";
                    break;
                case ConnectType.Oracle:
                case ConnectType.SQLite:
                case ConnectType.Db2:
                case ConnectType.GBase:
                case ConnectType.Dm:
                    sqlPack += " NOT LIKE '%' || ";
                    break;
                default:
                    break;
            }
            SqlBuilderProvider.Where(expression.Arguments[1], sqlPack);
            switch (sqlPack.DatabaseType)
            {
                case ConnectType.SQLServer:
                    sqlPack += " + '%'";
                    break;
                case ConnectType.MySql:
                case ConnectType.PostgreSQL:
                    sqlPack += ",'%')";
                    break;
                case ConnectType.Oracle:
                case ConnectType.SQLite:
                case ConnectType.Db2:
                case ConnectType.Dm:
                case ConnectType.GBase:
                    sqlPack += " || '%'";
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Contains
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        private static void Contains(MethodCallExpression expression, SqlPack sqlPack)
        {
            if (expression.Object != null)
            {
                SqlBuilderProvider.Where(expression.Object, sqlPack);
                switch (sqlPack.DatabaseType)
                {
                    case ConnectType.SQLServer:
                        sqlPack += " LIKE '%' + ";
                        break;
                    case ConnectType.MySql:
                    case ConnectType.PostgreSQL:
                        sqlPack += " LIKE CONCAT('%',";
                        break;
                    case ConnectType.Oracle:
                    case ConnectType.SQLite:
                    case ConnectType.Db2:
                    case ConnectType.GBase:
                    case ConnectType.Dm:
                        sqlPack += " LIKE '%' || ";
                        break;
                    default:
                        break;
                }
                SqlBuilderProvider.Where(expression.Arguments[0], sqlPack);
                switch (sqlPack.DatabaseType)
                {
                    case ConnectType.SQLServer:
                        sqlPack += " + '%'";
                        break;
                    case ConnectType.MySql:
                    case ConnectType.PostgreSQL:
                        sqlPack += ",'%')";
                        break;
                    case ConnectType.Oracle:
                    case ConnectType.SQLite:
                    case ConnectType.Db2:
                    case ConnectType.GBase:
                    case ConnectType.Dm:
                        sqlPack += " || '%'";
                        break;
                    default:
                        break;
                }
            }
            else if (expression.Arguments.Count > 1 && expression.Arguments[1] is MemberExpression memberExpression)
            {
                SqlBuilderProvider.Where(memberExpression, sqlPack);
                sqlPack += " IN ";
                SqlBuilderProvider.In(expression.Arguments[0], sqlPack);
            }
        }

        /// <summary>
        /// IsNullOrEmpty
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        private static void IsNullOrEmpty(MethodCallExpression expression, SqlPack sqlPack)
        {
            sqlPack += "(";
            SqlBuilderProvider.Where(expression.Arguments[0], sqlPack);
            sqlPack += " IS NULL OR ";
            SqlBuilderProvider.Where(expression.Arguments[0], sqlPack);
            sqlPack += " = ''";
            sqlPack += ")";
        }

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        private static void Equals(MethodCallExpression expression, SqlPack sqlPack)
        {
            if (expression.Object != null)
            {
                SqlBuilderProvider.Where(expression.Object, sqlPack);
            }
            var signIndex = sqlPack.Length;
            SqlBuilderProvider.Where(expression.Arguments[0], sqlPack);
            if (sqlPack.ToString().ToUpper().EndsWith("NULL"))
            {
                sqlPack.Sql.Insert(signIndex, " IS ");
            }
            else
            {
                sqlPack.Sql.Insert(signIndex, " = ");
            }
        }

        /// <summary>
        /// ToUpper
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        private static void ToUpper(MethodCallExpression expression, SqlPack sqlPack)
        {
            if (expression.Object != null)
            {
                sqlPack += "UPPER(";
                SqlBuilderProvider.Where(expression.Object, sqlPack);
                sqlPack += ")";
            }
        }

        /// <summary>
        /// ToLower
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        private static void ToLower(MethodCallExpression expression, SqlPack sqlPack)
        {
            if (expression.Object != null)
            {
                sqlPack += "LOWER(";
                SqlBuilderProvider.Where(expression.Object, sqlPack);
                sqlPack += ")";
            }
        }

        /// <summary>
        /// Trim
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        private static void Trim(MethodCallExpression expression, SqlPack sqlPack)
        {
            if (expression.Object != null)
            {
                if (sqlPack.DatabaseType == ConnectType.SQLServer)
                {
                    sqlPack += "LTRIM(RTRIM(";
                }
                else
                {
                    sqlPack += "TRIM(";
                }
                SqlBuilderProvider.Where(expression.Object, sqlPack);
                if (sqlPack.DatabaseType == ConnectType.SQLServer)
                {
                    sqlPack += "))";
                }
                else
                {
                    sqlPack += ")";
                }
            }
        }

        /// <summary>
        /// TrimStart
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        private static void TrimStart(MethodCallExpression expression, SqlPack sqlPack)
        {
            if (expression.Object != null)
            {
                sqlPack += "LTRIM(";
                SqlBuilderProvider.Where(expression.Object, sqlPack);
                sqlPack += ")";
            }
        }

        /// <summary>
        /// TrimEnd
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        private static void TrimEnd(MethodCallExpression expression, SqlPack sqlPack)
        {
            if (expression.Object != null)
            {
                sqlPack += "RTRIM(";
                SqlBuilderProvider.Where(expression.Object, sqlPack);
                sqlPack += ")";
            }
        }
        #endregion

        #region Override Base Class Methods
        /// <summary>
        /// In
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack In(MethodCallExpression expression, SqlPack sqlPack)
        {
            var val = expression?.ToObject();
            if (val != null)
            {
                sqlPack += "(";
                if (val.GetType().IsArray || typeof(IList).IsAssignableFrom(val.GetType()))
                {
                    var list = val as IList;
                    if (list?.Count > 0)
                    {
                        foreach (var item in list)
                        {
                            SqlBuilderProvider.In(Expression.Constant(item, item.GetType()), sqlPack);
                            sqlPack += ",";
                        }
                    }
                }
                else
                {
                    SqlBuilderProvider.In(Expression.Constant(val, val.GetType()), sqlPack);
                }
                if (sqlPack.Sql[sqlPack.Sql.Length - 1] == ',')
                    sqlPack.Sql.Remove(sqlPack.Sql.Length - 1, 1);
                sqlPack += ")";
            }
            return sqlPack;
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Where(MethodCallExpression expression, SqlPack sqlPack)
        {
            var key = expression.Method;
            if (key.IsGenericMethod)
                key = key.GetGenericMethodDefinition();
            if (methods.TryGetValue(key.Name, out Action<MethodCallExpression, SqlPack> action))
            {
                action(expression, sqlPack);
                return sqlPack;
            }
            else
            {
                try
                {
                    sqlPack.AddDbParameter(expression.ToObject());
                    return sqlPack;
                }
                catch
                {
                    throw new NotImplementedException("无法解析方法" + expression.Method);
                }
            }
        }

        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="sqlPack"></param>
        /// <returns></returns>
        public override SqlPack Insert(MethodCallExpression expression, SqlPack sqlPack)
        {
            var fields = new List<string>();
            var array = expression.ToObject() as object[];
            for (var i = 0; i < array.Length; i++)
            {
                if (sqlPack.DatabaseType != ConnectType.Oracle&& sqlPack.DatabaseType != ConnectType.Dm && sqlPack.DatabaseType != ConnectType.GBase)
                    sqlPack.Sql.Append("(");
                if (i > 0 && (sqlPack.DatabaseType == ConnectType.Oracle|| sqlPack.DatabaseType == ConnectType.Dm || sqlPack.DatabaseType == ConnectType.GBase))
                    sqlPack.Sql.Append(" UNION ALL SELECT ");
                var properties = array[i]?.GetType().GetProperties();
                foreach (var p in properties)
                {
                    var type = p.DeclaringType.ToString().Contains("AnonymousType") ? sqlPack.DefaultType : p.DeclaringType;
                    //(string columnName, bool isInsert, bool isUpdate) = sqlPack.GetColumnInfo(type, p);
                    var tuple = sqlPack.GetColumnInfo(type, p);
                    string columnName = tuple.Item1;
                    bool isInsert = tuple.Item2;
                    bool isUpdate = tuple.Item3;

                    if (isInsert)
                    {
                        var value = p.GetValue(array[i], null);
                        if (value != null || (sqlPack.IsEnableNullValue && value == null))
                        {
                            sqlPack.AddDbParameter(value);
                            if (!fields.Contains(columnName)) fields.Add(columnName);
                            sqlPack += ",";
                        }
                    }
                }
                if (sqlPack[sqlPack.Length - 1] == ',')
                {
                    sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
                    if (sqlPack.DatabaseType != ConnectType.Oracle && sqlPack.DatabaseType != ConnectType.Dm && sqlPack.DatabaseType != ConnectType.GBase)
                        sqlPack.Sql.Append("),");
                    else
                        sqlPack.Sql.Append(" FROM DUAL");
                }
            }
            if (sqlPack.Sql[sqlPack.Sql.Length - 1] == ',')
                sqlPack.Sql.Remove(sqlPack.Sql.Length - 1, 1);
            sqlPack.Sql = new StringBuilder(string.Format(sqlPack.ToString(), string.Join(",", fields).TrimEnd(',')));
            return sqlPack;
        }

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack GroupBy(MethodCallExpression expression, SqlPack sqlPack)
        {
            var array = (expression.ToObject() as IEnumerable<object>)?.ToList();
            if (array != null)
            {
                for (var i = 0; i < array.Count; i++)
                {
                    SqlBuilderProvider.GroupBy(Expression.Constant(array[i], array[i].GetType()), sqlPack);
                }
                sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
            }
            return sqlPack;
        }

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlPack</returns>
        public override SqlPack OrderBy(MethodCallExpression expression, SqlPack sqlPack, params OrderType[] orders)
        {
            var array = (expression.ToObject() as IEnumerable<object>)?.ToList();
            if (array != null)
            {
                for (var i = 0; i < array.Count; i++)
                {
                    SqlBuilderProvider.OrderBy(Expression.Constant(array[i], array[i].GetType()), sqlPack);
                    if (i <= orders.Length - 1)
                    {
                        sqlPack += $" { (orders[i] == OrderType.Descending ? "DESC" : "ASC")},";
                    }
                    else if (!array[i].ToString().ToUpper().Contains("ASC") && !array[i].ToString().ToUpper().Contains("DESC"))
                    {
                        sqlPack += " ASC,";
                    }
                    else
                    {
                        sqlPack += ",";
                    }
                }
                sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
            }
            return sqlPack;
        }
        #endregion
    }

    #endregion

    #region NewArrayExpressionResolve
    /// <summary>
    /// 表示创建一个新数组，并可能初始化该新数组的元素
    /// </summary>
    internal class NewArrayExpressionResolve : BaseSqlBuilder<NewArrayExpression>
    {
        #region Override Base Class Methods
        /// <summary>
        /// In
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack In(NewArrayExpression expression, SqlPack sqlPack)
        {
            sqlPack += "(";
            foreach (Expression expressionItem in expression.Expressions)
            {
                SqlBuilderProvider.In(expressionItem, sqlPack);
                sqlPack += ",";
            }
            if (sqlPack.Sql[sqlPack.Sql.Length - 1] == ',')
                sqlPack.Sql.Remove(sqlPack.Sql.Length - 1, 1);
            sqlPack += ")";
            return sqlPack;
        }

        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Insert(NewArrayExpression expression, SqlPack sqlPack)
        {
            foreach (Expression expressionItem in expression.Expressions)
            {
                SqlBuilderProvider.Insert(expressionItem, sqlPack);
                if (sqlPack.DatabaseType == ConnectType.Oracle|| sqlPack.DatabaseType == ConnectType.Dm || sqlPack.DatabaseType == ConnectType.GBase)
                    sqlPack += " UNION ALL SELECT ";
                else
                    sqlPack += ",";
            }
            if (sqlPack.Sql[sqlPack.Sql.Length - 1] == ',')
                sqlPack.Sql.Remove(sqlPack.Sql.Length - 1, 1);
            if (sqlPack.Sql.ToString().LastIndexOf(" UNION ALL SELECT ") > -1)
                sqlPack.Sql.Remove(sqlPack.Sql.Length - 18, 18);
            return sqlPack;
        }

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack GroupBy(NewArrayExpression expression, SqlPack sqlPack)
        {
            for (var i = 0; i < expression.Expressions.Count; i++)
            {
                SqlBuilderProvider.GroupBy(expression.Expressions[i], sqlPack);
            }
            sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
            return sqlPack;
        }

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlPack</returns>
        public override SqlPack OrderBy(NewArrayExpression expression, SqlPack sqlPack, params OrderType[] orders)
        {
            for (var i = 0; i < expression.Expressions.Count; i++)
            {
                SqlBuilderProvider.OrderBy(expression.Expressions[i], sqlPack);
                if (i <= orders.Length - 1)
                {
                    sqlPack += $" { (orders[i] == OrderType.Descending ? "DESC" : "ASC")},";
                }
                else if (expression.Expressions[i] is ConstantExpression order)
                {
                    if (!order.Value.ToString().ToUpper().Contains("ASC") && !order.Value.ToString().ToUpper().Contains("DESC"))
                        sqlPack += " ASC,";
                    else
                        sqlPack += ",";
                }
                else
                {
                    sqlPack += " ASC,";
                }
            }
            sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
            return sqlPack;
        }
        #endregion
    }
    #endregion

    #region NewExpressionResolve
    /// <summary>
    /// 表示一个构造函数调用
    /// </summary>
    internal class NewExpressionResolve : BaseSqlBuilder<NewExpression>
    {
        #region Override Base Class Methods
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Update(NewExpression expression, SqlPack sqlPack)
        {
            for (int i = 0; i < expression.Members.Count; i++)
            {
                var m = expression.Members[i];
                var t = m.DeclaringType.ToString().Contains("AnonymousType") ? sqlPack.DefaultType : m.DeclaringType;
                //(string columnName, bool isInsert, bool isUpdate) = sqlPack.GetColumnInfo(t, m);
                var tuple = sqlPack.GetColumnInfo(t, m);
                string columnName = tuple.Item1;
                bool isInsert = tuple.Item2;
                bool isUpdate = tuple.Item3;

                if (isUpdate)
                {
                    var value = expression.Arguments[i]?.ToObject();
                    if (value != null || (sqlPack.IsEnableNullValue && value == null))
                    {
                        sqlPack += columnName + " = ";
                        sqlPack.AddDbParameter(value);
                        sqlPack += ",";
                    }
                }
            }
            if (sqlPack[sqlPack.Length - 1] == ',')
            {
                sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
            }
            return sqlPack;
        }

        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Insert(NewExpression expression, SqlPack sqlPack)
        {
            if (sqlPack.DatabaseType != ConnectType.Oracle && sqlPack.DatabaseType != ConnectType.Dm && sqlPack.DatabaseType != ConnectType.GBase)
                sqlPack.Sql.Append("(");
            var fields = new List<string>();
            for (int i = 0; i < expression.Members?.Count; i++)
            {
                var m = expression.Members[i];
                var t = m.DeclaringType.ToString().Contains("AnonymousType") ? sqlPack.DefaultType : m.DeclaringType;
                //(string columnName, bool isInsert, bool isUpdate) = sqlPack.GetColumnInfo(t, m);
                var tuple = sqlPack.GetColumnInfo(t, m);
                string columnName = tuple.Item1;
                bool isInsert = tuple.Item2;
                bool isUpdate = tuple.Item3;

                if (isInsert)
                {
                    var value = expression.Arguments[i]?.ToObject();
                    if (value != null || (sqlPack.IsEnableNullValue && value == null))
                    {
                        sqlPack.AddDbParameter(value);
                        if (!fields.Contains(columnName)) fields.Add(columnName);
                        sqlPack += ",";
                    }
                }
            }
            if (sqlPack[sqlPack.Length - 1] == ',')
            {
                sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
                if (sqlPack.DatabaseType != ConnectType.Oracle && sqlPack.DatabaseType != ConnectType.Dm && sqlPack.DatabaseType != ConnectType.GBase)
                    sqlPack.Sql.Append(")");
                else
                    sqlPack.Sql.Append(" FROM DUAL");
            }
            sqlPack.Sql = new StringBuilder(string.Format(sqlPack.ToString(), string.Join(",", fields).TrimEnd(',')));
            return sqlPack;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Select(NewExpression expression, SqlPack sqlPack)
        {
            for (var i = 0; i < expression.Members.Count; i++)
            {
                var argument = expression.Arguments[i];
                var member = expression.Members[i];
                SqlBuilderProvider.Select(argument, sqlPack);
                //添加字段别名
                if (argument is MemberExpression memberExpression && memberExpression.Member.Name != member.Name &&
                    //lishuangquan 有可能该字段已经有映射了，有映射了的字段，再取别名无效
                    !sqlPack.SelectFields[sqlPack.SelectFields.Count - 1].Contains(" AS "))
                {
                    sqlPack.SelectFields[sqlPack.SelectFields.Count - 1] += " AS " + member.Name;
                }
            }
            return sqlPack;
        }

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack GroupBy(NewExpression expression, SqlPack sqlPack)
        {
            foreach (Expression item in expression.Arguments)
            {
                SqlBuilderProvider.GroupBy(item, sqlPack);
                sqlPack += ",";
            }
            sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
            return sqlPack;
        }

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlPack</returns>
        public override SqlPack OrderBy(NewExpression expression, SqlPack sqlPack, params OrderType[] orders)
        {
            for (var i = 0; i < expression.Arguments.Count; i++)
            {
                SqlBuilderProvider.OrderBy(expression.Arguments[i], sqlPack);
                if (i <= orders.Length - 1)
                    sqlPack += $" { (orders[i] == OrderType.Descending ? "DESC" : "ASC")},";
                else
                    sqlPack += " ASC,";
            }
            sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
            return sqlPack;
        }
        #endregion
    }
    #endregion

    #region ParameterExpressionResolve
    /// <summary>
    /// 表示命名参数表达式
    /// </summary>
    internal class ParameterExpressionResolve : BaseSqlBuilder<ParameterExpression>
    {
        #region Override Base Class Methods
        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Select(ParameterExpression expression, SqlPack sqlPack)
        {
            var tableName = sqlPack.GetTableName(expression.Type);
            sqlPack.SetTableAlias(tableName);
            var tableAlias = sqlPack.GetTableAlias(tableName);
            sqlPack.SelectFields.Add($"{tableAlias}.*");
            return sqlPack;
        }
        #endregion
    }
    #endregion

    #region UnaryExpressionResolve
    /// <summary>
    /// 表示具有一元运算符的表达式
    /// </summary>
    internal class UnaryExpressionResolve : BaseSqlBuilder<UnaryExpression>
    {
        #region Override Base Class Methods
        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Select(UnaryExpression expression, SqlPack sqlPack)
        {
            SqlBuilderProvider.Select(expression.Operand, sqlPack);
            return sqlPack;
        }

        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Insert(UnaryExpression expression, SqlPack sqlPack)
        {
            SqlBuilderProvider.Insert(expression.Operand, sqlPack);
            return sqlPack;
        }

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Update(UnaryExpression expression, SqlPack sqlPack)
        {
            SqlBuilderProvider.Update(expression.Operand, sqlPack);
            return sqlPack;
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Where(UnaryExpression expression, SqlPack sqlPack)
        {
            var startIndex = sqlPack.Length;
            SqlBuilderProvider.Where(expression.Operand, sqlPack);
            if (expression.NodeType == ExpressionType.Not)
            {
                var subString = sqlPack.ToString().Substring(startIndex, sqlPack.ToString().Length - startIndex).ToUpper();

                //IS NOT、IS                     
                if (subString.Contains("IS NOT"))
                {
                    var index = sqlPack.ToString().LastIndexOf("IS NOT");
                    if (index != -1) sqlPack.Sql.Replace("IS NOT", "IS", index, 6);
                }
                if (subString.Contains("IS") && subString.LastIndexOf("IS") != subString.LastIndexOf("IS NOT"))
                {
                    var index = sqlPack.ToString().LastIndexOf("IS");
                    if (index != -1) sqlPack.Sql.Replace("IS", "IS NOT", index, 2);
                }

                //NOT LIKE、LIKE
                if (subString.Contains("NOT LIKE"))
                {
                    var index = sqlPack.ToString().LastIndexOf("NOT LIKE");
                    if (index != -1) sqlPack.Sql.Replace("NOT LIKE", "LIKE", index, 8);
                }
                if (subString.Contains("LIKE") && subString.LastIndexOf("LIKE") != (subString.LastIndexOf("NOT LIKE") + 4))
                {
                    var index = sqlPack.ToString().LastIndexOf("LIKE");
                    if (index != -1) sqlPack.Sql.Replace("LIKE", "NOT LIKE", index, 4);
                }

                //NOT IN、IN
                if (subString.Contains("NOT IN"))
                {
                    var index = sqlPack.ToString().LastIndexOf("NOT IN");
                    if (index != -1) sqlPack.Sql.Replace("NOT IN", "IN", index, 6);
                }
                if (subString.Contains("IN") && subString.LastIndexOf("IN") != (subString.LastIndexOf("NOT IN") + 4))
                {
                    var index = sqlPack.ToString().LastIndexOf("IN");
                    if (index != -1) sqlPack.Sql.Replace("IN", "NOT IN", index, 2);
                }

                //AND、OR
                if (subString.Contains("AND"))
                {
                    var index = sqlPack.ToString().LastIndexOf("AND");
                    if (index != -1) sqlPack.Sql.Replace("AND", "OR", index, 3);
                }
                if (subString.Contains("OR"))
                {
                    var index = sqlPack.ToString().LastIndexOf("OR");
                    if (index != -1) sqlPack.Sql.Replace("OR", "AND", index, 2);
                }

                //=、<>
                if (subString.Contains(" = "))
                {
                    var index = sqlPack.ToString().LastIndexOf(" = ");
                    if (index != -1) sqlPack.Sql.Replace(" = ", " <> ", index, 3);
                }
                if (subString.Contains("<>"))
                {
                    var index = sqlPack.ToString().LastIndexOf("<>");
                    if (index != -1) sqlPack.Sql.Replace("<>", "=", index, 2);
                }

                //>、<
                if (subString.Contains(" > "))
                {
                    var index = sqlPack.ToString().LastIndexOf(" > ");
                    if (index != -1) sqlPack.Sql.Replace(" > ", " <= ", index, 3);
                }
                if (subString.Contains(" < "))
                {
                    var index = sqlPack.ToString().LastIndexOf(" < ");
                    if (index != -1) sqlPack.Sql.Replace(" < ", " >= ", index, 3);
                }

                //>=、<=
                if (subString.Contains(" >= "))
                {
                    var index = sqlPack.ToString().LastIndexOf(" >= ");
                    if (index != -1) sqlPack.Sql.Replace(" >= ", " < ", index, 4);
                }
                if (subString.Contains(" <= "))
                {
                    var index = sqlPack.ToString().LastIndexOf(" <= ");
                    if (index != -1) sqlPack.Sql.Replace(" <= ", " > ", index, 4);
                }
            }
            return sqlPack;
        }

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack GroupBy(UnaryExpression expression, SqlPack sqlPack)
        {
            SqlBuilderProvider.GroupBy(expression.Operand, sqlPack);
            return sqlPack;
        }

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlPack</returns>
        public override SqlPack OrderBy(UnaryExpression expression, SqlPack sqlPack, params OrderType[] orders)
        {
            SqlBuilderProvider.OrderBy(expression.Operand, sqlPack, orders);
            return sqlPack;
        }

        /// <summary>
        /// Max
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Max(UnaryExpression expression, SqlPack sqlPack)
        {
            SqlBuilderProvider.Max(expression.Operand, sqlPack);
            return sqlPack;
        }

        /// <summary>
        /// Min
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Min(UnaryExpression expression, SqlPack sqlPack)
        {
            SqlBuilderProvider.Min(expression.Operand, sqlPack);
            return sqlPack;
        }

        /// <summary>
        /// Avg
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Avg(UnaryExpression expression, SqlPack sqlPack)
        {
            SqlBuilderProvider.Avg(expression.Operand, sqlPack);
            return sqlPack;
        }

        /// <summary>
        /// Count
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Count(UnaryExpression expression, SqlPack sqlPack)
        {
            SqlBuilderProvider.Count(expression.Operand, sqlPack);
            return sqlPack;
        }

        /// <summary>
        /// Sum
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Sum(UnaryExpression expression, SqlPack sqlPack)
        {
            SqlBuilderProvider.Sum(expression.Operand, sqlPack);
            return sqlPack;
        }

        /// <summary>
        /// Join
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Join(UnaryExpression expression, SqlPack sqlPack)
        {
            SqlBuilderProvider.Join(expression.Operand, sqlPack);
            return sqlPack;
        }
        #endregion
    }
    #endregion

    #region  ISqlBuilder
    /// <summary>
    /// ISqlBuilder
    /// </summary>
    internal interface ISqlBuilder
    {
        #region Update
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        SqlPack Update(Expression expression, SqlPack sqlPack);
        #endregion

        #region Insert
        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        SqlPack Insert(Expression expression, SqlPack sqlPack);
        #endregion

        #region Select
        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns></returns>
        SqlPack Select(Expression expression, SqlPack sqlPack);
        #endregion

        #region Join
        /// <summary>
        /// Join
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns></returns>
        SqlPack Join(Expression expression, SqlPack sqlPack);
        #endregion

        #region Where
        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns></returns>
        SqlPack Where(Expression expression, SqlPack sqlPack);
        #endregion

        #region In
        /// <summary>
        /// In
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns></returns>
        SqlPack In(Expression expression, SqlPack sqlPack);
        #endregion

        #region GroupBy
        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns></returns>
        SqlPack GroupBy(Expression expression, SqlPack sqlPack);
        #endregion

        #region OrderBy
        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <param name="orders">排序类型</param>
        /// <returns></returns>
        SqlPack OrderBy(Expression expression, SqlPack sqlPack, params OrderType[] orders);
        #endregion

        #region Max
        /// <summary>
        /// Max
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        SqlPack Max(Expression expression, SqlPack sqlPack);
        #endregion

        #region Min
        /// <summary>
        /// Min
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        SqlPack Min(Expression expression, SqlPack sqlPack);
        #endregion

        #region Avg
        /// <summary>
        /// Avg
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        SqlPack Avg(Expression expression, SqlPack sqlPack);
        #endregion

        #region Count
        /// <summary>
        /// Count
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        SqlPack Count(Expression expression, SqlPack sqlPack);
        #endregion

        #region Sum
        /// <summary>
        /// Sum
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        SqlPack Sum(Expression expression, SqlPack sqlPack);
        #endregion
    }
    #endregion

    #region BaseSqlBuilder
    /// <summary>
    /// 抽象基类
    /// </summary>
    /// <typeparam name="T">泛型类型</typeparam>
    internal abstract class BaseSqlBuilder<T> : ISqlBuilder where T : Expression
    {
        #region Public Virtural Methods
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public virtual SqlPack Update(T expression, SqlPack sqlPack) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.Update方法");

        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public virtual SqlPack Insert(T expression, SqlPack sqlPack) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.Insert方法");

        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public virtual SqlPack Select(T expression, SqlPack sqlPack) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.Select方法");

        /// <summary>
        /// Join
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public virtual SqlPack Join(T expression, SqlPack sqlPack) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.Join方法");

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public virtual SqlPack Where(T expression, SqlPack sqlPack) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.Where方法");

        /// <summary>
        /// In
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public virtual SqlPack In(T expression, SqlPack sqlPack) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.In方法");

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public virtual SqlPack GroupBy(T expression, SqlPack sqlPack) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.GroupBy方法");

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlPack</returns>
        public virtual SqlPack OrderBy(T expression, SqlPack sqlPack, params OrderType[] orders) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.OrderBy方法");

        /// <summary>
        /// Max
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public virtual SqlPack Max(T expression, SqlPack sqlPack) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.Max方法");

        /// <summary>
        /// Min
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public virtual SqlPack Min(T expression, SqlPack sqlPack) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.Min方法");

        /// <summary>
        /// Avg
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public virtual SqlPack Avg(T expression, SqlPack sqlPack) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.Avg方法");

        /// <summary>
        /// Count
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public virtual SqlPack Count(T expression, SqlPack sqlPack) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.Count方法");

        /// <summary>
        /// Sum
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public virtual SqlPack Sum(T expression, SqlPack sqlPack) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.Sum方法");
        #endregion

        #region Implementation ISqlBuilder Interface
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public SqlPack Update(Expression expression, SqlPack sqlPack) => Update((T)expression, sqlPack);

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public SqlPack Insert(Expression expression, SqlPack sqlPack) => Insert((T)expression, sqlPack);

        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public SqlPack Select(Expression expression, SqlPack sqlPack) => Select((T)expression, sqlPack);

        /// <summary>
        /// Join
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public SqlPack Join(Expression expression, SqlPack sqlPack) => Join((T)expression, sqlPack);

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public SqlPack Where(Expression expression, SqlPack sqlPack) => Where((T)expression, sqlPack);

        /// <summary>
        /// In
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public SqlPack In(Expression expression, SqlPack sqlPack) => In((T)expression, sqlPack);

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public SqlPack GroupBy(Expression expression, SqlPack sqlPack) => GroupBy((T)expression, sqlPack);

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlPack</returns>
        public SqlPack OrderBy(Expression expression, SqlPack sqlPack, params OrderType[] orders) => OrderBy((T)expression, sqlPack, orders);

        /// <summary>
        /// Max
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public SqlPack Max(Expression expression, SqlPack sqlPack) => Max((T)expression, sqlPack);

        /// <summary>
        /// Min
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public SqlPack Min(Expression expression, SqlPack sqlPack) => Min((T)expression, sqlPack);

        /// <summary>
        /// Avg
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public SqlPack Avg(Expression expression, SqlPack sqlPack) => Avg((T)expression, sqlPack);

        /// <summary>
        /// Count
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public SqlPack Count(Expression expression, SqlPack sqlPack) => Count((T)expression, sqlPack);

        /// <summary>
        /// Sum
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public SqlPack Sum(Expression expression, SqlPack sqlPack) => Sum((T)expression, sqlPack);
        #endregion
    }
    #endregion

    #region SqlBuilder
    /// <summary>
    /// SqlBuilder
    /// </summary>
    internal static class SqlBuilder
    {
        #region fangwenhan add，使用委托,减少依赖

        /// <summary>
        /// 数据库类型，必须设置
        /// </summary>
        public static ConnectType DatabaseType { get; set; } = ConnectType.MySql;

        /// <summary>
        /// 获取表名,参数：实体类型。
        /// </summary>
        public static Func<Type, string> FuncGetTableName { get; set; }

        /// <summary>
        /// 获取列名，参数：实体类型，字段名
        /// </summary>
        public static Func<Type, string, string> FuncGetColumnName { get; set; }
        /// <summary>
        /// lishuangquan 加入全局SQL拦截，方便对SQL的监控与操作
        /// ORACLE数据库要设置默认的拦截器，使SQL语句变为大写
        /// </summary>
        public static Func<string, object, string> SqlIntercept { get; set; }
        /// <summary>
        /// 获取SQLBuilder
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static SqlBuilderCore<T> GetSqlBuilderCore<T>() where T : class
        {
            if (FuncGetColumnName == null || FuncGetTableName == null)
            {
                throw new ArgumentNullException("TableName/ColumnName is null ");
            }
            var builder= new SqlBuilderCore<T>(DatabaseType, FuncGetTableName, FuncGetColumnName);
            
            //lishuangquan sql默认全部转换为大写
            builder.SqlIntercept = (sql, paras) => sql.ToUpper();
            
            if (SqlIntercept != null)
            {
                //将每个方法带的SQL拦截去掉，改成这里加
                builder.SqlIntercept += SqlIntercept;
            }
            return builder;
        }

        #endregion

        #region Insert
        /// <summary>
        /// Insert
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="isEnableNullValue">是否对null值属性进行sql拼接操作，默认：是</param>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Insert<T>(
            Expression<Func<object>> expression = null,
            bool isEnableNullValue = true)
            where T : class
        {
            var builder = GetSqlBuilderCore<T>().Insert(expression, isEnableNullValue);
            return builder;
        }
        #endregion

        #region Delete
        /// <summary>
        /// Delete
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Delete<T>()
            where T : class
        {
            var builder = GetSqlBuilderCore<T>().Delete();
            return builder;
        }
        #endregion

        #region Update
        /// <summary>
        /// Update
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="isEnableNullValue">是否对null值属性进行sql拼接操作，默认：是</param>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Update<T>(
            Expression<Func<object>> expression = null,
            bool isEnableNullValue = true)
            where T : class
        {
            var builder = GetSqlBuilderCore<T>().Update(expression, isEnableNullValue);
            return builder;
        }
        #endregion

        #region Select
        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Select<T>(
            Expression<Func<T, object>> expression = null)
            where T : class
        {
            var builder = GetSqlBuilderCore<T>().Select(expression);
            return builder;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Select<T, T2>(
            Expression<Func<T, T2, object>> expression = null)
            where T : class
            where T2 : class
        {
            var builder = GetSqlBuilderCore<T>().Select(expression);
            return builder;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Select<T, T2, T3>(
            Expression<Func<T, T2, T3, object>> expression = null)
            where T : class
            where T2 : class
            where T3 : class
        {
            var builder = GetSqlBuilderCore<T>().Select(expression);
            return builder;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Select<T, T2, T3, T4>(
            Expression<Func<T, T2, T3, T4, object>> expression = null)
            where T : class
            where T2 : class
            where T3 : class
            where T4 : class
        {
            var builder = GetSqlBuilderCore<T>().Select(expression);
            return builder;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Select<T, T2, T3, T4, T5>(
            Expression<Func<T, T2, T3, T4, T5, object>> expression = null)
            where T : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
        {
            var builder = GetSqlBuilderCore<T>().Select(expression);
            return builder;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Select<T, T2, T3, T4, T5, T6>(
            Expression<Func<T, T2, T3, T4, T5, T6, object>> expression = null)
            where T : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
        {
            var builder = GetSqlBuilderCore<T>().Select(expression);
            return builder;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Select<T, T2, T3, T4, T5, T6, T7>(
            Expression<Func<T, T2, T3, T4, T5, T6, T7, object>> expression = null)
            where T : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
        {
            var builder = GetSqlBuilderCore<T>().Select(expression);
            return builder;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Select<T, T2, T3, T4, T5, T6, T7, T8>(
            Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, object>> expression = null)
            where T : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
        {
            var builder = GetSqlBuilderCore<T>().Select(expression);
            return builder;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <typeparam name="T9">泛型类型9</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Select<T, T2, T3, T4, T5, T6, T7, T8, T9>(
            Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, object>> expression = null)
            where T : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
            where T9 : class
        {
            var builder = GetSqlBuilderCore<T>().Select(expression);
            return builder;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <typeparam name="T9">泛型类型9</typeparam>
        /// <typeparam name="T10">泛型类型10</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Select<T, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, object>> expression = null)
            where T : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
            where T9 : class
            where T10 : class
        {
            var builder = GetSqlBuilderCore<T>().Select(expression);
            return builder;
        }
        #endregion

        #region Max
        /// <summary>
        /// Max
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Max<T>(
            Expression<Func<T, object>> expression)
            where T : class
        {
            var builder = GetSqlBuilderCore<T>().Max(expression);
            return builder;
        }
        #endregion

        #region Min
        /// <summary>
        /// Min
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Min<T>(
            Expression<Func<T, object>> expression)
            where T : class
        {
            var builder = GetSqlBuilderCore<T>().Min(expression);
            return builder;
        }
        #endregion

        #region Avg
        /// <summary>
        /// Avg
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Avg<T>(
            Expression<Func<T, object>> expression)
            where T : class
        {
            var builder = GetSqlBuilderCore<T>().Avg(expression);
            return builder;
        }
        #endregion

        #region Count
        /// <summary>
        /// Count
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Count<T>(
            Expression<Func<T, object>> expression = null)
            where T : class
        {
            var builder = GetSqlBuilderCore<T>().Count(expression);
            return builder;
        }
        #endregion

        #region Sum
        /// <summary>
        /// Sum
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Sum<T>(
            Expression<Func<T, object>> expression)
            where T : class
        {
            var builder = GetSqlBuilderCore<T>().Sum(expression);
            return builder;
        }
        #endregion

        #region GetTableName
        ///// <summary>
        ///// 获取实体对应的数据库表名
        ///// </summary>
        ///// <typeparam name="T">泛型类型</typeparam>
        ///// <returns>string</returns>
        //public static string GetTableName<T>(ConnectType databaseType)
        //    where T : class
        //{
        //    return new SqlBuilderCore<T>(databaseType).GetTableName();
        //}
        #endregion

        #region GetPrimaryKey
        ///// <summary>
        ///// 获取实体对应的数据库表的主键名(多主键)
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <returns></returns>
        //public static List<string> GetPrimaryKey<T>()
        //    where T : class
        //{
        //    return new SqlBuilderCore<T>(databaseType).GetPrimaryKey();
        //}
        #endregion
    }
    #endregion

    #region SqlBuilderCore
    /// <summary>
    /// SqlBuilderCore
    /// </summary>
    /// <typeparam name="T">泛型类型</typeparam>
    internal class SqlBuilderCore<T> where T : class
    {
        #region Private Field
        /// <summary>
        /// _sqlPack
        /// </summary>
        private SqlPack _sqlPack;
        #endregion

        #region Public Property
        /// <summary>
        /// SQL拦截委托
        /// </summary>
        public Func<string, object, string> SqlIntercept { get; set; }

        /// <summary>
        /// SQL语句
        /// </summary>
        public string Sql
        {
            get
            {
                var sql = this._sqlPack.ToString();
                //添加sql日志拦截
                sql = SqlIntercept?.Invoke(sql, this._sqlPack.DbParams) ?? sql;
                return sql;
            }
        }

        /// <summary>
        /// SQL格式化参数
        /// </summary>
        public Dictionary<string, object> Parameters => this._sqlPack.DbParams;

        ///// <summary>
        ///// Dapper格式化参数
        ///// </summary>
        //public DynamicParameters DynamicParameters => this._sqlPack.DbParams.ToDynamicParameters();

        /// <summary>
        /// SQL格式化参数
        /// </summary>
        public DbParameter[] DbParameters
        {
            get
            {
                DbParameter[] parameters = null;
                switch (this._sqlPack.DatabaseType)
                {
                    case ConnectType.SQLServer:
                        parameters = this._sqlPack.DbParams.ToSqlParameters();
                        break;
                    case ConnectType.MySql:
                        parameters = this._sqlPack.DbParams.ToMySqlParameters();
                        break;
                    case ConnectType.SQLite:
                        //parameters = this._sqlPack.DbParams.ToSQLiteParameters();
                        throw new Exception("SQLite未实现");

                    case ConnectType.Oracle:
                        parameters = this._sqlPack.DbParams.ToOracleParameters();
                        break;
                    case ConnectType.Dm:
                        parameters = this._sqlPack.DbParams.ToDmParameters();
                        break;
                    case ConnectType.GBase:
                        parameters = this._sqlPack.DbParams.ToGBaseParameters();
                        break;
                    case ConnectType.PostgreSQL:
                        //parameters = this._sqlPack.DbParams.ToNpgsqlParameters();
                        throw new Exception("PostgreSQL未实现");
                    case ConnectType.Db2:
                        parameters = this._sqlPack.DbParams.ToDB2Parameters();
                        break;
                    default:
                        throw new Exception("未实现");

                }
                return parameters;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// SqlBuilderCore
        /// </summary>
        /// <param name="dbType">数据库类型</param>
        /// <param name="funcGetTableName">获取表名委托</param>
        /// <param name="funcGetColumnName">获取列名委托</param>
        public SqlBuilderCore(ConnectType dbType, Func<Type, string> funcGetTableName, Func<Type, string, string> funcGetColumnName)
        {
            this._sqlPack = new SqlPack
            {
                DatabaseType = dbType,
                DefaultType = typeof(T),
                FuncGetTableName = funcGetTableName,
                FuncGetColumnName = funcGetColumnName,
            };
        }
        #endregion

        #region Public Methods
        #region Clear
        /// <summary>
        /// Clear
        /// </summary>
        public void Clear()
        {
            this._sqlPack.Clear();
        }
        #endregion

        #region Select
        /// <summary>
        /// SelectParser
        /// </summary>
        /// <param name="array">可变数量参数</param>
        /// <returns>string</returns>
        private string SelectParser(params Type[] array)
        {
            this._sqlPack.Clear();
            this._sqlPack.IsSingleTable = false;
            foreach (var item in array)
            {
                var tableName = this._sqlPack.GetTableName(item);
                this._sqlPack.SetTableAlias(tableName);
            }
            var _tableName = this._sqlPack.GetTableName(typeof(T));
            //Oracle表别名不支持AS关键字，列别名支持；
            if (this._sqlPack.DatabaseType == ConnectType.Oracle)
                return $"SELECT {{0}} FROM {_tableName} {this._sqlPack.GetTableAlias(_tableName)}";
            else
                return $"SELECT {{0}} FROM {_tableName} AS {this._sqlPack.GetTableAlias(_tableName)}";
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Select(Expression<Func<T, object>> expression = null)
        {
            var sql = SelectParser(typeof(T));
            if (expression == null)
            {
                //解析所有的属性名与数据库对应的列名
                var dicColumnMapping = GetColumnMapping(typeof(T));
                string tableAlias = _sqlPack.GetTableAlias(_sqlPack.GetTableName(typeof(T)));
                StringBuilder sbColumnStr = new StringBuilder(); ;
                foreach (var key in dicColumnMapping.Keys)
                {
                    //lishuangquan 20200502 注意Dapper-Extensions返回的列，有时候包含前缀"`","["等，有时候不包含
                    if (!string.Format(_sqlPack.FormatTempl, key).Equals(dicColumnMapping[key], StringComparison.OrdinalIgnoreCase) &&
                        !key.Equals(dicColumnMapping[key], StringComparison.OrdinalIgnoreCase))
                    {
                        sbColumnStr.AppendFormat(" {0}.{1} AS {2},", tableAlias, string.Format(_sqlPack.FormatTempl, dicColumnMapping[key]), key);
                    }
                    else
                    {
                        sbColumnStr.AppendFormat(" {0}.{1},", tableAlias, string.Format(_sqlPack.FormatTempl, key));
                    }
                }
                //去除最后的逗号
                this._sqlPack.Sql.AppendFormat(sql, sbColumnStr.ToString().TrimEnd(','));
            }
            else
            {
                SqlBuilderProvider.Select(expression.Body, this._sqlPack);
                this._sqlPack.Sql.AppendFormat(sql, this._sqlPack.SelectFieldsStr);
            }
            return this;
        }
        /// <summary>
        /// 根据ClassMapper中设置的类型，实体类属性名与数据库列明的映射关系
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetColumnMapping(Type t)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (var property in t.GetProperties())
            {
                var propertyAlias = _sqlPack.FuncGetColumnName(t, property.Name);
                //当属性被忽略时，传递过来是空值
                if (propertyAlias != null)
                {
                    dic.Add(property.Name.ToUpper(), propertyAlias.ToUpper());
                }
            }
            return dic;
        }
        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Select<T2>(Expression<Func<T, T2, object>> expression = null)
            where T2 : class
        {
            var sql = SelectParser(typeof(T), typeof(T2));
            if (expression == null)
            {
                this._sqlPack.Sql.AppendFormat(sql, "*");
            }
            else
            {
                SqlBuilderProvider.Select(expression.Body, this._sqlPack);
                this._sqlPack.Sql.AppendFormat(sql, this._sqlPack.SelectFieldsStr);
            }
            return this;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Select<T2, T3>(Expression<Func<T, T2, T3, object>> expression = null)
            where T2 : class
            where T3 : class
        {
            var sql = SelectParser(typeof(T), typeof(T2), typeof(T3));
            if (expression == null)
            {
                this._sqlPack.Sql.AppendFormat(sql, "*");
            }
            else
            {
                SqlBuilderProvider.Select(expression.Body, this._sqlPack);
                this._sqlPack.Sql.AppendFormat(sql, this._sqlPack.SelectFieldsStr);
            }
            return this;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Select<T2, T3, T4>(Expression<Func<T, T2, T3, T4, object>> expression = null)
            where T2 : class
            where T3 : class
            where T4 : class
        {
            var sql = SelectParser(typeof(T), typeof(T2), typeof(T3), typeof(T4));
            if (expression == null)
            {
                this._sqlPack.Sql.AppendFormat(sql, "*");
            }
            else
            {
                SqlBuilderProvider.Select(expression.Body, this._sqlPack);
                this._sqlPack.Sql.AppendFormat(sql, this._sqlPack.SelectFieldsStr);
            }
            return this;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Select<T2, T3, T4, T5>(Expression<Func<T, T2, T3, T4, T5, object>> expression = null)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
        {
            var sql = SelectParser(typeof(T), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
            if (expression == null)
            {
                this._sqlPack.Sql.AppendFormat(sql, "*");
            }
            else
            {
                SqlBuilderProvider.Select(expression.Body, this._sqlPack);
                this._sqlPack.Sql.AppendFormat(sql, this._sqlPack.SelectFieldsStr);
            }
            return this;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Select<T2, T3, T4, T5, T6>(Expression<Func<T, T2, T3, T4, T5, T6, object>> expression = null)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
        {
            var sql = SelectParser(typeof(T), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
            if (expression == null)
            {
                this._sqlPack.Sql.AppendFormat(sql, "*");
            }
            else
            {
                SqlBuilderProvider.Select(expression.Body, this._sqlPack);
                this._sqlPack.Sql.AppendFormat(sql, this._sqlPack.SelectFieldsStr);
            }
            return this;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Select<T2, T3, T4, T5, T6, T7>(Expression<Func<T, T2, T3, T4, T5, T6, T7, object>> expression = null)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
        {
            var sql = SelectParser(typeof(T), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
            if (expression == null)
            {
                this._sqlPack.Sql.AppendFormat(sql, "*");
            }
            else
            {
                SqlBuilderProvider.Select(expression.Body, this._sqlPack);
                this._sqlPack.Sql.AppendFormat(sql, this._sqlPack.SelectFieldsStr);
            }
            return this;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Select<T2, T3, T4, T5, T6, T7, T8>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, object>> expression = null)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
        {
            var sql = SelectParser(typeof(T), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
            if (expression == null)
            {
                this._sqlPack.Sql.AppendFormat(sql, "*");
            }
            else
            {
                SqlBuilderProvider.Select(expression.Body, this._sqlPack);
                this._sqlPack.Sql.AppendFormat(sql, this._sqlPack.SelectFieldsStr);
            }
            return this;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <typeparam name="T9">泛型类型9</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Select<T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, object>> expression = null)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
            where T9 : class
        {
            var sql = SelectParser(typeof(T), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
            if (expression == null)
            {
                this._sqlPack.Sql.AppendFormat(sql, "*");
            }
            else
            {
                SqlBuilderProvider.Select(expression.Body, this._sqlPack);
                this._sqlPack.Sql.AppendFormat(sql, this._sqlPack.SelectFieldsStr);
            }
            return this;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <typeparam name="T9">泛型类型9</typeparam>
        /// <typeparam name="T10">泛型类型10</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Select<T2, T3, T4, T5, T6, T7, T8, T9, T10>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, object>> expression = null)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
            where T9 : class
            where T10 : class
        {
            var sql = SelectParser(typeof(T), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
            if (expression == null)
            {
                this._sqlPack.Sql.AppendFormat(sql, "*");
            }
            else
            {
                SqlBuilderProvider.Select(expression.Body, this._sqlPack);
                this._sqlPack.Sql.AppendFormat(sql, this._sqlPack.SelectFieldsStr);
            }
            return this;
        }
        #endregion

        #region Join
        /// <summary>
        /// JoinParser
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="leftOrRightJoin">左连接或者右连接</param>
        /// <returns>SqlBuilderCore</returns>
        private SqlBuilderCore<T> JoinParser<T2>(Expression<Func<T, T2, bool>> expression, string leftOrRightJoin = "")
            where T2 : class
        {
            string joinTableName = this._sqlPack.GetTableName(typeof(T2));
            this._sqlPack.SetTableAlias(joinTableName);
            if (this._sqlPack.DatabaseType == ConnectType.Oracle || this._sqlPack.DatabaseType == ConnectType.Dm || _sqlPack.DatabaseType == ConnectType.GBase)
                this._sqlPack.Sql.Append($"{(string.IsNullOrEmpty(leftOrRightJoin) ? "" : " " + leftOrRightJoin)} JOIN {joinTableName} {this._sqlPack.GetTableAlias(joinTableName)} ON ");
            else
                this._sqlPack.Sql.Append($"{(string.IsNullOrEmpty(leftOrRightJoin) ? "" : " " + leftOrRightJoin)} JOIN {joinTableName} AS {this._sqlPack.GetTableAlias(joinTableName)} ON ");
            SqlBuilderProvider.Join(expression.Body, this._sqlPack);
            return this;
        }

        /// <summary>
        /// JoinParser2
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="leftOrRightJoin">左连接或者右连接</param>
        /// <returns>SqlBuilderCore</returns>
        private SqlBuilderCore<T> JoinParser2<T2, T3>(Expression<Func<T2, T3, bool>> expression, string leftOrRightJoin = "")
            where T2 : class
            where T3 : class
        {
            string joinTableName = this._sqlPack.GetTableName(typeof(T3));
            this._sqlPack.SetTableAlias(joinTableName);
            if (this._sqlPack.DatabaseType == ConnectType.Oracle || this._sqlPack.DatabaseType == ConnectType.Dm || _sqlPack.DatabaseType == ConnectType.GBase)
                this._sqlPack.Sql.Append($"{(string.IsNullOrEmpty(leftOrRightJoin) ? "" : " " + leftOrRightJoin)} JOIN {joinTableName} {this._sqlPack.GetTableAlias(joinTableName)} ON ");
            else
                this._sqlPack.Sql.Append($"{(string.IsNullOrEmpty(leftOrRightJoin) ? "" : " " + leftOrRightJoin)} JOIN {joinTableName} AS {this._sqlPack.GetTableAlias(joinTableName)} ON ");
            SqlBuilderProvider.Join(expression.Body, this._sqlPack);
            return this;
        }

        /// <summary>
        /// Join
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Join<T2>(Expression<Func<T, T2, bool>> expression)
            where T2 : class
        {
            return JoinParser(expression);
        }

        /// <summary>
        /// Join
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Join<T2, T3>(Expression<Func<T2, T3, bool>> expression)
            where T2 : class
            where T3 : class
        {
            return JoinParser2(expression);
        }

        /// <summary>
        /// InnerJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> InnerJoin<T2>(Expression<Func<T, T2, bool>> expression)
            where T2 : class
        {
            return JoinParser(expression, "INNER");
        }

        /// <summary>
        /// InnerJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> InnerJoin<T2, T3>(Expression<Func<T2, T3, bool>> expression)
            where T2 : class
            where T3 : class
        {
            return JoinParser2(expression, "INNER");
        }

        /// <summary>
        /// LeftJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> LeftJoin<T2>(Expression<Func<T, T2, bool>> expression)
            where T2 : class
        {
            return JoinParser(expression, "LEFT");
        }

        /// <summary>
        /// LeftJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> LeftJoin<T2, T3>(Expression<Func<T2, T3, bool>> expression)
            where T2 : class
            where T3 : class
        {
            return JoinParser2(expression, "LEFT");
        }

        /// <summary>
        /// RightJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> RightJoin<T2>(Expression<Func<T, T2, bool>> expression)
            where T2 : class
        {
            return JoinParser(expression, "RIGHT");
        }

        /// <summary>
        /// RightJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> RightJoin<T2, T3>(Expression<Func<T2, T3, bool>> expression)
            where T2 : class
            where T3 : class
        {
            return JoinParser2(expression, "RIGHT");
        }

        /// <summary>
        /// FullJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> FullJoin<T2>(Expression<Func<T, T2, bool>> expression)
            where T2 : class
        {
            return JoinParser(expression, "FULL");
        }

        /// <summary>
        /// FullJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> FullJoin<T2, T3>(Expression<Func<T2, T3, bool>> expression)
            where T2 : class
            where T3 : class
        {
            return JoinParser2(expression, "FULL");
        }
        #endregion

        #region Where
        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Where(Expression<Func<T, bool>> expression)
        {
            //if (!(expression.Body.NodeType == ExpressionType.Constant && expression.Body.ToObject() is bool b && b))
            //{
            //    this._sqlPack += " WHERE ";
            //    SqlBuilderProvider.Where(expression.Body, this._sqlPack);
            //}
            //return this;
            if (!(expression.Body.NodeType == ExpressionType.Constant && expression.Body.ToObject() is bool b && b))
            {
                this._sqlPack += " WHERE ";
                SqlBuilderProvider.Where(expression.Body, this._sqlPack);
            }
            else if (expression.Body.NodeType == ExpressionType.Constant && expression.Body.ToObject() is bool c && c)
            {
                this._sqlPack += " WHERE ";
                if (c)
                {
                    this._sqlPack += " 1 = 1 ";
                }
            }
            return this;
        }
        /// <summary>
        /// 多表查询的Where语句
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public SqlBuilderCore<T> Where<T2>(Expression<Func<T2, bool>> expression)
        {
            
            if (!(expression.Body.NodeType == ExpressionType.Constant && expression.Body.ToObject() is bool b && b))
            {
                this._sqlPack += " WHERE ";
                SqlBuilderProvider.Where(expression.Body, this._sqlPack);
            }
            else if (expression.Body.NodeType == ExpressionType.Constant && expression.Body.ToObject() is bool c && c)
            {
                this._sqlPack += " WHERE ";
                if (c)
                {
                    this._sqlPack += " 1 = 1 ";
                }
            }
            return this;
        }
        #endregion

        #region AndWhere
        /// <summary>
        /// AndWhere
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> AndWhere(Expression<Func<T, bool>> expression)
        {
            var sql = this._sqlPack.ToString();
            if (sql.Contains("WHERE") && !string.IsNullOrEmpty(sql.Substring("WHERE").Trim()))
            {
                this._sqlPack += " AND ";
            }
            else
            {
                this._sqlPack += " WHERE ";
            }
            _sqlPack += " ( ";
            SqlBuilderProvider.Where(expression.Body, this._sqlPack);
            _sqlPack += " ) ";
            return this;
        }
        /// <summary>
        /// 多表的AndWhere查询
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public SqlBuilderCore<T> AndWhere<T2>(Expression<Func<T2, bool>> expression)
        {
            var sql = this._sqlPack.ToString();
            if (sql.Contains("WHERE") && !string.IsNullOrEmpty(sql.Substring("WHERE").Trim()))
            {
                this._sqlPack += " AND ";
            }
            else
            {
                this._sqlPack += " WHERE ";
            }
            _sqlPack += " ( ";
            SqlBuilderProvider.Where(expression.Body, this._sqlPack);
            _sqlPack += " ) ";
            return this;
        }
        #endregion

        #region OrWhere
        /// <summary>
        /// OrWhere
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrWhere(Expression<Func<T, bool>> expression)
        {
            var sql = this._sqlPack.ToString();
            if (sql.Contains("WHERE") && !string.IsNullOrEmpty(sql.Substring("WHERE").Trim()))
            {
                this._sqlPack += " OR ";
            }
            else
            {
                this._sqlPack += " WHERE ";
            }
            _sqlPack += " ( ";
            SqlBuilderProvider.Where(expression.Body, this._sqlPack);
            this._sqlPack += " ) ";
            return this;
        }
        /// <summary>
        /// OrWhere
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrWhere<T2>(Expression<Func<T2, bool>> expression) where T2:class
        {
            var sql = this._sqlPack.ToString();
            if (sql.Contains("WHERE") && !string.IsNullOrEmpty(sql.Substring("WHERE").Trim()))
            {
                this._sqlPack += " OR ";
            }
            else
            {
                this._sqlPack += " WHERE ";
            }
            _sqlPack += " ( ";
            SqlBuilderProvider.Where(expression.Body, this._sqlPack);
            this._sqlPack += " ) ";
            return this;
        }
        #endregion

        #region WithKey
        /// <summary>
        /// 添加主键条件，主要针对更新实体和删除实体操作
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> WithKey(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("实体参数不能为空！");
            }
            var sql = this._sqlPack.ToString().ToUpper();
            if (!sql.Contains("SELECT") && !sql.Contains("UPDATE") && !sql.Contains("DELETE"))
            {
                throw new ArgumentException("此方法只能用于Select、Update、Delete方法！");
            }
            var tableName = this._sqlPack.GetTableName(typeof(T));
            var tableAlias = this._sqlPack.GetTableAlias(tableName);
            if (!string.IsNullOrEmpty(tableAlias)) tableAlias += ".";
            var keys = this._sqlPack.GetPrimaryKey(typeof(T), string.IsNullOrEmpty(tableAlias));
            if (keys.Count > 0 && entity != null)
            {
                for (int i = 0; i < keys.Count; i++)
                {
                    //var (key, property) = keys[i];
                    var tuple = keys[i];
                    var key = tuple.Item1;
                    var property = tuple.Item2;
                    if (!string.IsNullOrEmpty(key))
                    {
                        var keyValue = typeof(T).GetProperty(property)?.GetValue(entity, null);
                        if (keyValue != null)
                        {
                            this._sqlPack += $" {(sql.Contains("WHERE") || i > 0 ? "AND" : "WHERE")} {(tableAlias + key)} = ";
                            this._sqlPack.AddDbParameter(keyValue);
                        }
                        else
                        {
                            throw new ArgumentNullException("主键值不能为空！");
                        }
                    }
                }
            }
            else
            {
                throw new ArgumentException("实体不存在Key属性！");
            }
            return this;
        }

        /// <summary>
        /// 添加主键条件，主要针对更新实体和删除实体操作
        /// </summary>
        /// <param name="keyValue">主键值</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> WithKey(params dynamic[] keyValue)
        {
            if (keyValue == null)
            {
                throw new ArgumentNullException("keyValue不能为空！");
            }
            if (!keyValue.Any(o => o.GetType().IsValueType || o.GetType() == typeof(string)))
            {
                throw new ArgumentException("keyValue只能为值类型或者字符串类型数据！");
            }
            var sql = this._sqlPack.ToString().ToUpper();
            if (!sql.Contains("SELECT") && !sql.Contains("UPDATE") && !sql.Contains("DELETE"))
            {
                throw new ArgumentException("WithKey方法只能用于Select、Update、Delete方法！");
            }
            var tableName = this._sqlPack.GetTableName(typeof(T));
            var tableAlias = this._sqlPack.GetTableAlias(tableName);
            if (!string.IsNullOrEmpty(tableAlias)) tableAlias += ".";
            var keys = this._sqlPack.GetPrimaryKey(typeof(T), string.IsNullOrEmpty(tableAlias));
            if (keys.Count > 0 && keyValue != null)
            {
                for (int i = 0; i < keys.Count; i++)
                {
                    //var (key, property) = keys[i];
                    var tuple = keys[i];
                    var key = tuple.Item1;
                    var property = tuple.Item2;

                    if (!string.IsNullOrEmpty(key))
                    {
                        this._sqlPack += $" {(sql.Contains("WHERE") || i > 0 ? "AND" : "WHERE")} {(tableAlias + key)} = ";
                        this._sqlPack.AddDbParameter(keyValue[i]);
                    }
                }
            }
            else
            {
                throw new ArgumentException("实体不存在Key属性！");
            }
            return this;
        }
        #endregion

        #region GroupBy
        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> GroupBy(Expression<Func<T, object>> expression)
        {
            this._sqlPack += " GROUP BY ";
            SqlBuilderProvider.GroupBy(expression.Body, this._sqlPack);
            return this;
        }
        #endregion

        #region OrderBy
        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrderBy(Expression<Func<T, object>> expression, params OrderType[] orders)
        {
            this._sqlPack += " ORDER BY ";
            SqlBuilderProvider.OrderBy(expression.Body, this._sqlPack, orders);
            return this;
        }
        /// <summary>
        /// 多表查询的OrderBy
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="orders"></param>
        /// <returns></returns>
        public SqlBuilderCore<T> OrderBy<T2>(Expression<Func<T2, object>> expression, params OrderType[] orders)
        {
            this._sqlPack += " ORDER BY ";
            SqlBuilderProvider.OrderBy(expression.Body, this._sqlPack, orders);
            return this;
        }
        #endregion

        #region Page
        /// <summary>
        /// Page
        /// </summary>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="sql">自定义sql语句</param>
        /// <param name="parameters">自定义sql格式化参数</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Page(int pageSize, int pageIndex, string orderField, string sql = null, Dictionary<string, object> parameters = null)
        {
            var sb = new StringBuilder();
            if (!orderField.ToUpper().Contains(" ASC") && !orderField.ToUpper().Contains(" DESC"))
                orderField = this._sqlPack.GetColumnName(orderField);
            if (!string.IsNullOrEmpty(sql))
            {
                this._sqlPack.DbParams.Clear();
                if (parameters != null) this._sqlPack.DbParams = parameters;
            }
            sql = string.IsNullOrEmpty(sql) ? this._sqlPack.Sql.ToString().TrimEnd(';') : sql.TrimEnd(';');
            //SQLServer
            if (this._sqlPack.DatabaseType == ConnectType.SQLServer)
            {
                if (Regex.IsMatch(sql, "WITH", RegexOptions.IgnoreCase))
                    sb.Append($"IF OBJECT_ID(N'TEMPDB..#TEMPORARY') IS NOT NULL DROP TABLE #TEMPORARY;{sql} SELECT * INTO #TEMPORARY FROM T;SELECT COUNT(1) AS Total FROM #TEMPORARY;WITH R AS (SELECT ROW_NUMBER() OVER (ORDER BY {orderField}) AS RowNumber,* FROM #TEMPORARY) SELECT * FROM R  WHERE RowNumber BETWEEN {((pageIndex - 1) * pageSize + 1)} AND {(pageIndex * pageSize)};DROP TABLE #TEMPORARY;");
                else
                    sb.Append($"IF OBJECT_ID(N'TEMPDB..#TEMPORARY') IS NOT NULL DROP TABLE #TEMPORARY;SELECT * INTO #TEMPORARY FROM ({sql}) AS T;SELECT COUNT(1) AS Total FROM #TEMPORARY;SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY {orderField}) AS RowNumber, * FROM #TEMPORARY) AS N WHERE RowNumber BETWEEN {((pageIndex - 1) * pageSize + 1)} AND {(pageIndex * pageSize)};DROP TABLE #TEMPORARY;");
            }
            //Oracle，注意Oracle需要分开查询总条数和分页数据
            if (this._sqlPack.DatabaseType == ConnectType.Oracle)
            {
                if (Regex.IsMatch(sql, "WITH", RegexOptions.IgnoreCase))
                    sb.Append($"{sql},R AS (SELECT ROWNUM AS RowNumber,T.* FROM T WHERE ROWNUM <= {pageSize * pageIndex} ORDER BY {orderField}) SELECT * FROM R WHERE RowNumber>={pageSize * (pageIndex - 1) + 1}");
                else
                    sb.Append($"SELECT * FROM (SELECT X.*,ROWNUM AS RowNumber FROM ({sql} ORDER BY {orderField}) X WHERE ROWNUM <= {pageSize * pageIndex}) T WHERE T.RowNumber >= {pageSize * (pageIndex - 1) + 1}");
            }
            //DM，暂时不清楚，先按oracle的逻辑
            if (this._sqlPack.DatabaseType == ConnectType.Dm)
            {
                if (Regex.IsMatch(sql, "WITH", RegexOptions.IgnoreCase))
                    sb.Append($"{sql},R AS (SELECT ROWNUM AS RowNumber,T.* FROM T WHERE ROWNUM <= {pageSize * pageIndex} ORDER BY {orderField}) SELECT * FROM R WHERE RowNumber>={pageSize * (pageIndex - 1) + 1}");
                else
                    sb.Append($"SELECT * FROM (SELECT X.*,ROWNUM AS RowNumber FROM ({sql} ORDER BY {orderField}) X WHERE ROWNUM <= {pageSize * pageIndex}) T WHERE T.RowNumber >= {pageSize * (pageIndex - 1) + 1}");
            }

            //GBase，暂时不清楚，先按oracle的逻辑
            if (this._sqlPack.DatabaseType == ConnectType.GBase)
            {
                if (Regex.IsMatch(sql, "WITH", RegexOptions.IgnoreCase))
                    sb.Append($"{sql},R AS (SELECT ROWNUM AS RowNumber,T.* FROM T WHERE ROWNUM <= {pageSize * pageIndex} ORDER BY {orderField}) SELECT * FROM R WHERE RowNumber>={pageSize * (pageIndex - 1) + 1}");
                else
                    sb.Append($"SELECT * FROM (SELECT X.*,ROWNUM AS RowNumber FROM ({sql} ORDER BY {orderField}) X WHERE ROWNUM <= {pageSize * pageIndex}) T WHERE T.RowNumber >= {pageSize * (pageIndex - 1) + 1}");
            }
            //MySQL，注意8.0版本才支持WITH语法
            if (this._sqlPack.DatabaseType == ConnectType.MySql)
            {
                if (Regex.IsMatch(sql, "WITH", RegexOptions.IgnoreCase))
                    sb.Append($"{sql} SELECT COUNT(1) AS Total FROM T;{sql} SELECT * FROM T ORDER BY {orderField} LIMIT {pageSize} OFFSET {(pageSize * (pageIndex - 1))};");
                else
                    sb.Append($"DROP TEMPORARY TABLE IF EXISTS $TEMPORARY;CREATE TEMPORARY TABLE $TEMPORARY SELECT * FROM ({sql}) AS T;SELECT COUNT(1) AS Total FROM $TEMPORARY;SELECT * FROM $TEMPORARY AS X ORDER BY {orderField} LIMIT {pageSize} OFFSET {(pageSize * (pageIndex - 1))};DROP TABLE $TEMPORARY;");
            }
            //PostgreSQL
            if (this._sqlPack.DatabaseType == ConnectType.PostgreSQL)
            {
                if (Regex.IsMatch(sql, "WITH", RegexOptions.IgnoreCase))
                    sb.Append($"{sql} SELECT COUNT(1) AS Total FROM T;{sql} SELECT * FROM T ORDER BY {orderField} LIMIT {pageSize} OFFSET {(pageSize * (pageIndex - 1))};");
                else
                    sb.Append($"DROP TABLE IF EXISTS TEMPORARY_TABLE;CREATE TEMPORARY TABLE TEMPORARY_TABLE AS SELECT * FROM ({sql}) AS T;SELECT COUNT(1) AS Total FROM TEMPORARY_TABLE;SELECT * FROM TEMPORARY_TABLE AS X ORDER BY {orderField} LIMIT {pageSize} OFFSET {(pageSize * (pageIndex - 1))};DROP TABLE TEMPORARY_TABLE;");
            }
            //SQLite
            if (this._sqlPack.DatabaseType == ConnectType.SQLite)
            {
                if (Regex.IsMatch(sql, "WITH", RegexOptions.IgnoreCase))
                    sb.Append($"{sql} SELECT COUNT(1) AS Total FROM T;{sql} SELECT * FROM T ORDER BY {orderField} LIMIT {pageSize} OFFSET {(pageSize * (pageIndex - 1))};");
                else
                    sb.Append($"SELECT COUNT(1) AS Total FROM ({sql}) AS T;SELECT * FROM ({sql}) AS X ORDER BY {orderField} LIMIT {pageSize} OFFSET {(pageSize * (pageIndex - 1))};");
            }
            this._sqlPack.Sql.Clear().Append(sb);
            return this;
        }

        /// <summary>
        /// PageByWith
        /// </summary>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="sql">自定义sql语句</param>
        /// <param name="parameters">自定义sql格式化参数</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> PageByWith(int pageSize, int pageIndex, string orderField, string sql = null, Dictionary<string, object> parameters = null)
        {
            var sb = new StringBuilder();
            if (!orderField.ToUpper().Contains(" ASC") && !orderField.ToUpper().Contains(" DESC"))
                orderField = this._sqlPack.GetColumnName(orderField);
            if (!string.IsNullOrEmpty(sql))
            {
                this._sqlPack.DbParams.Clear();
                if (parameters != null) this._sqlPack.DbParams = parameters;
            }
            sql = string.IsNullOrEmpty(sql) ? this._sqlPack.Sql.ToString().TrimEnd(';') : sql.TrimEnd(';');
            //SQLServer
            if (this._sqlPack.DatabaseType == ConnectType.SQLServer)
            {
                if (Regex.IsMatch(sql, "WITH", RegexOptions.IgnoreCase))
                    sb.Append($"IF OBJECT_ID(N'TEMPDB..#TEMPORARY') IS NOT NULL DROP TABLE #TEMPORARY;{sql} SELECT * INTO #TEMPORARY FROM T;SELECT COUNT(1) AS Total FROM #TEMPORARY;WITH R AS (SELECT ROW_NUMBER() OVER (ORDER BY {orderField}) AS RowNumber,* FROM #TEMPORARY) SELECT * FROM R  WHERE RowNumber BETWEEN {((pageIndex - 1) * pageSize + 1)} AND {(pageIndex * pageSize)};DROP TABLE #TEMPORARY;");
                else
                    sb.Append($"IF OBJECT_ID(N'TEMPDB..#TEMPORARY') IS NOT NULL DROP TABLE #TEMPORARY;WITH T AS ({sql}) SELECT * INTO #TEMPORARY FROM T;SELECT COUNT(1) AS Total FROM #TEMPORARY;WITH R AS (SELECT ROW_NUMBER() OVER (ORDER BY {orderField}) AS RowNumber,* FROM #TEMPORARY) SELECT * FROM R  WHERE RowNumber BETWEEN {((pageIndex - 1) * pageSize + 1)} AND {(pageIndex * pageSize)};DROP TABLE #TEMPORARY;");
            }
            //Oracle，注意Oracle需要分开查询总条数和分页数据
            if (this._sqlPack.DatabaseType == ConnectType.Oracle)
            {
                if (Regex.IsMatch(sql, "WITH", RegexOptions.IgnoreCase))
                    sb.Append($"{sql},R AS (SELECT ROWNUM AS RowNumber,T.* FROM T WHERE ROWNUM <= {pageSize * pageIndex} ORDER BY {orderField}) SELECT * FROM R WHERE RowNumber>={pageSize * (pageIndex - 1) + 1}");
                else
                    sb.Append($"WITH T AS ({sql}),R AS (SELECT ROWNUM AS RowNumber,T.* FROM T WHERE ROWNUM <= {pageSize * pageIndex} ORDER BY {orderField}) SELECT * FROM R WHERE RowNumber>={pageSize * (pageIndex - 1) + 1}");
            }
            //Dm，暂时不清楚，先按oracle的逻辑
            if (this._sqlPack.DatabaseType == ConnectType.Dm)
            {
                if (Regex.IsMatch(sql, "WITH", RegexOptions.IgnoreCase))
                    sb.Append($"{sql},R AS (SELECT ROWNUM AS RowNumber,T.* FROM T WHERE ROWNUM <= {pageSize * pageIndex} ORDER BY {orderField}) SELECT * FROM R WHERE RowNumber>={pageSize * (pageIndex - 1) + 1}");
                else
                    sb.Append($"WITH T AS ({sql}),R AS (SELECT ROWNUM AS RowNumber,T.* FROM T WHERE ROWNUM <= {pageSize * pageIndex} ORDER BY {orderField}) SELECT * FROM R WHERE RowNumber>={pageSize * (pageIndex - 1) + 1}");
            }
            //GBase，暂时不清楚，先按oracle的逻辑
            if (this._sqlPack.DatabaseType == ConnectType.GBase)
            {
                if (Regex.IsMatch(sql, "WITH", RegexOptions.IgnoreCase))
                    sb.Append($"{sql},R AS (SELECT ROWNUM AS RowNumber,T.* FROM T WHERE ROWNUM <= {pageSize * pageIndex} ORDER BY {orderField}) SELECT * FROM R WHERE RowNumber>={pageSize * (pageIndex - 1) + 1}");
                else
                    sb.Append($"WITH T AS ({sql}),R AS (SELECT ROWNUM AS RowNumber,T.* FROM T WHERE ROWNUM <= {pageSize * pageIndex} ORDER BY {orderField}) SELECT * FROM R WHERE RowNumber>={pageSize * (pageIndex - 1) + 1}");
            }
            //MySQL，注意8.0版本才支持WITH语法
            if (this._sqlPack.DatabaseType == ConnectType.MySql)
            {
                if (Regex.IsMatch(sql, "WITH", RegexOptions.IgnoreCase))
                    sb.Append($"{sql} SELECT COUNT(1) AS Total FROM T;{sql} SELECT * FROM T ORDER BY {orderField} LIMIT {pageSize} OFFSET {(pageSize * (pageIndex - 1))};");
                else
                    sb.Append($"WITH T AS ({sql}) SELECT COUNT(1) AS Total FROM T;WITH T AS ({sql}) SELECT * FROM T ORDER BY {orderField} LIMIT {pageSize} OFFSET {(pageSize * (pageIndex - 1))};");
            }
            //PostgreSQL
            if (this._sqlPack.DatabaseType == ConnectType.PostgreSQL)
            {
                if (Regex.IsMatch(sql, "WITH", RegexOptions.IgnoreCase))
                    sb.Append($"{sql} SELECT COUNT(1) AS Total FROM T;{sql} SELECT * FROM T ORDER BY {orderField} LIMIT {pageSize} OFFSET {(pageSize * (pageIndex - 1))};");
                else
                    sb.Append($"WITH T AS ({sql}) SELECT COUNT(1) AS Total FROM T;WITH T AS ({sql}) SELECT * FROM T ORDER BY {orderField} LIMIT {pageSize} OFFSET {(pageSize * (pageIndex - 1))};");
            }
            //SQLite
            if (this._sqlPack.DatabaseType == ConnectType.SQLite)
            {
                if (Regex.IsMatch(sql, "WITH", RegexOptions.IgnoreCase))
                    sb.Append($"{sql} SELECT COUNT(1) AS Total FROM T;{sql} SELECT * FROM T ORDER BY {orderField} LIMIT {pageSize} OFFSET {(pageSize * (pageIndex - 1))};");
                else
                    sb.Append($"WITH T AS ({sql}) SELECT COUNT(1) AS Total FROM T;WITH T AS ({sql}) SELECT * FROM T ORDER BY {orderField} LIMIT {pageSize} OFFSET {(pageSize * (pageIndex - 1))};");
            }
            this._sqlPack.Sql.Clear().Append(sb);
            return this;
        }
        #endregion

        #region Delete
        /// <summary>
        /// Delete
        /// </summary>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Delete()
        {
            this._sqlPack.Clear();
            this._sqlPack.IsSingleTable = true;
            this._sqlPack += $"DELETE FROM {this._sqlPack.GetTableName(typeof(T))}";
            return this;
        }
        #endregion

        #region Update
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="isEnableNullValue">是否对null值属性进行sql拼接操作，默认：是</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Update(Expression<Func<object>> expression = null, bool isEnableNullValue = true)
        {
            this._sqlPack.Clear();
            this._sqlPack.IsSingleTable = true;
            this._sqlPack.IsEnableNullValue = isEnableNullValue;
            this._sqlPack += $"UPDATE {this._sqlPack.GetTableName(typeof(T))} SET ";
            SqlBuilderProvider.Update(expression.Body, this._sqlPack);
            return this;
        }
        #endregion

        #region Insert
        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="isEnableNullValue">是否对null值属性进行sql拼接操作，默认：是</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Insert(Expression<Func<object>> expression = null, bool isEnableNullValue = true)
        {
            this._sqlPack.Clear();
            this._sqlPack.IsSingleTable = true;
            this._sqlPack.IsEnableNullValue = isEnableNullValue;
            this._sqlPack += $"INSERT INTO {this._sqlPack.GetTableName(typeof(T))} ({{0}}) {(this._sqlPack.DatabaseType == ConnectType.Oracle ? "SELECT" : "VALUES")} ";
            SqlBuilderProvider.Insert(expression.Body, this._sqlPack);
            return this;
        }
        #endregion

        #region Max
        /// <summary>
        /// Max
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Max(Expression<Func<T, object>> expression)
        {
            this._sqlPack.Clear();
            this._sqlPack.IsSingleTable = true;
            SqlBuilderProvider.Max(expression.Body, this._sqlPack);
            return this;
        }
        #endregion

        #region Min
        /// <summary>
        /// Min
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Min(Expression<Func<T, object>> expression)
        {
            this._sqlPack.Clear();
            this._sqlPack.IsSingleTable = true;
            SqlBuilderProvider.Min(expression.Body, this._sqlPack);
            return this;
        }
        #endregion

        #region Avg
        /// <summary>
        /// Avg
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Avg(Expression<Func<T, object>> expression)
        {
            this._sqlPack.Clear();
            this._sqlPack.IsSingleTable = true;
            SqlBuilderProvider.Avg(expression.Body, this._sqlPack);
            return this;
        }
        #endregion

        #region Count
        /// <summary>
        /// Count
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Count(Expression<Func<T, object>> expression = null)
        {
            this._sqlPack.Clear();
            this._sqlPack.IsSingleTable = true;
            if (expression == null)
            {
                this._sqlPack.Sql.Append($"SELECT COUNT(*) FROM {this._sqlPack.GetTableName(typeof(T))}");
            }
            else
            {
                SqlBuilderProvider.Count(expression.Body, this._sqlPack);
            }
            return this;
        }
        #endregion

        #region Sum
        /// <summary>
        /// Sum
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Sum(Expression<Func<T, object>> expression)
        {
            this._sqlPack.Clear();
            this._sqlPack.IsSingleTable = true;
            SqlBuilderProvider.Sum(expression.Body, this._sqlPack);
            return this;
        }
        #endregion

        #region Top
        /// <summary>
        /// Top
        /// </summary>
        /// <param name="topNumber">top数量</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Top(long topNumber)
        {
            if (this._sqlPack.DatabaseType == ConnectType.SQLServer || _sqlPack.DatabaseType == ConnectType.GBase)
            {
                if (this._sqlPack.Sql.ToString().ToUpper().Contains("DISTINCT"))
                {
                    this._sqlPack.Sql.Replace("DISTINCT", $"DISTINCT TOP {topNumber}", this._sqlPack.Sql.ToString().IndexOf("DISTINCT"), 8);
                }
                else
                {
                    this._sqlPack.Sql.Replace("SELECT", $"SELECT TOP {topNumber}", this._sqlPack.Sql.ToString().IndexOf("SELECT"), 6);
                }
            }
            else if (this._sqlPack.DatabaseType == ConnectType.Oracle)
            {
                if (this._sqlPack.Sql.ToString().ToUpper().Contains("WHERE"))
                {
                    this._sqlPack.Sql.Append($" AND ROWNUM <= {topNumber}");
                }
                else
                {
                    this._sqlPack.Sql.Append($" WHERE ROWNUM <= {topNumber}");
                }
            }
            else if (this._sqlPack.DatabaseType == ConnectType.MySql || this._sqlPack.DatabaseType == ConnectType.SQLite || this._sqlPack.DatabaseType == ConnectType.PostgreSQL|| this._sqlPack.DatabaseType == ConnectType.Dm)
            {
                this._sqlPack.Sql.Append($" LIMIT {topNumber} OFFSET 0");
            }
            return this;
        }
        #endregion

        #region Distinct
        /// <summary>
        /// Distinct
        /// </summary>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Distinct()
        {
            this._sqlPack.Sql.Replace("SELECT", $"SELECT DISTINCT", this._sqlPack.Sql.ToString().IndexOf("SELECT"), 6);
            return this;
        }
        #endregion

        #region GetTableName
        /// <summary>
        /// 获取实体对应的表名
        /// </summary>
        /// <returns></returns>
        public string GetTableName()
        {
            return this._sqlPack.GetTableName(typeof(T));
        }
        #endregion

        #region GetPrimaryKey
        /// <summary>
        /// 获取实体对应表的主键名称
        /// </summary>
        /// <returns></returns>
        public List<string> GetPrimaryKey()
        {
            //return this._sqlPack.GetPrimaryKey(typeof(T), false).Select(o => o.key).ToList();
            return this._sqlPack.GetPrimaryKey(typeof(T), false).Select(o => o.Item1).ToList();
        }
        #endregion
        #endregion
    }
    #endregion

    #region DatabaseType
    ///// <summary>
    ///// 数据库类型
    ///// </summary>
    //public enum DatabaseType
    //{
    //    /// <summary>
    //    /// SqlServer数据库类型
    //    /// </summary>
    //    SQLServer,

    //    /// <summary>
    //    /// MySQL数据库类型
    //    /// </summary>
    //    MySQL,

    //    /// <summary>
    //    /// Oracle数据库类型
    //    /// </summary>
    //    Oracle,

    //    /// <summary>
    //    /// SQLite数据库类型
    //    /// </summary>
    //    SQLite,

    //    /// <summary>
    //    /// PostgreSQL数据库类型
    //    /// </summary>
    //    PostgreSQL,
    //}
    #endregion

    #region OrderType
    /// <summary>
    /// 排序方式
    /// </summary>
    internal enum OrderType
    {
        /// <summary>
        /// 升序
        /// </summary>
        Ascending,

        /// <summary>
        /// 降序
        /// </summary>
        Descending
    }
    #endregion

    //#region Attribute
    //#region TableAttribute
    ///// <summary>
    ///// 指定表名
    ///// </summary>
    //[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
    //public class TableAttribute : Attribute
    //{
    //    /// <summary>
    //    /// 构造函数
    //    /// </summary>
    //    /// <param name="name">数据库表名</param>
    //    public TableAttribute(string name = null)
    //    {
    //        if (name != null) this.Name = name;
    //    }

    //    /// <summary>
    //    /// 数据库表名
    //    /// </summary>
    //    public string Name { get; private set; }

    //    /// <summary>
    //    /// 数据库模式
    //    /// </summary>
    //    public string Schema { get; set; }
    //}
    //#endregion

    //#region ColumnAttribute
    ///// <summary>
    ///// 指定列名
    ///// </summary>
    //[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    //public class ColumnAttribute : Attribute
    //{
    //    /// <summary>
    //    /// 构造函数
    //    /// </summary>
    //    /// <param name="name">列名</param>    
    //    public ColumnAttribute(string name = null)
    //    {
    //        if (name != null) this.Name = name;
    //    }

    //    /// <summary>
    //    /// 数据库表列名
    //    /// </summary>
    //    public string Name { get; private set; }

    //    /// <summary>
    //    /// 新增是否有效
    //    /// </summary>
    //    public bool Insert { get; set; } = true;

    //    /// <summary>
    //    /// 更新是否有效
    //    /// </summary>
    //    public bool Update { get; set; } = true;
    //}
    //#endregion

    //#region KeyAttribute
    ///// <summary>
    ///// 指定表主键
    ///// </summary>
    //[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    //public class KeyAttribute : Attribute
    //{
    //    /// <summary>
    //    /// 构造函数
    //    /// </summary>
    //    /// <param name="name"></param>
    //    public KeyAttribute(string name = null)
    //    {
    //        if (name != null) this.Name = name;
    //    }

    //    /// <summary>
    //    /// 主键名称
    //    /// </summary>
    //    public string Name { get; set; }
    //}
    //#endregion
    //#endregion

    #region Extensions
    /// <summary>
    /// 扩展类
    /// </summary>
    public static class Extensions
    {
        #region Like
        /// <summary>
        /// LIKE
        /// </summary>
        /// <param name="this">扩展对象自身</param>
        /// <param name="value">包含的字符串</param>
        /// <returns>bool</returns>
        public static bool Like(this object @this, string value) => true;
        #endregion

        #region LikeLeft
        /// <summary>
        /// LIKE '% _ _ _'
        /// </summary>
        /// <param name="this">扩展对象自身</param>
        /// <param name="value">包含的字符串</param>
        /// <returns>bool</returns>
        public static bool LikeLeft(this object @this, string value) => true;
        #endregion

        #region LikeRight
        /// <summary>
        /// LIKE '_ _ _ %'
        /// </summary>
        /// <param name="this">扩展对象自身</param>
        /// <param name="value">包含的字符串</param>
        /// <returns>bool</returns>
        public static bool LikeRight(this object @this, string value) => true;
        #endregion

        #region NotLike
        /// <summary>
        /// NOT LIKE
        /// </summary>
        /// <param name="this">扩展对象自身</param>
        /// <param name="value">包含的字符串</param>
        /// <returns>bool</returns>
        public static bool NotLike(this object @this, string value) => true;
        #endregion

        #region In
        /// <summary>
        /// IN
        /// </summary>
        /// <typeparam name="T">IN数组里面的数据类型</typeparam>
        /// <param name="this">扩展对象自身</param>
        /// <param name="array">IN数组</param>
        /// <returns>bool</returns>
        public static bool In<T>(this object @this, params T[] array) => true;
        #endregion

        #region NotIn
        /// <summary>
        /// NOT IN
        /// </summary>
        /// <typeparam name="T">NOT IN数组里面的数据类型</typeparam>
        /// <param name="this">扩展对象自身</param>
        /// <param name="array">NOT IN数组</param>
        /// <returns>bool</returns>
        public static bool NotIn<T>(this object @this, params T[] array) => true;
        #endregion

        #region True
        /// <summary>
        /// True
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> True<T>() => parameter => true;
        #endregion

        #region False
        /// <summary>
        /// False
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> False<T>() => parameter => false;
        #endregion

        #region Or
        /// <summary>
        /// Or
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> @this, Expression<Func<T, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(@this.Body, invokedExpr), @this.Parameters);
        }
        #endregion

        #region And
        /// <summary>
        /// And
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> @this, Expression<Func<T, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(@this.Body, invokedExpr), @this.Parameters);
        }
        #endregion

        #region ToLambda
        /// <summary>
        /// ToLambda
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static Expression<T> ToLambda<T>(this Expression @this, params ParameterExpression[] parameters)
        {
            return Expression.Lambda<T>(@this, parameters);
        }
        #endregion

        #region ToObject
        /// <summary>
        /// 转换Expression为object
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static object ToObject(this Expression @this)
        {
            object obj = null;
            switch (@this.NodeType)
            {
                case ExpressionType.Constant:
                    obj = (@this as ConstantExpression)?.Value;
                    break;
                case ExpressionType.Convert:
                    obj = (@this as UnaryExpression)?.Operand?.ToObject();
                    break;
                default:
                    var isNullable = @this.Type.IsNullable();
                    switch (@this.Type.GetCoreType().Name.ToLower())
                    {
                        case "string":
                            obj = @this.ToLambda<Func<string>>().Compile().Invoke();
                            break;
                        case "int16":
                            if (isNullable)
                                obj = @this.ToLambda<Func<short?>>().Compile().Invoke();
                            else
                                obj = @this.ToLambda<Func<short>>().Compile().Invoke();
                            break;
                        case "int32":
                            if (isNullable)
                                obj = @this.ToLambda<Func<int?>>().Compile().Invoke();
                            else
                                obj = @this.ToLambda<Func<int>>().Compile().Invoke();
                            break;
                        case "int64":
                            if (isNullable)
                                obj = @this.ToLambda<Func<long?>>().Compile().Invoke();
                            else
                                obj = @this.ToLambda<Func<long>>().Compile().Invoke();
                            break;
                        case "decimal":
                            if (isNullable)
                                obj = @this.ToLambda<Func<decimal?>>().Compile().Invoke();
                            else
                                obj = @this.ToLambda<Func<decimal>>().Compile().Invoke();
                            break;
                        case "double":
                            if (isNullable)
                                obj = @this.ToLambda<Func<double?>>().Compile().Invoke();
                            else
                                obj = @this.ToLambda<Func<double>>().Compile().Invoke();
                            break;
                        case "datetime":
                            if (isNullable)
                                obj = @this.ToLambda<Func<DateTime?>>().Compile().Invoke();
                            else
                                obj = @this.ToLambda<Func<DateTime>>().Compile().Invoke();
                            break;
                        case "boolean":
                            if (isNullable)
                                obj = @this.ToLambda<Func<bool?>>().Compile().Invoke();
                            else
                                obj = @this.ToLambda<Func<bool>>().Compile().Invoke();
                            break;
                        case "byte":
                            if (isNullable)
                                obj = @this.ToLambda<Func<byte?>>().Compile().Invoke();
                            else
                                obj = @this.ToLambda<Func<byte>>().Compile().Invoke();
                            break;
                        case "char":
                            if (isNullable)
                                obj = @this.ToLambda<Func<char?>>().Compile().Invoke();
                            else
                                obj = @this.ToLambda<Func<char>>().Compile().Invoke();
                            break;
                        case "single":
                            if (isNullable)
                                obj = @this.ToLambda<Func<float?>>().Compile().Invoke();
                            else
                                obj = @this.ToLambda<Func<float>>().Compile().Invoke();
                            break;
                        default:
                            obj = @this.ToLambda<Func<object>>().Compile().Invoke();
                            break;
                    }
                    break;
            }
            return obj;
        }
        #endregion

        #region OrderBy
        /// <summary>
        /// linq正序排序扩展
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string property)
        {
            return source.BuildIOrderedQueryable<T>(property, "OrderBy");
        }

        /// <summary>
        /// linq倒叙排序扩展
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string property)
        {
            return source.BuildIOrderedQueryable<T>(property, "OrderByDescending");
        }

        /// <summary>
        /// linq正序多列排序扩展
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string property)
        {
            return source.BuildIOrderedQueryable<T>(property, "ThenBy");
        }

        /// <summary>
        /// linq倒序多列排序扩展
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, string property)
        {
            return source.BuildIOrderedQueryable<T>(property, "ThenByDescending");
        }

        /// <summary>
        /// 根据属性和排序方法构建IOrderedQueryable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="property"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static IOrderedQueryable<T> BuildIOrderedQueryable<T>(this IQueryable<T> source, string property, string methodName)
        {
            var props = property.Split('.');
            var type = typeof(T);
            var arg = Expression.Parameter(type, "x");
            Expression expr = arg;
            foreach (var prop in props)
            {
                // use reflection (not ComponentModel) to mirror LINQ
                var pi = type.GetProperty(prop);
                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }
            var delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
            var lambda = Expression.Lambda(delegateType, expr, arg);
            var result = typeof(Queryable).GetMethods().Single(
              method => method.Name == methodName
                && method.IsGenericMethodDefinition
                && method.GetGenericArguments().Length == 2
                && method.GetParameters().Length == 2)
              .MakeGenericMethod(typeof(T), type)
              .Invoke(null, new object[] { source, lambda });
            return (IOrderedQueryable<T>)result;
        }
        #endregion

        #region Substring
        /// <summary>
        /// 从分隔符开始向尾部截取字符串
        /// </summary>
        /// <param name="this">源字符串</param>
        /// <param name="separator">分隔符</param>
        /// <param name="lastIndexOf">true：从最后一个匹配的分隔符开始截取，false：从第一个匹配的分隔符开始截取，默认：true</param>
        /// <returns>string</returns>
        public static string Substring(this string @this, string separator, bool lastIndexOf = true)
        {
            var start = (lastIndexOf ? @this.LastIndexOf(separator) : @this.IndexOf(separator)) + separator.Length;
            var length = @this.Length - start;
            return @this.Substring(start, length);
        }
        #endregion

        #region GetCoreType
        /// <summary>
        /// 如果type是Nullable类型则返回UnderlyingType，否则则直接返回type本身
        /// </summary>
        /// <param name="this">类型</param>
        /// <returns>Type</returns>
        public static Type GetCoreType(this Type @this)
        {
            if (@this?.IsNullable() == true)
            {
                @this = Nullable.GetUnderlyingType(@this);
            }
            return @this;
        }
        #endregion

        #region IsNullable
        /// <summary>
        /// 判断类型是否是Nullable类型
        /// </summary>
        /// <param name="this">类型</param>
        /// <returns>bool</returns>
        public static bool IsNullable(this Type @this)
        {
            return @this.IsValueType && @this.IsGenericType && @this.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
        #endregion

        #region IsNull
        /// <summary>
        /// 是否为空
        /// </summary>
        /// <param name="this">object对象</param>
        /// <returns>bool</returns>
        public static bool IsNull(this object @this)
        {
            return @this == null || @this == DBNull.Value;
        }
        #endregion

        #region IsNullOrEmpty
        /// <summary>
        /// 判断字符串是否为空
        /// </summary>
        /// <param name="this">待验证的字符串</param>
        /// <returns>bool</returns>
        public static bool IsNullOrEmpty(this string @this)
        {
            return string.IsNullOrEmpty(@this);
        }
        #endregion

        #region IsNullOrWhiteSpace
        /// <summary>
        /// 判断字符串是否为空
        /// </summary>
        /// <param name="this">待验证的字符串</param>
        /// <returns>bool</returns>
        public static bool IsNullOrWhiteSpace(this string @this)
        {
            return string.IsNullOrWhiteSpace(@this);
        }
        #endregion

        #region ToSafeValue
        /// <summary>
        /// 转换为安全类型的值
        /// </summary>
        /// <param name="this">object对象</param>
        /// <param name="type">type</param>
        /// <returns>object</returns>
        public static object ToSafeValue(this object @this, Type type)
        {
            return @this == null ? null : Convert.ChangeType(@this, type.GetCoreType());
        }
        #endregion

        #region ToDynamicParameters
        ///// <summary>
        ///// DbParameter转换为DynamicParameters
        ///// </summary>
        ///// <param name="this"></param>
        ///// <returns></returns>
        //public static DynamicParameters ToDynamicParameters(this DbParameter[] @this)
        //{
        //    if (@this?.Length > 0)
        //    {
        //        var args = new DynamicParameters();
        //        @this.ToList().ForEach(p => args.Add(p.ParameterName, p.Value, p.DbType, p.Direction, p.Size));
        //        return args;
        //    }
        //    return null;
        //}

        ///// <summary>
        ///// DbParameter转换为DynamicParameters
        ///// </summary>
        ///// <param name="this"></param>
        ///// <returns></returns>
        //public static DynamicParameters ToDynamicParameters(this List<DbParameter> @this)
        //{
        //    if (@this?.Count > 0)
        //    {
        //        var args = new DynamicParameters();
        //        @this.ForEach(p => args.Add(p.ParameterName, p.Value, p.DbType, p.Direction, p.Size));
        //        return args;
        //    }
        //    return null;
        //}

        ///// <summary>
        /////  DbParameter转换为DynamicParameters
        ///// </summary>
        ///// <param name="this"></param>
        ///// <returns></returns>
        //public static DynamicParameters ToDynamicParameters(this DbParameter @this)
        //{
        //    if (@this != null)
        //    {
        //        var args = new DynamicParameters();
        //        args.Add(@this.ParameterName, @this.Value, @this.DbType, @this.Direction, @this.Size);
        //        return args;
        //    }
        //    return null;
        //}

        ///// <summary>
        /////  IDictionary转换为DynamicParameters
        ///// </summary>
        ///// <param name="this"></param>        
        ///// <returns></returns>
        //public static DynamicParameters ToDynamicParameters(this IDictionary<string, object> @this)
        //{
        //    if (@this?.Count > 0)
        //    {
        //        var args = new DynamicParameters();
        //        foreach (var item in @this)
        //        {
        //            args.Add(item.Key, item.Value);
        //        }
        //        return args;
        //    }
        //    return null;
        //}
        #endregion

        #region ToDbParameters
        /// <summary>
        /// An IDictionary&lt;string,object&gt; extension method that converts this object to a database parameters.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="command">The command.</param>
        /// <returns>The given data converted to a DbParameter[].</returns>
        public static DbParameter[] ToDbParameters(this IDictionary<string, object> @this, DbCommand command)
        {
            if (@this?.Count > 0)
            {
                return @this.Select(x =>
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = x.Key;
                    parameter.Value = x.Value;
                    return parameter;
                }).ToArray();
            }
            return null;
        }

        /// <summary>
        ///  An IDictionary&lt;string,object&gt; extension method that converts this object to a database parameters.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="connection">The connection.</param>        
        /// <returns>The given data converted to a DbParameter[].</returns>
        public static DbParameter[] ToDbParameters(this IDictionary<string, object> @this, DbConnection connection)
        {
            if (@this?.Count > 0)
            {
                var command = connection.CreateCommand();
                return @this.Select(x =>
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = x.Key;
                    parameter.Value = x.Value;
                    return parameter;
                }).ToArray();
            }
            return null;
        }
        #endregion

        #region ToSqlParameters
        /// <summary>
        /// An IDictionary&lt;string,object&gt; extension method that converts the @this to a SQL parameters.
        /// </summary>
        /// <param name="this">The @this to act on.</param>        
        /// <returns>@this as a SqlParameter[].</returns>
        public static SqlParameter[] ToSqlParameters(this IDictionary<string, object> @this)
        {
            if (@this?.Count > 0)
            {
                return @this.Select(x => new SqlParameter(x.Key.Replace("?", "@").Replace(":", "@"), x.Value)).ToArray();
            }
            return null;
        }
        #endregion

        #region ToMySqlParameters
        /// <summary>
        /// An IDictionary&lt;string,object&gt; extension method that converts the @this to a MySQL parameters.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a MySqlParameter[].</returns>
        public static MySqlParameter[] ToMySqlParameters(this IDictionary<string, object> @this)
        {
            if (@this?.Count > 0)
            {
                return @this.Select(x => new MySqlParameter(x.Key.Replace("@", "?").Replace(":", "?"), x.Value)).ToArray();
            }
            return null;
        }
        #endregion

        #region ToSQLiteParameters
        ///// <summary>
        ///// An IDictionary&lt;string,object&gt; extension method that converts the @this to a SQLite parameters.
        ///// </summary>
        ///// <param name="this">The @this to act on.</param>        
        ///// <returns>@this as a SQLiteParameter[].</returns>
        //public static SQLiteParameter[] ToSQLiteParameters(this IDictionary<string, object> @this)
        //{
        //    if (@this?.Count > 0)
        //    {
        //        return @this.Select(x => new SQLiteParameter(x.Key.Replace("?", "@").Replace(":", "@"), x.Value)).ToArray();
        //    }
        //    return null;
        //}
        #endregion

        #region ToOracleParameters
        /// <summary>
        /// An IDictionary&lt;string,object&gt; extension method that converts the @this to a Oracle parameters.
        /// </summary>
        /// <param name="this">The @this to act on.</param>        
        /// <returns>@this as a OracleParameter[].</returns>
        public static OracleParameter[] ToOracleParameters(this IDictionary<string, object> @this)
        {
            if (@this?.Count > 0)
            {
                return @this.Select(x => new OracleParameter(x.Key.Replace("?", ":").Replace("@", ":"), x.Value)).ToArray();
            }
            return null;
        }
        #endregion

        #region ToDmParameters
        /// <summary>
        /// An IDictionary&lt;string,object&gt; extension method that converts the @this to a Dm parameters.
        /// </summary>
        /// <param name="this">The @this to act on.</param>        
        /// <returns>@this as a DmParameter[].</returns>
        public static DmParameter[] ToDmParameters(this IDictionary<string, object> @this)
        {
            if (@this?.Count > 0)
            {
                return @this.Select(x => new DmParameter(x.Key.Replace("?", ":").Replace("@", ":"), x.Value)).ToArray();
            }
            return null;
        }
        #endregion
        #region ToGBaseParameters
        /// <summary>
        /// An IDictionary&lt;string,object&gt; extension method that converts the @this to a Dm parameters.
        /// </summary>
        /// <param name="this">The @this to act on.</param>        
        /// <returns>@this as a DmParameter[].</returns>
        public static DmParameter[] ToGBaseParameters(this IDictionary<string, object> @this)
        {
            if (@this?.Count > 0)
            {
                return @this.Select(x => new DmParameter(x.Key.Replace("?", ":").Replace("@", ":"), x.Value)).ToArray();
            }
            return null;
        }
        #endregion

        #region ToDB2Parameters
        public static IBM.Data.DB2.DB2Parameter[] ToDB2Parameters(this IDictionary<string, object> @this)
        {
            if (@this?.Count > 0)
            {
                return @this.Select(x => new IBM.Data.DB2.DB2Parameter(x.Key.Replace("?", "@").Replace(":", "@"), x.Value)).ToArray();
            }
            return null;
        }
        #endregion

        #region ToNpgsqlParameters
        ///// <summary>
        ///// An IDictionary&lt;string,object&gt; extension method that converts the @this to a PostgreSQL parameters.
        ///// </summary>
        ///// <param name="this">The @this to act on.</param>
        ///// <returns>@this as a NpgsqlParameter[].</returns>
        //public static NpgsqlParameter[] ToNpgsqlParameters(this IDictionary<string, object> @this)
        //{
        //    if (@this?.Count > 0)
        //    {
        //        return @this.Select(x => new NpgsqlParameter(x.Key.Replace("?", ":").Replace("@", ":"), x.Value)).ToArray();
        //    }
        //    return null;
        //}
        #endregion

        #region ToDataTable
        /// <summary>
        /// IDataReader转换为DataTable
        /// </summary>
        /// <param name="this">reader数据源</param>
        /// <returns>DataTable</returns>
        public static DataTable ToDataTable(this IDataReader @this)
        {
            var table = new DataTable();
            if (@this?.IsClosed == false)
            {
                table.Load(@this);
            }
            return table;
        }

        /// <summary>
        /// List集合转DataTable
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="this">list数据源</param>
        /// <returns>DataTable</returns>
        public static DataTable ToDataTable<T>(this List<T> @this)
        {
            DataTable dt = null;
            if (@this?.Count > 0)
            {
                dt = new DataTable(typeof(T).Name);
                var typeName = typeof(T).Name;
                var first = @this.FirstOrDefault();
                var firstTypeName = first.GetType().Name;
                if (typeName.Contains("Dictionary`2") || (typeName == "Object" && (firstTypeName == "DapperRow" || firstTypeName == "DynamicRow")))
                {
                    var dic = first as IDictionary<string, object>;
                    dt.Columns.AddRange(dic.Select(o => new DataColumn(o.Key, o.Value?.GetType().GetCoreType() ?? typeof(object))).ToArray());
                    var dics = @this.Select(o => o as IDictionary<string, object>);
                    foreach (var item in dics)
                    {
                        dt.Rows.Add(item.Select(o => o.Value).ToArray());
                    }
                }
                else
                {
                    var props = typeName == "Object" ? first.GetType().GetProperties() : typeof(T).GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);
                    foreach (var prop in props)
                    {
                        dt.Columns.Add(prop.Name, prop?.PropertyType.GetCoreType() ?? typeof(object));
                    }
                    foreach (var item in @this)
                    {
                        var values = new object[props.Length];
                        for (var i = 0; i < props.Length; i++)
                        {
                            if (!props[i].CanRead) continue;
                            values[i] = props[i].GetValue(item, null);
                        }
                        dt.Rows.Add(values);
                    }
                }
            }
            return dt;
        }
        #endregion

        #region ToDataSet
        /// <summary>
        /// IDataReader转换为DataSet
        /// </summary>
        /// <param name="this">reader数据源</param>
        /// <returns>DataSet</returns>
        public static DataSet ToDataSet(this IDataReader @this)
        {
            var ds = new DataSet();
            if (@this.IsClosed == false)
            {
                do
                {
                    var schemaTable = @this.GetSchemaTable();
                    var dt = new DataTable();
                    for (var i = 0; i < schemaTable.Rows.Count; i++)
                    {
                        var row = schemaTable.Rows[i];
                        dt.Columns.Add(new DataColumn((string)row["ColumnName"], (Type)row["DataType"]));
                    }
                    while (@this.Read())
                    {
                        var dataRow = dt.NewRow();
                        for (var i = 0; i < @this.FieldCount; i++)
                        {
                            dataRow[i] = @this.GetValue(i);
                        }
                        dt.Rows.Add(dataRow);
                    }
                    ds.Tables.Add(dt);
                }
                while (@this.NextResult());
            }
            return ds;
        }
        #endregion

        #region ToDynamic
        /// <summary>
        /// IDataReader数据转为dynamic对象
        /// </summary>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>dynamic</returns>
        public static dynamic ToDynamic(this IDataReader @this)
        {
            return @this.ToDynamics()?.FirstOrDefault();
        }

        /// <summary>
        /// IDataReader数据转为dynamic对象集合
        /// </summary>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>dynamic集合</returns>
        public static IEnumerable<dynamic> ToDynamics(this IDataReader @this)
        {
            if (@this?.IsClosed == false)
            {
                using (@this)
                {
                    while (@this.Read())
                    {
                        var row = new ExpandoObject() as IDictionary<string, object>;
                        for (var i = 0; i < @this.FieldCount; i++)
                        {
                            row.Add(@this.GetName(i), @this.GetValue(i));
                        }
                        yield return row;
                    }
                }
            }
        }
        #endregion

        #region ToDictionary
        /// <summary>
        /// IDataReader数据转为Dictionary对象
        /// </summary>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>Dictionary</returns>
        public static Dictionary<string, object> ToDictionary(this IDataReader @this)
        {
            return @this.ToDictionaries()?.FirstOrDefault();
        }

        /// <summary>
        /// IDataReader数据转为Dictionary对象集合
        /// </summary>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>Dictionary集合</returns>
        public static IEnumerable<Dictionary<string, object>> ToDictionaries(this IDataReader @this)
        {
            if (@this?.IsClosed == false)
            {
                using (@this)
                {
                    while (@this.Read())
                    {
                        var dic = new Dictionary<string, object>();
                        for (var i = 0; i < @this.FieldCount; i++)
                        {
                            dic[@this.GetName(i)] = @this.GetValue(i);
                        }
                        yield return dic;
                    }
                }
            }
        }
        #endregion

        #region ToEntity
        /// <summary>
        /// IDataReader数据转为强类型实体
        /// </summary>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>强类型实体</returns>
        public static T ToEntity<T>(this IDataReader @this)
        {
            var result = @this.ToEntities<T>();
            if (result != null)
            {
                return result.FirstOrDefault();
            }
            return default(T);
        }

        /// <summary>
        /// IDataReader数据转为强类型实体集合
        /// </summary>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>强类型实体集合</returns>
        public static IEnumerable<T> ToEntities<T>(this IDataReader @this)
        {
            if (@this?.IsClosed == false)
            {
                using (@this)
                {
                    var fields = new List<string>();
                    for (int i = 0; i < @this.FieldCount; i++)
                    {
                        fields.Add(@this.GetName(i));
                    }
                    while (@this.Read())
                    {
                        var instance = Activator.CreateInstance<T>();
                        var props = instance.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);
                        foreach (var p in props)
                        {
                            if (!p.CanWrite) continue;
                            var field = fields.Where(o => o.ToLower() == p.Name.ToLower()).FirstOrDefault();
                            if (!field.IsNullOrEmpty() && !@this[field].IsNull())
                            {
                                p.SetValue(instance, @this[field].ToSafeValue(p.PropertyType), null);
                            }
                        }
                        yield return instance;
                    }
                }
            }
        }
        #endregion

        #region ToList
        /// <summary>
        /// IDataReader转换为T集合
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>T类型集合</returns>
        public static List<T> ToList<T>(this IDataReader @this)
        {
            List<T> list = null;
            if (@this?.IsClosed == false)
            {
                var type = typeof(T);
                if (type.Name.Contains("IDictionary`2"))
                {
                    list = @this.ToDictionaries()?.Select(o => o as IDictionary<string, object>).ToList() as List<T>;
                }
                else if (type.Name.Contains("Dictionary`2"))
                {
                    list = @this.ToDictionaries()?.ToList() as List<T>;
                }
                else if (type.IsClass && type.Name != "Object" && type.Name != "String")
                {
                    list = @this.ToEntities<T>()?.ToList() as List<T>;
                }
                else
                {
                    var result = @this.ToDynamics()?.ToList();
                    list = result as List<T>;
                    if (list == null)
                    {
                        //适合查询单个字段的结果集
                        list = result.Select(o => (T)(o as IDictionary<string, object>)?.Select(x => x.Value).FirstOrDefault()).ToList();
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// IDataReader转换为T集合的集合
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>T类型集合的集合</returns>
        public static List<List<T>> ToLists<T>(this IDataReader @this)
        {
            var result = new List<List<T>>();
            if (@this?.IsClosed == false)
            {
                using (@this)
                {
                    var type = typeof(T);
                    do
                    {
                        #region IDictionary
                        if (type.Name.Contains("Dictionary`2"))
                        {
                            var list = new List<Dictionary<string, object>>();
                            while (@this.Read())
                            {
                                var dic = new Dictionary<string, object>();
                                for (var i = 0; i < @this.FieldCount; i++)
                                {
                                    dic[@this.GetName(i)] = @this.GetValue(i);
                                }
                                list.Add(dic);
                            }
                            if (type.Name.Contains("IDictionary`2"))
                            {
                                result.Add(list.Select(o => o as IDictionary<string, object>).ToList() as List<T>);
                            }
                            else
                            {
                                result.Add(list as List<T>);
                            }
                        }
                        #endregion

                        #region Class T
                        else if (type.IsClass && type.Name != "Object" && type.Name != "String")
                        {
                            var list = new List<T>();
                            var fields = new List<string>();
                            for (int i = 0; i < @this.FieldCount; i++)
                            {
                                fields.Add(@this.GetName(i));
                            }
                            while (@this.Read())
                            {
                                var instance = Activator.CreateInstance<T>();
                                var props = instance.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);
                                foreach (var p in props)
                                {
                                    if (!p.CanWrite) continue;
                                    var field = fields.Where(o => o.ToLower() == p.Name.ToLower()).FirstOrDefault();
                                    if (!field.IsNullOrEmpty() && !@this[field].IsNull())
                                    {
                                        p.SetValue(instance, @this[field].ToSafeValue(p.PropertyType), null);
                                    }
                                }
                                list.Add(instance);
                            }
                            result.Add(list);
                        }
                        #endregion

                        #region dynamic
                        else
                        {
                            var list = new List<dynamic>();
                            while (@this.Read())
                            {
                                var row = new ExpandoObject() as IDictionary<string, object>;
                                for (var i = 0; i < @this.FieldCount; i++)
                                {
                                    row.Add(@this.GetName(i), @this.GetValue(i));
                                }
                                list.Add(row);
                            }
                            var item = list as List<T>;
                            if (item == null)
                            {
                                //适合查询单个字段的结果集
                                item = list.Select(o => (T)(o as IDictionary<string, object>)?.Select(x => x.Value).FirstOrDefault()).ToList();
                            }
                            result.Add(item);
                        }
                        #endregion
                    } while (@this.NextResult());
                }
            }
            return result;
        }
        #endregion

        #region ToFirstOrDefault
        /// <summary>
        /// IDataReader转换为T类型对象
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>T类型对象</returns>
        public static T ToFirstOrDefault<T>(this IDataReader @this)
        {
            var list = @this.ToList<T>();
            if (list != null)
            {
                return list.FirstOrDefault();
            }
            return default(T);
        }
        #endregion

        #region SqlInject
        /// <summary>
        /// 判断是否sql注入
        /// </summary>
        /// <param name="this">sql字符串</param>
        /// <param name="pattern">正则表达式</param>
        /// <returns></returns>
        public static bool IsSqlInject(this string @this, string pattern = @"(?:')|(?:--)|(/\*(?:.|[\n\r])*?\*/)|(\b(select|update|union|and|or|delete|insert|trancate|char|into|substr|ascii|declare|exec|count|master|into|drop|execute)\b)")
        {
            if (@this.IsNullOrEmpty())
                return false;
            return Regex.IsMatch(@this, pattern, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 正则表达式替换sql
        /// </summary>
        /// <param name="this">sql字符串</param>
        /// <param name="pattern">正则表达式</param>
        /// <returns></returns>
        public static string ReplaceSqlWithRegex(this string @this, string pattern = @"(?:')|(?:--)|(/\*(?:.|[\n\r])*?\*/)|(\b(select|update|union|and|or|delete|insert|trancate|char|into|substr|ascii|declare|exec|count|master|into|drop|execute)\b)")
        {
            if (@this.IsNullOrEmpty())
                return @this;
            return Regex.Replace(@this, pattern, "");
        }
        #endregion

        #region Contains
        /// <summary>
        /// 正则判断是否包含目标字符串
        /// </summary>
        /// <param name="this">源字符串</param>
        /// <param name="value">目标字符串，例如：判断是否包含ASC或DESC为@"(/\*(?:|)*?\*/)|(\b(ASC|DESC)\b)"</param>
        /// <param name="options">匹配模式</param>
        /// <returns></returns>
        public static bool Contains(this string @this, string value, RegexOptions options)
        {
            return Regex.IsMatch(@this, value, options);
        }
        #endregion

        #region Attribute
        /// <summary>
        /// 获取首个指定特性
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(this MemberInfo @this) where T : Attribute
        {
            return @this.GetFirstOrDefaultAttribute<T>() as T;
        }

        /// <summary>
        /// 获取指定特性集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static object[] GetAttributes<T>(this MemberInfo @this) where T : Attribute
        {
            return @this?.GetCustomAttributes(typeof(T), false);
        }

        /// <summary>
        /// 获取首个指定特性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static object GetFirstOrDefaultAttribute<T>(this MemberInfo @this) where T : Attribute
        {
            return @this.GetAttributes<T>()?.FirstOrDefault();
        }

        /// <summary>
        /// 是否包含指定特性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool ContainsAttribute<T>(this MemberInfo @this) where T : Attribute
        {
            return @this.GetAttributes<T>()?.Length > 0;
        }
        #endregion
    }

    ///// <summary>
    ///// Oracle的DynamicParameters实现，用于支持Oracle游标类型
    ///// </summary>
    //public class OracleDynamicParameters : SqlMapper.IDynamicParameters
    //{
    //    private readonly DynamicParameters dynamicParameters = new DynamicParameters();

    //    private readonly List<OracleParameter> oracleParameters = new List<OracleParameter>();

    //    /// <summary>
    //    /// Add
    //    /// </summary>
    //    /// <param name="name"></param>
    //    /// <param name="oracleDbType"></param>
    //    /// <param name="direction"></param>
    //    public void Add(string name, OracleDbType oracleDbType, ParameterDirection direction)
    //    {
    //        var oracleParameter = new OracleParameter(name, oracleDbType, direction);
    //        oracleParameters.Add(oracleParameter);
    //    }

    //    /// <summary>
    //    /// Add
    //    /// </summary>
    //    /// <param name="name"></param>
    //    /// <param name="oracleDbType"></param>
    //    /// <param name="direction"></param>
    //    /// <param name="value"></param>
    //    /// <param name="size"></param>
    //    public void Add(string name, OracleDbType oracleDbType, ParameterDirection direction, object value = null, int? size = null)
    //    {
    //        OracleParameter oracleParameter;
    //        if (size.HasValue)
    //            oracleParameter = new OracleParameter(name, oracleDbType, size.Value, value, direction);
    //        else
    //            oracleParameter = new OracleParameter(name, oracleDbType, value, direction);
    //        oracleParameters.Add(oracleParameter);
    //    }

    //    /// <summary>
    //    /// AddParameters
    //    /// </summary>
    //    /// <param name="command"></param>
    //    /// <param name="identity"></param>
    //    public void AddParameters(IDbCommand command, SqlMapper.Identity identity)
    //    {
    //        ((SqlMapper.IDynamicParameters)dynamicParameters).AddParameters(command, identity);
    //        if (command is OracleCommand oracleCommand)
    //            oracleCommand.Parameters.AddRange(oracleParameters.ToArray());
    //    }
    //}
    #endregion

    #region SqlBuilderProvider
    /// <summary>
    /// SqlBuilderProvider
    /// </summary>
    internal class SqlBuilderProvider
    {
        #region Private Static Methods
        /// <summary>
        /// GetExpressionResolve
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns>ISqlBuilder</returns>
        private static ISqlBuilder GetExpressionResolve(Expression expression)
        {
            #region Implementation Of Expression
            //null
            if (expression == null)
            {
                throw new ArgumentNullException("expression", "不能为null");
            }
            //表示具有常数值的表达式
            else if (expression is ConstantExpression)
            {
                return new ConstantExpressionResolve();
            }
            //表示具有二进制运算符的表达式
            else if (expression is BinaryExpression)
            {
                return new BinaryExpressionResolve();
            }
            //表示访问字段或属性
            else if (expression is MemberExpression)
            {
                return new MemberExpressionResolve();
            }
            //表示对静态方法或实例方法的调用
            else if (expression is MethodCallExpression)
            {
                return new MethodCallExpressionResolve();
            }
            //表示创建一个新数组，并可能初始化该新数组的元素
            else if (expression is NewArrayExpression)
            {
                return new NewArrayExpressionResolve();
            }
            //表示一个构造函数调用
            else if (expression is NewExpression)
            {
                return new NewExpressionResolve();
            }
            //表示具有一元运算符的表达式
            else if (expression is UnaryExpression)
            {
                return new UnaryExpressionResolve();
            }
            //表示调用构造函数并初始化新对象的一个或多个成员
            else if (expression is MemberInitExpression)
            {
                return new MemberInitExpressionResolve();
            }
            //表示包含集合初始值设定项的构造函数调用
            else if (expression is ListInitExpression)
            {
                return new ListInitExpressionResolve();
            }
            //表示将委托或lambda表达式应用于参数表达式列表的表达式
            else if (expression is InvocationExpression)
            {
                return new InvocationExpressionResolve();
            }
            //描述一个lambda表达式
            else if (expression is LambdaExpression)
            {
                return new LambdaExpressionResolve();
            }
            //表示命名参数表达式
            else if (expression is ParameterExpression)
            {
                return new ParameterExpressionResolve();
            }
            #endregion

            #region Unimplementation Of Expression
            else if (expression is BlockExpression)
            {
                throw new NotImplementedException("未实现的BlockExpression2Sql");
            }
            else if (expression is ConditionalExpression)
            {
                throw new NotImplementedException("未实现的ConditionalExpression2Sql");
            }
            else if (expression is DebugInfoExpression)
            {
                throw new NotImplementedException("未实现的DebugInfoExpression2Sql");
            }
            else if (expression is DefaultExpression)
            {
                throw new NotImplementedException("未实现的DefaultExpression2Sql");
            }
            else if (expression is DynamicExpression)
            {
                throw new NotImplementedException("未实现的DynamicExpression2Sql");
            }
            else if (expression is GotoExpression)
            {
                throw new NotImplementedException("未实现的GotoExpression2Sql");
            }
            else if (expression is IndexExpression)
            {
                throw new NotImplementedException("未实现的IndexExpression2Sql");
            }
            else if (expression is LabelExpression)
            {
                throw new NotImplementedException("未实现的LabelExpression2Sql");
            }
            else if (expression is LoopExpression)
            {
                throw new NotImplementedException("未实现的LoopExpression2Sql");
            }
            else if (expression is RuntimeVariablesExpression)
            {
                throw new NotImplementedException("未实现的RuntimeVariablesExpression2Sql");
            }
            else if (expression is SwitchExpression)
            {
                throw new NotImplementedException("未实现的SwitchExpression2Sql");
            }
            else if (expression is TryExpression)
            {
                throw new NotImplementedException("未实现的TryExpression2Sql");
            }
            else if (expression is TypeBinaryExpression)
            {
                throw new NotImplementedException("未实现的TypeBinaryExpression2Sql");
            }
            throw new NotImplementedException("未实现的Expression2Sql");
            #endregion
        }
        #endregion

        #region Public Static Methods
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        public static void Update(Expression expression, SqlPack sqlPack) => GetExpressionResolve(expression).Update(expression, sqlPack);

        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        public static void Insert(Expression expression, SqlPack sqlPack) => GetExpressionResolve(expression).Insert(expression, sqlPack);

        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        internal static void Select(Expression expression, SqlPack sqlPack) => GetExpressionResolve(expression).Select(expression, sqlPack);

        /// <summary>
        /// Join
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        internal static void Join(Expression expression, SqlPack sqlPack) => GetExpressionResolve(expression).Join(expression, sqlPack);

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        internal static void Where(Expression expression, SqlPack sqlPack) => GetExpressionResolve(expression).Where(expression, sqlPack);

        /// <summary>
        /// In
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        internal static void In(Expression expression, SqlPack sqlPack) => GetExpressionResolve(expression).In(expression, sqlPack);

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        internal static void GroupBy(Expression expression, SqlPack sqlPack) => GetExpressionResolve(expression).GroupBy(expression, sqlPack);

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <param name="orders">排序方式</param>
        internal static void OrderBy(Expression expression, SqlPack sqlPack, params OrderType[] orders) => GetExpressionResolve(expression).OrderBy(expression, sqlPack, orders);

        /// <summary>
        /// Max
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        internal static void Max(Expression expression, SqlPack sqlPack) => GetExpressionResolve(expression).Max(expression, sqlPack);

        /// <summary>
        /// Min
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        internal static void Min(Expression expression, SqlPack sqlPack) => GetExpressionResolve(expression).Min(expression, sqlPack);

        /// <summary>
        /// Avg
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        internal static void Avg(Expression expression, SqlPack sqlPack) => GetExpressionResolve(expression).Avg(expression, sqlPack);

        /// <summary>
        /// Count
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        internal static void Count(Expression expression, SqlPack sqlPack) => GetExpressionResolve(expression).Count(expression, sqlPack);

        /// <summary>
        /// Sum
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        internal static void Sum(Expression expression, SqlPack sqlPack) => GetExpressionResolve(expression).Sum(expression, sqlPack);
        #endregion
    }
    #endregion
}

