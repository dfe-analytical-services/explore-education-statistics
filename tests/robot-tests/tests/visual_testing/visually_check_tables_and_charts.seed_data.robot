*** Settings ***
Resource            tables_and_charts.robot
Library             tables_and_charts.py

# Failing tag relates to https://dfedigital.atlassian.net/browse/EES-6892
Force Tags          GeneralPublic    Local    Failing

Suite Setup         do suite setup
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Test Cases ***
Check release /find-statistics/seed-publication-pupil-absence-in-schools-in-england/2016-17
    ${release}=    get release by url    /find-statistics/seed-publication-pupil-absence-in-schools-in-england/2016-17
    Check release    ${release}

Check release /find-statistics/seed-publication-permanent-and-fixed-period-exclusions-in-england/2016-17
    ${release}=    get release by url
    ...    /find-statistics/seed-publication-permanent-and-fixed-period-exclusions-in-england/2016-17
    Check release    ${release}


*** Keywords ***
do suite setup
    generate releases    tests/visual_testing/visually_check_tables_and_charts.seed_data.robot.csv
    user opens the browser
