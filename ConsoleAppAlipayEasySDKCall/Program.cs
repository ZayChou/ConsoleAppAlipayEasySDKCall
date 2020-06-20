using System;
using Alipay.EasySDK.Factory;
using Alipay.EasySDK.Kernel;
using Alipay.EasySDK.Payment.Common.Models;
using System.IO;
using Alipay.EasySDK.Payment.FaceToFace.Models;
using Alipay.EasySDK.Util.Generic.Models;
using System.Collections.Generic;
using NLog;
using RJCP.IO.Ports;
using Newtonsoft.Json.Linq;
using System.Threading;

using AlipayClassLibrary;


namespace ConsoleAppAlipayEasySDKCall
{
    class Program
    {
        // 应用私钥
        const string AppPrivateKeyPath = "C:\\Users\\Administrator\\Documents\\支付宝开放平台开发助手\\RSA密钥\\应用私钥2048.txt";
        // 正式环境公钥
        //const string AlipayPublicKeyPath = "C:\\Users\\Administrator\\Desktop\\二维码支付资料\\支付宝公钥.txt";
        // 沙箱公钥
        const string AlipayPublicKeyPath = "C:\\Users\\Administrator\\Desktop\\二维码支付资料\\沙箱密钥\\支付宝公钥.txt";

        // 正式环境APP ID
        //const string AppId = "2017032906468970";
        // 沙箱APP ID
        const string AppId = "2016101900723805";

        // 正式环境网关               
        // const string GatewayHost = "openapi.alipay.com";
        const string GatewayHost = "openapi.alipaydev.com";

        // 测试设备ID
        const string deviceId = "000014720007";

        // 商户授权资金订单号
        public static string auth_no;
        // 买家ID
        public static string buyer_Id;


        public delegate void DeleFunc();

        static void Main(string[] args)
        {

            AlipayClass alipay = new AlipayClass();

            string result = alipay.Init();
            Console.WriteLine(result);
            string sf;
            alipay.PrecreateQRcode("1", "333", "7", out sf);


            Console.ReadLine();

            // 1. 设置参数（全局只需设置一次）
            //Factory.SetOptions(GetConfig());

            //SerialPortStream serialPortStream = new SerialPortStream("COM3", 115200, 8, Parity.None, StopBits.One);

            //serialPortStream.Open();

            //serialPortStream.DataReceived += SerialPortStream_DataReceived;


            //PrecreateQRcode("自动售票机售票", deviceId + "_" + DateTime.Now.ToString(), "7");

            //while (true)
            //{ }

        }

        private static void SerialPortStream_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (sender is SerialPortStream)
            {
                var stream = sender as SerialPortStream;
                byte[] buff = new byte[stream.BytesToRead];
                stream.Read(buff, 0, stream.BytesToRead);

                string str = System.Text.Encoding.Default.GetString(buff, 0, buff.Length - 1);
                // FaceToFacePay("闸机进站消费", deviceId + "_" + DateTime.Now.ToString(), "7", str);
                //int callMethod = 1;

                string outTradeNo;
                outTradeNo = deviceId + "_" + DateTime.Now.ToString();


                FaceToFacePay("当面付支付", outTradeNo, "0.01", str);


                //TradeQuery(outTradeNo);

               Refund(outTradeNo, "0.01");

                //FundFreeze(str, outTradeNo, "闸机进站冻结", 7.00);

                //string outRequestNo = deviceId + "_" + DateTime.Now.ToString() + "_" + "Unfreeze";
                //FundUnfreeze(auth_no, outRequestNo, 7.00, "授权资金解冻");

                // 支付宝不支持调用
                //string senceNo = deviceId + "_" + DateTime.Now.ToString() + "_" + "DecodeUse";
                //DecodeUse(str, senceNo);

                //outTradeNo = deviceId + "_" + DateTime.Now.ToString() +  "pay";
                //FundFreezeToPay("闸机进站消费", outTradeNo, "2", auth_no, buyer_Id);


                //FaceToFacePay("闸机进站消费", deviceId + "_" + DateTime.Now.ToString(), "7", str);
            }
        }

        private static Config GetConfig()
        {
            return new Config()
            {
                Protocol = "https",

                GatewayHost = GatewayHost,

                SignType = "RSA2",

                AppId = AppId,

                // 为避免私钥随源码泄露，推荐从文件中读取私钥字符串而不是写入源码中
                MerchantPrivateKey = ReadFile(AppPrivateKeyPath),

                AlipayPublicKey = ReadFile(AlipayPublicKeyPath)
            };
        }

