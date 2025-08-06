using System;
using System.Linq;

namespace TPC
{
    public static class AuxiliarMethods
    {
        private static Type[] _extensionTypes;
        public static Type[] ExtensionTypes
        {
            get
            {
                if (_extensionTypes != null)
                    return _extensionTypes;
                _extensionTypes = typeof(CharacterExtension).Assembly.GetTypes().Where(x => !x.IsAbstract && x.IsSubclassOf(typeof(CharacterExtension))).ToArray();
                return _extensionTypes;
            }
        }

        public static string[] ExtensionNames => ExtensionTypes.Select(x => x.ToString()).ToArray();
    }
}