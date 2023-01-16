*** Settings ***
Resource            ../libs/public-common.robot

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          GeneralPublic    Prod


*** Test Cases ***
Navigate to Find Statistics page
    [Tags]    Local    Dev
    environment variable should be set    PUBLIC_URL
    user navigates to find statistics page on public frontend

Validate Related information section and links exist
    ${relatedInformation}=    get webelement    css:[aria-labelledby="related-information"]

    user checks element contains child element    ${relatedInformation}    xpath://h2[text()="Related information"]

    user checks page contains link with text and url
    ...    Education statistics: data catalogue
    ...    /data-catalogue
    ...    ${relatedInformation}
    user checks page contains link with text and url
    ...    Education statistics: methodology
    ...    /methodology
    ...    ${relatedInformation}
    user checks page contains link with text and url
    ...    Education statistics: glossary
    ...    /glossary
    ...    ${relatedInformation}

Validate bootstrapped themes filters exist
    [Tags]    Local    Dev    NotAgainstProd
    user checks radio is checked    All themes
    user checks page contains radio    Pupils and schools
    user checks page contains radio    UI tests - Publication and Release UI Permissions Theme

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
    [Tags]    Local    Dev
    user clicks button    Release type
    user checks radio is checked    Show all
    user checks page contains radio    National statistics
    user checks page contains radio    Official statistics
    user checks page contains radio    Experimental statistics
    user checks page contains radio    Ad hoc statistics
    user checks page contains radio    Management information

Validate sort controls exist
    [Tags]    Local    Dev
    user checks radio is checked    Newest
    user checks page contains radio    Oldest
    user checks page contains radio    A to Z

Validate publications list exists
    [Tags]    Local    Dev
    user checks page contains element    testid:publicationsList

Filter by theme
    [Tags]    Local    Dev
    user clicks radio    Pupils and schools
    user checks page contains button    Pupils and schools

Remove theme filter
    [Tags]    Local    Dev
    user clicks button    Pupils and schools
    user checks page does not contain button    Pupils and schools
    user checks radio is checked    All themes

Filter by release type
    [Tags]    Local    Dev
    user clicks radio    Official statistics
    user checks page contains button    Official statistics

Remove release type filter
    [Tags]    Local    Dev
    user clicks button    Official statistics
    user checks page does not contain button    Official statistics
    user checks radio is checked    Show all

Searching
    [Tags]    Local    Dev
    user clicks element    id:searchTerm
    user presses keys    Pupil absence in schools in England
    user clicks button    Search
    user checks page contains button    Pupil absence in schools in England
    user checks radio is checked    Relevance
    user checks list item contains    testid:publicationsList    1    Pupil absence in schools in England

Removing search
    [Tags]    Local    Dev
    user clicks button    Pupil absence in schools in England
    user checks page does not contain button    Pupil absence in schools in England
    user checks radio is checked    Newest

Clear all filters
    [Tags]    Local    Dev
    user clicks element    id:searchTerm
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
