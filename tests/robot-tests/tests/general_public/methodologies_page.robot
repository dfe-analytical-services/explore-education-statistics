*** Settings ***
Resource    ../libs/library.robot

Force Tags  GeneralPublic

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Navigate to /methodologies page
    [Tags]  HappyPath
    environment variable should be set  PUBLIC_URL
    user goes to url   %{PUBLIC_URL}/methodologies
    user waits until page contains element   xpath://h1[text()="Methodologies"]

Validate page contents
    [Tags]  HappyPath
    user checks page contains accordion    Pupils and schools
    user checks accordion is in position   Pupils and schools   1

    user opens accordion section    Pupils and schools

    user opens details dropdown     Exclusions
    user checks page contains link with text and url  Pupil exclusion statistics: methodology   /methodologies/permanent-and-fixed-period-exclusions-in-england

    user opens details dropdown     Pupil absence
    user checks page contains link with text and url  Pupil absence statistics: methodology   /methodologies/pupil-absence-in-schools-in-england

    user opens details dropdown     School applications
    user checks page contains link with text and url  Secondary and primary school applications and offers: methodology   /methodologies/secondary-and-primary-schools-applications-and-offers

Validate Related information section links exist
    [Tags]  HappyPath
    user checks page contains element   xpath://a[text()="Find statistics and data"]
    user checks page contains element   xpath://a[text()="Education statistics: glossary"]

