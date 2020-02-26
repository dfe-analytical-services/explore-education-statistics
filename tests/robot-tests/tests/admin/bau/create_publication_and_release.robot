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
    user checks page does not contain element   xpath://button[text()="UI tests - create publication %{RUN_IDENTIFIER}"]
    user clicks link  Create new publication
    user waits until page contains heading    Create new publication

Select an existing methodology
    [Tags]  HappyPath
    user waits until page contains element   xpath://label[text()="Choose an existing methodology"]
    user clicks element          xpath://label[text()="Choose an existing methodology"]
    user checks element is visible    xpath://label[text()="Select methodology"]
    user selects from list by label  css:#createPublicationForm-selectedMethodologyId   Test methodology

Select contact "Sean Gibson"
    [Tags]  HappyPath
    user selects from list by label  css:#createPublicationForm-selectedContactId   Sean Gibson
    user checks summary list item "Email" should be "sen.statistics@education.gov.uk"
    user checks summary list item "Telephone" should be "01325340987"

Error message appears when user clicks continue is title is empty
    [Tags]  HappyPath
    user checks element is not visible  css:#createPublicationForm-publicationTitle-error
    user clicks button   Create publication
    user checks element is visible  css:#createPublicationForm-publicationTitle-error

Enter new publication title
    [Tags]  HappyPath
    user enters text into element  css:#createPublicationForm-publicationTitle   UI tests - create publication %{RUN_IDENTIFIER}
    user checks element is not visible  css:#createPublicationForm-publicationTitle-error

User redirects to the dashboard when clicking the Create publication button
    [Tags]  HappyPath
    user clicks button   Create publication
    user waits until page contains element   xpath://span[text()="Welcome"]

Verify that new publication has been created
    [Tags]  HappyPath
    user selects theme "Test theme" and topic "UI test topic %{RUN_IDENTIFIER}" from the admin dashboard
    user waits until page contains element   xpath://button[text()="UI tests - create publication %{RUN_IDENTIFIER}"]
    user checks page contains accordion   UI tests - create publication %{RUN_IDENTIFIER}
    user opens accordion section  UI tests - create publication %{RUN_IDENTIFIER}
    user checks page contains element   xpath://div/dt[text()="Methodology"]/../dd/a[text()="Test methodology"]
    user checks summary list item "Releases" should be "No releases created"

Create new release
    [Tags]   HappyPath
    user clicks element  css:[data-testid="Create new release link for UI tests - create publication %{RUN_IDENTIFIER}"]

Check release page has correct fields
    [Tags]  HappyPath
    user waits until page contains element  css:#releaseSummaryForm-timePeriodCoverage
    user waits until page contains element  css:#releaseSummaryForm-timePeriodCoverageStartYear
    user waits until page contains element  css:[id="scheduledPublishDate.day"]
    user waits until page contains element  css:[id="scheduledPublishDate.month"]
    user waits until page contains element  css:[id="scheduledPublishDate.year"]
    user waits until page contains element  css:[id="nextReleaseDate.day"]
    user waits until page contains element  css:[id="nextReleaseDate.month"]
    user waits until page contains element  css:[id="nextReleaseDate.year"]

User fills in form
    [Tags]  HappyPath
    user selects from list by label  css:#releaseSummaryForm-timePeriodCoverage  Spring Term
    user enters text into element  css:#releaseSummaryForm-timePeriodCoverageStartYear  2025
    user enters text into element  css:[id="scheduledPublishDate.day"]  24
    user enters text into element  css:[id="scheduledPublishDate.month"]  10
    user enters text into element  css:[id="scheduledPublishDate.year"]   2025
    user enters text into element  css:[id="nextReleaseDate.day"]  25
    user enters text into element  css:[id="nextReleaseDate.month"]  10
    user enters text into element  css:[id="nextReleaseDate.year"]  2026
    user clicks element  xpath://label[text()="National Statistics"]

Click Create new release button
    [Tags]   HappyPath
    user clicks button   Create new release
    user waits until page contains element  xpath://h1/span[text()="Edit release"]
    user checks page contains heading 1  UI tests - create publication %{RUN_IDENTIFIER}

Verify Release summary
    [Tags]  HappyPath
    user checks page contains element   xpath://li/a[text()="Release summary" and contains(@class, 'current-page')]
    user checks page contains heading 2    Release summary
    user checks summary list item "Publication title" should be "UI tests - create publication %{RUN_IDENTIFIER}"
    user checks summary list item "Time period" should be "Spring Term"
    user checks summary list item "Release period" should be "2025 to 2026"  # Correct?
    user checks summary list item "Lead statistician" should be "Sean Gibson"
    user checks summary list item "Scheduled release" should be "24 October 2025"
    user checks summary list item "Next release expected" should be "25 October 2026"
    user checks summary list item "Release type" should be "National Statistics"

Edit release
    [Tags]  HappyPath  UnderConstruction


