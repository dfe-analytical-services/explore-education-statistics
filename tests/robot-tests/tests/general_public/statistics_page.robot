*** Settings ***
Resource            ../libs/public-common.robot

Suite Setup         user opens the browser
Suite Teardown      user closes the browser

Force Tags          GeneralPublic    Local    Dev    Test    Preprod

*** Test Cases ***
Navigate to Find Statistics page
    environment variable should be set    PUBLIC_URL
    user goes to url    %{PUBLIC_URL}/find-statistics
    user waits until h1 is visible    Find statistics and data
    user waits for page to finish loading

# TODO: User uses search

Validate accordion sections exist
    user waits until page contains accordion section    Children, early years and social care
    user waits until page contains accordion section    Destination of pupils and students
    user waits until page contains accordion section    Finance and funding
    user waits until page contains accordion section    Further education
    user waits until page contains accordion section    Higher education
    user waits until page contains accordion section    Pupils and schools
    user waits until page contains accordion section    School and college outcomes and performance
    user waits until page contains accordion section    Teachers and school workforce
    user waits until page contains accordion section    UK education and training statistics

Open "Pupils and schools" accordion section
    user opens accordion section    Pupils and schools

Validate "Pupils and schools" section
    [Documentation]    EES-1577
    user waits until accordion section contains text    Pupils and schools    Admission appeals
    user waits until accordion section contains text    Pupils and schools    Exclusions
    user waits until accordion section contains text    Pupils and schools    Parental responsibility measures
    user waits until accordion section contains text    Pupils and schools    Pupil absence
    user waits until accordion section contains text    Pupils and schools    Pupil projections
    user waits until accordion section contains text    Pupils and schools    School and pupil numbers
    user waits until accordion section contains text    Pupils and schools    School applications
    user waits until accordion section contains text    Pupils and schools    School capacity
    user waits until accordion section contains text    Pupils and schools    Special educational needs (SEN)

Validate "Pupil absence" details component
    user opens details dropdown    Pupil absence

    user waits until details dropdown contains publication    Pupil absence    Pupil absence in schools in England
    user checks publication bullet contains link    Pupil absence in schools in England    View statistics and data
    user checks publication bullet contains link    Pupil absence in schools in England    Create your own tables
    user checks publication bullet does not contain link    Pupil absence in schools in England    Statistics at DfE

    user waits until details dropdown contains publication    Pupil absence
    ...    Pupil absence in schools in England: autumn and spring
    #user checks publication bullet contains link    Pupil absence in schools in England: autumn and spring    Statistics at DfE
    #user checks publication bullet does not contain link    Pupil absence in schools in England: autumn and spring    View statistics and data

    user waits until details dropdown contains publication    Pupil absence
    ...    Pupil absence in schools in England: autumn term
    #user checks publication bullet contains link    Pupil absence in schools in England: autumn term    Statistics at DfE
    #user checks publication bullet does not contain link    Pupil absence in schools in England: autumn term    View statistics and data
