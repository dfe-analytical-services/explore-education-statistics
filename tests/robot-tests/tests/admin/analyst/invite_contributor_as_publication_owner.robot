*** Settings ***
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot
Resource            ../../libs/tables-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData


*** Variables ***
${PUBLICATION_NAME}=    UI tests - invite contributor %{RUN_IDENTIFIER}


*** Test Cases ***
Create Publication as bau1
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user create test release via api    ${PUBLICATION_ID}    AY    2000
    user create test release via api    ${PUBLICATION_ID}    AY    2001
    user create test release via api    ${PUBLICATION_ID}    AY    2002

    user navigates to publication page from dashboard    ${PUBLICATION_NAME}

    user waits until page contains link    Team access
    user clicks link    Team access
    user waits until page contains link    Add or remove release contributors
    user waits until h3 is visible    Update release access

Validate "Manage release contributors" page
    user clicks link    Add or remove release contributors
    user waits until h2 is visible    Manage release contributors (Academic Year 2002/03)
    user waits until page contains    There are no contributors for this release's publication.

Validate Invite new users page
    user clicks button    Go back

    user clicks link    Add or remove publication contributors

    user waits until page contains element    id:email
    user checks checkbox is checked    Academic Year 2002/03
    user checks checkbox is checked    Academic Year 2001/02
    user checks checkbox is checked    Academic Year 2000/01

Assign various release access permissions to analysts
    user changes to bau1

    user gives analyst publication owner access    ${PUBLICATION_NAME}
    ...    EES-test.ANALYST1@education.gov.uk

    user gives release access to analyst    ${PUBLICATION_NAME} - Academic Year 2000/01    Contributor
    ...    EES-test.ANALYST3@education.gov.uk

    user gives release access to analyst    ${PUBLICATION_NAME} - Academic Year 2001/02    Contributor
    ...    EES-test.ANALYST2@education.gov.uk

Sign in as analyst1 and go to Manage team access page
    user changes to analyst1
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user clicks link    Team access

    user waits until page contains    Update release access
    user checks summary list contains    Release    Academic Year 2002/03 (Not live)
    user checks summary list contains    Status    Draft
    user checks page contains    There are no approvers or pending approver invites for this release.
    user checks page contains    There are no contributors or pending contributor invites for this release.
    user waits until page contains link    Add or remove release contributors

Validate Select release dropdown
    user checks select contains x options    id:currentRelease    3
    user checks select contains option    id:currentRelease    Academic Year 2002/03
    user checks select contains option    id:currentRelease    Academic Year 2001/02
    user checks select contains option    id:currentRelease    Academic Year 2000/01

Invite existing user analyst2 to be a contributor for 2002/03 release
    user clicks link    Add or remove publication contributors
    user waits until page contains    Invite a user to edit this publication
    user enters text into element    id:email    EES-test.ANALYST2@education.gov.uk

    user checks checkbox is checked    Academic Year 2002/03
    user checks checkbox is checked    Academic Year 2001/02
    user checks checkbox is checked    Academic Year 2000/01

    user clicks checkbox    Academic Year 2001/02
    user clicks checkbox    Academic Year 2000/01

    user checks checkbox is checked    Academic Year 2002/03
    user checks checkbox is not checked    Academic Year 2001/02
    user checks checkbox is not checked    Academic Year 2000/01

    user clicks button    Invite user
    user waits until page contains    Update release access
    user checks summary list contains    Release    Academic Year 2002/03 (Not live)

Validate contributors for 2002/03 release
    user waits until page contains    Analyst2 User2 (ees-test.analyst2@education.gov.uk)

    user checks page does not contain    Analyst1 User1 (ees-test.analyst1@education.gov.uk)
    user checks page does not contain    Analyst3 User3 (ees-test.analyst3@education.gov.uk)
    user checks page does not contain    There are no contributors or pending contributor invites for this release.

