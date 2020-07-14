*** Settings ***
Resource    ../../libs/admin-common.robot

Force Tags  Admin  Local  Dev  AltersData

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Variables ***
${TOPIC_NAME}        UI test topic %{RUN_IDENTIFIER}
${PUBLICATION_NAME}  UI tests - publish release %{RUN_IDENTIFIER}

*** Test Cases ***
Create new publication for "UI tests topic" topic
    [Tags]  HappyPath
    environment variable should be set   RUN_IDENTIFIER
    user selects theme "Test theme" and topic "${TOPIC_NAME}" from the admin dashboard
    user waits until page contains link    Create new publication
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

Approve release
    [Tags]  HappyPath
    ${PUBLISH_DATE_DAY}=  get datetime  %d
    ${PUBLISH_DATE_MONTH}=  get datetime  %m
    ${PUBLISH_DATE_MONTH_WORD}=  get datetime  %B
    ${PUBLISH_DATE_YEAR}=  get datetime  %Y
    set suite variable  ${PUBLISH_DATE_DAY}
    set suite variable  ${PUBLISH_DATE_MONTH}
    set suite variable  ${PUBLISH_DATE_MONTH_WORD}
    set suite variable  ${PUBLISH_DATE_YEAR}

    user clicks button  Edit release status
    user waits until page contains heading 2  Edit release status

    user clicks element   css:input[data-testid="Approved for publication"]
    user enters text into element  id:releaseStatusForm-internalReleaseNote  Approved by UI tests
    user clicks element  css:input[data-testid="As soon as possible"]
    user enters text into element  id:releaseStatusForm-nextReleaseDate-month   12
    user enters text into element  id:releaseStatusForm-nextReleaseDate-year    3001

    user clicks button   Update status

Verify release is scheduled
    [Tags]  HappyPath
    user waits until page contains heading 2  Release status
    user checks summary list item "Current status" should be "Approved"
    user checks summary list item "Scheduled release" should be "${PUBLISH_DATE_DAY} ${PUBLISH_DATE_MONTH_WORD} ${PUBLISH_DATE_YEAR}"
    user checks summary list item "Next release expected" should be "December 3001"

Wait for release process status to be Complete
    [Tags]  HappyPath
    # EES-1007 - Release process status doesn't automatically update
    user waits for release process status to be  Complete    900
    user checks page does not contain button  Edit release status

User goes to public Find Statistics page
    [Tags]  HappyPath
    environment variable should be set   PUBLIC_URL
    user goes to url   %{PUBLIC_URL}/find-statistics
    user waits until page contains heading  Find statistics and data
    user waits for page to finish loading

Verify newly published release is on Find Statistics page
    [Tags]  HappyPath
    user checks page contains accordion   Test theme
    user opens accordion section  Test theme
    user checks accordion section contains text   Test theme   ${TOPIC_NAME}

    user opens details dropdown  ${TOPIC_NAME}
    user checks details dropdown contains publication    ${TOPIC_NAME}  ${PUBLICATION_NAME}
    user checks publication bullet contains link   ${PUBLICATION_NAME}  View statistics and data
    user checks publication bullet contains link   ${PUBLICATION_NAME}  Create your own tables online
    user checks publication bullet does not contain link  ${PUBLICATION_NAME}   Statistics at DfE

Navigate to newly published release page
    [Tags]  HappyPath
    user clicks element   css:[data-testid="view-stats-ui-tests-publish-release-%{RUN_IDENTIFIER}"]
    user waits until page contains heading  ${PUBLICATION_NAME}

Verify release URL and page caption
    [Tags]  HappyPath
    user checks url contains  %{PUBLIC_URL}/find-statistics/ui-tests-publish-release-%{RUN_IDENTIFIER}
    user checks element contains  css:[data-testid="page-title-caption"]  Financial Year 3000-01

Verify publish and update dates
    [Tags]  HappyPath
    user checks element contains  css:[data-testid="published-date"]   ${PUBLISH_DATE_DAY} ${PUBLISH_DATE_MONTH_WORD} ${PUBLISH_DATE_YEAR}
    user checks element contains  css:[data-testid="next-update"] time   December 3001

Verify accordions are correct
    [Tags]  HappyPath
    user checks accordion is in position   National Statistics  1
    user checks accordion is in position   Contact us  2
