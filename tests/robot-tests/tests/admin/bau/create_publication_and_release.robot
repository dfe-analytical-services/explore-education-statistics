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
    user waits until page contains title caption  ${TOPIC_NAME}
    user waits until page contains heading 1    Create new publication

Selects no methodology
    [Tags]  HappyPath
    user waits until page contains element   xpath://label[text()="No methodology"]
    user selects radio  No methodology

Enters contact details
    [Tags]  HappyPath
    user enters text into element  id:publicationForm-teamName        Post-16 statistics team
    user enters text into element  id:publicationForm-teamEmail       post16.statistics@education.gov.uk
    user enters text into element  id:publicationForm-contactName     Suzanne Wallace
    user enters text into element  id:publicationForm-contactTelNo    0123456789

Error message appears when submitting and title is empty
    [Tags]  HappyPath
    user checks element is not visible  id:publicationForm-title-error
    user clicks button   Save publication
    user waits until element is visible  id:publicationForm-title-error

Enter new publication title
    [Tags]  HappyPath
    user enters text into element  id:publicationForm-title  ${PUBLICATION_NAME} (created)
    user checks element is not visible  id:publicationForm-title-error

User redirects to the dashboard after saving publication
    [Tags]  HappyPath
    user clicks button   Save publication
    user waits until page contains heading 1  Dashboard

Verify that new publication has been created
    [Tags]  HappyPath
    user selects theme "Test theme" and topic "${TOPIC_NAME}" from the admin dashboard
    user waits until page contains button   ${PUBLICATION_NAME} (created)
    user checks page contains accordion   ${PUBLICATION_NAME} (created)
    user opens accordion section  ${PUBLICATION_NAME} (created)
    user checks summary list item "Team" should be "Post-16 statistics team"
    user checks summary list item "Team" should be "post16.statistics@education.gov.uk"
    user checks summary list item "Contact" should be "Suzanne Wallace"
    user checks summary list item "Contact" should be "0123456789"
    user checks summary list item "Methodology" should be "No methodology assigned"
    user checks summary list item "Releases" should be "No releases created"

Go to edit publication
    [Tags]  HappyPath
    user clicks element  css:[data-testid="Edit publication link for ${PUBLICATION_NAME} (created)"]
    user waits until page contains title caption  ${PUBLICATION_NAME} (created)
    user waits until page contains heading 1    Edit publication

Update publication
    [Tags]  HappyPath
    user enters text into element  id:publicationForm-title  ${PUBLICATION_NAME}
    user selects radio  Choose an existing methodology
    user waits until page contains element  xpath://option[text()="Test methodology [Approved]"]
    user selects from list by label  id:publicationForm-methodologyId   Test methodology [Approved]
    user enters text into element  id:publicationForm-teamName      Special educational needs statistics team
    user enters text into element  id:publicationForm-teamEmail     sen.statistics@education.gov.uk
    user enters text into element  id:publicationForm-contactName   Sean Gibson
    user enters text into element  id:publicationForm-contactTelNo  0987654321
    user clicks button   Save publication

Verify publication has been updated
    [Tags]  HappyPath
    user waits until page contains heading 1  Dashboard
    user selects theme "Test theme" and topic "${TOPIC_NAME}" from the admin dashboard
    user waits until page contains button   ${PUBLICATION_NAME}
    user checks page contains accordion   ${PUBLICATION_NAME}
    user opens accordion section  ${PUBLICATION_NAME}
    user checks summary list item "Team" should be "Special educational needs statistics team"
    user checks summary list item "Team" should be "sen.statistics@education.gov.uk"
    user checks summary list item "Contact" should be "Sean Gibson"
    user checks summary list item "Contact" should be "0987654321"
    user checks summary list item "Methodology" should be "Test methodology"
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
    user waits until page contains title caption  Edit release
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
