using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Data;
using System.Threading;
using System.Threading.Tasks;


namespace Aden.Util.Common
{
    public static class DynamicToLINQ
    {
        public static IList GetList(this IQueryable source)
        {
            if (source == null) return null;
            Type eleType = source.ElementType;
            Type listType = typeof(List<>).MakeGenericType(eleType);
            IList list = Activator.CreateInstance(listType) as IList;

            foreach (var qry in source)
            {
                list.Add(qry);
            }
            return list;
        }

        public static DataTable GroupBy(this DataTable data, string groupbyfields, string sumfields)
        {
            groupbyfields = groupbyfields.ToStringTrim();
            string tmpgroupbyfields = "1";
            string selectfields = "";
            LambdaExpression[] exprgroupby = null;

            if (groupbyfields != "")
            {
                List<string> lfields = groupbyfields.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                exprgroupby = data.Columns.ToExpressions(lfields, out tmpgroupbyfields);
                tmpgroupbyfields = string.Format("new ({0})", tmpgroupbyfields);
                selectfields = string.Join(",", lfields.Select(q => string.Format("Key.{0}", q)).ToArray());
            }

            IQueryable qry = data.AsEnumerable().AsParallel().AsQueryable().GroupBy(tmpgroupbyfields, "it", exprgroupby);

            if (sumfields.ToStringTrim() == "") return qry.Select("new (" + selectfields + ")").ConvertToDataTable();

            string tmpsumfields = "";
            LambdaExpression[] exprsum = data.Columns.ToExpressions(sumfields.ToStringTrim().Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).ToList(), out tmpsumfields);
            if (selectfields != "") selectfields += ",";
            selectfields += tmpsumfields;
            selectfields = string.Format("new ({0})", selectfields);

            qry = qry.Select(selectfields, exprsum);

            return qry.ConvertToDataTable();
        }

