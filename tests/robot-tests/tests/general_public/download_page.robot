*** Settings ***
Resource            ../libs/public-common.robot

Suite Setup         user opens the browser
Suite Teardown      user closes the browser

Force Tags          GeneralPublic    Local    Dev    Test    Preprod    Failing

*** Test Cases ***
Navigate to /download-data page
    [Tags]    HappyPath
    environment variable should be set    PUBLIC_URL
    user goes to url    %{PUBLIC_URL}
    user waits until h1 is visible    Choose how to explore our statistics and data
    user clicks link    Download latest data files
    user waits until h1 is visible    Download latest data files
    user checks url contains    %{PUBLIC_URL}/download-latest-data

Validate Pupils and schools contains Pupil absence files
    [Documentation]    EES-562
    [Tags]    HappyPath
    user waits until page contains accordion section    Pupils and schools
    user waits for page to finish loading
    user opens accordion section    Pupils and schools

    user waits until accordion section contains text    Pupils and schools    Pupil absence

Validate Pupil absence data downloads are available
    [Documentation]    EES-562
    [Tags]    HappyPath    NotAgainstLocal
    environment variable should be set    CONTENT_API_URL

    user opens details dropdown    Pupil absence
    user waits until details contains link    Pupil absence    Absence by characteristic
    user checks page contains link with text and url    Absence by characteristic
    ...    %{CONTENT_API_URL}/download/pupil-absence-in-schools-in-england/2016-17/data/absence_by_characteristic.csv
    user waits until details contains link    Pupil absence    Absence by geographic level
    user checks page contains link with text and url    Absence by geographic level
    ...    %{CONTENT_API_URL}/download/pupil-absence-in-schools-in-england/2016-17/data/absence_by_geographic_level.csv
    user waits until details contains link    Pupil absence    Absence by term
    user checks page contains link with text and url    Absence by term
    ...    %{CONTENT_API_URL}/download/pupil-absence-in-schools-in-england/2016-17/data/absence_by_term.csv
    user waits until details contains link    Pupil absence    Absence for four year olds
    user checks page contains link with text and url    Absence for four year olds
    ...    %{CONTENT_API_URL}/download/pupil-absence-in-schools-in-england/2016-17/data/absence_for_four_year_olds.csv
    user waits until details contains link    Pupil absence    Absence in prus
    user checks page contains link with text and url    Absence in prus
    ...    %{CONTENT_API_URL}/download/pupil-absence-in-schools-in-england/2016-17/data/absence_in_prus.csv
    user waits until details contains link    Pupil absence    Absence number missing at least one session by reason
    user checks page contains link with text and url    Absence number missing at least one session by reason
    ...    %{CONTENT_API_URL}/download/pupil-absence-in-schools-in-england/2016-17/data/absence_number_missing_at_least_one_session_by_reason.csv
    user waits until details contains link    Pupil absence    Absence rate percent bands
    user checks page contains link with text and url    Absence rate percent bands
    ...    %{CONTENT_API_URL}/download/pupil-absence-in-schools-in-england/2016-17/data/absence_rate_percent_bands.csv
    user waits until details contains link    Pupil absence    Download all data and files for this release
    user checks page contains link with text and url    Download all data and files for this release
    ...    %{CONTENT_API_URL}/download/pupil-absence-in-schools-in-england/2016-17/ancillary/pupil-absence-in-schools-in-england_2016-17.zip

Download Absence in prus CSV
    [Tags]    HappyPath    NotAgainstLocal
    download file    xpath://ul[@data-testid="Pupil absence in schools in England"]//a[text()="Absence in prus"]
    ...    absence_in_prus.csv
    downloaded file should have first line    absence_in_prus.csv
    ...    time_identifier,time_period,geographic_level,country_code,country_name,region_code,region_name,old_la_code,new_la_code,la_name,school_type,num_schools,enrolments,sess_possible,sess_overall,sess_authorised,sess_unauthorised,sess_overall_percent,sess_authorised_percent,sess_unauthorised_percent,enrolments_pa_10_exact,enrolments_pa_10_exact_percent,sess_auth_illness,sess_auth_appointments,sess_auth_religious,sess_auth_study,sess_auth_traveller,sess_auth_holiday,sess_auth_ext_holiday,sess_auth_excluded,sess_auth_other,sess_auth_totalreasons,sess_unauth_holiday,sess_unauth_late,sess_unauth_other,sess_unauth_noyet,sess_unauth_totalreasons,sess_overall_totalreasons

