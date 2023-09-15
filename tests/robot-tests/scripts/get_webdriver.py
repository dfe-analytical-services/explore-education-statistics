import os
from pathlib import Path
from typing import Optional

from webdriver_manager.chrome import ChromeDriverManager


def get_webdriver(version: Optional[str] = None) -> None:
    driver_path = ChromeDriverManager(driver_version=version).install()
    driver_dir = Path(driver_path).parents[0]

    os.environ["PATH"] += os.pathsep + str(driver_dir)


if __name__ == "__main__":
    get_webdriver()
