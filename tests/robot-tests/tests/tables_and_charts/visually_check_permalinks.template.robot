*** Settings ***
Resource            ../libs/public-common.robot
Resource            permalinks.robot

Force Tags          GeneralPublic    Local    Dev    Test    Preprod

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

*** Test Cases ***
{{test_case_template}}

Check permalink {{permalink_id}}
    Check permalink with id    {{permalink_id}}

{{/test_case_template}}
