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
    user creates test release via api    ${PUBLICATION_ID}    AY    2000
    user creates test release via api    ${PUBLICATION_ID}    AY    2001
    user creates test release via api    ${PUBLICATION_ID}    AY    2002

    user navigates to publication page from dashboard    ${PUBLICATION_NAME}

    user waits until page contains link    Team access
    user clicks link    Team access
    user waits until page contains link    Manage release contributors
    user waits until h3 is visible    Update release access

Validate "Manage release contributors" page
    user clicks link    Manage release contributors
    user waits until h2 is visible    Manage release contributors (Academic year 2002/03)
    user waits until page contains    There are no contributors for this release's publication.

Validate Invite new users page
    user clicks button    Go back

    user clicks link    Invite new contributors

    user waits until page contains element    id:email
    user checks checkbox is checked    Academic year 2002/03
    user checks checkbox is checked    Academic year 2001/02
    user checks checkbox is checked    Academic year 2000/01

Assign various release access permissions to analysts
    user changes to bau1

    user gives analyst publication owner access    ${PUBLICATION_NAME}
    ...    EES-test.ANALYST1@education.gov.uk

    user gives release access to analyst
    ...    ${PUBLICATION_NAME}
    ...    Academic year 2000/01
    ...    Contributor
    ...    EES-test.ANALYST3@education.gov.uk

    user gives release access to analyst
    ...    ${PUBLICATION_NAME}
    ...    Academic year 2001/02
    ...    Contributor
    ...    EES-test.ANALYST2@education.gov.uk

Sign in as analyst1 and go to Manage team access page
    user changes to analyst1
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user clicks link    Team access

    user waits until page contains    Update release access
    user checks summary list contains    Release    Academic year 2002/03 (Not live)
    user checks summary list contains    Status    Draft
    user checks page contains    There are no approvers or pending approver invites for this release.
    user checks page contains    There are no contributors or pending contributor invites for this release.
    user waits until page contains link    Manage release contributors

Validate Select release dropdown
    user checks select contains x options    id:currentRelease    3
    user checks select contains option    id:currentRelease    Academic year 2002/03
    user checks select contains option    id:currentRelease    Academic year 2001/02
    user checks select contains option    id:currentRelease    Academic year 2000/01

Invite existing user analyst2 to be a contributor for 2002/03 release
    user clicks link    Invite new contributors
    user waits until page contains    Invite a user to edit this publication
    user enters text into element    id:email    EES-test.ANALYST2@education.gov.uk

    user checks checkbox is checked    Academic year 2002/03
    user checks checkbox is checked    Academic year 2001/02
    user checks checkbox is checked    Academic year 2000/01

    user clicks checkbox    Academic year 2001/02
    user clicks checkbox    Academic year 2000/01

    user checks checkbox is checked    Academic year 2002/03
    user checks checkbox is not checked    Academic year 2001/02
    user checks checkbox is not checked    Academic year 2000/01

    user clicks button    Invite user
    user waits until page contains    Update release access
    user checks summary list contains    Release    Academic year 2002/03 (Not live)

Validate contributors for 2002/03 release
    user checks table body has x rows    1    testid:releaseContributors
    user checks table cell contains    1    1    Analyst2 User2    testid:releaseContributors
    user checks table cell contains    1    2    ees-test.analyst2@education.gov.uk    testid:releaseContributors

Add new contributors to release
    user clicks link    Manage release contributors
    user waits until page contains    Manage release contributors (Academic year 2002/03)

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
    user checks table body has x rows    1    testid:releaseContributors
    user checks table cell contains    1    1    Analyst3 User3    testid:releaseContributors
    user checks table cell contains    1    2    ees-test.analyst3@education.gov.uk    testid:releaseContributors

Remove all analyst3 contributor access to publication
    user clicks remove user button for row    Analyst3 User3
    user waits until modal is visible    Confirm user removal
    user clicks button    Confirm
    user waits until modal is not visible    Confirm user removal

Validate contributors for 2002/03 release for the third time
    user waits until page does not contain element    testid:releaseContributors
    user checks page contains    There are no contributors or pending contributor invites for this release.

Validate contributors for 2001/02 release
    user chooses select option    id:currentRelease    Academic year 2001/02
    user checks summary list contains    Release    Academic year 2001/02 (Not live)

    user checks table body has x rows    1    testid:releaseContributors
    user checks table cell contains    1    1    Analyst2 User2    testid:releaseContributors
    user checks table cell contains    1    2    ees-test.analyst2@education.gov.uk    testid:releaseContributors

Validate contributors for 2000/01 release
    user chooses select option    id:currentRelease    Academic year 2000/01
    user checks summary list contains    Release    Academic year 2000/01 (Not live)

    user waits until page does not contain element    testid:releaseContributors
    user checks page contains    There are no contributors or pending contributor invites for this release.

Invite brand new user
    user clicks link    Invite new contributors
    user waits until page contains    Invite a user to edit this publication
    user enters text into element    id:email    ees-analyst-%{RUN_IDENTIFIER}@education.gov.uk

    user clicks button    Invite user
    user waits until page contains    Update release access
    user checks summary list contains    Release    Academic year 2000/01 (Not live)

Validate contributors for 2000/01 release again
    user checks page contains element    testid:releaseContributors

    user checks table body has x rows    1    testid:releaseContributors
    user checks table cell contains    1    1    ${EMPTY}    testid:releaseContributors
    user checks table cell contains    1    2    ees-analyst-%{RUN_IDENTIFIER}@education.gov.uk
    ...    testid:releaseContributors
    user checks table cell contains    1    2    Pending invite    testid:releaseContributors
    user checks table cell contains    1    3    Cancel invite    testid:releaseContributors

Cancel contributor invite
    user clicks button in table cell    1    3    Cancel invite    testid:releaseContributors
    user waits until modal is visible    Confirm cancelling of user invites
    ...    Are you sure you want to cancel all invites to releases under this publication for email address ees-analyst-%{RUN_IDENTIFIER}@education.gov.uk?
    user clicks button    Confirm
    user waits until modal is not visible    Confirm cancelling of user invites
    user waits until page does not contain element    testid:releaseContributors
    user checks page contains    There are no contributors or pending contributor invites for this release.


*** Keywords ***
user clicks remove user button for row
    [Arguments]    ${text}
    ${row}=    get webelement    xpath://tbody/tr/td[.="${text}"]/..
    ${remove_user_button}=    get child element    ${row}    xpath://button[text()="Remove"]
    user clicks element    ${remove_user_button}
