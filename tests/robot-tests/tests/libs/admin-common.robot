*** Settings ***
Resource    ./common.robot
Library     admin-utilities.py
Library     String


*** Variables ***
${BAU1_BROWSER}=        bau1
${ANALYST1_BROWSER}=    analyst1


*** Keywords ***
user logs in via identity provider
    [Arguments]    ${email}    ${password}

    IF    "%{IDENTITY_PROVIDER}" == "KEYCLOAK"
        user waits until page contains element    xpath://h1[contains(text(), "Sign in to your account")]
        user enters text into element    id:username    ${email}
        user enters text into element    id:password    ${password}
        user clicks element    id:kc-login
    ELSE IF    "%{IDENTITY_PROVIDER}" == "AZURE"
        user waits until page contains element    xpath://*[.='Sign in']
        user enters text into element    xpath://*[@name='loginfmt']    ${email}
        user clicks element    //*[@type='submit']
        user waits until page contains element    xpath://*[@name='passwd']
        user enters text into element    xpath://*[@name='passwd']    ${password}
        user clicks element    xpath://*[@type='submit']
        user waits until page contains element    xpath://*[.='Stay signed in?']
        user clicks element    id:idBtn_Back
    END

user signs in as bau1
    [Arguments]    ${open_browser}=True    ${alias}=${BAU1_BROWSER}
    IF    ${open_browser}
        user opens the browser    ${alias}
    END
    user navigates to admin frontend
    user waits until h1 is visible    Sign in    %{WAIT_MEDIUM}
    user signs in as    ADMIN
    user navigates to admin dashboard    Bau1
    user checks breadcrumb count should be    2
    user checks nth breadcrumb contains    1    Home
    user checks nth breadcrumb contains    2    Administrator dashboard

user signs in as analyst1
    [Arguments]    ${open_browser}=True    ${alias}=${ANALYST1_BROWSER}
    IF    ${open_browser}
        user opens the browser    ${alias}
    END
    user navigates to admin frontend
    user waits until h1 is visible    Sign in
    user signs in as    ANALYST
    user navigates to admin dashboard    Analyst1
    user checks breadcrumb count should be    2
    user checks nth breadcrumb contains    1    Home
    user checks nth breadcrumb contains    2    Administrator dashboard

user switches to bau1 browser
    user switches browser    ${BAU1_BROWSER}

user switches to analyst1 browser
    user switches browser    ${ANALYST1_BROWSER}

user changes to bau1
    user clicks link    Sign out    css:#navigation
    user signs in as bau1    False

user changes to analyst1
    user clicks link    Sign out    css:#navigation
    user signs in as analyst1    False

user selects dashboard theme and topic if possible
    [Arguments]
    ...    ${theme_name}=%{TEST_THEME_NAME}
    ...    ${topic_name}=%{TEST_TOPIC_NAME}
    user waits until page does not contain loading spinner
    ${DROPDOWNS_EXIST}=    user checks dashboard theme topic dropdowns exist
    IF    ${DROPDOWNS_EXIST}
        user chooses select option    id:publicationsReleases-themeTopic-themeId    ${theme_name}
        user waits until page contains element    id:publicationsReleases-themeTopic-topicId
        user chooses select option    id:publicationsReleases-themeTopic-topicId    ${topic_name}
        user waits until page contains    ${theme_name} / ${topic_name}
    END

user navigates to release page from dashboard
    [Arguments]
    ...    ${RELEASE_TABLE_TESTID}
    ...    ${LINK_TEXT}
    ...    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}
    ...    ${THEME_NAME}=%{TEST_THEME_NAME}
    ...    ${TOPIC_NAME}=%{TEST_TOPIC_NAME}

    user navigates to publication page from dashboard
    ...    ${PUBLICATION_NAME}
    ...    ${THEME_NAME}
    ...    ${TOPIC_NAME}

    user waits for page to finish loading
    ${ROW}=    user gets table row    ${RELEASE_NAME}    testid:${RELEASE_TABLE_TESTID}
    user scrolls to element    ${ROW}

    user clicks element    xpath://a[text()="${LINK_TEXT}"]    ${ROW}    # "user clicks link" doesn't work
    user waits until h2 is visible    Release summary    %{WAIT_SMALL}
    user checks summary list contains    Publication title    ${PUBLICATION_NAME}

user navigates to draft release page from dashboard
    [Arguments]
    ...    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}
    ...    ${THEME_NAME}=%{TEST_THEME_NAME}
    ...    ${TOPIC_NAME}=%{TEST_TOPIC_NAME}
    ...    ${ACTION_LINK_TEXT}=Edit
    user navigates to release page from dashboard
    ...    publication-draft-releases
    ...    ${ACTION_LINK_TEXT}
    ...    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}
    ...    ${THEME_NAME}
    ...    ${TOPIC_NAME}

