*** Settings ***
Resource    ../libs/library.robot

Force Tags  GeneralPublic

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Navigate to Find Statistics page
    [Tags]  HappyPath
    user goes to url   ${url}/statistics
    user waits until page contains  Browse to find the statistics and data you’re looking for

User uses search
    [Tags]   UnderConstruction
    user checks page contains  css:body

Validate accordion sections exist
    [Tags]  HappyPath
    user checks page contains accordion  Children, early years and social care
    user checks page contains accordion  Destination of pupils and students
    user checks page contains accordion  Finance and funding
    user checks page contains accordion  Further education
    user checks page contains accordion  Higher education
    user checks page contains accordion  Pupils and schools
    user checks page contains accordion  School and college outcomes and performance
    user checks page contains accordion  Teachers and school workforce
    user checks page contains accordion  UK education and training statistics

Open "Pupils and schools" accordion section
    [Tags]  HappyPath
    user opens accordion section  Pupils and schools

Validate "Pupils and schools" section
    [Tags]  HappyPath   Failing
    user checks accordion section contains details  Pupils and schools    Admission appeals
    user checks accordion section contains details  Pupils and schools    Exclusions
    user checks accordion section contains details  Pupils and schools    Parental responsibility measures
    user checks accordion section contains details  Pupils and schools    Pupil absence
    user checks accordion section contains details  Pupils and schools    Pupil projections
    user checks accordion section contains details  Pupils and schools    School and pupil numbers
    user checks accordion section contains details  Pupils and schools    School applications
    user checks accordion section contains details  Pupils and schools    School capacity
    user checks accordion section contains details  Pupils and schools    Special educational needs (SEN)

Validate "Pupil absence" details component
    [Tags]  HappyPath
    user opens details dropdown  Pupil absence

    user checks details dropdown contains publication   Pupil absence   Pupil absence in schools in England
    user checks page does not contain   Pupil absence in schools in England - currently available via Statistics at DfE

    user checks details dropdown contains publication   Pupil absence   Pupil absence in schools in England: autumn and spring
    user checks page contains   Pupil absence in schools in England: autumn term - currently available via Statistics at DfE

    user checks details dropdown contains publication   Pupil absence   Pupil absence in schools in England: autumn term
    user checks page contains   Pupil absence in schools in England: autumn and spring - currently available via Statistics at DfE
