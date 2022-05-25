*** Settings ***
Resource    ./common.robot
Library     admin-utilities.py


*** Variables ***
${BAU1_BROWSER}         bau1
${ANALYST1_BROWSER}     analyst1


*** Keywords ***
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
    user signs out
    user signs in as bau1    False

user changes to analyst1
    user signs out
    user signs in as analyst1    False

user signs out
    user clicks link    Sign out    css:#navigation

user selects theme and topic from admin dashboard
    [Arguments]    ${theme}    ${topic}
    user navigates to admin dashboard
    ${correct_theme_and_topic_selected}=    user is on admin dashboard with theme and topic selected    %{ADMIN_URL}
    ...    ${theme}    ${topic}
    IF    ${correct_theme_and_topic_selected} is ${FALSE}
        user chooses select option    id:publicationsReleases-themeTopic-themeId    ${theme}
        user waits until page contains element    id:publicationsReleases-themeTopic-topicId    %{WAIT_MEDIUM}
        user chooses select option    id:publicationsReleases-themeTopic-topicId    ${topic}
    END
    user waits until h2 is visible    ${theme}    %{WAIT_MEDIUM}
    user waits until h3 is visible    ${topic}
    user waits until page does not contain loading spinner
    user waits until page contains element
    ...    xpath://*[@data-testid="accordion"]|//*[text()="No publications available"]
    ...    %{WAIT_MEDIUM}

user navigates to editable release summary from admin dashboard
    [Arguments]
    ...    ${PUBLICATION_NAME}
    ...    ${DETAILS_HEADING}
    ...    ${THEME_NAME}=%{TEST_THEME_NAME}
    ...    ${TOPIC_NAME}=%{TEST_TOPIC_NAME}
    user navigates to release summary from admin dashboard
    ...    ${PUBLICATION_NAME}
    ...    ${DETAILS_HEADING}
    ...    ${THEME_NAME}
    ...    ${TOPIC_NAME}
    ...    Edit release

user navigates to editable release amendment summary from admin dashboard
    [Arguments]
    ...    ${PUBLICATION_NAME}
    ...    ${DETAILS_HEADING}
    ...    ${THEME_NAME}=%{TEST_THEME_NAME}
    ...    ${TOPIC_NAME}=%{TEST_TOPIC_NAME}
    user navigates to release summary from admin dashboard
    ...    ${PUBLICATION_NAME}
    ...    ${DETAILS_HEADING}
    ...    ${THEME_NAME}
    ...    ${TOPIC_NAME}
    ...    Edit release amendment

user navigates to readonly release summary from admin dashboard
    [Arguments]
    ...    ${PUBLICATION_NAME}
    ...    ${DETAILS_HEADING}
    ...    ${THEME_NAME}=%{TEST_THEME_NAME}
    ...    ${TOPIC_NAME}=%{TEST_TOPIC_NAME}
    user navigates to release summary from admin dashboard
    ...    ${PUBLICATION_NAME}
    ...    ${DETAILS_HEADING}
    ...    ${THEME_NAME}
    ...    ${TOPIC_NAME}
    ...    View release

user navigates to release summary from admin dashboard
    [Arguments]
    ...    ${PUBLICATION_NAME}
    ...    ${DETAILS_HEADING}
    ...    ${THEME_NAME}=%{TEST_THEME_NAME}
    ...    ${TOPIC_NAME}=%{TEST_TOPIC_NAME}
    ...    ${RELEASE_SUMMARY_LINK_TEXT}=Edit release
    ${details}=    user opens release summary on the admin dashboard
    ...    ${PUBLICATION_NAME}
    ...    ${DETAILS_HEADING}
    ...    ${THEME_NAME}
    ...    ${TOPIC_NAME}

    ${summary_button}=    user waits until element contains link    ${details}    ${RELEASE_SUMMARY_LINK_TEXT}
    ...    %{WAIT_SMALL}
    user clicks element    ${summary_button}
    user waits until h2 is visible    Release summary    %{WAIT_SMALL}
    user checks summary list contains    Publication title    ${PUBLICATION_NAME}

