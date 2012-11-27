using System;
using KollaSoft;
using Awful.Core;

namespace Awful.Core.Test
{
    public class Settings : KollaSoft.Settings, AwfulApplicationSettings
    {
        private const string CURRENT_PROFILE_ID_KEY = "0";
        private const string DB_CONNECTION_KEY = "1";

        private const int CURRENT_PROFILE_ID_DEFAULT = -1;
        private const string DB_CONNECTION_DEFAULT = null;

        public Settings() : base() { }

        public override void LoadSettings() { }

        public int CurrentProfileID
        {
            get { return this.GetValueOrDefault<int>(CURRENT_PROFILE_ID_KEY, CURRENT_PROFILE_ID_DEFAULT); }
            set { this.AddOrUpdateValue(CURRENT_PROFILE_ID_KEY, value); }
        }

        public int CurrentThemeID
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
