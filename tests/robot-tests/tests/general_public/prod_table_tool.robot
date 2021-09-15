*** Settings ***
Resource            ../libs/public-common.robot

Suite Setup         user opens the browser
Suite Teardown      user closes the browser

Force Tags          GeneralPublic    Prod

*** Test Cases ***
Go to 'data-tables' page
    user navigates to data tables page on public frontend

Check page contains correct themes
    user checks page for details section    Children's social care

    user checks page for details section    COVID-19

    user checks page for details section    Destination of pupils and students

    user checks page for details section    Early years

    user checks page for details section    Finance and funding

    user checks page for details section    Further education

    user checks page for details section    Higher education

    user checks page for details section    Pupils and schools

    user checks page for details section    School and college outcomes and performance

    user checks page for details section    Teachers and school workforce

    user checks page for details section    UK education and training statistics

check publications have correct number of releases
    # There are 56 publications that have a live release with a subject on prod
    # this selector gets each input field present on the data-tables page
    # if the number goes down then we know there is a problem with a given release
    user checks element count is x    //*[@name="publicationId"]    56

check 'Children's social care' contains correct topics
    user expands details section    Children's social care
    user checks page for details dropdown    Children in need and child protection
    user checks page for details dropdown    Children looked after
    user checks page for details dropdown    Children's social work workforce
    user checks page for details dropdown    Outcomes for children in social care
    user checks page for details dropdown    Secure children's homes
    user checks page for details dropdown    Serious incident notifications

check 'COVID-19' contains correct topics
    user expands details section    COVID-19
    user checks page for details dropdown    Attendance
    user checks page for details dropdown    Confirmed cases reported by Higher Education providers
    user checks page for details dropdown    Devices
    user checks page for details dropdown    Testing

check 'Destination of pupils and students' contains correct topics
    user expands details section    Destination of pupils and students
    user checks page for details dropdown    Destinations of key stage 4 and 16-18 pupils
    user checks page for details dropdown    NEET and participation

check 'Early years' contains correct topics
    user expands details section    Early years
    user checks page for details dropdown    Childcare and early years
    user checks page for details dropdown    Early years foundation stage profile

check 'Finance and funding' contains correct topics
    user expands details section    Finance and funding
    user checks page for details dropdown    Local authority and school finance
    user checks page for details dropdown    Student loan forecasts

check 'Further education' contains correct topics
    user expands details section    Further education
    user checks page for details dropdown    Further education and skills
    user checks page for details dropdown    Further education: outcome-based success measures

check 'Higher education' contains correct topics
    user expands details section    Higher education
    user checks page for details dropdown    Higher education graduate employment and earnings
    user checks page for details dropdown    Participation measures in higher education
    user checks page for details dropdown    Widening participation in higher education

check 'Pupils and schools' contains correct topics
    user expands details section    Pupils and schools
    user checks page for details dropdown    Academy transfers
    user checks page for details dropdown    Admission appeals
    user checks page for details dropdown    Exclusions
    user checks page for details dropdown    Parental responsibility measures
    user checks page for details dropdown    Pupil absence
    user checks page for details dropdown    Pupil projections
    user checks page for details dropdown    School and pupil numbers
    user checks page for details dropdown    School applications
    user checks page for details dropdown    School capacity
    user checks page for details dropdown    Special educational needs (SEN)

check 'School and college outcomes and performance' contains correct topics
    user expands details section    School and college outcomes and performance
    user checks page for details dropdown    16 to 19 attainment
    user checks page for details dropdown    GCSEs (key stage 4)
    user checks page for details dropdown    Key stage 2

check 'Teachers and school workforce' contains correct topics
    user expands details section    Teachers and school workforce
    user checks page for details dropdown    Initial teacher training (ITT)
    user checks page for details dropdown    School workforce

check 'UK education and training statistics' contains correct topics
    user expands details section    UK education and training statistics
    user checks page for details dropdown    UK education and training statistics
