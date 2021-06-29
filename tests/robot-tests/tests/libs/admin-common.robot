*** Settings ***
Resource    ./common.robot
Library     admin-utilities.py

*** Keywords ***
user signs in as bau1
    [Arguments]  ${open_browser}=True
    run keyword if  ${open_browser}  user opens the browser
    environment variable should be set   ADMIN_URL
    user goes to url  %{ADMIN_URL}
    user waits until h1 is visible    Sign in
    user signs in as  ADMIN

    user waits until h1 is visible   Dashboard
    user waits until page contains title caption  Welcome Bau1
    user waits until page contains element   css:#publicationsReleases-themeTopic-themeId,[data-testid='no-permission-to-access-releases']   %{WAIT_LONG}

    user checks breadcrumb count should be  2
    user checks nth breadcrumb contains  1   Home
    user checks nth breadcrumb contains  2   Administrator dashboard

user signs in as analyst1
    [Arguments]  ${open_browser}=True
    run keyword if  ${open_browser}  user opens the browser
    environment variable should be set   ADMIN_URL
    user goes to url  %{ADMIN_URL}
    user waits until h1 is visible    Sign in
    user signs in as  ANALYST
    user waits until h1 is visible  Dashboard
    user waits until page contains title caption  Welcome Analyst1
    user waits until page contains element  css:[id="publicationsReleases-themeTopic-themeId"]  60 

    # @TODO: Luke - See if this test id is being stripped out of the DOM by React
    # no selector with this data-test id is present 
    # user waits until page contains element   css:#publicationsReleases-themeTopic-themeId,[data-testid='no-permission-to-access-releases']  %{WAIT_LONG}

    user checks breadcrumb count should be  2
    user checks nth breadcrumb contains  1   Home
    user checks nth breadcrumb contains  2   Administrator dashboard

user changes to bau1
    user signs out
    user signs in as bau1  False

user changes to analyst1
    user signs out
    user signs in as analyst1  False

user signs out
    user clicks link  Sign out
    user waits until h1 is visible  Signed out
    user waits until page contains  You have successfully signed out

user selects theme and topic from admin dashboard
    [Arguments]  ${theme}  ${topic}
    ${current_url}=  Get Location
    run keyword if  "${current_url}" != "%{ADMIN_URL}"  user navigates to admin dashboard
    user waits until page contains link  Manage publications and releases  90
    user clicks link   Manage publications and releases
    user waits until page contains element   id:publicationsReleases-themeTopic-themeId  60 
    user selects from list by label  id:publicationsReleases-themeTopic-themeId  ${theme}
    user waits until page contains element   id:publicationsReleases-themeTopic-topicId  60 
    user selects from list by label  id:publicationsReleases-themeTopic-topicId  ${topic}
    user waits until h2 is visible  ${theme}  60 
    user waits until h3 is visible  ${topic}  60

user navigates to release summary from admin dashboard
    [Arguments]   ${PUBLICATION_NAME}    ${DETAILS_HEADING}
    user opens publication on the admin dashboard   ${PUBLICATION_NAME}

    ${accordion}=  user gets accordion section content element   ${PUBLICATION_NAME}
    user opens details dropdown   ${DETAILS_HEADING}  ${accordion}
    ${details}=  user gets details content element   ${DETAILS_HEADING}  ${accordion}

    user waits until parent contains element   ${details}   xpath:.//a[text()="Edit this release"]  60
    ${edit_button}=  get child element  ${details}  xpath:.//a[text()="Edit this release"]
    user clicks element   ${edit_button}

    user waits until h2 is visible  Release summary  60
    user checks summary list contains   Publication title  ${PUBLICATION_NAME}

user creates publication
    [Arguments]   ${title}
    user waits until h1 is visible  Create new publication  60
    user waits until page contains element  id:publicationForm-title  60
    user enters text into element  id:publicationForm-title   ${title}
    user enters text into element  id:publicationForm-teamName        Attainment statistics team
    user enters text into element  id:publicationForm-teamEmail       Attainment.STATISTICS@education.gov.uk
    user enters text into element  id:publicationForm-contactName     Tingting Shu
    user enters text into element  id:publicationForm-contactTelNo    0123456789
    user clicks button   Save publication
    user waits until h1 is visible  Dashboard  60

