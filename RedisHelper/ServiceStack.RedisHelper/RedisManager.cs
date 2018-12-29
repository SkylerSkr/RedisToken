using ServiceStack.Redis;
using System;
using System.Linq;
using System.Reflection;

namespace ServiceStack.RedisHelper
{
    public class RedisManager
    {
        /// <summary>
        /// 同步锁
        /// </summary>
        private static readonly object obj = new object();
        private static PooledRedisClientManager prcm;

        /// <summary>
        /// 静态构造方法，初始化链接池管理对象
        /// </summary>
        static RedisManager()
        {
            CreateManager();
        }

        /// <summary>
        /// 创建链接池管理对象
        /// </summary>
        private static void CreateManager()
        {
            //注册破解
            Register();

            string[] WriteServerConStr = SplitString(System.Configuration.ConfigurationSettings.AppSettings["WriteServerConStr"], ",");
            string[] ReadServerConStr = SplitString(System.Configuration.ConfigurationSettings.AppSettings["ReadServerConStr"], ",");
            var MaxWritePoolSize = System.Configuration.ConfigurationSettings.AppSettings["MaxWritePoolSize"];
            var MaxReadPoolSize = System.Configuration.ConfigurationSettings.AppSettings["MaxReadPoolSize"];
            var DefaultDb = System.Configuration.ConfigurationSettings.AppSettings["DefaultDb"];
            var AutoStart = System.Configuration.ConfigurationSettings.AppSettings["AutoStart"];
            RedisConfig.VerifyMasterConnections = false;//阿里云不关用不了
            prcm = new PooledRedisClientManager(ReadServerConStr, WriteServerConStr,
                             new RedisClientManagerConfig
                             {
                                 MaxWritePoolSize = Convert.ToInt32(MaxWritePoolSize),
                                 MaxReadPoolSize = Convert.ToInt32(MaxReadPoolSize),
                                 DefaultDb = Convert.ToInt32(DefaultDb),
                                 AutoStart = Convert.ToBoolean(AutoStart),
                             },Convert.ToInt32(DefaultDb),50,5);


        }

        private static string[] SplitString(string strSource, string split)
        {
            return strSource.Split(split.ToArray());
        }

        /// <summary>
        /// 注册破解
        /// </summary>
        private static void Register()
        {
            var licenseUtils = typeof(LicenseUtils);
            var members = licenseUtils.FindMembers(MemberTypes.All, BindingFlags.NonPublic | BindingFlags.Static, null, null);
            Type activatedLicenseType = null;
            foreach (var memberInfo in members)
            {
                if (memberInfo.Name.Equals("__activatedLicense", StringComparison.OrdinalIgnoreCase) && memberInfo is FieldInfo fieldInfo)
                    activatedLicenseType = fieldInfo.FieldType;
            }

            if (activatedLicenseType != null)
            {
                var licenseKey = new LicenseKey
                {
                    Expiry = DateTime.Today.AddYears(100),
                    Ref = "ServiceStack",
                    Name = "Enterprise",
                    Type = LicenseType.Enterprise
                };

                var constructor = activatedLicenseType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(LicenseKey) }, null);
                if (constructor != null)
                {
                    var activatedLicense = constructor.Invoke(new object[] { licenseKey });
                    var activatedLicenseField = licenseUtils.GetField("__activatedLicense", BindingFlags.NonPublic | BindingFlags.Static);
                    if (activatedLicenseField != null)
                        activatedLicenseField.SetValue(null, activatedLicense);
                }
            }
        }

        /// <summary>
        /// 客户端缓存操作对象
        /// </summary>
        public static IRedisClient GetClient()
        {
            if (prcm == null)
            {
                lock (obj)
                {
                    CreateManager();
                }
            }
            return prcm.GetClient();
        }
    }
}
