*** Settings ***
Resource    ../../libs/admin-common.robot

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
    [Tags]  HappyPath
    user checks element should contain   css:#draft-releases-tab   View draft releases (2)
    user clicks element   css:#draft-releases-tab
    user checks draft releases tab contains publication  Pupil absence in schools in England
    user checks draft releases tab contains publication  Permanent and fixed-period exclusions in England

Validate Analyst1 can see correct scheduled releases
    [Tags]  HappyPath
    user checks element should contain   css:#scheduled-releases-tab   View scheduled releases (0)
    user clicks element   css:#scheduled-releases-tab
    user checks element contains  css:#scheduled-releases   There are currently no scheduled releases

Validate Analyst1 cannot create a publiction
    [Tags]  HappyPath   UnderConstruction
