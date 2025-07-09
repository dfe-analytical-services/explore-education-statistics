*** Comments ***
#
# This test suite tests the keywords that form the basis of our seed data generation mechanism. Keeping these
# keywords in regular test runs prevents them from becoming stale over time in between them being used to
# seed / reseed environments.
#
# Because these tests set up data under the standard Test Theme set up for each unique test run, they will be
# torn down again afterwards.
#


*** Settings ***
Library             ../libs/admin_api.py
Resource            ../libs/admin-common.robot
Resource            seed_data_common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Local    Dev


*** Variables ***
${PUBLICATION_NAME}     Seed data mechanism publication %{RUN_IDENTIFIER}


*** Test Cases ***
Create publication and releases
    ${PUBLICATION_ID}=    user creates test publication via api
    ...    ${PUBLICATION_NAME}

    user creates a fully populated published release
    ...    ${PUBLICATION_ID}
    ...    ${PUBLICATION_NAME}

    user gives analyst publication owner access
    ...    ${PUBLICATION_NAME}

    user gives release access to analyst
    ...    ${PUBLICATION_NAME}
    ...    Academic year 2025/26
    ...    Approver
