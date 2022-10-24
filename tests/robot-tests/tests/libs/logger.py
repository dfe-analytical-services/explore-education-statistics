import logging
import time
from functools import lru_cache


@lru_cache(maxsize=50)
def get_logger(name: str) -> logging.Logger:
    logger = logging.getLogger(name=name)
    logger.setLevel(logging.INFO)

    formatter = logging.Formatter("%(asctime)s: %(message)s")

    handler = logging.StreamHandler()
    handler.setFormatter(formatter)

    logger.addHandler(handler)

    logging.Formatter.converter = time.gmtime

    logger.debug(" **** New Session Created for {}  **** ".format(name))
    return logger
