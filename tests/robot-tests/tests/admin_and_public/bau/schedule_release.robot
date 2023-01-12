*** Settings ***
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot
Resource            ../../libs/admin/manage-content-common.robot
Resource            ../../libs/public-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser

Force Tags          Admin    Local    Dev    AltersData


*** Variables ***
${PUBLICATION_NAME}=    UI tests - schedule release %{RUN_IDENTIFIER}
${RELEASE_TYPE}         Calendar Year 2001


*** Test Cases ***
Create new publication and release via API
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user create test release via api    ${PUBLICATION_ID}    CY    2001    AdHocStatistics

    user adds publication role to user via api
    ...    EES-test.ANALYST1@education.gov.uk
    ...    ${PUBLICATION_ID}
    ...    Approver

Approve release
    user signs in as analyst1
    user navigates to draft release page from dashboard
    ...    ${PUBLICATION_NAME}
    ...    ${RELEASE_TYPE}
    user clicks link    Sign off
    user approves release for scheduled release    0
    user waits for scheduled release to be published

Verify newly published release is on Find Statistics page
    user checks publication is on find statistics page    ${PUBLICATION_NAME}
