using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aden.Model;
using Aden.Model.MastData;
using Aden.Model.WeChat;
using Aden.Model.Common;
using Aden.DAL.WeChatPay;

namespace Aden.BLL.WeChatPay
{
    public class RechargeHelper
    {
        /// <summary>
        /// 更新用户
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static int ModifyWxUser(WxUserMast param)
        {
            RechargeFactory factory = new RechargeFactory();
            try
            {
                if (param == null) throw new Exception("Param is null");
                int result = factory.ModifyWxUser(param);
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetCostCenterCode");
                return -1;
            }
        }

        /// <summary>
        /// 根据卡号取得卡信息
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static CCMast GetCostCenterCode(WxUserMast param)
        {
            RechargeFactory factory = new RechargeFactory();
            try
            {
                if (param == null) throw new Exception("Param is null");
                CCMast ccm = factory.GetCostCenterCode(param);
                if (ccm == null) throw new Exception("DAL.WeChat.RechargeFactory.GetCostCenterCode()==null");
                return ccm;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetCostCenterCode");
                return null;
            }
        }

        /// <summary>
        /// 根据卡号取得卡信息
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static CardInfo GetCardInfo(CardInfoParam param)
        {
            RechargeFactory factory = new RechargeFactory();
            try
            {
                if (param == null) throw new Exception("Param is null");
                CardInfo card = factory.GetCardInfo(param);
                if (card == null) throw new Exception("DAL.WeChat.RechargeFactory.GetCardInfo()==null");
                return card;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetCardInfo");
                return null;
            }
        }

        /// <summary>
        /// 根据卡号取得卡信息
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static UserBalance GetBalance(CardInfoParam param)
        {
            RechargeFactory factory = new RechargeFactory();
            try
            {
                if (param == null) throw new Exception("Param is null");
                UserBalance userBalance = factory.GetBalance(param);
                if (userBalance == null) throw new Exception("DAL.WeChat.RechargeFactory.GetBalance()==null");
                return userBalance;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetBalance");
                throw ex;
            }
        }

        /// <summary>
        /// 取得最后一笔充值成功记录
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static UserBalance GetLastRecharge(CardInfoParam param)
        {
            RechargeFactory factory = new RechargeFactory();
            try
            {
                if (param == null) throw new Exception("Param is null");
                UserBalance userBalance = new UserBalance();
                WxOrder wxOrder = factory.GetLastRecharge(param);
                //if (wxOrder == null) throw new Exception("DAL.WeChat.RechargeFactory.GetLastRecharge()==null");
                userBalance.lastOrder = wxOrder;
                return userBalance;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetLastRecharge");
                throw ex;
            }
        }

        /// <summary>
        /// 取得交易记录
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static List<TransLog> GetTransLog(TransLogParam param)
        {
            RechargeFactory factory = new RechargeFactory();
            try
            {
                if (param == null) throw new Exception("Param is null");
                List<TransLog> lstTLog = factory.GetTransLog(param);
                if (lstTLog == null || lstTLog.Count == 0) throw new Exception("DAL.WeChat.RechargeFactory.GetTransLog()==null");
                return lstTLog;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetTransLog");
                return null;
            }
        }

        /// <summary>
        /// 根据WechatId取得卡信息
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static List<WxUserCard> GetCardList(WxUserMast param)
        {
            RechargeFactory factory = new RechargeFactory();
            try
            {
                if (param == null) throw new Exception("Param is null");
                List<WxUserCard> lstCardList = factory.GetCardList(param);
                if (lstCardList == null || lstCardList.Count == 0) throw new Exception("DAL.WeChat.RechargeFactory.GetCardList()==null");
                return lstCardList;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetCardList");
                return null;
            }
        }

        /// <summary>
        /// 绑定/解绑指定卡
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static int BindCard(CardInfo param, string mode)
        {
            RechargeFactory factory = new RechargeFactory();
            try
            {
                if (param == null) throw new Exception("Param is null");
                int result = factory.BindCard(param, mode);
                if (result == 0) throw new Exception("DAL.WeChat.RechargeFactory.BindCard()==0");
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "BindCard");
                return 0;
            }
        }
    }
}