user creates release for publication
    [Arguments]  ${publication}  ${time_period_coverage}  ${start_year}
    user waits until page contains title caption  ${publication}
    user waits until h1 is visible  Create new release  60
    user waits until page contains element  id:releaseSummaryForm-timePeriodCoverage  60
    user selects from list by label  id:releaseSummaryForm-timePeriodCoverageCode  ${time_period_coverage}
    user enters text into element  id:releaseSummaryForm-timePeriodCoverageStartYear  ${start_year}
    user clicks radio   National Statistics
    user clicks radio if exists  Create new template
    user clicks button  Create new release
    user waits until page contains element  xpath://a[text()="Edit release summary"]  60
    user waits until h2 is visible  Release summary  60
    
user opens publication on the admin dashboard
    [Arguments]  
    ...  ${publication}    
    ...  ${theme}=%{TEST_THEME_NAME}    
    ...  ${topic}=%{TEST_TOPIC_NAME}
    
    user navigates to admin dashboard
    user selects theme and topic from admin dashboard  ${theme}  ${topic}
    user waits until page contains accordion section   ${publication}   %{WAIT_MEDIUM}
    ${accordion}=  user opens accordion section  ${publication}
    [Return]  ${accordion}
        
user creates methodology for publication
    [Arguments]
    ...  ${publication}
    ...  ${theme}=%{TEST_THEME_NAME}
    ...  ${topic}=%{TEST_TOPIC_NAME}

    ${accordion}=  user opens publication on the admin dashboard   ${publication}  ${theme}  ${topic}
    user clicks button   Create methodology  ${accordion}
    user waits until h2 is visible  Methodology summary
    user checks summary list contains   Title   ${publication}
    user checks summary list contains   Status  Draft
    user checks summary list contains   Published on  Not yet published
    
user views methodology for publication
    [Arguments]  ${publication}     ${methodology_title}=${publication}
    ${accordion}=  user opens publication on the admin dashboard   ${publication}
    user views methodology for open publication accordion  ${accordion}  ${methodology_title}

user views methodology for open publication accordion
    [Arguments]  ${accordion}     ${methodology_title}
    user opens details dropdown  ${methodology_title}  ${accordion}
    user clicks link   Edit this methodology
    user waits until h2 is visible  Methodology summary 
    
user links publication to external methodology
    [Arguments]  
    ...  ${publication}
    ...  ${title}=${publication}
    ...  ${link}=https://example.com
    
    ${accordion}=  user opens publication on the admin dashboard   ${publication}
    user clicks button   Link to an externally hosted methodology  ${accordion}
    user waits until legend is visible   Link to an externally hosted methodology
    user enters text into textfield  Link title   ${title}
    user enters text into textfield  URL   ${link}
    user clicks button  Save
    
user edits an external methodology
    [Arguments]  
    ...  ${publication}
    ...  ${new_title}=${publication} updated
    ...  ${new_link}=https://example.com/updated
    ...  ${original_title}=${publication}
    ...  ${original_link}=https://example.com
    
    ${accordion}=  user opens publication on the admin dashboard   ${publication}
    user clicks button   Edit  ${accordion}
    user waits until legend is visible   Link to an externally hosted methodology
    user checks textfield contains  Link title  ${original_title}
    user checks textfield contains  URL  ${original_link}
    user enters text into textfield  Link title   ${new_title}
    user enters text into textfield  URL   ${new_link}
    user clicks button  Save
    
user removes an external methodology from publication
    [Arguments]  ${publication}
    ${accordion}=  user opens publication on the admin dashboard   ${publication}
    user clicks button   Remove  ${accordion}
                
user adds basic release content
    [Arguments]  ${publication}
    user clicks button  Add a summary text block
    user waits until element contains  id:releaseSummary  This section is empty  60
    user clicks button   Edit block  id:releaseSummary
    user presses keys  Test summary text for ${publication}
    user clicks element   css:body  # To ensure Save button gets clicked
    user clicks button   Save  id:releaseSummary
    user waits until element contains  id:releaseSummary  Test summary text for ${publication}  60

    user clicks button  Add a headlines text block  id:releaseHeadlines
    user waits until element contains  id:releaseHeadlines  This section is empty  60
    user clicks button  Edit block  id:releaseHeadlines
    user presses keys   Test headlines summary text for ${publication}
    user clicks button  Save  id:releaseHeadlines
    user waits until element contains  id:releaseHeadlines  Test headlines summary text for ${publication}  60

    user waits until button is enabled  Add new section
    user clicks button  Add new section

    user changes accordion section title  1   Test section one   css:#releaseMainContent
    user adds text block to editable accordion section   Test section one   css:#releaseMainContent
    user adds content to accordion section text block  Test section one   1    Test content block for ${publication}   css:#releaseMainContent

