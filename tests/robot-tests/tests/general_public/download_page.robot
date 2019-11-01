*** Settings ***
Resource    ../libs/common.robot

Force Tags  GeneralPublic  Dev  Test

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Navigate to /download-data page
    [Tags]  HappyPath
    environment variable should be set   PUBLIC_URL
    user goes to url   %{PUBLIC_URL}
    user waits until page contains heading   Choose how to explore our statistics and data
    user clicks link   Download data files
    user waits until page contains heading   Download data files
    user checks url contains   %{PUBLIC_URL}/download-data

Validate Pupils and schools contains correct details components
    [Tags]  HappyPath
    user checks page contains accordion   Pupils and schools
    user opens accordion section   Pupils and schools

    user checks accordion section contains text  Pupils and schools   Pupil absence
    user checks accordion section contains text  Pupils and schools   School applications
    user checks accordion section contains text  Pupils and schools   Exclusions

Validate Pupil absence data downloads are available
    [Documentation]  EES-562
    [Tags]  HappyPath   NotAgainstLocal   Failing
    environment variable should be set   DATA_API_URL

    user opens details dropdown  Pupil absence
    user checks details dropdown contains download link  Pupil absence  Absence by characteristic, 2016/17
    user checks page contains link with text and url  Absence by characteristic, 2016/17   %{DATA_API_URL}/download/pupil-absence-in-schools-in-england/2016-17/data/absence_by_characteristic.csv
    user checks details dropdown contains download link  Pupil absence  Absence by geographic level, 2016/17
    user checks page contains link with text and url  Absence by geographic level, 2016/17   %{DATA_API_URL}/download/pupil-absence-in-schools-in-england/2016-17/data/absence_by_geographic_level.csv
    user checks details dropdown contains download link  Pupil absence  Absence by term, 2016/17
    user checks page contains link with text and url  Absence by term, 2016/17   %{DATA_API_URL}/download/pupil-absence-in-schools-in-england/2016-17/data/absence_by_term.csv
    user checks details dropdown contains download link  Pupil absence  Absence for four year olds, 2016/17
    user checks page contains link with text and url  Absence for four year olds, 2016/17   %{DATA_API_URL}/download/pupil-absence-in-schools-in-england/2016-17/data/absence_for_four_year_olds.csv
    user checks details dropdown contains download link  Pupil absence  Absence in PRUs, 2016/17
    user checks page contains link with text and url  Absence in PRUs, 2016/17   %{DATA_API_URL}/download/pupil-absence-in-schools-in-england/2016-17/data/absence_in_prus.csv
    user checks details dropdown contains download link  Pupil absence  Absence metadata, 2016/17
    user checks page contains link with text and url  Absence metadata, 2016/17   %{DATA_API_URL}/download/pupil-absence-in-schools-in-england/2016-17/data/absence_metadata.docx
    user checks details dropdown contains download link  Pupil absence  Absence number missing at least one session by reason, 2016/17
    user checks page contains link with text and url  Absence number missing at least one session by reason, 2016/17   %{DATA_API_URL}/download/pupil-absence-in-schools-in-england/2016-17/data/absence_number_missing_at_least_one_session_by_reason.csv
    user checks details dropdown contains download link  Pupil absence  Absence rate percent bands, 2016/17
    user checks page contains link with text and url  Absence rate percent bands, 2016/17   %{DATA_API_URL}/download/pupil-absence-in-schools-in-england/2016-17/data/absence_rate_percent_bands.csv
    user closes details dropdown  Pupil absence

Validate School applications data downloads are available
    [Documentation]  EES-562
    [Tags]  HappyPath   NotAgainstLocal   Failing
    user opens details dropdown   School applications
    user checks details dropdown contains download link  School applications   Applications and offers by school phase, 2018
    user checks page contains link with text and url  Applications and offers by school phase, 2018   %{DATA_API_URL}/download/secondary-and-primary-schools-applications-and-offers/2018/data/school_applications_and_offers.csv
    user closes details dropdown  School applications

Validate Exclusions data downloads are available
    [Documentation]  EES-562
    [Tags]  HappyPath   NotAgainstLocal   Failing
    user opens details dropdown  Exclusions
    user checks details dropdown contains download link  Exclusions  Duration of fixed exclusions, 2016/17
    user checks page contains link with text and url  Duration of fixed exclusions, 2016/17   %{DATA_API_URL}/download/permanent-and-fixed-period-exclusions-in-england/2016-17/data/exclusions_duration_of_fixed_exclusions.csv
    user checks details dropdown contains download link  Exclusions  Exclusion metadata, 2016/17
    user checks page contains link with text and url  Exclusion metadata, 2016/17   %{DATA_API_URL}/download/permanent-and-fixed-period-exclusions-in-england/2016-17/data/exclusion_metadata.docx
    user checks details dropdown contains download link  Exclusions  Exclusions by characteristic, 2016/17
    user checks page contains link with text and url  Exclusions by characteristic, 2016/17   %{DATA_API_URL}/download/permanent-and-fixed-period-exclusions-in-england/2016-17/data/exclusions_by_characteristic.csv
    user checks details dropdown contains download link  Exclusions  Exclusions by geographic level, 2016/17
    user checks page contains link with text and url  Exclusions by geographic level, 2016/17   %{DATA_API_URL}/download/permanent-and-fixed-period-exclusions-in-england/2016-17/data/exclusions_by_geographic_level.csv
    user checks details dropdown contains download link  Exclusions  Exclusions by reason, 2016/17
    user checks page contains link with text and url  Exclusions by reason, 2016/17    %{DATA_API_URL}/download/permanent-and-fixed-period-exclusions-in-england/2016-17/data/exclusions_by_reason.csv
    user checks details dropdown contains download link  Exclusions  Number of fixed exclusions, 2016/17
    user checks page contains link with text and url  Number of fixed exclusions, 2016/17    %{DATA_API_URL}/download/permanent-and-fixed-period-exclusions-in-england/2016-17/data/exclusions_number_of_fixed_exclusions.csv
    user checks details dropdown contains download link  Exclusions  Total days missed due to fixed period exclusions, 2016/17
    user checks page contains link with text and url  Total days missed due to fixed period exclusions, 2016/17    %{DATA_API_URL}/download/permanent-and-fixed-period-exclusions-in-england/2016-17/data/exclusions_total_days_missed_fixed_exclusions.csv
    user closes details dropdown  Exclusions
