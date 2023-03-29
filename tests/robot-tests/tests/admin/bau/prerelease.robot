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
${TOPIC_NAME}=                              %{TEST_TOPIC_NAME}
${PUBLICATION_NAME}=                        UI tests - prerelease %{RUN_IDENTIFIER}
${DATABLOCK_NAME}=                          UI test table
${DATABLOCK_FEATURED_NAME}=                 UI test featured table name
${DATABLOCK_FEATURED_TABLE_DESCRIPTION}=    UI test featured table description


*** Test Cases ***
Create test publication and release via API
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    CY    2000

Upload subject
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    Calendar year 2000

    user uploads subject    UI test subject    upload-file-test.csv    upload-file-test.meta.csv

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
    user enters text into element    id:dataGuidanceForm-subjects-0-content    Test file guidance content

    user clicks button    Save guidance

    user waits until page contains button    Edit guidance

Add featured table
    user clicks link    Data blocks
    user waits until h2 is visible    Data blocks

    user clicks link    Create data block
    user waits until table tool wizard step is available    1    Choose a subject

    user waits until page contains    UI test subject
    user clicks radio    UI test subject
    user clicks element    id:publicationSubjectForm-submit

    user chooses location, time period and filters
    user validates table rows

    user enters text into element    id:dataBlockDetailsForm-name    ${DATABLOCK_NAME}
    user enters text into element    id:dataBlockDetailsForm-heading    UI test table title
    user enters text into element    id:dataBlockDetailsForm-source    UI test source

    user clicks checkbox    Set as a featured table for this publication
    user waits until page contains element    id:dataBlockDetailsForm-highlightName
    user enters text into element    id:dataBlockDetailsForm-highlightName    ${DATABLOCK_FEATURED_NAME}
    user enters text into element    id:dataBlockDetailsForm-highlightDescription
    ...    ${DATABLOCK_FEATURED_TABLE_DESCRIPTION}

    user clicks button    Save data block
    user waits until page contains    Delete this data block

Add basic release content
    user clicks link    Content
    user waits until h1 is visible    ${PUBLICATION_NAME}
    user waits until h2 is visible    ${PUBLICATION_NAME}
    user adds basic release content    ${PUBLICATION_NAME}

Add public prerelease access list
    user clicks link    Pre-release access
    user waits until h2 is visible    Manage pre-release user access
    user creates public prerelease access list    Initial test public access list

Update public prerelease access list
    user updates public prerelease access list    Updated test public access list

Go to "Sign off" page
    user clicks link    Sign off
    user waits until h2 is visible    Sign off
    user waits until page contains button    Edit release status

Approve release and wait for it to be Scheduled
    ${day}=    get current datetime    %-d    2
    ${month}=    get current datetime    %-m    2
    ${month_word}=    get current datetime    %B    2
    ${year}=    get current datetime    %Y    2

    user clicks button    Edit release status
    user clicks radio    Approved for publication
    user enters text into element    id:releaseStatusForm-internalReleaseNote    Approved by prerelease UI tests
    user waits until page contains element    xpath://label[text()="On a specific date"]/../input
    user clicks radio    On a specific date
    user waits until page contains    Publish date
    user enters text into element    id:releaseStatusForm-publishScheduled-day    ${day}
    user enters text into element    id:releaseStatusForm-publishScheduled-month    ${month}
    user enters text into element    id:releaseStatusForm-publishScheduled-year    ${year}
    user enters text into element    id:releaseStatusForm-nextReleaseDate-month    1
    user enters text into element    id:releaseStatusForm-nextReleaseDate-year    2001
    user clicks button    Update status
    user waits until h2 is visible    Confirm publish date
    user clicks button    Confirm

    user checks summary list contains    Current status    Approved
    user checks summary list contains    Scheduled release    ${day} ${month_word} ${year}
    user checks summary list contains    Next release expected    January 2001
    user waits for release process status to be    Scheduled    %{WAIT_MEDIUM}