user creates approved methodology
    [Arguments]  ${title}
    user waits until h1 is visible  Manage methodologies
    user waits until page contains element  id:approved-methodologies-tab
    user clicks element  id:approved-methodologies-tab
    ${is_approved}=  run keyword and return status  user waits until page contains element  xpath://section[@id="approved-methodologies"]//a[text()="${title}"]  1
    user clicks element  id:draft-methodologies-tab
    ${is_draft}=  run keyword and return status  user waits until page contains element  xpath://section[@id="draft-methodologies"]//a[text()="${title}"]  1
    run keyword if  ${is_approved} == False and ${is_draft} == False  run keywords
    ...  user creates methodology  ${title}
    ...  AND    user approves methodology  ${title}
    run keyword if  ${is_draft} == True   run keywords
    ...  user clicks element    id:draft-methodologies-tab
    ...  AND    user clicks link  ${title}
    ...  AND    user approves methodology  ${title}

user creates methodology
    [Arguments]  ${title}
    user waits until h1 is visible  Manage methodologies
    user clicks link  Create new methodology
    user waits until h1 is visible  Create new methodology
    user enters text into element  id:createMethodologyForm-title   ${title}
    user clicks button  Create methodology
    user waits until page contains title caption  Edit methodology
    user waits until h1 is visible  ${title}

user approves methodology
    [Arguments]  ${title}
    user waits until page contains title caption  Edit methodology
    user waits until h1 is visible  ${title}
    user clicks link  Sign off
    user clicks button  Edit status
    user waits until h2 is visible  Edit methodology status
    user clicks radio  Approved for publication
    user enters text into element  id:methodologyStatusForm-latestInternalReleaseNote  Test release note
    user clicks button  Update status

    user waits until h2 is visible  Methodology status
    user checks page contains tag  Approved

user creates public prerelease access list
    [Arguments]  ${content}
    user clicks link  Public access list
    user waits until h2 is visible  Public pre-release access list
    user clicks button   Create public pre-release access list
    user presses keys  CTRL+a+BACKSPACE
    user presses keys  ${content}
    user clicks button  Save access list
    user waits until element contains  css:[data-testid="publicPreReleaseAccessListPreview"]  ${content}

user updates public prerelease access list
    [Arguments]  ${content}
    user clicks link  Public access list
    user waits until h2 is visible  Public pre-release access list
    user clicks button   Edit public pre-release access list
    user presses keys  CTRL+a
    user presses keys  BACKSPACE
    user presses keys  ${content}
    user clicks button  Save access list
    user waits until element contains  css:[data-testid="publicPreReleaseAccessListPreview"]  ${content}

user checks draft releases tab contains publication
    [Arguments]    ${publication_name}
    user checks page contains element   xpath://*[@id="draft-releases"]//h3[text()="${publication_name}"]

user waits until draft releases tab contains publication
    [Arguments]    ${publication_name}
    user waits until page contains element   xpath://*[@id="draft-releases"]//h3[text()="${publication_name}"]

user checks draft releases tab publication has release
    [Arguments]   ${publication_name}   ${release_text}
    user checks page contains element   xpath://*[@id="draft-releases"]//*[@data-testid="releaseByStatusTab ${publication_name}"]//*[contains(@data-testid, "${release_text}")]

user checks scheduled releases tab contains publication
    [Arguments]    ${publication_name}
    user checks page contains element   xpath://*[@id="scheduled-releases"]//h3[text()="${publication_name}"]

user waits until scheduled releases tab contains publication
    [Arguments]    ${publication_name}
    user waits until page contains element   xpath://*[@id="scheduled-releases"]//h3[text()="${publication_name}"]

user checks scheduled releases tab publication has release
    [Arguments]   ${publication_name}   ${release_text}
    user checks page contains element   xpath://*[@id="scheduled-releases"]//*[@data-testid="releaseByStatusTab ${publication_name}"]//*[contains(@data-testid, "${release_text}")]

user clicks footnote radio
    [Arguments]  ${subject_label}  ${radio_label}
    user clicks element  xpath://*[@data-testid="footnote-subject ${subject_label}"]//label[text()="${radio_label}"]/../input[@type="radio"]

user clicks footnote checkbox
    [Arguments]  ${label}    ${parent}=css:body
    user waits until parent contains element  ${parent}   xpath:.//*[@id="footnoteForm"]//label[text()="${label}"]/../input
    ${checkbox}=  get child element  ${parent}    xpath://*[@id="footnoteForm"]//label[text()="${label}"]/../input
    page should contain checkbox  ${checkbox}
    user scrolls to element   ${checkbox}
    wait until element is enabled   ${checkbox}
    user clicks element     ${checkbox}

