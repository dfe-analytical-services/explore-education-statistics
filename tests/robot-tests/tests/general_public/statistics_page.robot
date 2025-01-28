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
    ...    Data catalogue
    ...    /data-catalogue
    ...    ${related_information}
    user checks page contains link with text and url
    ...    Methodology
    ...    /methodology
    ...    ${related_information}
    user checks page contains link with text and url
    ...    Glossary
    ...    /glossary
    ...    ${related_information}

Validate themes filters exist
    user checks select contains option    id:filters-form-theme    All themes
    user checks selected option label    id:filters-form-theme    All themes
    user checks select contains option    id:filters-form-theme    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user checks select contains option    id:filters-form-theme    ${ROLE_PERMISSIONS_THEME_TITLE}

Validate release type filters exist
    user checks select contains option    id:filters-form-release-type    All release types
    user checks selected option label    id:filters-form-release-type    All release types
    user checks select contains option    id:filters-form-release-type    Accredited official statistics
    user checks select contains option    id:filters-form-release-type    Official statistics
    user checks select contains option    id:filters-form-release-type    Official statistics in development
    user checks select contains option    id:filters-form-release-type    Experimental statistics
    user checks select contains option    id:filters-form-release-type    Ad hoc statistics
    user checks select contains option    id:filters-form-release-type    Management information

Validate sort controls exist
    user checks radio is checked    Newest
    user checks page contains radio    Oldest
    user checks page contains radio    A to Z

Validate publications list exists
    user checks page contains element    testid:publicationsList

Filter by theme
    user chooses select option    id:filters-form-theme    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user checks page contains button    ${PUPILS_AND_SCHOOLS_THEME_TITLE}

Remove theme filter
    user clicks button    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user checks page does not contain button    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user checks selected option label    id:filters-form-theme    All themes

Filter by release type
    user chooses select option    id:filters-form-release-type    Official statistics
    user checks page contains button    Official statistics

Remove release type filter
    user clicks button    Official statistics
    user checks page does not contain button    Official statistics
    user checks selected option label    id:filters-form-release-type    All release types

Searching
    # filter by theme first to make sure we get the seed publication on dev.
    user chooses select option    id:filters-form-theme    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user checks page contains button    ${PUPILS_AND_SCHOOLS_THEME_TITLE}

    user clicks element    id:searchForm-search
    user presses keys    pupil absence
    user clicks button    Search
    user checks page contains button    pupil absence
    user checks list item contains    testid:publicationsList    1    ${PUPIL_ABSENCE_PUBLICATION_TITLE}

Removing search
    user clicks button    pupil absence
    user checks page does not contain button    ${PUPIL_ABSENCE_PUBLICATION_TITLE}

Reset all filters
    user clicks element    id:searchForm-search
    user presses keys    pupil
    user clicks button    Search
    user chooses select option    id:filters-form-theme    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user chooses select option    id:filters-form-release-type    Official statistics

    user checks page contains button    pupil
    user checks page contains button    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user checks page contains button    Official statistics

    user clicks button    Reset filters

    user checks page does not contain button    pupil
    user checks page does not contain button    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user checks page does not contain button    Official statistics
    user checks page does not contain button    Reset filters

    user checks selected option label    id:filters-form-theme    All themes
    user checks selected option label    id:filters-form-release-type    All release types
