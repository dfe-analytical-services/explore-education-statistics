*** Settings ***
Resource    ../libs/library.robot

Force Tags  GeneralPublic

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Navigate to glossary page
    [Tags]  HappyPath
    environment variable should be set   PUBLIC_URL
    user goes to url   %{PUBLIC_URL}
    user waits until page contains heading  Choose how to explore our statistics and data

    user clicks link   Education statistics: glossary
    user waits until page contains heading   Glossary
    user checks url contains  %{PUBLIC_URL}/glossary

Validate glossary accordion sections
    [Tags]   HappyPath
    user checks accordion is in position  A   1
    user checks accordion is in position  B   2
    user checks accordion is in position  C   3
    user checks accordion is in position  D   4
    user checks accordion is in position  Z   26

Search for Pupil referral unit
    [Tags]  HappyPath
    user clicks element   css:#pageSearchForm-input
    user verifies accordion is closed  P

    user presses keys  Pupil referral unit
    user checks element contains   xpath://*[@id="pageSearchForm-resultsLabel"]   Found 1 result
    user clicks element   css:#pageSearchForm-option-0

    user verifies accordion is open  P
    user checks element is visible  css:#pupil-referral-unit
    user checks page contains   An alternative education provision specifically organised to provide education for children who are not able to attend school and may not otherwise receive a suitable education.
