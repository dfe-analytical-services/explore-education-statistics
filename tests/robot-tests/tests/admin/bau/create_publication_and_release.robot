*** Settings ***
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      teardown suite
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData


*** Variables ***
${PUBLICATION_NAME}         UI tests - create publication %{RUN_IDENTIFIER}

${CREATED_THEME_ID}         ${EMPTY}
${CREATED_THEME_NAME}       UI test theme - create publication %{RUN_IDENTIFIER}
${CREATED_TOPIC_NAME}       UI test topic - create publication %{RUN_IDENTIFIER}


*** Test Cases ***
Go to Create publication page for "UI tests topic" topic
    user selects theme and topic from admin dashboard    %{TEST_THEME_NAME}    %{TEST_TOPIC_NAME}
    user waits until page contains link    Create new publication
    user checks page does not contain button    ${PUBLICATION_NAME}
    user clicks link    Create new publication
    user waits until page contains title caption    %{TEST_TOPIC_NAME}
    user waits until h1 is visible    Create new publication

Enters contact details
    user enters text into element    id:publicationForm-teamName    Post-16 statistics team
    user enters text into element    id:publicationForm-teamEmail    post16.statistics@education.gov.uk
    user enters text into element    id:publicationForm-contactName    UI Tests Contact Name
    user enters text into element    id:publicationForm-contactTelNo    0123456789

Option to set superseding publication should not appear
    user checks page does not contain element    id:publicationForm-supersede

Error message appears when submitting and title is empty
    user checks element is not visible    id:publicationForm-title-error    %{WAIT_SMALL}
    user clicks button    Save publication
    user waits until element is visible    id:publicationForm-title-error    %{WAIT_SMALL}

Enter new publication title
    user enters text into element    id:publicationForm-title    ${PUBLICATION_NAME} (created)
    user checks element is not visible    id:publicationForm-title-error    %{WAIT_SMALL}

Enter new publication summary
    user enters text into element    id:publicationForm-summary    ${PUBLICATION_NAME} summary

User redirects to the dashboard after saving publication
    user clicks button    Save publication
    user waits until h1 is visible    Dashboard

Verify that new publication has been created
    bau user goes to publication page from dashboard    ${PUBLICATION_NAME} (created)

    user checks testid element contains    Team name for ${PUBLICATION_NAME} (created)    Post-16 statistics team
    user checks testid element contains    Team email for ${PUBLICATION_NAME} (created)
    ...    post16.statistics@education.gov.uk
    user checks testid element contains    Contact name for ${PUBLICATION_NAME} (created)    UI Tests Contact Name
    user checks testid element contains    Contact phone number for ${PUBLICATION_NAME} (created)    0123456789
    user checks testid element contains    Releases for ${PUBLICATION_NAME} (created)    No releases created

Create new test theme and topic
    ${theme_id}    user creates theme via api    ${CREATED_THEME_NAME}
    ${topic_id}    user creates topic via api    ${CREATED_TOPIC_NAME}    ${theme_id}
    set suite variable    ${CREATED_THEME_ID}    ${theme_id}

Go to edit publication
    user clicks element    testid:Edit publication link for ${PUBLICATION_NAME} (created)
    user waits until page contains title caption    ${PUBLICATION_NAME} (created)    %{WAIT_SMALL}
    user waits until h1 is visible    Manage publication

Update publication
    user enters text into element    id:publicationForm-title    ${PUBLICATION_NAME}
    user enters text into element    id:publicationForm-summary    ${PUBLICATION_NAME} summary updated
    user chooses select option    id:publicationForm-themeId    ${CREATED_THEME_NAME}
    user chooses select option    id:publicationForm-topicId    ${CREATED_TOPIC_NAME}
    user enters text into element    id:publicationForm-teamName    Special educational needs statistics team
    user enters text into element    id:publicationForm-teamEmail    sen.statistics@education.gov.uk
    user enters text into element    id:publicationForm-contactName    UI Tests Contact Name
    user enters text into element    id:publicationForm-contactTelNo    0987654321
    user clicks button    Save publication
    user waits until h2 is visible    Confirm publication changes
    user clicks button    Confirm

Add a methodology
    bau user creates methodology for publication    ${PUBLICATION_NAME}    ${CREATED_THEME_NAME}
    ...    ${CREATED_TOPIC_NAME}

Verify publication has been updated
    bau user goes to publication page from dashboard    ${PUBLICATION_NAME}    ${CREATED_THEME_NAME}
    ...    ${CREATED_TOPIC_NAME}
    user checks testid element contains    Team name for ${PUBLICATION_NAME}
    ...    Special educational needs statistics team
    user checks testid element contains    Team email for ${PUBLICATION_NAME}    sen.statistics@education.gov.uk
    user checks testid element contains    Contact name for ${PUBLICATION_NAME}    UI Tests Contact Name
    user checks testid element contains    Contact phone number for ${PUBLICATION_NAME}    0987654321
    user checks testid element contains    Methodology for ${PUBLICATION_NAME}    ${PUBLICATION_NAME}
    user checks testid element contains    Releases for ${PUBLICATION_NAME}    No releases created

Go to 'Create new release'
    user clicks element    testid:Create new release link for ${PUBLICATION_NAME}
    user waits until page contains element    id:releaseSummaryForm-timePeriodCoverage
    user waits until page contains element    id:releaseSummaryForm-timePeriodCoverageStartYear

Verify Release type options
    user checks page contains radio    Ad hoc statistics
    user checks page contains radio    Experimental statistics
    user checks page contains radio    Management information
    user checks page contains radio    National statistics
    user checks page contains radio    Official statistics

Create new release
    user chooses select option    id:releaseSummaryForm-timePeriodCoverageCode    Spring Term
    user enters text into element    id:releaseSummaryForm-timePeriodCoverageStartYear    2025
    user clicks radio    National statistics
    user clicks button    Create new release
    user waits until page contains title caption    Edit release for Spring Term 2025/26
    user waits until h1 is visible    ${PUBLICATION_NAME}

Verify created release summary
    user checks page contains element    xpath://li/a[text()="Summary" and contains(@aria-current, 'page')]
    user waits until h2 is visible    Release summary
    user verifies release summary    ${PUBLICATION_NAME}    ${PUBLICATION_NAME} summary updated    Spring Term
    ...    2025/26    UI Tests Contact Name    National statistics

Edit release summary
    user waits until page contains link    Edit release summary
    user clicks link    Edit release summary
    user waits until h2 is visible    Edit release summary
    user waits until page contains element    id:releaseSummaryForm-timePeriodCoverageStartYear
    user chooses select option    id:releaseSummaryForm-timePeriodCoverageCode    Summer Term
    user enters text into element    id:releaseSummaryForm-timePeriodCoverageStartYear    2026
    user clicks radio    Official statistics
    user clicks button    Update release summary

Verify updated release summary
    user checks page contains element    xpath://li/a[text()="Summary" and contains(@aria-current, 'page')]
    user verifies release summary    ${PUBLICATION_NAME}    ${PUBLICATION_NAME} summary updated    Summer Term
    ...    2026/27    UI Tests Contact Name    Official statistics


*** Keywords ***
teardown suite
    IF    "${CREATED_THEME_ID}" != ""
        user deletes theme via api    ${CREATED_THEME_ID}
    END
    user closes the browser
