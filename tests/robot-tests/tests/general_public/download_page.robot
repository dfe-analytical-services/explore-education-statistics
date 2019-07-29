*** Settings ***
Resource    ../libs/library.robot

Force Tags  GeneralPublic

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Navigate to /download page
    [Tags]  HappyPath   UnderConstruction
    user checks page contains element   css:body
