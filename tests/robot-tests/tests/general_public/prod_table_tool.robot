*** Settings ***
Resource            ../libs/public-common.robot

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          GeneralPublic    Prod

*** Test Cases ***
Go to 'data-tables' page
    user navigates to data tables page on public frontend

Check page contains correct themes
    user waits until page contains details dropdown    Children's social care

    user waits until page contains details dropdown    COVID-19

    user waits until page contains details dropdown    Destination of pupils and students

    user waits until page contains details dropdown    Early years

    user waits until page contains details dropdown    Finance and funding

    user waits until page contains details dropdown    Further education

    user waits until page contains details dropdown    Higher education

    user waits until page contains details dropdown    Pupils and schools

    user waits until page contains details dropdown    School and college outcomes and performance

    user waits until page contains details dropdown    Teachers and school workforce

    user waits until page contains details dropdown    UK education and training statistics

check publications have correct number of releases
    # There are this number of publications that have a live release with a subject on prod
    # this selector gets each input field present on the data-tables page
    # if the number goes down then we know there is a problem with a given release
    user checks element count is x    //*[@name="publicationId"]    57

check 'Children's social care' contains correct topics
    user opens details dropdown    Children's social care
    user waits until page contains details dropdown    Children in need and child protection
    user waits until page contains details dropdown    Children looked after
    user waits until page contains details dropdown    Children's social work workforce
    user waits until page contains details dropdown    Outcomes for children in social care
    user waits until page contains details dropdown    Secure children's homes
    user waits until page contains details dropdown    Serious incident notifications

check 'COVID-19' contains correct topics
    user opens details dropdown    COVID-19
    user waits until page contains details dropdown    Attendance
    user waits until page contains details dropdown    Confirmed cases reported by Higher Education providers
    user waits until page contains details dropdown    Devices
    user waits until page contains details dropdown    Testing

check 'Destination of pupils and students' contains correct topics
    user opens details dropdown    Destination of pupils and students
    user waits until page contains details dropdown    Destinations of key stage 4 and 16-18 pupils
    user waits until page contains details dropdown    NEET and participation

check 'Early years' contains correct topics
    user opens details dropdown    Early years
    user waits until page contains details dropdown    Childcare and early years
    user waits until page contains details dropdown    Early years foundation stage profile

check 'Finance and funding' contains correct topics
    user opens details dropdown    Finance and funding
    user waits until page contains details dropdown    Local authority and school finance
    user waits until page contains details dropdown    Student loan forecasts

check 'Further education' contains correct topics
    user opens details dropdown    Further education
    user waits until page contains details dropdown    Further education and skills
    user waits until page contains details dropdown    Further education: outcome-based success measures

check 'Higher education' contains correct topics
    user opens details dropdown    Higher education
    user waits until page contains details dropdown    Higher education graduate employment and earnings
    user waits until page contains details dropdown    Participation measures in higher education
    user waits until page contains details dropdown    Widening participation in higher education

check 'Pupils and schools' contains correct topics
    user opens details dropdown    Pupils and schools
    user waits until page contains details dropdown    Academy transfers
    user waits until page contains details dropdown    Admission appeals
    user waits until page contains details dropdown    Exclusions
    user waits until page contains details dropdown    Parental responsibility measures
    user waits until page contains details dropdown    Pupil absence
    user waits until page contains details dropdown    Pupil projections
    user waits until page contains details dropdown    School and pupil numbers
    user waits until page contains details dropdown    School applications
    user waits until page contains details dropdown    School capacity
    user waits until page contains details dropdown    Special educational needs (SEN)

check 'School and college outcomes and performance' contains correct topics
    user opens details dropdown    School and college outcomes and performance
    user waits until page contains details dropdown    16 to 19 attainment
    user waits until page contains details dropdown    GCSEs (key stage 4)
    user waits until page contains details dropdown    Key stage 2

check 'Teachers and school workforce' contains correct topics
    user opens details dropdown    Teachers and school workforce
    user waits until page contains details dropdown    Initial teacher training (ITT)
    user waits until page contains details dropdown    School workforce

check 'UK education and training statistics' contains correct topics
    user opens details dropdown    UK education and training statistics
    user waits until page contains details dropdown    UK education and training statistics
