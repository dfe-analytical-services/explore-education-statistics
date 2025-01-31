import argparse
import csv
import os

if __name__ == "__main__":
    ap = argparse.ArgumentParser()
    ap.add_argument(
        "-f",
        "--file",
        required=True,
        help="the permalink urls csv to use to generate the robot test script from the template",
    )
    ap.add_argument("-t", "--target", required=True, help="the target filename of the generated robot test script")
    args = vars(ap.parse_args())

    permalinks_csv_filepath = args["file"]

    permalink_ids: list[str] = []

    with open(permalinks_csv_filepath, "r", encoding="utf-8-sig") as csv_file:
        csv_reader = csv.reader(csv_file, delimiter=",")
        next(csv_reader)
        for row in csv_reader:
            permalink_ids.append(row[0])

    robot_tests_folder = os.path.join(os.getcwd(), "tests", "visual_testing")

    with open(
        f"{robot_tests_folder}/visually_check_permalinks.template.robot", "r", encoding="utf-8-sig"
    ) as robot_template_file:
        test_template = robot_template_file.read()

        header, template_and_footer = test_template.split("{{test_case_template}}")
        test_case_template_section, footer = template_and_footer.split("{{/test_case_template}}")
        generated_test_script = header

        for permalink_id in permalink_ids:
            generated_test_script += test_case_template_section.replace("{{permalink_id}}", permalink_id)

        print(generated_test_script)

        target_filename = args["target"]

        with open(f"{robot_tests_folder}/{target_filename}", "w", encoding="utf-8-sig") as generated_robot_file:
            generated_robot_file.write(generated_test_script)
