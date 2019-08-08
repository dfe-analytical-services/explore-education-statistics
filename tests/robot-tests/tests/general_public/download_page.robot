*** Settings ***
Resource    ../libs/library.robot

Force Tags  GeneralPublic

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Navigate to /download-data page
    [Tags]  HappyPath
    environment variable should be set   PUBLIC_URL
    user goes to url   %{PUBLIC_URL}
    user waits until page contains element   xpath://h1[text()="Choose how to explore our statistics and data"]
    user clicks link   Download data files
    user waits until page contains element   xpath://h1[text()="Download data files"]
    user checks url contains   %{PUBLIC_URL}/download-data

Validate Pupils and schools contains correct details components
    [Tags]  HappyPath
    user checks page contains accordion   Pupils and schools
    user opens accordion section   Pupils and schools

    user checks accordion section contains details  Pupils and schools   Pupil absence
    user checks accordion section contains details  Pupils and schools   School applications
    user checks accordion section contains details  Pupils and schools   Exclusions

Validate Pupil absence data downloads are avaialble
    [Tags]  HappyPath   NotAgainstLocal   UnderConstruction
    environment variable should be set   DATA_API_URL
    #user checks page contains link with text and url  Absence by characteristic   %{DATA_API_URL}/download/pupil-absence-in-schools-in-england/2016-17/absence_by_characteristic.csv
    user opens details dropdown  Pupil absence
    user checks details dropdown contains download text  Pupil absence  Absence by characteristic (csv, 58 Mb)