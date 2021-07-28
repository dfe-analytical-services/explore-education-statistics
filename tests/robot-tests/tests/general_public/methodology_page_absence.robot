*** Settings ***
Resource            ../libs/public-common.robot

Suite Setup         user opens the browser
Suite Teardown      user closes the browser

Force Tags          GeneralPublic    Local    Dev    Test    Preprod

*** Test Cases ***
Navigate to Pupil absence in schools in England methodology page
    [Tags]    HappyPath  Failing  NotAgainstDev
    environment variable should be set    PUBLIC_URL
    user goes to url    %{PUBLIC_URL}/methodology
    user waits until h1 is visible    Methodologies
    user waits for page to finish loading

    user opens accordion section    Pupils and schools
    user opens details dropdown    Pupil absence

User navigates to absence methodology page
    [Tags]    HappyPath  Failing  NotAgainstDev
    user checks page contains methodology link
    ...    Pupil absence
    ...    Pupil absence in schools in England
    ...    Pupil absence statistics: methodology
    ...    %{PUBLIC_URL}/methodology/pupil-absence-in-schools-in-england
    user clicks methodology link
    ...    Pupil absence
    ...    Pupil absence in schools in England
    ...    Pupil absence statistics: methodology
    user waits until h1 is visible    Pupil absence statistics: methodology

Validate Published date, Last updated date
    [Tags]    HappyPath  Failing  NotAgainstDev
    user checks testid element contains    published-date    22 March 2018

Validate accordion sections order
    [Tags]    HappyPath  Failing  NotAgainstDev
    user checks accordion is in position    1. Overview of absence statistics    1    id:content
    user checks accordion is in position    2. National Statistics badging    2    id:content
    user checks accordion is in position    3. Methodology    3    id:content
    user checks accordion is in position    4. Data collection    4    id:content
    user checks accordion is in position    5. Data processing    5    id:content
    user checks accordion is in position    6. Data quality    6    id:content
    user checks accordion is in position    7. Contacts    7    id:content

    user checks accordion is in position    Annex A - Calculations    1    id:annexes
    user checks accordion is in position    Annex B - School attendance codes    2    id:annexes
    user checks accordion is in position    Annex C - Links to pupil absence national statistics and data    3
    ...    id:annexes
    user checks accordion is in position    Annex D - Standard breakdowns    4    id:annexes
    user checks accordion is in position    Annex E - Timeline    5    id:annexes
    user checks accordion is in position    Annex F - Absence rates over time    6    id:annexes

Validate page has Print this page link
    [Tags]    HappyPath  Failing  NotAgainstDev
    user waits until page contains button    Print this page

Search for "pupil"
    [Documentation]    EES-807
    [Tags]    HappyPath  Failing  NotAgainstDev
    user verifies accordion is closed    1. Overview of absence statistics

    user clicks element    id:pageSearchForm-input
    user presses keys    pupil
    user waits until element contains    id:pageSearchForm-resultsLabel    Found 127 results
    user clicks element    id:pageSearchForm-option-0

    user verifies accordion is open    1. Overview of absence statistics
    user waits until element is visible    id:section1-1
    user waits until page contains    All maintained schools are required to provide 2 possible sessions per day,

Search for "specific enquiry"
    [Documentation]    EES-807
    [Tags]    HappyPath  Failing  NotAgainstDev
    user verifies accordion is closed    7. Contacts

    user clears element text    id:pageSearchForm-input
    user presses keys    specific enquiry    id:pageSearchForm-input
    user waits until element contains    id:pageSearchForm-resultsLabel    Found 1 result
    user clicks element    id:pageSearchForm-option-0

    user verifies accordion is open    7. Contacts
    user checks page contains    If you have a specific enquiry about absence and exclusion statistics and
    user checks element is visible    xpath://h4[contains(text(),"School absence and exclusions team")]
