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
    user waits until page contains element    id:releaseHeadlines-charts-tab
    user scrolls to element    id:releaseHeadlines-charts-tab
    user clicks element    id:releaseHeadlines-charts-tab
    user waits until page contains element    css:.recharts-legend-item-text
    user takes screenshot of element    id:releaseHeadlines-charts    chart.png
