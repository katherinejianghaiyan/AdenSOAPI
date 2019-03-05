using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aden.Util.Database;
using Aden.Util.Common;
using Aden.Model.Common;
using Aden.Model.SOAccount;
using Aden.Model.MastData;
using Aden.Model.SOMastData;
using Aden.Model.MenuOrder;
using Aden.DAL.SOAccount;
using Aden.DAL.SalesOrder;
using System.Data;




namespace Aden.DAL.SFEED
{
    public class SFEEDFactory
    {

        public List<CCWhs> GetCC(AuthCompany param)
        {
            try
            {
                List<SingleField> lstCC = new List<SingleField>();

                UserAuthParam uParam = new UserAuthParam();
                string strSql = string.Empty;

                uParam.userGuid = param.userGuid;
                uParam.action = param.action;
                uParam.fieldName = "costcenter";
                uParam.recursion = false;
                lstCC = (new AccountFactory()).GetAuthList(uParam);

                string strCostCenterCode = string.Join("','", lstCC.Select(r => r.code).Distinct().ToArray());
                strCostCenterCode = "('" + strCostCenterCode + "')";


                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