Navigate to prerelease page
    ${current_url}=    get location
    ${RELEASE_URL}=    remove substring from right of string    ${current_url}    /status
    set suite variable    ${RELEASE_URL}
    user navigates to admin frontend    ${RELEASE_URL}/prerelease/content

Validate prerelease has not started
    user waits until h1 is visible    Pre-release access is not yet available
    user checks breadcrumb count should be    2
    user checks nth breadcrumb contains    1    Home
    user checks nth breadcrumb contains    2    Pre-release access

    ${day}=    get current datetime    %d    1
    ${month}=    get current datetime    %m    1
    ${year}=    get current datetime    %Y    1
    ${time_start}=    format uk to local datetime    ${year}-${month}-${day}T00:00:00    %-d %B %Y at %H:%M
    ${time_end}=    format uk to local datetime    ${year}-${month}-${day}T23:59:00    %-d %B %Y at %H:%M
    user checks page contains    Pre-release access will be available from ${time_start} until ${time_end}.

Go to prerelease access page
    user navigates to admin frontend    ${RELEASE_URL}/prerelease-access
    user waits until h2 is visible    Manage pre-release user access

Check the invite emails field is required
    user clicks button    Invite new users
    user waits until element contains    id:preReleaseUserAccessForm-emails-error
    ...    Enter 1 or more email addresses

Check the invite emails field is invalid for invalid email addresses
    ${emails}=    Catenate    SEPARATOR=\n
    ...    EES-test.ANALYST1@education.gov.uk
    ...    test@test.com
    ...    invalid-1
    ...    invalid-2
    user enters text into element    css:textarea[name="emails"]    ${emails}
    user clicks button    Invite new users
    user waits until element contains    id:preReleaseUserAccessForm-emails-error
    ...    'invalid-1' is not a valid email address

Invite users to the prerelease
    ${emails}=    Catenate    SEPARATOR=\n
    ...    simulate-delivered@notifications.service.gov.uk
    ...    EES-test.ANALYST1@education.gov.uk
    user enters text into element    css:textarea[name="emails"]    ${emails}
    user clicks button    Invite new users
    ${modal}=    user waits until modal is visible    Confirm pre-release invitations
    user waits until element contains    ${modal}    Email notifications will be sent immediately

    user checks list has x items    testid:invitableList    2    ${modal}
    user checks list item contains    testid:invitableList    1    simulate-delivered@notifications.service.gov.uk
    ...    ${modal}
    user checks list item contains    testid:invitableList    2    EES-test.ANALYST1@education.gov.uk    ${modal}
    user clicks button    Confirm
    user checks table column heading contains    1    1    User email
    user checks table cell contains    1    1    simulate-delivered@notifications.service.gov.uk
    user checks table cell contains    2    1    EES-test.ANALYST1@education.gov.uk

Refresh page and check prerelease user list isn't duplicated
    [Documentation]    EES-3535
    user reloads page
    user waits until table is visible
    user checks table body has x rows    2
    user checks table cell in offset row contains    1    0    1    ees-test.analyst1@education.gov.uk
    user checks table cell in offset row contains    2    0    1    simulate-delivered@notifications.service.gov.uk

Check the invite emails field is invalid for addresses that are all already invited or accepted
    ${emails}=    Catenate    SEPARATOR=\n
    ...    simulate-delivered@notifications.service.gov.uk
    ...    EES-test.ANALYST1@education.gov.uk
    user enters text into element    css:textarea[name="emails"]    ${emails}
    user clicks button    Invite new users
    user waits until element contains    id:preReleaseUserAccessForm-emails-error
    ...    All of the email addresses have already been invited or accepted

