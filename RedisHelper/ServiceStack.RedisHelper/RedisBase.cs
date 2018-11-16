using ServiceStack.Redis;
using System;
using System.Linq;

namespace ServiceStack.RedisHelper
{/// <summary>
 /// RedisBase类，是redis操作的基类，继承自IDisposable接口，主要用于释放内存
 /// </summary>
    public abstract class RedisBase : IDisposable
    {
        public IRedisClient Core { get; set; }

        private bool _disposed = false;
        //static RedisBase()
        //{
        //    Core = RedisManager.GetClient(); 
        //}

        public RedisBase()
        {
            Core = RedisManager.GetClient();
        }

        /// <summary>
        /// Redis获取自增长序号(正则) 并设置60秒过期 防止缓存穿透
        /// </summary>
        /// <returns></returns>
        public long IncrId(string pattern, int incr)
        {

            long count = 1;
            var result = Core.Get<string>(pattern);
            if (string.IsNullOrEmpty(result))
            {
                count = Core.GetKeysByPattern(pattern).Count() + incr;
            }
            else
            {
                count = Convert.ToInt32(result) + incr;
            }
            Core.Set<string>(pattern, count.ToString(), new TimeSpan(0, 0, 60));

            return count;


        }
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    Core.Dispose();
                    Core = null;
                }
            }
            this._disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// 保存数据DB文件到硬盘
        /// </summary>
        public void Save()
        {
            Core.Save();
        }
        /// <summary>
        /// 异步保存数据DB文件到硬盘
        /// </summary>
        public void SaveAsync()
        {
            Core.SaveAsync();
        }
    }
}
