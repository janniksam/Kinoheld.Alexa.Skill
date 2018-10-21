using Alexa.NET.Request;

namespace Kinoheld.Application.Extensions
{
    public static class IntentExtensions
    {
        public static string GetSlot(this Intent intent, string slotName)
        {
            if (intent?.Slots == null)
            {
                return null;
            }

            if (intent.Slots.ContainsKey(slotName))
            {
                return intent.Slots[slotName].Value;
            }

            return null;

        }
    }
}