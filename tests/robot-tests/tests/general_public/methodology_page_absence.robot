*** Settings ***
Resource            ../libs/public-common.robot
Resource            ../seed_data/seed_data_theme_1_constants.robot

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          GeneralPublic    Local    Dev    PreProd


*** Test Cases ***
Navigate to Seed Data Theme 1 methodologies
    user navigates to public methodologies page
    user opens accordion section    ${PUPILS_AND_SCHOOLS_THEME_TITLE}

Go to Seed Data Theme 1 Publication 1 Methodology 1
    user checks page contains link with text and url
    ...    ${PUPIL_ABSENCE_METHODOLOGY_TITLE}
    ...    ${PUPIL_ABSENCE_METHODOLOGY_RELATIVE_URL}
    user clicks link    ${PUPIL_ABSENCE_METHODOLOGY_TITLE}
    user waits until h1 is visible    ${PUPIL_ABSENCE_METHODOLOGY_TITLE}
    user waits until page contains title caption    Methodology

Validate Published date
    user checks summary list contains    Published    22 March 2018

Validate Last updated is not visible
    user waits until page does not contain testid    Last updated

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
    ${related_information}=    get webelement    css:[aria-labelledby="related-information"]

    user checks element contains child element    ${related_information}    xpath://h2[text()="Related information"]

    user checks element contains child element    ${related_information}    xpath://h3[text()="Publications"]
    user checks page contains link with text and url
    ...    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    ...    ${PUPIL_ABSENCE_PUBLICATION_RELATIVE_URL}
    ...    ${related_information}

    user checks element contains child element    ${related_information}    xpath://h3[text()="Related pages"]
    user checks page contains link with text and url
    ...    Find statistics and data
    ...    /find-statistics
    ...    ${related_information}
    user checks page contains link with text and url
    ...    Education statistics: glossary
    ...    /glossary
    ...    ${related_information}

Validate page has Print this page link
    user waits until page contains button    Print this page

Search for "pupil"
    [Documentation]    EES-807

    # This is the number of text matches found when using the in-browser javascript search.
    # This number can vary depending on the data itself as it will scan all the html on the page.
    # This value of "5" matches:
    #    1. A text hit in an h3 in content section 1.
    #    1. A text hit in the paragraph content of content section 1.
    #    3. A text hit in the content of annex section 1.
    #    4. A text hit in the section title of annex section 3.
    #    5. A mention of the publication title in the "Contact us" section.
    ${expected_occurrences_of_pupil}=    Set Variable    5

    user verifies accordion is closed    1. Overview of absence statistics

    user enters text into element    id:pageSearchForm-input    pupil
    user waits until element contains    id:pageSearchForm-resultsLabel
    ...    Found ${expected_occurrences_of_pupil} results
    user clicks element    id:pageSearchForm-option-0

    user verifies accordion is open    1. Overview of absence statistics
    user waits until element is visible    id:content-section-0-content-1
    user waits until page contains    The data used to publish absence statistics is collected via the school census
    user waits until element is visible    id:content-section-0-content-2
    user waits until page contains    All maintained schools are required to provide 2 possible sessions per day

Search for "specific enquiry"
    [Documentation]    EES-807

    # The words "specific inquiry" occur once in the Contact Us Section,
    # and once in some custom text the user has uploaded to the "7. Contacts"
    # section.
    ${expected_occurrences_of_specific_inquiry}=    Set Variable    2

    user verifies accordion is closed    7. Contacts

    user clears element text    id:pageSearchForm-input
    user enters text into element    id:pageSearchForm-input    specific enquiry
    user waits until element contains    id:pageSearchForm-resultsLabel
    ...    Found ${expected_occurrences_of_specific_inquiry} results
    user clicks element    id:pageSearchForm-option-0

    user verifies accordion is open    7. Contacts
    user checks page contains    If you have a specific enquiry about absence and exclusion statistics and
    user checks element is visible    xpath://h4[contains(text(),"School absence and exclusions team")]