user navigates to scheduled release page from dashboard
    [Arguments]
    ...    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}
    ...    ${THEME_NAME}=%{TEST_THEME_NAME}
    ...    ${TOPIC_NAME}=%{TEST_TOPIC_NAME}
    user navigates to release page from dashboard
    ...    publication-scheduled-releases
    ...    Edit
    ...    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}
    ...    ${THEME_NAME}
    ...    ${TOPIC_NAME}

user navigates to published release page from dashboard
    [Arguments]
    ...    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}
    ...    ${THEME_NAME}=%{TEST_THEME_NAME}
    ...    ${TOPIC_NAME}=%{TEST_TOPIC_NAME}
    user navigates to release page from dashboard
    ...    publication-published-releases
    ...    View
    ...    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}
    ...    ${THEME_NAME}
    ...    ${TOPIC_NAME}

user creates publication
    [Arguments]    ${title}
    user waits until h1 is visible    Create new publication
    user waits until page contains element    id:publicationForm-title    %{WAIT_SMALL}
    user enters text into element    id:publicationForm-title    ${title}
    user enters text into element    id:publicationForm-summary    ${title} summary
    user enters text into element    id:publicationForm-teamName    Attainment statistics team
    user enters text into element    id:publicationForm-teamEmail    Attainment.STATISTICS@education.gov.uk
    user enters text into element    id:publicationForm-contactName    UI Tests Contact Name
    user enters text into element    id:publicationForm-contactTelNo    0123456789
    user clicks button    Save publication
    user waits until h1 is visible    Dashboard    %{WAIT_MEDIUM}

user creates release from publication page
    [Arguments]    ${publication}    ${time_period_coverage}    ${start_year}
    user waits until page contains title caption    Manage publication    %{WAIT_SMALL}
    user waits until h1 is visible    ${publication}

    user waits until page contains link    Create new release
    user clicks link    Create new release

    user waits until page contains element    id:releaseSummaryForm-timePeriodCoverage    %{WAIT_SMALL}
    user chooses select option    id:releaseSummaryForm-timePeriodCoverageCode    ${time_period_coverage}
    user enters text into element    id:releaseSummaryForm-timePeriodCoverageStartYear    ${start_year}
    user clicks radio    National statistics
    user clicks radio if exists    Create new template
    user waits until button is enabled    Create new release    %{WAIT_SMALL}
    user clicks button    Create new release

    user waits until page contains element    xpath://a[text()="Edit release summary"]    %{WAIT_SMALL}
    user waits until h2 is visible    Release summary    %{WAIT_SMALL}

user navigates to publication page from dashboard
    [Arguments]
    ...    ${publication}
    ...    ${theme}=%{TEST_THEME_NAME}
    ...    ${topic}=%{TEST_TOPIC_NAME}

    user navigates to admin dashboard if needed    %{ADMIN_URL}
    user waits until h1 is visible    Dashboard    %{WAIT_SMALL}
    user selects dashboard theme and topic if possible    ${theme}    ${topic}
    user scrolls to element    xpath://a[text()="${publication}"]
    user clicks link    ${publication}
    user waits until h1 is visible    ${publication}
    user waits until h2 is visible    Manage releases
    user waits until page does not contain loading spinner

user creates methodology for publication
    [Arguments]
    ...    ${publication}
    ...    ${theme}=%{TEST_THEME_NAME}
    ...    ${topic}=%{TEST_TOPIC_NAME}

    user navigates to methodologies on publication page    ${publication}    ${theme}    ${topic}

    user clicks button    Create new methodology
    user verifies methodology summary details    ${publication}

user navigates to details on publication page
    [Arguments]
    ...    ${publication}
    ...    ${theme}=%{TEST_THEME_NAME}
    ...    ${topic}=%{TEST_TOPIC_NAME}
    user navigates to publication page from dashboard    ${publication}    ${theme}    ${topic}

    user clicks link    Details
    user waits until h2 is visible    Publication details

user navigates to methodologies on publication page
    [Arguments]
    ...    ${publication}
    ...    ${theme}=%{TEST_THEME_NAME}
    ...    ${topic}=%{TEST_TOPIC_NAME}
    user navigates to publication page from dashboard    ${publication}    ${theme}    ${topic}

    user clicks link    Methodologies
    user waits until h2 is visible    Manage methodologies    %{WAIT_SMALL}

user navigates to methodology
    [Arguments]
    ...    ${publication}
    ...    ${methodology_title}
    ...    ${action_button_text}=Edit
    user navigates to methodologies on publication page    ${publication}
    ${ROW}=    user gets table row    ${methodology_title}    testid:methodologies
    user clicks element    xpath://*[text()="${action_button_text}"]    ${ROW}

    user waits until h2 is visible    Methodology summary