Add new contributors to release
    user clicks link    Add or remove release contributors
    user waits until page contains    Manage release contributors (Academic Year 2002/03)

    user checks page does not contain    Analyst1 User1 (ees-test.analyst1@education.gov.uk)
    user checks checkbox is checked    Analyst2 User2 (ees-test.analyst2@education.gov.uk)
    user checks checkbox is not checked    Analyst3 User3 (ees-test.analyst3@education.gov.uk)

    user clicks checkbox    Analyst2 User2 (ees-test.analyst2@education.gov.uk)
    user clicks checkbox    Analyst3 User3 (ees-test.analyst3@education.gov.uk)

    user checks checkbox is not checked    Analyst2 User2 (ees-test.analyst2@education.gov.uk)
    user checks checkbox is checked    Analyst3 User3 (ees-test.analyst3@education.gov.uk)

    user clicks button    Update contributors
    user waits until page contains    Update release access

Validate contributors for 2002/03 release again
    user checks page contains    Analyst3 User3 (ees-test.analyst3@education.gov.uk)
    user checks page does not contain    Analyst1 User1 (ees-test.analyst1@education.gov.uk)
    user checks page does not contain    Analyst2 User2 (ees-test.analyst2@education.gov.uk)

Remove all analyst3 contributor access to publication
    user clicks remove user button for row    Analyst3 User3 (ees-test.analyst3@education.gov.uk)
    user waits until modal is visible    Confirm user removal
    user clicks button    Confirm
    user waits until modal is not visible    Confirm user removal

Validate contributors for 2002/03 release for the third time
    user waits until page does not contain    Analyst3 User3 (ees-test.analyst3@education.gov.uk)
    user checks page does not contain    Analyst1 User1 (ees-test.analyst1@education.gov.uk)
    user checks page does not contain    Analyst2 User2 (ees-test.analyst2@education.gov.uk)
    user checks page contains    There are no contributors or pending contributor invites for this release.

Validate contributors for 2001/02 release
    user chooses select option    id:currentRelease    Academic Year 2001/02
    user checks summary list contains    Release    Academic Year 2001/02 (Not live)

    user waits until page contains    Analyst2 User2 (ees-test.analyst2@education.gov.uk)
    user checks page does not contain    Analyst1 User1 (ees-test.analyst1@education.gov.uk)
    user checks page does not contain    Analyst3 User3 (ees-test.analyst3@education.gov.uk)

Validate contributors for 2000/01 release
    user chooses select option    id:currentRelease    Academic Year 2000/01
    user checks summary list contains    Release    Academic Year 2000/01 (Not live)

    user checks page does not contain    Analyst1 User1 (ees-test.analyst1@education.gov.uk)
    user checks page does not contain    Analyst2 User2 (ees-test.analyst2@education.gov.uk)
    user checks page does not contain    Analyst3 User3 (ees-test.analyst3@education.gov.uk)

Invite brand new user
    user clicks link    Add or remove publication contributors
    user waits until page contains    Invite a user to edit this publication
    user enters text into element    id:email    ees-analyst-%{RUN_IDENTIFIER}@education.gov.uk

    user clicks button    Invite user
    user waits until page contains    Update release access
    user checks summary list contains    Release    Academic Year 2000/01 (Not live)

Validate contributors for 2000/01 release again
    user checks page does not contain    Analyst1 User1 (ees-test.analyst1@education.gov.uk)
    user checks page does not contain    Analyst2 User2 (ees-test.analyst2@education.gov.uk)
    user checks page does not contain    Analyst3 User3 (ees-test.analyst3@education.gov.uk)
    user waits until page contains    ees-analyst-%{RUN_IDENTIFIER}@education.gov.uk    %{WAIT_SMALL}
    user checks page contains tag    Pending Invite


*** Keywords ***
user clicks remove user button for row
    [Arguments]    ${text}
    ${row}=    get webelement    xpath://tbody/tr/td[.="${text}"]/..
    ${remove_user_button}=    get child element    ${row}    xpath://button[text()="Remove"]
    user clicks element    ${remove_user_button}
