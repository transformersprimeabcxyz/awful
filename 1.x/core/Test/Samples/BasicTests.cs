using System;
using System.Collections.Generic;
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

namespace Awful.Core.Test
{    
    public class BasicTests : SilverlightTest
    {
        [TestMethod]
        public void AlwaysPass()
        {
            Assert.IsTrue(true, "method intended to always pass");
        }
             
        [TestMethod]
        [Description("This test always fails intentionally")]
        public void AlwaysFail()
        {
            Assert.IsFalse(true, "method intended to always fail");
        }
        
        /*
        [TestMethod]
        public void TestInternalMethod()
        {
            string joinedString = StringJoiner.AppendStringsWithSymbol(new List<string>() { "hello", "there" }, '?');
            Assert.IsTrue(string.Compare(joinedString, "hello?there") == 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AppendStringsWithDashes_EmptyCollection_ExpectedException()
        {
            StringJoiner.AppendStringsWithDashes(new List<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AppendstringsWithDashes_NullCollection_ExpectedException()
        {
            StringJoiner.AppendStringsWithDashes(null);
        }
        */
    }
}
