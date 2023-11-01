using System;
using System.Collections.Generic;

namespace Maxst.Token
{
    public class TokenDictionary
    {
        private Dictionary<string, object> dictionary;

        public TokenDictionary(Dictionary<string, object> d)
        {
            dictionary = d;
        }

        public void SetTokenDictionary(Dictionary<string, object> d)
        {
            dictionary = d;
        }
        public Dictionary<string, object> GetTokenDictionary()
        {
            return dictionary;
        }

        public T GetTypedValue<T>(string key)
        {
            if (dictionary.TryGetValue(key, out object value))
            {
                if (value is T castedValue)
                {
                    return castedValue;
                }
                else
                {
                    throw new InvalidCastException($"[TokenDictionary] GetValue type : {typeof(T)} InvalidCastException");
                }
            }
            else
            {
                throw new Exception($"[TokenDictionary] dictionary.TryGetValue Exception error");
            }
        }
    }
}