user checks footnote checkbox is selected
    [Arguments]  ${label}   ${parent}=css:body
    user waits until parent contains element  ${parent}   xpath:.//*[@id="footnoteForm"]//label[contains(text(), "${label}")]/../input
    ${checkbox}=  get child element   ${parent}   xpath://*[@id="footnoteForm"]//label[contains(text(), "${label}")]/../input
    wait until element is enabled   ${checkbox}
    checkbox should be selected     ${checkbox}

user opens nth editable accordion section
    [Arguments]  ${section_num}  ${parent}=css:body
    user waits until parent contains element  ${parent}  xpath:.//*[@data-testid="editableAccordionSection"][${section_num}]
    ${section}=  get child element  ${parent}  xpath:.//*[@data-testid="editableAccordionSection"][${section_num}]
    ${header_button}=  get child element  ${section}  css:h2 > button[aria-expanded]
    ${is_expanded}=  get element attribute  ${header_button}  aria-expanded
    run keyword if  '${is_expanded}' != 'true'  user clicks element  ${header_button}
    user checks element attribute value should be  ${header_button}  aria-expanded  true

user changes accordion section title
    [Arguments]  ${section_num}  ${title}  ${parent}=id:releaseMainContent
    user opens nth editable accordion section  ${section_num}  ${parent}
    ${section}=  get child element  ${parent}  xpath:.//*[@data-testid="editableAccordionSection"][${section_num}]
    user clicks button  Edit section title  ${section}
    user waits until parent contains element  ${section}  css:input[name="heading"]
    ${input}=  get child element  ${section}  css:input[name="heading"]
    user enters text into element  ${input}  ${title}
    user clicks button  Save section title  ${section}
    user waits until parent contains element  ${section}  xpath:.//h2/button[@aria-expanded and text()="${title}"]

user checks accordion section contains x blocks
    [Arguments]  ${section_name}  ${num_blocks}  ${parent}=css:[data-testid="accordion"]
    ${section}=  user gets accordion section content element  ${section_name}  ${parent}
    ${blocks}=  get child elements  ${section}  css:[data-testid="editableSectionBlock"]
    length should be  ${blocks}  ${num_blocks}

user adds text block to editable accordion section
    [Arguments]  ${section_name}   ${parent}=css:[data-testid="accordion"]
    ${section}=  user gets accordion section content element  ${section_name}  ${parent}
    user clicks button  Add text block  ${section}
    user waits until element contains  ${section}  This section is empty

user adds data block to editable accordion section
    [Arguments]   ${section_name}   ${block_name}   ${parent}=css:[data-testid="accordion"]
    ${section}=  user gets accordion section content element  ${section_name}   ${parent}
    user waits for page to finish loading
    user clicks button  Add data block   ${section}
    ${block_list}=  get child element  ${section}  css:select[name="selectedDataBlock"]
    user selects from list by label  ${block_list}  Dates data block name
    user waits until parent contains element  ${section}   css:table
    user clicks button  Embed  ${section}

user adds content to accordion section text block
    [Arguments]  ${section_name}  ${block_num}  ${content}   ${parent}=[data-testid="accordion"]
    ${section}=  user gets accordion section content element  ${section_name}   ${parent}
    ${block}=  get child element  ${section}  css:[data-testid="editableSectionBlock"]:nth-of-type(${block_num})
    user clicks button  Edit block  ${block}
    user presses keys  CTRL+a
    user presses keys  BACKSPACE
    user presses keys  ${content}
    user clicks button  Save  ${block}
    user waits until element contains  ${block}  ${content}

user checks accordion section text block contains
    [Arguments]  ${section_name}  ${block_num}  ${content}   ${parent}=[data-testid="accordion"]
    ${section}=  user gets accordion section content element  ${section_name}  ${parent}
    ${block}=  get child element  ${section}  css:[data-testid="editableSectionBlock"]:nth-of-type(${block_num})
    user waits until element contains  ${block}  ${content}

user deletes editable accordion section content block
    [Arguments]  ${section_name}  ${block_num}  ${parent}=[data-testid="accordion"]
    ${section}=  user gets accordion section content element  ${section_name}   ${parent}
    ${block}=  get child element  ${section}  css:[data-testid="editableSectionBlock"]:nth-of-type(${block_num})
    user clicks button  Remove block  ${block}
    user clicks button  Confirm

