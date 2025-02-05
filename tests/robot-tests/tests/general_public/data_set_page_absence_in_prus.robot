*** Settings ***
Resource            ../libs/public-common.robot
Resource            ../seed_data/seed_data_theme_1_constants.robot

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          GeneralPublic    Local    Dev    Preprod


*** Test Cases ***
Navigate to Absence in PRUs data set from data catalogue
    user navigates to data catalogue page on public frontend
    user chooses select option    id:filters-form-theme    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user clicks link    Absence in PRUs

Validate title
    user waits until h1 is visible    Absence in PRUs    %{WAIT_MEDIUM}
    user waits until page contains title caption    Data set from ${PUPIL_ABSENCE_PUBLICATION_TITLE}

Validate data set info
    user checks page contains    Latest data
    user checks page contains    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user checks page contains    ${PUPIL_ABSENCE_RELEASE_NAME}

Validate zip contains correct files
    [Documentation]    EES-4147
    user clicks button containing text    Download data set (ZIP)

    sleep    8    # wait for file to download
    ${list}=    create list    data/absence_in_prus.csv    data-guidance/data-guidance.txt
    zip should contain directories and files    seed-publication-pupil-absence-in-schools-in-england_2016-17.zip
    ...    ${list}

Validate data set details
    user checks summary list contains    Theme    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user checks summary list contains    Publication    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user checks summary list contains    Release    ${PUPIL_ABSENCE_RELEASE_NAME}
    user checks summary list contains    Release type    Official statistics
    user checks summary list contains    Geographic levels    Local authority, National, Regional
    user checks summary list contains    Indicators    Authorised absence rate
    user checks summary list contains    Indicators    Number of authorised absence sessions
    user checks summary list contains    Indicators    Number of authorised holiday sessions
    user checks summary list contains    Indicators    Show 24 more indicators
    user checks summary list contains    Time period    2013/14 to 2016/17

Validate data set preview
    user checks table column heading contains    1    1    time_identifier    testid:preview-table
    user checks table column heading contains    1    2    time_period    testid:preview-table
    user checks table column heading contains    1    3    geographic_level    testid:preview-table
    user checks table column heading contains    1    4    country_code    testid:preview-table

    user checks table body has x rows    5    testid:preview-table
    user checks table cell contains    1    1    Academic year    testid:preview-table
    user checks table cell contains    1    2    201617    testid:preview-table
    user checks table cell contains    1    3    National    testid:preview-table
    user checks table cell contains    1    4    E92000001    testid:preview-table

Validate data set variables
    user checks table column heading contains    1    1    Variable name    testid:variables-table
    user checks table column heading contains    1    2    Variable description    testid:variables-table

    user checks table body has x rows    5    testid:variables-table
    user checks table cell contains    1    1    enrolments    testid:variables-table
    user checks table cell contains    1    2    Number of pupil enrolments    testid:variables-table

    user checks table cell contains    2    1    enrolments_pa_10_exact    testid:variables-table
    user checks table cell contains    2    2    Number of persistent absentees    testid:variables-table

    user checks table cell contains    3    1    enrolments_pa_10_exact_percent    testid:variables-table
    user checks table cell contains    3    2    Percentage of persistent absentees    testid:variables-table

    user checks table cell contains    4    1    num_schools    testid:variables-table
    user checks table cell contains    4    2    Number of schools    testid:variables-table

    user checks table cell contains    5    1    sess_auth_appointments    testid:variables-table
    user checks table cell contains    5    2    Number of medical appointments sessions    testid:variables-table

    user checks page contains button    Show all 27 variables

Validate using this data
    user checks page contains button    Download this data set (ZIP)
    user checks page contains link    View or create your own tables

Validate table tool link
    user clicks link    View or create your own tables
    user waits until h1 is visible    Create your own tables

    user checks page contains    Choose locations
    user checks previous table tool step contains    1    Publication    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user checks previous table tool step contains    2    Data set    Absence in PRUs
