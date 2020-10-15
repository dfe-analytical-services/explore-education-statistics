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
    user waits until page contains element   id:publicationsReleases-themeTopic-themeId   180

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
    user waits until page contains element   id:publicationsReleases-themeTopic-themeId   180

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
    user waits until page contains link  Manage publications and releases  60
    user clicks link   Manage publications and releases
    user waits until page contains element   id:publicationsReleases-themeTopic-themeId
    user selects from list by label  id:publicationsReleases-themeTopic-themeId  ${theme}
    user waits until page contains element   id:publicationsReleases-themeTopic-topicId
    user selects from list by label  id:publicationsReleases-themeTopic-topicId  ${topic}
    user waits until h2 is visible  ${theme}
    user waits until h3 is visible  ${topic}

user navigates to release summary from admin dashboard
    [Arguments]   ${PUBLICATION_NAME}    ${DETAILS_HEADING}
    user selects theme and topic from admin dashboard  %{TEST_THEME_NAME}  %{TEST_TOPIC_NAME}
    user waits until page contains accordion section   ${PUBLICATION_NAME}
    user opens accordion section  ${PUBLICATION_NAME}

    ${accordion}=  user gets accordion section content element   ${PUBLICATION_NAME}
    user opens details dropdown   ${DETAILS_HEADING}  ${accordion}
    ${details}=  user gets details content element   ${DETAILS_HEADING}  ${accordion}

    user waits until parent contains element   ${details}   xpath:.//a[text()="Edit this release"]
    ${edit_button}=  get child element  ${details}  xpath:.//a[text()="Edit this release"]
    user clicks element   ${edit_button}

    user waits until h2 is visible  Release summary
    user checks summary list contains   Publication title  ${PUBLICATION_NAME}

user creates publication
    [Arguments]   ${title}
    user waits until h1 is visible  Create new publication
    user waits until page contains element  id:publicationForm-title
    user enters text into element  id:publicationForm-title   ${title}
    user clicks radio     No methodology
    user enters text into element  id:publicationForm-teamName        Attainment statistics team
    user enters text into element  id:publicationForm-teamEmail       Attainment.STATISTICS@education.gov.uk
    user enters text into element  id:publicationForm-contactName     Tingting Shu
    user enters text into element  id:publicationForm-contactTelNo    0123456789
    user clicks button   Save publication
    user waits until h1 is visible  Dashboard

user creates release for publication
    [Arguments]  ${publication}  ${time_period_coverage}  ${start_year}
    user waits until page contains title caption  ${publication}
    user waits until h1 is visible  Create new release
    user waits until page contains element  id:releaseSummaryForm-timePeriodCoverage
    user selects from list by label  id:releaseSummaryForm-timePeriodCoverage  ${time_period_coverage}
    user enters text into element  id:releaseSummaryForm-timePeriodCoverageStartYear  ${start_year}
    user clicks element if exists   css:[data-testid="Create new template"]
    user clicks radio   National Statistics
    user clicks button  Create new release
    user waits until page contains element  xpath://span[text()="Edit release"]
    user waits until h2 is visible  Release summary

user adds basic release content
    [Arguments]  ${publication}
    user clicks button  Add a summary text block
    user waits until element contains  id:releaseSummary  This section is empty
    user clicks button   Edit block  id:releaseSummary
    user presses keys  Test summary text for ${publication}
    user clicks button   Save  id:releaseSummary
    user waits until element contains  id:releaseSummary  Test summary text for ${publication}

    user clicks button  Add a headlines text block  id:releaseHeadlines
    user waits until element contains  id:releaseHeadlines  This section is empty
    user clicks button  Edit block  id:releaseHeadlines
    user presses keys   Test headlines summary text for ${publication}
    user clicks button  Save  id:releaseHeadlines
    user waits until element contains  id:releaseHeadlines  Test headlines summary text for ${publication}

    user waits until button is enabled  Add new section
    user clicks button  Add new section

    user changes accordion section title  1   Test section one
    user adds text block to editable accordion section   Test section one
    user adds content to accordion section text block  Test section one   1    Test content block for ${publication}

user creates approved methodology
    [Arguments]  ${title}
    user waits until h1 is visible  Manage methodologies
    user waits until page contains element  id:approved-methodologies-tab
    user clicks element  id:approved-methodologies-tab
    ${is_approved}=  run keyword and return status  user checks page contains element  xpath://section[@id="approved-methodologies"]//a[text()="${title}"]
    user clicks element  id:draft-methodologies-tab
    ${is_draft}=  run keyword and return status  user checks page contains element  xpath://section[@id="draft-methodologies"]//a[text()="${title}"]
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
    user waits until page contains element  id:live-methodologies-tab
    user clicks element  id:live-methodologies-tab
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
    user clicks link  Release status
    user clicks button  Edit status
    user waits until h2 is visible  Edit methodology status
    user clicks radio  Approved for publication
    user enters text into element  id:methodologyStatusForm-internalReleaseNote  Test release note
    user clicks button  Update status

    user waits until h2 is visible  Methodology status
    user checks page contains tag  Approved

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

