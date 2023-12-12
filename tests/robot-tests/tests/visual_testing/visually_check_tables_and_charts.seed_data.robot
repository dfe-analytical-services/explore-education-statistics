*** Settings ***
Library             tables_and_charts.py
Resource            tables_and_charts.robot
Resource            ../seed_data/seed_data_constants.robot

Force Tags          GeneralPublic    Local    Dev

Suite Setup         do suite setup
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Test Cases ***
Check release /find-statistics/permanent-and-fixed-period-exclusions-in-england/2016-17
    ${release}=    get release by url    /find-statistics/permanent-and-fixed-period-exclusions-in-england/2016-17
    Check release    ${release}

Check release ${SEED_DATA_THEME_1_PUBLICATION_1_RELATIVE_URL}/2016-17
    ${release}=    get release by url    ${SEED_DATA_THEME_1_PUBLICATION_1_RELATIVE_URL}/2016-17
    Check release    ${release}


*** Keywords ***
do suite setup
    generate releases    tests/visual_testing/visually_check_tables_and_charts.seed_data.robot.csv
    user opens the browser
