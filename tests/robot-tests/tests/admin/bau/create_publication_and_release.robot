*** Settings ***
Resource    ../../libs/admin-common.robot

Force Tags  Admin  Local  Dev  AltersData

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Variables ***
${TOPIC_NAME}        UI test topic %{RUN_IDENTIFIER}
${PUBLICATION_NAME}  UI tests - create publication %{RUN_IDENTIFIER}

*** Test Cases ***
Go to Create publication page for "UI tests topic" topic
    [Tags]  HappyPath
    environment variable should be set   RUN_IDENTIFIER
    user selects theme "Test theme" and topic "${TOPIC_NAME}" from the admin dashboard
    user waits until page contains link    Create new publication
    user checks page does not contain button  ${PUBLICATION_NAME}
    user clicks link  Create new publication
    user waits until page contains heading    Create new publication

Select an existing methodology
    [Tags]  HappyPath
    user waits until page contains element   xpath://label[text()="Choose an existing methodology"]
    user clicks element          xpath://label[text()="Choose an existing methodology"]
    user checks element is visible    xpath://label[text()="Select methodology"]
    user selects from list by label  id:createPublicationForm-selectedMethodologyId   Test methodology [Approved]

Select contact "Sean Gibson"
    [Tags]  HappyPath
    user selects from list by label  css:#createPublicationForm-selectedContactId   Sean Gibson - (Special educational needs statistics team)
    user checks summary list item "Email" should be "sen.statistics@education.gov.uk"
    user checks summary list item "Telephone" should be "01325340987"

Error message appears when user clicks continue is title is empty
    [Tags]  HappyPath
    user checks element is not visible  css:#createPublicationForm-publicationTitle-error
    user clicks button   Create publication
    user waits until element is visible  css:#createPublicationForm-publicationTitle-error

Enter new publication title
    [Tags]  HappyPath
    user enters text into element  css:#createPublicationForm-publicationTitle  ${PUBLICATION_NAME}
    user checks element is not visible  css:#createPublicationForm-publicationTitle-error

User redirects to the dashboard when clicking the Create publication button
    [Tags]  HappyPath
    user clicks button   Create publication
    user waits until page contains element   xpath://span[text()="Welcome"]

Verify that new publication has been created
    [Tags]  HappyPath
    user selects theme "Test theme" and topic "${TOPIC_NAME}" from the admin dashboard
    user waits until page contains button   ${PUBLICATION_NAME}
    user checks page contains accordion   ${PUBLICATION_NAME}
    user opens accordion section  ${PUBLICATION_NAME}
    user checks page contains element   xpath://div/dt[text()="Methodology"]/../dd//a[text()="Test methodology"]
    user checks summary list item "Releases" should be "No releases created"

Create new release
    [Tags]   HappyPath
    user clicks element  css:[data-testid="Create new release link for ${PUBLICATION_NAME}"]
    user waits until page contains element  id:releaseSummaryForm-timePeriodCoverage
    user waits until page contains element  id:releaseSummaryForm-timePeriodCoverageStartYear
    user selects from list by label  id:releaseSummaryForm-timePeriodCoverage  Spring Term
    user enters text into element  id:releaseSummaryForm-timePeriodCoverageStartYear  2025
    user clicks element   css:input[data-testid="National Statistics"]
    user clicks button   Create new release
    user waits until page contains element  xpath://h1/span[text()="Edit release"]
    user waits until page contains heading 1  ${PUBLICATION_NAME}

Verify created release summary
    [Tags]  HappyPath
    user checks page contains element   xpath://li/a[text()="Release summary" and contains(@aria-current, 'page')]
    user waits until page contains heading 2  Release summary
    user checks summary list item "Publication title" should be "${PUBLICATION_NAME}"
    user checks summary list item "Time period" should be "Spring Term"
    user checks summary list item "Release period" should be "2025/26"
    user checks summary list item "Lead statistician" should be "Sean Gibson"
    user checks summary list item "Scheduled release" should be "Not scheduled"
    user checks summary list item "Next release expected" should be "Not set"
    user checks summary list item "Release type" should be "National Statistics"

Edit release summary
    [Tags]  HappyPath
    user waits until page contains link  Edit release summary
    user clicks link  Edit release summary
    user waits until page contains heading 2  Edit release summary
    user waits until page contains element  id:releaseSummaryForm-timePeriodCoverageStartYear
    user selects from list by label  id:releaseSummaryForm-timePeriodCoverage  Summer Term
    user enters text into element  id:releaseSummaryForm-timePeriodCoverageStartYear  2026
    user clicks element   css:input[data-testid="Official Statistics"]
    user clicks button   Update release summary

Verify updated release summary
    [Tags]  HappyPath
    user checks page contains element   xpath://li/a[text()="Release summary" and contains(@aria-current, 'page')]
    user waits until page contains heading 2  Release summary
    user checks summary list item "Publication title" should be "${PUBLICATION_NAME}"
    user checks summary list item "Time period" should be "Summer Term"
    user checks summary list item "Release period" should be "2026/27"
    user checks summary list item "Lead statistician" should be "Sean Gibson"
    user checks summary list item "Scheduled release" should be "Not scheduled"
    user checks summary list item "Next release expected" should be "Not set"
    user checks summary list item "Release type" should be "Official Statistics"
