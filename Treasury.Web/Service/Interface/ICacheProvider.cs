namespace Treasury.Web.Service.Interface
{
    public interface ICacheProvider
    {
        /// <summary>
        /// 抓取 Cache
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object Get(string key);

        /// <summary>
        /// 刪除(重設) Cache
        /// </summary>
        /// <param name="key"></param>
        void Invalidate(string key);

        /// <summary>
        /// 判斷是否有 Cache
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool IsSet(string key);

        /// <summary>
        /// 設定 Cache
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="cacheTime"></param>
        void Set(string key, object data, int cacheTime = 30);
    }
}
