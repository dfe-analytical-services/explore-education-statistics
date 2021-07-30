*** Settings ***
Library             ../../../libs/admin_api.py
Resource            ../../../libs/admin-common.robot
Resource            ../../../libs/common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser

Force Tags          Admin    Local    Dev    AltersData    Footnotes

*** Variables ***
${PUBLICATION_NAME}     UI tests - publication_owner %{RUN_IDENTIFIER}
${RELEASE_TYPE}         Academic Year 2025/26
${RELEASE_NAME}         ${PUBLICATION_NAME} - ${RELEASE_TYPE}
${SUBJECT_NAME}         UI test subject

*** Test Cases ***
Create new publication and release via API
    [Tags]    HappyPath
    ${PUBLICATION_ID}    user creates test publication via api    ${PUBLICATION_NAME}
    user create test release via api    ${PUBLICATION_ID}    AY    2025

Give Analyst1 User1 publication owner access
    [Tags]    HappyPath
    user gives analyst publication owner access    ${PUBLICATION_NAME}

Sign in as Analyst1 User1 (publication owner)
    [Tags]    HappyPath
    user changes to analyst1

Check publication owner can create methodology for publication
    user creates methodology for publication    ${PUBLICATION_NAME}
    user checks summary list contains    Title    ${PUBLICATION_NAME}
    user checks summary list contains    Status    Draft
    user checks summary list contains    Published on    Not yet published

Check publication owner can approve methodology for publication
    user clicks link    Sign off
    user changes methodology status to Approved

Check publication owner can upload subject file
    [Tags]    HappyPath
    user navigates to editable release summary from admin dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_TYPE} (not Live)
    user clicks link    Data and files
    user waits until page does not contain loading spinner
    user uploads subject    ${SUBJECT_NAME}    seven_filters.csv    seven_filters.meta.csv

Check publication owner can add meta guidance to ${SUBJECT_NAME}
    [Tags]    HappyPath
    user clicks link    Metadata guidance
    user waits until page does not contain loading spinner
    user enters text into element    id:metaGuidanceForm-content    Test meta guidance content
    user waits until page contains accordion section    ${SUBJECT_NAME}
    user enters text into meta guidance data file content editor    ${SUBJECT_NAME}
    ...    meta guidance content
    user clicks button    Save guidance

Navigate to 'Footnotes' page
    [Tags]    HappyPath
    user waits for page to finish loading
    user clicks link    Footnotes
    user waits until h2 is visible    Footnotes

Check publication owner can add a footnote to ${SUBJECT_NAME}
    [Tags]    HappyPath
    user waits until page contains link    Create footnote
    user clicks link    Create footnote
    user waits until page does not contain loading spinner
    user clicks footnote subject radio    ${SUBJECT_NAME}    Applies to all data
    user clicks element    id:footnoteForm-content
    user enters text into element    id:footnoteForm-content    test footnote from the publication owner! (analyst)
    user clicks button    Save footnote
    user waits until h2 is visible    Footnotes    60

Add public prerelease access list
    [Tags]    HappyPath
    user clicks link    Pre-release access
    user waits until page does not contain loading spinner
    user creates public prerelease access list    Test public access list

Go to "Sign off" page
    [Tags]    HappyPath
    user clicks link    Sign off
    user waits until page does not contain loading spinner
    user clicks button    Edit release status

Check publication owner can edit release status to "Ready for higher review"
    [Tags]    HappyPath
    user clicks radio    Ready for higher review
    user enters text into element    id:releaseStatusForm-latestInternalReleaseNote
    ...    ready for higher review (publication owner)
    user clicks button    Update status
    user waits until element is visible    id:CurrentReleaseStatus-Awaiting higher review

Check publication owner can edit release status to "In draft"
    [Tags]    HappyPath
    user clicks button    Edit release status
    user waits until h2 is visible    Edit release status    30
    user clicks radio    In draft
    user enters text into element    id:releaseStatusForm-latestInternalReleaseNote
    ...    Moving back to Draft state (publication owner)
    user clicks button    Update status

Check that a publication owner can make a new release
    [Tags]    HappyPath
    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    user waits until page does not contain loading spinner
    user clicks link    Create new release
    user creates release for publication    ${PUBLICATION_NAME}    Academic Year Q1    2020

Check publication owner can upload subject file on new release
    [Tags]    HappyPath
    user clicks link    Data and files
    user uploads subject    ${SUBJECT_NAME}    seven_filters.csv    seven_filters.meta.csv

Check publication owner can add meta guidance to ${SUBJECT_NAME} on new release
    [Tags]    HappyPath
    user clicks link    Metadata guidance
    user waits until page does not contain loading spinner
    user enters text into element    id:metaGuidanceForm-content    Test meta guidance content
    user waits until page contains accordion section    ${SUBJECT_NAME}
    user enters text into meta guidance data file content editor    ${SUBJECT_NAME}
    ...    meta guidance content
    user clicks button    Save guidance

Navigate to administration as bau1 and swap publication owner role for release approver
    [Tags]    HappyPath
    user changes to bau1
    user removes publication owner access from analyst    ${PUBLICATION_NAME}
    user gives release access to analyst    ${RELEASE_NAME}    Approver

Check release approver can access release
    [Tags]    HappyPath
    user changes to analyst1
    user navigates to editable release summary from admin dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_TYPE} (not Live)

Navigate to 'Footnotes' section
    [Tags]    HappyPath
    user clicks link    Footnotes
    user waits until h2 is visible    Footnotes

Add a Footnote as a release approver
    [Tags]    HappyPath
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
    [Tags]    HappyPath
    user clicks link    Content
    user clicks button    Add note
    user enters text into element    id:createReleaseNoteForm-reason    Test release note one
    user clicks button    Save note
    ${date}    get current datetime    %-d %B %Y
    user waits until element contains    css:#releaseNotes li:nth-of-type(1) time    ${date}
    user waits until element contains    css:#releaseNotes li:nth-of-type(1) p    Test release note one

Check release approver can publish a release
    user clicks link    Sign off
    user approves release for immediate publication
