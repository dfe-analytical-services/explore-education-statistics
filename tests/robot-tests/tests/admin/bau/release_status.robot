*** Settings ***
Resource    ../../libs/admin-common.robot

Force Tags  Admin  Local  Dev  AltersData

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Variables ***
${TOPIC_NAME}        UI test topic %{RUN_IDENTIFIER}
${PUBLICATION_NAME}  UI tests - release status %{RUN_IDENTIFIER}

*** Test Cases ***
Create new publication for "UI tests topic" topic
    [Tags]  HappyPath
    environment variable should be set   RUN_IDENTIFIER
    user selects theme "Test theme" and topic "${TOPIC_NAME}" from the admin dashboard
    user waits until page contains link     Create new publication
    user checks page does not contain button   ${PUBLICATION_NAME}
    user clicks link  Create new publication
    user creates publication without methodology  ${PUBLICATION_NAME}   Tingting Shu - (Attainment statistics team)

Verify new publication
    [Tags]  HappyPath
    user selects theme "Test theme" and topic "${TOPIC_NAME}" from the admin dashboard
    user waits until page contains button  ${PUBLICATION_NAME}

Create new release
    [Tags]  HappyPath
    user opens accordion section  ${PUBLICATION_NAME}
    user clicks element  css:[data-testid="Create new release link for ${PUBLICATION_NAME}"]
    user creates release for publication  ${PUBLICATION_NAME}  Financial Year  3000

Verify release summary
    [Tags]  HappyPath
    user checks page contains element   xpath://li/a[text()="Release summary" and contains(@aria-current, 'page')]
    user waits until page contains heading 2  Release summary
    user checks summary list item "Publication title" should be "${PUBLICATION_NAME}"

Go to "Release status" tab
    [Tags]  HappyPath
    user clicks link   Release status
    user waits until page contains heading 2  Release status
    user waits until page contains button  Edit release status

Submit release for Higher Review
    [Tags]  HappyPath
    user clicks button  Edit release status
    user clicks element   css:input[data-testid="Ready for higher review"]
    user enters text into element  id:releaseStatusForm-internalReleaseNote     Submitted for Higher Review
    user enters text into element  id:releaseStatusForm-nextReleaseDate-month   12
    user enters text into element  id:releaseStatusForm-nextReleaseDate-year    3001
    user clicks button   Update status

Verify release status is Higher Review
    [Tags]  HappyPath
    user waits until page contains heading 2  Release status
    user checks summary list item "Current status" should be "Awaiting higher review"
    user checks summary list item "Scheduled release" should be "Not scheduled"
    user checks summary list item "Next release expected" should be "December 3001"

Approve release
    [Tags]  HappyPath
    user clicks button  Edit release status
    user waits until page contains heading 2  Edit release status

    user clicks element   css:input[data-testid="Approved for publication"]
    user enters text into element   id:releaseStatusForm-internalReleaseNote    Approved for release

    user clicks element  css:input[data-testid="On a specific date"]
    user enters text into element  id:releaseStatusForm-publishScheduled-day    1
    user enters text into element  id:releaseStatusForm-publishScheduled-month  12
    user enters text into element  id:releaseStatusForm-publishScheduled-year   3000

    user enters text into element  id:releaseStatusForm-nextReleaseDate-month   3
    user enters text into element  id:releaseStatusForm-nextReleaseDate-year    3002

    user clicks button   Update status

Verify release status is Approved
    [Tags]  HappyPath
    user waits until page contains heading 2  Release status
    user checks summary list item "Current status" should be "Approved"
    user checks summary list item "Scheduled release" should be "1 December 3000"
    user checks summary list item "Next release expected" should be "March 3002"
    user waits for release process status to be  Scheduled  180

Move release status back to Draft
    [Tags]  HappyPath
    user clicks button  Edit release status
    user clicks element   css:input[data-testid="In draft"]

    user enters text into element   id:releaseStatusForm-internalReleaseNote    Moved back to draft

    user enters text into element  id:releaseStatusForm-nextReleaseDate-month   1
    user enters text into element  id:releaseStatusForm-nextReleaseDate-year    3001

    user clicks button   Update status

Verify release status is Draft
    [Tags]  HappyPath
    user waits until page contains heading 2  Release status
    user checks summary list item "Current status" should be "Draft"
    user checks summary list item "Scheduled release" should be "Not scheduled"
    user checks summary list item "Next release expected" should be "January 3001"
