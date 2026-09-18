"""
Details of the Azure DevOps pipeline that a UI test run is part of.
"""

import os
from typing import Optional
from urllib.parse import quote


def get_pipeline_artifacts_url(collection_uri: str, project: str, build_id: str) -> Optional[str]:
    if not collection_uri or not project or not build_id:
        return None

    encoded_project = quote(project, safe="")
    encoded_build_id = quote(build_id, safe="")
    return (
        f"{collection_uri.rstrip('/')}/{encoded_project}/_build/results"
        f"?buildId={encoded_build_id}&view=artifacts&pathAsName=false&type=publishedArtifacts"
    )


def current_results_url() -> Optional[str]:
    """
    Where to send a reader to see the results of this test run.

    Built from the variables that Azure DevOps predefines for every task, and None
    outside of a pipeline, so that local test runs report without a link to results that
    do not exist.

    A release deploys a build, so its BUILD_BUILDID is the application build being
    deployed rather than this test run, and its artifacts are the application's. The
    release itself is the only thing that knows about the tests, so link to that instead.
    """
    release_url = os.getenv("RELEASE_RELEASEWEBURL")

    if release_url:
        return release_url

    return get_pipeline_artifacts_url(
        collection_uri=os.getenv("SYSTEM_TEAMFOUNDATIONCOLLECTIONURI", ""),
        project=os.getenv("SYSTEM_TEAMPROJECT", ""),
        build_id=os.getenv("BUILD_BUILDID", ""),
    )
