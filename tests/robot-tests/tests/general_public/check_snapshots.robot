*** Settings ***
Resource        ../libs/common.robot
Library         ../libs/snapshots.py

Test Setup      fail test fast if required

Force Tags      GeneralPublic    Snapshots    Prod

*** Test Cases ***
Compare current Find Statistics page with snapshot
    validate find stats snapshot

Compare current Table tool page with snapshot
    validate table tool snapshot

Compare current Data catalogue page with snapshot
    validate data catalogue snapshot