user edits methodology summary for publication
    [Arguments]
    ...    ${publication}
    ...    ${existing_methodology_title}
    ...    ${new_methodology_title}
    ...    ${action_button_text}=Edit
    user navigates to methodology    ${publication}
    ...    ${existing_methodology_title}
    ...    ${action_button_text}

    user clicks link    Edit summary
    user waits until h2 is visible    Edit methodology summary    %{WAIT_MEDIUM}

    IF    "${existing_methodology_title}" == "${publication}"
        user checks radio is checked    Use publication title
        user waits until element is not visible    label:Enter methodology title    10
    ELSE
        user checks radio is checked    Set an alternative title
        user waits until element is visible    label:Enter methodology title    %{WAIT_SMALL}
        user checks input field contains    label:Enter methodology title    ${existing_methodology_title}
    END

    IF    "${new_methodology_title}" == "${publication}"
        user clicks radio    Use publication title
        user waits until element is not visible    label:Enter methodology title    10
    ELSE
        user clicks radio    Set an alternative title
        user waits until element is visible    label:Enter methodology title    %{WAIT_SMALL}
        user enters text into element    label:Enter methodology title    ${new_methodology_title}
    END

    user clicks button    Update methodology
    user verifies methodology summary details    ${publication}    ${new_methodology_title}

user verifies methodology summary details
    [Arguments]
    ...    ${publication}
    ...    ${methodology_title}=${publication}
    ...    ${status}=Draft
    ...    ${published_on}=Not yet published
    user waits until h2 is visible    Methodology summary    %{WAIT_MEDIUM}
    user checks summary list contains    Title    ${methodology_title}
    user checks summary list contains    Published on    ${published_on}
    user checks summary list contains    Owning publication    ${publication}
    user checks page contains tag    ${status}

user approves methodology for publication
    [Arguments]
    ...    ${publication}
    ...    ${methodology_title}=${publication}
    ...    ${theme}=%{TEST_THEME_NAME}
    ...    ${topic}=%{TEST_TOPIC_NAME}
    ...    ${publishing_strategy}=Immediately
    ...    ${with_release}=

    approve methodology for publication
    ...    ${publication}
    ...    ${methodology_title}
    ...    ${theme}
    ...    ${topic}
    ...    ${publishing_strategy}
    ...    ${with_release}
    ...    Edit

user approves methodology amendment for publication
    [Arguments]
    ...    ${publication}
    ...    ${methodology_title}=${publication}
    ...    ${theme}=%{TEST_THEME_NAME}
    ...    ${topic}=%{TEST_TOPIC_NAME}
    ...    ${publishing_strategy}=Immediately
    ...    ${with_release}=

    approve methodology for publication
    ...    ${publication}
    ...    ${methodology_title}
    ...    ${theme}
    ...    ${topic}
    ...    ${publishing_strategy}
    ...    ${with_release}
    ...    Edit

approve methodology for publication
    [Arguments]
    ...    ${publication}
    ...    ${methodology_title}
    ...    ${theme}
    ...    ${topic}
    ...    ${publishing_strategy}
    ...    ${with_release}
    ...    ${action_button_text}

    user navigates to methodologies on publication page    ${publication}    ${theme}    ${topic}

    ${ROW}=    user gets table row    ${methodology_title}    testid:methodologies
    user clicks element    xpath://*[text()="${action_button_text}"]    ${ROW}
    user waits until h2 is visible    Methodology summary

    approve methodology from methodology view    ${publishing_strategy}    ${with_release}

approve methodology from methodology view
    [Arguments]
    ...    ${publishing_strategy}=Immediately
    ...    ${with_release}=
    user clicks link    Sign off
    user waits until h2 is visible    Sign off
    user changes methodology status to Approved    ${publishing_strategy}    ${with_release}

user creates methodology amendment for publication
    [Arguments]
    ...    ${publication}
    ...    ${methodology_title}=${publication}
    ...    ${theme}=%{TEST_THEME_NAME}
    ...    ${topic}=%{TEST_TOPIC_NAME}
    user navigates to methodologies on publication page    ${publication}    ${theme}    ${topic}

    ${ROW}=    user gets table row    ${methodology_title}    testid:methodologies
    user clicks element    xpath://*[text()="Amend"]    ${ROW}

    user waits until modal is visible    Confirm you want to amend this published methodology
    user clicks button    Confirm
    user waits until modal is not visible    Confirm you want to amend this published methodology
    user waits until h2 is visible    Methodology summary

user cancels methodology amendment for publication
    [Arguments]
    ...    ${publication}
    ...    ${methodology_title}=${publication}
    ...    ${theme}=%{TEST_THEME_NAME}
    ...    ${topic}=%{TEST_TOPIC_NAME}
    user navigates to methodologies on publication page    ${publication}    ${theme}    ${topic}

    ${ROW}=    user gets table row    ${methodology_title}    testid:methodologies
    user clicks element    xpath://*[text()="Cancel amendment"]    ${ROW}

    user waits until modal is visible    Confirm you want to cancel this amended methodology
    user clicks button    Confirm
    user waits until modal is not visible    Confirm you want to cancel this amended methodology

