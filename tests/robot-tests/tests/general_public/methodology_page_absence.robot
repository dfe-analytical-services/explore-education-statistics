*** Settings ***
Resource            ../libs/public-common.robot

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          GeneralPublic    Local    Dev


*** Test Cases ***
Navigate to Pupil absence in schools in England methodology page
    user navigates to public methodologies page
    user opens accordion section    Pupils and schools
    user opens details dropdown    Pupil absence

Go to Pupil absence methodology page
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
    user waits until page contains title caption    Methodology

Validate Published date
    user checks summary list contains    Published    22 March 2018

Validate Last updated is not visible
    user checks page does not contain testid    Last updated

Validate accordion sections order
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

Validate Related information section and links exist
    ${relatedInformation}=    get webelement    css:[aria-labelledby="related-information"]

    user checks element contains child element    ${relatedInformation}    xpath://h2[text()="Related information"]

    user checks element contains child element    ${relatedInformation}    xpath://h3[text()="Publications"]
    user checks page contains link with text and url
    ...    Pupil absence in schools in England
    ...    /find-statistics/pupil-absence-in-schools-in-england
    ...    ${relatedInformation}

    user checks element contains child element    ${relatedInformation}    xpath://h3[text()="Related pages"]
    user checks page contains link with text and url
    ...    Find statistics and data
    ...    /find-statistics
    ...    ${relatedInformation}
    user checks page contains link with text and url
    ...    Education statistics: glossary
    ...    /glossary
    ...    ${relatedInformation}

Validate page has Print this page link
    user waits until page contains button    Print this page

Search for "pupil"
    [Documentation]    EES-807
    user verifies accordion is closed    1. Overview of absence statistics

    user enters text into element    id:pageSearchForm-input    pupil
    user waits until element contains    id:pageSearchForm-resultsLabel    Found 127 results
    user clicks element    id:pageSearchForm-option-0

    user verifies accordion is open    1. Overview of absence statistics
    user waits until element is visible    id:section1-1
    user waits until page contains    All maintained schools are required to provide 2 possible sessions per day,

Search for "specific enquiry"
    [Documentation]    EES-807
    user verifies accordion is closed    7. Contacts

    user clears element text    id:pageSearchForm-input
    user enters text into element    id:pageSearchForm-input    specific enquiry
    user waits until element contains    id:pageSearchForm-resultsLabel    Found 1 result
    user clicks element    id:pageSearchForm-option-0

    user verifies accordion is open    7. Contacts
    user checks page contains    If you have a specific enquiry about absence and exclusion statistics and
    user checks element is visible    xpath://h4[contains(text(),"School absence and exclusions team")]
