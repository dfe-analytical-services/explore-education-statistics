from logging import warning
import argparse
import os

from tables_and_charts import generate_releases, get_releases_by_url

ap = argparse.ArgumentParser()
ap.add_argument("-f", "--file", required=True,
                help="the datablocks csv to use to generate the robot test script from the template")
ap.add_argument("-t", "--target", required=True,
                help="the target filename of the generated robot test script")
args = vars(ap.parse_args())

datablocks_csv_filepath = args['file']
datablocks_csv_filename = os.path.basename(datablocks_csv_filepath)

generate_releases(datablocks_csv_filepath)

with open('visually_check_tables_and_charts.template.robot', 'r', encoding='utf-8-sig') as robot_template_file:

    test_template = robot_template_file.read()

    header, template_and_footer = test_template.split('{{test_case_template}}')
    test_case_template_section, footer = template_and_footer.split('{{/test_case_template}}')
    generated_test_script = header

    releases = get_releases_by_url().values()

    for release in releases:
        warning(release.url)
        generated_test_script += test_case_template_section.replace('{{release_url}}', release.url)

    generated_test_script += footer.replace('{{datablocks_csv_filename}}', datablocks_csv_filename)

    warning(generated_test_script)

    with open(args['target'], 'w', encoding='utf-8-sig') as generated_robot_file:
        generated_robot_file.write(generated_test_script)
