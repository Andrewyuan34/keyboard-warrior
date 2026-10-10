"""Check tracked source hygiene; deliberately not a Unity build or game test."""

import pathlib
import re
import subprocess
import sys


ROOT = pathlib.Path(__file__).resolve().parents[1]
CJK = re.compile(
    r"[\u2e80-\u2fdf\u3007\u31c0-\u31ef\u3400-\u4dbf\u4e00-\u9fff"
    r"\uf900-\ufaff\U00020000-\U0002ffff\U00030000-\U000323af]"
)


def main():
    result = subprocess.run(
        ["git", "ls-files", "-z"], cwd=ROOT, check=True, stdout=subprocess.PIPE
    )
    paths = set(result.stdout.decode("utf-8").split("\0")) - {""}
    errors = []
    generated_dirs = {
        "library", "temp", "obj", "logs", "usersettings", "build", "builds",
        "memorycaptures", "recordings", ".vs", ".idea",
    }
    text_extensions = {
        ".cs", ".py", ".json", ".yml", ".yaml", ".unity", ".prefab",
        ".meta", ".shader", ".hlsl", ".cginc", ".inputactions", ".md",
        ".txt", ".ps1", ".sh", ".cmd", ".bat", ".toml", ".xml",
        ".uxml", ".uss", ".asmdef", ".asmref", ".csv", ".html", ".css",
    }
    marker = re.compile(r"^(?:<{7} .+|>{7} .+)$", re.MULTILINE)
    for name in sorted(paths):
        if CJK.search(name):
            errors.append("Repository filenames must use English: " + ascii(name))
        relative = pathlib.PurePosixPath(name)
        parts = relative.parts
        if (len(parts) > 1 and parts[0].lower() == "game"
                and parts[1].lower() in generated_dirs):
            errors.append("Generated Unity directory is tracked: " + name)
        if parts[0].lower() in {"builds", "artifacts"}:
            errors.append("Build output is tracked: " + name)
        if relative.name == ".env" or (
            relative.name.startswith(".env.") and relative.name != ".env.example"
        ):
            errors.append("Local environment file is tracked: " + name)

        file_path = ROOT / relative
        if not file_path.is_file():
            errors.append("Tracked file is missing from checkout: " + name)
            continue
        try:
            content = file_path.read_text(encoding="utf-8-sig")
        except UnicodeDecodeError:
            if relative.suffix.lower() in text_extensions:
                errors.append("Expected UTF-8 text: " + name)
            continue
        if "\0" in content:
            if relative.suffix.lower() in text_extensions:
                errors.append("Unexpected NUL character in text file: " + name)
            continue
        cjk_match = CJK.search(content)
        if cjk_match:
            line = content.count("\n", 0, cjk_match.start()) + 1
            errors.append(
                f"Repository text must use English: {name}:{line}. "
                "Use Unicode escapes for non-English input-test fixtures."
            )
        if relative.suffix.lower() in text_extensions:
            if marker.search(content):
                errors.append("Unresolved merge marker: " + name)

    asset_prefix = "Game/Assets/"
    assets = {
        p for p in paths if p.startswith(asset_prefix)
        and not any(part.startswith(".") or part.endswith("~")
                    for part in pathlib.PurePosixPath(p).parts[2:])
    }
    folders = set()
    for name in assets:
        parent = pathlib.PurePosixPath(name).parent
        while str(parent).startswith(asset_prefix):
            folders.add(str(parent))
            parent = parent.parent
    for name in sorted(assets):
        if name.endswith(".meta"):
            target = name[:-5]
            meta = ROOT / name
            is_folder = meta.is_file() and re.search(
                r"^folderAsset:\s*yes\s*$", meta.read_text(encoding="utf-8-sig"),
                re.MULTILINE,
            )
            if target not in paths and target not in folders and not is_folder:
                errors.append("Orphaned asset/folder meta: " + name)
        elif name + ".meta" not in paths:
            errors.append("Missing asset meta: " + name + ".meta")
    for folder in sorted(folders):
        if folder + ".meta" not in paths:
            errors.append("Missing folder meta: " + folder + ".meta")

    unity_files = (
        "Game/ProjectSettings/ProjectVersion.txt",
        "Game/Packages/manifest.json",
        "Game/Packages/packages-lock.json",
    )
    if any(p.startswith("Game/") for p in paths):
        for name in unity_files:
            if name not in paths:
                errors.append("Unity bootstrap is missing: " + name)
    else:
        print("Unity project not initialized; only collaboration files are checked.")
    for error in errors:
        print("ERROR: " + error, file=sys.stderr)
    if errors:
        return 1
    print("Repository checks passed. No Unity compilation, gameplay, or LFS-download validation was performed.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
