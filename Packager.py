import argparse
import pathlib
from zipfile import ZipFile
import os

parser = argparse.ArgumentParser()
parser.add_argument("folder")
args = parser.parse_args()
folder = args.folder

with ZipFile(os.path.join(folder, f"{folder}.zip"), "w") as zip_object:
    zip_object.write(os.path.join(folder, "Icon.png"), "icon.png")
    zip_object.write(os.path.join(folder, "manifest.json"), "manifest.json")
    zip_object.write(os.path.join(folder, "README.md"), "README.md")
    zip_object.write(
        os.path.join(folder, f"bin/Debug/netstandard2.0/{folder}.dll"), f"{folder}.dll"
    )
    if pathlib.Path(os.path.join(folder, "CHANGELOG.md")).is_file():
        zip_object.write(os.path.join(folder, "CHANGELOG.md"), "CHANGELOG.md")
