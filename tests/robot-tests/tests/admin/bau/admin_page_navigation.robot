*** Settings ***
Resource            ../../libs/admin-common.robot
Resource            ../../libs/common.robot

Library             ../../libs/admin_api.py

Force Tags          Admin    Local    Dev

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Test Cases ***
Navigates correctly to administration page
    user navigates to    %{ADMIN_URL}/
    user waits until h1 is visible    Dashboard
    user clicks link    Platform administration

    user waits until page finishes loading
    user waits until h1 is visible    Platform administration

Navigate correctly to Manage incomplete imports
    user navigates to    %{ADMIN_URL}/administration
    user waits until h1 is visible    Platform administration

    user clicks link    Manage incomplete imports

    user waits until page finishes loading
    user waits until h1 is visible    Incomplete imports

Navigate correctly to Manage boundary data page
    user navigates to    %{ADMIN_URL}/administration
    user waits until h1 is visible    Platform administration

    user clicks link    Manage boundary data

    user waits until page finishes loading
    user waits until h1 is visible    Boundary data

Navigate correctly to Upload boundary data file page
    user navigates to    %{ADMIN_URL}/administration/boundary-data
    user waits until h1 is visible    Boundary data
    user clicks link    Upload boundary data file

    user waits until page finishes loading
    user waits until page contains title caption    Upload boundary data

Navigate correctly to Manage glossary page
    user navigates to    %{ADMIN_URL}/administration
    user waits until h1 is visible    Platform administration

    user clicks link    Manage glossary

    user waits until page finishes loading
    user waits until h1 is visible    Glossary

Navigate correctly to View feedback page
    user navigates to    %{ADMIN_URL}/administration
    user waits until h1 is visible    Platform administration

    user clicks link    View feedback

    user waits until page finishes loading
    user waits until h1 is visible    Feedback

Navigate correctly to Manage Education in Numbers page
    user navigates to    %{ADMIN_URL}/administration
    user waits until h1 is visible    Platform administration

    user clicks link    Manage Education in Numbers

    user waits until page finishes loading
    user waits until h1 is visible    Education in Numbers pages

Navigate correctly to Service announcements page
    user navigates to    %{ADMIN_URL}/administration
    user waits until h1 is visible    Platform administration

    user clicks link    Service announcement

    user waits until page finishes loading
    user waits until h1 is visible    Service announcement

Navigate correctly to Manage users page
    user navigates to    %{ADMIN_URL}/administration
    user waits until h1 is visible    Platform administration

    user clicks link    Manage users

    user waits until page finishes loading
    user waits until h1 is visible    Users

Navigate correctly to Invite new users page
    user navigates to    %{ADMIN_URL}/administration
    user waits until h1 is visible    Platform administration

    user clicks link    Invite new users

    user waits until page finishes loading
    user waits until h1 is visible    Pending invites

Navigate correctly to Pre-release users page
    user navigates to    %{ADMIN_URL}/administration
    user waits until h1 is visible    Platform administration

    user clicks link    Pre-release users

    user waits until page finishes loading
    user waits until h1 is visible    Pre-release users

Navigate to documentation root page
    user navigates to    %{ADMIN_URL}/documentation
    user waits until h1 is visible    Documentation
