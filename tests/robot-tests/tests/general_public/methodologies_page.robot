*** Settings ***
Resource    ../libs/public-common.robot

Force Tags  GeneralPublic  Local  Dev  Test

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Navigate to /methodology page
    [Tags]  HappyPath
    environment variable should be set  PUBLIC_URL
    user goes to url   %{PUBLIC_URL}/methodology
    user waits until page contains heading   Methodologies

Validate page contents
    [Tags]  HappyPath
    user checks page contains accordion    Pupils and schools

    user opens accordion section    Pupils and schools

    user opens details dropdown     Exclusions
    user checks page contains methodology link  Exclusions   Permanent and fixed-period exclusions in England   /methodology/permanent-and-fixed-period-exclusions-in-england

    user opens details dropdown     Pupil absence
    user checks page contains methodology link  Pupil absence  Pupil absence in schools in England   /methodology/pupil-absence-in-schools-in-england

    user opens details dropdown     School applications
    user checks page contains methodology link  School applications   Secondary and primary schools applications and offers   /methodology/secondary-and-primary-schools-applications-and-offers

Validate Related information section links exist
    [Tags]  HappyPath
    user checks page contains element   xpath://a[text()="Find statistics and data"]
    user checks page contains element   xpath://a[text()="Education statistics: glossary"]

