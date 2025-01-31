*** Settings ***
Resource            ../libs/public-common.robot
Resource            permalinks.robot

Force Tags          GeneralPublic    Dev

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Test Cases ***
Check permalink bd6a1e7e-445a-40be-9a26-82b3cb827efc
    Check permalink with id    bd6a1e7e-445a-40be-9a26-82b3cb827efc

Check permalink b231d0c0-08a2-4ee8-b16c-26253025c644
    Check permalink with id    b231d0c0-08a2-4ee8-b16c-26253025c644