Invite a further list of new users but mixed with existing invitees and accepted users
    ${emails}=    Catenate    SEPARATOR=\n
    ...    simulate-delivered@notifications.service.gov.uk
    ...    simulate-delivered-2@notifications.service.gov.uk
    ...    simulate-delivered-3@notifications.service.gov.uk
    ...    EES-test.ANALYST1@education.gov.uk
    user enters text into element    css:textarea[name="emails"]    ${emails}
    user clicks button    Invite new users
    ${modal}=    user waits until modal is visible    Confirm pre-release invitations
    user waits until element contains    ${modal}    Email notifications will be sent immediately

    user checks list has x items    testid:invitableList    2    ${modal}

    user checks list item contains    testid:invitableList    1    simulate-delivered-2@notifications.service.gov.uk
    ...    ${modal}
    user checks list item contains    testid:invitableList    2    simulate-delivered-3@notifications.service.gov.uk
    ...    ${modal}

    user checks list has x items    testid:acceptedList    1    ${modal}
    user checks list item contains    testid:acceptedList    1    EES-test.ANALYST1@education.gov.uk    ${modal}

    user checks list has x items    testid:invitedList    1    ${modal}
    user checks list item contains    testid:invitedList    1    simulate-delivered@notifications.service.gov.uk
    ...    ${modal}

    user waits until button is enabled    Confirm    10
    user clicks button    Confirm
    user waits until table is visible    css:body    %{WAIT_SMALL}
    user checks table column heading contains    1    1    User email

    user checks table cell contains    1    1    ees-test.analyst1@education.gov.uk
    user checks table cell contains    2    1    simulate-delivered@notifications.service.gov.uk
    user checks table cell contains    3    1    simulate-delivered-2@notifications.service.gov.uk
    user checks table cell contains    4    1    simulate-delivered-3@notifications.service.gov.uk

Validate prerelease has not started for Analyst user
    user changes to analyst1
    user navigates to admin frontend    ${RELEASE_URL}/prerelease/content

    user waits until h1 is visible    Pre-release access is not yet available
    user checks breadcrumb count should be    2
    user checks nth breadcrumb contains    1    Home
    user checks nth breadcrumb contains    2    Pre-release access

    ${day}=    get current datetime    %d    1
    ${month}=    get current datetime    %m    1
    ${year}=    get current datetime    %Y    1
    ${time_start}=    format uk to local datetime    ${year}-${month}-${day}T00:00:00    %-d %B %Y at %H:%M
    ${time_end}=    format uk to local datetime    ${year}-${month}-${day}T23:59:00    %-d %B %Y at %H:%M
    user checks page contains    Pre-release access will be available from ${time_start} until ${time_end}.

Start prerelease
    user changes to bau1
    ${day}=    get current datetime    %-d    1
    ${month}=    get current datetime    %-m    1
    ${month_word}=    get current datetime    %B    1
    ${year}=    get current datetime    %Y    1
    user navigates to admin frontend    ${RELEASE_URL}/status
    user clicks button    Edit release status
    user enters text into element    id:releaseStatusForm-publishScheduled-day    ${day}
    user enters text into element    id:releaseStatusForm-publishScheduled-month    ${month}
    user enters text into element    id:releaseStatusForm-publishScheduled-year    ${year}
    user clicks button    Update status
    user waits until h2 is visible    Confirm publish date
    user clicks button    Confirm

    user checks summary list contains    Current status    Approved
    user checks summary list contains    Scheduled release    ${day} ${month_word} ${year}
    user waits for release process status to be    Scheduled    %{WAIT_MEDIUM}

Validate prerelease has started
    ${current_url}=    get location
    ${RELEASE_URL}=    remove substring from right of string    ${current_url}    /status
    set suite variable    ${RELEASE_URL}
    user navigates to admin frontend    ${RELEASE_URL}/prerelease/content

    user checks breadcrumb count should be    2
    user checks nth breadcrumb contains    1    Home
    user checks nth breadcrumb contains    2    Pre-release access

    user waits until page contains title caption    Calendar year 2000    %{WAIT_SMALL}
    user waits until h1 is visible    ${PUBLICATION_NAME}

    user waits until element contains    id:releaseSummary    Test summary text for ${PUBLICATION_NAME}
    ...    %{WAIT_SMALL}
    user waits until element contains    id:releaseHeadlines    Test headlines summary text for ${PUBLICATION_NAME}
    ...    %{WAIT_SMALL}

