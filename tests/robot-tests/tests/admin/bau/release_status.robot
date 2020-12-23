*** Settings ***
Resource    ../../libs/admin-common.robot
Library     ../../libs/admin_api.py

Force Tags  Admin  Local  Dev  AltersData

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Variables ***
${TOPIC_NAME}        %{TEST_TOPIC_NAME}
${PUBLICATION_NAME}  UI tests - release status %{RUN_IDENTIFIER}

*** Keywords ***
user checks checklist errors contains
    [Arguments]  ${text}
    user waits until element contains  css:[data-testid="releaseChecklist-errors"]  ${text}

user checks checklist warnings contains
    [Arguments]  ${text}
    user waits until element contains  css:[data-testid="releaseChecklist-warnings"]  ${text}

user checks checklist errors contains link
    [Arguments]  ${text}
    user waits until parent contains element  css:[data-testid="releaseChecklist-errors"]  link:${text}

user checks checklist warnings contains link
    [Arguments]  ${text}
    user waits until parent contains element  css:[data-testid="releaseChecklist-warnings"]  link:${text}

*** Test Cases ***
Create new publication and release via API
    [Tags]  HappyPath
    ${PUBLICATION_ID}=  user creates test publication via api   ${PUBLICATION_NAME}
    user create test release via api  ${PUBLICATION_ID}   FY    3000

Go to release sign off page
    [Tags]  HappyPath
    user navigates to release summary from admin dashboard  ${PUBLICATION_NAME}  Financial Year 3000-01 (not Live)
    user clicks link  Sign off
    user waits until h2 is visible  Sign off

Verify release checklist with errors
    [Tags]  HappyPath
    user clicks button  Edit release status
    user waits until h2 is visible  Edit release status

    user waits until page contains testid  releaseChecklist-errors
    user checks checklist errors contains  1 issue that must be resolved before this release can be published
    user checks checklist errors contains link  Public pre-release access list is required

    user waits until page contains testid  releaseChecklist-warnings
    user checks checklist warnings contains  3 potential issues that do not need to be resolved to publish this release
    user checks checklist warnings contains link  No methodology attached to publication
    user checks checklist warnings contains link  No next release expected date
    user checks checklist warnings contains link  No data files uploaded

    user checks page does not contain testid  releaseChecklist-success

Submit release for Higher Review
    [Tags]  HappyPath
    user clicks radio  Ready for higher review
    user enters text into element  id:releaseStatusForm-internalReleaseNote     Submitted for Higher Review
    user enters text into element  id:releaseStatusForm-nextReleaseDate-month   12
    user enters text into element  id:releaseStatusForm-nextReleaseDate-year    3001
    user clicks button   Update status

Verify release status is Higher Review
    [Tags]  HappyPath
    user waits until h2 is visible  Sign off
    user checks summary list contains  Current status  Awaiting higher review
    user checks summary list contains  Scheduled release  Not scheduled
    user checks summary list contains  Next release expected  December 3001

Verify updated release checklist
    [Tags]  HappyPath
    user clicks button  Edit release status
    user waits until h2 is visible  Edit release status

    user waits until page contains testid  releaseChecklist-errors
    user checks checklist errors contains  1 issue that must be resolved before this release can be published
    user checks checklist errors contains link  Public pre-release access list is required

    user waits until page contains testid  releaseChecklist-warnings
    user checks checklist warnings contains  2 potential issues that do not need to be resolved to publish this release
    user checks checklist warnings contains link  No methodology attached to publication
    user checks checklist warnings contains link  No data files uploaded

    user checks page does not contain testid  releaseChecklist-success

Add public prerelease access list via release checklist
    [Tags]  HappyPath
    user clicks link  Public pre-release access list is required
    user creates public prerelease access list   Test public access list

Verify release checklist does not contain errors
    [Tags]  HappyPath
    user clicks link  Sign off
    user clicks button  Edit release status
    user waits until h2 is visible  Edit release status

    user waits until page contains testid  releaseChecklist-warnings
    user checks checklist warnings contains  2 potential issues that do not need to be resolved to publish this release
    user checks checklist warnings contains link  No methodology attached to publication
    user checks checklist warnings contains link  No data files uploaded

    user checks page does not contain testid  releaseChecklist-errors
    user checks page does not contain testid  releaseChecklist-success

Approve release
    [Tags]  HappyPath
    user clicks radio   Approved for publication
    user enters text into element   id:releaseStatusForm-internalReleaseNote    Approved for release

    user clicks radio  On a specific date
    user enters text into element  id:releaseStatusForm-publishScheduled-day    1
    user enters text into element  id:releaseStatusForm-publishScheduled-month  12
    user enters text into element  id:releaseStatusForm-publishScheduled-year   3000

    user enters text into element  id:releaseStatusForm-nextReleaseDate-month   3
    user enters text into element  id:releaseStatusForm-nextReleaseDate-year    3002

    user clicks button   Update status

Verify release status is Approved
    [Tags]  HappyPath
    user waits until h2 is visible  Sign off
    user checks summary list contains  Current status  Approved
    user checks summary list contains  Scheduled release  1 December 3000
    user checks summary list contains  Next release expected  March 3002
    user waits for release process status to be  Scheduled  180

Move release status back to Draft
    [Tags]  HappyPath
    user clicks button  Edit release status
    user clicks radio  In draft

    user enters text into element   id:releaseStatusForm-internalReleaseNote    Moved back to draft

    user enters text into element  id:releaseStatusForm-nextReleaseDate-month   1
    user enters text into element  id:releaseStatusForm-nextReleaseDate-year    3001

    user clicks button   Update status

Verify release status is Draft
    [Tags]  HappyPath
    user waits until h2 is visible  Sign off
    user checks summary list contains  Current status  Draft
    user checks summary list contains  Scheduled release  Not scheduled
    user checks summary list contains  Next release expected  January 3001