user adds note to methodology
    [Arguments]
    ...    ${note}
    user clicks button    Add note
    user enters text into element    label:New methodology note    ${note}
    user clicks button    Save note
    ${date}=    get current datetime    %-d %B %Y
    user waits until element contains    css:#methodologyNotes time    ${date}
    user waits until element contains    css:#methodologyNotes p    ${note}

user removes methodology note
    [Arguments]
    ...    ${note}
    ...    ${parent}
    user clicks button    Remove note    ${parent}
    user clicks button    Confirm
    user waits until page does not contain    ${note}

user edits methodology note
    [Arguments]
    ...    ${note}
    ...    ${day}
    ...    ${month}
    ...    ${year}
    user clicks button    Edit note    xpath://p[text()="${note}"]/ancestor::li
    user enters text into element    label:Day    ${day}
    user enters text into element    label:Month    ${month}
    user enters text into element    label:Year    ${year}
    user enters text into element    label:Edit methodology note    ${note} - edited
    user clicks button    Update note
    user waits until page contains button    Add note

user creates public prerelease access list
    [Arguments]    ${content}
    user clicks link    Public access list
    user waits until h2 is visible    Public pre-release access list
    user clicks button    Create public pre-release access list
    user presses keys    CTRL+a
    user presses keys    BACKSPACE
    user enters text into element    id:publicPreReleaseAccessForm-preReleaseAccessList    ${content}
    user clicks button    Save access list
    user waits until element contains    id:publicPreReleaseAccessForm-preReleaseAccessList    ${content}
    ...    %{WAIT_SMALL}

user updates public prerelease access list
    [Arguments]    ${content}
    user waits until h2 is visible    Public pre-release access list
    user clicks button    Edit public pre-release access list
    user presses keys    CTRL+a
    user presses keys    BACKSPACE
    user enters text into element    id:publicPreReleaseAccessForm-preReleaseAccessList    ${content}
    user clicks button    Save access list
    user waits until element contains    id:publicPreReleaseAccessForm-preReleaseAccessList    ${content}

user clicks footnote subject radio
    [Arguments]    ${subject_label}    ${radio_label}
    user clicks element
    ...    xpath://*[@data-testid="footnote-subject ${subject_label}"]//label[text()="${radio_label}"]/../input[@type="radio"]

user opens footnote subject dropdown
    [Arguments]    ${subject_label}    ${dropdown_label}
    user opens details dropdown    ${dropdown_label}    testid:footnote-subject ${subject_label}

user clicks footnote subject checkbox
    [Arguments]    ${subject_label}    ${dropdown_label}    ${label}
    user waits until page contains element    testid:footnote-subject ${subject_label}
    ${details}=    user gets details content element    ${dropdown_label}    testid:footnote-subject ${subject_label}
    user waits until page contains element    label:${label}
    user waits until parent contains element    ${details}    label:${label}
    ${checkbox}=    get child element    ${details}    label:${label}
    page should contain checkbox    ${checkbox}
    user scrolls to element    ${checkbox}
    wait until element is enabled    ${checkbox}
    user clicks element    ${checkbox}
    checkbox should be selected    ${checkbox}

user gets data guidance data file content editor
    [Arguments]    ${accordion_heading}
    user waits until page contains element    id:dataGuidance-dataFiles
    ${accordion}=    user gets accordion section content element    ${accordion_heading}    id:dataGuidance-dataFiles
    user waits until parent contains element    ${accordion}    xpath:.//*[@data-testid="Content"]//*[@role="textbox"]
    ${editor}=    get child element    ${accordion}    xpath:.//*[@data-testid="Content"]//*[@role="textbox"]
    [Return]    ${editor}

user enters text into data guidance data file content editor
    [Arguments]    ${accordion_heading}    ${text}
    ${accordion}=    user gets accordion section content element    ${accordion_heading}    id:dataGuidance-dataFiles
    user checks element does not contain child element    ${accordion}    testid:fileGuidanceContent-focused
    ${editor}=    user gets data guidance data file content editor    ${accordion_heading}
    user clicks element    ${editor}
    user checks element contains child element    ${accordion}    testid:fileGuidanceContent-focused
    user enters text into element    ${editor}    ${text}

user creates amendment for release
    [Arguments]    ${PUBLICATION_NAME}    ${RELEASE_NAME}
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}

    ${ROW}=    user gets table row    ${RELEASE_NAME}    testid:publication-published-releases
    user clicks element    xpath://*[text()="Amend"]    ${ROW}

    ${modal}=    user waits until modal is visible    Confirm you want to amend this published release
    user clicks button    Confirm    ${modal}
    user waits until page contains title    ${PUBLICATION_NAME}
    user waits until page contains title caption    Amend release for ${RELEASE_NAME}
    user checks page contains tag    Amendment

user deletes subject file
    [Arguments]    ${SUBJECT_NAME}
    user waits until page contains accordion section    ${SUBJECT_NAME}
    user opens accordion section    ${SUBJECT_NAME}
    user scrolls to accordion section content    ${SUBJECT_NAME}
    ${accordion}=    user gets accordion section content element    ${SUBJECT_NAME}
    ${button}=    user gets button element    Delete files    ${accordion}
    user clicks element    ${button}
    user clicks button    Confirm

