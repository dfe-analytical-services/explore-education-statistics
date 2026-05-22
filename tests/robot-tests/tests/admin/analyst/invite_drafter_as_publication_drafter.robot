*** Settings ***
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot
Resource            ../../libs/tables-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      do suite teardown
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData


*** Variables ***
${PUBLICATION_NAME}=    Invite drafter %{RUN_IDENTIFIER}
${INVITEE_EMAIL}        ees-analyst-%{RUN_IDENTIFIER}@education.gov.uk


*** Test Cases ***
Create Publication as bau1
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}

    user navigates to publication page from dashboard    ${PUBLICATION_NAME}

    user waits until page contains link    Team access
    user clicks link    Team access
    user waits until h2 is visible    Manage team access
    user waits until page contains element    id:inviteDrafterForm-email
    user waits until page contains button    Invite drafter
    user waits until h3 is visible    Publication drafters
    user waits until h3 is visible    Publication approvers

Assign various release access permissions to analysts
    user changes to bau1
    user gives analyst publication drafter access    ${PUBLICATION_NAME}
    ...    EES-test.ANALYST1@education.gov.uk

Sign in as analyst1 and go to Manage team access page
    user changes to analyst1
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user clicks link    Team access

    user waits until page contains element    id:inviteDrafterForm-email
    user checks page contains    There are no approvers, or pending approver invites, for this publication.

Invite existing user analyst2 to be a drafter for the publication
    user enters text into element    id:inviteDrafterForm-email    EES-test.ANALYST2@education.gov.uk
    user clicks button    Invite drafter

Validate updated drafters displayed for the publication
    user checks table body has x rows    2    testid:publicationDrafterRoles
    user checks table cell contains    1    1    Analyst1 User1    testid:publicationDrafterRoles
    user checks table cell contains    1    2    ees-test.analyst1@education.gov.uk    testid:publicationDrafterRoles
    user checks table cell contains    2    1    Analyst2 User2    testid:publicationDrafterRoles
    user checks table cell contains    2    2    ees-test.analyst2@education.gov.uk    testid:publicationDrafterRoles

Invite brand new user
    user enters text into element    id:inviteDrafterForm-email    ${INVITEE_EMAIL}
    user clicks button    Invite drafter

Validate drafters table contains the brand new user
    user checks table body has x rows    3    testid:publicationDrafterRoles
    user checks table cell contains    1    1    Analyst1 User1    testid:publicationDrafterRoles
    user checks table cell contains    1    2    ees-test.analyst1@education.gov.uk    testid:publicationDrafterRoles
    user checks table cell contains    2    1    Analyst2 User2    testid:publicationDrafterRoles
    user checks table cell contains    2    2    ees-test.analyst2@education.gov.uk    testid:publicationDrafterRoles
    user checks table cell contains    3    2    ${INVITEE_EMAIL}    testid:publicationDrafterRoles

Remove analyst2 as a drafter for the publication
    user clicks remove user button for row    Analyst2 User2
    user waits until page contains element    testid:modal-title
    user waits until modal is visible    Confirm drafter removal
    user clicks button    Confirm

Validate removed analyst2 no longer visible in publication drafter roles table
    user checks table body has x rows    2    testid:publicationDrafterRoles
    user checks table cell contains    1    1    Analyst1 User1    testid:publicationDrafterRoles
    user checks table cell contains    1    2    ees-test.analyst1@education.gov.uk    testid:publicationDrafterRoles
    user checks table cell contains    2    2    ${INVITEE_EMAIL}    testid:publicationDrafterRoles

Cancel brand new user drafter invite for the publication
    user clicks cancel invite button for row    ${INVITEE_EMAIL}
    user waits until page contains element    testid:modal-title
    user waits until modal is visible    Confirm cancelling of user invite
    user clicks button    Confirm

Validate removed brand new user no longer visible in publication drafter roles table
    user checks table body has x rows    1    testid:publicationDrafterRoles
    user checks table cell contains    1    1    Analyst1 User1    testid:publicationDrafterRoles
    user checks table cell contains    1    2    ees-test.analyst1@education.gov.uk    testid:publicationDrafterRoles

Invite existing user analyst2 to be a drafter for the publication again
    user enters text into element    id:inviteDrafterForm-email    EES-test.ANALYST2@education.gov.uk
    user clicks button    Invite drafter

Validate publication drafters table containst analyst2 again
    user checks table body has x rows    2    testid:publicationDrafterRoles
    user checks table cell contains    1    1    Analyst1 User1    testid:publicationDrafterRoles
    user checks table cell contains    1    2    ees-test.analyst1@education.gov.uk    testid:publicationDrafterRoles
    user checks table cell contains    2    1    Analyst2 User2    testid:publicationDrafterRoles
    user checks table cell contains    2    2    ees-test.analyst2@education.gov.uk    testid:publicationDrafterRoles


*** Keywords ***
user clicks remove user button for row
    [Arguments]    ${text}
    ${row}=    get webelement    xpath://tbody/tr/td[.="${text}"]/..
    user clicks button    Remove    ${row}

user clicks cancel invite button for row
    [Arguments]    ${text}
    ${row}=    get webelement    xpath://tbody/tr/td[contains(., "${text}")]/..
    user clicks button    Cancel invite    ${row}

do suite teardown
    user closes the browser
    delete user invite via api    ${INVITEE_EMAIL}