user opens release summary on the admin dashboard
    [Arguments]
    ...    ${PUBLICATION_NAME}
    ...    ${DETAILS_HEADING}
    ...    ${THEME_NAME}=%{TEST_THEME_NAME}
    ...    ${TOPIC_NAME}=%{TEST_TOPIC_NAME}
    user opens publication on the admin dashboard    ${PUBLICATION_NAME}    ${THEME_NAME}    ${TOPIC_NAME}

    ${accordion}=    user gets accordion section content element    ${PUBLICATION_NAME}
    user opens details dropdown    ${DETAILS_HEADING}    ${accordion}
    ${details}=    user gets details content element    ${DETAILS_HEADING}    ${accordion}
    [Return]    ${details}

user creates publication
    [Arguments]    ${title}
    user waits until h1 is visible    Create new publication    %{WAIT_SMALL}
    user waits until page contains element    id:publicationForm-title    %{WAIT_SMALL}
    user enters text into element    id:publicationForm-title    ${title}
    user enters text into element    id:publicationForm-teamName    Attainment statistics team
    user enters text into element    id:publicationForm-teamEmail    Attainment.STATISTICS@education.gov.uk
    user enters text into element    id:publicationForm-contactName    UI Tests Contact Name
    user enters text into element    id:publicationForm-contactTelNo    0123456789
    user clicks button    Save publication
    user waits until h1 is visible    Dashboard    %{WAIT_MEDIUM}

user creates release for publication
    [Arguments]    ${publication}    ${time_period_coverage}    ${start_year}
    user waits until page contains title caption    ${publication}
    user waits until h1 is visible    Create new release    %{WAIT_SMALL}
    user waits until page contains element    id:releaseSummaryForm-timePeriodCoverage    %{WAIT_SMALL}
    user chooses select option    id:releaseSummaryForm-timePeriodCoverageCode    ${time_period_coverage}
    user enters text into element    id:releaseSummaryForm-timePeriodCoverageStartYear    ${start_year}
    user clicks radio    National statistics
    user clicks radio if exists    Create new template
    user clicks button    Create new release

    user waits until page contains element    xpath://a[text()="Edit release summary"]    %{WAIT_SMALL}
    user waits until h2 is visible    Release summary    %{WAIT_SMALL}

user opens publication on the admin dashboard
    [Arguments]
    ...    ${publication}
    ...    ${theme}=%{TEST_THEME_NAME}
    ...    ${topic}=%{TEST_TOPIC_NAME}
    user selects theme and topic from admin dashboard    ${theme}    ${topic}
    user waits until page contains accordion section    ${publication}    %{WAIT_MEDIUM}
    ${accordion}=    user opens accordion section    ${publication}
    [Return]    ${accordion}

user creates methodology for publication
    [Arguments]
    ...    ${publication}
    ...    ${theme}=%{TEST_THEME_NAME}
    ...    ${topic}=%{TEST_TOPIC_NAME}

    ${accordion}=    user opens publication on the admin dashboard    ${publication}    ${theme}    ${topic}
    user clicks button    Create methodology    ${accordion}
    user verifies methodology summary details    ${publication}

user views methodology for publication
    [Arguments]
    ...    ${publication}
    ...    ${methodology_title}=${publication}
    ...    ${view_button_text}=Edit methodology
    ${accordion}=    user opens publication on the admin dashboard    ${publication}
    user views methodology for open publication accordion
    ...    ${accordion}
    ...    ${methodology_title}
    ...    ${view_button_text}

user views methodology amendment for publication
    [Arguments]    ${publication}    ${methodology_title}=${publication}
    ${accordion}=    user opens publication on the admin dashboard    ${publication}
    user views methodology for open publication accordion    ${accordion}    ${methodology_title}
    ...    Edit amendment

user views methodology for open publication accordion
    [Arguments]
    ...    ${accordion}
    ...    ${methodology_title}
    ...    ${edit_button_text}=Edit methodology
    user opens details dropdown    ${methodology_title}    ${accordion}
    user clicks link    ${edit_button_text}    ${accordion}
    user waits until h2 is visible    Methodology summary

user edits methodology summary for publication
    [Arguments]
    ...    ${publication}
    ...    ${existing_methodology_title}
    ...    ${new_methodology_title}
    ...    ${edit_button_text}=Edit methodology
    user views methodology for publication    ${publication}    ${existing_methodology_title}    ${edit_button_text}
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
    ...    Edit methodology

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
    ...    Edit amendment