user approves original release for immediate publication
    user approves release for immediate publication    original

user approves amended release for immediate publication
    user approves release for immediate publication    amendment

user approves release for immediate publication
    [Arguments]    ${release_type}=original    ${NEXT_RELEASE_MONTH}=01    ${NEXT_RELEASE_YEAR}=2200
    user clicks link    Sign off
    user waits until page does not contain loading spinner
    user waits until h2 is visible    Sign off
    user waits until page contains button    Edit release status
    user clicks button    Edit release status
    user waits until h2 is visible    Edit release status
    user checks page does not contain    Notify subscribers by email
    user clicks radio    Approved for publication
    IF    '${release_type}' == 'amendment'
        user waits until page contains    Notify subscribers by email
    END
    user enters text into element    id:releaseStatusForm-internalReleaseNote    Approved by UI tests
    user clicks radio    Immediately
    user enters text into element    id:releaseStatusForm-nextReleaseDate-month    ${NEXT_RELEASE_MONTH}
    user enters text into element    id:releaseStatusForm-nextReleaseDate-year    ${NEXT_RELEASE_YEAR}
    user clicks button    Update status
    user waits until h2 is visible    Sign off    %{RELEASE_COMPLETE_WAIT}
    user checks summary list contains    Current status    Approved
    user waits for release process status to be    Complete    %{RELEASE_COMPLETE_WAIT}
    user reloads page    # EES-1448
    user checks page does not contain button    Edit release status

user navigates to admin dashboard
    [Arguments]    ${USER}=
    user navigates to admin dashboard if needed    %{ADMIN_URL}
    user waits until h1 is visible    Dashboard
    IF    "${USER}" != ""
        user waits until page contains title caption    Welcome ${USER}
    END
    user waits until page contains element
    ...    css:[data-testid='topic-publications'],[data-testid='no-permission-to-access-releases']
    ...    %{WAIT_SMALL}

user uploads subject
    [Arguments]    ${SUBJECT_NAME}    ${SUBJECT_FILE}    ${META_FILE}
    user clicks link    Data and files
    user waits until page contains element    id:dataFileUploadForm-subjectTitle    %{WAIT_SMALL}
    user enters text into element    id:dataFileUploadForm-subjectTitle    ${SUBJECT_NAME}
    user chooses file    id:dataFileUploadForm-dataFile    ${FILES_DIR}${SUBJECT_FILE}
    user chooses file    id:dataFileUploadForm-metadataFile    ${FILES_DIR}${META_FILE}
    user clicks button    Upload data files
    user waits until h2 is visible    Uploaded data files    %{WAIT_SMALL}
    user waits until page contains accordion section    ${SUBJECT_NAME}    %{WAIT_SMALL}
    user opens accordion section    ${SUBJECT_NAME}
    ${section}=    user gets accordion section content element    ${SUBJECT_NAME}
    user checks headed table body row contains    Status    Complete    ${section}    %{WAIT_LONG}

user puts release into draft
    [Arguments]
    ...    ${release_note}=Moving back to draft
    ...    ${next_release_date_month}=
    ...    ${next_release_date_year}=
    ...    ${expected_scheduled_release_date}=Not scheduled
    ...    ${expected_next_release_date}=Not set

    user clicks button    Edit release status
    user waits until h2 is visible    Edit release status    %{WAIT_SMALL}
    user clicks radio    In draft
    user enters text into element    id:releaseStatusForm-internalReleaseNote    ${release_note}
    IF    "${next_release_date_month}" != "${EMPTY}"
        user enters text into element    id:releaseStatusForm-nextReleaseDate-month    ${next_release_date_month}
    END
    IF    "${next_release_date_year}" != "${EMPTY}"
        user enters text into element    id:releaseStatusForm-nextReleaseDate-year    ${next_release_date_year}
    END
    user clicks button    Update status
    user checks summary list contains    Current status    In Draft
    user checks summary list contains    Scheduled release    ${expected_scheduled_release_date}
    user checks summary list contains    Next release expected    ${expected_next_release_date}

user puts release into higher level review
    user clicks link    Sign off
    user waits until page does not contain loading spinner
    user waits until h2 is visible    Sign off    %{WAIT_SMALL}
    user clicks button    Edit release status
    user waits until h2 is visible    Edit release status    %{WAIT_SMALL}
    user clicks radio    Ready for higher review (this will notify approvers)
    user enters text into element    id:releaseStatusForm-internalReleaseNote    Ready for higher review
    user clicks button    Update status
    user waits until element is visible    id:CurrentReleaseStatus-Awaiting higher review    %{WAIT_SMALL}

