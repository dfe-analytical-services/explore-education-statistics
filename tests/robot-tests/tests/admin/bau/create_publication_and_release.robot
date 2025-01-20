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


*** Test Cases ***
Go to Create publication page for "UI tests theme" theme
    user selects dashboard theme if possible
    user waits until page contains link    Create new publication
    user checks page does not contain button    ${PUBLICATION_NAME}
    user clicks link    Create new publication
    user waits until page contains title caption    %{TEST_THEME_NAME}
    user waits until h1 is visible    Create new publication

Enters contact details
    user enters text into element    name:teamName    Post-16 statistics team
    user enters text into element    name:teamEmail    post16.statistics@education.gov.uk
    user enters text into element    name:contactName    UI Tests Contact Name
    user enters text into element    name:contactTelNo    0123456789

Option to set superseding publication should not appear
    user checks page does not contain element    name:supersede

Error message appears when submitting and title is empty
    user checks element is not visible    id:publicationForm-title-error    %{WAIT_SMALL}
    user clicks button    Save publication
    user waits until element is visible    id:publicationForm-title-error    %{WAIT_SMALL}

Enter new publication title
    user enters text into element    name:title    ${PUBLICATION_NAME} (created)
    user checks element is not visible    id:publicationForm-title-error    %{WAIT_SMALL}

Enter new publication summary
    user enters text into element    name:summary    ${PUBLICATION_NAME} summary

User redirects to the dashboard after saving publication
    user clicks button    Save publication
    user waits until h1 is visible    Dashboard

Verify new publication has no releases
    user navigates to publication page from dashboard    ${PUBLICATION_NAME} (created)

    user checks page contains    You have no scheduled releases.
    user checks page contains    You have no draft releases.
    user checks page contains    You have no published releases.

Verify new publication's contact
    user clicks link    Contact
    user waits until h2 is visible    Contact for this publication

    user checks summary list contains    Team name    Post-16 statistics team
    user checks summary list contains    Team email    post16.statistics@education.gov.uk
    user checks summary list contains    Contact name    UI Tests Contact Name
    user checks summary list contains    Contact telephone    123456789

Create new test theme
    ${theme_id}=    user creates theme via api    ${CREATED_THEME_NAME}
    set suite variable    ${CREATED_THEME_ID}    ${theme_id}

Update publication's details
    user clicks link    Details
    user waits until h2 is visible    Publication details

    user clicks button    Edit publication details

    user enters text into element    name:title    ${PUBLICATION_NAME}
    user enters text into element    name:summary    ${PUBLICATION_NAME} summary updated
    user chooses select option    name:themeId    ${CREATED_THEME_NAME}

    user clicks button    Update publication details

    ${modal}=    user waits until modal is visible    Confirm publication changes
    user clicks button    Confirm    ${modal}
    user waits until page contains button    Edit publication details

Verify publication details have been updated
    user checks summary list contains    Publication title    ${PUBLICATION_NAME}
    user checks summary list contains    Publication summary    ${PUBLICATION_NAME} summary updated
    user checks summary list contains    Theme    ${CREATED_THEME_NAME}
    user checks summary list contains    Superseding publication    This publication is not archived

Update publication's contact
    user clicks link    Contact
    user waits until h2 is visible    Contact for this publication

    user clicks button    Edit contact details

    user enters text into element    name:teamName    Special educational needs statistics team
    user enters text into element    name:teamEmail    sen.statistics@education.gov.uk
    user enters text into element    name:contactName    UI Tests Contact Name
    user enters text into element    name:contactTelNo    0987654321

    user clicks button    Update contact details

    ${modal}=    user waits until modal is visible    Confirm contact changes
    user clicks button    Confirm    ${modal}
    user waits until page contains button    Edit contact details

Verify contact details have been updated
    user checks summary list contains    Team name    Special educational needs statistics team
    user checks summary list contains    Team email    sen.statistics@education.gov.uk
    user checks summary list contains    Contact name    UI Tests Contact Name
    user checks summary list contains    Contact telephone    0987654321

Add a methodology
    user creates methodology for publication    ${PUBLICATION_NAME}    ${CREATED_THEME_NAME}

Verify new methodology is attached to publication
    user checks summary list contains    Title    ${PUBLICATION_NAME}
    user checks summary list contains    Published on    Not yet published
    user checks summary list contains    Owning publication    ${PUBLICATION_NAME}

    user clicks link    ${PUBLICATION_NAME}
    user waits until h2 is visible    Manage releases

    user clicks link    Methodologies
    user waits until h2 is visible    Manage methodologies

    ${ROW}=    user gets table row    ${PUBLICATION_NAME}

    user checks element contains    ${ROW}    Owned
    user checks element contains    ${ROW}    Draft
    user checks element contains    ${ROW}    Not yet published
    user checks element contains link    ${ROW}    Edit
    user checks element contains button    ${ROW}    Delete draft

Create new release for publication
    user clicks link    Releases
    user waits until h2 is visible    Manage releases

    user clicks link    Create new release
    user waits until h1 is visible    Create new release

    user waits until page contains element    name:timePeriodCoverageCode
    user waits until page contains element    name:timePeriodCoverageStartYear

Verify Release type options
    user checks page contains radio    Accredited official statistics
    user checks page contains radio    Official statistics
    user checks page contains radio    Official statistics in development
    user checks page contains radio    Ad hoc statistics
    user checks page contains radio    Management information

Create new release
    user chooses select option    name:timePeriodCoverageCode    Spring term
    user enters text into element    name:timePeriodCoverageStartYear    2025
    user clicks radio    Accredited official statistics
    user clicks button    Create new release
    user waits until page contains title caption    Edit release for Spring term 2025/26
    user waits until h1 is visible    ${PUBLICATION_NAME}

Verify created release summary
    user checks page contains element    xpath://li/a[text()="Summary" and contains(@aria-current, 'page')]
    user waits until h2 is visible    Release summary
    user verifies release summary    Spring term
    ...    2025/26    Accredited official statistics

Edit release summary
    user waits until page contains link    Edit release summary
    user clicks link    Edit release summary
    user waits until h2 is visible    Edit release summary
    user waits until page contains element    name:timePeriodCoverageStartYear
    user chooses select option    name:timePeriodCoverageCode    Summer term
    user enters text into element    name:timePeriodCoverageStartYear    2026
    user clicks radio    Official statistics
    user clicks button    Update release summary

Verify updated release summary
    user waits until h2 is visible    Release summary
    user checks page contains element    xpath://li/a[text()="Summary" and contains(@aria-current, 'page')]
    user verifies release summary    Summer term
    ...    2026/27    Official statistics


*** Keywords ***
teardown suite
    IF    "${CREATED_THEME_ID}" != ""
        user deletes theme via api    ${CREATED_THEME_ID}
    END
    user closes the browser
