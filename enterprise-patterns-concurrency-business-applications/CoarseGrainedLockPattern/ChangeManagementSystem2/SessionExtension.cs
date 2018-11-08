using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ChangeManagementSystem
{
    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            JsonSerializerSettings jss = new JsonSerializerSettings();
            jss.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            session.SetString(key, JsonConvert.SerializeObject(value, jss));
        }

        public static T Get<T>(this ISession session, string key)
        {
            JsonSerializerSettings jss = new JsonSerializerSettings();
            jss.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            var value = session.GetString(key);
            return value == null ? default(T) :
                                  JsonConvert.DeserializeObject<T>(value, jss);
        }
    }
}