user gets meta guidance data file content editor
    [Arguments]   ${accordion_heading}
    user waits until page contains element   id:metaGuidance-dataFiles
    ${accordion}=  user gets accordion section content element  ${accordion_heading}   id:metaGuidance-dataFiles
    user waits until parent contains element  ${accordion}   xpath:.//*[@data-testid="Content"]//*[@role="textbox"]
    ${editor}=  get child element  ${accordion}  xpath:.//*[@data-testid="Content"]//*[@role="textbox"]
    [Return]  ${editor}

user enters text into meta guidance data file content editor
    [Arguments]   ${accordion_heading}   ${text}
    ${editor}=  user gets meta guidance data file content editor  ${accordion_heading}
    user checks page does not contain testid   fileGuidanceContent-focused
    user clicks element  ${editor}
    user waits until page contains testid   fileGuidanceContent-focused
    user enters text into element  ${editor}  ${text}

user creates amendment for release
    [Arguments]  ${PUBLICATION_NAME}  ${RELEASE_NAME}  ${RELEASE_STATUS}
    user opens publication on the admin dashboard   ${PUBLICATION_NAME}
    ${accordion}=  user gets accordion section content element  ${PUBLICATION_NAME}
    user opens details dropdown   ${RELEASE_NAME} ${RELEASE_STATUS}  ${accordion}
    ${details}=  user gets details content element  ${RELEASE_NAME} ${RELEASE_STATUS}  ${accordion}
    user waits until parent contains element   ${details}   xpath:.//a[text()="View this release"]
    user clicks button  Amend this release  ${details}
    user clicks button  Confirm

user deletes subject file
    [Arguments]  ${SUBJECT_NAME}
    user waits until page contains accordion section  ${SUBJECT_NAME}
    user opens accordion section  ${SUBJECT_NAME}
    user scrolls to accordion section content  ${SUBJECT_NAME}
    ${accordion}=  user gets accordion section content element  ${SUBJECT_NAME}
    ${button}=  user gets button element  Delete files  ${accordion}
    user clicks element  ${button}
    user clicks button  Confirm

user approves release for immediate publication
    user waits until h2 is visible  Sign off
    user waits until page contains button  Edit release status
    user clicks button  Edit release status
    user waits until h2 is visible  Edit release status
    user clicks radio   Approved for publication
    user enters text into element  id:releaseStatusForm-latestInternalReleaseNote  Approved by UI tests
    user clicks radio   Immediately
    user clicks button   Update status
    user waits until h2 is visible  Sign off  %{WAIT_MEDIUM}
    user checks summary list contains  Current status  Approved
    user waits for release process status to be  Complete    ${release_complete_wait}
    user reloads page  # EES-1448
    user checks page does not contain button  Edit release status

user navigates to admin dashboard 
    [Arguments]  ${USER}=
    user goes to url  %{ADMIN_URL}
    user waits until h1 is visible   Dashboard
    Run keyword if  "${USER}" != ""  user waits until page contains title caption  Welcome ${USER}
    user waits until page contains element   css:#publicationsReleases-themeTopic-themeId,[data-testid='no-permission-to-access-releases']

user uploads subject 
    [Arguments]  ${SUBJECT_NAME}  ${SUBJECT_FILE}  ${META_FILE}
    user waits until page contains element  id:dataFileUploadForm-subjectTitle  60
    user enters text into element  id:dataFileUploadForm-subjectTitle   ${SUBJECT_NAME}
    user chooses file   id:dataFileUploadForm-dataFile       ${FILES_DIR}${SUBJECT_FILE}
    user chooses file   id:dataFileUploadForm-metadataFile   ${FILES_DIR}${META_FILE}
    user clicks button  Upload data files
    user waits until h2 is visible  Uploaded data files  60
    user waits until page contains accordion section   ${SUBJECT_NAME}  60
    user opens accordion section   ${SUBJECT_NAME}
    ${section}=  user gets accordion section content element  ${SUBJECT_NAME}
    user checks headed table body row contains  Status  Complete  ${section}  %{WAIT_LONG}