Download All files ZIP
    [Tags]    HappyPath    NotAgainstLocal
    download file
    ...    xpath://ul[@data-testid="Pupil absence in schools in England"]//a[text()="Download all data and files for this release"]
    ...    pupil-absence-in-schools-in-england_2016-17.zip
    zip should contain file    pupil-absence-in-schools-in-england_2016-17.zip    absence_by_characteristic.csv
    zip should contain file    pupil-absence-in-schools-in-england_2016-17.zip    absence_by_geographic_level.csv
    zip should contain file    pupil-absence-in-schools-in-england_2016-17.zip    absence_by_term.csv
    zip should contain file    pupil-absence-in-schools-in-england_2016-17.zip    absence_for_four_year_olds.csv
    zip should contain file    pupil-absence-in-schools-in-england_2016-17.zip    absence_in_prus.csv
    zip should contain file    pupil-absence-in-schools-in-england_2016-17.zip
    ...    absence_number_missing_at_least_one_session_by_reason.csv
    zip should contain file    pupil-absence-in-schools-in-england_2016-17.zip    absence_rate_percent_bands.csv
    zip should contains x files    pupil-absence-in-schools-in-england_2016-17.zip    7

    user closes details dropdown    Pupil absence

Validate Pupil and schools contains Exclusions files
    [Tags]    HappyPath    NotAgainstLocal
    user waits until accordion section contains text    Pupils and schools    Exclusions

Validate Exclusions data downloads are available
    [Documentation]    EES-562
    [Tags]    HappyPath    NotAgainstLocal
    user opens details dropdown    Exclusions
    user waits until details contains link    Exclusions    Duration of fixed exclusions
    user checks page contains link with text and url    Duration of fixed exclusions
    ...    %{CONTENT_API_URL}/download/permanent-and-fixed-period-exclusions-in-england/2016-17/data/exclusions_duration_of_fixed_exclusions.csv
    user waits until details contains link    Exclusions    Exclusions by characteristic
    user checks page contains link with text and url    Exclusions by characteristic
    ...    %{CONTENT_API_URL}/download/permanent-and-fixed-period-exclusions-in-england/2016-17/data/exclusions_by_characteristic.csv
    user waits until details contains link    Exclusions    Exclusions by geographic level
    user checks page contains link with text and url    Exclusions by geographic level
    ...    %{CONTENT_API_URL}/download/permanent-and-fixed-period-exclusions-in-england/2016-17/data/exclusions_by_geographic_level.csv
    user waits until details contains link    Exclusions    Exclusions by reason
    user checks page contains link with text and url    Exclusions by reason
    ...    %{CONTENT_API_URL}/download/permanent-and-fixed-period-exclusions-in-england/2016-17/data/exclusions_by_reason.csv
    user waits until details contains link    Exclusions    Number of fixed exclusions
    user checks page contains link with text and url    Number of fixed exclusions
    ...    %{CONTENT_API_URL}/download/permanent-and-fixed-period-exclusions-in-england/2016-17/data/exclusions_number_of_fixed_exclusions.csv
    user waits until details contains link    Exclusions    Total days missed due to fixed period exclusions
    user checks page contains link with text and url    Total days missed due to fixed period exclusions
    ...    %{CONTENT_API_URL}/download/permanent-and-fixed-period-exclusions-in-england/2016-17/data/exclusions_total_days_missed_fixed_exclusions.csv
    user waits until details contains link    Exclusions    Download all data and files for this release
    user checks page contains link with text and url    Download all data and files for this release
    ...    %{CONTENT_API_URL}/download/permanent-and-fixed-period-exclusions-in-england/2016-17/ancillary/permanent-and-fixed-period-exclusions-in-england_2016-17.zip
    user closes details dropdown    Exclusions
