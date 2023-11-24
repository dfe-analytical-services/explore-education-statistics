from robot.libraries.BuiltIn import BuiltIn
from SeleniumLibrary.keywords.waiting import WaitingKeywords

sl_instance = None
element_finder_instance = None
waiting_instance = None


def sl():
    global sl_instance
    if sl_instance is None:
        sl_instance = BuiltIn().get_library_instance("SeleniumLibrary")
    return sl_instance


def element_finder():
    global element_finder_instance
    if element_finder_instance is None:
        element_finder_instance = sl()._element_finder
    return element_finder_instance


def waiting():
    global waiting_instance
    if waiting_instance is None:
        waiting_instance = WaitingKeywords(sl())
    return waiting_instance


def clear_instances():
    global sl_instance, element_finder_instance, waiting_instance
    sl_instance = None
    element_finder_instance = None
    waiting_instance = None
