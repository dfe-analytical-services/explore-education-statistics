*** Settings ***
Resource    ../libs/admin-common.robot

Force Tags  Admin  Local  Dev  AltersData  UnderConstruction

Suite Setup       user signs in
Suite Teardown    user closes the browser

*** Test Cases ***
To do
    [Tags]  HappyPath

# Create publication
# Create release
# Upload subject
# Upload more subjects
# Upload ancillary files
