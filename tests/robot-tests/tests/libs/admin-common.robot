*** Settings ***
Resource    ./common.robot
Library     admin-utilities.py

*** Keywords ***
user signs in as bau1
  user opens the browser

  environment variable should be set   ADMIN_URL
  user goes to url  %{ADMIN_URL}
  user waits until page contains heading 1    Sign in

  environment variable should be set   IDENTITY_LOCAL_STORAGE_BAU
  set to local storage   GovUk.Education.ExploreEducationStatistics.Adminuser:%{ADMIN_URL}:GovUk.Education.ExploreEducationStatistics.Admin   %{IDENTITY_LOCAL_STORAGE_BAU}
  environment variable should be set   IDENTITY_COOKIE_BAU
  set cookie from json   %{IDENTITY_COOKIE_BAU}

  user goes to url  %{ADMIN_URL}
  user waits until page contains heading 1   Dashboard
  user waits until page contains title caption  Welcome Bau1
  user waits until page contains element   css:#selectTheme   180
  user checks element should contain    css:[data-testid="breadcrumbs--list"] li:nth-child(1)     Home
  user checks element should contain    css:[data-testid="breadcrumbs--list"] li:nth-child(2)     Administrator dashboard

user signs in as analyst1
  user opens the browser

  environment variable should be set   ADMIN_URL
  user goes to url  %{ADMIN_URL}
  user waits until page contains heading 1    Sign in

  environment variable should be set   IDENTITY_LOCAL_STORAGE_ANALYST
  set to local storage   GovUk.Education.ExploreEducationStatistics.Adminuser:%{ADMIN_URL}:GovUk.Education.ExploreEducationStatistics.Admin   %{IDENTITY_LOCAL_STORAGE_ANALYST}
  environment variable should be set   IDENTITY_COOKIE_ANALYST
  set cookie from json   %{IDENTITY_COOKIE_ANALYST}

  user goes to url  %{ADMIN_URL}
  user waits until page contains heading 1  Dashboard
  user waits until page contains title caption  Welcome Analyst1
  user waits until page contains element   css:#selectTheme   180
  user checks element should contain    css:[data-testid="breadcrumbs--list"] li:nth-child(1)     Home
  user checks element should contain    css:[data-testid="breadcrumbs--list"] li:nth-child(2)     Administrator dashboard

user selects theme "${theme}" and topic "${topic}" from the admin dashboard
    user waits until page contains element  id:my-publications-tab   60
    user clicks element   id:my-publications-tab
    user waits until page contains element   id:selectTheme
    user checks element contains  id:my-publications-tab  Manage publications and releases
    user selects from list by label  id:selectTheme  ${theme}
    user waits until page contains element   id:selectTopic
    user selects from list by label  id:selectTopic  ${topic}
    user waits until page contains heading 2  ${theme}
    user waits until page contains heading 3  ${topic}

user creates publication
    [Arguments]   ${title}
    user waits until page contains heading 1  Create new publication
    user waits until page contains element  id:publicationForm-title
    user enters text into element  id:publicationForm-title   ${title}
    user clicks radio     No methodology
    user enters text into element  id:publicationForm-teamName        Attainment statistics team
    user enters text into element  id:publicationForm-teamEmail       Attainment.STATISTICS@education.gov.uk
    user enters text into element  id:publicationForm-contactName     Tingting Shu
    user enters text into element  id:publicationForm-contactTelNo    0123456789
    user clicks button   Save publication
    user waits until page contains heading 1  Dashboard

user creates release for publication
    [Arguments]  ${publication}  ${time_period_coverage}  ${start_year}
    user waits until page contains title caption  ${publication}
    user waits until page contains heading 1  Create new release
    user waits until page contains element  id:releaseSummaryForm-timePeriodCoverage
    user selects from list by label  id:releaseSummaryForm-timePeriodCoverage  ${time_period_coverage}
    user enters text into element  id:releaseSummaryForm-timePeriodCoverageStartYear  ${start_year}
    user clicks radio   National Statistics
    user clicks button  Create new release
    user waits until page contains element  xpath://span[text()="Edit release"]
    user waits until page contains heading 2  Release summary

user creates approved methodology
    [Arguments]  ${title}
    user waits until page contains heading 1  Manage methodologies
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
    user waits until page contains heading 1  Manage methodologies
    user waits until page contains element  id:live-methodologies-tab
    user clicks element  id:live-methodologies-tab
    user clicks link  Create new methodology
    user waits until page contains heading 1  Create new methodology
    user enters text into element  id:createMethodologyForm-title   ${title}
    user clicks button  Create methodology
    user waits until page contains title caption  Edit methodology
    user waits until page contains heading 1  ${title}

user approves methodology
    [Arguments]  ${title}
    user waits until page contains title caption  Edit methodology
    user waits until page contains heading 1  ${title}
    user clicks link  Release status
    user clicks button  Edit status
    user waits until page contains heading 2  Edit methodology status
    user clicks radio  Approved for publication
    user enters text into element  id:methodologyStatusForm-internalReleaseNote  Test release note
    user clicks button  Update status

    user waits until page contains heading 2  Methodology status
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
    wait until page contains element  xpath://*[@id="create-footnote-form"]//label[text()="${label}"]/../input
    page should contain checkbox  xpath://*[@id="create-footnote-form"]//label[text()="${label}"]/../input
    user scrolls to element   xpath://*[@id="create-footnote-form"]//label[text()="${label}"]/../input
    wait until element is enabled   xpath://*[@id="create-footnote-form"]//label[text()="${label}"]/../input
    user clicks element     xpath://*[@id="create-footnote-form"]//label[text()="${label}"]/../input

user checks footnote checkbox is selected
    [Arguments]  ${label}
    wait until element is enabled   xpath://*[@id="create-footnote-form"]//label[contains(text(), "${label}")]/../input
    checkbox should be selected     xpath://*[@id="create-footnote-form"]//label[contains(text(), "${label}")]/../input

user adds data block to editable accordion section
    [Arguments]   ${accordion_name}   ${block_name}
    user opens accordion section  ${accordion_name}
    ${accordion_section}=  user gets accordion content element  ${accordion_name}
    ${add_block_button}=   get child element  ${accordion_section}  xpath:.//button[text()="Add data block"]
    user clicks element   ${add_block_button}
    ${block_list}=  get child element  ${accordion_section}  css:select[name="selectedDataBlock"]
    user selects from list by label  ${block_list}  Dates data block name
    user waits until parent contains element  ${accordion_section}   css:table
    ${embed_button}=   get child element   ${accordion_section}   xpath:.//button[text()="Embed"]
    user clicks element  ${embed_button}
