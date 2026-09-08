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
    user checks table contains column heading    2016/17
    user checks table contains column heading    2015/16
    user checks table contains column heading    2014/15
    user checks table contains column heading    2013/14
    user checks table contains column heading    2012/13

    user checks cell by row and column heading contains    Authorised absence rate    2016/17    3.4%
    user checks cell by row and column heading contains    Authorised absence rate    2015/16    3.4%
    user checks cell by row and column heading contains    Authorised absence rate    2014/15    3.5%
    user checks cell by row and column heading contains    Authorised absence rate    2013/14    3.5%
    user checks cell by row and column heading contains    Authorised absence rate    2012/13    4.2%

    user checks cell by row and column heading contains    Overall absence rate    2016/17    4.7%
    user checks cell by row and column heading contains    Overall absence rate    2015/16    4.6%
    user checks cell by row and column heading contains    Overall absence rate    2014/15    4.6%
    user checks cell by row and column heading contains    Overall absence rate    2013/14    4.5%
    user checks cell by row and column heading contains    Overall absence rate    2012/13    5.3%

    user checks cell by row and column heading contains    Unauthorised absence rate    2016/17    1.3%
    user checks cell by row and column heading contains    Unauthorised absence rate    2015/16    1.1%
    user checks cell by row and column heading contains    Unauthorised absence rate    2014/15    1.1%
    user checks cell by row and column heading contains    Unauthorised absence rate    2013/14    1.1%
    user checks cell by row and column heading contains    Unauthorised absence rate    2012/13    1.1%