user approves release for scheduled publication
    [Arguments]
    ...    ${publish_date_day}
    ...    ${publish_date_month}
    ...    ${publish_date_year}
    ...    ${next_release_month}=01
    ...    ${next_release_year}=2200
    ...    ${update_amendment_published_date}=${False}
    user clicks link    Sign off
    user waits until page does not contain loading spinner
    user waits until h2 is visible    Sign off    %{WAIT_SMALL}
    user waits until page contains button    Edit release status    %{WAIT_SMALL}

    user clicks button    Edit release status
    user waits until h2 is visible    Edit release status    %{WAIT_SMALL}

    user clicks radio    Approved for publication
    user enters text into element    id:releaseStatusForm-internalReleaseNote    Approved by UI tests
    IF    ${update_amendment_published_date}
        user waits until page contains    Update published date
        user clicks checkbox    Update published date
    END
    user clicks radio    On a specific date
    user waits until page contains    Publish date
    user enters text into element    id:releaseStatusForm-publishScheduled-day    ${publish_date_day}
    user enters text into element    id:releaseStatusForm-publishScheduled-month    ${publish_date_month}
    user enters text into element    id:releaseStatusForm-publishScheduled-year    ${publish_date_year}
    user enters text into element    id:releaseStatusForm-nextReleaseDate-month    ${next_release_month}
    user enters text into element    id:releaseStatusForm-nextReleaseDate-year    ${next_release_year}

    user clicks button    Update status
    user waits until h2 is visible    Confirm publish date    %{WAIT_SMALL}
    user clicks button    Confirm

user waits for scheduled release to be published immediately
    # It's possible that the actual scheduled "stage scheduled releases" function might pick up the staging of this
    # scheduled Release before we get a chance to manually trigger the "stage scheduled releases immediately" function
    # ourselves - hence we need to account for it going into "Started" state while it stages before we've manually
    # triggered the function, as well as the standard "Scheduled" state that we would normally expect a scheduled
    # Release to fall into.
    ${release_id}=    get release id from url
    user waits until page contains element
    ...    xpath://*[@id='release-process-status-Scheduled' or @id='release-process-status-Started']    %{WAIT_SMALL}
    trigger immediate staging of scheduled release    ${release_id}
    user reloads page
    user waits until page contains details dropdown    View stages    %{WAIT_SMALL}
    user opens details dropdown    View stages
    user waits until page contains    content - Scheduled    %{WAIT_MEDIUM}
    user waits until page contains    files - Complete    %{WAIT_MEDIUM}
    trigger immediate publishing of scheduled release    ${release_id}
    user waits until page contains element    id:release-process-status-Complete    %{WAIT_MEDIUM}

user verifies release summary
    [Arguments]    ${PUBLICATION_NAME}    ${PUBLICATION_SUMMARY}    ${TIME_PERIOD}    ${RELEASE_PERIOD}    ${LEAD_STATISTICIAN}    ${RELEASE_TYPE}
    user waits until h2 is visible    Release summary
    user checks summary list contains    Publication title    ${PUBLICATION_NAME}
    user checks summary list contains    Publication summary    ${PUBLICATION_SUMMARY}
    user checks summary list contains    Lead statistician    ${LEAD_STATISTICIAN}

    user checks summary list contains    Time period    ${TIME_PERIOD}
    user checks summary list contains    Release period    ${RELEASE_PERIOD}
    user checks summary list contains    Release type    ${RELEASE_TYPE}

user changes methodology status to Approved
    [Arguments]
    ...    ${publishing_strategy}=Immediately
    ...    ${with_release}=
    ${is_publishing_strategy_with_release}=    Evaluate    '${publishing_strategy}' == 'WithRelease'
    user clicks button    Edit status
    user clicks element    id:methodologyStatusForm-status-Approved
    user enters text into element    id:methodologyStatusForm-latestInternalReleaseNote    Approved by UI tests
    user clicks element    id:methodologyStatusForm-publishingStrategy-${publishing_strategy}
    IF    ${is_publishing_strategy_with_release} is ${TRUE}
        user waits until element is enabled    css:[name="withReleaseId"]
        user chooses select option    css:[name="withReleaseId"]    ${with_release}
    END
    user clicks button    Update status
    user waits until h2 is visible    Sign off
    user checks summary list contains    Status    Approved
    user checks summary list contains    Internal note    Approved by UI tests
    IF    ${is_publishing_strategy_with_release} is ${TRUE}
        user checks summary list contains    When to publish    With a specific release
        user checks summary list contains    Publish with release    ${with_release}
    ELSE
        user checks summary list contains    When to publish    ${publishing_strategy}
    END

user changes methodology status to Draft
    user clicks link    Sign off
    user waits until h2 is visible    Sign off

    user clicks button    Edit status
    user clicks element    id:methodologyStatusForm-status-Draft
    user clicks button    Update status
    user waits until h2 is visible    Sign off
    user checks page contains tag    In Draft

