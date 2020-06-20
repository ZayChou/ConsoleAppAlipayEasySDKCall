using Alipay.EasySDK.Factory;
using Alipay.EasySDK.Kernel;
using Alipay.EasySDK.Payment.Common.Models;
using Alipay.EasySDK.Payment.FaceToFace.Models;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace AlipayClassLibrary
{
    public class AlipayClass
    {
        /// <summary>
        /// 应用ID
        /// </summary>
        public string AppId { get; set; } = "2016101900723805";
        /// <summary>
        /// 支付宝服务器地址
        /// </summary>
        public string GatewayHost { get; set; } = "openapi.alipaydev.com";
        /// <summary>
        /// 应用私钥相对执行目录地址
        /// </summary>
        public string AppPrivateKeyPath { get; set; } = "./应用私钥2048.txt";
        /// <summary>
        /// 支付宝公钥相对执行目录地址
        /// </summary>
        public string AlipayPublicKeyPath { get; set; } = "./支付宝公钥.txt";
        public bool ReadFile(string filePath , out string stream)
        {
            try
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.
                using StreamReader sr = new StreamReader(filePath);
                stream = sr.ReadToEnd();
                return true;
            }
            catch (Exception e)
            {
                // Let the user know what went wrong.
                //Console.WriteLine("The file could not be read:");
                //Console.WriteLine(e.Message)
                stream = e.Message;
                return false;
            }
        }
        /// <summary>
        /// 读取密钥文件
        /// </summary>
        /// <returns>success 成功 ，其它失败原因</returns>
        public string Init()
        {
            string appKey;
            if (!ReadFile(AppPrivateKeyPath, out appKey))
                return appKey;
            string alipayKey;
            if (!ReadFile(AlipayPublicKeyPath, out alipayKey))
                return alipayKey;

            Config config = new Config()
            {
                Protocol = "https",

                GatewayHost = GatewayHost,

                SignType = "RSA2",

                AppId = AppId,

                MerchantPrivateKey = appKey,

                AlipayPublicKey = alipayKey
            };
            Factory.SetOptions(config);

            return "success";
        }

        /// <summary>
        /// 二维码支付请求
        /// </summary>
        /// <param name="subject">订单标题</param>
        /// <param name="outTradeNo">设备订单号，不能重复，建议deviceID + time</param>
        /// <param name="totalAmount">订单金额</param>
        /// <param name="outPut">return true 函数调用成功，输出二维码字符串；rerun false 输出接口调用失败原因</param>
        /// <returns>true 成功 false 失败</returns>
        public bool PrecreateQRcode(string subject, string outTradeNo, string totalAmount, out string outPut)
        {
            try
            {
                // 1. 发起API调用
                AlipayTradePrecreateResponse response = Factory.Payment.FaceToFace().PreCreate(subject, outTradeNo, totalAmount);
                // 2. 处理响应或异常
                if ("10000".Equals(response.Code))
                {
                    // "交易预创建，请求二维码调用成功"
                    outPut = response.QrCode;
                    return true;
                }
                else
                {
                    outPut = response.Msg + "," + response.SubMsg;
                }
            }
            catch (Exception ex)
            {
                outPut = ex.Message;
            }
            return false;
        }

        /// <summary>
        /// 查询二维码支付结果
        /// </summary>
        /// <param name="outTradeNo">设备订单号</param>
        /// <param name="outPut">return true 函数调用成功，"TRADE_SUCCESS" 交易已支付，rerun false 输出接口调用失败原因</param>
        /// <returns></returns>
        public bool TradeQuery(string outTradeNo , out string outPut)
        {
            try
            {
                // 1. 发起API调用
                AlipayTradeQueryResponse response = Factory.Payment.Common().Query(outTradeNo);
                // 2. 处理响应或异常
                if ("10000".Equals(response.Code))
                {
                    //Console.WriteLine("调用成功");
                    outPut = response.TradeStatus;
                    return true;
                }
                else
                {
                    //Console.WriteLine("调用失败，原因：" + response.Msg + "，" + response.SubMsg);
                    outPut = response.Msg + "，" + response.SubMsg;
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine("调用遭遇异常，原因：" + ex.Message);
                outPut = ex.Message;
            }
            return false;
        }
  
    }
}
