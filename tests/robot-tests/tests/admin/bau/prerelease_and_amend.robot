*** Settings ***
Library             Collections
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot
Resource            ../../libs/admin/manage-content-common.robot
Resource            ../../libs/tables-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData


*** Variables ***
${PUBLICATION_NAME}=                Prerelease and amend %{RUN_IDENTIFIER}
${RELEASE_NAME}=                    Calendar year 2000
${SCHEDULED_PRERELEASE_WARNING}=
...                                 Pre-release users will have access to a preview of the release 24 hours before the scheduled publish date.
${IMMEDIATE_PRERELEASE_WARNING}=
...                                 Pre-release users will not have access to a preview of the release if it is published immediately.


*** Test Cases ***
Create test publication and release via API
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    CY    2000

Upload subject
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    Calendar year 2000

    user uploads subject and waits until complete    UI test subject    upload-file-test.csv
    ...    upload-file-test.meta.csv

Add metadata guidance
    user clicks link    Data guidance
    user waits until h2 is visible    Public data guidance

    user waits until page contains element    id:dataGuidance-dataFiles

    user checks summary list contains    Filename    upload-file-test.csv
    user checks summary list contains    Geographic levels
    ...    Local authority; Local authority district; Local enterprise partnership; Opportunity area; Parliamentary constituency; RSC region; Regional; Ward
    user checks summary list contains    Time period    2005 to 2020

    user opens details dropdown    Variable names and descriptions

    user checks table column heading contains    1    1    Variable name    css:table[data-testid="Variables"]
    user checks table column heading contains    1    2    Variable description    css:table[data-testid="Variables"]

    user checks table cell contains    1    1    admission_numbers    id:dataGuidance-dataFiles
    user checks table cell contains    1    2    Admission Numbers    id:dataGuidance-dataFiles

    user enters text into element    id:dataGuidanceForm-content    Test metadata guidance content
    user enters text into element    id:dataGuidanceForm-dataSets-0-content    Test file guidance content

    user clicks button    Save guidance

Add headline text block to Content page
    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Test headlines summary text for ${PUBLICATION_NAME}

Add public prerelease access list
    user clicks link    Pre-release access
    user waits until h2 is visible    Manage pre-release user access
    user creates public prerelease access list    Initial test public access list

Go to "Sign off" page
    user clicks link    Sign off
    user waits until h2 is visible    Sign off

Check that there are no Pre-release warnings
    user clicks link    Sign off
    user waits until h2 is visible    Sign off

    user clicks button    Edit release status
    user waits until h2 is visible    Edit release status
    user clicks radio    Approved for publication
    user enters text into element    id:releaseStatusForm-internalReleaseNote    Approved by UI tests
    user clicks radio    Immediately
    user checks page does not contain    WARNING
    user checks page does not contain    ${IMMEDIATE_PRERELEASE_WARNING}
    user clicks radio    On a specific date
    user checks page does not contain    WARNING
    user checks page does not contain    ${SCHEDULED_PRERELEASE_WARNING}

Invite Pre-release users
    user clicks link    Pre-release access
    user waits until h2 is visible    Manage pre-release user access
    ${emails}=    Catenate    SEPARATOR=\n
    ...    simulate-delivered-prerelease-and-amend-1@notifications.service.gov.uk
    ...    simulate-delivered-prerelease-and-amend-2@notifications.service.gov.uk
    user enters text into element    css:textarea[name="emails"]    ${emails}
    user clicks button    Invite new users
    ${modal}=    user waits until modal is visible    Confirm pre-release invitations
    user waits until element contains    ${modal}
    ...    Email notifications will be sent when the release is approved for publication.

    user checks list has x items    testid:invitableList    2    ${modal}
    user checks list item contains    testid:invitableList    1
    ...    simulate-delivered-prerelease-and-amend-1@notifications.service.gov.uk    ${modal}
    user checks list item contains    testid:invitableList    2
    ...    simulate-delivered-prerelease-and-amend-2@notifications.service.gov.uk    ${modal}
    user clicks button    Confirm

Go to "Sign off" page again
    user clicks link    Sign off
    user waits until h2 is visible    Sign off

