"""Compile-check the game and editor C# without Unity (works while Unity is open or closed).

Usage:
    python -I compile_check.py [--extra path/to/New.cs ...] [--out folder] [--playmode-tests]

--playmode-tests also compiles code inside #if UNITY_INCLUDE_TESTS (PlayMode scenario tests
that live in Assembly-CSharp) with NUnit and UnityEngine.TestRunner referenced.

It reads Unity's generated Assembly-CSharp(.csproj) and Assembly-CSharp-Editor.csproj,
references the DLLs in Library/ScriptAssemblies instead of the project references,
adds every .cs under Assets/Script that the csproj does not list yet (new files),
then runs `dotnet build` on both. Prints "OK" or the compiler errors.
"""
import os
import re
import subprocess
import sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))


def project(name, out, extra_compile, extra_refs, extra_defines=""):
    text = open(os.path.join(ROOT, name + ".csproj"), encoding="utf-8-sig").read()
    compiles = [os.path.join(ROOT, c) for c in re.findall(r'<Compile Include="([^"]+)"', text)]
    compiles = [c for c in compiles if not c.endswith(".cs") or os.path.exists(c)]
    hints = re.findall(r"<HintPath>([^<]+)</HintPath>", text)
    refs = [h if os.path.isabs(h) else os.path.join(ROOT, h) for h in hints]
    for proj in re.findall(r'<ProjectReference Include="([^"]+)\.csproj"', text):
        dll = os.path.join(ROOT, "Library", "ScriptAssemblies", proj + ".dll")
        if os.path.exists(dll):
            refs.append(dll)
    refs += extra_refs
    known = {os.path.normcase(os.path.abspath(c)) for c in compiles}
    compiles += [c for c in extra_compile if os.path.normcase(os.path.abspath(c)) not in known]
    defines = re.search(r"<DefineConstants>([^<]+)</DefineConstants>", text).group(1) + extra_defines
    lang = re.search(r"<LangVersion>([^<]+)</LangVersion>", text).group(1)

    items = "\n".join(f'    <Compile Include="{c}" />' for c in compiles)
    references = "\n".join(
        f'    <Reference Include="{os.path.splitext(os.path.basename(r))[0]}"><HintPath>{r}</HintPath><Private>false</Private></Reference>'
        for r in refs)
    body = f"""<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
    <EnableDefaultItems>false</EnableDefaultItems>
    <LangVersion>{lang}</LangVersion>
    <DefineConstants>{defines}</DefineConstants>
    <NoWarn>0169;0414;0649;0618;0067;0108;0114;0252;8632;1998</NoWarn>
    <AssemblyName>{name}</AssemblyName>
    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
    <DisableImplicitFrameworkReferences>true</DisableImplicitFrameworkReferences>
    <NoStdLib>true</NoStdLib>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
  </PropertyGroup>
  <ItemGroup>
{items}
  </ItemGroup>
  <ItemGroup>
{references}
  </ItemGroup>
</Project>"""
    folder = os.path.join(out, name)
    os.makedirs(folder, exist_ok=True)
    path = os.path.join(folder, name + ".csproj")
    open(path, "w", encoding="utf-8").write(body)
    return folder


def new_files(editor):
    found = []
    for base, _, files in os.walk(os.path.join(ROOT, "Assets", "Script")):
        in_editor = os.sep + "Editor" in base
        if in_editor != editor:
            continue
        found += [os.path.join(base, f) for f in files if f.endswith(".cs")]
    return found


def build(folder):
    result = subprocess.run(["dotnet", "build", "-nologo", "-v:q"], cwd=folder, capture_output=True, text=True)
    errors = sorted({l.strip() for l in result.stdout.splitlines() if " error " in l})
    return result.returncode == 0, errors


def main():
    args = sys.argv[1:]
    out = args[args.index("--out") + 1] if "--out" in args else os.path.join(os.environ.get("TEMP", "/tmp"), "pv_compile_check")
    extra = [os.path.abspath(a) for i, a in enumerate(args) if i > 0 and args[i - 1] == "--extra"]

    test_refs, test_defines = [], ""
    runtime_text = open(os.path.join(ROOT, "Assembly-CSharp.csproj"), encoding="utf-8-sig").read()
    if "--playmode-tests" in args or "UNITY_INCLUDE_TESTS" in runtime_text:
        editor_text = open(os.path.join(ROOT, "Assembly-CSharp-Editor.csproj"), encoding="utf-8-sig").read()
        test_refs = [h if os.path.isabs(h) else os.path.join(ROOT, h) for h in re.findall(r"<HintPath>([^<]+nunit[^<]+)</HintPath>", editor_text)]
        test_refs.append(os.path.join(ROOT, "Library", "ScriptAssemblies", "UnityEngine.TestRunner.dll"))
        test_defines = "" if "UNITY_INCLUDE_TESTS" in runtime_text else ";UNITY_INCLUDE_TESTS"
    runtime = project("Assembly-CSharp", out, new_files(False) + [e for e in extra if os.sep + "Editor" not in e], test_refs, test_defines)
    ok, errors = build(runtime)
    print("Assembly-CSharp:", "OK" if ok else "FAILED")
    for e in errors:
        print("  " + e)
    if not ok:
        sys.exit(1)

    built = os.path.join(runtime, "bin", "Debug", "netstandard2.1", "Assembly-CSharp.dll")
    editor = project("Assembly-CSharp-Editor", out, new_files(True) + [e for e in extra if os.sep + "Editor" in e], [built])
    ok, errors = build(editor)
    print("Assembly-CSharp-Editor:", "OK" if ok else "FAILED")
    for e in errors:
        print("  " + e)
    sys.exit(0 if ok else 1)


if __name__ == "__main__":
    main()
