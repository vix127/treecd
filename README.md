# treecd

  <a href="LICENSE">
    <img src="https://img.shields.io/github/license/vix127/treecd" alt="License">
  </a>

An interactive, tree-style directory browser. Start it in any directory, navigate
the file system with the arrow keys, and select the directory you want to switch to.

## Build

```bash
dotnet build src/Treecd.csproj
```

## Run

```bash
dotnet run --project src/Treecd.csproj
```

## Keys

- `Up` / `Down` - move the selection
- `Right` - expand a directory
- `Left` - collapse a directory
- `Enter` - select the highlighted directory and exit
- `Esc` / `Q` - cancel without changing directory

## Shell integration

`treecd` cannot change the working directory of its parent shell directly. It
prints the selected directory to standard output and changes its own working
directory. To make `treecd` behave like `cd`, wrap it in a shell function such
as the following PowerShell example:

```powershell
function treecd {
    $target = & treecd.exe @args
    if ($LASTEXITCODE -eq 0 -and $target) {
        Set-Location $target
    }
}
```

After adding the function to your profile, run `treecd` as usual.
