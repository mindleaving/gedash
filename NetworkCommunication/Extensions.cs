namespace NetworkCommunication
{
    public static class Extensions
    {
        public static bool InSet<T>(this T value, params T[] items)
        {
            foreach (var item in items)
            {
                if (value.Equals(item))
                    return true;
            }
            return false;
        }
    }
}
