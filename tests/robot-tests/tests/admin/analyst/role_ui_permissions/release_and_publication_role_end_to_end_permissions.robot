*** Settings ***
Library             ../../../libs/admin_api.py
Resource            ../../../libs/admin-common.robot
Resource            ../../../libs/common.robot
Resource            ../../../libs/admin/manage-content-common.robot
Resource            ../../../libs/admin/analyst/role_ui_permissions.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData    Footnotes


*** Variables ***
${PUBLICATION_NAME}     Publication_drafter %{RUN_IDENTIFIER}
${RELEASE_TYPE}         Academic year 2025/26
${RELEASE_NAME}         ${PUBLICATION_NAME} - ${RELEASE_TYPE}
${SUBJECT_NAME}         UI test subject


*** Test Cases ***
Create new publication and release via API
    ${PUBLICATION_ID}    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    AY    2025

Give Analyst1 User1 publication drafter access
    user gives analyst publication drafter access    ${PUBLICATION_NAME}

Sign in as Analyst1 User1 (publication drafter)
    user changes to analyst1

Check publication drafter can create methodology for publication
    user creates methodology for publication    ${PUBLICATION_NAME}

Check publication drafter can update methodology summary
    user edits methodology summary for publication
    ...    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME} - Updated methodology

Check publication drafter cannot approve methodology for publication
    user clicks link    Sign off
    user waits until page finishes loading
    user clicks button    Edit status

    user waits until h2 is visible    Edit methodology status
    user checks element is enabled    id:methodologyStatusForm-status-Draft
    user checks element is enabled    id:methodologyStatusForm-status-HigherLevelReview
    user checks element is disabled    id:methodologyStatusForm-status-Approved

Check publication drafter can upload subject file
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_TYPE}
    user uploads subject and waits until complete    ${SUBJECT_NAME}    seven_filters.csv    seven_filters.meta.csv

Check publication drafter can add data guidance to ${SUBJECT_NAME}
    user navigates to Data Guidance page and adds data guidance for subject    ${SUBJECT_NAME}
    ...    data guidance content

Navigate to 'Footnotes' page
    user waits until page finishes loading
    user clicks link    Footnotes
    user waits until h2 is visible    Footnotes

Check publication drafter can add a footnote to ${SUBJECT_NAME}
    user waits until page contains link    Create footnote
    user clicks link    Create footnote
    user waits until page finishes loading
    user clicks footnote subject radio    ${SUBJECT_NAME}    Applies to all data
    user clicks element    id:footnoteForm-content
    user enters text into element    id:footnoteForm-content    test footnote from the publication drafter! (analyst)
    user clicks button    Save footnote
    user waits until h2 is visible    Footnotes    %{WAIT_SMALL}

Add headline text block to Content page
    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Add public prerelease access list
    user clicks link    Pre-release access
    user waits until page finishes loading
    user creates public prerelease access list    Test public access list

Go to "Sign off" page
    user clicks link    Sign off
    user waits until page finishes loading
    user clicks button    Edit release status

Check publication drafter can edit release status to "Ready for higher review"
    user clicks radio    Ready for higher review (this will notify approvers)
    user enters text into element    id:releaseStatusForm-internalReleaseNote
    ...    ready for higher review (publication drafter)
    user clicks button    Update status
    user waits until element is visible    id:CurrentReleaseStatus-Awaiting higher review

Validates Release status table is correct
    user waits until page contains element    css:table
    user checks element count is x    xpath://table/tbody/tr    1
    ${date}    get london date
    table cell should contain    css:table    2    1    ${date}    # Date
    table cell should contain    css:table    2    2    HigherLevelReview    # Status
    # Internal note
    table cell should contain    css:table    2    3    ready for higher review (publication drafter)
    table cell should contain    css:table    2    4    1    # Release version
    table cell should contain    css:table    2    5    ees-test.analyst1@education.gov.uk    # By user

Check publication drafter can edit release status to "In draft"
    user puts release into draft    release_note=Moving back to Draft state (publication drafter)

Validates Release status table is correct again
    user waits until page contains element    css:table
    user checks element count is x    xpath://table/tbody/tr    2
    ${date}    get london date

    # New In draft row
    table cell should contain    css:table    2    1    ${date}    # Date
    table cell should contain    css:table    2    2    Draft    # Status
    # Internal note
    table cell should contain    css:table    2    3    Moving back to Draft state (publication drafter)
    table cell should contain    css:table    2    4    1    # Release version
    table cell should contain    css:table    2    5    ees-test.analyst1@education.gov.uk    # By user

    # Higher review row
    table cell should contain    css:table    3    1    ${date}    # Date
    table cell should contain    css:table    3    2    HigherLevelReview    # Status
    # Internal note
    table cell should contain    css:table    3    3    ready for higher review (publication drafter)
    table cell should contain    css:table    3    4    1    # Release version
    table cell should contain    css:table    3    5    ees-test.analyst1@education.gov.uk    # By user

Check that a publication drafter can make a new release
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user creates release from publication page    ${PUBLICATION_NAME}    Academic year Q1    2020

Check publication drafter can upload subject file on new release
    user uploads subject and waits until complete    ${SUBJECT_NAME}    seven_filters.csv    seven_filters.meta.csv

Check publication drafter can add data guidance to ${SUBJECT_NAME} on new release
    user navigates to Data Guidance page and adds data guidance for subject    ${SUBJECT_NAME}
    ...    data guidance content

