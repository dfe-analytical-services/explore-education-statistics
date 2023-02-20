*** Settings ***
Force Tags          Admin    Local    Dev    AltersData

*** Test Cases ***
Do something that passes
    Log to console  Yaaay

Do something that might fail
    ${random} =	Evaluate	random.randint(0, 3)
    Log to console   ${random}
#    IF  ${random} < 2
    Fail  Arrrg
#    END    