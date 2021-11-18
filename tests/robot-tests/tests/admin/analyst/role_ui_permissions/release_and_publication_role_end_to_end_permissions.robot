*** Settings ***
Library             ../../../libs/admin_api.py
Resource            ../../../libs/admin-common.robot
Resource            ../../../libs/common.robot
Resource            ../../../libs/admin/analyst/role_ui_permissions.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData    Footnotes

*** Variables ***
${PUBLICATION_NAME}     UI tests - publication_owner %{RUN_IDENTIFIER}
${RELEASE_TYPE}         Academic Year 2025/26
${RELEASE_NAME}         ${PUBLICATION_NAME} - ${RELEASE_TYPE}
${SUBJECT_NAME}         UI test subject

*** Test Cases ***
Create new publication and release via API
    ${PUBLICATION_ID}    user creates test publication via api    ${PUBLICATION_NAME}
    user create test release via api    ${PUBLICATION_ID}    AY    2025

Give Analyst1 User1 publication owner access
    user gives analyst publication owner access    ${PUBLICATION_NAME}

Sign in as Analyst1 User1 (publication owner)
    user changes to analyst1

Check publication owner can create methodology for publication
    user creates methodology for publication    ${PUBLICATION_NAME}
    user verifies methodology summary details    ${PUBLICATION_NAME}

Check publication owner can update methodology summary
    user edits methodology summary for publication
    ...    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME} - Updated methodology

Check publication owner cannot approve methodology for publication
    user cannot see the edit status controls for methodology

Check publication owner can upload subject file
    user navigates to editable release summary from admin dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_TYPE} (not Live)
    user uploads subject    ${SUBJECT_NAME}    seven_filters.csv    seven_filters.meta.csv

Check publication owner can add data guidance to ${SUBJECT_NAME}
    user clicks link    Data guidance
    user waits until page does not contain loading spinner
    user enters text into element    id:dataGuidanceForm-content    Test data guidance content
    user waits until page contains accordion section    ${SUBJECT_NAME}
    user enters text into data guidance data file content editor    ${SUBJECT_NAME}
    ...    data guidance content
    user clicks button    Save guidance

Navigate to 'Footnotes' page
    user waits for page to finish loading
    user clicks link    Footnotes
    user waits until h2 is visible    Footnotes

Check publication owner can add a footnote to ${SUBJECT_NAME}
    user waits until page contains link    Create footnote
    user clicks link    Create footnote
    user waits until page does not contain loading spinner
    user clicks footnote subject radio    ${SUBJECT_NAME}    Applies to all data
    user clicks element    id:footnoteForm-content
    user enters text into element    id:footnoteForm-content    test footnote from the publication owner! (analyst)
    user clicks button    Save footnote
    user waits until h2 is visible    Footnotes    60

Add public prerelease access list
    user clicks link    Pre-release access
    user waits until page does not contain loading spinner
    user creates public prerelease access list    Test public access list

Go to "Sign off" page
    user clicks link    Sign off
    user waits until page does not contain loading spinner
    user clicks button    Edit release status

Check publication owner can edit release status to "Ready for higher review"
    user clicks radio    Ready for higher review
    user enters text into element    id:releaseStatusForm-latestInternalReleaseNote
    ...    ready for higher review (publication owner)
    user clicks button    Update status
    user waits until element is visible    id:CurrentReleaseStatus-Awaiting higher review

Validates Release status table is correct
    user waits until page contains element    css:table
    user checks element count is x    xpath://table/tbody/tr    1
    ${datetime}    get current datetime    %-d %B %Y
    table cell should contain    css:table    2    1    ${datetime}    # Date
    table cell should contain    css:table    2    2    HigherLevelReview    # Status
    table cell should contain    css:table    2    3    ready for higher review (publication owner)    # Internal note
    table cell should contain    css:table    2    4    1    # Release version
    table cell should contain    css:table    2    5    ees-analyst1@education.gov.uk    # By user

Check publication owner can edit release status to "In draft"
    user clicks button    Edit release status
    user waits until h2 is visible    Edit release status    30
    user clicks radio    In draft
    user enters text into element    id:releaseStatusForm-latestInternalReleaseNote
    ...    Moving back to Draft state (publication owner)
    user clicks button    Update status

Validates Release status table is correct again
    user waits until page contains element    css:table
    user checks element count is x    xpath://table/tbody/tr    2
    ${datetime}    get current datetime    %-d %B %Y

    # New In draft row
    table cell should contain    css:table    2    1    ${datetime}    # Date
    table cell should contain    css:table    2    2    Draft    # Status
    # Internal note
    table cell should contain    css:table    2    3    Moving back to Draft state (publication owner)
    table cell should contain    css:table    2    4    1    # Release version
    table cell should contain    css:table    2    5    ees-analyst1@education.gov.uk    # By user

    # Higher review row
    table cell should contain    css:table    3    1    ${datetime}    # Date
    table cell should contain    css:table    3    2    HigherLevelReview    # Status
    table cell should contain    css:table    3    3    ready for higher review (publication owner)    # Internal note
    table cell should contain    css:table    3    4    1    # Release version
    table cell should contain    css:table    3    5    ees-analyst1@education.gov.uk    # By user

Check that a publication owner can make a new release
    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    user waits until page does not contain loading spinner
    user clicks link    Create new release
    user creates release for publication    ${PUBLICATION_NAME}    Academic Year Q1    2020

Check publication owner can upload subject file on new release
    user uploads subject    ${SUBJECT_NAME}    seven_filters.csv    seven_filters.meta.csv