        public static string ReadFile(string filePath)
        {
            try
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.
                using StreamReader sr = new StreamReader(filePath);
                return sr.ReadToEnd();
            }
            catch (Exception e)
            {
                // Let the user know what went wrong.
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
                return string.Empty;
            }
        }
        // 交易查询
        public static string TradeQuery(string outTradeNo)
        {
            try
            {
                // 1. 发起API调用
                AlipayTradeQueryResponse response = Factory.Payment.Common().Query(outTradeNo);
                // 2. 处理响应或异常
                if ("10000".Equals(response.Code))
                {
                    Console.WriteLine("调用成功");
                    return response.TradeStatus;
                }
                else
                {
                    Console.WriteLine("调用失败，原因：" + response.Msg + "，" + response.SubMsg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("调用遭遇异常，原因：" + ex.Message);
                throw ex;
            }
            return "QueryError";
        }
        // 交易取消
        public static string TradeCancel(string outTradeNo)
        {
            try
            {
                // 2. 发起API调用
                AlipayTradeCancelResponse response = Factory.Payment.Common().Cancel(outTradeNo);
                // 3. 处理响应或异常
                if ("10000".Equals(response.Code))
                {
                    Console.WriteLine("调用成功");
                    return response.Action;
                }
                else
                {
                    Console.WriteLine("调用失败，原因：" + response.Msg + "，" + response.SubMsg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("调用遭遇异常，原因：" + ex.Message);
                throw ex;
            }
            return "CancelError";
        }
        // 交易关闭
        public static bool TradeClose(string outTradeNo)
        {
            try
            {
                // 2. 发起API调用
                AlipayTradeCancelResponse response = Factory.Payment.Common().Cancel(outTradeNo);
                // 3. 处理响应或异常
                if ("10000".Equals(response.Code))
                {
                    Console.WriteLine("调用成功");
                    return true;
                }
                else
                {
                    Console.WriteLine("调用失败，原因：" + response.Msg + "，" + response.SubMsg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("调用遭遇异常，原因：" + ex.Message);
                throw ex;
            }
            return false;
        }
        // 资金授权冻结接口
        public static bool FundFreeze(string authCode, string outTradeNo, string orderTitle, double amount)
        {
            try
            {

                Dictionary<string, string> textParaDic = new Dictionary<string, string>();
                Dictionary<string, object> bizParaDic = new Dictionary<string, object>();

                // 说明：支付授权码，25~30开头的长度为16~24位的数字，实际字符串长度以开发者获取的付款码长度为准
                bizParaDic.Add("auth_code", authCode);
                // 说明：授权码类型 目前支持\&quot;bar_code\&quot;和\&quot;security_code\&quot;，分别对应付款码和刷脸场景
                bizParaDic.Add("auth_code_type", "bar_code");
                // 说明：商户授权资金订单号 ,不能包含除中文、英文、数字以外的字符，创建后不能修改，需要保证在商户端不重复。
                bizParaDic.Add("out_order_no", outTradeNo);
                // 说明：商户本次资金操作的请求流水号，用于标示请求流水的唯一性，不能包含除中文、英文、数字以外的字符，需要保证在商户端不重复。
                bizParaDic.Add("out_request_no", outTradeNo);
                // 说明：业务订单的简单描述，如商品名称等
                bizParaDic.Add("order_title", orderTitle);
                // 说明：需要冻结的金额，单位为：元（人民币），精确到小数点后两位 取值范围：[0.01,100000000.00]
                bizParaDic.Add("amount", amount);
                // 说明：销售产品码，后续新接入预授权当面付的业务，新当面资金授权取值PRE_AUTH，境外预授权取值OVERSEAS_INSTORE_AUTH。
                bizParaDic.Add("product_code", "PRE_AUTH");

                AlipayOpenApiGenericResponse response = Factory.Util.Generic().Execute("alipay.fund.auth.order.freeze", textParaDic, bizParaDic);


                if ("10000".Equals(response.Code))
                {
                    Console.WriteLine("资金冻结接口调用成功");
                    auth_no = GetJsonProperty("alipay_fund_auth_order_freeze_response", response.HttpBody, "auth_no");
                    buyer_Id = GetJsonProperty("alipay_fund_auth_order_freeze_response", response.HttpBody, "payer_user_id");
                    if ("SUCCESS".Equals(GetJsonProperty("alipay_fund_auth_order_freeze_response", response.HttpBody, "status")))
                        return true;
                }
                else
                {
                    Console.WriteLine("调用失败，原因：" + response.Msg + "，" + response.SubMsg);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("调用遭遇异常，原因：" + ex.Message);
                throw ex;
            }
            return false;
        }
        // 资金解冻接口
        public static bool FundUnfreeze(string authNo, string outRequestNo, double amount, string remark)
        {
            try
            {
                Dictionary<string, string> textParaDic = new Dictionary<string, string>();
                Dictionary<string, object> bizParaDic = new Dictionary<string, object>();

                // 说明：支付宝资金授权订单号
                bizParaDic.Add("auth_no", authNo);
                // 说明：商户本次资金操作的请求流水号，同一商户每次不同的资金操作请求，商户请求流水号不能重复
                bizParaDic.Add("out_request_no", outRequestNo);

                // 说明：需要冻结的金额，单位为：元（人民币），精确到小数点后两位 取值范围：[0.01,100000000.00]
                bizParaDic.Add("amount", amount);
                // 说明：业务订单的简单描述，如商品名称等
                bizParaDic.Add("remark", remark);

                AlipayOpenApiGenericResponse response = Factory.Util.Generic().Execute("alipay.fund.auth.order.unfreeze", textParaDic, bizParaDic);

                if ("10000".Equals(response.Code))
                {
                    Console.WriteLine("资金冻结接口调用成功");
                    return true;
                }
                else
                {
                    Console.WriteLine("调用失败，原因：" + response.Msg + "，" + response.SubMsg);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("调用遭遇异常，原因：" + ex.Message);
                throw ex;
            }
            return false;
        }
        // 当面付支付接口
        public static bool FaceToFacePay(string subject, string outTradeNo, string amount, string authCode)
        {
            try
            {
                // 2. 发起API调用（当面付）
                AlipayTradePayResponse response = Factory.Payment.FaceToFace().Pay(subject, outTradeNo, amount, authCode);
                // 3. 处理响应或异常
                if ("10000".Equals(response.Code))
                {
                    Console.WriteLine("当面付接口调用成功");
                    return true;
                }
                else if ("40004".Equals(response.Code))
                {
                    Console.WriteLine("支付失败，原因：" + response.Msg + "，" + response.SubMsg);
                }
                else if ("10003".Equals(response.Code))
                {
                    Console.WriteLine("等待用户付款，原因：" + response.Msg + "，" + response.SubMsg);
                }
                else
                {
                    Console.WriteLine("调用失败，原因：" + response.Msg + "，" + response.SubMsg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("调用遭遇异常，原因：" + ex.Message);
                throw ex;
            }
            return false;
        }

        // 全额交易退款接口
        public static bool Refund(string outTradeNo, string amount)
        {
            try
            {
                // 2. 发起API调用
                AlipayTradeRefundResponse response = Factory.Payment.Common().Refund(outTradeNo, amount);
                // 3. 处理响应或异常
                if ("10000".Equals(response.Code))
                {
                    Console.WriteLine("调用成功");
                    return true;
                }
                else
                {
                    Console.WriteLine("调用失败，原因：" + response.Msg + "，" + response.SubMsg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("调用遭遇异常，原因：" + ex.Message);
                throw ex;
            }
            return false;
        }
        // 冻结转支付  不可用
        public static bool FundFreezeToPay(string subject, string outTradeNo, string amount, string authNo, string buyerId)
        {
            try
            {

                Dictionary<string, string> textParaDic = new Dictionary<string, string>();
                Dictionary<string, object> bizParaDic = new Dictionary<string, object>();

                // 说明：商户授权资金订单号 ,不能包含除中文、英文、数字以外的字符，创建后不能修改，需要保证在商户端不重复。
                bizParaDic.Add("out_trade_no", outTradeNo);

                bizParaDic.Add("product_code", "PRE_AUTH_ONLINE");

                // 说明：支付授权码，25~30开头的长度为16~24位的数字，实际字符串长度以开发者获取的付款码长度为准
                bizParaDic.Add("auth_no", authNo);
                // 说明：业务订单的简单描述，如商品名称等
                bizParaDic.Add("subject", subject);
                // 说明：订单总金额，单位为元，精确到小数点后两位，取值范围[0.01,100000000] 如果同时传入【可打折金额】和【不可打折金额】，该参数可以不用传入； 如果同时传入了【可打折金额】，【不可打折金额】，【订单总金额】三者，则必须满足如下条件：【订单总金额】=【可打折金额】+【不可打折金额】
                bizParaDic.Add("total_amount", amount);
                // 买家ID
                bizParaDic.Add("buyer_id", buyerId);
                // 卖家ID
                bizParaDic.Add("seller_id", "2088102180292281");

                // 说明：预授权确认模式，授权转交易请求中传入，适用于预授权转交易业务使用，目前只支持PRE_AUTH(预授权产品码) COMPLETE：转交易支付完成结束预授权，解冻剩余金额; NOT_COMPLETE：转交易支付完成不结束预授权，不解冻剩余金额
                bizParaDic.Add("auth_confirm_mode", "COMPLETE");

                AlipayOpenApiGenericResponse response = Factory.Util.Generic().Execute("alipay.trade.pay_response", textParaDic, bizParaDic);

                if ("10000".Equals(response.Code))
                {
                    Console.WriteLine("冻结转支付接口调用成功");
                    if ("SUCCESS".Equals(GetJsonProperty("alipay.trade.pay", response.HttpBody, "status")))
                        return true;
                }
                else if ("40004".Equals(response.Code))
                {
                    Console.WriteLine("支付失败，原因：" + response.Msg + "，" + response.SubMsg);
                }
                else if ("10003".Equals(response.Code))
                {
                    Console.WriteLine("等待用户付款，原因：" + response.Msg + "，" + response.SubMsg);
                }
                else
                {
                    Console.WriteLine("调用失败，原因：" + response.Msg + "，" + response.SubMsg);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("调用遭遇异常，原因：" + ex.Message);
                throw ex;
            }
            return false;
        }
        // 当面付付款码解码 不支持
        public static string DecodeUse(string dynamicId, string senceNo)
        {
            try
            {
                Dictionary<string, string> textParaDic = new Dictionary<string, string>();
                Dictionary<string, object> bizParaDic = new Dictionary<string, object>();

                // 说明：付款码码值
                bizParaDic.Add("dynamic_id", dynamicId);
                // 说明：外部业务号，用于标识这笔解码请求，对同一个码的重复解码请求，sence_no必须与上一次保持一致，每次请求的sence_no必须不一样，如alipay.marketing.facetoface.decode.use接口配合alipay.trade.pay（统一收单交易支付接口）一并使用时，alipay.trade.pay接口的extend_params属性中必须设置DYNAMIC_TOKEN_OUT_BIZ_NO，且值必须与sence_no保持一致。
                bizParaDic.Add("sence_no", senceNo);

                AlipayOpenApiGenericResponse response = Factory.Util.Generic().Execute("alipay.marketing.facetoface.decode.use", textParaDic, bizParaDic);

                if ("10000".Equals(response.Code))
                {
                    Console.WriteLine("付款码解码接口调用成功");
                    return GetJsonProperty("aalipay.marketing.facetoface.decode.use_response", response.HttpBody, "user_id");
                }
                else
                {
                    Console.WriteLine("调用失败，原因：" + response.Msg + "，" + response.SubMsg);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("调用遭遇异常，原因：" + ex.Message);
                throw ex;
            }
            return "error";
        }

        // 交易预创建，生成正扫二维码请求接口
        public static string PrecreateQRcode(string subject, string outTradeNo, string totalAmount)
        {
            try
            {
                // 1. 发起API调用
                AlipayTradePrecreateResponse response = Factory.Payment.FaceToFace().PreCreate(subject, outTradeNo, totalAmount);
                // 2. 处理响应或异常
                if ("10000".Equals(response.Code))
                {
                    Console.WriteLine("交易预创建，请求二维码调用成功");
                    return response.QrCode;
                }
                else
                {
                    Console.WriteLine("调用失败，原因：" + response.Msg + "，" + response.SubMsg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("调用遭遇异常，原因：" + ex.Message);
                throw ex;
            }
            return "CreateError";
        }
        // 获取httpResponse中JSON属性
        public static string GetJsonProperty(string interfaceName, string json, string property)
        {
            JObject jObject = JObject.Parse(json);
            return jObject[interfaceName][property].ToString();
        }
    }
}
