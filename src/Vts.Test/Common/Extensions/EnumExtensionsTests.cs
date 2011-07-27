using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Vts.Extensions;

namespace Vts.Test.Common
{
    /// <summary>
    /// test check that bitmap definitions in Enums are setup correctly
    /// ref: http://msdn.microsoft.com/en-us/library/cc138362.aspx
    /// </summary>
    [TestFixture] 
    public class EnumExtensionsTests
    {
        enum Days
        {
            None = 0x0,
            Sunday = 0x1, 
            Monday = 0x2,
            Tuesday = 0x4,
            Wednesday = 0x8,
            Thursday = 0x10,
            Friday = 0x20,
            Saturday = 0x40
        }

        [Test]
        public void validate_Has_returns_correct_values()
        {
            Days meetingDays = Days.Tuesday | Days.Thursday;
            Assert.IsTrue(meetingDays.Has(Days.Tuesday));
            Assert.IsFalse(meetingDays.Has(Days.Monday));
        }

        [Test]
        public void validate_Is_returns_correct_values()
        {
            Days meetingDays = Days.Tuesday | Days.Thursday;
            Assert.IsTrue(meetingDays.Is(Days.Tuesday | Days.Thursday));
            Assert.IsFalse(meetingDays.Is(Days.Tuesday | Days.Monday));
        }

        [Test]
        public void validate_Add_returns_correct_values()
        {
            Days meetingDays = Days.Tuesday;
            meetingDays = meetingDays.Add(Days.Thursday);
            Assert.IsTrue(meetingDays == (Days.Tuesday | Days.Thursday));
        }
        [Test]
        public void validate_Remove_returns_correct_values()
        {
            Days meetingDays = Days.Tuesday | Days.Thursday;
            Assert.IsTrue(meetingDays.Remove(Days.Tuesday) == Days.Thursday);
        }

    }

}