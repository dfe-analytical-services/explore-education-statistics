*** Settings ***
Resource    ../libs/public-common.robot

Force Tags  GeneralPublic  Local  Dev  Test  Preprod

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Navigate to publication release page
    [Tags]  HappyPath
    environment variable should be set  PUBLIC_URL
    user goes to url  %{PUBLIC_URL}/find-statistics/pupil-absence-in-schools-in-england

Click fast track link for 'Pupil absence rates' data block
    [Tags]  HappyPath
    user waits until h1 is visible  Pupil absence in schools in England
    user opens accordion section  Pupil absence rates
    user scrolls to accordion section content  Pupil absence rates
    user waits until h3 is visible  Explore and edit this data online
    user clicks link  Explore data

Validate selected step options
    [Tags]  HappyPath
    user waits until h1 is visible  Create your own tables online
    user waits until page contains element    css:table

    user checks previous table tool step contains  1    Publication     Pupil absence in schools in England

    user checks previous table tool step contains  2    Subject         Absence by characteristic

    user checks previous table tool step contains  3    National        England

    user checks previous table tool step contains  4    Start date      2012/13
    user checks previous table tool step contains  4    End date        2016/17

    user checks previous table tool step contains  5    Indicators      Authorised absence rate
    user checks previous table tool step contains  5    Indicators      Overall absence rate
    user checks previous table tool step contains  5    Indicators      Unauthorised absence rate
    user checks previous table tool step contains  5    School type     Total
    user checks previous table tool step contains  5    Characteristic  Total

Validate table data
    [Tags]  HappyPath
    user checks table column heading contains  css:table  1   1   2012/13
    user checks table column heading contains  css:table  1   2   2013/14
    user checks table column heading contains  css:table  1   3   2014/15
    user checks table column heading contains  css:table  1   4   2015/16
    user checks table column heading contains  css:table  1   5   2016/17

    user checks results table row heading contains  1    1      England
    user checks results table row heading contains  1    2      Authorised absence rate
    user checks results table row heading contains  2    1      Unauthorised absence rate
    user checks results table row heading contains  3    1      Overall absence rate

    # Authorised absence rate
    user checks results table cell contains  1    1     4.2%
    user checks results table cell contains  1    2     3.5%
    user checks results table cell contains  1    3     3.5%
    user checks results table cell contains  1    4     3.4%
    user checks results table cell contains  1    5     3.4%

    # Unauthorised absence rate
    user checks results table cell contains  2    1     1.1%
    user checks results table cell contains  2    2     1.1%
    user checks results table cell contains  2    3     1.1%
    user checks results table cell contains  2    4     1.1%
    user checks results table cell contains  2    5     1.3%

    # Overall absence rate
    user checks results table cell contains  3    1     5.3%
    user checks results table cell contains  3    2     4.5%
    user checks results table cell contains  3    3     4.6%
    user checks results table cell contains  3    4     4.6%
    user checks results table cell contains  3    5     4.7%