Swap the publication drafter role for publication approver to test approving the methodology
    user changes to bau1
    user removes publication drafter access from analyst    ${PUBLICATION_NAME}
    user gives analyst publication approver access    ${PUBLICATION_NAME}

Check publication approver can approve methodology for publication
    user changes to analyst1
    user navigates to methodology    ${PUBLICATION_NAME}    ${PUBLICATION_NAME} - Updated methodology

    user clicks link    Sign off
    user waits until page finishes loading
    user clicks button    Edit status

    user waits until h2 is visible    Edit methodology status
    user checks element is enabled    id:methodologyStatusForm-status-Draft
    user checks element is enabled    id:methodologyStatusForm-status-HigherLevelReview
    user checks element is enabled    id:methodologyStatusForm-status-Approved

    user clicks button    Cancel
    user waits until h2 is not visible    Edit methodology status
    user changes methodology status to Approved

Swap the publication approver role for publication drafter to test removing the approved methodology
    user changes to bau1
    user removes publication approver access from analyst    ${PUBLICATION_NAME}
    user gives analyst publication drafter access    ${PUBLICATION_NAME}

Check publication drafter cannot remove approved methodology
    user changes to analyst1
    user navigates to methodologies on publication page    ${PUBLICATION_NAME}
    user cannot see the remove controls for methodology

Swap the publication drafter role for publication approver to test unapproving the methodology
    user changes to bau1
    user removes publication drafter access from analyst    ${PUBLICATION_NAME}
    user gives analyst publication approver access    ${PUBLICATION_NAME}

Check publication approver can unapprove methodology
    user changes to analyst1
    user navigates to methodology    ${PUBLICATION_NAME}    ${PUBLICATION_NAME} - Updated methodology
    user changes methodology status to Draft

Check publication approver can approve methodology for publishing with the release
    approve methodology from methodology view
    ...    publishing_strategy=WithRelease
    ...    with_release=${RELEASE_NAME}

Check publication approver can access release
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_TYPE}

Navigate to 'Footnotes' section
    user clicks link    Footnotes
    user waits until h2 is visible    Footnotes

Add a Footnote as a publication approver
    user waits until page contains link    Create footnote
    user clicks link    Create footnote
    user waits until h2 is visible    Create footnote
    user clicks footnote subject radio    ${SUBJECT_NAME}    Applies to all data
    user clicks element    id:footnoteForm-content
    user enters text into element    id:footnoteForm-content
    ...    A footnote as analyst1 User1 with the publication approver role
    user clicks button    Save footnote
    user waits until h2 is visible    Footnotes

Check publication approver can create a release note
    user clicks link    Content
    user adds a release note    Test release note one
    ${date}    get london date
    user waits until element contains    css:#release-notes li:nth-of-type(1) time    ${date}
    user waits until element contains    css:#release-notes li:nth-of-type(1) p    Test release note one

Check publication approver can publish a release
    user approves original release for immediate publication

Swap the publication approver role for publication drafter now that the publication is live
    user changes to bau1
    user removes publication approver access from analyst    ${PUBLICATION_NAME}
    user gives analyst publication drafter access    ${PUBLICATION_NAME}

Check publication drafter can create and cancel methodology amendments on a live publication
    user changes to analyst1
    user creates methodology amendment for publication    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME} - Updated methodology
    user cancels methodology amendment for publication    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME} - Updated methodology
    user creates methodology amendment for publication    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME} - Updated methodology

Swap the publication drafter role for publication approver to test approving methodology amendments
    user changes to bau1
    user removes publication drafter access from analyst    ${PUBLICATION_NAME}
    user gives analyst publication approver access    ${PUBLICATION_NAME}

Check publication approver can approve methodology amendments on a live publication
    user changes to analyst1
    user approves methodology amendment for publication    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME} - Updated methodology

Swap the publication approver role for publication drafter to test creating a new release amendment
    user changes to bau1
    user removes publication approver access from analyst    ${PUBLICATION_NAME}
    user gives analyst publication drafter access    ${PUBLICATION_NAME}

Check publication drafter can create a new release amendment
    user changes to analyst1
    user creates amendment for release    ${PUBLICATION_NAME}    ${RELEASE_TYPE}

Create a new methodology amendment
    user creates methodology amendment for publication    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME} - Updated methodology

Swap the publication drafter role for publication approver to test approving the methodology amendment
    user changes to bau1
    user removes publication drafter access from analyst    ${PUBLICATION_NAME}
    user gives analyst publication approver access    ${PUBLICATION_NAME}

Check publication approver can approve the methodology amendment for publishing with the release amendment
    user changes to analyst1
    user approves methodology amendment for publication
    ...    publication=${PUBLICATION_NAME}
    ...    methodology_title=${PUBLICATION_NAME} - Updated methodology
    ...    publishing_strategy=WithRelease
    ...    with_release=${RELEASE_NAME}

Swap the publication approver role for publication drafter to test that an approved methodology amendment cannot be cancelled
    user changes to bau1
    user removes publication approver access from analyst    ${PUBLICATION_NAME}
    user gives analyst publication drafter access    ${PUBLICATION_NAME}

Check publication drafter cannot cancel approved methodology amendment
    user changes to analyst1
    user navigates to methodologies on publication page    ${PUBLICATION_NAME}

    user cannot see the cancel amendment controls for methodology
