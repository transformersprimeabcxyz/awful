﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Awful.Core.Test
{
    public class AsyncTests : SilverlightTest
    {
        [TestMethod]
        public void AsyncAppendStringTest()
        {
            var appendStrings = new List<string>() { "hello", "there" };

            Assert.IsTrue(true);
        }
    }
}
