import datetime
import os
import pytz
from tests.libs.selenium_elements import sl


def get_london_date(offset_days: int = 0, format_string: str = "%-d %B %Y") -> str:
    return _get_date_and_time(offset_days, format_string, "Europe/London")


def get_london_date_and_time(offset_days: int = 0, format_string: str = "%-d %B %Y") -> str:
    return _get_date_and_time(offset_days, format_string, "Europe/London")


def get_local_browser_date_and_time(offset_days: int = 0, format_string: str = "%-d %B %Y") -> str:
    return _get_date_and_time(offset_days, format_string, _get_browser_timezone())


def get_london_day_of_month(offset_days: int = 0) -> str:
    return get_london_date_and_time(offset_days, "%-d")


def get_london_month_date(offset_days: int = 0) -> str:
    return get_london_date_and_time(offset_days, "%-m")


def get_london_month_word(offset_days: int = 0) -> str:
    return get_london_date_and_time(offset_days, "%B")


def get_london_year(offset_days: int = 0) -> str:
    return get_london_date_and_time(offset_days, "%Y")


def _get_browser_timezone():
    return sl().driver.execute_script("return Intl.DateTimeFormat().resolvedOptions().timeZone;")


def _get_date_and_time(offset_days: int, format_string: str, timezone: str) -> str:
    return _format_datetime(
        datetime.datetime.now(pytz.timezone(timezone)) + datetime.timedelta(days=offset_days), format_string
    )


def _format_datetime(datetime: datetime, format_string: str) -> str:
    if os.name == "nt":
        format_string = format_string.replace("%-", "%#")

    return datetime.strftime(format_string)
