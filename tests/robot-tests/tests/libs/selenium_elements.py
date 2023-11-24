from robot.libraries.BuiltIn import BuiltIn
from SeleniumLibrary.keywords.waiting import WaitingKeywords


def sl():
    return BuiltIn().get_library_instance("SeleniumLibrary")


def element_finder():
    return sl()._element_finder


def waiting():
    return WaitingKeywords(sl())
