import argparse
import glob
import os
import sys

from vfx_effectcraft import PROJECTS_DIR, GrayscaleOnlyRuleError, export_to_unity, render_project


def collect(targets):
    if not targets:
        return sorted(glob.glob(os.path.join(PROJECTS_DIR, "*", "*.ecproj")))
    found = []
    for t in targets:
        if t.lower().endswith(".ecproj"):
            path = t if os.path.isabs(t) else os.path.abspath(t)
            if not os.path.exists(path):
                path = os.path.join(PROJECTS_DIR, t)
            found.append(path)
            continue
        folder = t if os.path.isdir(t) else os.path.join(PROJECTS_DIR, t)
        hits = sorted(glob.glob(os.path.join(folder, "*.ecproj")))
        if not hits:
            hits = sorted(glob.glob(os.path.join(PROJECTS_DIR, "*", t + ".ecproj")))
        if not hits:
            raise SystemExit("nothing matches '%s' (give a .ecproj path, a set folder name under projects/, or a comp name)" % t)
        found += hits
    return found


def main():
    parser = argparse.ArgumentParser(description="Render edited EffectCraft projects to sprite sheets, contact strips and GIFs.")
    parser.add_argument("targets", nargs="*", help=".ecproj files, set names (e.g. WeaponV2Style) or comp names; default: every project")
    parser.add_argument("--comp", help="comp to render when a project holds several (default: comp named like the file, else the active comp)")
    parser.add_argument("--unity", action="store_true", help="copy each sheet into Assets/Image/VFX/Weapon")
    parser.add_argument("--no-gallery", action="store_true", help="skip rebuilding index.html")
    args = parser.parse_args()
    failed = []
    for project in collect(args.targets):
        rel = os.path.relpath(project, PROJECTS_DIR)
        try:
            sheet = render_project(project, comp=args.comp)
            print("rendered", rel)
            if args.unity:
                print("  unity", export_to_unity(sheet))
        except GrayscaleOnlyRuleError as e:
            failed.append(rel)
            print("BLOCKED", rel, "-", e)
    if not args.no_gallery:
        import make_vfx_gallery
        make_vfx_gallery.main()
    if failed:
        sys.exit("grayscale rule failed for: " + ", ".join(failed))


if __name__ == "__main__":
    main()
