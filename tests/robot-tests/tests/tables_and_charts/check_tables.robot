*** Settings ***
Resource            ../libs/common.robot
Library             ../libs/visual.py

Force Tags          GeneralPublic    Local

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

*** Test Cases ***
Test
    environment variable should be set    PUBLIC_URL
    user navigates to public frontend    %{PUBLIC_URL}/find-statistics/pupil-absence-in-schools-in-england
    user waits until page contains element    id:releaseHeadlines-tables-tab
    user scrolls to element    id:releaseHeadlines-tables-tab
    user clicks element    id:releaseHeadlines-tables-tab
    user waits until element contains    testid:dataTableCaption
    ...    'Absence by characteristic' in England between 2012/13 and 2016/17    %{WAIT_SMALL}
    user takes screenshot of element    id:releaseHeadlines-tables    table.png
