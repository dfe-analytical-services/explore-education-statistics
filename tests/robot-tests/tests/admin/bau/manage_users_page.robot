*** Settings ***
Resource            ../../libs/admin-common.robot
Resource            ../../libs/common.robot

Library             ../../libs/admin_api.py

Force Tags          Admin    Local    Dev

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser

*** Test Cases ***
Navigate to manage users page as bau1
    user goes to url    %{ADMIN_URL}/administration/users
    user checks table column heading contains    1    1    Name
    user checks table column heading contains    1    2    Email
    user checks table column heading contains    1    3    Role
    user checks table column heading contains    1    4    Actions

Assert that test users are present in table
    user checks results table row heading contains    1    1    Analyst1 User1
    user checks results table row heading contains    2    1    Analyst2 User2
    user checks results table row heading contains    3    1    Analyst3 User3
    user checks results table row heading contains    4    1    Bau1 User1
    user checks results table row heading contains    5    1    Bau2 User2

    user checks results table cell contains    1    1    ees-analyst1@education.gov.uk
    user checks results table cell contains    1    2    Analyst
    user checks results table cell contains    1    3    Manage

    user checks results table cell contains    2    1    ees-analyst2@education.gov.uk
    user checks results table cell contains    2    2    Analyst
    user checks results table cell contains    2    3    Manage

    user checks results table cell contains    3    1    ees-analyst3@education.gov.uk
    user checks results table cell contains    3    2    Analyst
    user checks results table cell contains    3    3    Manage

    user checks results table cell contains    4    1    ees-bau1@education.gov.uk
    user checks results table cell contains    4    2    BAU User
    user checks results table cell contains    4    3    Manage

    user checks results table cell contains    5    1    ees-bau2@education.gov.uk
    user checks results table cell contains    5    2    BAU User
    user checks results table cell contains    5    3    Manage