Validate contact banner is shown
    user checks testid element contains    notificationBanner    If you have an enquiry about this release contact
    user checks testid element contains    notificationBanner    UI test team name: ui_test@test.com

Validate metadata guidance page
    user clicks link    Data guidance

    user waits until page contains title caption    Calendar year 2000    %{WAIT_SMALL}
    user waits until h1 is visible    ${PUBLICATION_NAME}

    user waits until h2 is visible    Data guidance    %{WAIT_SMALL}
    user waits until page contains    Test metadata guidance content    %{WAIT_SMALL}

    user waits until page contains accordion section    UI test subject    %{WAIT_SMALL}
    user checks there are x accordion sections    1

    user opens accordion section    UI test subject
    user checks summary list contains    Filename    upload-file-test.csv
    user checks summary list contains    Geographic levels
    ...    Local authority; Local authority district; Local enterprise partnership; Opportunity area; Parliamentary constituency; RSC region; Regional; Ward
    user checks summary list contains    Time period    2005 to 2020
    user checks summary list contains    Content    Test file guidance content

    user opens details dropdown    Variable names and descriptions

    user checks table column heading contains    1    1    Variable name
    user checks table column heading contains    1    2    Variable description

    user checks table cell contains    1    1    admission_numbers
    user checks table cell contains    1    2    Admission Numbers

Go back to prerelease content page
    user clicks link    Back
    user checks breadcrumb count should be    2
    user checks nth breadcrumb contains    1    Home
    user checks nth breadcrumb contains    2    Pre-release access

    user waits until page contains title caption    Calendar year 2000    %{WAIT_SMALL}
    user waits until h1 is visible    ${PUBLICATION_NAME}

Validate public prerelease access list
    user clicks link    Pre-release access list
    user waits until page contains title caption    Calendar year 2000    %{WAIT_SMALL}
    user waits until h1 is visible    ${PUBLICATION_NAME}
    user waits until h2 is visible    Pre-release access list    %{WAIT_SMALL}
    user waits until page contains    Updated test public access list    %{WAIT_SMALL}

Go back to prerelease content page again
    user clicks link    Back
    user waits until h1 is visible    ${PUBLICATION_NAME}
    user checks breadcrumb count should be    2
    user checks nth breadcrumb contains    1    Home
    user checks nth breadcrumb contains    2    Pre-release access

Go to prerelease table tool page
    user clicks link    Table tool
    user waits until h1 is visible    Create your own tables
    user waits until table tool wizard step is available    1    Choose a subject    %{WAIT_SMALL}

Validate featured tables
    user waits until page contains element    id:featuredTables
    user checks element count is x    css:#featuredTables li    1
    user checks element should contain    css:#featuredTables li:first-child a    ${DATABLOCK_FEATURED_NAME}
    user checks element should contain    css:#featuredTables li:first-child [id^="highlight-description"]
    ...    ${DATABLOCK_FEATURED_TABLE_DESCRIPTION}

Go to featured table and validate table
    user clicks link    ${DATABLOCK_FEATURED_NAME}
    user validates table rows

Create and validate custom table
    user clicks link    Table tool

    user waits until h1 is visible    Create your own tables

    user clicks link    Create your own table
    user waits until table tool wizard step is available    1    Choose a subject    %{WAIT_SMALL}

    user waits until page contains    UI test subject    %{WAIT_SMALL}
    user clicks radio    UI test subject
    user clicks element    id:publicationSubjectForm-submit

    user chooses location, time period and filters
    user validates table rows

Go to prerelease methodology page
    user clicks link    Methodologies
    user waits until h1 is visible    Methodologies

Validate no methodologies
    user waits until page contains    No methodologies available

Create and validate methodology
    user creates methodology for publication    ${PUBLICATION_NAME}
    approve methodology from methodology view
    user navigates to admin frontend    ${RELEASE_URL}/prerelease/methodologies
    user waits until h1 is visible    Methodologies
    user waits until page contains    ${PUBLICATION_NAME} (Owned)
    user clicks link    ${PUBLICATION_NAME} (Owned)
    user waits until h1 is visible    ${PUBLICATION_NAME}

