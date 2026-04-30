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
    user waits until page contains link    Manage publication drafters
    user waits until h3 is visible    Invite a user to edit this publication
    user waits until h3 is visible    Edit drafters for this publication

Validate "Manage publication drafters" page
    user clicks link    Manage publication drafters
    user waits until page contains element    id:inviteDrafterForm-email

Assign various release access permissions to analysts
    user changes to bau1

    user gives analyst publication drafter access    ${PUBLICATION_NAME}
    ...    EES-test.ANALYST1@education.gov.uk

Sign in as analyst1 and go to Manage team access page
    user changes to analyst1
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user clicks link    Team access

    user waits until page contains link    Manage publication drafters
    user checks page contains    There are no publication approvers.

User navigates to the "Manage publication drafters" page
    user clicks link    Manage publication drafters
    user waits until page contains element    id:inviteDrafterForm-email

Invite existing user analyst2 to be a drafter for the publication
    user enters text into element    id:inviteDrafterForm-email    EES-test.ANALYST2@education.gov.uk
    user clicks button    Invite user
    # Could do with a wait check here? But what do we wait for?

Validate updated drafters displayed for the publication
    user checks checkbox is checked    Analyst1 User1 (ees-test.analyst1@education.gov.uk)
    user checks checkbox is checked    Analyst2 User2 (ees-test.analyst2@education.gov.uk)

User navigates back to team access page
    user clicks button    Go back
    user waits until page contains link    Manage publication drafters

    user checks table body has x rows    2    testid:publicationRoles
    user checks table cell contains    1    1    Analyst1 User1    testid:publicationRoles
    user checks table cell contains    1    2    ees-test.analyst1@education.gov.uk    testid:publicationRoles
    user checks table cell contains    2    1    Analyst2 User2    testid:publicationRoles
    user checks table cell contains    2    2    ees-test.analyst2@education.gov.uk    testid:publicationRoles

User navigates back the "Manage publication drafters" page
    user clicks link    Manage publication drafters
    user waits until page contains element    id:inviteDrafterForm-email

Update drafters for publication
    user clicks checkbox    Analyst1 User1 (ees-test.analyst1@education.gov.uk)

    user checks checkbox is not checked    Analyst1 User1 (ees-test.analyst1@education.gov.uk)
    user checks checkbox is checked    Analyst2 User2 (ees-test.analyst2@education.gov.uk)

    user clicks checkbox    Analyst1 User1 (ees-test.analyst1@education.gov.uk)
    user clicks checkbox    Analyst2 User2 (ees-test.analyst2@education.gov.uk)

    user checks checkbox is checked    Analyst1 User1 (ees-test.analyst1@education.gov.uk)
    user checks checkbox is not checked    Analyst2 User2 (ees-test.analyst2@education.gov.uk)

    user clicks button    Update drafters
    # Could do with a wait check here? But what do we wait for?

Validate updated drafters for publication
    Page Should Contain Checkbox    [HOW DO I DO THIS?]
    Page Should Not Contain Checkbox    [HOW DO I DO THIS?]

User navigates back to team access page
    user clicks button    Go back
    user waits until page contains link    Manage publication drafters

    user checks table body has x rows    1    testid:publicationRoles
    user checks table cell contains    1    1    Analyst2 User2    testid:publicationRoles
    user checks table cell contains    1    2    ees-test.analyst2@education.gov.uk    testid:publicationRoles


*** Keywords ***
user clicks remove user button for row
    [Arguments]    ${text}
    ${row}=    get webelement    xpath://tbody/tr/td[.="${text}"]/..
    user clicks button    Remove    ${row}

do suite teardown
    user closes the browser
    delete user invite via api    ${INVITEE_EMAIL}
