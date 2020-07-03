*** Settings ***
Resource    ../../libs/admin-common.robot

Force Tags  Admin  Local  Dev  AltersData

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Test Cases ***
Create new publication for "UI tests topic" topic
    [Tags]  HappyPath
    environment variable should be set   RUN_IDENTIFIER
    user selects theme "Test theme" and topic "UI test topic %{RUN_IDENTIFIER}" from the admin dashboard
    user waits until page contains link    Create new publication
    user checks page does not contain button   UI tests - publish release %{RUN_IDENTIFIER}
    user clicks link  Create new publication
    user creates publication  UI tests - publish release %{RUN_IDENTIFIER}   Test methodology   Tingting Shu - (Attainment statistics team)

Verify new publication
    [Tags]  HappyPath
    user selects theme "Test theme" and topic "UI test topic %{RUN_IDENTIFIER}" from the admin dashboard
    user waits until page contains button  UI tests - publish release %{RUN_IDENTIFIER}
    user checks page contains accordion   UI tests - publish release %{RUN_IDENTIFIER}
    user opens accordion section  UI tests - publish release %{RUN_IDENTIFIER}
    user checks summary list item "Methodology" should be "Test methodology"
    user checks summary list item "Releases" should be "No releases created"

Create new release
    [Tags]   HappyPath
    user clicks element  css:[data-testid="Create new release link for UI tests - publish release %{RUN_IDENTIFIER}"]
    user creates release for publication  UI tests - publish release %{RUN_IDENTIFIER}  Financial Year  2000

Verify release summary
    [Tags]  HappyPath
    user checks page contains element   xpath://li/a[text()="Release summary" and contains(@aria-current, 'page')]
    user waits until page contains heading 2  Release summary
    user checks summary list item "Publication title" should be "UI tests - publish release %{RUN_IDENTIFIER}"
    user checks summary list item "Time period" should be "Financial Year"
    user checks summary list item "Release period" should be "2000"
    user checks summary list item "Lead statistician" should be "Tingting Shu"
    user checks summary list item "Scheduled release" should be "Not scheduled"
    user checks summary list item "Next release expected" should be "Not set"
    user checks summary list item "Release type" should be "National Statistics"

Go to "Release status" tab
    [Tags]  HappyPath
    user clicks link   Release status
    user waits until page contains heading 2  Release status
    user waits until page contains button  Edit release status

Approve the release
    [Tags]  HappyPath
    ${PUBLISH_DATE_DAY}=  get datetime  %d
    ${PUBLISH_DATE_MONTH}=  get datetime  %m
    ${PUBLISH_DATE_MONTH_WORD}=  get datetime  %B
    ${PUBLISH_DATE_YEAR}=  get datetime  %Y
    ${NEXT_RELEASE_YEAR}=  evaluate  str(int(${PUBLISH_DATE_YEAR}) + 1)
    set suite variable  ${PUBLISH_DATE_DAY}
    set suite variable  ${PUBLISH_DATE_MONTH}
    set suite variable  ${PUBLISH_DATE_MONTH_WORD}
    set suite variable  ${PUBLISH_DATE_YEAR}
    set suite variable  ${NEXT_RELEASE_YEAR}

    user clicks button  Edit release status
    user waits until page contains heading 2  Edit release status

    user clicks element   css:input[data-testid="Approved for publication"]
    user clicks element   id:releaseStatusForm-internalReleaseNote
    user presses keys  Approved by UI tests on ${PUBLISH_DATE_DAY} ${PUBLISH_DATE_MONTH_WORD} ${PUBLISH_DATE_YEAR}
    user clicks element  css:input[data-testid="On a specific date"]

    user enters text into element  id:releaseStatusForm-publishScheduled-day  ${PUBLISH_DATE_DAY}
    user enters text into element  id:releaseStatusForm-publishScheduled-month  ${PUBLISH_DATE_MONTH}
    user enters text into element  id:releaseStatusForm-publishScheduled-year   ${PUBLISH_DATE_YEAR}

    user enters text into element  id:releaseStatusForm-nextReleaseDate-month  ${PUBLISH_DATE_MONTH}
    user enters text into element  id:releaseStatusForm-nextReleaseDate-year  ${NEXT_RELEASE_YEAR}

    user clicks button   Update status

Verify that the release is Scheduled
    [Tags]  HappyPath
    user waits until page contains heading 2  Release status
    user waits until page contains element  id:release-process-status-Scheduled   180
    # EES-1029
    #user checks summary list item "Scheduled release" should be "${PUBLISH_DATE_DAY} ${PUBLISH_DATE_MONTH_WORD} ${PUBLISH_DATE_YEAR}"
    user checks summary list item "Next release expected" should be "${PUBLISH_DATE_MONTH_WORD} ${NEXT_RELEASE_YEAR}"

Trigger release on demand
    [Tags]  HappyPath
    ${CURRENT_URL}=  get location
    ${RELEASE_GUID}=  get release guid from release status page url  ${CURRENT_URL}
    user triggers release on demand  ${RELEASE_GUID}

Wait for release process status to be Complete
    [Tags]  HappyPath
    # EES-1007 - Release process status doesn't automatically update
    user waits for release process status to be  Complete    900

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
    user checks accordion section contains text   Test theme   UI test topic %{RUN_IDENTIFIER}

    user opens details dropdown  UI test topic %{RUN_IDENTIFIER}
    user checks details dropdown contains publication    UI test topic %{RUN_IDENTIFIER}    UI tests - publish release %{RUN_IDENTIFIER}
    user checks publication bullet contains link   UI tests - publish release %{RUN_IDENTIFIER}    View statistics and data
    user checks publication bullet contains link   UI tests - publish release %{RUN_IDENTIFIER}    Create your own tables online
    user checks publication bullet does not contain link  UI tests - publish release %{RUN_IDENTIFIER}   Statistics at DfE

Navigate to newly published release page
    [Tags]  HappyPath
    user clicks element   css:[data-testid="view-stats-ui-tests-publish-release-%{RUN_IDENTIFIER}"]
    user waits until page contains heading  UI tests - publish release %{RUN_IDENTIFIER}

Verify release URL and page caption
    [Tags]  HappyPath
    user checks url contains  %{PUBLIC_URL}/find-statistics/ui-tests-publish-release-%{RUN_IDENTIFIER}
    user checks element contains  css:[data-testid="page-title-caption"]  Financial Year 2000-01

Verify publish and update dates
    [Tags]  HappyPath   Failing
    [Documentation]   EES-952
    user checks element contains  css:[data-testid="published-date"]   ${PUBLISH_DATE_DAY} ${PUBLISH_DATE_MONTH_WORD} ${PUBLISH_DATE_YEAR}
    user checks element contains  xpath://*[@data-testid="next-update"]/strong/time   ${PUBLISH_DATE_MONTH_WORD} ${NEXT_RELEASE_YEAR}

Verify accordions are correct
    [Tags]  HappyPath
    user checks accordion is in position   Methodology  1
    user checks accordion is in position   National Statistics  2
    user checks accordion is in position   Contact us  3
