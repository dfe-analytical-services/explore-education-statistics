*** Settings ***
Resource    ../../libs/common.robot

Force Tags  Admin  Local  Dev

Suite Setup       user signs in as analyst1
Suite Teardown    user closes the browser

*** Test Cases ***
Validate Analyst1 can see correct Themes and Topics
    [Tags]  HappyPath
    user checks list contains x elements   css:#selectTheme   1
    user checks list contains label   css:#selectTheme   Pupils and schools
    user checks list contains x elements   css:#selectTopic   2
    user checks list contains label   css:#selectTopic   Exclusions
    user checks list contains label   css:#selectTopic   Pupil absence

Validate Analyst1 can see correct draft releases
    [Tags]  HappyPath   UnderConstruction

Validate Analyst1 can see correct scheduled releases
    [Tags]  HappyPath   UnderConstruction

Validate Analyst1 cannot create a publiction
    [Tags]  HappyPath   UnderConstruction
