using System.IO;
using TreeCD.FileSystem;
using TreeCD.Navigation;
using Xunit;

public class TreeNavigatorTests
{
    [Fact]
    public void InitialSelectionIsFirstEntry()
    {
        var root = Directory.CreateTempSubdirectory("treecd-test");
        try
        {
            Directory.CreateDirectory(Path.Combine(root.FullName, "alpha"));
            File.WriteAllText(Path.Combine(root.FullName, "zeta.txt"), "x");

            var navigator = new TreeNavigator(root.FullName, new PhysicalFileSystem());

            Assert.NotNull(navigator.SelectedNode);
            Assert.Equal("alpha", navigator.SelectedNode!.Name);
            Assert.Equal(2, navigator.VisibleNodes.Count);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public void ExpandingAndCollapsingChangesVisibleChildren()
    {
        var root = Directory.CreateTempSubdirectory("treecd-test");
        try
        {
            var child = Directory.CreateDirectory(Path.Combine(root.FullName, "child"));
            File.WriteAllText(Path.Combine(child.FullName, "file.txt"), "x");

            var navigator = new TreeNavigator(root.FullName, new PhysicalFileSystem());
            Assert.Equal("child", navigator.SelectedNode!.Name);
            Assert.Single(navigator.VisibleNodes);

            navigator.ExpandSelected();
            Assert.Equal(2, navigator.VisibleNodes.Count);

            navigator.CollapseSelected();
            Assert.Single(navigator.VisibleNodes);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public void EnterOnDirectoryCompletesWithItsPath()
    {
        var root = Directory.CreateTempSubdirectory("treecd-test");
        try
        {
            Directory.CreateDirectory(Path.Combine(root.FullName, "target"));

            var navigator = new TreeNavigator(root.FullName, new PhysicalFileSystem());
            navigator.TryCompleteSelection();

            Assert.True(navigator.IsCompleted);
            Assert.Equal(Path.Combine(root.FullName, "target"), navigator.SelectedPath);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }
}
