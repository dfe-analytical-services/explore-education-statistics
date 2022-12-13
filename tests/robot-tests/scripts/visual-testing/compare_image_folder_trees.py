import argparse
import os

from compare_images import compare_images


def compare_images_in_folder_trees(first_folderpath: str, second_folderpath: str, diff_folderpath: str) -> None:

    first_folderpath_absolute = os.path.abspath(os.path.join(os.getcwd(), first_folderpath))
    second_folderpath_absolute = os.path.abspath(os.path.join(os.getcwd(), second_folderpath))
    diff_folderpath_absolute = os.path.abspath(os.path.join(os.getcwd(), diff_folderpath))

    for folder, _, original_files in os.walk(first_folderpath_absolute):
        for original_filename in original_files:
            if not original_filename.endswith(".png"):
                continue
            original_folder_relative_path = os.path.relpath(folder, first_folderpath_absolute)
            second_filepath = f"{second_folderpath_absolute}/{original_folder_relative_path}/{original_filename}"

            if not os.path.exists(second_filepath):
                print(f"Comparison file does not exist at {second_filepath}")
                continue

            diff_folderpath = f"{diff_folderpath_absolute}/{original_folder_relative_path}"
            compare_images(f"{folder}/{original_filename}", second_filepath, diff_folderpath, original_filename)


ap = argparse.ArgumentParser()
ap.add_argument("-f", "--first", required=True, help="first folder tree")
ap.add_argument("-s", "--second", required=True, help="second folder tree")
ap.add_argument("-d", "--diff", required=True, help="output path for diff folder tree")
args = vars(ap.parse_args())

compare_images_in_folder_trees(args["first"], args["second"], args["diff"])