Validate prerelease has started for Analyst user
    user changes to analyst1
    user navigates to admin frontend    ${RELEASE_URL}/prerelease/content

    user checks breadcrumb count should be    2
    user checks nth breadcrumb contains    1    Home
    user checks nth breadcrumb contains    2    Pre-release access

    user waits until page contains title caption    Calendar year 2000    %{WAIT_SMALL}
    user waits until h1 is visible    ${PUBLICATION_NAME}

    user waits until element contains    id:releaseSummary    Test summary text for ${PUBLICATION_NAME}
    user waits until element contains    id:releaseHeadlines    Test headlines summary text for ${PUBLICATION_NAME}

Validate public metdata guidance for Analyst user
    user clicks link    Data guidance

    user waits until page contains title caption    Calendar year 2000    %{WAIT_SMALL}
    user waits until h1 is visible    ${PUBLICATION_NAME}

    user waits until h2 is visible    Data guidance    %{WAIT_SMALL}
    user waits until page contains    Test metadata guidance content    %{WAIT_SMALL}

    user waits until page contains accordion section    UI test subject    %{WAIT_SMALL}
    user checks there are x accordion sections    1

    user opens accordion section    UI test subject
    user checks summary list contains    Filename    upload-file-test.csv
    user checks summary list contains    Geographic levels
    ...    Local authority; Local authority district; Local enterprise partnership; Opportunity area; Parliamentary constituency; RSC region; Regional; Ward
    user checks summary list contains    Time period    2005 to 2020
    user checks summary list contains    Content    Test file guidance content

    user opens details dropdown    Variable names and descriptions

    user checks table column heading contains    1    1    Variable name
    user checks table column heading contains    1    2    Variable description

    user checks table cell contains    1    1    admission_numbers
    user checks table cell contains    1    2    Admission Numbers

Go back to prerelease content page as Analyst user
    user clicks link    Back
    user checks breadcrumb count should be    2
    user checks nth breadcrumb contains    1    Home
    user checks nth breadcrumb contains    2    Pre-release access

    user waits until page contains title caption    Calendar year 2000    %{WAIT_SMALL}
    user waits until h1 is visible    ${PUBLICATION_NAME}

Validate public prerelease access list as Analyst user
    user clicks link    Pre-release access list

    user waits until page contains title caption    Calendar year 2000    %{WAIT_SMALL}
    user waits until h1 is visible    ${PUBLICATION_NAME}

    user waits until h2 is visible    Pre-release access list    %{WAIT_SMALL}
    user waits until page contains    Updated test public access list    %{WAIT_SMALL}

Go back to prerelease content page again as Analyst user
    user clicks link    Back

    user checks breadcrumb count should be    2
    user checks nth breadcrumb contains    1    Home
    user checks nth breadcrumb contains    2    Pre-release access

    user waits until page contains title caption    Calendar year 2000    %{WAIT_SMALL}
    user waits until h1 is visible    ${PUBLICATION_NAME}

Go to prerelease table tool page as Analyst user
    user clicks link    Table tool

    user waits until h1 is visible    Create your own tables
    user waits until table tool wizard step is available    1    Choose a subject    %{WAIT_SMALL}

Validate featured tables as Analyst user
    user waits until page contains element    id:featuredTables    %{WAIT_SMALL}
    user checks element count is x    css:#featuredTables li    1
    user checks element should contain    css:#featuredTables li:first-child a    ${DATABLOCK_FEATURED_NAME}
    user checks element should contain    css:#featuredTables li:first-child [id^="highlight-description"]
    ...    ${DATABLOCK_FEATURED_TABLE_DESCRIPTION}

Go to featured table and validate table as Analyst user
    user clicks link    ${DATABLOCK_FEATURED_NAME}
    user validates table rows

