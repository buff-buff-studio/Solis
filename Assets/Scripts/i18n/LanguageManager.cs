namespace i18n
{
    /*
    /// <summary>
    /// Used to localize strings.
    /// </summary>
    public static class LanguageManager
    {
        /// <summary>
        /// Localizes a string using the given key.
        /// Replaces {0}, {1}, etc. with the given args.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string Localize(string key, params object[] args)
        {
            return Localize(Hash(key), args);
        }
        
        /// <summary>
        /// Localizes a string using the given key.
        /// Replaces {0}, {1}, etc. with the given args.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string Localize(int key, params object[] args)
        {
            if (!_Entries.TryGetValue(key, out var entry)) 
                return "[missing]";
            
            var values = entry.values;
            return string.Format(values[values.Length > 1 ? Random.Range(0, values.Length) : 0], args);
        }
        
        /// <summary>
        /// Hashes a string to an integer, for faster lookups.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int Hash(string key)
        {
            return Animator.StringToHash(key);   
        }
    }
    */
}