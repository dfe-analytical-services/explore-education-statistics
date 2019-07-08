*** Settings ***
Resource    ../libs/library.robot

Force Tags  GeneralPublic

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Navigate to Pupil absence in schools in England methodology page
    [Tags]  HappyPath
    user goes to url  ${url}/methodologies
    user opens accordion section  Pupils and schools
    user opens details dropdown   Pupil absence
    user clicks link    Pupil absence statistics: methodology
    user waits until page contains element   xpath://h1[text()="Pupil absence statistics: methodology"]
