*** Settings ***
Resource    ../libs/admin-common.robot

Force Tags  Admin  Local  Dev

Suite Setup       user signs in
Suite Teardown    user closes the browser

*** Test Cases ***
Create Datablock test publication
    [Tags]  HappyPath
    user selects theme "Test theme" and topic "Test topic" from the admin dashboard
    user waits until page contains element    xpath://a[text()="Create new publication"]     60

Navigate to Manage data tab
    [Tags]  HappyPath
    user checks page contains accordion  Test publication
    user opens accordion section  Test publication
    user opens details section  Academic Year, 2020 to 2021 (not Live)
    user clicks element   xpath://a[@data-testid="Edit release link for Test publication, Academic Year, 2020 to 2021 (not Live)"]
    user waits until page contains heading   Test publication
    user clicks element    xpath://li/a[text()="Manage data"]

Upload subject
    [Tags]   HappyPath
    user enters text into element  css:#dataFileUploadForm-subjectTitle   UI test subject
    choose file   css=#dataFileUploadForm-dataFile     ${CURDIR}${/}files${/}upload-file-test.csv
    choose file   css=#dataFileUploadForm-metadataFile  ${CURDIR}${/}files${/}upload-file-test.meta.csv
    user clicks element   xpath://button[text()="Upload data files"]

Does this happen?
    [Tags]  HappyPath
    sleep   10000