Check publication owner can add data guidance to ${SUBJECT_NAME} on new release
    user clicks link    Data guidance
    user waits until page does not contain loading spinner
    user enters text into element    id:dataGuidanceForm-content    Test data guidance content
    user waits until page contains accordion section    ${SUBJECT_NAME}
    user enters text into data guidance data file content editor    ${SUBJECT_NAME}
    ...    data guidance content
    user clicks button    Save guidance

Swap the publication owner role for release approver to test approving the methodology
    user changes to bau1
    user removes publication owner access from analyst    ${PUBLICATION_NAME}
    user gives release access to analyst    ${RELEASE_NAME}    Approver

Check release approver can approve methodology for publication
    user changes to analyst1
    user views methodology for publication    ${PUBLICATION_NAME}
    approve methodology from methodology view

Swap the release approver role for publication owner to test removing the approved methodology
    user changes to bau1
    user removes release access from analyst    ${PUBLICATION_NAME}    ${RELEASE_TYPE}    Approver
    user gives analyst publication owner access    ${PUBLICATION_NAME}

Check publication owner cannot remove approved methodology
    user changes to analyst1
    ${accordion}    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    ${details}    user opens details dropdown    ${PUBLICATION_NAME}    ${accordion}
    user cannot see the remove controls for methodology    ${details}

Swap the publication owner role for release approver to test unapproving the methodology
    user changes to bau1
    user removes publication owner access from analyst    ${PUBLICATION_NAME}
    user gives release access to analyst    ${RELEASE_NAME}    Approver

Check release approver can unapprove methodology
    user changes to analyst1
    user views methodology for publication    ${PUBLICATION_NAME}
    user clicks link    Sign off
    user changes methodology status to Draft

Check release approver can approve methodology for publishing with the release
    approve methodology from methodology view
    ...    publishing_strategy=WithRelease
    ...    with_release=${RELEASE_NAME}

Check release approver can access release
    user navigates to editable release summary from admin dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_TYPE} (not Live)

Navigate to 'Footnotes' section
    user clicks link    Footnotes
    user waits until h2 is visible    Footnotes

Add a Footnote as a release approver
    user waits until page contains link    Create footnote
    user clicks link    Create footnote
    user waits until h2 is visible    Create footnote
    user clicks footnote subject radio    ${SUBJECT_NAME}    Applies to all data
    user clicks element    id:footnoteForm-content
    user enters text into element    id:footnoteForm-content
    ...    A footnote as analyst1 User1 with the release approver role
    user clicks button    Save footnote
    user waits until h2 is visible    Footnotes

Check release approver can create a release note
    user clicks link    Content
    user clicks button    Add note
    user enters text into element    id:createReleaseNoteForm-reason    Test release note one
    user clicks button    Save note
    ${date}    get current datetime    %-d %B %Y
    user waits until element contains    css:#releaseNotes li:nth-of-type(1) time    ${date}
    user waits until element contains    css:#releaseNotes li:nth-of-type(1) p    Test release note one

Check release approver can publish a release
    user clicks link    Sign off
    user approves original release for immediate publication

Swap the release approver role for publication owner now that the publication is live
    user changes to bau1
    user removes release access from analyst    ${PUBLICATION_NAME}    ${RELEASE_TYPE}    Approver
    user gives analyst publication owner access    ${PUBLICATION_NAME}

Check publication owner can create and cancel methodology amendments on a live publication
    user changes to analyst1
    user creates methodology amendment for publication    ${PUBLICATION_NAME}
    user cancels methodology amendment for publication    ${PUBLICATION_NAME}
    user creates methodology amendment for publication    ${PUBLICATION_NAME}

Swap the publication owner role for release approver to test approving methodology amendments
    user changes to bau1
    user removes publication owner access from analyst    ${PUBLICATION_NAME}
    user gives release access to analyst    ${RELEASE_NAME}    Approver

Check release approver can approve methodology amendments on a live publication
    user changes to analyst1
    user approves methodology amendment for publication    ${PUBLICATION_NAME}

Swap the release approver role for publication owner to test creating a new release amendment
    user changes to bau1
    user removes release access from analyst    ${PUBLICATION_NAME}    ${RELEASE_TYPE}    Approver
    user gives analyst publication owner access    ${PUBLICATION_NAME}

Check publication owner can create a new release amendment
    user changes to analyst1
    user creates amendment for release    ${PUBLICATION_NAME}    ${RELEASE_TYPE}    (Live - Latest release)

Create a new methodology amendment
    user creates methodology amendment for publication    ${PUBLICATION_NAME}

Swap the publication owner role for release approver to test approving the methodology amendment
    user changes to bau1
    user removes publication owner access from analyst    ${PUBLICATION_NAME}
    user gives release access to analyst    ${RELEASE_NAME}    Approver

Check release approver can approve the methodology amendment for publishing with the release amendment
    user changes to analyst1
    user approves methodology amendment for publication
    ...    publication=${PUBLICATION_NAME}
    ...    publishing_strategy=WithRelease
    ...    with_release=${RELEASE_NAME}

Swap the release approver role for publication owner to test that an approved methodology amendment cannot be cancelled
    user changes to bau1
    user removes release access from analyst    ${PUBLICATION_NAME}    ${RELEASE_TYPE}    Approver
    user gives analyst publication owner access    ${PUBLICATION_NAME}

Check publication owner cannot cancel approved methodology amendment
    user changes to analyst1
    ${accordion}    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    ${details}    user opens details dropdown    ${PUBLICATION_NAME}    ${accordion}
    user cannot see the cancel amendment controls for methodology    ${details}
