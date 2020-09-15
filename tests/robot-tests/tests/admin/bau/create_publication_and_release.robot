*** Settings ***
Resource    ../../libs/admin-common.robot

Force Tags  Admin  Local  Dev  AltersData

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Variables ***
${TOPIC_NAME}        %{TEST_TOPIC_NAME}
${PUBLICATION_NAME}  UI tests - create publication %{RUN_IDENTIFIER}
${METHODOLOGY_NAME}  UI test methodology

*** Test Cases ***
Create approved 'Test methodology'
    [Tags]  HappyPath
    user clicks link  manage methodologies
    user creates approved methodology  ${METHODOLOGY_NAME}
    user clicks link  Home

Go to Create publication page for "UI tests topic" topic
    [Tags]  HappyPath
    user selects theme "Test theme" and topic "${TOPIC_NAME}" from the admin dashboard
    user waits until page contains link    Create new publication
    user checks page does not contain button  ${PUBLICATION_NAME}
    user clicks link  Create new publication
    user waits until page contains title caption  ${TOPIC_NAME}
    user waits until h1 is visible    Create new publication

Selects no methodology
    [Tags]  HappyPath
    user waits until page contains element   xpath://label[text()="No methodology"]
    user clicks radio  No methodology

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
    user waits until h1 is visible  Dashboard

Verify that new publication has been created
    [Tags]  HappyPath
    user selects theme "Test theme" and topic "${TOPIC_NAME}" from the admin dashboard
    user waits until page contains accordion section   ${PUBLICATION_NAME} (created)
    user opens accordion section  ${PUBLICATION_NAME} (created)
    user checks testid element contains  Team name for ${PUBLICATION_NAME} (created)  Post-16 statistics team
    user checks testid element contains  Team email for ${PUBLICATION_NAME} (created)  post16.statistics@education.gov.uk
    user checks testid element contains  Contact name for ${PUBLICATION_NAME} (created)  Suzanne Wallace
    user checks testid element contains  Contact phone number for ${PUBLICATION_NAME} (created)  0123456789
    user checks testid element contains  Methodology for ${PUBLICATION_NAME} (created)  No methodology assigned
    user checks testid element contains  Releases for ${PUBLICATION_NAME} (created)  No releases created

Go to edit publication
    [Tags]  HappyPath
    user clicks testid element  Edit publication link for ${PUBLICATION_NAME} (created)
    user waits until page contains title caption  ${PUBLICATION_NAME} (created)
    user waits until h1 is visible    Edit publication

Update publication
    [Tags]  HappyPath
    user enters text into element  id:publicationForm-title  ${PUBLICATION_NAME}
    user clicks radio  Choose an existing methodology
    user waits until page contains element  xpath://option[text()="${METHODOLOGY_NAME} [Approved]"]
    user selects from list by label  id:publicationForm-methodologyId   ${METHODOLOGY_NAME} [Approved]
    user enters text into element  id:publicationForm-teamName      Special educational needs statistics team
    user enters text into element  id:publicationForm-teamEmail     sen.statistics@education.gov.uk
    user enters text into element  id:publicationForm-contactName   Sean Gibson
    user enters text into element  id:publicationForm-contactTelNo  0987654321
    user clicks button   Save publication

Verify publication has been updated
    [Tags]  HappyPath
    user waits until h1 is visible  Dashboard
    user selects theme "Test theme" and topic "${TOPIC_NAME}" from the admin dashboard
    user waits until page contains accordion section   ${PUBLICATION_NAME}
    user opens accordion section  ${PUBLICATION_NAME}
    user checks testid element contains  Team name for ${PUBLICATION_NAME}  Special educational needs statistics team
    user checks testid element contains  Team email for ${PUBLICATION_NAME}  sen.statistics@education.gov.uk
    user checks testid element contains  Contact name for ${PUBLICATION_NAME}  Sean Gibson
    user checks testid element contains  Contact phone number for ${PUBLICATION_NAME}  0987654321
    user checks testid element contains  Methodology for ${PUBLICATION_NAME}  ${METHODOLOGY_NAME}
    user checks testid element contains  Releases for ${PUBLICATION_NAME}  No releases created

Create new release
    [Tags]   HappyPath
    user clicks testid element  Create new release link for ${PUBLICATION_NAME}
    user waits until page contains element  id:releaseSummaryForm-timePeriodCoverage
    user waits until page contains element  id:releaseSummaryForm-timePeriodCoverageStartYear
    user selects from list by label  id:releaseSummaryForm-timePeriodCoverage  Spring Term
    user enters text into element  id:releaseSummaryForm-timePeriodCoverageStartYear  2025
    user clicks radio  National Statistics
    user clicks button   Create new release
    user waits until page contains title caption  Edit release
    user waits until h1 is visible  ${PUBLICATION_NAME}

Verify created release summary
    [Tags]  HappyPath
    user checks page contains element   xpath://li/a[text()="Release summary" and contains(@aria-current, 'page')]
    user waits until h2 is visible  Release summary
    user checks summary list contains  Publication title  ${PUBLICATION_NAME}
    user checks summary list contains  Time period  Spring Term
    user checks summary list contains  Release period  2025/26
    user checks summary list contains  Lead statistician  Sean Gibson
    user checks summary list contains  Scheduled release  Not scheduled
    user checks summary list contains  Next release expected  Not set
    user checks summary list contains  Release type  National Statistics

Edit release summary
    [Tags]  HappyPath
    user waits until page contains link  Edit release summary
    user clicks link  Edit release summary
    user waits until h2 is visible  Edit release summary
    user waits until page contains element  id:releaseSummaryForm-timePeriodCoverageStartYear
    user selects from list by label  id:releaseSummaryForm-timePeriodCoverage  Summer Term
    user enters text into element  id:releaseSummaryForm-timePeriodCoverageStartYear  2026
    user clicks radio  Official Statistics
    user clicks button   Update release summary

Verify updated release summary
    [Tags]  HappyPath
    user checks page contains element   xpath://li/a[text()="Release summary" and contains(@aria-current, 'page')]
    user waits until h2 is visible  Release summary
    user checks summary list contains  Publication title  ${PUBLICATION_NAME}
    user checks summary list contains  Time period  Summer Term
    user checks summary list contains  Release period  2026/27
    user checks summary list contains  Lead statistician  Sean Gibson
    user checks summary list contains  Scheduled release  Not scheduled
    user checks summary list contains  Next release expected  Not set
    user checks summary list contains  Release type  Official Statistics
