#!/usr/bin/env python3
from __future__ import annotations

import argparse
import logging
import multiprocessing
from logging.handlers import RotatingFileHandler
from pathlib import Path

from square_rod_api.config import ServiceConfig
from square_rod_api.http_server import serve


def configure_logging(config: ServiceConfig) -> logging.Logger:
    config.logs_root.mkdir(parents=True, exist_ok=True)
    logger = logging.getLogger("square_rod_api")
    logger.setLevel(logging.INFO)
    logger.handlers.clear()
    formatter = logging.Formatter(
        "%(asctime)s %(levelname)s %(processName)s %(threadName)s %(message)s"
    )
    file_handler = RotatingFileHandler(
        config.logs_root / "service.log",
        maxBytes=10 * 1024 * 1024,
        backupCount=10,
        encoding="utf-8",
    )
    file_handler.setFormatter(formatter)
    logger.addHandler(file_handler)
    console = logging.StreamHandler()
    console.setFormatter(formatter)
    logger.addHandler(console)
    logging.getLogger("square_rod_api.http").handlers = logger.handlers
    logging.getLogger("square_rod_api.http").setLevel(logging.INFO)
    logging.getLogger("square_rod_api.http").propagate = False
    return logger


def main() -> int:
    parser = argparse.ArgumentParser(description="Square Rod V7 local HTTP API")
    parser.add_argument(
        "--config",
        type=Path,
        default=Path(__file__).resolve().parent / "api_config.local.json",
    )
    args = parser.parse_args()
    config = ServiceConfig.load(args.config)
    logger = configure_logging(config)
    try:
        serve(config, logger)
    except KeyboardInterrupt:
        logger.info("Service interrupted")
    except Exception:
        logger.exception("Service failed")
        return 1
    return 0


if __name__ == "__main__":
    multiprocessing.freeze_support()
    raise SystemExit(main())
