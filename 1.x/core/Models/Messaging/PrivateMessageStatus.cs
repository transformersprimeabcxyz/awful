using System;

namespace Awful.Core.Models.Messaging
{
    public enum PrivateMessageStatus
    {
        UNKNOWN=0,
        NEW,
        READ,
        CANCELED,
        REPLIED,
        FORWARDED
    }
}
