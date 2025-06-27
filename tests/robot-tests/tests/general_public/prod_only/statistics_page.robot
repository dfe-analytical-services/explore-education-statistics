*** Settings ***
Resource            ../../libs/public-common.robot

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          GeneralPublic    Prod

# TODO EES-6063 - Remove this test suite


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
    user checks select contains option    id:filters-form-theme    Children's social care
    user checks select contains option    id:filters-form-theme    COVID-19
    user checks select contains option    id:filters-form-theme    Destination of pupils and students
    user checks select contains option    id:filters-form-theme    Early years
    user checks select contains option    id:filters-form-theme    Finance and funding
    user checks select contains option    id:filters-form-theme    Further education
    user checks select contains option    id:filters-form-theme    Higher education
    user checks select contains option    id:filters-form-theme    Pupils and schools
    user checks select contains option    id:filters-form-theme    School and college outcomes and performance
    user checks select contains option    id:filters-form-theme    Teachers and school workforce
    user checks select contains option    id:filters-form-theme    UK education and training statistics

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
    user chooses select option    id:filters-form-theme    Pupils and schools
    user checks page contains button    Pupils and schools

Remove theme filter
    user clicks button    Pupils and schools
    user checks page does not contain button    Pupils and schools
    user checks selected option label    id:filters-form-theme    All themes

Filter by release type
    user chooses select option    id:filters-form-release-type    Official statistics
    user checks page contains button    Official statistics

Remove release type filter
    user clicks button    Official statistics
    user checks page does not contain button    Official statistics
    user checks selected option label    id:filters-form-release-type    All release types

Searching
    user clicks element    id:searchForm-search
    user presses keys    pupil absence
    user clicks button    Search
    user checks page contains button    pupil absence
    user clicks radio    Relevance
    user waits until page finishes loading
    user checks page contains button    pupil absence
    user checks list item contains    testid:publicationsList    1    Pupil absence in schools in England

Removing search
    user clicks button    pupil absence
    user checks page does not contain button    pupil absence

Reset all filters
    user clicks element    id:searchForm-search
    user presses keys    pupil
    user clicks button    Search
    user chooses select option    id:filters-form-theme    Pupils and schools
    user chooses select option    id:filters-form-release-type    Official statistics

    user checks page contains button    pupil
    user checks page contains button    Pupils and schools
    user checks page contains button    Official statistics

    user clicks button    Reset filters

    user checks page does not contain button    pupil
    user checks page does not contain button    Pupils and schools
    user checks page does not contain button    Official statistics
    user checks page does not contain button    Reset filters

    user checks selected option label    id:filters-form-theme    All themes
    user checks selected option label    id:filters-form-release-type    All release types