approve methodology for publication
    [Arguments]
    ...    ${publication}
    ...    ${methodology_title}
    ...    ${theme}
    ...    ${topic}
    ...    ${publishing_strategy}
    ...    ${with_release}
    ...    ${edit_button_text}

    ${accordion}=    user opens publication on the admin dashboard    ${publication}    ${theme}    ${topic}
    user opens details dropdown    ${methodology_title}    ${accordion}
    user clicks link    ${edit_button_text}    ${accordion}
    approve methodology from methodology view    ${publishing_strategy}    ${with_release}

approve methodology from methodology view
    [Arguments]
    ...    ${publishing_strategy}=Immediately
    ...    ${with_release}=
    user clicks link    Sign off
    user changes methodology status to Approved    ${publishing_strategy}    ${with_release}

user creates approved methodology for publication
    [Arguments]
    ...    ${publication}
    ...    ${theme}=%{TEST_THEME_NAME}
    ...    ${topic}=%{TEST_TOPIC_NAME}

    user creates methodology for publication    ${publication}    ${theme}    ${topic}
    user approves methodology for publication    ${publication}    ${publication}    ${theme}    ${topic}

user creates methodology amendment for publication
    [Arguments]
    ...    ${publication}
    ...    ${methodology_title}=${publication}
    ...    ${theme}=%{TEST_THEME_NAME}
    ...    ${topic}=%{TEST_TOPIC_NAME}
    ${accordion}=    user opens publication on the admin dashboard    ${publication}
    user opens details dropdown    ${methodology_title}    ${accordion}
    user clicks button    Amend methodology    ${accordion}
    user waits until modal is visible    Confirm you want to amend this live methodology
    user clicks button    Confirm
    user waits until modal is not visible    Confirm you want to amend this live methodology
    user waits until h2 is visible    Methodology summary

user cancels methodology amendment for publication
    [Arguments]
    ...    ${publication}
    ...    ${methodology_title}=${publication}
    ...    ${theme}=%{TEST_THEME_NAME}
    ...    ${topic}=%{TEST_TOPIC_NAME}
    ${accordion}=    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    user opens details dropdown    ${methodology_title}    ${accordion}
    user clicks button    Cancel amendment    ${accordion}
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
    user waits until page contains    ${note} - edited

user links publication to external methodology
    [Arguments]
    ...    ${publication}
    ...    ${title}=External methodology
    ...    ${link}=https://example.com
    ${accordion}=    user opens publication on the admin dashboard    ${publication}
    user clicks link    Use an external methodology    ${accordion}
    user waits until page contains title    Link to an externally hosted methodology
    user enters text into element    label:Link title    ${title}
    user enters text into element    label:URL    ${link}
    user clicks button    Save
    user waits until page does not contain button    Save
    ${accordion}=    user opens publication on the admin dashboard    ${publication}
    user waits until parent contains element    ${accordion}    //*[text()="External methodology (External)"]

user edits an external methodology
    [Arguments]
    ...    ${publication}
    ...    ${new_title}=External methodology updated
    ...    ${new_link}=https://example.com/updated
    ...    ${original_title}=External methodology
    ...    ${original_link}=https://example.com

    ${accordion}=    user opens publication on the admin dashboard    ${publication}
    user clicks link    Edit external methodology    ${accordion}
    user waits until page contains title    Edit external methodology link
    user checks input field contains    label:Link title    ${original_title}
    user checks input field contains    label:URL    ${original_link}
    user enters text into element    label:Link title    ${new_title}
    user enters text into element    label:URL    ${new_link}
    user clicks button    Save

user removes an external methodology from publication
    [Arguments]    ${publication}
    ${accordion}=    user opens publication on the admin dashboard    ${publication}
    user clicks button    Remove external methodology    ${accordion}
    user waits until modal is visible    Remove external methodology
    user clicks button    Confirm
    user waits until modal is not visible    Remove external methodology

user creates public prerelease access list
    [Arguments]    ${content}
    user clicks link    Public access list
    user waits until h2 is visible    Public pre-release access list
    user clicks button    Create public pre-release access list
    user presses keys    CTRL+a+BACKSPACE
    user presses keys    ${content}
    user clicks button    Save access list
    user waits until element contains    css:[data-testid="publicPreReleaseAccessListPreview"]    ${content}