Create and validate custom table as Analyst user
    user clicks link    Table tool

    user waits until h1 is visible    Create your own tables

    user clicks link    Create your own table
    user waits until table tool wizard step is available    1    Choose a subject    %{WAIT_SMALL}

    user waits until page contains    UI test subject    %{WAIT_SMALL}
    user clicks radio    UI test subject
    user clicks element    id:publicationSubjectForm-submit

    user chooses location, time period and filters
    user validates table rows

Go to prerelease methodology page as Analyst user
    user clicks link    Methodologies
    user waits until h1 is visible    Methodologies

Validate methodology as Analyst user
    user waits until page contains    ${PUBLICATION_NAME} (Owned)
    user clicks link    ${PUBLICATION_NAME} (Owned)
    user waits until h1 is visible    ${PUBLICATION_NAME}

Unschedule release
    [Documentation]    EES-2826
    # EES-2826 Cancel scheduled publishing because ReleaseStatus row in table storage isn't removed
    # by test topic teardown. Unscheduling prevents an error when the scheduled publishing begins.
    user changes to bau1
    user navigates to admin frontend    ${RELEASE_URL}/status
    user puts release into draft    expected_next_release_date=January 2001


*** Keywords ***
user chooses location, time period and filters
    user waits until table tool wizard step is available    2    Choose locations    %{WAIT_MEDIUM}

    user opens details dropdown    Ward
    user clicks checkbox    Nailsea Youngwood
    user clicks checkbox    Syon
    user clicks element    id:locationFiltersForm-submit

    user waits until table tool wizard step is available    3    Choose time period    %{WAIT_MEDIUM}

    user waits until page contains element    id:timePeriodForm-start
    ${timePeriodStartList}=    get list items    id:timePeriodForm-start
    ${timePeriodEndList}=    get list items    id:timePeriodForm-end
    ${expectedList}=    create list    Please select    2005    2007    2008    2010    2011    2012    2016    2017
    lists should be equal    ${timePeriodStartList}    ${expectedList}
    lists should be equal    ${timePeriodEndList}    ${expectedList}

    user chooses select option    id:timePeriodForm-start    2005
    user chooses select option    id:timePeriodForm-end    2017
    user clicks element    id:timePeriodForm-submit

    user waits until table tool wizard step is available    4    Choose your filters
    user checks previous table tool step contains    3    Time period    2005 to 2017

    user checks indicator checkbox is checked    Admission Numbers

    user clicks element    id:filtersForm-submit

user validates table rows
    user waits until results table appears    %{WAIT_LONG}
    user waits until element contains    testid:dataTableCaption
    ...    Admission Numbers for 'UI test subject' in Nailsea Youngwood and Syon between 2005 and 2017

    user checks table column heading contains    1    1    Admission Numbers

    ${row}=    user gets row number with heading    Nailsea Youngwood
    user checks table heading in offset row contains    ${row}    0    2    2005
    user checks table heading in offset row contains    ${row}    1    1    2010
    user checks table heading in offset row contains    ${row}    2    1    2011
    user checks table heading in offset row contains    ${row}    3    1    2012
    user checks table heading in offset row contains    ${row}    4    1    2016

    user checks table cell in offset row contains    ${row}    0    1    3,612
    user checks table cell in offset row contains    ${row}    1    1    9,304
    user checks table cell in offset row contains    ${row}    2    1    9,603
    user checks table cell in offset row contains    ${row}    3    1    8,150
    user checks table cell in offset row contains    ${row}    4    1    4,198

    ${row}=    user gets row number with heading    Syon
    user checks table heading in offset row contains    ${row}    0    2    2007
    user checks table heading in offset row contains    ${row}    1    1    2008
    user checks table heading in offset row contains    ${row}    2    1    2010
    user checks table heading in offset row contains    ${row}    3    1    2012
    user checks table heading in offset row contains    ${row}    4    1    2017

    user checks table cell in offset row contains    ${row}    0    1    9,914
    user checks table cell in offset row contains    ${row}    1    1    5,505
    user checks table cell in offset row contains    ${row}    2    1    6,060
    user checks table cell in offset row contains    ${row}    3    1    1,109
    user checks table cell in offset row contains    ${row}    4    1    1,959