user gives analyst publication owner access
    [Arguments]    ${PUBLICATION_NAME}    ${ANALYST_EMAIL}=EES-test.ANALYST1@education.gov.uk
    user gives publication access to analyst    ${PUBLICATION_NAME}    Owner    ${ANALYST_EMAIL}

user gives analyst publication approver access
    [Arguments]    ${PUBLICATION_NAME}    ${ANALYST_EMAIL}=EES-test.ANALYST1@education.gov.uk
    user gives publication access to analyst    ${PUBLICATION_NAME}    Approver    ${ANALYST_EMAIL}

user removes publication owner access from analyst
    [Arguments]    ${PUBLICATION_NAME}    ${ANALYST_EMAIL}=EES-test.ANALYST1@education.gov.uk
    user removes publication access from analyst    ${PUBLICATION_NAME}    Owner    ${ANALYST_EMAIL}

user removes publication release approver access from analyst
    [Arguments]    ${PUBLICATION_NAME}    ${ANALYST_EMAIL}=EES-test.ANALYST1@education.gov.uk
    user removes publication access from analyst    ${PUBLICATION_NAME}    Approver    ${ANALYST_EMAIL}

user gives publication access to analyst
    [Arguments]
    ...    ${PUBLICATION_NAME}
    ...    ${ROLE}
    ...    ${ANALYST_EMAIL}=EES-test.ANALYST1@education.gov.uk
    user goes to manage user    ${ANALYST_EMAIL}
    user chooses select option    css:[name="selectedPublicationId"]    ${PUBLICATION_NAME}
    user waits until element is enabled    css:[name="selectedPublicationRole"]
    user chooses select option    css:[name="selectedPublicationRole"]    ${ROLE}
    user clicks button    Add publication access
    user waits until parent contains element    testid:publicationAccessTable
    ...    xpath://tbody/tr[td[//th[text()="Publication"] and text()="${PUBLICATION_NAME}"] and td[//th[text()="Role"] and text()="${ROLE}"]]

user removes publication access from analyst
    [Arguments]
    ...    ${PUBLICATION_NAME}
    ...    ${ROLE}
    ...    ${ANALYST_EMAIL}=EES-test.ANALYST1@education.gov.uk
    user goes to manage user    ${ANALYST_EMAIL}
    ${table}=    user gets testid element    publicationAccessTable
    ${row}=    get child element    ${table}
    ...    xpath://tbody/tr[td[//th[text()="Publication"] and text()="${PUBLICATION_NAME}"] and td[//th[text()="Role"] and text()="${ROLE}"]]
    user clicks button    Remove    ${row}
    user waits until page does not contain loading spinner

user gives release access to analyst
    [Arguments]
    ...    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}
    ...    ${ROLE}
    ...    ${ANALYST_EMAIL}=EES-test.ANALYST1@education.gov.uk
    user goes to manage user    ${ANALYST_EMAIL}
    user scrolls to element    css:[name="selectedReleaseId"]
    user chooses select option    css:[name="selectedReleaseId"]    ${PUBLICATION_NAME} - ${RELEASE_NAME}
    user waits until element is enabled    css:[name="selectedReleaseRole"]
    user chooses select option    css:[name="selectedReleaseRole"]    ${ROLE}
    user clicks button    Add release access
    user waits until parent contains element    testid:releaseAccessTable
    ...    xpath://tbody/tr[td[//th[text()="Publication"] and text()="${PUBLICATION_NAME}"] and td[//th[text()="Release"] and text()="${RELEASE_NAME}"] and td[//th[text()="Role"] and text()="${ROLE}"]]

user removes release access from analyst
    [Arguments]    ${PUBLICATION_NAME}    ${RELEASE_NAME}    ${ROLE}    ${ANALYST_EMAIL}=EES-test.ANALYST1@education.gov.uk
    user goes to manage user    ${ANALYST_EMAIL}
    ${table}=    user gets testid element    releaseAccessTable
    ${row}=    get child element    ${table}
    ...    xpath://tbody/tr[td[//th[text()="Publication"] and text()="${PUBLICATION_NAME}"] and td[//th[text()="Release"] and text()="${RELEASE_NAME}"] and td[//th[text()="Role"] and text()="${ROLE}"]]
    user clicks button    Remove    ${row}
    user waits until page does not contain loading spinner

user goes to manage user
    [Arguments]    ${EMAIL_ADDRESS}
    user navigates to admin frontend    %{ADMIN_URL}/administration/users
    user waits until h1 is visible    Users
    user waits until table is visible
    user clicks link    Manage    xpath://td[text()="${EMAIL_ADDRESS}"]/..
    # stale element exception if you don't wait until it's enabled
    user waits until element is enabled    css:[name="selectedPublicationId"]
    user waits until element is enabled    css:[name="selectedReleaseId"]