user approves release for scheduled release
    [Arguments]  ${DAYS_TILL_RELEASE}  ${RELEASE_MONTH}  ${RELEASE_YEAR}
    ${PUBLISH_DATE_DAY}=  get current datetime  %-d  ${DAYS_TILL_RELEASE}
    ${PUBLISH_DATE_MONTH}=  get current datetime  %-m   ${DAYS_TILL_RELEASE}
    ${PUBLISH_DATE_MONTH_WORD}=  get current datetime  %B  ${DAYS_TILL_RELEASE}
    ${PUBLISH_DATE_YEAR}=  get current datetime  %Y  ${DAYS_TILL_RELEASE}
    set suite variable  ${PUBLISH_DATE_DAY}
    set suite variable  ${PUBLISH_DATE_MONTH}
    set suite variable  ${PUBLISH_DATE_MONTH_WORD}
    set suite variable  ${PUBLISH_DATE_YEAR}

    user waits until h2 is visible  Sign off
    user waits until page contains button  Edit release status

    user clicks button  Edit release status
    user waits until h2 is visible  Edit release status

    user clicks radio   Approved for publication
    user enters text into element  id:releaseStatusForm-latestInternalReleaseNote  Approved by UI tests
    user clicks radio  On a specific date
    user waits until page contains   Publish date
    user enters text into element  id:releaseStatusForm-publishScheduled-day    ${PUBLISH_DATE_DAY}
    user enters text into element  id:releaseStatusForm-publishScheduled-month  ${PUBLISH_DATE_MONTH}
    user enters text into element  id:releaseStatusForm-publishScheduled-year   ${PUBLISH_DATE_YEAR}
    user enters text into element  id:releaseStatusForm-nextReleaseDate-month   ${RELEASE_MONTH}
    user enters text into element  id:releaseStatusForm-nextReleaseDate-year    ${RELEASE_YEAR}

    user clicks button   Update status
    user waits until h1 is visible  Confirm publish date
    user clicks button  Confirm

user creates new content section
    [Arguments]   ${SECTION_NUMBER}  ${CONTENT_SECTION_NAME}
    user waits until button is enabled  Add new section
    user clicks button  Add new section
    user changes accordion section title  ${SECTION_NUMBER}   ${CONTENT_SECTION_NAME}

user verifies release summary
    [Arguments]  ${PUBLICATION_NAME}  ${TIME_PERIOD}  ${RELEASE_PERIOD}  ${LEAD_STATISTICIAN}  ${RELEASE_TYPE}
    user waits until h2 is visible     Release summary
    user checks summary list contains  Publication title      ${PUBLICATION_NAME}
    user checks summary list contains  Time period            ${TIME_PERIOD}
    user checks summary list contains  Release period         ${RELEASE_PERIOD}
    user checks summary list contains  Lead statistician      ${LEAD_STATISTICIAN}
    user checks summary list contains  Release type           ${RELEASE_TYPE}

user changes methodology status to Approved
    user clicks button  Edit status
    user clicks element  id:methodologyStatusForm-status-Approved
    user enters text into element  id:methodologyStatusForm-latestInternalReleaseNote  Approved by UI tests
    user clicks button  Update status

user gives analyst publication owner access
    [Arguments]  ${ANALYST_EMAIL}  ${PUBLICATION_NAME}
    user clicks link  Manage  xpath://td[text()="${ANALYST_EMAIL}"]/..
    user waits until page does not contain loading spinner

    # stale element exception if you don't wait until it's enabled    
    user waits until button is enabled  Add publication access
    user scrolls to element  css:[name="selectedPublicationId"]
    
    user waits until element is enabled  css:[name="selectedPublicationId"]
    user selects from list by label  css:[name="selectedPublicationId"]  ${PUBLICATION_NAME}

    user waits until element is enabled  css:[name="selectedPublicationRole"]
    user selects from list by label  css:[name="selectedPublicationRole"]  Owner
    user clicks button  Add publication access

user removes publication owner access from analyst 
    [Arguments]  ${PUBLICATION_NAME}
    user waits until element is enabled  css:[name="selectedPublicationId"]
    user scrolls to element  css:[name="selectedPublicationId"]
    user clicks element  testid:remove-publication-role-${PUBLICATION_NAME}
    user waits until page does not contain loading spinner

user gives analyst release access 
    [Arguments]    ${RELEASE_NAME}  ${ROLE}
    user waits until element is enabled  css:[name="selectedReleaseId"]
    user scrolls to element  css:[name="selectedReleaseId"]
    user selects from list by label  css:[name="selectedReleaseId"]  ${RELEASE_NAME}
    user waits until element is enabled  css:[name="selectedReleaseRole"]
    user selects from list by label  css:[name="selectedReleaseRole"]  ${ROLE}
    user clicks button  Add release access
    user waits until page does not contain loading spinner