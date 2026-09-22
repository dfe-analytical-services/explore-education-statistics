"""
Details of the Azure DevOps pipeline that a UI test run is part of.
"""

import os
from typing import Optional
from urllib.parse import quote

from tests.libs.logger import get_logger

logger = get_logger(__name__)


def get_pipeline_artifacts_url(collection_uri: str, project: str, build_id: str) -> Optional[str]:
    if not collection_uri or not project or not build_id:
        return None

    encoded_project = quote(project, safe="")
    encoded_build_id = quote(build_id, safe="")
    return (
        f"{collection_uri.rstrip('/')}/{encoded_project}/_build/results"
        f"?buildId={encoded_build_id}&view=artifacts&pathAsName=false&type=publishedArtifacts"
    )


def get_release_url(collection_uri: str, project: str, release_id: str) -> Optional[str]:
    if not collection_uri or not project or not release_id:
        return None

    encoded_project = quote(project, safe="")
    encoded_release_id = quote(release_id, safe="")
    return f"{collection_uri.rstrip('/')}/{encoded_project}/_release?releaseId={encoded_release_id}&_a=release-summary"


def _current_release_url(collection_uri: str, project: str) -> Optional[str]:
    """
    Azure DevOps predefines the release's own URL, but it is not exposed in every release
    configuration, so fall back to building it from the release id.
    """
    return os.getenv("RELEASE_RELEASEWEBURL") or get_release_url(
        collection_uri, project, os.getenv("RELEASE_RELEASEID", "")
    )


def current_results_url() -> Optional[str]:
    """
    Where to send a reader to see the results of this test run.

    A release deploys a build, so its BUILD_BUILDID is the application build being
    deployed rather than this test run, and its artifacts are the application's. The
    release itself is the only thing that knows about the tests, so link to that instead.

    None outside of a pipeline, so that local test runs report without a link to results
    that do not exist.
    """
    collection_uri = os.getenv("SYSTEM_TEAMFOUNDATIONCOLLECTIONURI", "")
    project = os.getenv("SYSTEM_TEAMPROJECT", "")

    release_url = _current_release_url(collection_uri, project)

    if release_url:
        logger.info(f"Linking the UI test report to this release: {release_url}")
        return release_url

    artifacts_url = get_pipeline_artifacts_url(collection_uri, project, os.getenv("BUILD_BUILDID", ""))

    # Names only, never values. A release that reaches here was not recognised as one, and
    # the names tell us which variable to use instead.
    release_variables = sorted(name for name in os.environ if name.startswith("RELEASE_"))
    if release_variables:
        logger.warning(f"This looks like a release but its URL could not be built from {release_variables}")

    logger.info(f"Linking the UI test report to the build artifacts: {artifacts_url}")
    return artifacts_url
