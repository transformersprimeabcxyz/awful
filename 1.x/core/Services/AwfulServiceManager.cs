using System;
using System.Windows;
using Awful.Core.Models.Messaging.Interfaces;

namespace Awful.Core.Services
{
    public static class AwfulServiceManager
    {
        public static IPrivateMessagingService PrivateMessageService
        {
            get { return AwfulPrivateMessageService.Service; }
        }
    }
}