user updates public prerelease access list
    [Arguments]    ${content}
    user clicks link    Public access list
    user waits until h2 is visible    Public pre-release access list
    user clicks button    Edit public pre-release access list
    user presses keys    CTRL+a
    user presses keys    BACKSPACE
    user presses keys    ${content}
    user clicks button    Save access list
    user waits until element contains    css:[data-testid="publicPreReleaseAccessListPreview"]    ${content}

user checks draft releases tab contains publication
    [Arguments]    ${publication_name}
    user checks page contains element    xpath://*[@id="draft-releases"]//h3[text()="${publication_name}"]

user waits until draft releases tab contains publication
    [Arguments]    ${publication_name}
    user waits until page contains element    xpath://*[@id="draft-releases"]//h3[text()="${publication_name}"]

user checks draft releases tab publication has release
    [Arguments]    ${publication_name}    ${release_text}
    user checks page contains element
    ...    xpath://*[@id="draft-releases"]//*[@data-testid="releaseByStatusTab ${publication_name}"]//*[contains(@data-testid, "${release_text}")]

user checks scheduled releases tab contains publication
    [Arguments]    ${publication_name}
    user checks page contains element    xpath://*[@id="scheduled-releases"]//h3[text()="${publication_name}"]

user waits until scheduled releases tab contains publication
    [Arguments]    ${publication_name}
    user waits until page contains element    xpath://*[@id="scheduled-releases"]//h3[text()="${publication_name}"]

user checks scheduled releases tab publication has release
    [Arguments]    ${publication_name}    ${release_text}
    user checks page contains element
    ...    xpath://*[@id="scheduled-releases"]//*[@data-testid="releaseByStatusTab ${publication_name}"]//*[contains(@data-testid, "${release_text}")]

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
    [Arguments]    ${PUBLICATION_NAME}    ${RELEASE_NAME}    ${RELEASE_STATUS}
    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    ${accordion}=    user gets accordion section content element    ${PUBLICATION_NAME}
    user opens details dropdown    ${RELEASE_NAME} ${RELEASE_STATUS}    ${accordion}
    ${details}=    user gets details content element    ${RELEASE_NAME} ${RELEASE_STATUS}    ${accordion}
    user waits until parent contains element    ${details}    xpath:.//a[text()="View release"]
    user clicks button    Amend release    ${details}
    user clicks button    Confirm
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
    [Arguments]    ${release_type}=original
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
    user enters text into element    id:releaseStatusForm-latestInternalReleaseNote    Approved by UI tests
    user clicks radio    Immediately
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
    ...    css:#publicationsReleases-themeTopic-themeId,[data-testid='no-permission-to-access-releases']
    ...    %{WAIT_LONG}

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

user puts release into higher level review
    user clicks link    Sign off
    user waits until page does not contain loading spinner
    user waits until h2 is visible    Sign off
    user clicks button    Edit release status
    user waits until h2 is visible    Edit release status    %{WAIT_SMALL}
    user clicks radio    Ready for higher review
    user enters text into element    id:releaseStatusForm-latestInternalReleaseNote    Ready for higher review
    user clicks button    Update status
    user waits until element is visible    id:CurrentReleaseStatus-Awaiting higher review

