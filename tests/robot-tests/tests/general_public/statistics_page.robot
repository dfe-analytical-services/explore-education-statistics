*** Settings ***
Resource            ../libs/public-common.robot
Resource            ../seed_data/seed_data_theme_1_constants.robot
Resource            ../seed_data/seed_data_theme_2_constants.robot

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          GeneralPublic    Local    Dev    PreProd


*** Test Cases ***
Navigate to Find Statistics page
    environment variable should be set    PUBLIC_URL
    user navigates to public find statistics page

Validate Related information section and links exist
    ${related_information}=    get webelement    css:[aria-labelledby="related-information"]

    user checks element contains child element    ${related_information}    xpath://h2[text()="Related information"]

    user checks page contains link with text and url
    ...    Education statistics: data catalogue
    ...    /data-catalogue
    ...    ${related_information}
    user checks page contains link with text and url
    ...    Education statistics: methodology
    ...    /methodology
    ...    ${related_information}
    user checks page contains link with text and url
    ...    Education statistics: glossary
    ...    /glossary
    ...    ${related_information}

Validate themes filters exist
    user checks radio is checked    All themes
    user checks page contains radio    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user checks page contains radio    ${ROLE_PERMISSIONS_THEME_TITLE}

Validate release type filters exist
    user clicks button    Release type
    user checks radio is checked    Show all
    user checks page contains radio    National statistics
    user checks page contains radio    Official statistics
    user checks page contains radio    Official statistics in development
    user checks page contains radio    Experimental statistics
    user checks page contains radio    Ad hoc statistics
    user checks page contains radio    Management information

Validate sort controls exist
    user checks radio is checked    Newest
    user checks page contains radio    Oldest
    user checks page contains radio    A to Z

Validate publications list exists
    user checks page contains element    testid:publicationsList

Filter by theme
    user clicks radio    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user checks page contains button    ${PUPILS_AND_SCHOOLS_THEME_TITLE}

Remove theme filter
    user clicks button    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user checks page does not contain button    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user checks radio is checked    All themes

Filter by release type
    user clicks radio    Official statistics
    user checks page contains button    Official statistics

Remove release type filter
    user clicks button    Official statistics
    user checks page does not contain button    Official statistics
    user checks radio is checked    Show all

Searching
    user clicks element    id:searchForm-search
    user presses keys    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user clicks button    Search
    user checks page contains button    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user checks radio is checked    Relevance
    user checks list item contains    testid:publicationsList    1    ${PUPIL_ABSENCE_PUBLICATION_TITLE}

Removing search
    user clicks button    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user checks page does not contain button    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user checks radio is checked    Newest

Clear all filters
    user clicks element    id:searchForm-search
    user presses keys    pupil
    user clicks button    Search
    user clicks radio    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user clicks radio    Official statistics

    user checks page contains button    pupil
    user checks page contains button    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user checks page contains button    Official statistics

    user clicks button    Clear all filters

    user checks page does not contain button    pupil
    user checks page does not contain button    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user checks page does not contain button    Official statistics
    user checks page does not contain button    Clear all filters

    user checks radio is checked    All themes
    user checks radio is checked    Show all