user waits until modal is visible
    [Arguments]
    ...    ${modal_title}
    ...    ${modal_text}=${EMPTY}

    user waits until parent contains element    css:[role="dialog"]    xpath://h2[.="${modal_title}"]
    IF    "${modal_text}" != "${EMPTY}"
        user waits until parent contains element    css:[role="dialog"]    xpath://*[.="${modal_text}"]
    END
    ${modal_element}=    get webelement    css:[role="dialog"]
    [Return]    ${modal_element}

user waits until modal is not visible
    [Arguments]    ${modal_title}    ${wait}=${timeout}
    user waits until page does not contain element    css:[role="dialog"]    ${wait}
    user waits until h2 is not visible    ${modal_title}

user gets resolved comments
    [Arguments]    ${parent}=css:body
    user waits until parent contains element    ${parent}    testid:resolvedComments
    ${comments}=    get child element    ${parent}    testid:resolvedComments
    [Return]    ${comments}

user gets unresolved comments
    [Arguments]    ${parent}=css:body
    user waits until parent contains element    ${parent}    testid:unresolvedComments
    ${comments}=    get child element    ${parent}    testid:unresolvedComments
    [Return]    ${comments}

user gets unresolved comment
    [Arguments]    ${comment_text}    ${parent}=css:body
    ${comments}=    user gets unresolved comments    ${parent}
    user waits until parent contains element    ${comments}
    ...    xpath:./li[.//*[@data-testid="comment-content" and text()="${comment_text}"]]
    ${result}=    get child element    ${comments}
    ...    xpath:./li[.//*[@data-testid="comment-content" and text()="${comment_text}"]]
    [Return]    ${result}

user gets resolved comment
    [Arguments]    ${comment_text}    ${parent}=css:body
    ${comments}=    user gets resolved comments    ${parent}
    user waits until parent contains element    ${comments}
    ...    xpath:./li[.//*[@data-testid="comment-content" and text()="${comment_text}"]]
    ${result}=    get child element    ${comments}
    ...    xpath:./li[.//*[@data-testid="comment-content" and text()="${comment_text}"]]
    [Return]    ${result}

user closes Set Page View box
    user clicks element    id:pageViewToggleButton
    user waits until element is not visible    id:editingMode    %{WAIT_SMALL}

# This keyword will work for any URL containing the pattern release/<guid>, and will return the guid segment of the URL.
# For example, in the URL https://localhost/publication/<publication id>/release/<release id>/status, this
# keyword would return the <release id> segment of the URL.

get release id from url
    ${current_url}=    Get Location
    @{release_id_match}=    Get Regexp Matches    ${current_url}
    ...    release\/([0-9A-Fa-f]{8}[-]?[0-9A-Fa-f]{4}[-]?[0-9A-Fa-f]{4}[-]?[0-9A-Fa-f]{4}[-]?[0-9A-Fa-f]{12})    1
    ${release_id}=    Get From List    ${release_id_match}    0
    [Return]    ${release_id}

user adds free text key stat
    [Arguments]    ${title}    ${statistic}    ${trend}    ${guidance_title}    ${guidance_text}
    user clicks button    Add free text key statistic
    user waits until page contains element    id:editableKeyStatTextForm-create-title

    user enters text into element    id:editableKeyStatTextForm-create-title    ${title}
    user enters text into element    id:editableKeyStatTextForm-create-statistic    ${statistic}
    user enters text into element    id:editableKeyStatTextForm-create-trend    ${trend}
    user enters text into element    id:editableKeyStatTextForm-create-guidanceTitle    ${guidance_title}

    user clicks element    id:editableKeyStatTextForm-create-guidanceText
    user presses keys    ${guidance_text}

    user clicks button    Save
    user waits until page does not contain button    Save

user updates free text key stat
    [Arguments]    ${tile_num}    ${title}    ${statistic}    ${trend}    ${guidance_title}    ${guidance_text}
    user waits until page contains element    xpath://*[@data-testid="keyStat"][${tile_num}]

    user clicks element    xpath://*[@data-testid="keyStat"][${tile_num}]//button[contains(text(), "Edit")]

    user waits until page contains button    Save

    user enters text into element    xpath://*[@data-testid="keyStat"][${tile_num}]//input[@name="title"]    ${title}
    user enters text into element    xpath://*[@data-testid="keyStat"][${tile_num}]//input[@name="statistic"]
    ...    ${statistic}
    user enters text into element    xpath://*[@data-testid="keyStat"][${tile_num}]//input[@name="trend"]    ${trend}
    user enters text into element    xpath://*[@data-testid="keyStat"][${tile_num}]//input[@name="guidanceTitle"]
    ...    ${guidance_title}

    user clears element text    xpath://*[@data-testid="keyStat"][${tile_num}]//*[contains(@class, "ck-content")]
    user presses keys    ${guidance_text}

    user clicks button    Save
    user waits until page does not contain button    Save

user removes key stat
    [Arguments]    ${tile_num}
    user waits until page contains element    xpath://*[@data-testid="keyStat"][${tile_num}]
    user clicks element    xpath://*[@data-testid="keyStat"][${tile_num}]//button[contains(text(), "Remove")]
