*** Settings ***
Resource            ../libs/common.robot
Library             ../libs/visual.py
Library             ../libs/charts_and_tables.py

Force Tags          GeneralPublic    Local

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

*** Test Cases ***
Test
    get content block urls
    get fast track urls
