*** Settings ***
Resource            ../libs/public-common.robot

Suite Setup         user opens the browser
Suite Teardown      user closes the browser

Force Tags          GeneralPublic    Prod

*** Test Cases ***
Navigate to Find Statistics page
    environment variable should be set    PUBLIC_URL
    user goes to url    %{PUBLIC_URL}/find-statistics
    user waits until h1 is visible    Find statistics and data
    user waits for page to finish loading

Validate accordion sections exist
    user waits until page contains accordion section    Children's social care
    user waits until page contains accordion section    COVID-19
    user waits until page contains accordion section    Destination of pupils and students
    user waits until page contains accordion section    Early years
    user waits until page contains accordion section    Finance and funding
    user waits until page contains accordion section    Further education
    user waits until page contains accordion section    Higher education
    user waits until page contains accordion section    Pupils and schools
    user waits until page contains accordion section    School and college outcomes and performance
    user waits until page contains accordion section    Teachers and school workforce
    user waits until page contains accordion section    UK education and training statistics

Open and validate "Children, early years and social care" accordion section
    user opens accordion section    Children's social care
    user waits until accordion section contains text    Children's social care    Children in need and child protection

    user waits until accordion section contains text    Children's social care    Children looked after

    user waits until accordion section contains text    Children's social care    Children's social work workforce

    user waits until accordion section contains text    Children's social care    Outcomes for children in social care

    user waits until accordion section contains text    Children's social care    Secure children's homes

    user waits until accordion section contains text    Children's social care    Serious incident notifications

Open and validate "COVID-19" accordion section
    user opens accordion section    COVID-19
    user waits until accordion section contains text    COVID-19    Attendance

    user waits until accordion section contains text    COVID-19    Confirmed cases reported by Higher Education providers

    user waits until accordion section contains text    COVID-19    Devices

    user waits until accordion section contains text    COVID-19    Testing

Open and validate "Destination of pupils and students" accordion section
    user opens accordion section    Destination of pupils and students
    user waits until accordion section contains text    Destination of pupils and students
    ...    Destinations of key stage 4 and 16-18 pupils

    user waits until accordion section contains text    Destination of pupils and students    NEET and participation

Open and validate "Early years" accordion section
    user opens accordion section    Early years
    user waits until accordion section contains text    Early years    Childcare and early years

    user waits until accordion section contains text    Early years    Early years foundation stage profile

    user waits until accordion section contains text    Early years    Early years surveys

Open and validate "Finance and funding" accordion section
    user opens accordion section    Finance and funding
    user waits until accordion section contains text    Finance and funding    Local authority and school finance
    user waits until accordion section contains text    Finance and funding    Student loan forecasts

Open and validate "Further education" accordion section
    user opens accordion section    Further education
    user waits until accordion section contains text    Further education    FE choices

    user waits until accordion section contains text    Further education    Further education and skills

    user waits until accordion section contains text    Further education
    ...    Further education: outcome-based success measures

Open and validate "Higher education" accordion section
    user opens accordion section    Higher education

    user waits until accordion section contains text    Higher education
    ...    Higher education graduate employment and earnings

    user waits until accordion section contains text    Higher education    Participation measures in higher education

    user waits until accordion section contains text    Higher education    Widening participation in higher education

Open and validate "Pupils and schools" accordion section
    user opens accordion section    Pupils and schools

    user waits until accordion section contains text    Pupils and schools    Academy transfers

    user waits until accordion section contains text    Pupils and schools    Admission appeals

    user waits until accordion section contains text    Pupils and schools    Exclusions

    user waits until accordion section contains text    Pupils and schools    Parental responsibility measures

    user waits until accordion section contains text    Pupils and schools    Pupil absence

    user waits until accordion section contains text    Pupils and schools    Pupil projections

    user waits until accordion section contains text    Pupils and schools    School and pupil numbers

    user waits until accordion section contains text    Pupils and schools    School applications

    user waits until accordion section contains text    Pupils and schools    School capacity

    user waits until accordion section contains text    Pupils and schools    Special educational needs (SEN)

Open and validate "School and college outcomes and performance" accordion section
    user opens accordion section    School and college outcomes and performance

    user waits until accordion section contains text    School and college outcomes and performance    16 to 19 attainment

    user waits until accordion section contains text    School and college outcomes and performance    GCSEs (key stage 4)

    user waits until accordion section contains text    School and college outcomes and performance    Key stage 1

    user waits until accordion section contains text    School and college outcomes and performance    Key stage 2

Open and validate "Teachers and school workforce" accordion section
    user opens accordion section    Teachers and school workforce

    user waits until accordion section contains text    Teachers and school workforce    Initial teacher training (ITT)

    user waits until accordion section contains text    Teachers and school workforce    School workforce

Open and validate "UK education and training statistics" accordion section
    user opens accordion section    UK education and training statistics
    user waits until accordion section contains text    UK education and training statistics    UK education and training statistics
