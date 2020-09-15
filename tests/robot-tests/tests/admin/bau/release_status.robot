*** Settings ***
Resource    ../../libs/admin-common.robot

Force Tags  Admin  Local  Dev  AltersData

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Variables ***
${TOPIC_NAME}        %{TEST_TOPIC_NAME}
${PUBLICATION_NAME}  UI tests - release status %{RUN_IDENTIFIER}

*** Test Cases ***
Create new publication for "UI tests topic" topic
    [Tags]  HappyPath
    user selects theme "Test theme" and topic "${TOPIC_NAME}" from the admin dashboard
    user waits until page contains link     Create new publication
    user checks page does not contain button   ${PUBLICATION_NAME}
    user clicks link  Create new publication
    user creates publication    ${PUBLICATION_NAME}

Verify new publication
    [Tags]  HappyPath
    user selects theme "Test theme" and topic "${TOPIC_NAME}" from the admin dashboard
    user waits until page contains button  ${PUBLICATION_NAME}

Create new release
    [Tags]  HappyPath
    user opens accordion section  ${PUBLICATION_NAME}
    user clicks testid element  Create new release link for ${PUBLICATION_NAME}
    user creates release for publication  ${PUBLICATION_NAME}  Financial Year  3000

Verify release summary
    [Tags]  HappyPath
    user checks page contains element   xpath://li/a[text()="Release summary" and contains(@aria-current, 'page')]
    user waits until h2 is visible  Release summary
    user checks summary list contains  Publication title  ${PUBLICATION_NAME}

Go to "Release status" tab
    [Tags]  HappyPath
    user clicks link   Release status
    user waits until h2 is visible  Release status
    user waits until page contains button  Edit release status

Submit release for Higher Review
    [Tags]  HappyPath
    user clicks button  Edit release status
    user clicks radio  Ready for higher review
    user enters text into element  id:releaseStatusForm-internalReleaseNote     Submitted for Higher Review
    user enters text into element  id:releaseStatusForm-nextReleaseDate-month   12
    user enters text into element  id:releaseStatusForm-nextReleaseDate-year    3001
    user clicks button   Update status

Verify release status is Higher Review
    [Tags]  HappyPath
    user waits until h2 is visible  Release status
    user checks summary list contains  Current status  Awaiting higher review
    user checks summary list contains  Scheduled release  Not scheduled
    user checks summary list contains  Next release expected  December 3001

Approve release
    [Tags]  HappyPath
    user clicks button  Edit release status
    user waits until h2 is visible  Edit release status

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
    user waits until h2 is visible  Release status
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
    user waits until h2 is visible  Release status
    user checks summary list contains  Current status  Draft
    user checks summary list contains  Scheduled release  Not scheduled
    user checks summary list contains  Next release expected  January 3001
