import pyderman
import platform
from pathlib import Path
import os


def get_webdriver(version: str) -> None:
    filename = 'chromedriver.exe' if platform.system() == "Windows" else 'chromedriver'
    pyderman.install(file_directory='./webdriver/',
                     filename=filename,
                     verbose=True,
                     chmod=True,
                     overwrite=False,
                     return_info=True,
                     version=version)

    os.environ["PATH"] += os.pathsep + str(Path('./webdriver').absolute())


if __name__ == '__main__':
    get_webdriver('latest')
