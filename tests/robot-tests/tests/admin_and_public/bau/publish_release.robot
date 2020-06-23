*** Settings ***
Resource    ../../libs/admin-common.robot

Force Tags  Admin  Local  Dev  AltersData

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Test Cases ***
Go to Create publication page for "UI tests topic" topic
    [Tags]  HappyPath
    environment variable should be set   RUN_IDENTIFIER
    user selects theme "Test theme" and topic "UI test topic %{RUN_IDENTIFIER}" from the admin dashboard
    user waits until page contains element    xpath://a[text()="Create new publication"]     60
    user checks page does not contain element   xpath://button[text()="UI tests - publish release %{RUN_IDENTIFIER}"]
    user clicks link  Create new publication
    user waits until page contains heading    Create new publication

Enter new publication title
    [Tags]  HappyPath
    user enters text into element  css:#createPublicationForm-publicationTitle   UI tests - publish release %{RUN_IDENTIFIER}
    user checks element is not visible  css:#createPublicationForm-publicationTitle-error

Select "Select a methodology later"
    [Tags]  HappyPath
    user waits until page contains element   xpath://label[text()="Choose an existing methodology"]
    user clicks element          xpath://label[text()="Select a methodology later"]

Select contact "Tingting Shu"
    [Tags]  HappyPath
    user selects from list by label  css:#createPublicationForm-selectedContactId   Tingting Shu - (Attainment statistics team)
    user checks summary list item "Email" should be "Attainment.STATISTICS@education.gov.uk"
    user checks summary list item "Telephone" should be "0370 000 2288"

User redirects to the dashboard when clicking the Create publication button
    [Tags]  HappyPath
    user clicks button   Create publication
    user waits until page contains element   xpath://span[text()="Welcome"]

Verify that new publication has been created
    [Tags]  HappyPath
    user selects theme "Test theme" and topic "UI test topic %{RUN_IDENTIFIER}" from the admin dashboard
    user waits until page contains element   xpath://button[text()="UI tests - publish release %{RUN_IDENTIFIER}"]
    user checks page contains accordion   UI tests - publish release %{RUN_IDENTIFIER}
    user opens accordion section  UI tests - publish release %{RUN_IDENTIFIER}
    user checks summary list item "Methodology" should be "No methodology assigned"
    user checks summary list item "Releases" should be "No releases created"

Create new release
    [Tags]   HappyPath
    user clicks element  css:[data-testid="Create new release link for UI tests - publish release %{RUN_IDENTIFIER}"]

Check release page has correct fields
    [Tags]  HappyPath
    user waits until page contains element  css:#releaseSummaryForm-timePeriodCoverage
    user waits until page contains element  css:#releaseSummaryForm-timePeriodCoverageStartYear
    user waits until page contains element  css:#releaseSummaryForm-scheduledPublishDate-day
    user waits until page contains element  css:#releaseSummaryForm-scheduledPublishDate-month
    user waits until page contains element  css:#releaseSummaryForm-scheduledPublishDate-year
    user waits until page contains element  css:#releaseSummaryForm-nextReleaseDate-day
    user waits until page contains element  css:#releaseSummaryForm-nextReleaseDate-month
    user waits until page contains element  css:#releaseSummaryForm-nextReleaseDate-year

User fills in form
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

    user selects from list by label  css:#releaseSummaryForm-timePeriodCoverage  Financial Year
    user enters text into element  css:#releaseSummaryForm-timePeriodCoverageStartYear  2000

    user enters text into element  css:#releaseSummaryForm-scheduledPublishDate-day  ${PUBLISH_DATE_DAY}
    user enters text into element  css:#releaseSummaryForm-scheduledPublishDate-month  ${PUBLISH_DATE_MONTH}
    user enters text into element  css:#releaseSummaryForm-scheduledPublishDate-year   ${PUBLISH_DATE_YEAR}

    user enters text into element  css:#releaseSummaryForm-nextReleaseDate-month  ${PUBLISH_DATE_MONTH}
    user enters text into element  css:#releaseSummaryForm-nextReleaseDate-year  ${NEXT_RELEASE_YEAR}
    user clicks element   css:[data-testid="National Statistics"]

Click Create new release button
    [Tags]   HappyPath
    user clicks button   Create new release
    user waits until page contains element  xpath://h1/span[text()="Edit release"]
    user checks page contains heading 1  UI tests - publish release %{RUN_IDENTIFIER}

Verify Release summary
    [Tags]  HappyPath
    user checks page contains element   xpath://li/a[text()="Release summary" and contains(@aria-current, 'page')]
    user checks page contains heading 2    Release summary
    user checks summary list item "Publication title" should be "UI tests - publish release %{RUN_IDENTIFIER}"
    user checks summary list item "Time period" should be "Financial Year"
    user checks summary list item "Release period" should be "2000"
    user checks summary list item "Lead statistician" should be "Tingting Shu"

    # EES-952
    #user checks summary list item "Scheduled release" should be "${PUBLISH_DATE_DAY} ${PUBLISH_DATE_MONTH_WORD} ${PUBLISH_DATE_YEAR}"

    user checks summary list item "Next release expected" should be "${PUBLISH_DATE_MONTH_WORD} ${NEXT_RELEASE_YEAR}"
    user checks summary list item "Release type" should be "National Statistics"

Go to Release status tab
    [Tags]  HappyPath
    user clicks link   Release status
    user waits until page contains element  xpath://h2[text()="Release Status"]
    user waits until element is enabled  xpath://button[text()="Update release status"]

Approve the release
    [Tags]  HappyPath
    user clicks button  Update release status

    user waits until page contains element  xpath://input[@data-testid="Approved for publication"]
    user waits until element is enabled   xpath://input[@data-testid="Approved for publication"]
    user clicks element   xpath://input[@data-testid="Approved for publication"]

    user clicks element   xpath://textarea[@id="releaseStatusForm-internalReleaseNote"]
    user presses keys  Approved by UI tests on ${PUBLISH_DATE_DAY} ${PUBLISH_DATE_MONTH_WORD} ${PUBLISH_DATE_YEAR}

    user clicks button   Update

Verify that the release is Scheduled
    [Tags]  HappyPath
    user waits until page contains element  css:#release-process-status-Scheduled   180

Trigger release on demand
    [Tags]  HappyPath
    ${CURRENT_URL}=  get location
    ${RELEASE_GUID}=  get release guid from release status page url  ${CURRENT_URL}
    user triggers release on demand  ${RELEASE_GUID}

Verify that the release is Started
    [Tags]  HappyPath
    # EES-1007 - Release process status doesn't automatically update
    user waits for release process status to be  Started   180
    #user waits until page contains element  css:#release-process-status-Started   600

Wait for release process status to be Complete
    [Tags]  HappyPath
    # EES-1007 - Release process status doesn't automatically update
    user waits for release process status to be  Complete    900
    #user waits until page contains element  css:#release-process-status-Complete   600

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
    user checks accordion is in position   National Statistics   1
    user checks accordion is in position   Contact us            2