Approve release again
    user clicks link    Sign off
    user waits until h2 is visible    Sign off
    user clicks button    Edit release status
    user waits until h2 is visible    Edit release status
    user clicks radio    Approved for publication
    user enters text into element    id:releaseStatusForm-internalReleaseNote    Approved by UI tests
    user clicks radio    On a specific date
    user waits until page contains    ${SCHEDULED_PRERELEASE_WARNING}
    user clicks radio    Immediately
    user checks page contains    ${IMMEDIATE_PRERELEASE_WARNING}
    user clicks button    Update status
    user waits until h2 is visible    Sign off
    user checks summary list contains    Current status    Approved
    user waits until page contains element    id:release-process-status-Complete    %{WAIT_MEDIUM}

user creates amendment for release
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}

    user waits until page contains element    testid:publication-published-releases
    ${ROW}=    user gets table row    ${RELEASE_NAME}    testid:publication-published-releases
    user clicks element    xpath://*[text()="Amend"]    ${ROW}

    ${modal}=    user waits until modal is visible    Confirm you want to amend this published release
    user clicks button    Confirm    ${modal}
    user waits until page contains title    ${PUBLICATION_NAME}
    user waits until page contains title caption    Amend release for ${RELEASE_NAME}
    user checks page contains tag    Amendment

Add basic release content
    user navigates to content page    ${PUBLICATION_NAME}

    # FALSE to not add headline block, as we needed to add that to publish the original release
    user adds basic release content    ${PUBLICATION_NAME}    ${False}

Add release note to amendment
    user clicks button    Add note
    user enters text into element    id:create-release-note-form-reason    Test release note
    user clicks button    Save note
    ${date}=    get london date
    user waits until element contains    css:#release-notes li:nth-of-type(1) time    ${date}
    user waits until element contains    css:#release-notes li:nth-of-type(1) p    Test release note

Check amendment has no prerelease users
    user clicks link    Pre-release access
    user waits until h2 is visible    Manage pre-release user access
    user waits until page contains button    Invite new users
    user checks page contains    No pre-release users have been invited.

Check that there is no Pre-release warning text on the sign off page during amendment
    user clicks link    Sign off
    user waits until h2 is visible    Sign off
    user clicks button    Edit release status
    user waits until h2 is visible    Edit release status
    user clicks radio    Approved for publication
    user enters text into element    id:releaseStatusForm-internalReleaseNote    Approved by UI tests
    user clicks radio    Immediately
    user checks page does not contain    WARNING
    user checks page does not contain    ${IMMEDIATE_PRERELEASE_WARNING}
    user clicks radio    On a specific date
    user checks page does not contain    WARNING
    user checks page does not contain    ${SCHEDULED_PRERELEASE_WARNING}

Invite Pre-release users during amendment
    user clicks link    Pre-release access
    user waits until h2 is visible    Manage pre-release user access
    ${emails}=    Catenate    SEPARATOR=\n
    ...    simulate-delivered-prerelease-and-amend-3@notifications.service.gov.uk
    ...    EES-test.ANALYST1@education.gov.uk
    user enters text into element    css:textarea[name="emails"]    ${emails}
    user clicks button    Invite new users
    ${modal}=    user waits until modal is visible    Confirm pre-release invitations
    user waits until element contains    ${modal}
    ...    Email notifications will be sent when the release is approved for publication.

    user checks list has x items    testid:invitableList    2    ${modal}
    user checks list item contains    testid:invitableList    1
    ...    simulate-delivered-prerelease-and-amend-3@notifications.service.gov.uk    ${modal}
    user checks list item contains    testid:invitableList    2    EES-test.ANALYST1@education.gov.uk    ${modal}
    user clicks button    Confirm

Check amendment has original release's public access list and then update
    user clicks link    Public access list
    user checks page contains    Initial test public access list
    user updates public prerelease access list    Amended test public access list

Validate prerelease has not started for Analyst user during amendment as it is still in draft
    ${current_url}=    get location
    ${RELEASE_URL}=    remove substring from right of string    ${current_url}
    ...    /prerelease-access#preReleaseAccess-publicList
    set suite variable    ${RELEASE_URL}
    user changes to analyst1
    user navigates to    ${RELEASE_URL}/prerelease/content
    user waits until h1 is visible    Forbidden

