*** Settings ***
Resource            ../../libs/public-common.robot

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          GeneralPublic    Prod


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
    user checks radio is checked    All themes
    user checks page contains radio    Children's social care
    user checks page contains radio    COVID-19
    user checks page contains radio    Destination of pupils and students
    user checks page contains radio    Early years
    user checks page contains radio    Finance and funding
    user checks page contains radio    Further education
    user checks page contains radio    Higher education
    user checks page contains radio    Pupils and schools
    user checks page contains radio    School and college outcomes and performance
    user checks page contains radio    Teachers and school workforce
    user checks page contains radio    UK education and training statistics

Validate release type filters exist
    user clicks button    Release type
    user checks radio is checked    Show all
    user checks page contains radio    Accredited official statistics
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
    user clicks radio    Pupils and schools
    user checks page contains button    Pupils and schools

Remove theme filter
    user clicks button    Pupils and schools
    user checks page does not contain button    Pupils and schools
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
    user presses keys    pupil absence
    user clicks button    Search
    user checks page contains button    pupil absence
    user checks list item contains    testid:publicationsList    1    ${PUPIL_ABSENCE_PUBLICATION_TITLE}

Removing search
    user clicks button    pupil absence
    user checks page does not contain button    ${PUPIL_ABSENCE_PUBLICATION_TITLE}

Clear all filters
    user clicks element    id:searchForm-search
    user presses keys    pupil
    user clicks button    Search
    user clicks radio    Pupils and schools
    user clicks radio    Official statistics

    user checks page contains button    pupil
    user checks page contains button    Pupils and schools
    user checks page contains button    Official statistics

    user clicks button    Clear all filters

    user checks page does not contain button    pupil
    user checks page does not contain button    Pupils and schools
    user checks page does not contain button    Official statistics
    user checks page does not contain button    Clear all filters

    user checks radio is checked    All themes
    user checks radio is checked    Show all
