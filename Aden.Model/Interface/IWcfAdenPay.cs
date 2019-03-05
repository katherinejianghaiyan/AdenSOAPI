using System.Collections.Generic;
using System.ServiceModel;
using System.Data;

namespace Aden.Model.Interface
{
    [ServiceContract(Namespace = "AdenServices.Model.Interface", Name = "IWcfAdenPay")]
    public interface IWcfAdenPay
    {
        /// <summary>
        /// 客户端发起, 获取服务端已支付未同步过的记录
        /// </summary>
        /// <param name="timeStamp">时间戳,用于加密和解密</param>
        /// <param name="identity">身份验证</param>
        /// <param name="clientGuid">客户端Guid</param>
        /// <returns>服务端返回加密后的PaiedRecord清单序列化对象字符串</returns>
        [OperationContract]
        string GetPaiedRecordListFromServer(string timeStamp, string identity, string clientGuid);

        /// <summary>
        /// 客户端发起, 发送处理后的结果到服务端,服务端后续处理结果
        /// </summary>
        /// <param name="timeStamp">时间戳,用于加密和解密</param>
        /// <param name="identity">身份验证</param>
        /// <param name="data">客户端处理后的结果SyncResult清单序列化字符串</param>
        [OperationContract]
        bool SendResultToServer(string timeStamp, string identity, string data);

        /// <summary>
        /// 服务端发起,请求同步数据至客户端,同时接收结果
        /// </summary>
        /// <param name="timeStamp">时间戳,用于加密和解密</param>
        /// <param name="identity">身份验证</param>
        /// <param name="data">加密后的PaiedRecord清单序列化对象字符串</param>
        /// <returns>客户端处理后的结果SyncResult清单序列化字符串</returns>
        [OperationContract]
        string SyncPaiedRecordToClient(string timeStamp, string identity, string data, string clientGuid);

        /// <summary>
        /// 服务端发起, 发送服务端注册用户信息到客户端
        /// </summary>
        /// <param name="timeStamp">时间戳,用于加密和解密</param>
        /// <param name="identity">身份验证</param>
        /// <param name="data">加密后的用户对象序列化字符串(单一)</param>
        /// <returns>字符串,表示错误信息或者服务端已经处理</returns>
        [OperationContract]
        string GetCardIdsFromServer(string timeStamp, string identity, string clientGuid);

        /// <summary>
        /// 客户端发起,推送用户清单数据到服务端
        /// </summary>
        /// <param name="timeStamp">时间戳,用于加密和解密</param>
        /// <param name="identity">身份验证</param>
        /// <param name="data">加密后的用户清单对象序列化字符串(List)</param>
        [OperationContract]
        bool SyncPosUserMastToServer(string timeStamp, string identity, string data, string clientGuid);

        /// <summary>
        /// 客户端请求获取需要传送的数据集配置信息
        /// </summary>
        /// <param name="timeStamp">时间戳,用于加密和解密</param>
        /// <param name="identity">身份验证</param>
        /// <param name="clientGuid"></param>
        /// <returns>加密后的用户清单对象序列化字符串(List)</returns>
        [OperationContract]
        string GetPosClientDataConfig(string timeStamp, string identity, string clientGuid);

        /// <summary>
        /// 发送数据集
        /// </summary>
        /// <param name="timeStamp">时间戳,用于加密和解密</param>
        /// <param name="identity">身份验证</param>
        /// <param name="ds">数据集</param>
        /// <returns></returns>
        [OperationContract]
        bool SendPosDataSet(string timeStamp, string identity, string clientGuid, DataSet ds);

        /// <summary>
        /// 客户端获取记录的最大日期
        /// </summary>
        /// <param name="timeStamp">时间戳,用于加密和解密</param>
        /// <param name="identity">身份验证</param>
        /// <param name="actionType">记录集类型</param>
        /// <param name="clientGuid"></param>
        /// <returns>加密日期字符串</returns>
        [OperationContract]
        string GetRecordMaxDate(string timeStamp, string identity, string actionType, string clientGuid);

        /// <summary>
        /// 发送充值消费记录数据
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <param name="identity"></param>
        /// <param name="date"></param>
        /// <param name="clientGuid"></param>
        /// <returns></returns>
        [OperationContract]
        bool SendPosRecords(string timeStamp, string identity, string date, string clientGuid);

        #region 主动调用

        /// <summary>
        /// 获取Pos端数据库数据集
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="tblSqls"></param>
        /// <returns></returns>
        [OperationContract]
        DataSet GetDataSetFromClient(string dbName, Dictionary<string, string> tblSqls);

        /// <summary>
        /// 执行SQL,Pos端数据库数据集
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="tblSqls"></param>
        /// <returns></returns>
        [OperationContract]
        int ExecSQLFromClient(string dbName, string sql);

        /// <summary>
        /// 获取卡余额数据
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <param name="identity"></param>
        /// <param name="cardNumber"></param>
        /// <returns></returns>
        [OperationContract]
        string GetPosCardCurrentAmt(string timeStamp, string identity, string cardNumber);

        #endregion
    }
}
