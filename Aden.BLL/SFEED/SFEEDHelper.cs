using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aden.Model;
using Aden.Model.Common;
using Aden.Model.SOAccount;
using Aden.Model.MastData;
using Aden.Model.SOMastData;
using Aden.Model.MenuOrder;
using Aden.DAL.SOAccount;

using Aden.DAL.SFEED;

namespace Aden.BLL.SFEED
{
    public class SFEEDHelper
    {
        private readonly static SFEEDFactory SFEEDFactory = new SFEEDFactory();

        public static List<CCWhs> GetCC(AuthCompany param)
        {
            try
            {
                List<CCWhs> lstCCWhs = SFEEDFactory.GetCC(param);
                if (lstCCWhs == null) throw new Exception("DAL.MenuOrder.WeeklyMenuFactory.GetCCWhs()==null");
                return lstCCWhs;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "lstCCWhs");
                return null;
            }
        }
    }
}
