*** Settings ***
Resource            ../../libs/admin-common.robot
Resource            ../../libs/common.robot

Library             ../../libs/admin_api.py

Force Tags          Admin    Local    Dev

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Test Cases ***
Navigate to administration page as bau1
    user navigates to    %{ADMIN_URL}/administration

Navigate to Manage boundary data page
    user clicks link    Manage boundary data

    user waits until page finishes loading
    user waits until h1 is visible    Boundary data
