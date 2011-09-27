using NUnit.Framework;
using RazorFileTestModels;
using RazorFileTests.Support;

namespace RazorFileTests
{
    [TestFixture]
    public class ParentChildTests : ParentChildTestBase
    {
        [Test]
        public void SimpleParentChild()
        {
            var testName = "SimpleParentChild";

            // Init Parent-Child test
            InitParentChildTest(testName);

            // Execute
            var model = new ParentModel() { ChildFolderName = testName };
            ExecuteParentChildTest(model);

            // Assert
            base.AssertParentChildrenExist();
        }
    }
}
