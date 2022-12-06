import argparse
import csv
import os
import shutil

if __name__ == "__main__":
    ap = argparse.ArgumentParser(description="Generate test cases for visual testing of tables and charts")
    ap.add_argument(
        "-f",
        "--file",
        required=True,
        help="the datablocks csv to use to generate the robot test script from the template",
    )
    ap.add_argument("-t", "--target", required=True, help="the target filename of the generated robot test script")
    args = vars(ap.parse_args())

    datablocks_csv_filepath = args["file"]

    release_urls = set()

    with open(datablocks_csv_filepath, "r", encoding="utf-8-sig") as csv_file:
        csv_reader = csv.reader(csv_file, delimiter=",")
        # skip header
        next(csv_reader)
        for row in csv_reader:
            publication_slug = row[4]
            release_slug = row[2]
            release_urls.add(f"/find-statistics/{publication_slug}/{release_slug}")

    robot_tests_folder = os.path.join(os.getcwd(), "tests", "visual_testing")

    with open(
        f"{robot_tests_folder}/visually_check_tables_and_charts.template.robot", "r", encoding="utf-8-sig"
    ) as robot_template_file:

        test_template = robot_template_file.read()

        header, template_and_footer = test_template.split("{{test_case_template}}")
        test_case_template_section, footer = template_and_footer.split("{{/test_case_template}}")
        generated_test_script = header

        for release_url in release_urls:
            print(f"Generating test for release: {release_url}")
            generated_test_script += test_case_template_section.replace("{{release_url}}", release_url)

        target_robot_test_filename = args["target"]
        target_robot_test_csv_filename = f"{target_robot_test_filename}.csv"

        generated_test_script += footer.replace(
            "{{datablocks_csv_filename}}", f"tests/visual_testing/{target_robot_test_csv_filename}"
        )

        print(generated_test_script)

        with open(
            f"{robot_tests_folder}/{target_robot_test_filename}", "w", encoding="utf-8-sig"
        ) as generated_robot_file:
            generated_robot_file.write(generated_test_script)

        shutil.copy(datablocks_csv_filepath, f"{robot_tests_folder}/{target_robot_test_csv_filename}")