Approve amendment for a scheduled release and check warning text
    user changes to bau1
    ${day}=    get london day of month    offset_days=2
    ${month}=    get london month date    offset_days=2
    ${month_word}=    get london month word    offset_days=2
    ${year}=    get london year    offset_days=2
    user navigates to    ${RELEASE_URL}/status
    user clicks button    Edit release status
    user clicks radio    Approved for publication
    user enters text into element    id:releaseStatusForm-internalReleaseNote    Approved by prerelease UI tests
    user clicks radio    Immediately
    user checks page contains    ${IMMEDIATE_PRERELEASE_WARNING}
    user waits until page contains element    xpath://label[text()="On a specific date"]/../input
    user clicks radio    On a specific date
    user checks page contains    ${SCHEDULED_PRERELEASE_WARNING}
    user waits until page contains    Publish date
    user enters text into element    id:releaseStatusForm-publishScheduled-day    ${day}
    user enters text into element    id:releaseStatusForm-publishScheduled-month    ${month}
    user enters text into element    id:releaseStatusForm-publishScheduled-year    ${year}
    user clicks button    Update status
    user waits until h2 is visible    Confirm publish date
    user clicks button    Confirm

    user checks summary list contains    Current status    Approved
    user checks summary list contains    Scheduled release    ${day} ${month_word} ${year}
    user waits for release process status to be    Scheduled    %{WAIT_MEDIUM}

Validate prerelease window is not yet open for Analyst user
    user changes to analyst1
    user navigates to    ${RELEASE_URL}/prerelease/content

    user waits until h1 is visible    Pre-release access is not yet available
    user checks breadcrumb count should be    2
    user checks nth breadcrumb contains    1    Home
    user checks nth breadcrumb contains    2    Pre-release access

    ${start_date}=    get london date    offset_days=1
    ${end_date}=    get london date    offset_days=2

    user checks page contains
    ...    Pre-release access will be available from ${start_date} at 00:00 until it is published on ${end_date}.

Start prerelease
    user changes to bau1
    ${day}=    get london day of month    offset_days=1
    ${month}=    get london month date    offset_days=1
    ${month_word}=    get london month word    offset_days=1
    ${year}=    get london year    offset_days=1
    user navigates to    ${RELEASE_URL}/status
    user clicks button    Edit release status
    user clicks radio    On a specific date
    user checks page contains    ${SCHEDULED_PRERELEASE_WARNING}
    user enters text into element    id:releaseStatusForm-publishScheduled-day    ${day}
    user enters text into element    id:releaseStatusForm-publishScheduled-month    ${month}
    user enters text into element    id:releaseStatusForm-publishScheduled-year    ${year}
    user clicks button    Update status
    user waits until h2 is visible    Confirm publish date
    user clicks button    Confirm

    user checks summary list contains    Current status    Approved
    user checks summary list contains    Scheduled release    ${day} ${month_word} ${year}
    user waits for release process status to be    Scheduled    %{WAIT_MEDIUM}

Validate prerelease has started for Analyst user after amendment
    user changes to analyst1
    user navigates to    ${RELEASE_URL}/prerelease/content

    user checks breadcrumb count should be    2
    user checks nth breadcrumb contains    1    Home
    user checks nth breadcrumb contains    2    Pre-release access

    user waits until page contains title caption    Calendar year 2000    %{WAIT_SMALL}
    user waits until h1 is visible    ${PUBLICATION_NAME}

    user waits until element contains    id:releaseSummary    Test summary text for ${PUBLICATION_NAME}
    user waits until element contains    id:releaseHeadlines    Test headlines summary text for ${PUBLICATION_NAME}

Validate contact banner is shown
    user checks testid element contains    notificationBanner    If you have an enquiry about this release contact
    user checks testid element contains    notificationBanner    UI test team name: ui_test@test.com

Validate public prerelease access list as Analyst user
    user clicks link    Pre-release access list

    user waits until page contains title caption    Calendar year 2000    %{WAIT_SMALL}
    user waits until h1 is visible    ${PUBLICATION_NAME}

    user waits until h2 is visible    Pre-release access list    %{WAIT_SMALL}
    user waits until page contains    Amended test public access list    %{WAIT_SMALL}


*** Keywords ***
do suite teardown
    user closes the browser
    delete user invite via api    simulate-delivered-prerelease-and-amend-1@notifications.service.gov.uk
    delete user invite via api    simulate-delivered-prerelease-and-amend-2@notifications.service.gov.uk
    delete user invite via api    simulate-delivered-prerelease-and-amend-3@notifications.service.gov.uk
