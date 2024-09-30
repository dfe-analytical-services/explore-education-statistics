*** Settings ***
Resource            tables_and_charts.robot
Library             tables_and_charts.py

Force Tags          VisualTesting    GeneralPublic    Local    Dev    Test    Preprod

Suite Setup         do suite setup
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Test Cases ***
{{test_case_template}}

Check release {{release_url}}
    ${release}=    get release by url    {{release_url}}
    Check release    ${release}

{{/test_case_template}}


*** Keywords ***
do suite setup
    generate releases    {{datablocks_csv_filename}}
    user opens the browser
