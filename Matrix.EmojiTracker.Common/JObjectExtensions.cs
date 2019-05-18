using Newtonsoft.Json.Linq;

namespace Matrix.EmojiTracker.Common
{
    public static class JObjectExtensions
    {
        public static T GetValue<T>(this JObject obj, string property)
        {
            return obj.GetValue(property).ToObject<T>();
        }

        public static void SetValue(this JObject obj, string property, object value)
        {
            if (obj.ContainsKey(property)) obj.Property(property).Remove();
            obj.AddAfterSelf(new JProperty(property, value));
        }
    }
}
