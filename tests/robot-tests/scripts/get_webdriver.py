import os
import platform
from pathlib import Path

import pyderman


def get_webdriver(version: str) -> None:
    chromedriver_filename = "chromedriver.exe" if platform.system() == "Windows" else "chromedriver"
    pyderman.install(
        file_directory="./webdriver/",
        filename=chromedriver_filename,
        verbose=True,
        chmod=True,
        overwrite=False,
        version=version,
    )

    os.environ["PATH"] += os.pathsep + str(Path("./webdriver").absolute())


if __name__ == "__main__":
    get_webdriver("latest")