        public static IList ToList(this IList target, IQueryable source)
        {
            if (source == null || !source.Any()) return target;

            Type type = target[0].GetType();
            foreach (var qry in source)
            {
                try
                {
                    var item = Activator.CreateInstance(type);

                    foreach (PropertyInfo prop in source.ElementType.GetProperties())
                    {
                        object v = prop.GetValue(qry, null);
                        if (prop.DeclaringType.IsNumber())
                        {
                            if (v == null) v = 0;
                            else v = Math.Round(v.ToDouble(), 4);
                        }
                        type.GetProperty(prop.Name).SetValue(item, v, null);
                    };

                    target.Add(item);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            return target;
        }

        public static DataTable ConvertToDataTable(this IQueryable source)
        {
            return source.ConvertToDataTable("", false);
        }

        public static DataTable ConvertToDataTable(this IQueryable source, string tableName, bool needIndex)
        {
            if (source == null || !source.Any()) return null;
            if (tableName.ToStringTrim() == "") tableName = "table";
            DataTable dt = new DataTable(tableName);
            if (needIndex)
            {
                dt.Columns.Add("index", typeof(int));
                dt.PrimaryKey = new DataColumn[] { dt.Columns["index"] };
            }

            List<MemberInfo> listF = new List<MemberInfo>();
            foreach (MemberInfo field in source.ElementType.GetMembers())
            {
                if (field.MemberType != MemberTypes.Property && field.MemberType != MemberTypes.Field) continue;
                listF.Add(field);
                Type type = field.MemberType == MemberTypes.Field ? ((FieldInfo)field).FieldType : ((PropertyInfo)field).PropertyType;
                type = Nullable.GetUnderlyingType(type) ?? type;
                if (type == typeof(double)) type = typeof(decimal);
                dt.Columns.Add(field.Name, type);
            };

            dt.BeginLoadData();
            int index = 0;

            foreach (var item in source)
            {
                DataRow row = dt.NewRow();
                if (needIndex) row["index"] = index++;

                foreach (MemberInfo field in listF)
                {
                    object v = field.MemberType == MemberTypes.Field ? ((FieldInfo)field).GetValue(item) : ((PropertyInfo)field).GetValue(item, null);
                    if (dt.Columns[field.Name].DataType.IsNumber())
                        v = Math.Round(v.ToDouble(), 6);

                    row[field.Name] = v;
                }
                dt.Rows.Add(row);


                /*
				ArrayList arr = new ArrayList();
				if (needIndex) arr.Add(index++);

				foreach (MemberInfo field in listF)
				{
					object v = field.MemberType == MemberTypes.Field ? ((FieldInfo)field).GetValue(item) : ((PropertyInfo)field).GetValue(item, null);
					if (dt.Columns[field.Name].DataType.IsNumber())
						v = Math.Round(v.ToDouble(), 6);

					arr.Add(v);
				}
				dt.Rows.Add(arr);
				*/
            }

            dt.EndLoadData();

            return dt;
        }



        /// <param name="TotalFields">FieldName, 合并字段(相同累计，例如Account,Warehouse);排序字段(按顺序累计，例如period)     </param>
        public static DataTable ConvertToDataTable(this IQueryable source, Dictionary<string, string> TotalFields)
        {
            System.Data.DataTable dt = source.ConvertToDataTable();

            //无合计字段，无公式字段
            if (TotalFields == null || !TotalFields.Any()) return dt;

            var qryOrderbys = TotalFields.Where(q => q.Value.ToStringTrim().Contains(";") && q.Key.IsAggregate()).GroupBy(q => q.Value.Trim().Replace(" ", "")).Select(q => q.Key); //含有合并和排序字段
                                                                                                                                                                             //数据集只有一条记录，只有公式字段
            if (dt.Rows.Count == 1 || !qryOrderbys.Any()) return dt.SetColumnExpression(TotalFields.Where(q => !q.Value.IsAggregate()).ToDictionary(q => q.Key, q => q.Value));


            #region 合计字段

            foreach (string key in TotalFields.Where(q => q.Key.IsAggregate()).Select(q => q.Key))
            {
                string field = key.SplitByAs("Field");

                if (dt.Columns.Contains(field)) continue;
                dt.Columns.Add(field.ToLower(), typeof(double));
            }
            #endregion

            //复制有期末没有发生额
            DataTable dtSum = dt.Clone();
            IQueryable qry = dt.AsEnumerable().AsQueryable();
            dt.BeginLoadData();
            dtSum.BeginLoadData();
            foreach (string sOrderby in qryOrderbys)
            {
                string groupbyfields = "";
                LambdaExpression[] exprgroupby = dt.Columns.ToExpressions(sOrderby.SplitBy(";", 0).Split(',').ToList(), out groupbyfields);
                if (groupbyfields == "") groupbyfields = "1";
                else groupbyfields = string.Format("new ({0})", groupbyfields);
                if (!qry.GroupBy(groupbyfields, "it", exprgroupby).Where("it.Count()>1").Any()) continue; //每一组只有一条记录

                string tmporderby = sOrderby;
                List<string> diffFields = tmporderby.Split(new char[] { ';' })[1].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                string[] sameFields = tmporderby.Split(new char[] { ';' })[0].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                string[] sumFields = TotalFields.Where(q => q.Value == sOrderby).Select(q => q.Key.SplitByAs("Field")).ToArray();

                string sdiff = "";
                string sdiffwhere = "";
                int x = 0;
                foreach (string s in diffFields)
                {
                    sdiff += string.Format(",{0}", s);
                    sdiffwhere += string.Format(" and trim(isnull({0},''))='{1}'", s, "{" + (x++) + "}");
                }

                if (sdiff.StartsWith(",")) sdiff = sdiff.Substring(1);
                if (sdiffwhere.StartsWith(" and ")) sdiffwhere = sdiffwhere.Substring(5);
                DataTable dtDiff = source.GroupBy(string.Format("new ({0})", sdiff), "it").Select(string.Format("new (Key.{0})", sdiff.Replace(",", "Key."))).OrderBy(sdiff).ConvertToDataTable();

                if (tmporderby.StartsWith(";")) tmporderby = sOrderby.Substring(1);

                DataRow[] drs = dt.Select("", tmporderby.Replace(';', ','));

                for (int i = 1; i < drs.Length; i++)
                {
                    bool sameLast = !sameFields.Where(q => drs[i][q].ToStringTrim() != drs[i - 1][q].ToStringTrim()).Any(); //与上一条记录相同

                    List<object> listVals0 = new List<object>();
                    List<object> listVals1 = new List<object>();
                    foreach (string s in diffFields)
                    {
                        listVals0.Add(drs[i - 1][s]);
                        listVals1.Add(drs[i][s]);
                    }
                    int diffrow0 = dtDiff.Rows.IndexOf(dtDiff.Select(string.Format(sdiffwhere, listVals0.ToArray()))[0]);
                    int diffrow1 = dtDiff.Rows.Count;
                    if (sameLast) diffrow1 = dtDiff.Rows.IndexOf(dtDiff.Select(string.Format(sdiffwhere, listVals1.ToArray()))[0]);

                    while (++diffrow0 < diffrow1)
                    {
                        DataRow dr = dtSum.NewRow();
                        // bool allzero = true;
                        foreach (string f in sumFields)
                        {
                            dr[f] = drs[i - 1][f];
                            //   if (allzero && dr[f].ToInt() != 0) allzero = false;
                        }
                        //  if (allzero) continue;

                        foreach (string f in sameFields) dr[f] = drs[i - 1][f];
                        foreach (string f in diffFields) dr[f] = dtDiff.Rows[diffrow0][f];
                        dtSum.Rows.Add(dr);
                    }

                    if (!sameLast) continue;

                    foreach (string f in sumFields)
                        drs[i][f] = drs[i - 1][f].ToDouble() + drs[i][f].ToDouble();
                }
            }
            dtSum.EndLoadData();
            if (dtSum.AsEnumerable().Any()) dt.Merge(dtSum);
            dt.EndLoadData();
            dt = dt.SetColumnExpression(TotalFields.Where(q => !q.Value.IsAggregate()).ToDictionary(q => q.Key, q => q.Value));
            return dt;
        }



        private static DataTable SetColumnExpression(this DataTable data, Dictionary<string, string> FormulaFields)
        {
            foreach (string key in FormulaFields.Keys)
            {
                string field = key.SplitByAs("Field");
                if (data.Columns.Contains(field)) continue;

                string formula = key.SplitByAs("Formula");
                data.Columns.Add(field.ToLower(), typeof(double), formula);
            }
            return data;
        }

        public static LambdaExpression ToExpression(this ParameterExpression datarow, DataColumn column)
        {
            try
            {
                MethodInfo mi = typeof(DataRowExtensions).GetMethod("Field", new Type[] { typeof(DataRow), typeof(string) });
                if (column.DataType.IsValueType) mi = mi.MakeGenericMethod(Type.GetType("System.Nullable`1[" + column.DataType.FullName + "]"));//typeof(int?));
                else mi = mi.MakeGenericMethod(column.DataType);
                ConstantExpression constant = Expression.Constant(column.ColumnName, typeof(string));

                return Expression.Lambda(Expression.Call(mi, datarow, constant), datarow);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static List<LambdaExpression> ToExpressions(this List<DataColumn> columns)
        {
            if (columns == null || columns.Count == 0) return null;

            ParameterExpression datarow = Expression.Parameter(typeof(DataRow), "datarow");
            List<LambdaExpression> retVal = new List<LambdaExpression>();

            foreach (DataColumn column in columns)
            {
                retVal.Add(datarow.ToExpression(column));
            }
            return retVal;
        }

        public static LambdaExpression[] ToExpressions(this DataColumnCollection columns)
        {
            if (columns == null || columns.Count == 0) return null;

            ParameterExpression datarow = Expression.Parameter(typeof(DataRow), "datarow");
            List<LambdaExpression> retVal = new List<LambdaExpression>();

            foreach (DataColumn column in columns)
            {
                retVal.Add(datarow.ToExpression(column));
            }
            return retVal.ToArray();
        }

        public static LambdaExpression[] ToExpressions(this DataColumnCollection columns, List<string> fields)
        {
            string s = "";
            return columns.ToExpressions(fields, out s);
        }

        public static LambdaExpression[] ToExpressions(this DataColumnCollection columns, List<string> fields, out string outFields)
        {
            outFields = "";

            if (fields == null || fields.Count == 0) return null;
            fields = fields.ToList();
            List<DataColumn> listColumns = new List<DataColumn>();
            int index = -1;

            DataColumn[] dcs = new DataColumn[columns.Count];
            columns.CopyTo(dcs, 0);

            foreach (DataColumn column in dcs.OrderByDescending(q => q.ColumnName.Length))
            {
                if (!fields.Where(q => q.SplitByAs("Formula").ToLower().Contains(column.ColumnName.ToLower())).Any()) continue;

                for (int i = 0; i < fields.Count; i++)
                {
                    string field = string.Format(" as {0}", fields[i].SplitByAs("Field"));
                    string formula = fields[i].SplitByAs("Formula");
                    int pos = -1;
                    while (++pos < formula.Length)
                    {
                        pos = formula.ToLower().IndexOf(column.ColumnName.ToLower(), pos);
                        if (pos == -1) break;

                        string tmps = "";
                        if (pos > 0) tmps = formula[pos - 1].ToStringTrim();
                        tmps += column.ColumnName;
                        if ((pos + column.ColumnName.Length) < (formula.Length)) tmps += formula[pos + column.ColumnName.Length].ToStringTrim();

                        //该字段是其它字段名的部分
                        if (tmps != column.ColumnName && columns.Contains(tmps)) continue;

                        if (!listColumns.Contains(column))
                        {
                            listColumns.Add(column);
                            index++;
                        }

                        formula = formula.Substring(0, pos) + string.Format("@{0}(it)", index) + formula.Substring(pos + column.ColumnName.Length);
                    }

                    fields[i] = formula + field;
                }
            }

            outFields = "";
            foreach (string f in fields)
                outFields += string.Format(",{0}", f);

            if (outFields.StartsWith(",")) outFields = outFields.Substring(1);

            return listColumns.ToExpressions().ToArray();
        }


        public static List<string> GetColumns(this DataColumnCollection columns, string formula)
        {
            if (columns == null || columns.Count == 0 || formula.ToStringTrim() == "") return null;

            formula = formula.ToLower();
            List<string> retVal = new List<string>();
            DataColumn[] dcs = new DataColumn[columns.Count];
            columns.CopyTo(dcs, 0);
            foreach (DataColumn col in dcs.OrderByDescending(q => q.ColumnName.Length))
            {
                if (!formula.Contains(col.ColumnName.ToLower())) continue;

                formula = formula.Replace(col.ColumnName.ToLower(), "");
                retVal.Add(col.ColumnName);
            }

            if (retVal.Count == 0) return null;
            return retVal;
        }


        public static string GetFieldExpr(string type, List<string> KeyFields, params string[] others)
        {
            string tmp = "";
            string format = "";
            if (type == "GroupbySelect") tmp = "Key.";
            if (type == "Groupby" || type == "GroupbySelect" || type == "Select") format = "new ({0})";

            string retVal = "";
            if (KeyFields != null)
                foreach (string s in KeyFields)
                    retVal += string.Format(",{0}{1}", tmp, s);

            if (others != null)
                for (int i = 0; i < others.Length; i++) retVal += string.Format(",{0}", others[i]);
            if (retVal.StartsWith(",")) retVal = retVal.Substring(1);

            if (format.ToStringTrim() == "") return retVal;
            retVal = string.Format(format, retVal);
            return retVal;
        }
    }
}