user approves release for scheduled release
    [Arguments]    ${DAYS_UNTIL_RELEASE}    ${NEXT_RELEASE_MONTH}=01    ${NEXT_RELEASE_YEAR}=2200
    ${PUBLISH_DATE_DAY}=    get current datetime    %-d    ${DAYS_UNTIL_RELEASE}
    ${PUBLISH_DATE_MONTH}=    get current datetime    %-m    ${DAYS_UNTIL_RELEASE}
    ${PUBLISH_DATE_MONTH_WORD}=    get current datetime    %B    ${DAYS_UNTIL_RELEASE}
    ${PUBLISH_DATE_YEAR}=    get current datetime    %Y    ${DAYS_UNTIL_RELEASE}
    set suite variable    ${PUBLISH_DATE_DAY}
    set suite variable    ${PUBLISH_DATE_MONTH}
    set suite variable    ${PUBLISH_DATE_MONTH_WORD}
    set suite variable    ${PUBLISH_DATE_YEAR}

    user clicks link    Sign off
    user waits until page does not contain loading spinner
    user waits until h2 is visible    Sign off
    user waits until page contains button    Edit release status

    user clicks button    Edit release status
    user waits until h2 is visible    Edit release status

    user clicks radio    Approved for publication
    user enters text into element    id:releaseStatusForm-latestInternalReleaseNote    Approved by UI tests
    user clicks radio    On a specific date
    user waits until page contains    Publish date
    user enters text into element    id:releaseStatusForm-publishScheduled-day    ${PUBLISH_DATE_DAY}
    user enters text into element    id:releaseStatusForm-publishScheduled-month    ${PUBLISH_DATE_MONTH}
    user enters text into element    id:releaseStatusForm-publishScheduled-year    ${PUBLISH_DATE_YEAR}
    user enters text into element    id:releaseStatusForm-nextReleaseDate-month    ${NEXT_RELEASE_MONTH}
    user enters text into element    id:releaseStatusForm-nextReleaseDate-year    ${NEXT_RELEASE_YEAR}

    user clicks button    Update status
    user waits until h2 is visible    Confirm publish date
    user clicks button    Confirm

user verifies release summary
    [Arguments]    ${PUBLICATION_NAME}    ${TIME_PERIOD}    ${RELEASE_PERIOD}    ${LEAD_STATISTICIAN}    ${RELEASE_TYPE}
    user waits until h2 is visible    Release summary
    user checks summary list contains    Publication title    ${PUBLICATION_NAME}
    user checks summary list contains    Time period    ${TIME_PERIOD}
    user checks summary list contains    Release period    ${RELEASE_PERIOD}
    user checks summary list contains    Lead statistician    ${LEAD_STATISTICIAN}
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
    IF    ${is_publishing_strategy_with_release} is ${TRUE}
        user checks summary list contains    When to publish    With a specific release
        user checks summary list contains    Publish with release    ${with_release}
    ELSE
        user checks summary list contains    When to publish    ${publishing_strategy}
    END

user changes methodology status to Draft
    user clicks button    Edit status
    user clicks element    id:methodologyStatusForm-status-Draft
    user clicks button    Update status
    user waits until h2 is visible    Sign off
    user checks page contains tag    In Draft

user gives analyst publication owner access
    [Arguments]    ${PUBLICATION_NAME}    ${ANALYST_EMAIL}=EES-test.ANALYST1@education.gov.uk
    user goes to manage user    ${ANALYST_EMAIL}
    user chooses select option    css:[name="selectedPublicationId"]    ${PUBLICATION_NAME}
    user waits until element is enabled    css:[name="selectedPublicationRole"]
    user chooses select option    css:[name="selectedPublicationRole"]    Owner
    user clicks button    Add publication access
    user waits until page does not contain loading spinner

user gives release access to analyst
    [Arguments]    ${RELEASE_NAME}    ${ROLE}    ${ANALYST_EMAIL}=EES-test.ANALYST1@education.gov.uk
    user goes to manage user    ${ANALYST_EMAIL}
    user scrolls to element    css:[name="selectedReleaseId"]
    user chooses select option    css:[name="selectedReleaseId"]    ${RELEASE_NAME}
    user waits until element is enabled    css:[name="selectedReleaseRole"]
    user chooses select option    css:[name="selectedReleaseRole"]    ${ROLE}
    user clicks button    Add release access
    user waits until page does not contain loading spinner

user removes publication owner access from analyst
    [Arguments]    ${PUBLICATION_NAME}    ${ANALYST_EMAIL}=EES-test.ANALYST1@education.gov.uk
    user goes to manage user    ${ANALYST_EMAIL}
    ${table}=    user gets testid element    publicationAccessTable
    ${row}=    get child element    ${table}
    ...    xpath://tbody/tr[td[//th[text()="Publication"] and text()="${PUBLICATION_NAME}"] and td[//th[text()="Role"] and text()="Owner"]]
    user clicks button    Remove    ${row}
    user waits until page does not contain loading spinner

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
    user waits until h1 is visible    Users    %{WAIT_SMALL}
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
    user waits until element is not visible    id:editingMode
