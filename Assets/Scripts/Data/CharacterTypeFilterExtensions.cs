namespace Solis.Data
{
    /// <summary>
    /// Used to filter characters by type.
    /// </summary>
    public static class CharacterTypeFilterExtensions
    {
        /// <summary>
        /// Filters the character type. Returns true if the character type matches the filter.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool Filter(this CharacterTypeFilter filter, CharacterType type)
        {
            return filter switch
            {
                CharacterTypeFilter.Human => type == CharacterType.Human,
                CharacterTypeFilter.Robot => type == CharacterType.Robot,
                CharacterTypeFilter.Both => true,
                _ => false
            };
        }
    }
}