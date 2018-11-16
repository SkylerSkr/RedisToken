using ServiceStack.RedisHelper;
using System;
using System.Collections.Generic;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {

            using (var helper = new DoRedisString())
            {
                //var a= helper.Core.GetValueFromHash("user:1","Token");
                var id = helper.IncrId("user:*", 1);
                Register(new User()
                { ID = id.ToString(), PassWord = "123", Token = Guid.NewGuid().ToString(), UserName = "123" });
            }

            Console.WriteLine(Validate("d1cb400b-5183-4e26-aa0f-3c892b33185d"));

            Login("1", "123");

            Console.WriteLine(Validate("d1cb400b-5183-4e26-aa0f-3c892b33185d"));

            Console.ReadKey();
        }

        #region 验证       
        /// <summary>
        /// 验证是否有此Token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        static bool Validate(string token)
        {
            using (var helper = new DoRedisHash())
            {
                return helper.HashContainsEntry("Tokens", token);
            }
        }
        #endregion

        #region 登陆
        /// <summary>
        /// 登陆
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="passWord"></param>
        static string Login(string userId, string passWord)
        {
            if (IsAuth(userId, passWord))//验证密码是否正确
            {
                using (var helper = new DoRedisHash())
                {

                    //移除失效token
                    var token = helper.GetValueFromHash("user:" + userId, "Token");
                    helper.RemoveEntryFromHash("Tokens", token);

                    token = Guid.NewGuid().ToString();
                    helper.SetEntryInHash("Tokens", token, userId);//更新到tokens散列
                    helper.SetEntryInHash("user:" + userId, "Token", token);//更新用户token
                    return token;
                }
            }
            return null;
        }

        static bool IsAuth(string userId, string passWord)
        {
            using (var helper = new DoRedisHash())
            {
                var p = helper.GetValueFromHash("user:" + userId, "PassWord");
                return passWord == p;
            }
        }
        #endregion

        #region 注册
        /// <summary>
        /// 注册
        /// </summary>
        /// <returns></returns>
        static bool Register(User user)
        {
            using (var helper = new DoRedisHash())
            {
                helper.Core.SetRangeInHash("user:" + user.ID, ObjectToKeyValuePairs(user));
                helper.SetEntryInHash("Tokens", user.Token, user.ID);
            }
            return true;
        }

        /// <summary>
        /// 反射转KeyValuePairs
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        static IEnumerable<KeyValuePair<string, string>> ObjectToKeyValuePairs(User user)
        {
            foreach (System.Reflection.PropertyInfo p in user.GetType().GetProperties())
            {
                yield return new KeyValuePair<string, string>(p.Name, (string)p.GetValue(user));
            }
        }
        #endregion

    }
}