user clicks footnote checkbox
    [Arguments]  ${label}
    user waits until page contains element  xpath://*[@id="footnoteForm"]//label[text()="${label}"]/../input
    page should contain checkbox  xpath://*[@id="footnoteForm"]//label[text()="${label}"]/../input
    user scrolls to element   xpath://*[@id="footnoteForm"]//label[text()="${label}"]/../input
    wait until element is enabled   xpath://*[@id="footnoteForm"]//label[text()="${label}"]/../input
    user clicks element     xpath://*[@id="footnoteForm"]//label[text()="${label}"]/../input

user checks footnote checkbox is selected
    [Arguments]  ${label}
    wait until element is enabled   xpath://*[@id="footnoteForm"]//label[contains(text(), "${label}")]/../input
    checkbox should be selected     xpath://*[@id="footnoteForm"]//label[contains(text(), "${label}")]/../input

user opens nth editable accordion section
    [Arguments]  ${section_num}  ${parent}=css:body
    user waits until parent contains element  ${parent}  xpath:.//*[@data-testid="editableAccordionSection"][${section_num}]
    ${section}=  get child element  ${parent}  xpath:.//*[@data-testid="editableAccordionSection"][${section_num}]
    ${header_button}=  get child element  ${section}  css:h2 > button[aria-expanded]
    ${is_expanded}=  get element attribute  ${header_button}  aria-expanded
    run keyword if  '${is_expanded}' != 'true'  user clicks element  ${header_button}
    user checks element attribute value should be  ${header_button}  aria-expanded  true

user changes accordion section title
    [Arguments]  ${section_num}  ${title}  ${parent}=id:releaseContentAccordion
    user opens nth editable accordion section  ${section_num}  ${parent}
    ${section}=  get child element  ${parent}  xpath:.//*[@data-testid="editableAccordionSection"][${section_num}]
    user clicks button  Edit section title  ${section}
    user waits until parent contains element  ${section}  css:input[name="heading"]
    ${input}=  get child element  ${section}  css:input[name="heading"]
    user enters text into element  ${input}  ${title}
    user clicks button  Save section title  ${section}
    user waits until parent contains element  ${section}  xpath:.//h2/button[@aria-expanded and text()="${title}"]

user checks accordion section contains x blocks
    [Arguments]  ${section_name}  ${num_blocks}
    ${section}=  user gets accordion section content element  ${section_name}
    ${blocks}=  get child elements  ${section}  css:[data-testid="editableSectionBlock"]
    length should be  ${blocks}  ${num_blocks}

user adds text block to editable accordion section
    [Arguments]  ${section_name}
    ${section}=  user gets accordion section content element  ${section_name}
    user clicks button  Add text block  ${section}
    user waits until element contains  ${section}  This section is empty

user adds data block to editable accordion section
    [Arguments]   ${section_name}   ${block_name}
    ${section}=  user gets accordion section content element  ${section_name}
    user clicks button  Add data block   ${section}
    ${block_list}=  get child element  ${section}  css:select[name="selectedDataBlock"]
    user selects from list by label  ${block_list}  Dates data block name
    user waits until parent contains element  ${section}   css:table
    user clicks button  Embed  ${section}

user adds content to accordion section text block
    [Arguments]  ${section_name}  ${block_num}  ${content}
    ${section}=  user gets accordion section content element  ${section_name}
    ${block}=  get child element  ${section}  css:[data-testid="editableSectionBlock"]:nth-of-type(${block_num})
    user clicks button  Edit block  ${block}
    user presses keys  CTRL+a
    user presses keys  BACKSPACE
    user presses keys  ${content}
    user clicks button  Save  ${block}
    user waits until element contains  ${block}  ${content}

user checks accordion section text block contains
    [Arguments]  ${section_name}  ${block_num}  ${content}
    ${section}=  user gets accordion section content element  ${section_name}
    ${block}=  get child element  ${section}  css:[data-testid="editableSectionBlock"]:nth-of-type(${block_num})
    user waits until element contains  ${block}  ${content}

user deletes editable accordion section content block
    [Arguments]  ${section_name}  ${block_num}
    ${section}=  user gets accordion section content element  ${section_name}
    ${block}=  get child element  ${section}  css:[data-testid="editableSectionBlock"]:nth-of-type(${block_num})
    user clicks button  Remove block  ${block}
    user clicks button  Confirm
