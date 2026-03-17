*** Settings ***
Resource            ../libs/public-common.robot
Resource            ../seed_data/seed_data_theme_1_constants.robot

Force Tags          GeneralPublic    Local    Dev    Test    Preprod

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Test Cases ***
Navigate to publication release page
    environment variable should be set    PUBLIC_URL
    user navigates to    %{PUBLIC_URL}${PUPIL_ABSENCE_PUBLICATION_RELATIVE_URL}

Click fast track link for 'Pupil absence rates' data block
    user waits until h1 is visible    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user scrolls to element    id:section-pupil-absence-rates
    user waits until h3 is visible    Explore and edit this data online
    user clicks link containing text    Explore data    testid:Data block - Generic data block - National

Validate Publication selected step option
    user waits until h1 is visible    Create your own tables    %{WAIT_SMALL}
    user waits until page contains element    css:table
    user checks previous table tool step contains    1    Publication    ${PUPIL_ABSENCE_PUBLICATION_TITLE}

Validate Subject selected step option
    [Tags]    NotAgainstDev    NotAgainstTest
    user checks previous table tool step contains    2    Data set    Absence by characteristic

Validate other selected step options
    user checks previous table tool step contains    3    National    England
    user checks previous table tool step contains    4    Time period    2012/13 to 2016/17
    user checks previous table tool step contains    5    Indicators    Authorised absence rate
    user checks previous table tool step contains    5    Indicators    Overall absence rate
    user checks previous table tool step contains    5    Indicators    Unauthorised absence rate
    user checks previous table tool step contains    5    School type    Total
    user checks previous table tool step contains    5    Characteristic    Total

Validate table data
    user checks table column heading contains    1    1    2012/13
    user checks table column heading contains    1    2    2013/14
    user checks table column heading contains    1    3    2014/15
    user checks table column heading contains    1    4    2015/16
    user checks table column heading contains    1    5    2016/17

    user checks table row heading contains    1    1    Authorised absence rate
    user checks table row heading contains    2    1    Overall absence rate
    user checks table row heading contains    3    1    Unauthorised absence rate

    # Authorised absence rate
    user checks table cell contains    1    1    4.2%
    user checks table cell contains    1    2    3.5%
    user checks table cell contains    1    3    3.5%
    user checks table cell contains    1    4    3.4%
    user checks table cell contains    1    5    3.4%

    # Overall absence rate
    user checks table cell contains    2    1    5.3%
    user checks table cell contains    2    2    4.5%
    user checks table cell contains    2    3    4.6%
    user checks table cell contains    2    4    4.6%
    user checks table cell contains    2    5    4.7%

    # Unauthorised absence rate
    user checks table cell contains    3    1    1.1%
    user checks table cell contains    3    2    1.1%
    user checks table cell contains    3    3    1.1%
    user checks table cell contains    3    4    1.1%
    user checks table cell contains    3    5    1.3%
