*** Settings ***
Resource        ../libs/common.robot

Test Setup      fail test fast if required

Force Tags      GeneralPublic    Local    Dev    Prod    UnderConstruction

*** Test Cases ***
Send slack alert
    send slack alert    TESTING!!!
